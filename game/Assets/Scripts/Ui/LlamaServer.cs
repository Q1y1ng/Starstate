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
        private static float startingSince;

        public static bool Running { get { return proc != null && !proc.HasExited; } }

        public static IEnumerator EnsureRunning(LlmConfig cfg, Action<string> status, Action<bool> done)
        {
            // 防呆：协程若因宿主对象销毁/场景切换被中断，starting 会永远卡在 true
            // → 之后每次都走“已有启动在途”分支，白等 240s 且自动拉起永久失效。
            if (starting && Time.realtimeSinceStartup - startingSince > 300f) starting = false;

            // ① 已有服务在跑（外部启动过 / 本进程已拉起）→ 健康即直接用
            status("正在检查本地模型服务…");
            bool healthy = false;
            yield return LlmClient.HealthPoll(cfg, 4f, s => { }, ok => healthy = ok);
            if (healthy) { done(true); yield break; }

            // ② 本进程已拉起但尚未健康：只等待，禁止再 Start（防双开吃满内存）
            if (proc != null && !proc.HasExited)
            {
                bool ok2 = false, settled2 = false;
                status("本地服务已在启动，等待模型加载…");
                yield return PollWithExitWatch(cfg, proc, 240f, status, r => { ok2 = r; settled2 = true; });
                while (!settled2) yield return null;
                done(ok2);
                yield break;
            }

            // ③ 已有启动在途（自动拉起中点测试/交谈）：只等结果，禁止双开
            if (starting)
            {
                bool waitOk = false, waitSettled = false;
                yield return WaitForInFlightStart(cfg, status, r => { waitOk = r; waitSettled = true; });
                while (!waitSettled) yield return null;
                done(waitOk);
                yield break;
            }

            // ④ 端口被占或系统里已有 llama-server：外部实例，只等健康，绝不双开
            int foreign = CountForeignLlama();
            if (PortBusy(cfg.port) || foreign > 0)
            {
                status("检测到已有 llama-server（端口 " + cfg.port + (foreign > 0 ? "，进程×" + foreign : "") +
                       "），等待其就绪…（禁止双开）");
                bool waitOk = false, waitSettled = false;
                yield return PollWithExitWatch(cfg, null, 180f, status, r => { waitOk = r; waitSettled = true; });
                while (!waitSettled) yield return null;
                if (waitOk) { done(true); yield break; }
                status("已有服务端口占用但健康检查失败。请在任务管理器结束 llama-server 后重试，或检查端口/路径设置。");
                done(false);
                yield break;
            }

            // ⑤ 找运行时与模型
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

            // ⑥ 拉起：首选 D:\AI 标定配置（benchmark_2026-08-31 / quant_IQ4vsQ3 报告 + 2026-09-13 ngram-mod 实测）——
            //    9B 模型在本机（RTX 3060 6GB）标定值：-ngl 28 固定 + KV 双 q4_0 量化 + ub128 + fa on + t16 绑核；
            //    再叠 ngram-mod 自投机（零显存成本，冷 +15%~+67%、重复请求最高 +121%）；
            //    失败才降档（显存极端不足时），保证总能跑起来。
            //    starting 必须在 Process.Start 之前置位，否则并发 EnsureRunning 会双开（内存打满事故）。
            string threads = " -t 16 --cpu-range 0-19 -ub 128 -fa on";
            var preset = LlmPresets.Find(cfg.preset);
            string[] labels = { "标定配置", "降档（ngl16·KV q8_0）", "纯CPU" };
            int[] ngls = { preset != null ? preset.ngl : 28, 16, 0 };
            int baseCtx = Math.Max(1024, Math.Min(preset != null ? preset.ctx : cfg.ctx, 131072));
            int[] ctxs = { baseCtx, Math.Min(baseCtx, 32768), Math.Min(baseCtx, 16384) };
            string[] kvTypes = { "q4_0", "q8_0", "q8_0" };
            string presetArgs = preset != null && !string.IsNullOrEmpty(preset.args) ? " " + preset.args.Trim() : "";
            string lora = ResolveLora(cfg);

            // 尝试序列：每档先试带自投机；服务端不认这几个参数（旧版 llama.cpp）会立即退出，
            // 此时用同档不带自投机重试，并让后续档位都不再带（一次探测，不反复浪费启动时间）。
            string specArgs = SpecArgs(cfg);
            int[] tiers = { 0, 0, 1, 2 };
            bool[] specTries = { true, false, false, false };
            var seen = new HashSet<string>();

            starting = true;
            startingSince = Time.realtimeSinceStartup;
            for (int a = 0; a < tiers.Length; a++)
            {
                int t = tiers[a];
                bool useSpec = specTries[a] && specArgs.Length > 0;
                string args = "-m \"" + gguf + "\" --host 127.0.0.1 --port " + cfg.port +
                              " -c " + ctxs[t] + " -ngl " + ngls[t] +
                              " -ctk " + kvTypes[t] + " -ctv " + kvTypes[t] +
                              threads + presetArgs +
                              (useSpec ? " " + specArgs : "") +
                              " --jinja --reasoning off";   // 关闭思维链：正文直接输出（--reasoning off 为 b10343 规范开关）
                if (!string.IsNullOrEmpty(lora))
                    args += " --lora \"" + lora + "\"";
                if (!seen.Add(args)) continue;

                string label = (preset != null ? preset.label : labels[t])
                             + (t > 0 ? "（降档）" : "")
                             + (useSpec ? "·自投机" : (specArgs.Length > 0 ? "·无自投机" : ""));
                status("正在启动本地模型服务（" + label + "，上下文 " + ctxs[t] + "）…");
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
                    status("本地模型已就绪（" + label + "）：" + Path.GetFileName(gguf));
                    done(true);
                    yield break;
                }
                Kill();
            }
            starting = false;
            status("各档启动配置均失败，AI 增强暂不可用（游戏照常运行）。");
            done(false);
        }

        /// <summary>自投机解码启动参数；关闭时返回空串（空串=启动参数不带 --spec-*）。
        /// 参数留空则用内置默认值——旧存档里没这个字段时也能享受提速。</summary>
        private static string SpecArgs(LlmConfig cfg)
        {
            if (cfg == null || cfg.noSpec) return "";
            return string.IsNullOrEmpty(cfg.specArgs) ? LlmConfig.DefaultSpecArgs : cfg.specArgs.Trim();
        }

        /// <summary>端口是否已被占用（被占则禁止再拉第二个实例）。</summary>
        private static bool PortBusy(int port)
        {
            try
            {
                var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return false;
            }
            catch { return true; }
        }

        /// <summary>系统中其它 llama-server 进程数（排除本类已跟踪的 proc）。</summary>
        private static int CountForeignLlama()
        {
            int n = 0;
            try
            {
                int selfId = 0;
                try { if (proc != null && !proc.HasExited) selfId = proc.Id; } catch { /* ignore */ }
                foreach (var p in System.Diagnostics.Process.GetProcessesByName("llama-server"))
                {
                    try
                    {
                        if (selfId != 0 && p.Id == selfId) continue;
                        n++;
                    }
                    catch { /* ignore */ }
                    finally { try { p.Dispose(); } catch { /* ignore */ } }
                }
            }
            catch { /* 权限等 */ }
            return n;
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

        /// <summary>解析单个路径：文件直返，目录则取其中第一个 gguf。</summary>
        private static string ResolvePath(string p)
        {
            try
            {
                if (File.Exists(p)) return p;
                if (Directory.Exists(p))
                    foreach (var f in Directory.GetFiles(p, "*.gguf")) return f;
            }
            catch { /* 路径非法等 */ }
            return null;
        }

        /// <summary>解析要挂的 LoRA 路径（预设优先；空或文件不存在=不挂）。</summary>
        private static string ResolveLora(LlmConfig cfg)
        {
            var preset = LlmPresets.Find(cfg != null ? cfg.preset : null);
            string raw = preset != null ? preset.loraPath : (cfg != null ? cfg.loraPath : "");
            if (string.IsNullOrEmpty(raw)) return null;
            return File.Exists(raw) ? raw : null;
        }

        private static string ResolveGguf(LlmConfig cfg)
        {
            var preset = LlmPresets.Find(cfg != null ? cfg.preset : null);
            if (preset != null && !string.IsNullOrEmpty(preset.modelPath))
            {
                string pm = ResolvePath(preset.modelPath);
                if (pm != null) return pm;
                // 预设模型不在（未下载/换机器）→ 回落到自定义路径，而不是直接失败
            }
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
