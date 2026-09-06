using System;
using System.Collections;
using System.Collections.Generic;
using Starstate.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace Starstate.Ui
{
    /// <summary>OpenAI 兼容 Chat Completions 客户端（UnityWebRequest 协程驱动）。</summary>
    public static class LlmClient
    {
        [Serializable]
        private class ChatReq
        {
            public string model;
            public ChatMsg[] messages;
            public float temperature;
            public int max_tokens;
            public bool stream;
        }

        [Serializable]
        private class ChatResp
        {
            public ChoiceDto[] choices;
        }

        [Serializable]
        private class ChoiceDto
        {
            public RespMsg message;
        }

        [Serializable]
        private class RespMsg
        {
            public string content;
            public string reasoning_content;   // 思维链模型：正文为空时兜底用
        }

        /// <summary>发起一次对话补全。ok 返回 message.content 原文；err 返回错误说明。
        /// cancelled：可选取消谓词（如交谈弹层已关闭）——命中即中止请求，ok/err 均不回调。</summary>
        public static IEnumerator Chat(LlmConfig cfg, ChatMsg[] messages, int maxTokens,
            Action<string> ok, Action<string> err, Func<bool> cancelled = null)
        {
            var req = new ChatReq
            {
                model = string.IsNullOrEmpty(cfg.model) ? "local" : cfg.model,
                messages = messages,
                temperature = cfg.temperature,
                max_tokens = maxTokens,
                stream = false,
            };
            string body = JsonUtility.ToJson(req);
            using (var web = new UnityWebRequest(cfg.endpoint, "POST"))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(body);
                web.uploadHandler = new UploadHandlerRaw(bytes);
                web.downloadHandler = new DownloadHandlerBuffer();
                web.SetRequestHeader("Content-Type", "application/json");
                web.SetRequestHeader("Accept", "application/json");
                if (!string.IsNullOrEmpty(cfg.apiKey))
                    web.SetRequestHeader("Authorization", "Bearer " + cfg.apiKey);
                web.timeout = 60;
                var op = web.SendWebRequest();
                while (!op.isDone)
                {
                    if (cancelled != null && cancelled())
                    {
                        web.Abort();   // 会话已作废：中止在途请求，省流量与占用
                        yield break;
                    }
                    yield return null;
                }
                if (web.result != UnityWebRequest.Result.Success)
                {
                    err("连接失败：" + WebErr(web, cfg));
                    yield break;
                }
                if (web.responseCode >= 300)
                {
                    err("服务返回 " + web.responseCode + "：" + Trim(web.downloadHandler.text, 160));
                    yield break;
                }
                try
                {
                    var resp = JsonUtility.FromJson<ChatResp>(web.downloadHandler.text);
                    if (resp == null || resp.choices == null || resp.choices.Length == 0 || resp.choices[0].message == null)
                    {
                        err("响应格式异常（无 choices.message）");
                        yield break;
                    }
                    string content = resp.choices[0].message.content;
                    if (string.IsNullOrEmpty(content))
                        content = resp.choices[0].message.reasoning_content; // 思维链模型兜底
                    if (string.IsNullOrEmpty(content))
                    {
                        err("模型返回了空内容");
                        yield break;
                    }
                    ok(content);
                }
                catch (Exception e)
                {
                    err("解析响应失败：" + e.Message);
                }
            }
        }

        /// <summary>把 endpoint 换算成健康检查地址（llama-server: /health）。</summary>
        public static string HealthUrl(LlmConfig cfg)
        {
            string base0 = cfg.endpoint ?? "";
            int i = base0.IndexOf("/v1/", StringComparison.Ordinal);
            if (i > 0) base0 = base0.Substring(0, i);
            return base0.TrimEnd('/') + "/health";
        }

        /// <summary>轮询健康检查直至就绪或超时。</summary>
        public static IEnumerator HealthPoll(LlmConfig cfg, float timeoutSec, Action<string> status, Action<bool> done)
        {
            string url = HealthUrl(cfg);
            float t = 0f;
            while (t < timeoutSec)
            {
                using (var web = UnityWebRequest.Get(url))
                {
                    web.timeout = 3;
                    var op = web.SendWebRequest();
                    while (!op.isDone) yield return null;
                    if (web.result == UnityWebRequest.Result.Success && web.responseCode < 300)
                    {
                        done(true);
                        yield break;
                    }
                }
                status("正在等待本地模型就绪…（" + (int)t + "s）");
                yield return new WaitForSecondsRealtime(2f);
                t += 2f;
            }
            done(false);
        }

        private static string WebErr(UnityWebRequest web, LlmConfig cfg)
        {
            if (web.result == UnityWebRequest.Result.ConnectionError)
                return web.error + "（检查服务是否已启动：" + HealthUrl(cfg) + "）";
            return web.error;
        }

        private static string Trim(string s, int n)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\n", " ");
            return s.Length > n ? s.Substring(0, n) + "…" : s;
        }
    }
}
