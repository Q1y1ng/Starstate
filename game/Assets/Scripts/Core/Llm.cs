using System;
using System.Collections.Generic;
using System.Text;

namespace Starstate.Core
{
    /// <summary>
    /// 大模型增强层（纯 C#，可测试）。设计原则：
    /// ① LLM 只产文本与"情绪分类"，所有数值效果走白名单映射，模型给不了数字，不破坏平衡；
    /// ② 解析全程容错（代码块包裹/多余文字/缺字段均能回退），AI 不可用时游戏走原内容。
    /// </summary>

    [Serializable]
    public class LlmConfig
    {
        public bool enabled = true;              // AI 增强总开关
        public string mode = "local";            // local=本机 llama-server / remote=外部 OpenAI 兼容 API
        public string endpoint = "http://127.0.0.1:8817/v1/chat/completions";
        public string apiKey = "";
        public string model = "local";           // llama-server 忽略模型名，外部 API 填真实名
        public string serverExe = "D:/AI/llama.cpp/llama-server.exe";
        // 默认模型：本机 2026-09-15 新增的 4B 级「消融+RP 同权重」件——比 9B+LoRA 快 2.7 倍（44~51 t/s vs 16.5）、
        // 只需 2.5GB 显存/内存（可与浏览器/编辑器共存，避开本机 16GB 换页降速），且不需要另挂 LoRA。
        public string modelPath = "D:/AI/models/Qwen3.5-4B-Deckard-HERETIC/Qwen3.5-4B-Deckard-HERETIC-UNCENSORED-Thinking.i1-Q4_K_M.gguf";
        public string loraPath = "";   // 默认不挂：4B Deckard 已内置 RP 微调；Ornith-9B 预设才需要 RP-LoRA
        public string preset = "qwen35-4b";   // 模型预设 id（空=自定义，完全按上面三项+ctx 走）
        public int port = 8817;
        public int ctx = 32768;                  // 32K：游戏的提示词才几百字，32K 够长线对话；4B 模型 KV 小，显存无压力
        // ngram-mod 自投机解码（2026-09-13 本机实测：冷 +15%~+67%、重复请求最高 +121%，零额外显存）
        // 原理：不加载草稿模型，用跨请求共享的 ngram 哈希池在历史文本里找重复片段当草稿，
        //       目标模型批量验证——游戏里 system 提示词与相似句式反复出现，正是它的甜点。
        // 旧版 llama.cpp 不认这几个参数会在启动时直接退出，LlamaServer 会自动去掉它们重试。
        // ⚠️ 用**否定式** noSpec：JsonUtility 反序列化旧存档时缺字段只能给默认值，
        //    若写成 `spec` 就会因缺字段而静默关掉提速；写成 noSpec 则缺失=false=启用。
        public bool noSpec;                      // true = 关闭自投机
        public string specArgs = "";            // 空 = 用内置默认参数（LlamaConfig 里的 DefaultSpecArgs）
        public const string DefaultSpecArgs =
            "--spec-type ngram-mod --spec-ngram-mod-n-match 24 --spec-ngram-mod-n-min 48 --spec-ngram-mod-n-max 64";
        public float temperature = 0.9f;
        public bool autoStart = false;           // 默认不启动即加载；首次交谈/测试连接时再唤醒
    }

    [Serializable]
    public class ChatMsg
    {
        public string role;
        public string content;
    }

    /// <summary>
    /// 模型预设：本机（RTX 3060 6GB）已标定的几套“模型 + 启动参数”组合。
    /// 为什么要预设：不同模型的 ngl / 上下文 / 是否挂 LoRA 完全不同，写在启动器里就没法切了。
    /// 数值来自 `D:\AI\bat\32_*` `33_*` `30_*` 的实测标定与本项目的横向评测（2026-09-16）。
    /// </summary>
    [Serializable]
    public class LlmPreset
    {
        public string id = "";
        public string label = "";        // 设置面板显示名
        public string modelPath = "";
        public string loraPath = "";
        public int ctx = 32768;
        public int ngl = 99;
        public string args = "";          // 附加启动参数（预重为默认值）
        public string note = "";          // 速度/文风取舍提示
    }

    public static class LlmPresets
    {
        public const string Qwen4B = "qwen35-4b";
        public const string Gemma4B = "gemma4-e4b";
        public const string Ornith9B = "ornith-9b";

        public static readonly LlmPreset[] All =
        {
            new LlmPreset
            {
                id = Qwen4B, label = "4B·Qwen3.5 Deckard（速度优先）",
                modelPath = "D:/AI/models/Qwen3.5-4B-Deckard-HERETIC/Qwen3.5-4B-Deckard-HERETIC-UNCENSORED-Thinking.i1-Q4_K_M.gguf",
                loraPath = "", ctx = 32768, ngl = 99,
                note = "实测 44~51 t/s（现役 9B 的 2.7 倍），2.5GB 可与其他软件共存；消融+RP 同一份权重，不需 LoRA；文风偏简洁",
            },
            new LlmPreset
            {
                id = Gemma4B, label = "4B·Gemma-4 E4B（折中）",
                modelPath = "D:/AI/models/Gemma-4-E4B-it-uncensored/gemma-4-E4B-it-uncensored-Q4_K_M.gguf",
                loraPath = "", ctx = 32768, ngl = 42,
                note = "实测 31~40 t/s，4.97GB；中文机关腔最好的一档，另有原生多模态（游戏未用）",
            },
            new LlmPreset
            {
                id = Ornith9B, label = "9B·Ornith-Heretic + RP-LoRA（文风优先）",
                modelPath = "D:/AI/models/Ornith-1.5-9B-Heretic-Q4_K_M/Ornith-1.5-9B-Uncensored-Q4_K_M.gguf",
                loraPath = "D:/AI/models/Qwen3.5-9B-NSFW-RP-LoRA/NSFW-RP-RolePlay.qwen3.5-9b.q8_0.gguf",
                ctx = 16384, ngl = 28,
                note = "实测 16.5 t/s，台词最鲜活（RP-LoRA）；但 5.3GB 在 16GB 机器上与浏览器共存易换页降速",
            },
        };

        public static LlmPreset Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var p in All) if (p.id == id) return p;
            return null;
        }
    }

    /// <summary>
    /// 结构化输出 schema（OpenAI 兼容的 response_format.json_schema）。
    ///
    /// 为什么必须用**完整 schema** 而不是 `{"type":"json_object"}`：
    /// 本机 llama.cpp b10343 对 json_object **不做约束**（实测与不加时一样 58~67% 通过），
    /// 而带 schema（可转 GBNF）时 **12/12 全过、流式也 3/3**，速度不变。
    /// 字段名必须与 LlmJson.ParseTalk / ParseMicro 读的键一致，改这里请同步改解析器。
    /// </summary>
    public static class LlmSchemas
    {
        public const string Talk =
            "{\"type\":\"object\",\"properties\":{" +
            "\"greeting\":{\"type\":\"string\"}," +
            "\"lines\":{\"type\":\"array\",\"items\":{\"type\":\"string\"}}," +
            "\"options\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{" +
            "\"label\":{\"type\":\"string\"},\"mood\":{\"type\":\"string\"},\"reply\":{\"type\":\"string\"}}," +
            "\"required\":[\"label\",\"mood\",\"reply\"]}}}," +
            "\"required\":[\"greeting\",\"lines\",\"options\"]}";

        public const string Micro =
            "{\"type\":\"object\",\"properties\":{" +
            "\"title\":{\"type\":\"string\"}," +
            "\"paras\":{\"type\":\"array\",\"items\":{\"type\":\"string\"}}," +
            "\"options\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{" +
            "\"label\":{\"type\":\"string\"},\"mood\":{\"type\":\"string\"},\"result\":{\"type\":\"string\"}}," +
            "\"required\":[\"label\",\"mood\",\"result\"]}}}," +
            "\"required\":[\"title\",\"paras\",\"options\"]}";

        /// <summary>连通性测试用：只要求一个整数字段。</summary>
        public const string Ping =
            "{\"type\":\"object\",\"properties\":{\"ok\":{\"type\":\"integer\"}},\"required\":[\"ok\"]}";

        public const string SchemaName = "starstate";

        /// <summary>
        /// 自检：schema 是手写拼出来的字符串，很容易在改动时漏括号/错字段名——
        /// 这里用内置迷你 JSON 解析器验一遍（字段名必须与 LlmJson 的读取键一致）。
        /// </summary>
        public static bool Validate(out string error)
        {
            error = "";
            var checks = new[]
            {
                new object[] { "Talk", Talk, new[] { "greeting", "lines", "options" } },
                new object[] { "Micro", Micro, new[] { "title", "paras", "options" } },
                new object[] { "Ping", Ping, new[] { "ok" } },
            };
            foreach (var c in checks)
            {
                string name = (string)c[0];
                var obj = MiniJson.Parse((string)c[1]) as Dictionary<string, object>;
                if (obj == null) { error = name + " 不是合法 JSON 对象"; return false; }
                var props = obj.ContainsKey("properties") ? obj["properties"] as Dictionary<string, object> : null;
                if (props == null) { error = name + " 缺少 properties"; return false; }
                foreach (var k in (string[])c[2])
                    if (!props.ContainsKey(k)) { error = name + " 缺少字段 " + k; return false; }
            }
            return true;
        }
    }

    // ---------------- LLM 输出 DTO（JsonUtility 友好） ----------------

    [Serializable]
    public class TalkOptionDto
    {
        public string label = "";
        public string mood = "neutral";      // warm / neutral / distant
        public string reply = "";
    }

    [Serializable]
    public class NpcTalkDto
    {
        public string greeting = "";
        public string[] lines;
        public TalkOptionDto[] options;
    }

    [Serializable]
    public class MicroOptionDto
    {
        public string label = "";
        public string mood = "neutral";      // good / tough / neutral
        public string result = "";
    }

    [Serializable]
    public class MicroDto
    {
        public string title = "";
        public string[] paras;
        public MicroOptionDto[] options;
    }

    // ---------------- 提示词 ----------------

    public static class LlmPrompt
    {
        public static string System()
        {
            var sb = new StringBuilder();
            sb.Append("你是人生模拟游戏《STARSTATE》的中文叙事引擎。世界观要点：架空王朝“中华帝国”，");
            sb.Append("1700年太祖建国，制度化贤能帝制；官员分官（十品）与吏（三等）两轨；市级行政机关命名“局”；");
            sb.Append("机关内部没有政党；2026年处于AI时代，《AI治理法》确立“算法可以提供意见，不得代替法定权力主体作出最终政治决定”。");
            sb.Append("玩家是大同市人民政府市长（七品·正厅），常委会副主席，每天在办公桌前签批卷宗。\n");
            sb.Append("文风：克制、写实、有机关与公文质感；称呼自然；禁止口号腔、禁止出戏的网络梗。\n");
            sb.Append("硬性要求：只输出一个 JSON 对象，不要代码块标记，不要任何解释文字；JSON 语法符号只用半角双引号，字段名严格按给定的 schema，不要发明新字段，不要在文本中提及任何数值或点数。");
            sb.Append("若用户任务标注【纯文本】，则只输出纯文本本身，不要 JSON、不要代码块。");
            return sb.ToString();
        }

        public static string NpcTalkUser(GameState st, string npcId)
        {
            var def = Npcs.Defs.ContainsKey(npcId) ? Npcs.Defs[npcId] : new Npcs.Def { name = "同事", title = "同事", grade = "", traits = "" };
            var r = Npcs.Get(st, npcId);
            var d = GameClock.Parse(st.date);
            var sb = new StringBuilder();
            sb.Append($"【当前】{GameClock.FmtFull(d)}（{PhaseLabel(st.phase)}）；玩家：{st.player.name}，{st.grade}，{st.player.post}，士气{st.player.morale}，压力{st.player.stress}。\n");
            sb.Append($"【交谈对象】{def.name}，{def.title}（{def.grade}），{def.age}岁，性格：{def.traits}。{def.desc}\n");
            sb.Append($"【双方关系】熟悉{r.familiar}/100，信任{r.trust}，评价{r.evalv}。\n");
            if (r.memories.Count > 0)
            {
                var m = r.memories[r.memories.Count - 1];
                sb.Append($"【最近记忆】{m.date} {m.text}\n");
            }
            sb.Append("【任务】写一段此刻两人的日常交谈（茶水间、走廊或工位皆可），符合其性格与双方关系深浅。\n");
            sb.Append("输出 JSON：{\"greeting\":\"开场（含神态动作，不超过40字）\",\"lines\":[\"后续对话1\",\"后续对话2\"],\"options\":[{\"label\":\"玩家回应（不超过10字）\",\"mood\":\"warm或neutral或distant\",\"reply\":\"对方反应（不超过30字）\"},{\"label\":\"…\",\"mood\":\"…\",\"reply\":\"…\"}]}。options 恰好2个。");
            return sb.ToString();
        }

        public static string MicroUser(GameState st)
        {
            var d = GameClock.Parse(st.date);
            string social = ContentRegistry.Years.ContainsKey(d.Year) ? ContentRegistry.Years[d.Year].social : "";
            var sb = new StringBuilder();
            sb.Append($"【日期】{GameClock.FmtFull(d)}；玩家：{st.player.name}，{st.grade}，大同市人民政府{st.player.post}；精力{st.player.energy}，压力{st.player.stress}；合规{st.compliance}，效率{st.efficiency}。\n");
            if (!string.IsNullOrEmpty(social)) sb.Append($"【当月城市背景】{social}\n");
            sb.Append("【任务】写一件今天发生在市政府办公桌上的不起眼小事（一份材料、一个电话、一场雨、一次排队、一句闲话……），不要戏剧化，不要涉及省管干部任免。\n");
            sb.Append("输出 JSON：{\"title\":\"标题（不超过10字）\",\"paras\":[\"第一段（不超过90字）\",\"第二段（不超过80字）\"],\"options\":[{\"label\":\"玩家选择（不超过10字）\",\"mood\":\"good或tough或neutral\",\"result\":\"结果一句话（不超过40字）\"},{\"label\":\"…\",\"mood\":\"…\",\"result\":\"…\"}]}。options 恰好2个。");
            return sb.ToString();
        }

        /// <summary>周五例会 AI 周评：引用本周真实计划、卷宗与事件（纯文本回复）。</summary>
        public static string WeekReviewUser(GameState st)
        {
            var sb = new StringBuilder();
            var d = GameClock.Parse(st.date);
            string weekStart = GameClock.Iso(GameClock.MondayOf(d));   // 本周一：引用素材必须落在本周
            sb.Append($"【本周】{d.Year}年第{st.week.index}周；玩家：{st.player.name}，{st.grade}，大同市市长。【纯文本】\n");
            sb.Append($"【周计划】签批{st.plan.work} 调研{st.plan.study} 会商{st.plan.social} 关系{st.plan.family} 休整{st.plan.rest}（精力分配）。\n");
            int pending = st.pendingDossierIds.Count + (st.activeDossier != null && !st.activeDossier.resolved ? 1 : 0);
            sb.Append($"【案头】待办卷宗{pending}件；合规{st.compliance} 效率{st.efficiency}；本周程序问题{st.yearIntegrity}。\n");
            if (st.dossierLog != null && st.dossierLog.Count > 0)
            {
                int n = 0;
                for (int i = st.dossierLog.Count - 1; i >= 0 && n < 3; i--)
                {
                    var e = st.dossierLog[i];
                    if (string.CompareOrdinal(e.date ?? "", weekStart) < 0) break;   // 只引本周，不拿上上周的件充数
                    sb.Append($"【本周卷宗】{e.date} {e.disposition}：{e.title}（漏查{e.issuesMissed}）\n");
                    n++;
                }
            }
            int taken = 0;
            for (int i = st.log.Count - 1; i >= 0 && taken < 2; i--)
            {
                var l = st.log[i];
                if (l.kind == "系统" || string.IsNullOrEmpty(l.text)) continue;
                if (string.CompareOrdinal(l.date ?? "", weekStart) < 0) break;       // 同上：只引本周
                sb.Append($"【本周事件】{l.date} {l.text}\n");
                taken++;
            }
            sb.Append("【任务】你是市政府办公厅主任周谨（闸门、周到、嘴严）。写周五你向市长汇报时补的一两句：结合本周真实卷宗与两把尺，具体、克制，带一点挡驾或提醒。不超过60字。");
            return sb.ToString();
        }

        /// <summary>月末家信（父母来信，纯文本）。引用真实住房/婚恋/职级状态，克制温情。</summary>
        public static string FamilyLetterUser(GameState st)
        {
            var d = GameClock.Parse(st.date);
            var sb = new StringBuilder();
            string family = st.hasChild ? "已婚有孩" : st.married ? "已婚" : string.IsNullOrEmpty(st.partner) ? "单身" : "与" + st.partner + "恋爱中";
            sb.Append($"【日期】{GameClock.FmtFull(d)}；玩家：{st.player.name}，{st.grade}，{st.player.post}；住房{st.housing}，{family}；士气{st.player.morale}，压力{st.player.stress}。【纯文本】\n");
            if (d.Month == 1 || d.Month == 2) sb.Append("【时令】临近春节，信里会带一点年味。\n");
            else if (d.Month == 9) sb.Append("【时令】入秋，父母会关心工作是否忙、身体是否吃得消。\n");
            sb.Append("【任务】以父母口吻写一封家信（母亲执笔、父亲补一两句的感觉）：嘘寒问暖但不啰嗦，会提到一件家中具体小事（邻居、天气、腌菜、体检、老同事等），结尾叮嘱保重身体。不要问游戏数值，不要出戏，不要网络梗。不超过120字。");
            return sb.ToString();
        }

        /// <summary>同事微信短讯（纯文本）：从熟悉度较高的 NPC 里抽一人，写 1–2 条工作外的闲话。</summary>
        public static string WeChatUser(GameState st, string npcId)
        {
            var def = Npcs.Defs.ContainsKey(npcId) ? Npcs.Defs[npcId] : new Npcs.Def { name = "同事", title = "同事", grade = "", traits = "普通" };
            var r = Npcs.Get(st, npcId);
            var sb = new StringBuilder();
            sb.Append($"【日期】{st.date}；发送人：{def.name}（{def.title}），性格：{def.traits}；熟悉{r.familiar}/100，信任{r.trust}。【纯文本】\n");
            sb.Append("【任务】写 1—2 条微信消息（可用“对方正在输入…”不必），像同事下班后的闲聊：吐槽食堂、约周末、转发新闻、问材料进展皆可。口语化、短句，不要表情包堆砌，不要超过80字。");
            return sb.ToString();
        }

        /// <summary>M2 每日氛围：办公桌外的世界一句话（纯文本，≤ 40 字）。</summary>
        public static string AmbienceUser(GameState st)
        {
            var d = GameClock.Parse(st.date);
            string social = ContentRegistry.Years.ContainsKey(d.Year) ? ContentRegistry.Years[d.Year].social : "";
            var sb = new StringBuilder();
            sb.Append($"【日期】{GameClock.FmtFull(d)}；地点：大同市人民政府办公楼；玩家：{st.player.name}，大同市市长。【纯文本】\n");
            if (!string.IsNullOrEmpty(social)) sb.Append($"【当月城市背景】{social}\n");
            sb.Append($"【当前】合规{st.compliance}，效率{st.efficiency}，案头待办{st.pendingDossierIds.Count}件，精力{st.player.energy}，压力{st.player.stress}。\n");
            sb.Append("【任务】写**一句话**（不超过 40 字）作为当日的氛围：窗外的天、机关的走廊味、楼道里的脚步声、食堂的菜、一则简短的本地讯息皆可。");
            sb.Append("要克制、具体、有画面感；不要人物对话，不要提问，不要提及任何数值，不要标题。只输出这一句话本身。");
            return sb.ToString();
        }

        /// <summary>M2 批示涓色：把市长刚做的处置涓成一句公文批语（纯文本，≤ 30 字）。</summary>
        public static string RemarkUser(GameState st, string dossierTitle, string optionLabel, string resultText)
        {
            var sb = new StringBuilder();
            sb.Append($"【场景】{st.date}，{st.player.name}（{st.grade}，大同市人民政府市长）在办公桌上签批一份来文。【纯文本】\n");
            sb.Append($"【来文】{dossierTitle}\n");
            sb.Append($"【处置】{optionLabel}\n");
            if (!string.IsNullOrEmpty(resultText)) sb.Append($"【后续】{resultText}\n");
            sb.Append("【任务】写一句他写在卷宗上的批语（不超过 30 字）：公文口吻，文言白话相间，克制；");
            sb.Append("不要“批示：”前缀，不要引号，不要解释，不要提及任何数值或点数。只输出这一句批语。");
            return sb.ToString();
        }

        private static string PhaseLabel(Phase p)
        {
            switch (p)
            {
                case Phase.Prologue: return "序章";
                case Phase.Day: return "工作日";
                case Phase.WeekPlan: return "周计划";
                case Phase.WeekEnd: return "周点评";
                case Phase.Weekend: return "周末";
                case Phase.MonthEnd: return "月度结算";
                default: return "";
            }
        }
    }

    // ---------------- 解析（容错；Core 纯 C#，自带迷你 JSON 解析器） ----------------

    /// <summary>
    /// 极简 JSON 解析：对象→Dictionary，数组→List，标量→string/double/bool/null。
    /// **实例式**（不走静态 pos）——若将来把解析放到后台线程，或两处解析交错，也不会互相踩游标。
    /// </summary>
    internal sealed class MiniJson
    {
        private int pos;

        public static object Parse(string json)
        {
            var p = new MiniJson();
            var v = p.Value(json);
            p.Ws(json);
            return v;
        }

        private void Ws(string s) { while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++; }

        private object Value(string s)
        {
            Ws(s);
            if (pos >= s.Length) throw new FormatException("json eof");
            char c = s[pos];
            if (c == '{') return Obj(s);
            if (c == '[') return Arr(s);
            if (c == '"') return Str(s);
            if (Match(s, "true")) return true;
            if (Match(s, "false")) return false;
            if (Match(s, "null")) return null;
            return Num(s);
        }

        private bool Match(string s, string lit)
        {
            if (pos + lit.Length > s.Length || s.Substring(pos, lit.Length) != lit) return false;
            pos += lit.Length;
            return true;
        }

        private object Obj(string s)
        {
            pos++; // {
            var d = new Dictionary<string, object>();
            Ws(s);
            if (pos < s.Length && s[pos] == '}') { pos++; return d; }
            while (true)
            {
                Ws(s);
                if (pos >= s.Length || s[pos] != '"') throw new FormatException("key expected");
                string k = Str(s);
                Ws(s);
                if (pos >= s.Length || s[pos] != ':') throw new FormatException("colon expected");
                pos++;
                d[k] = Value(s);
                Ws(s);
                if (pos < s.Length && s[pos] == ',') { pos++; continue; }
                if (pos < s.Length && s[pos] == '}') { pos++; return d; }
                throw new FormatException("obj not closed");
            }
        }

        private object Arr(string s)
        {
            pos++; // [
            var l = new List<object>();
            Ws(s);
            if (pos < s.Length && s[pos] == ']') { pos++; return l; }
            while (true)
            {
                l.Add(Value(s));
                Ws(s);
                if (pos < s.Length && s[pos] == ',') { pos++; continue; }
                if (pos < s.Length && s[pos] == ']') { pos++; return l; }
                throw new FormatException("arr not closed");
            }
        }

        private string Str(string s)
        {
            pos++; // "
            var sb = new StringBuilder();
            while (pos < s.Length)
            {
                char c = s[pos];
                if (c == '"') { pos++; return sb.ToString(); }
                if (c == '\\')
                {
                    pos++;
                    if (pos >= s.Length) break;
                    char e = s[pos];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'u':
                            if (pos + 4 < s.Length)
                            {
                                sb.Append((char)Convert.ToInt32(s.Substring(pos + 1, 4), 16));
                                pos += 4;
                            }
                            break;
                    }
                    pos++;
                }
                else { sb.Append(c); pos++; }
            }
            throw new FormatException("string not closed");
        }

        private object Num(string s)
        {
            int start = pos;
            while (pos < s.Length && "-+.eE0123456789".IndexOf(s[pos]) >= 0) pos++;
            double d;
            if (!double.TryParse(s.Substring(start, pos - start), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out d))
                throw new FormatException("bad number");
            return d;
        }
    }

    public static class LlmJson
    {
        /// <summary>从模型输出中提取首个平衡的 JSON 对象（容忍 ``` 包裹与前后杂文）。</summary>
        public static string ExtractFirstJson(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            raw = raw.Replace("```json", "```");
            int start = raw.IndexOf('{');
            if (start < 0) return null;
            bool inStr = false, esc = false;
            int depth = 0;
            for (int i = start; i < raw.Length; i++)
            {
                char c = raw[i];
                if (inStr)
                {
                    if (esc) esc = false;
                    else if (c == '\\') esc = true;
                    else if (c == '"') inStr = false;
                    continue;
                }
                if (c == '"') inStr = true;
                else if (c == '{') depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0) return raw.Substring(start, i - start + 1);
                }
            }
            return null;
        }

        public static NpcTalkDto ParseTalk(string raw)
        {
            string json = ExtractFirstJson(raw);
            if (json == null) return null;
            try
            {
                var obj = MiniJson.Parse(json) as Dictionary<string, object>;
                if (obj == null) return null;
                var dto = new NpcTalkDto { greeting = S(obj, "greeting") };
                dto.lines = StrArray(obj, "lines");
                var opts = ObjArray(obj, "options");
                var list = new List<TalkOptionDto>();
                foreach (var o in opts)
                    list.Add(new TalkOptionDto { label = S(o, "label"), mood = S(o, "mood"), reply = S(o, "reply") });
                dto.options = list.ToArray();
                return Sanitize(dto);
            }
            catch { return null; }
        }

        public static MicroDto ParseMicro(string raw)
        {
            string json = ExtractFirstJson(raw);
            if (json == null) return null;
            try
            {
                var obj = MiniJson.Parse(json) as Dictionary<string, object>;
                if (obj == null) return null;
                var dto = new MicroDto { title = S(obj, "title") };
                dto.paras = StrArray(obj, "paras");
                var opts = ObjArray(obj, "options");
                var list = new List<MicroOptionDto>();
                foreach (var o in opts)
                    list.Add(new MicroOptionDto { label = S(o, "label"), mood = S(o, "mood"), result = S(o, "result") });
                dto.options = list.ToArray();
                return Sanitize(dto);
            }
            catch { return null; }
        }

        private static string S(Dictionary<string, object> d, string k)
        {
            object v;
            return d.TryGetValue(k, out v) && v != null ? v.ToString() : "";
        }

        private static string[] StrArray(Dictionary<string, object> d, string k)
        {
            object v;
            if (!d.TryGetValue(k, out v)) return new string[0];
            var l = v as List<object>;
            if (l == null) return new string[0];
            var arr = new string[l.Count];
            for (int i = 0; i < l.Count; i++) arr[i] = l[i] == null ? "" : l[i].ToString();
            return arr;
        }

        private static List<Dictionary<string, object>> ObjArray(Dictionary<string, object> d, string k)
        {
            var res = new List<Dictionary<string, object>>();
            object v;
            if (!d.TryGetValue(k, out v)) return res;
            var l = v as List<object>;
            if (l == null) return res;
            foreach (var o in l)
            {
                var dd = o as Dictionary<string, object>;
                if (dd != null) res.Add(dd);
            }
            return res;
        }

        private static string Cut(string s, int n)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\n", " ").Trim();
            return s.Length > n ? s.Substring(0, n) : s;
        }

        private static string MoodOr(string m, string fallback)
        {
            return string.IsNullOrEmpty(m) ? fallback : m.Trim().ToLower();
        }

        public static NpcTalkDto Sanitize(NpcTalkDto d)
        {
            if (d == null) return null;
            d.greeting = Cut(d.greeting, 60);
            if (d.lines == null) d.lines = new string[0];
            if (d.lines.Length > 2) Array.Resize(ref d.lines, 2);
            for (int i = 0; i < d.lines.Length; i++) d.lines[i] = Cut(d.lines[i], 90);
            if (d.options == null || d.options.Length == 0)
            {
                d.options = new[]
                {
                    new TalkOptionDto { label = "顺着话头聊聊", mood = "warm", reply = "" },
                    new TalkOptionDto { label = "先去忙了", mood = "neutral", reply = "" },
                };
            }
            if (d.options.Length > 3) Array.Resize(ref d.options, 3);
            foreach (var o in d.options)
            {
                o.label = string.IsNullOrEmpty(Cut(o.label, 12)) ? "回应" : Cut(o.label, 12);
                o.mood = MoodOr(o.mood, "neutral");
                if (o.mood != "warm" && o.mood != "distant") o.mood = "neutral";
                o.reply = Cut(o.reply, 40);
            }
            return d;
        }

        public static MicroDto Sanitize(MicroDto d)
        {
            if (d == null) return null;
            d.title = string.IsNullOrEmpty(Cut(d.title, 12)) ? "今日小插曲" : Cut(d.title, 12);
            if (d.paras == null || d.paras.Length == 0) d.paras = new[] { "平平无奇的一天，公务照旧。" };
            if (d.paras.Length > 3) Array.Resize(ref d.paras, 3);
            for (int i = 0; i < d.paras.Length; i++) d.paras[i] = Cut(d.paras[i], 110);
            if (d.options == null || d.options.Length == 0)
            {
                d.options = new[]
                {
                    new MicroOptionDto { label = "把它办妥", mood = "good", result = "小事办妥，心里踏实。" },
                    new MicroOptionDto { label = "按部就班", mood = "neutral", result = "日子照旧往前。" },
                };
            }
            if (d.options.Length > 3) Array.Resize(ref d.options, 3);
            foreach (var o in d.options)
            {
                o.label = string.IsNullOrEmpty(Cut(o.label, 12)) ? "继续" : Cut(o.label, 12);
                o.mood = MoodOr(o.mood, "neutral");
                if (o.mood != "good" && o.mood != "tough") o.mood = "neutral";
                o.result = Cut(o.result, 50);
            }
            return d;
        }
    }

    // ---------------- 效果白名单与事件构建 ----------------

    public static class LlmGameplay
    {
        /// <summary>交谈选项 → 白名单微效果（含关系记忆），返回给 UI 显示的效果摘要。</summary>
        public static string ApplyTalk(GameState st, string npcId, TalkOptionDto opt)
        {
            if (opt == null) opt = new TalkOptionDto { mood = "neutral" };
            int fam = 0, trust = 0, morale = 0, stress = 0, energy = -2;
            switch (opt.mood)
            {
                case "warm": fam = 2; trust = 2; morale = 1; break;
                case "distant": fam = 1; stress = 1; break;
                default: fam = 1; break;
            }
            Npcs.Mod(st, new RelDelta { id = npcId, familiar = fam, trust = trust, memo = string.IsNullOrEmpty(opt.reply) ? "" : "交谈：" + opt.reply });
            var p = st.player;
            p.morale = Clamp(p.morale + morale, 0, 100);
            p.stress = Clamp(p.stress + stress, 0, 100);
            p.energy = Clamp(p.energy + energy, 0, 100);

            // 记忆只保留最近 6 条，防止长线膨胀
            var r = Npcs.Get(st, npcId);
            while (r.memories.Count > 6) r.memories.RemoveAt(0);

            var parts = new List<string>();
            if (fam != 0) parts.Add("熟悉" + Sig(fam));
            if (trust != 0) parts.Add("信任" + Sig(trust));
            if (morale != 0) parts.Add("士气" + Sig(morale));
            if (stress != 0) parts.Add("压力" + Sig(stress));
            parts.Add("精力" + Sig(energy));
            return string.Join("，", parts.ToArray());
        }

        /// <summary>把模型输出构建成可进主循环的事件（效果只来自情绪白名单，不用模型给的数字）。</summary>
        public static GameEvent BuildMicroEvent(GameState st, MicroDto dto)
        {
            if (dto == null) return null;
            var paras = new List<string>(dto.paras);
            var opts = new List<EventOption>();
            foreach (var o in dto.options)
            {
                Effects fx;
                string tail;
                switch (o.mood)
                {
                    case "good": fx = new Effects { morale = 2, energy = -2 }; tail = "这件小事像午后的茶，回了点甜。"; break;
                    case "tough": fx = new Effects { stress = 2, political = 1 }; tail = "你把这页翻了过去——机关里，有些硬是必须咽的。"; break;
                    default: fx = new Effects { energy = -1 }; tail = "小事归档，日子继续。"; break;
                }
                string result = string.IsNullOrEmpty(o.result) ? tail : o.result;
                opts.Add(new EventOption { label = o.label, effects = fx, result = result });
            }
            return new GameEvent
            {
                id = "_ai_micro",
                type = "society",
                title = "小插曲 · " + dto.title,
                paras = paras,
                options = opts,
            };
        }

        private static string Sig(int v) { return v >= 0 ? "+" + v : v.ToString(); }
        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);

        // ---------------- M2：批示与每日氛围（纯文本，严格回退） ----------------

        /// <summary>
        /// 确定性批语（无 AI 时的回退）：按处置类型给一句公文味道的批语。
        /// 注意：这里**不暴露任何数值**（不写“合规+3”），只写签批的人会写的话。
        /// </summary>
        public static string FallbackRemark(string label, int missed)
        {
            string t = label ?? "";
            string s;
            if (t.Contains("退回") || t.Contains("补正") || t.Contains("补")) s = "退回来文单位，补齐依据后再报。";
            else if (t.Contains("重新") || t.Contains("比价") || t.Contains("核")) s = "请复算后重新报批。";
            else if (t.Contains("请") || t.Contains("请示") || t.Contains("报省")) s = "此事权限不在市里，按程序报省。";
            else if (t.Contains("暂缓") || t.Contains("压") || t.Contains("缓")) s = "先放一放，待方案成熟再议。";
            else if (t.Contains("照准") || t.Contains("同意")) s = "同意，请按规定办理。";
            else s = "按程序办理。";
            if (missed > 0) s += "（签得快，未必看得全。）";
            return s;
        }

        /// <summary>批语清洗：单行、去引号与代码块、限长；不合格返回 null（调用方保留回退批语）。</summary>
        public static string SanitizeRemark(string raw, int maxLen = 40)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            string s = Clean(raw);
            if (s.Length == 0) return null;
            if (s.Length > maxLen) s = s.Substring(0, maxLen).TrimEnd('，', '。', '、', ' ') + "。";
            return s;
        }

        /// <summary>氛围句清洗：单行、无引号、限长（不超过 60 字）。</summary>
        public static string SanitizeAmbience(string raw, int maxLen = 60)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            string s = Clean(raw);
            if (s.Length == 0) return null;
            return s.Length > maxLen ? s.Substring(0, maxLen).TrimEnd('，', '。', '、', ' ') + "。" : s;
        }

        /// <summary>去代码块/引号/换行，压缩空白。</summary>
        static string Clean(string raw)
        {
            string s = raw.Trim();
            s = s.Replace("```json", "").Replace("```", "");
            s = s.Replace("\r", "").Replace("\n", " ");
            s = s.Replace('"', ' ');
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            // 去掉模型爱加的标签前缀
            while (s.StartsWith("批语：") || s.StartsWith("批语:") || s.StartsWith("批示：") || s.StartsWith("批示:"))
                s = s.Substring(3).TrimStart('：', ':', ' ');
            return s.Trim();
        }
    }
}
