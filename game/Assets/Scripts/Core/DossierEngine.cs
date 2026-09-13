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
        static readonly List<string> order = new List<string>();   // 注册顺序（不依赖 Dictionary.Keys 枚举顺序）
        static readonly Random rng = new Random(20260901);

        /// <summary>注册期自检发现的问题（pageRef 越界 / ruleKey 未注册 / 无页面 / 无选项）。内容门禁用。</summary>
        public static readonly List<string> ValidationErrors = new List<string>();

        public static void ClearRuntime() { RuntimeInstances.Clear(); }

        /// <summary>仅测试/热重载：清空注册表。新局勿调用。</summary>
        public static void Clear()
        {
            byId.Clear(); poolIds.Clear(); order.Clear(); RuntimeInstances.Clear(); ValidationErrors.Clear();
        }

        public static bool IsRegistered(string id) => byId.ContainsKey(id);

        /// <summary>注册表 id 快照（内容审计/测试用；副本，调用方改不了注册表）。</summary>
        public static List<string> RegisteredIds() { return new List<string>(order); }

        public static void Register(Dossier d)
        {
            if (d == null || string.IsNullOrEmpty(d.id)) return;
            byId[d.id] = d;
            if (!order.Contains(d.id)) order.Add(d.id);
            Validate(d);
        }

        /// <summary>注册期自检：埋雷指向不存在的页 / 口径未登记 → 该雷永远查不出或提示对不上。</summary>
        static void Validate(Dossier d)
        {
            int pages = d.pages != null ? d.pages.Count : 0;
            if (pages == 0) ValidationErrors.Add(d.id + "：无页面");
            if (d.options == null || d.options.Count == 0) ValidationErrors.Add(d.id + "：无处置选项");
            if (d.issues == null) return;
            foreach (var i in d.issues)
            {
                int p = 1;
                int.TryParse(i.pageRef, out p);
                if (p < 1 || p > pages)
                    ValidationErrors.Add(d.id + "：issue " + i.id + " pageRef=" + i.pageRef + " 超出页数 " + pages);
                if (!string.IsNullOrEmpty(i.ruleKey) && Rulebook.AnyRegistered && !Rulebook.IsRegistered(i.ruleKey))
                    ValidationErrors.Add(d.id + "：issue " + i.id + " ruleKey 未注册 " + i.ruleKey);
            }
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

        /// <summary>周一：按本周预算装配待办卷宗（积压→教学/高光→随机池→按需生成补货）。</summary>
        public static void AssignWeek(GameState st, IEnumerable<string> forcedIds = null)
        {
            int budget = Math.Max(3, st.weekDossierBudget);
            var carry = new List<string>(st.pendingDossierIds);
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
            var taken = new HashSet<string>(st.dossierFired);

            // 0. 上周未办结的件先压在案头（积压也是一把尺）
            foreach (var id in carry)
            {
                if (st.pendingDossierIds.Count >= budget) break;
                if (taken.Contains(id) || st.pendingDossierIds.Contains(id)) continue;
                st.pendingDossierIds.Add(id);
            }

            // 1. 手写 showcase / deadline 优先补位（按注册序，未用过，且已到投放档期）
            foreach (var id in order)
            {
                if (st.pendingDossierIds.Count >= budget) break;
                Dossier d;
                if (!byId.TryGetValue(id, out d)) continue;
                if (d.kind != "showcase" && d.kind != "deadline") continue;
                if (taken.Contains(id)) continue;
                // 季节档期：未到 releaseFrom 不投放（否则防汛/巡视类件会在九月被提前消费）
                if (!string.IsNullOrEmpty(d.releaseFrom) && string.Compare(d.releaseFrom, st.date) > 0) continue;
                if (d.kind == "deadline" && !string.IsNullOrEmpty(d.deadline) && string.Compare(d.deadline, st.date) < 0) continue;
                st.pendingDossierIds.Add(id);
            }

            // 2. 随机池补满（季节档期同样要卡：手写件是 RegisterPool 进来的，会从这里绕回案头）
            var candidates = new List<string>();
            foreach (var id in poolIds)
            {
                if (taken.Contains(id) || st.pendingDossierIds.Contains(id)) continue;
                Dossier pd;
                if (byId.TryGetValue(id, out pd)
                    && !string.IsNullOrEmpty(pd.releaseFrom) && string.Compare(pd.releaseFrom, st.date) > 0)
                    continue;   // 未到投放档期
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

            // 3. 仍不够 → 按需生成模板件（十年长线不能依赖启动时那 36 件的播种）
            //    不预先登记：id 由存档内序号派生且生成过程纯确定，需要时用 Respawn 原样重建。
            while (st.pendingDossierIds.Count < budget)
            {
                st.dossierSeq++;
                string genId = DossierGenerator.SpawnOne(GameClock.Parse(st.date), st.dossierSeq).id;
                if (taken.Contains(genId) || st.pendingDossierIds.Contains(genId)) continue;
                st.pendingDossierIds.Add(genId);
            }
        }

        /// <summary>打开下一件待办；无则 false。</summary>
        public static bool OpenNext(GameState st)
        {
            if (st.activeDossier != null && !st.activeDossier.resolved) return true;
            if (st.pendingDossierIds.Count == 0) return false;
            string id = st.pendingDossierIds[0];
            st.pendingDossierIds.RemoveAt(0);
            var proto = ResolveDefinition(st, id, true);   // 注册/重建：读档后不再出现“取不到定义”
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

        /// <summary>取卷宗定义：注册表克隆 → 按 id 原样重建（模板件 id 可由存档内序号反解）。</summary>
        static Dossier ResolveDefinition(GameState st, string id, bool register)
        {
            var d = Clone(id);
            if (d != null) return d;
            var gen = DossierGenerator.Respawn(id, GameClock.Parse(st.date));
            if (gen != null && register) Register(gen);
            return gen;
        }

        public static Dossier Current(GameState st)
        {
            if (st?.activeDossier == null) return null;
            Dossier d;
            if (RuntimeInstances.TryGetValue(st.activeDossier.id, out d)) return d;
            return ResolveDefinition(st, st.activeDossier.id, false);
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

        /// <summary>
        /// “尽责市长”启发式：供**快进静默办结**与**平衡探针**使用。
        ///
        /// 绝不能只看 complianceDelta：本项目里灰区处置常常“面子上合规分更高”
        /// （“压一压”“内部消化”“以整改为主”），却会写程序违规记录。
        /// 旧实现只看合规分，于是自动推进十年必然把违规记录攒到满（探针实测：18 个月 10 次）。
        /// </summary>
        public static int BestOptionIndex(GameState st, Dossier d)
        {
            if (d == null || d.options == null || d.options.Count == 0) return 0;
            int best = 0, bestScore = int.MinValue;
            for (int i = 0; i < d.options.Count; i++)
            {
                string reason;
                if (!OptionAvailable(st, d.options[i], out reason)) continue;
                var o = d.options[i];
                int score = o.complianceDelta + o.efficiencyDelta / 2;
                if (o.gray) score -= 60;
                if (o.effects != null && o.effects.integrity != null) score -= 120;
                if (o.effects != null && o.effects.setMarks != null)
                    foreach (var m in o.effects.setMarks)
                        if (RiskMarks.Contains(m)) score -= 20;
                if (score > bestScore) { bestScore = score; best = i; }
            }
            return best;
        }

        /// <summary>会挂上后续代价的灰区标记（快进启发式里额外扣分，让自动推进不会主动积账）。</summary>
        static readonly HashSet<string> RiskMarks = new HashSet<string>
        {
            "stat_fudge", "yq_delete", "safety_loose", "budget_split", "procure_hold",
            "petition_paper", "yibao_soft", "land_directed", "avoid_soft", "xun_paper_close",
            "province_report_loose", "province_dd_selfcheck", "home_vague", "med_favor",
        };

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

        /// <summary>卷宗签批结算：未查出的雷按严重度扣两把尺；写档案与日志；关卷。</summary>
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
            // 按时办结且无漏查 → 效率不再倒扣（办得干净本身就是效率），但也不会凭空增长：
            // 效率的正增长只能来自“敢拍板”的处置（照准/特事特办），这就是两把尺的张力所在。
            if (missed == 0 && !overdue && eDelta < 0) eDelta = 0;

            // —— 风险账本（M3）：合规分是“当下评价”，风险账本是“历史存疑”，两者走向可以相反 ——
            // 制度不会因为你签得快而不记账；灰区处置、漏查重大雷、逾期都在账上。
            int risk = 0;
            if (opt.gray) risk += 2;
            if (overdue) risk += 1;
            foreach (var issue in d.issues)
                if (!act.foundIssues.Contains(issue.id) && issue.severity >= 3) risk += 2 + issue.severity;
            if (risk > 0) Flow.AddRisk(st, risk, "卷宗：" + d.title);

            st.compliance = Math.Max(0, Math.Min(100, st.compliance + cDelta));
            st.efficiency = Math.Max(0, Math.Min(100, st.efficiency + eDelta));
            if (missed > 0) st.yearIntegrity += 1;
            st.month.dossiersResolved++;
            st.month.missedIssues += missed;
            if (overdue) st.month.overdueCount++;
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
            act.pendingResultTitle = d.title;
            act.pendingResultParas = new List<string>();
            if (!string.IsNullOrEmpty(opt.result)) act.pendingResultParas.Add(opt.result);
            if (missed > 0)
                act.pendingResultParas.Add($"（材料里还有 {missed} 处你没看见的问题。签字就是责任。）");
            else if (d.issues.Count > 0 && found == d.issues.Count)
                act.pendingResultParas.Add("（该查的你都查了。批语落在纸上，轻，却压得住人。）");

            st.AddLog("卷宗", $"{opt.label}：{d.title}" + (missed > 0 ? $"（漏查 {missed}）" : ""));

            // 批示：先落一条确定性批语（无 AI 时就是它），有 AI 时 UI 会在后台请模型涓色后覆盖。
            // 放在 Core 里而不是 UI：无 AI 的机器上也要有“签批”的味道。
            st.pendingDossierId = d.id;
            st.remarkDossier = d.id;
            st.remark = LlmGameplay.FallbackRemark(opt.label, missed);
            st.remarkAi = false;
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
