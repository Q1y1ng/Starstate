using System;
using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 卷宗引擎（Phase 5 Papers Please）：登记、周分配、核对、签批结算、两把尺。
    /// 高光/教学件手写注册；模板件由 DossierGenerator 填充。
    /// </summary>
    public static class DossierEngine
    {
        static readonly Dictionary<string, Dossier> byId = new Dictionary<string, Dossier>();
        static readonly List<string> poolIds = new List<string>();
        static readonly Random rng = new Random(20260901);

        public static void ClearRuntime() { RuntimeInstances.Clear(); }

        /// <summary>仅测试/热重载：清空注册表。新局勿调用。</summary>
        public static void Clear() { byId.Clear(); poolIds.Clear(); RuntimeInstances.Clear(); }

        public static bool IsRegistered(string id) => byId.ContainsKey(id);

        public static void Register(Dossier d)
        {
            if (d == null || string.IsNullOrEmpty(d.id)) return;
            byId[d.id] = d;
        }

        /// <summary>入随机池（kind 非 showcase 的模板件）。</summary>
        public static void RegisterPool(Dossier d)
        {
            Register(d);
            if (!poolIds.Contains(d.id)) poolIds.Add(d.id);
        }

        public static Dossier Clone(string id)
        {
            if (!byId.TryGetValue(id, out var src)) return null;
            // JsonUtility 不可用（Core 禁 UnityEngine）——浅克隆列表
            var d = new Dossier
            {
                id = src.id,
                kind = src.kind,
                form = src.form,
                title = src.title,
                docNo = src.docNo,
                org = src.org,
                deadline = src.deadline,
                checkBudget = src.checkBudget,
                generated = src.generated,
            };
            foreach (var p in src.pages)
            {
                var np = new DossierPage { title = p.title, table = p.table };
                if (p.paras != null) np.paras.AddRange(p.paras);
                d.pages.Add(np);
            }
            foreach (var i in src.issues)
                d.issues.Add(new DossierIssue { id = i.id, pageRef = i.pageRef, detectHint = i.detectHint, ruleKey = i.ruleKey, severity = i.severity });
            foreach (var o in src.options)
            {
                var no = new DossierOption
                {
                    label = o.label,
                    whenMark = o.whenMark,
                    whenNotMark = o.whenNotMark,
                    lockReason = o.lockReason,
                    result = o.result,
                    complianceDelta = o.complianceDelta,
                    efficiencyDelta = o.efficiencyDelta,
                    gray = o.gray,
                    effects = Flow.CloneEffects(o.effects),
                };
                d.options.Add(no);
            }
            return d;
        }

        /// <summary>周一：按本周预算装配待办卷宗（教学件优先，再抽池）。</summary>
        public static void AssignWeek(GameState st, IEnumerable<string> forcedIds = null)
        {
            st.pendingDossierIds.Clear();
            if (forcedIds != null)
            {
                foreach (var id in forcedIds)
                {
                    if (IsRegistered(id) && !st.pendingDossierIds.Contains(id))
                        st.pendingDossierIds.Add(id);
                }
                return;
            }
            int budget = Math.Max(3, st.weekDossierBudget);
            var taken = new HashSet<string>(st.dossierFired);

            // 1. 手写 showcase / deadline 优先补位（按注册序，未用过）
            foreach (var id in byId.Keys)
            {
                if (st.pendingDossierIds.Count >= budget) break;
                var d = byId[id];
                if (d.kind != "showcase" && d.kind != "deadline") continue;
                if (taken.Contains(id)) continue;
                if (d.kind == "deadline" && d.deadline != null && string.Compare(d.deadline, st.date) < 0) continue;
                st.pendingDossierIds.Add(id);
            }

            // 2. 随机池补满
            var candidates = new List<string>();
            foreach (var id in poolIds)
            {
                if (taken.Contains(id) || st.pendingDossierIds.Contains(id)) continue;
                candidates.Add(id);
            }
            // 加权洗牌（weight 暂按 1）
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = candidates[i]; candidates[i] = candidates[j]; candidates[j] = tmp;
            }
            foreach (var id in candidates)
            {
                if (st.pendingDossierIds.Count >= budget) break;
                st.pendingDossierIds.Add(id);
            }
        }

        /// <summary>打开下一件待办；无则 false。</summary>
        public static bool OpenNext(GameState st)
        {
            if (st.activeDossier != null && !st.activeDossier.resolved) return true;
            if (st.pendingDossierIds.Count == 0) return false;
            string id = st.pendingDossierIds[0];
            st.pendingDossierIds.RemoveAt(0);
            var proto = Clone(id);
            if (proto == null) return false;
            // 运行时实例挂 runtime 侧：用 openDossierIds + 全局注册表克隆缓存
            RuntimeInstances[id] = proto;
            if (!st.openDossierIds.Contains(id)) st.openDossierIds.Add(id);
            st.activeDossier = new ActiveDossier
            {
                id = id,
                page = 1,
                checksLeft = Math.Max(1, proto.checkBudget),
            };
            return true;
        }

        static readonly Dictionary<string, Dossier> RuntimeInstances = new Dictionary<string, Dossier>();

        public static Dossier Current(GameState st)
        {
            if (st?.activeDossier == null) return null;
            if (RuntimeInstances.TryGetValue(st.activeDossier.id, out var d)) return d;
            return Clone(st.activeDossier.id);
        }

        public static void TurnPage(GameState st, int delta)
        {
            var act = st?.activeDossier;
            var d = Current(st);
            if (act == null || d == null || d.pages.Count == 0) return;
            act.page = Math.Max(1, Math.Min(d.pages.Count, act.page + delta));
        }

        /// <summary>核对当前页：发现埋在本页的雷。案头口径的加成在呈现层（DeskHint）与 Flow 文案。</summary>
        public static DossierIssue CheckPage(GameState st)
        {
            var act = st?.activeDossier;
            var d = Current(st);
            if (act == null || d == null || act.checksLeft <= 0) return null;
            act.checksLeft--;
            foreach (var issue in d.issues)
            {
                if (issue.discovered) continue;
                if (act.foundIssues.Contains(issue.id)) continue;
                int pageRef = 1;
                int.TryParse(issue.pageRef, out pageRef);
                if (pageRef < 1) pageRef = 1;
                if (pageRef != act.page) continue;
                issue.discovered = true;
                act.foundIssues.Add(issue.id);
                return issue;
            }
            return null;
        }

        public static bool OptionAvailable(GameState st, DossierOption o, out string lockReason)
        {
            lockReason = "";
            if (o == null) { lockReason = "无效选项"; return false; }
            if (!string.IsNullOrEmpty(o.whenNotMark) && st.HasMark(o.whenNotMark))
            {
                lockReason = string.IsNullOrEmpty(o.lockReason) ? "时机已过" : o.lockReason;
                return false;
            }
            if (!string.IsNullOrEmpty(o.whenMark) && !st.HasMark(o.whenMark))
            {
                lockReason = string.IsNullOrEmpty(o.lockReason) ? "还缺一个契机" : o.lockReason;
                return false;
            }
            return true;
        }

        /// <summary>签批结算：未查出的雷按严重度扣两把尺；写档案与日志；关卷。</summary>
        public static DossierLogEntry Resolve(GameState st, int optionIndex)
        {
            var act = st?.activeDossier;
            var d = Current(st);
            if (act == null || d == null || act.resolved) return null;
            if (optionIndex < 0 || optionIndex >= d.options.Count) return null;
            var opt = d.options[optionIndex];

            int missed = 0, found = act.foundIssues.Count;
            int severitySum = 0;
            foreach (var issue in d.issues)
            {
                if (!act.foundIssues.Contains(issue.id))
                {
                    missed++;
                    severitySum += Math.Max(1, issue.severity);
                }
            }

            int cDelta = opt.complianceDelta - severitySum * 4;
            int eDelta = opt.efficiencyDelta;
            bool overdue = !string.IsNullOrEmpty(d.deadline) && string.Compare(st.date, d.deadline) > 0;
            if (overdue) eDelta -= 8;

            st.compliance = Math.Max(0, Math.Min(100, st.compliance + cDelta));
            st.efficiency = Math.Max(0, Math.Min(100, st.efficiency + eDelta));
            if (missed > 0) st.yearIntegrity += 1;
            st.yearGradePoints += (cDelta >= 0 ? 2 : -2) + (eDelta >= 0 ? 1 : -1);
            st.yearTaskCount++;

            if (opt.effects != null) Flow.ApplyEffects(st, Flow.CloneEffects(opt.effects));

            var entry = new DossierLogEntry
            {
                date = st.date,
                dossierId = d.id,
                title = d.title,
                disposition = opt.label,
                issuesMissed = missed,
                issuesFound = found,
                complianceDelta = cDelta,
                efficiencyDelta = eDelta,
                overdue = overdue,
            };
            st.dossierLog.Add(entry);
            if (st.dossierLog.Count > 400) st.dossierLog.RemoveRange(0, 80);
            if (!st.dossierFired.Contains(d.id)) st.dossierFired.Add(d.id);
            st.openDossierIds.Remove(d.id);
            RuntimeInstances.Remove(d.id);

            act.resolved = true;
            act.pendingResultTitle = string.IsNullOrEmpty(opt.result) ? d.title : d.title;
            act.pendingResultParas = new List<string>();
            if (!string.IsNullOrEmpty(opt.result)) act.pendingResultParas.Add(opt.result);
            if (missed > 0)
                act.pendingResultParas.Add($"（材料里还有 {missed} 处你没看见的问题。签字就是责任。）");
            else if (d.issues.Count > 0 && found == d.issues.Count)
                act.pendingResultParas.Add("（该查的你都查了。批语落在纸上，轻，却压得住人。）");

            st.AddLog("卷宗", $"{opt.label}：{d.title}" + (missed > 0 ? $"（漏查 {missed}）" : ""));
            return entry;
        }

        public static void CloseActive(GameState st)
        {
            if (st?.activeDossier == null) return;
            RuntimeInstances.Remove(st.activeDossier.id);
            st.activeDossier = null;
        }

        /// <summary>本周是否还有未办结。</summary>
        public static bool HasWork(GameState st)
        {
            if (st == null) return false;
            if (st.activeDossier != null && !st.activeDossier.resolved) return true;
            return st.pendingDossierIds.Count > 0;
        }

        public static void ClampScores(GameState st)
        {
            st.compliance = Math.Max(0, Math.Min(100, st.compliance));
            st.efficiency = Math.Max(0, Math.Min(100, st.efficiency));
        }
    }
}
