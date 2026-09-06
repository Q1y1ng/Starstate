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
        public string modelPath = "D:/AI/models/Ornith-1.5-9B-Heretic-Q4_K_M/Ornith-1.5-9B-Uncensored-Q4_K_M.gguf";
        public string loraPath = "D:/AI/models/Qwen3.5-9B-NSFW-RP-LoRA/NSFW-RP-RolePlay.qwen3.5-9b.q8_0.gguf"; // 默认挂载 RP-LoRA（作者选定：对话最鲜活）
        public int port = 8817;
        public int ctx = 65536;                  // 64K 上下文（Ornith-1.5-9B）
        public float temperature = 0.9f;
        public bool autoStart = true;            // 本地模式下自动拉起 llama-server
    }

    [Serializable]
    public class ChatMsg
    {
        public string role;
        public string content;
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
            sb.Append("玩家是长安市发展和改革局综合科的年轻公务员。\n");
            sb.Append("文风：克制、写实、有机关生活质感；称呼自然；禁止口号腔、禁止出戏的网络梗。\n");
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
            sb.Append($"【日期】{GameClock.FmtFull(d)}；玩家：{st.player.name}，{st.grade}，{st.player.post}；精力{st.player.energy}，压力{st.player.stress}。\n");
            if (!string.IsNullOrEmpty(social)) sb.Append($"【当月城市背景】{social}\n");
            sb.Append("【任务】写一件今天发生在机关里的不起眼的小事（一份材料、一个电话、一场雨、一次排队、一句闲话……），不要戏剧化，不要涉及人事任免。\n");
            sb.Append("输出 JSON：{\"title\":\"标题（不超过10字）\",\"paras\":[\"第一段（不超过90字）\",\"第二段（不超过80字）\"],\"options\":[{\"label\":\"玩家选择（不超过10字）\",\"mood\":\"good或tough或neutral\",\"result\":\"结果一句话（不超过40字）\"},{\"label\":\"…\",\"mood\":\"…\",\"result\":\"…\"}]}。options 恰好2个。");
            return sb.ToString();
        }

        /// <summary>周五例会 AI 科长周评：引用本周真实计划、任务与事件（纯文本回复）。</summary>
        public static string WeekReviewUser(GameState st)
        {
            var sb = new StringBuilder();
            var d = GameClock.Parse(st.date);
            sb.Append($"【本周】{d.Year}年第{st.week.index}周；玩家：{st.player.name}，{st.grade}。【纯文本】\n");
            sb.Append($"【周计划】岗位{st.plan.work} 学习{st.plan.study} 人际{st.plan.social} 家庭{st.plan.family} 休整{st.plan.rest}（精力分配）。\n");
            sb.Append(st.weekEndData != null ? $"【本周任务】{st.week.tasks.Count} 项，综合评级 {st.weekEndData.avgGrade}。\n" : $"【本周任务】{st.week.tasks.Count} 项。\n");
            int taken = 0;
            for (int i = st.log.Count - 1; i >= 0 && taken < 3; i--)
            {
                var l = st.log[i];
                if (l.kind == "系统" || string.IsNullOrEmpty(l.text)) continue;
                sb.Append($"【本周事件】{l.date} {l.text}\n");
                taken++;
            }
            sb.Append("【任务】你是科长周衡之（严谨、护短、话少而准）。写周五例会散会后你对这位科员说的一两句点评：结合本周的真实计划与事件，具体、克制，带一点难得的肯定或一句点到为止的提醒。不超过60字。");
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

    /// <summary>极简 JSON 解析：对象→Dictionary，数组→List，标量→string/double/bool/null。</summary>
    internal static class MiniJson
    {
        private static int pos;

        public static object Parse(string json)
        {
            pos = 0;
            var v = Value(json);
            Ws(json);
            return v;
        }

        private static void Ws(string s) { while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++; }

        private static object Value(string s)
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

        private static bool Match(string s, string lit)
        {
            if (pos + lit.Length > s.Length || s.Substring(pos, lit.Length) != lit) return false;
            pos += lit.Length;
            return true;
        }

        private static object Obj(string s)
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

        private static object Arr(string s)
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

        private static string Str(string s)
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

        private static object Num(string s)
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
    }
}
