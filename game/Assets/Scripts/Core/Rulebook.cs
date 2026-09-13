using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 口径手册条目：案头最多摊 4 份（Papers Please 式桌面）；新件顶掉最旧。
    /// 与卷宗 issue.ruleKey 匹配时，DeskHint 给核对线索。
    /// </summary>
    public class RuleDef
    {
        public string id = "";
        public string title = "";
        public string category = "";    // 数字/程序/土地/算法/统计/安全
        public string body = "";        // 正文：可执行的核对口径，不是抒情
        public string source = "";      // 出处（文号/领导口头，增强质感）
    }

    public static class Rulebook
    {
        public const int DeskSlots = 4;

        static readonly Dictionary<string, RuleDef> byId = new Dictionary<string, RuleDef>();
        static readonly List<string> order = new List<string>();

        public static void ClearRegistry()
        {
            byId.Clear();
            order.Clear();
        }

        public static void Register(RuleDef r)
        {
            if (r == null || string.IsNullOrEmpty(r.id)) return;
            byId[r.id] = r;
            if (!order.Contains(r.id)) order.Add(r.id);
        }

        public static bool IsRegistered(string id) => !string.IsNullOrEmpty(id) && byId.ContainsKey(id);

        /// <summary>注册表是否已装入口径（用于内容自检时区分“没注册”与“还没装”）</summary>
        public static bool AnyRegistered { get { return byId.Count > 0; } }

        public static RuleDef Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            RuleDef d;
            return byId.TryGetValue(id, out d) ? d : null;
        }

        public static string Title(string id)
        {
            var d = Get(id);
            return d != null ? d.title : id;
        }

        static void EnsureDesk(GameState st, string ruleId)
        {
            if (st.deskRules == null) st.deskRules = new List<string>();
            if (st.deskRules.Contains(ruleId)) return;
            // 满员：始终顶掉最旧（案头第一位）；若被顶的是正在摊开的，则收起
            while (st.deskRules.Count >= DeskSlots)
            {
                string kicked = st.deskRules[0];
                st.deskRules.RemoveAt(0);
                if (st.openRule == kicked) st.openRule = "";
            }
            st.deskRules.Add(ruleId);
        }

        /// <summary>授予：入档案全集，并尽量上案头（满则顶旧）。</summary>
        public static void Grant(GameState st, string ruleId)
        {
            if (st == null || string.IsNullOrEmpty(ruleId) || !byId.ContainsKey(ruleId)) return;
            if (!st.knownRules.Contains(ruleId)) st.knownRules.Add(ruleId);
            EnsureDesk(st, ruleId);
            if (string.IsNullOrEmpty(st.openRule)) st.openRule = ruleId;
        }

        /// <summary>摊开到案头：已知即可；不在案头则先上案头（满则顶旧）。</summary>
        public static bool Open(GameState st, string ruleId)
        {
            if (st == null || string.IsNullOrEmpty(ruleId)) return false;
            if (!st.knownRules.Contains(ruleId)) return false;
            EnsureDesk(st, ruleId);
            st.openRule = ruleId;
            return true;
        }

        public static void CloseDesk(GameState st)
        {
            if (st != null) st.openRule = "";
        }

        public static bool OnDesk(GameState st, string ruleId)
        {
            return st != null && st.deskRules != null && st.deskRules.Contains(ruleId);
        }

        /// <summary>档案检索：标题/类别/正文/出处包含关键字（空=全部）。</summary>
        public static List<string> Search(GameState st, string query)
        {
            var result = new List<string>();
            if (st == null || st.knownRules == null) return result;
            string q = string.IsNullOrEmpty(query) ? "" : query.Trim();
            foreach (var id in st.knownRules)
            {
                if (q.Length == 0) { result.Add(id); continue; }
                var d = Get(id);
                if (d == null) continue;
                if (d.title.Contains(q) || d.category.Contains(q) || d.body.Contains(q) || d.source.Contains(q))
                    result.Add(id);
            }
            return result;
        }

        /// <summary>案头口径是否命中该 issue（用于核对加成）。</summary>
        public static bool MatchesOpen(GameState st, DossierIssue issue)
        {
            if (st == null || issue == null) return false;
            if (string.IsNullOrEmpty(st.openRule)) return false;
            if (string.IsNullOrEmpty(issue.ruleKey)) return false;
            return st.openRule == issue.ruleKey;
        }

        /// <summary>该卷宗是否存在与案头口径匹配的未发现问题。</summary>
        public static DossierIssue FindOpenRuleIssue(GameState st, Dossier d)
        {
            if (st == null || d == null || d.issues == null) return null;
            if (string.IsNullOrEmpty(st.openRule)) return null;
            foreach (var i in d.issues)
            {
                if (i.discovered) continue;
                if (!string.IsNullOrEmpty(i.ruleKey) && i.ruleKey == st.openRule) return i;
            }
            return null;
        }

        /// <summary>案头口径的「核对提示」：命中则给 detectHint；未命中给通用程序提示。</summary>
        public static string DeskHint(GameState st, Dossier d)
        {
            if (st == null || d == null) return "";
            var open = Get(st.openRule);
            if (open == null) return "";
            var hit = FindOpenRuleIssue(st, d);
            if (hit != null)
                return $"案头《{open.title}》：{hit.detectHint}";
            return $"案头《{open.title}》——本件暂未对上该口径，仍可按程序核对。";
        }
    }
}
