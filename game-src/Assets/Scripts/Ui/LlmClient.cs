using System;
using System.Collections;
using System.Text;
using Starstate.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace Starstate.Ui
{
    /// <summary>OpenAI 兼容 Chat Completions 客户端（UnityWebRequest 协程驱动；支持 SSE 流式）。</summary>
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
        private class MsgList { public ChatMsg[] messages; }

        /// <summary>
        /// 带结构化输出的请求体（手写拼装：schema 是任意 JSON，JsonUtility 拼不了嵌套 schema）。
        /// 用显式 json_schema 而不是 json_object —— 本机 b10343 对 json_object 不做约束（实测无效），
        /// 带 schema 时 12/12 全过（流式 3/3），速度不变。
        /// </summary>
        private static string BuildJsonBody(string model, ChatMsg[] messages, float temperature, int maxTokens,
            bool stream, string schema)
        {
            string msgs = JsonUtility.ToJson(new MsgList { messages = messages });
            // 去掉 JsonUtility 的外层包裹，只留数组（恰好去一个尾部大括号，不用 TrimEnd——那会误伤内容里的 }）
            const string prefix = "{\"messages\":";
            if (msgs.StartsWith(prefix, StringComparison.Ordinal))
                msgs = msgs.Substring(prefix.Length, msgs.Length - prefix.Length - 1);
            return "{\"model\":\"" + model.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"," +
                   "\"messages\":" + msgs + "," +
                   "\"temperature\":" + temperature.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                   "\"max_tokens\":" + maxTokens + "," +
                   "\"stream\":" + (stream ? "true" : "false") + "," +
                   "\"response_format\":{\"type\":\"json_schema\",\"json_schema\":{" +
                   "\"name\":\"" + LlmSchemas.SchemaName + "\",\"strict\":true,\"schema\":" + schema + "}}}";
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

        /// <summary>SSE 增量下载器：把 data: 行里的 delta.content 拼成全文，并回调部分文本。</summary>
        private class SseHandler : DownloadHandlerScript
        {
            private readonly StringBuilder acc = new StringBuilder();
            private readonly StringBuilder contentAcc = new StringBuilder();
            private readonly Action<string> onPartial;
            private string pending = "";
            private string full = "";
            private string contentOnly = "";

            public SseHandler(Action<string> onPartial) : base(new byte[32 * 1024])
            {
                this.onPartial = onPartial;
            }

            /// <summary>含思维链的全文（流式预览用）。</summary>
            public string FullText { get { return full; } }

            /// <summary>仅正文 content（思维链模型会先吐 reasoning；解析用这个）。</summary>
            public string ContentOnly { get { return contentOnly; } }

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                if (data == null || dataLength <= 0) return false;
                pending += Encoding.UTF8.GetString(data, 0, dataLength);
                int idx;
                while ((idx = pending.IndexOf('\n')) >= 0)
                {
                    string line = pending.Substring(0, idx).Trim();
                    pending = pending.Substring(idx + 1);
                    if (line.Length == 0 || !line.StartsWith("data:")) continue;
                    string payload = line.Substring(5).Trim();
                    if (payload == "[DONE]") continue;
                    string piece = ExtractContent(payload);
                    string contentPiece = ExtractJsonStringField(payload, "content");
                    if (!string.IsNullOrEmpty(contentPiece))
                    {
                        contentAcc.Append(contentPiece);
                        contentOnly = contentAcc.ToString();
                    }
                    if (string.IsNullOrEmpty(piece)) continue;
                    acc.Append(piece);
                    full = acc.ToString();
                    if (onPartial != null) onPartial(full);
                }
                return true;
            }

            /// <summary>从 SSE JSON 片段里抠出 content/delta 字符串（容错，不依赖 JsonUtility 嵌套）。
            /// 思维链模型会先回 reasoning_content；content 为空时回退。</summary>
            private static string ExtractContent(string json)
            {
                if (string.IsNullOrEmpty(json)) return "";
                string content = ExtractJsonStringField(json, "content");
                if (!string.IsNullOrEmpty(content)) return content;
                return ExtractJsonStringField(json, "reasoning_content");
            }

            private static string ExtractJsonStringField(string json, string field)
            {
                string key = "\"" + field + "\":";
                int i = json.IndexOf(key, StringComparison.Ordinal);
                if (i < 0) return "";
                i += key.Length;
                while (i < json.Length && (json[i] == ' ' || json[i] == '\t')) i++;
                if (i >= json.Length || json[i] != '"') return "";
                i++;
                var sb = new StringBuilder();
                while (i < json.Length)
                {
                    char c = json[i++];
                    if (c == '\\' && i < json.Length)
                    {
                        char n = json[i++];
                        switch (n)
                        {
                            case 'n': sb.Append('\n'); break;
                            case 't': sb.Append('\t'); break;
                            case 'r': sb.Append('\r'); break;
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case 'u':
                                if (i + 3 < json.Length)
                                {
                                    string hex = json.Substring(i, 4);
                                    i += 4;
                                    try { sb.Append((char)Convert.ToInt32(hex, 16)); }
                                    catch { /* 忽略坏转义 */ }
                                }
                                break;
                            default: sb.Append(n); break;
                        }
                    }
                    else if (c == '"') break;
                    else sb.Append(c);
                }
                return sb.ToString();
            }
        }

        /// <summary>发起一次对话补全。ok 返回 message.content 原文；err 返回错误说明。
        /// cancelled：可选取消谓词（如交谈弹层已关闭）——命中即中止请求，ok/err 均不回调。
        /// jsonSchema：需要模型产出**合法 JSON** 时传入 schema（LlmSchemas.*），否则 null。
        ///       本机实测（2026-09-16）：不加约束时三个模型的 JSON 通过率只有 25%~67%，
        ///       加 schema 后 12/12 全过且速度不变（json_object 无效，必须用完整 schema）——
        ///       叙事游戏解析失败就静默回退静态台词，所以这是硬需求。</summary>
        public static IEnumerator Chat(LlmConfig cfg, ChatMsg[] messages, int maxTokens,
            Action<string> ok, Action<string> err, Func<bool> cancelled = null, string jsonSchema = null)
        {
            string modelName = string.IsNullOrEmpty(cfg.model) ? "local" : cfg.model;
            string body = string.IsNullOrEmpty(jsonSchema)
                ? JsonUtility.ToJson(new ChatReq
                  {
                      model = modelName, messages = messages, temperature = cfg.temperature,
                      max_tokens = maxTokens, stream = false,
                  })
                : BuildJsonBody(modelName, messages, cfg.temperature, maxTokens, false, jsonSchema);
            using (var web = new UnityWebRequest(cfg.endpoint, "POST"))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(body);
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
                        web.Abort();
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
                string content = ParseChatBody(web.downloadHandler.text, out string parseErr);
                if (content == null) { err(parseErr); yield break; }
                ok(content);
            }
        }

        /// <summary>流式对话补全（SSE）。onDelta 每收到增量回调一次当前全文；完成后 ok(全文)。
        /// 服务端若忽略 stream 返回整包 JSON，会自动回退解析。cancelled 语义同 Chat。</summary>
        public static IEnumerator ChatStream(LlmConfig cfg, ChatMsg[] messages, int maxTokens,
            Action<string> onDelta, Action<string> ok, Action<string> err, Func<bool> cancelled = null,
            string jsonSchema = null)
        {
            string modelName = string.IsNullOrEmpty(cfg.model) ? "local" : cfg.model;
            string body = string.IsNullOrEmpty(jsonSchema)
                ? JsonUtility.ToJson(new ChatReq
                  {
                      model = modelName, messages = messages, temperature = cfg.temperature,
                      max_tokens = maxTokens, stream = true,
                  })
                : BuildJsonBody(modelName, messages, cfg.temperature, maxTokens, true, jsonSchema);
            using (var web = new UnityWebRequest(cfg.endpoint, "POST"))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(body);
                web.uploadHandler = new UploadHandlerRaw(bytes);
                var sse = new SseHandler(onDelta);
                web.downloadHandler = sse;
                web.SetRequestHeader("Content-Type", "application/json");
                web.SetRequestHeader("Accept", "text/event-stream");
                if (!string.IsNullOrEmpty(cfg.apiKey))
                    web.SetRequestHeader("Authorization", "Bearer " + cfg.apiKey);
                web.timeout = 180;
                var op = web.SendWebRequest();
                while (!op.isDone)
                {
                    if (cancelled != null && cancelled())
                    {
                        web.Abort();
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
                string full = sse.ContentOnly;
                if (string.IsNullOrEmpty(full)) full = sse.FullText;
                if (string.IsNullOrEmpty(full))
                {
                    // 服务端可能忽略了 stream，回退整包解析
                    string content = ParseChatBody(web.downloadHandler.text, out string parseErr);
                    if (content == null) { err(parseErr); yield break; }
                    if (onDelta != null) onDelta(content);
                    ok(content);
                    yield break;
                }
                ok(full);
            }
        }

        private static string ParseChatBody(string raw, out string err)
        {
            err = "";
            try
            {
                var resp = JsonUtility.FromJson<ChatResp>(raw);
                if (resp == null || resp.choices == null || resp.choices.Length == 0 || resp.choices[0].message == null)
                {
                    err = "响应格式异常（无 choices.message）";
                    return null;
                }
                string content = resp.choices[0].message.content;
                if (string.IsNullOrEmpty(content))
                    content = resp.choices[0].message.reasoning_content;
                if (string.IsNullOrEmpty(content))
                {
                    err = "模型返回了空内容";
                    return null;
                }
                return content;
            }
            catch (Exception e)
            {
                err = "解析响应失败：" + e.Message;
                return null;
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
