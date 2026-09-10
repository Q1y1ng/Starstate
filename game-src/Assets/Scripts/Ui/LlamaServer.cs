using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Starstate.Core;
using UnityEngine;

namespace Starstate.Ui
{
    /// <summary>
    /// 本地 llama-server 生命周期管理：未运行时自动拉起（OpenAI 兼容端口），
    /// 轮询 /health 就绪；应用退出时回收子进程。模型路径可为 gguf 文件或其所在目录。
    /// </summary>
    public static class LlamaServer
    {
        private static System.Diagnostics.Process proc;
        private static bool starting;   // 防止测试连接与自动拉起并发启动两个服务进程

        public static bool Running { get { return proc != null && !proc.HasExited; } }

        public static IEnumerator EnsureRunning(LlmConfig cfg, Action<string> status, Action<bool> done)
        {
            // ① 已有服务在跑（外部启动过）→ 直接用
            status("正在检查本地模型服务…");
            bool healthy = false;
            yield return LlmClient.HealthPoll(cfg, 4f, s => { }, ok => healthy = ok);
            if (healthy) { done(true); yield break; }

            // ② 已有启动在途（自动拉起中点测试/交谈）：只等结果，禁止双开
            if (starting)
            {
                bool waitOk = false, waitSettled = false;
                yield return WaitForInFlightStart(cfg, status, r => { waitOk = r; waitSettled = true; });
                while (!waitSettled) yield return null;
                done(waitOk);
                yield break;
            }

            // ③ 找运行时与模型
            string exe = ResolveExe(cfg);
            if (exe == null)
            {
                status("未找到 llama-server.exe——请手动启动本地模型服务，或在设置中填写路径。");
                done(false);
                yield break;
            }
            string gguf = ResolveGguf(cfg);
            if (gguf == null)
            {
                status("未找到模型 gguf 文件（检查设置中的模型路径）。");
                done(false);
                yield break;
            }

            // ④ 拉起：首选 D:\AI 标定配置（benchmark_2026-08-31 / quant_IQ4vsQ3 报告）——
            //    9B 模型在本机（RTX 3060 6GB）标定值：-ngl 28 固定 + KV 双 q4_0 量化 + ub128 + fa on + t16 绑核；
            //    失败才降档（显存极端不足时），保证总能跑起来。
            //    starting 必须在 Process.Start 之前置位，否则并发 EnsureRunning 会双开（内存打满事故）。
            string threads = " -t 16 --cpu-range 0-19 -ub 128 -fa on";
            string[] labels = { "标定配置（ngl28·KV q4_0 量化）", "降档（ngl16·KV q8_0）", "纯CPU" };
            int[] ngls = { 28, 16, 0 };
            int baseCtx = Math.Max(1024, Math.Min(cfg.ctx, 65536));
            int[] ctxs = { baseCtx, Math.Min(baseCtx, 32768), Math.Min(baseCtx, 16384) };
            string[] kvTypes = { "q4_0", "q8_0", "q8_0" };
            var seen = new HashSet<string>();

            starting = true;
            for (int a = 0; a < labels.Length; a++)
            {
                string args = "-m \"" + gguf + "\" --host 127.0.0.1 --port " + cfg.port +
                              " -c " + ctxs[a] + " -ngl " + ngls[a] +
                              " -ctk " + kvTypes[a] + " -ctv " + kvTypes[a] +
                              threads +
                              " --jinja --reasoning off";   // 关闭思维链：正文直接输出（--reasoning off 为 b10343 规范开关）
                if (!string.IsNullOrEmpty(cfg.loraPath) && File.Exists(cfg.loraPath))
                    args += " --lora \"" + cfg.loraPath + "\"";
                if (!seen.Add(args)) continue;

                status("正在启动本地模型服务（" + labels[a] + "，上下文 " + ctxs[a] + "）…");
                try
                {
                    var si = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exe,
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(exe) ?? "",
                    };
                    proc = System.Diagnostics.Process.Start(si);
                }
                catch (Exception e)
                {
                    starting = false;
                    status("启动 llama-server 失败：" + e.Message);
                    done(false);
                    yield break;
                }

                // 等模型加载；进程提前退出（如显存不足）立即换下一档
                bool ok = false, settled = false;
                yield return PollWithExitWatch(cfg, proc, 240f, status, r => { ok = r; settled = true; });
                while (!settled) yield return null;
                if (ok)
                {
                    starting = false;
                    status("本地模型已就绪（" + labels[a] + "）：" + Path.GetFileName(gguf));
                    done(true);
                    yield break;
                }
                Kill();
            }
            starting = false;
            status("各档启动配置均失败，AI 增强暂不可用（游戏照常运行）。");
            done(false);
        }

        /// <summary>等待另一路 EnsureRunning 完成：健康即成功；starting 结束仍未就绪则失败。</summary>
        private static IEnumerator WaitForInFlightStart(LlmConfig cfg, Action<string> status, Action<bool> done)
        {
            float t = 0f;
            string url = LlmClient.HealthUrl(cfg);
            while (t < 240f)
            {
                using (var web = UnityEngine.Networking.UnityWebRequest.Get(url))
                {
                    web.timeout = 3;
                    var op = web.SendWebRequest();
                    while (!op.isDone) yield return null;
                    if (web.result == UnityEngine.Networking.UnityWebRequest.Result.Success && web.responseCode < 300)
                    {
                        done(true);
                        yield break;
                    }
                }
                if (!starting)
                {
                    // 启动流程已结束仍未健康：确认一次后判失败
                    bool h = false;
                    yield return LlmClient.HealthPoll(cfg, 4f, s => { }, ok => h = ok);
                    done(h);
                    yield break;
                }
                status("正在等待本地模型就绪…（另一启动流程进行中，" + (int)t + "s）");
                yield return new WaitForSecondsRealtime(2f);
                t += 2f;
            }
            done(false);
        }

        /// <summary>健康轮询＋进程退出监视：进程挂了立即返回 false，不做无谓等待。</summary>
        private static IEnumerator PollWithExitWatch(LlmConfig cfg, System.Diagnostics.Process p,
            float timeoutSec, Action<string> status, Action<bool> done)
        {
            float t = 0f;
            string url = LlmClient.HealthUrl(cfg);
            while (t < timeoutSec)
            {
                if (p != null && p.HasExited)
                {
                    done(false);
                    yield break;
                }
                using (var web = UnityEngine.Networking.UnityWebRequest.Get(url))
                {
                    web.timeout = 3;
                    var op = web.SendWebRequest();
                    while (!op.isDone)
                    {
                        if (p != null && p.HasExited) { done(false); yield break; }
                        yield return null;
                    }
                    if (web.result == UnityEngine.Networking.UnityWebRequest.Result.Success && web.responseCode < 300)
                    {
                        done(true);
                        yield break;
                    }
                }
                status("正在等待本地模型就绪…（" + (int)t + "s，加载大模型可能需要一至两分钟）");
                yield return new WaitForSecondsRealtime(2f);
                t += 2f;
            }
            done(false);
        }

        public static void Kill()
        {
            try
            {
                if (proc != null && !proc.HasExited) proc.Kill();
            }
            catch { /* 进程已退出等 */ }
            if (proc != null)
            {
                try { proc.Dispose(); } catch { /* 释放句柄 */ }
            }
            proc = null;
        }

        private static string ResolveExe(LlmConfig cfg)
        {
            if (!string.IsNullOrEmpty(cfg.serverExe) && File.Exists(cfg.serverExe)) return cfg.serverExe;
            string[] candidates =
            {
                "D:/AI/llama.cpp/llama-server.exe",
                "C:/llama.cpp/llama-server.exe",
                "D:/llama.cpp/llama-server.exe",
            };
            foreach (var c in candidates) if (File.Exists(c)) return c;
            return null;
        }

        private static string ResolveGguf(LlmConfig cfg)
        {
            string p = cfg.modelPath;
            if (string.IsNullOrEmpty(p)) return null;
            if (File.Exists(p)) return p;
            if (Directory.Exists(p))
            {
                foreach (var f in Directory.GetFiles(p, "*.gguf"))
                    return f; // 目录内首个 gguf
            }
            return null;
        }
    }
}
