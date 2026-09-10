using System;
using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 流程引擎 v2：事件调度（精确/每年循环/标记/随机/动态）、场景状态机、效果结算、
    /// 任务检定、周月节奏、时间快进、年度考核与结局。纯 C#（无 UnityEngine 依赖）。
    /// </summary>
    public static class Flow
    {
        private static readonly List<GameEvent> All = new List<GameEvent>();
        private static readonly Dictionary<string, GameEvent> ById = new Dictionary<string, GameEvent>();
        private static readonly List<string> RandomPool = new List<string>();
        private static readonly Random Rng = new Random();

        private static readonly int[] GradeEval = { 4, 3, 1, -1, -3 };      // S A B C D → 科长评价
        private static readonly int[] GradeMorale = { 3, 2, 0, -2, -4 };    // S A B C D → 士气
        private static readonly string[] GradeNames = { "S", "A", "B", "C", "D" };

        private const double PoolDailyChance = 0.45;   // 工作日随机池每日抽取概率（Phase 4 加权单抽）

        // ---------------- 注册 ----------------

        public static void Register(GameEvent ev)
        {
            if (ById.ContainsKey(ev.id)) return;
            All.Add(ev);
            ById.Add(ev.id, ev);
            if (ev.when != null && ev.when.randomP > 0) RandomPool.Add(ev.id);
        }

        /// <summary>测试/工具：事件是否已注册。</summary>
        public static bool IsRegistered(string id) => !string.IsNullOrEmpty(id) && ById.ContainsKey(id);

        // ---------------- 生命周期 ----------------

        public static void Begin(GameState st)
        {
            Npcs.Ensure(st);
            st.phase = Phase.Prologue;
            st.date = GameClock.Iso(GameClock.GameStart);
            st.queue.Clear();
            st.queue.Add("pre0"); st.queue.Add("pre1"); st.queue.Add("pre_grand"); st.queue.Add("p1");
            st.queue.Add("pre_exam"); st.queue.Add("pre_city"); st.queue.Add("p0"); st.queue.Add("p_ambition");
            st.queue.Add("p2");
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.hasPending = false;
            st.week.index = 1;
            st.month.key = GameClock.MonthKey(GameClock.GameStart);
        }

        private static DateTime Today(GameState st) => GameClock.Parse(st.date);

        private static void CollectDue(GameState st, DateTime d)
        {
            Clocks.DailyTick(st);
            st.queue.Clear();
            string iso = GameClock.Iso(d);
            string md = d.ToString("MM-dd");

            // ① 回响队列：到期的延迟事件（选择的长影子；带 flag/标记门槛的回响未达标即耗散）
            for (int i = st.echoQueue.Count - 1; i >= 0; i--)
            {
                var j = st.echoQueue[i];
                if (string.CompareOrdinal(j.dueOn ?? "", iso) > 0) continue;
                st.echoQueue.RemoveAt(i);
                if (!ById.ContainsKey(j.eventId)) continue;
                var def = ById[j.eventId];
                var w = def.when;
                if (w != null && !string.IsNullOrEmpty(w.flag) && !st.GetFlag(w.flag)) continue;
                if (!MarksMet(def, st)) continue;
                if (st.Fired(j.eventId)) continue;
                st.queue.Add(j.eventId);
                st.MarkFired(j.eventId);
            }

            // ② 日期 / 每年循环 / 标记触发（每年与标记路径增加叙事标记门槛）
            foreach (var ev in All)
            {
                var w = ev.when;
                if (w == null) continue;
                if (!string.IsNullOrEmpty(w.date))
                {
                    // 日期触发可叠加标记/路线门槛（date+flag 组合）
                    bool flagOk = string.IsNullOrEmpty(w.flag) || st.GetFlag(w.flag);
                    bool routeOk = string.IsNullOrEmpty(w.route) || st.route == w.route;
                    if (w.date == iso && flagOk && routeOk && !st.Fired(ev.id)) { st.queue.Add(ev.id); st.MarkFired(ev.id); }
                    continue;
                }
                if (!string.IsNullOrEmpty(w.md))
                {
                    if (d.Year >= (w.fromYear == 0 ? 2027 : w.fromYear) && w.md == md
                        && !st.Fired(ev.id + "@" + d.Year) && MarksMet(ev, st))
                    {
                        st.queue.Add(ev.id); st.MarkFired(ev.id + "@" + d.Year);
                    }
                    continue;
                }
                if (!string.IsNullOrEmpty(w.flag))
                {
                    if (st.GetFlag(w.flag) && !st.Fired(ev.id) && MarksMet(ev, st)) { st.queue.Add(ev.id); st.MarkFired(ev.id); }
                }
            }

            // ③ 随机池：加权抽取（事件不再“触发即删”，支持次数/冷却/标记/志向门槛）
            PoolRoll(st, d, iso);

            // ④ 时钟满格 → 触发事件
            CheckClocks(st);
        }

        /// <summary>随机池抽取：候选（未超次数、不在冷却、标记/志向匹配）按权重单抽一件。</summary>
        private static void PoolRoll(GameState st, DateTime d, string iso)
        {
            if (!GameClock.IsWorkday(d)) return;
            var cands = new List<GameEvent>();
            var weights = new List<double>();
            double total = 0;
            foreach (var id in RandomPool)
            {
                var ev = ById[id];
                int max = ev.when.maxFires <= 0 ? 1 : ev.when.maxFires;
                if (st.FireCount(id) >= max) continue;
                if (ev.when.cooldownDays > 0 && LastFireDaysAgo(st, id, iso) < ev.when.cooldownDays) continue;
                if (!MarksMet(ev, st)) continue;
                if (!string.IsNullOrEmpty(ev.when.ambition) && ev.when.ambition != st.ambition) continue;
                double w = ev.when.weight > 0 ? ev.when.weight : 1.0;
                cands.Add(ev); weights.Add(w); total += w;
            }
            if (cands.Count == 0 || Rng.NextDouble() >= PoolDailyChance) return;

            double r = Rng.NextDouble() * total;
            int pick = cands.Count - 1;
            for (int i = 0; i < cands.Count; i++) { r -= weights[i]; if (r <= 0) { pick = i; break; } }
            var chosen = cands[pick];
            st.queue.Add(chosen.id);
            st.MarkFireAdditional(chosen.id);
            st.poolLog.Add(chosen.id + "@" + iso);
            if (st.poolLog.Count > 500) st.poolLog.RemoveRange(0, st.poolLog.Count - 500);
        }

        private static int LastFireDaysAgo(GameState st, string id, string iso)
        {
            string prefix = id + "@";
            for (int i = st.poolLog.Count - 1; i >= 0; i--)
            {
                if (!st.poolLog[i].StartsWith(prefix)) continue;
                try
                {
                    return (int)GameClock.Parse(iso).Subtract(GameClock.Parse(st.poolLog[i].Substring(prefix.Length))).TotalDays;
                }
                catch { return int.MaxValue; }
            }
            return int.MaxValue;
        }

        /// <summary>事件级叙事标记门槛（requireMarks / requireNotMarks）。when 为 null 的事件视为无门槛。</summary>
        private static bool MarksMet(GameEvent ev, GameState st)
        {
            var w = ev.when;
            if (w == null) return true;
            if (w.requireMarks != null)
                foreach (var m in w.requireMarks) if (!st.HasMark(m)) return false;
            if (w.requireNotMarks != null)
                foreach (var m in w.requireNotMarks) if (st.HasMark(m)) return false;
            return true;
        }

        /// <summary>时钟满格 → 入队触发事件（每时钟一次）。</summary>
        private static void CheckClocks(GameState st)
        {
            foreach (var c in st.clocks)
            {
                if (c.value < c.max || string.IsNullOrEmpty(c.onFullEventId)) continue;
                string key = "clockfull_" + c.id;
                if (st.Fired(key) || !ById.ContainsKey(c.onFullEventId)) continue;
                st.queue.Add(c.onFullEventId);
                st.MarkFired(key);
            }
        }

        // ---------------- 回响与入队（Phase 4 叙事引擎） ----------------

        /// <summary>安排延迟回响：afterDays 天后该事件自动入队（选择的长影子）。</summary>
        public static void ScheduleEcho(GameState st, string eventId, int afterDays)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            st.echoQueue.Add(new EchoJob
            {
                eventId = eventId,
                dueOn = GameClock.Iso(GameClock.AddDays(GameClock.Parse(st.date), Math.Max(1, afterDays))),
            });
        }

        /// <summary>注册表里存在才入队；动态事件可反复触发，脚本事件一生一次。</summary>
        public static bool TryEnqueueIfRegistered(GameState st, string id)
        {
            if (!ById.ContainsKey(id)) return false;
            if (!ById[id].dynamic)
            {
                if (st.Fired(id)) return false;
                st.MarkFired(id);
            }
            st.queue.Add(id);
            return true;
        }

        internal static double RngNextDouble() => Rng.NextDouble();

        // ---------------- 场景呈现 ----------------

        public static Scene CurrentScene(GameState st)
        {
            if (st.hasPending)
                return new Scene { kind = "result", title = st.pendingTitle, paras = st.pendingParas, options = new List<string> { "继 续" } };

            switch (st.phase)
            {
                case Phase.WeekPlan: return WeekPlanScene(st);
                case Phase.WeekEnd: return WeekEndScene(st);
                case Phase.Weekend: return WeekendScene(st);
                case Phase.MonthEnd: return MonthEndScene(st);
                case Phase.Ending:
                    var e = st.endingData ?? Career.ComputeEnding(st);
                    return new Scene { kind = "ending", title = e.title, paras = e.paras, options = new List<string> { "再走一遍（新游戏）", "离开" } };
                default: return EventScene(st); // Prologue / Day
            }
        }

        private static string Fill(string text, GameState st)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("{你}", st.player.name).Replace("{年}", Today(st).Year.ToString());
        }

        private static bool WhenMet(OptionWhen w, GameState st)
        {
            if (w == null) return true;
            if (!string.IsNullOrEmpty(w.grade) && !st.grade.StartsWith(w.grade)) return false;
            if (!string.IsNullOrEmpty(w.route) && st.route != w.route) return false;
            if (!string.IsNullOrEmpty(w.flag) && !st.GetFlag(w.flag)) return false;
            if (!string.IsNullOrEmpty(w.notFlag) && st.GetFlag(w.notFlag)) return false;
            if (w.minGradeYears > 0 && Career.GradeYears(st) < w.minGradeYears) return false;
            if (w.minOutstanding > 0 && st.outstandingYears < w.minOutstanding) return false;
            if (w.minBaseExpMonths > 0 && st.baseExpMonths < w.minBaseExpMonths) return false;
            return true;
        }

        private class OptView
        {
            public EventOption opt;
            public string lockReason;   // null = 可点
        }

        /// <summary>叙事性门槛不满足 → 锁定可见并给原因（结构性门槛仍隐藏，见 WhenMet）。</summary>
        private static string LockReason(EventOption o, GameState st)
        {
            var w = o.when;
            if (w == null) return null;
            if (!string.IsNullOrEmpty(w.mark) && !st.HasMark(w.mark)) return "需要一次契机";
            if (!string.IsNullOrEmpty(w.notMark) && st.HasMark(w.notMark)) return "已错过";
            if (!string.IsNullOrEmpty(w.relNpc))
            {
                var r = st.relations.Find(x => x.id == w.relNpc);
                int f = r != null ? r.familiar : 0;
                int t = r != null ? r.trust : 0;
                if (f < w.minFamiliar) return "需要与" + Npcs.Name(w.relNpc) + "更熟（" + f + "/" + w.minFamiliar + "）";
                if (t < w.minTrust) return "需要" + Npcs.Name(w.relNpc) + "的信任（" + t + "/" + w.minTrust + "）";
            }
            return null;
        }

        private static List<OptView> VisibleOptions(GameEvent ev, GameState st)
        {
            var list = new List<OptView>();
            foreach (var o in ev.options)
            {
                if (!WhenMet(o.when, st)) continue;
                list.Add(new OptView { opt = o, lockReason = LockReason(o, st) });
            }
            if (list.Count == 0) list.Add(new OptView { opt = ev.options[0] });
            return list;
        }

        private static GameEvent TakeNext(GameState st)
        {
            if (st.runtimeEvent != null) return st.runtimeEvent;
            if (st.queue.Count == 0 && string.IsNullOrEmpty(st.currentEvent)) return null;
            if (string.IsNullOrEmpty(st.currentEvent))
            {
                string id = st.queue[0];
                st.queue.RemoveAt(0);
                st.currentEvent = id;
            }
            var def = ById.ContainsKey(st.currentEvent) ? ById[st.currentEvent] : null;
            if (def == null) { st.currentEvent = null; return null; }
            if (def.dynamic)
            {
                st.currentEvent = null;
                st.runtimeEvent = BuildDynamic(def.id, st);
                return st.runtimeEvent;
            }
            return def;
        }

        private static Scene EventScene(GameState st)
        {
            var ev = TakeNext(st);
            if (ev != null)
            {
                var paras = new List<string>();
                foreach (var p in ev.paras) paras.Add(Fill(p, st));
                var vis = VisibleOptions(ev, st);
                var opts = new List<string>();
                var locks = new List<string>();
                bool anyLocked = false;
                foreach (var v in vis)
                {
                    opts.Add(v.lockReason == null ? v.opt.label : v.opt.label + " 🔒");
                    locks.Add(v.lockReason ?? "");
                    if (v.lockReason != null) anyLocked = true;
                }
                var scene = new Scene { kind = "event", title = Fill(ev.title, st), paras = paras, options = opts };
                if (anyLocked) scene.optionLocks = locks;
                scene.docNo = st.phase == Phase.Day ? DocNoFor(ev.id, st) : null;   // 入职后才按公文渲染
                return scene;
            }
            // 队列为空：日常场景（伪事件）
            st.runtimeEvent = BuildGenericDay(st);
            var g = st.runtimeEvent;
            var gparas = new List<string>();
            foreach (var p in g.paras) gparas.Add(Fill(p, st));
            return new Scene { kind = "day", title = g.title, paras = gparas, options = new List<string> { g.options[0].label } };
        }

        private static GameEvent BuildGenericDay(GameState st)
        {
            var d = Today(st);
            string holiday = GameClock.HolidayName(d);
            if (holiday != null && st.queue.Count == 0 && st.currentEvent == null)
            {
                return new GameEvent
                {
                    id = "_generic_holiday", type = "society", title = holiday,
                    paras = new List<string>
                    {
                        $"{holiday}。按帝国假期制度，今日不上班。长安的大街小巷比工作日更热闹，写字楼比你更安静。",
                    },
                    options = new List<EventOption>
                    {
                        new EventOption{ label="享受假日", effects=new Effects{ energy=5, morale=1 }, result="假日的一天，从从容容。" },
                    },
                };
            }

            // 有一定概率生成一件岗位任务（Phase 3：任务密度提升）
            if (GameClock.IsWorkday(d) && Rng.NextDouble() < 0.45 && st.player.energy > 30)
                return TaskGenerator.Generate(st, d);

            string focus = st.week.focus ?? "work";
            if (!ContentRegistry.GenericDayPools.ContainsKey(focus)) focus = "work";
            var pool = ContentRegistry.GenericDayPools[focus];
            string text = pool[Rng.Next(pool.Length)];

            Effects fx;
            string title;
            if (focus == "work") { fx = new Effects { exec = 1 }; title = "日常 · 埋头工作"; }
            else if (focus == "study") { fx = new Effects { professional = 1 }; title = "日常 · 学习充电"; }
            else if (focus == "social") { fx = new Effects { comm = 1 }; title = "日常 · 走动人际"; }
            else { fx = new Effects { morale = 1 }; title = "日常 · 休整调整"; }

            return new GameEvent
            {
                id = "_generic_day", type = "work", title = title,
                paras = new List<string> { text },
                options = new List<EventOption> { new EventOption { label = "继续", effects = fx, result = "" } },
            };
        }

        private static Scene WeekPlanScene(GameState st)
        {
            var d = Today(st);
            var paras = new List<string>
            {
                $"第 {st.week.index} 周。把这一周的精力分给五件事——你的分配，决定这一周长什么样子。",
                "（岗位投入越高任务评级越稳；学习长专业、人际长关系、家庭养士气、休整回精力降压力。总量 100，分不完的会白白流走。）",
            };
            return new Scene
            {
                kind = "week_plan",
                title = $"{GameClock.FmtFull(d)} · 周计划",
                paras = paras,
                options = new List<string> { "开始这一周" },
                planValues = new[] { st.plan.work, st.plan.study, st.plan.social, st.plan.family, st.plan.rest },
            };
        }

        /// <summary>文号：由事件 id 稳定派生（同一文件常年一个号，像真的归过档）。</summary>
        private static string DocNoFor(string id, GameState st)
        {
            int n = (id.GetHashCode() & 0x7fffffff) % 180 + 7;
            return "长发改〔" + Today(st).Year + "〕第 " + n + " 号";
        }

        private static Scene WeekEndScene(GameState st)
        {
            var d = Today(st);
            var w = st.weekEndData;
            string place = string.IsNullOrEmpty(st.seconded) ? "综合科" : st.seconded;
            var paras = new List<string>
            {
                $"周五下午，{place}例会。本周的活捋一遍——谁的活、到什么程度、下周怎么办。",
                $"本周你经手 {w.tasks.Count} 项任务，评级分布：{w.avgGrade}。",
                $"楼里的事：{w.peerLine}",
            };
            return new Scene
            {
                kind = "week_end",
                title = $"{GameClock.Fmt(d)} · 周五 · 例会点评",
                paras = paras,
                options = new List<string> { "认真记下，下周改进", "顺便找科长单独聊两句" },
            };
        }

        private static Scene WeekendScene(GameState st)
        {
            var paras = new List<string>
            {
                "周末两天。机关的周末属于自己——怎么过，也是一种选择。",
                "（休整回精力降压力；加班攒评价耗精力；其余各有各的长进。）",
            };
            // 上下文化：把这一周真实发生的事带进周末的语境
            var recent = new List<string>();
            for (int i = st.log.Count - 1; i >= 0 && recent.Count < 2; i--)
                if (st.log[i].kind != "系统" && !string.IsNullOrEmpty(st.log[i].text))
                    recent.Add(st.log[i].text);
            if (recent.Count > 0) paras.Insert(1, "这一周：" + string.Join("；", recent.ToArray()) + "。");
            return new Scene
            {
                kind = "weekend",
                title = "周末",
                paras = paras,
                options = new List<string> { "彻底休整", "自习充电", "约同学聚聚", "回单位加点班" },
            };
        }

        private static Scene MonthEndScene(GameState st)
        {
            var m = st.monthEndData;
            var paras = new List<string>
            {
                $"—— {m.monthLabel} · 月度结算 ——",
                $"任务档案：本月经手 {m.tasksTotal} 项任务，其中 S/A 级 {m.best} 项，C/D 级 {m.worst} 项；程序合规标记 {m.integrityCount} 条。",
                $"收支：工资＋基本补贴入账 {st.player.monthlyIn} 元，房租与生活开支 {st.player.monthlyOut} 元，本月结余 {m.netIncome} 元（现积蓄 {st.player.savings + m.netIncome} 元）。",
            };
            if (st.player.probationMonths < 12)
                paras.Add($"试用期进度：{st.player.probationMonths} / 12 个月。");
            if (!string.IsNullOrEmpty(m.monthDigest)) paras.Add(m.monthDigest);
            if (!string.IsNullOrEmpty(m.flavor)) paras.Add(m.flavor);
            return new Scene { kind = "month_end", title = "月度结算", paras = paras, options = new List<string> { "翻页，进入下个月" } };
        }

        // ---------------- 选择与推进 ----------------

        public static void Choose(GameState st, int idx)
        {
            // ① 结果等待“继续”
            if (st.hasPending)
            {
                st.hasPending = false;
                st.pendingTitle = "";
                st.pendingParas.Clear();
                AfterPending(st);
                return;
            }

            switch (st.phase)
            {
                case Phase.WeekPlan: ChooseWeekPlan(st, idx); return;
                case Phase.WeekEnd: ChooseWeekEnd(st, idx); return;
                case Phase.Weekend: ChooseWeekend(st, idx); return;
                case Phase.MonthEnd: ChooseMonthEnd(st, idx); return;
                case Phase.Ending: return; // 由 UI 层处理（新游戏/退出）
            }

            // ② 事件（脚本事件 / 动态事件 / 日常伪事件）
            GameEvent ev = TakeNext(st);
            if (ev == null) { EndOfDay(st); return; }

            var vis = VisibleOptions(ev, st);
            if (idx < 0 || idx >= vis.Count) idx = 0;
            var view = vis[idx];
            if (view.lockReason != null) return;   // 锁定选项不可点（UI 应置灰；兜底不吞选择）
            var opt = view.opt;
            // 克隆效果表：注册事件的 Effects 是共享实例，直接用会让 rel/morale 跨次执行累积
            var eff = CloneEffects(opt.effects);

            string grade = null;
            if (opt.check != null)
            {
                grade = TaskCheck(st, opt.check.main, opt.check.bonus);
                if (eff.task != null)
                {
                    eff.task.grade = grade;
                    eff.rel.Add(new RelDelta { id = "zhou", evalv = GradeEval[GradeIndex(grade)] });
                    eff.morale += GradeMorale[GradeIndex(grade)];
                }
            }
            else if (eff.task != null)
            {
                eff.task.grade = "B";
            }

            string result = Fill(opt.result ?? "", st);
            if (grade != null) result = result.Replace("{grade}", $"评级 {grade}");
            else result = result.Replace("{grade}", "");

            ApplyEffects(st, eff, grade);

            st.runtimeEvent = null;
            st.currentEvent = null;
            if (!string.IsNullOrEmpty(result))
            {
                st.hasPending = true;
                st.pendingTitle = Fill(ev.title, st) + " · 结果";
                st.pendingParas = new List<string> { result };
            }
            else
            {
                AfterPending(st);
            }
        }

        private static void AfterPending(GameState st)
        {
            switch (st.phase)
            {
                case Phase.WeekEnd:
                    if (GameClock.IsLastDayOfMonth(Today(st))) SettleMonth(st);
                    else BeginWeekend(st);
                    return;
                case Phase.Weekend:
                    GotoMonday(st); return;
                case Phase.MonthEnd:
                    NextMonthDay(st); return;
                case Phase.Ending:
                    return; // 停在结局
                default:
                    DayAdvance(st); return;
            }
        }

        private static void DayAdvance(GameState st)
        {
            if (st.queue.Count > 0) return; // 下一个事件将由 EventScene 取出
            EndOfDay(st);
        }

        // ---------------- 日/周/月节奏 ----------------

        private static void EndOfDay(GameState st)
        {
            var d = Today(st);
            if (st.PopFlag("__goto_work")) { BeginWork(st); return; }
            if (st.phase == Phase.Prologue) { BeginWork(st); return; } // 安全兜底
            if (d.DayOfWeek == DayOfWeek.Friday && !st.week.reviewed) { BeginWeekEnd(st); return; }
            if (GameClock.IsLastDayOfMonth(d)) { SettleMonth(st); return; }
            NextDay(st);
        }

        private static void NextDay(GameState st)
        {
            var d = GameClock.AddDays(Today(st), 1);
            st.date = GameClock.Iso(d);
            st.currentEvent = null;
            st.runtimeEvent = null;
            Drift(st, d);
            CollectDue(st, d);
            if (d.DayOfWeek == DayOfWeek.Monday) { ResetWeek(st, d); st.phase = Phase.WeekPlan; }
            else st.phase = Phase.Day;
        }

        private static void BeginWork(GameState st)
        {
            var d = GameClock.ProbationStart;
            st.date = GameClock.Iso(d);
            st.phase = Phase.WeekPlan;
            st.currentEvent = null;
            st.runtimeEvent = null;
            CollectDue(st, d);        // 先建队列（NpcTick 的入队会被日清空冲掉，顺序不可反）
            ResetWeek(st, d);
            st.month.key = GameClock.MonthKey(d);
            st.AddLog("系统", "入职：长安市发展和改革局综合科，科员（吏三），试用期一年");
        }

        private static void ResetWeek(GameState st, DateTime d)
        {
            st.week.index = GameClock.WeekIndex(d);
            st.week.focus = null;
            st.week.tasks.Clear();
            st.week.reviewed = false;
            st.plan.hasPlan = false;
            st.rival.progress = Math.Min(100, st.rival.progress + 1 + Rng.Next(2));   // 竞争者也在往前走
            NpcTick.WeeklyTick(st);   // 周一：人物随时间变动（淡忘/偶发主动找你）
        }

        private static void Drift(GameState st, DateTime d)
        {
            var p = st.player;
            if (GameClock.IsWorkday(d))
            {
                int drain = Math.Max(2, 8 - st.plan.rest / 12);      // 休整投入越高，日常耗损越低
                int stressUp = st.plan.rest >= 30 ? 1 : 2;
                p.energy = Clamp(p.energy - drain, 0, 100);
                p.stress = Clamp(p.stress + stressUp, 0, 100);
            }
            else
            {
                p.energy = Clamp(p.energy + 12, 0, 100);
                p.stress = Clamp(p.stress - 6, 0, 100);
            }
        }

        // ---------------- 周计划（Phase 4：精力槽位分配） ----------------

        /// <summary>UI 编辑器调整槽位（±delta，总量不超过 100）。</summary>
        public static void AdjustPlan(GameState st, int slot, int delta)
        {
            if (slot < 0 || slot > 4) return;
            int[] v = { st.plan.work, st.plan.study, st.plan.social, st.plan.family, st.plan.rest };
            int total = v[0] + v[1] + v[2] + v[3] + v[4];
            int nv = Clamp(v[slot] + delta, 0, 100);
            if (delta > 0) nv = Math.Min(nv, v[slot] + Math.Max(0, 100 - total));
            v[slot] = nv;
            st.plan.work = v[0]; st.plan.study = v[1]; st.plan.social = v[2];
            st.plan.family = v[3]; st.plan.rest = v[4];
        }

        /// <summary>快进用自动计划：疲惫时强制休整倾斜，否则沿用玩家上一周的计划。</summary>
        public static void AutoPlan(GameState st)
        {
            if (st.player.energy < 45 || st.player.stress > 70)
            {
                st.plan.work = 30; st.plan.study = 5; st.plan.social = 5; st.plan.family = 10; st.plan.rest = 50;
            }
            ChooseWeekPlan(st, 0);
        }

        /// <summary>计划主导槽 → 日常文案池口径（family 并入 rest 池）。</summary>
        private static string DominantFocus(GameState st)
        {
            int[] v = { st.plan.work, st.plan.study, st.plan.social, st.plan.rest };
            string[] k = { "work", "study", "social", "rest" };
            int best = 0;
            for (int i = 1; i < 4; i++) if (v[i] > v[best]) best = i;
            return v[best] <= 0 ? "work" : k[best];
        }

        private static void ChooseWeekPlan(GameState st, int idx)
        {
            st.week.focus = DominantFocus(st);
            st.plan.hasPlan = true;
            st.phase = Phase.Day;
            if (st.queue.Count == 0) EndOfDay(st); // 空周兜底（正常不会发生）
        }

        // ---------------- 周点评 ----------------

        private static void BeginWeekEnd(GameState st)
        {
            st.week.reviewed = true;
            st.phase = Phase.WeekEnd;

            var data = new WeekEndData();
            data.tasks.AddRange(st.week.tasks);
            int sum = 0;
            foreach (var t in data.tasks) sum += GradeIndex(t.grade) + 1; // S=5..D=1
            data.avgGrade = data.tasks.Count == 0 ? "—" : GradeNames[Math.Max(0, Math.Min(4, sum / data.tasks.Count - 1))];

            int zhou = 0;
            foreach (var t in data.tasks) zhou += GradeEval[GradeIndex(t.grade)];
            data.zhouDelta = Math.Max(-8, Math.Min(8, zhou));

            PlanGrowth(st, data.growth, ref data.stressDelta);
            data.moraleDelta = st.plan.family / 10;
            data.peerLine = ContentRegistry.PeerLines[Rng.Next(ContentRegistry.PeerLines.Length)];
            st.weekEndData = data;
        }

        /// <summary>周计划 → 成长与压力结算（周五例会与快进静默周共用一套口径）。</summary>
        private static void PlanGrowth(GameState st, Attrs growth, ref int stressDelta)
        {
            var p = st.plan;
            growth.exec += p.work / 20;
            growth.admin += p.work / 40;
            growth.professional += p.study / 10;
            growth.comm += p.social / 15;
            growth.political += p.social / 30;
            stressDelta += p.work / 15 + p.study / 20 + p.social / 30 - p.rest / 4;

            // 人际投入：随机同事/熟人熟悉度上升（投得越多见的人越多）
            if (p.social > 0)
            {
                int ticks = Math.Min(2, 1 + p.social / 35);
                for (int i = 0; i < ticks; i++)
                {
                    var id = Npcs.Order[Rng.Next(Npcs.Order.Length)];
                    Npcs.Mod(st, new RelDelta { id = id, familiar = 1 + p.social / 40 });
                }
            }
            if (p.family > 0)
                st.AddLog("人物", string.IsNullOrEmpty(st.partner)
                    ? "你给家里打了几个电话，母亲絮叨了半天饭菜"
                    : "你把两个晚上留给了" + st.partner + "和家里");
        }

        /// <summary>把本周的精力分配讲成一句话（周五点评用）。</summary>
        private static string PlanNarrative(GameState st)
        {
            var p = st.plan;
            var segs = new List<string>();
            if (p.work >= 30) segs.Add("岗位上压了重活");
            else if (p.work > 0) segs.Add("岗位上的活按部就班");
            if (p.study >= 20) segs.Add("给自己留了整块的学习时间");
            else if (p.study > 0) segs.Add("零碎时间读了不少东西");
            if (p.social >= 20) segs.Add("走廊里没少走动");
            else if (p.social > 0) segs.Add("该打的招呼都打了");
            if (p.family > 0) segs.Add(string.IsNullOrEmpty(st.partner) ? "给家里打了几个电话" : "留了时间给" + st.partner + "和家里");
            if (p.rest >= 30) segs.Add("也认真让自己歇了歇");
            else if (p.rest > 0) segs.Add("喘了几口气");
            if (segs.Count == 0) segs.Add("精力没有去向，日子就没有形状");
            return "这一周：" + string.Join("，", segs.ToArray()) + "。";
        }

        private static void ChooseWeekEnd(GameState st, int idx)
        {
            var w = st.weekEndData;
            var eff = new Effects
            {
                professional = w.growth.professional, admin = w.growth.admin,
                exec = w.growth.exec, comm = w.growth.comm, political = w.growth.political,
                stress = w.stressDelta, morale = w.moraleDelta,
            };
            eff.rel.Add(new RelDelta { id = "zhou", evalv = w.zhouDelta });

            var paras = new List<string>();
            if (w.tasks.Count > 0)
                paras.Add($"周衡之本周在你交的活上画了很多红——也留了很多话。你的综合表现：{w.avgGrade}。");
            else
                paras.Add("本周你没有独立经手的任务。周衡之瞥了你一眼：“下周给你压点担子。”");

            paras.Add(PlanNarrative(st));
            paras.Add(w.peerLine);

            if (idx == 1)
            {
                eff.rel.Add(new RelDelta { id = "zhou", trust = 2, familiar = 2 });
                eff.energy -= 3;
                paras.Add("你留下来单独聊了几句。周衡之讲了讲他看好的方向，也点了点你的短板——领导的注意力，本身就是一种资源。");
            }

            ApplyEffects(st, eff, null);
            st.hasPending = true;
            st.pendingTitle = "周五 · 点评小结";
            st.pendingParas = paras;
        }

        // ---------------- 周末 ----------------

        private static void BeginWeekend(GameState st)
        {
            st.phase = Phase.Weekend;
            var p = st.player;
            p.energy = Clamp(p.energy + 24, 0, 100);
            p.stress = Clamp(p.stress - 12, 0, 100);
        }

        private static void ChooseWeekend(GameState st, int idx)
        {
            Effects fx;
            string text;
            switch (idx)
            {
                case 1: fx = new Effects { professional = 1, energy = -4 }; text = "你把一个上午给了规划文本，一个下午给了《监察法》条文。周末的教室只有你一个人——还有一种上进的孤独感。"; break;
                case 2: fx = new Effects { comm = 1, morale = 3, energy = -3, rel = new List<RelDelta> { new RelDelta { id = "su", familiar = 3 }, new RelDelta { id = "xu", familiar = 3 } } }; text = "同批新人聚了顿火锅。许飞聊各自的科室，苏晴聊通勤，何斌聊八卦——你发现这批人里，你最信任的可能是最安静的苏晴。"; break;
                case 3: fx = new Effects { exec = 1, energy = -14, stress = 2, reputation = 1, rel = new List<RelDelta> { new RelDelta { id = "zhou", trust = 1, evalv = 1, memo = "周末还能看见他" } } }; text = "你在空荡的办公室里加了一天班。周一大家看到系统里的文档更新时间，什么都没说——但什么都说了。"; break;
                default: fx = new Effects { energy = 6, stress = -6, morale = 2 }; text = "你把手机调成勿扰，睡了懒觉，去了趟渭河生态带。风从水面上来，把一周的文件气都吹散了。"; break;
            }
            ApplyEffects(st, fx, null);
            st.hasPending = true;
            st.pendingTitle = "周末 · 小结";
            st.pendingParas = new List<string> { text };
        }

        private static void GotoMonday(GameState st)
        {
            var d = GameClock.NextWorkday(Today(st)); // 周五之后 → 下周一
            st.date = GameClock.Iso(d);
            st.currentEvent = null;
            st.runtimeEvent = null;
            Drift(st, d);
            CollectDue(st, d);        // 先建队列，后周 tick（同 BeginWork）
            ResetWeek(st, d);
            st.phase = Phase.WeekPlan;
        }

        // ---------------- 月度结算 ----------------

        private static void SettleMonth(GameState st)
        {
            st.phase = Phase.MonthEnd;
            var d = Today(st);
            var data = new MonthEndData
            {
                monthLabel = $"{d.Year}年{d.Month}月",
                tasksTotal = st.month.tasks.Count,
                netIncome = st.player.monthlyIn - st.player.monthlyOut,
                probation = st.player.probationMonths + 1,
                integrityCount = st.integrity.Count,
                flavor = ContentRegistry.MonthFlavor(st.month.key) ?? "",
            };
            foreach (var t in st.month.tasks)
            {
                int g = GradeIndex(t.grade);
                if (g <= 1) data.best++;       // S/A
                else if (g >= 3) data.worst++; // C/D
            }
            // 本月大事记（上下文化）：这个月真实发生的非系统事件
            string mk = GameClock.MonthKey(d);
            var digest = new List<string>();
            for (int i = 0; i < st.log.Count && digest.Count < 3; i++)
                if (st.log[i].date.StartsWith(mk) && st.log[i].kind != "系统" && !string.IsNullOrEmpty(st.log[i].text))
                    digest.Add(st.log[i].text);
            if (digest.Count > 0) data.monthDigest = "本月大事记：" + string.Join("；", digest.ToArray()) + "。";
            st.monthEndData = data;
        }

        private static void ChooseMonthEnd(GameState st, int idx)
        {
            var m = st.monthEndData;
            st.player.savings += m.netIncome;
            st.player.probationMonths = m.probation;
            var lastDay = Today(st);
            st.AddLog("系统", $"{m.monthLabel}月度结算：结余 {m.netIncome} 元；试用期进度 {m.probation}/12；本月任务 {m.tasksTotal} 项。");
            st.month.key = GameClock.MonthKey(GameClock.AddDays(lastDay, 1));
            st.month.tasks.Clear();

            st.hasPending = true;
            st.pendingTitle = $"{m.monthLabel} · 结账";
            st.pendingParas = new List<string>
            {
                "你把本月的台账归档，给工牌套换了新膜。",
                m.probation >= 12
                    ? "试用期已经届满——转正考核在向你招手。"
                    : $"距离试用期考核还有 {12 - m.probation} 个月。路还长，但每一格都算数。",
            };
        }

        private static void NextMonthDay(GameState st)
        {
            var d = GameClock.AddDays(Today(st), 1);
            st.date = GameClock.Iso(d);
            st.currentEvent = null;
            st.runtimeEvent = null;
            Drift(st, d);
            CollectDue(st, d);
            if (d.DayOfWeek == DayOfWeek.Monday) { ResetWeek(st, d); st.phase = Phase.WeekPlan; }
            else st.phase = Phase.Day;
        }

        // ---------------- 时间快进（十年长线必需） ----------------

        /// <summary>
        /// 自动度过没有安排事件的日常：周一自动定重心、周末自动休整、周五自动点评、
        /// 月末自动结算；工作日按概率自动完成岗位任务。停在下一个脚本事件或年度考核处。
        /// </summary>
        public static void FastForward(GameState st)
        {
            if (st.phase != Phase.Day || st.hasPending) return;
            // 护栏：屏上有待抉择事件时禁止快进（否则事件被标记已触发却未结算，永久丢失）
            if (!string.IsNullOrEmpty(st.currentEvent)) return;
            if (st.runtimeEvent != null && st.runtimeEvent.id != "_generic_day" && st.runtimeEvent.id != "_gen_task") return;
            int days = 0, tasks = 0, months = 0;
            int guard = 0;
            while (guard++ < 8000)
            {
                if (st.phase == Phase.WeekPlan)
                    AutoPlan(st);          // 自动按上周计划跑；疲惫时强制休整倾斜
                var d = Today(st);
                if (st.queue.Count > 0) break;                       // 有安排了，交还玩家
                if (d.DayOfWeek == DayOfWeek.Friday && !st.week.reviewed) { SilentWeek(st); continue; }
                if (GameClock.IsLastDayOfMonth(d)) { SilentMonth(st, ref months); continue; }
                NextDay(st);
                days++;
                if (st.phase == Phase.WeekPlan) AutoPlan(st);
                if (st.queue.Count > 0) break;
                var nd = Today(st);
                // 每周首件任务必生成，其余工作日 45% 概率——保证长线档案密度稳定
                if (GameClock.IsWorkday(nd) && st.player.energy > 35 &&
                    (st.week.tasks.Count == 0 || Rng.NextDouble() < 0.45))
                {
                    st.runtimeEvent = TaskGenerator.Generate(st, nd);
                    int before = st.tasks.Count;
                    Choose(st, 0);                                    // 自动“认真完成”
                    st.hasPending = false;                            // 吞掉结果，不推进日结
                    if (st.tasks.Count > before) tasks++;
                }
            }
            var paras = new List<string>();
            if (days > 0 || tasks > 0 || months > 0)
            {
                if (days > 0) paras.Add($"日子一页页翻过去：{days} 个日常，开会、写材料、对数据、下楼调研——机关的年轮就是这些细碎的日子刻出来的。");
                if (tasks > 0) paras.Add($"你经手了 {tasks} 项日常任务，档案上添了 {tasks} 行记录。");
                if (months > 0) paras.Add($"{months} 张月度结账单。工资准时到账，物价缓慢上涨，城市的新闻换了好几茬。");
                paras.Add("——有一件事，需要你亲自到场。");
                st.hasPending = true;
                st.pendingTitle = "时光荏苒";
                st.pendingParas = paras;
            }
        }

        private static void SilentWeek(GameState st)
        {
            st.week.reviewed = true;
            var growth = new Attrs();
            int stress = 0;
            PlanGrowth(st, growth, ref stress);
            var a = st.player.attrs;
            a.professional = Clamp(a.professional + growth.professional, 0, 100);
            a.admin = Clamp(a.admin + growth.admin, 0, 100);
            a.exec = Clamp(a.exec + growth.exec, 0, 100);
            a.comm = Clamp(a.comm + growth.comm, 0, 100);
            a.political = Clamp(a.political + growth.political, 0, 100);
            st.player.stress = Clamp(st.player.stress + stress, 0, 100);
            st.player.morale = Clamp(st.player.morale + st.plan.family / 10, 0, 100);
        }

        private static void SilentMonth(GameState st, ref int months)
        {
            var d = Today(st);
            st.player.savings += st.player.monthlyIn - st.player.monthlyOut;
            st.player.probationMonths++;
            months++;
            st.month.key = GameClock.MonthKey(GameClock.AddDays(d, 1));
            st.month.tasks.Clear();
            var nd = GameClock.AddDays(d, 1);
            st.date = GameClock.Iso(nd);
            st.currentEvent = null;
            st.runtimeEvent = null;
            Drift(st, nd);
            CollectDue(st, nd);
            if (nd.DayOfWeek == DayOfWeek.Monday) { ResetWeek(st, nd); st.phase = Phase.WeekPlan; }
            else st.phase = Phase.Day;
        }

        // ---------------- 动态事件（状态相关的系统事件） ----------------

        private static GameEvent BuildDynamic(string id, GameState st)
        {
            switch (id)
            {
                case "sys_personnel": return BuildPersonnel(st);
                case "sys_annual_eval": return BuildAnnualEval(st);
                case "sys_ending": return BuildEnding(st);
                case "npc_initiative": return BuildNpcInitiative(st);
            }
            return null;
        }

        /// <summary>NPC 主动来找你（NpcTick 每周 10% 概率）：借材料 / 打听口径 / 约饭。</summary>
        private static GameEvent BuildNpcInitiative(GameState st)
        {
            var pool = new List<string>();
            foreach (var nid in Npcs.Order)
            {
                var r = st.relations.Find(x => x.id == nid);
                if (r != null && r.familiar >= 8) pool.Add(nid);
            }
            if (pool.Count == 0) pool.Add("zhou");
            string nid2 = pool[Rng.Next(pool.Count)];
            string nm = Npcs.Name(nid2);
            string kind = ((NpcKind)(Rng.Next(3))).ToString();

            if (kind == NpcKind.Ask.ToString())
                return new GameEvent
                {
                    id = "npc_initiative", type = "person", title = $"{nm}来找你对一份材料的口径",
                    paras = new List<string>
                    {
                        $"{nm}拿着一份材料走到你桌前：“有个数据口径想跟你核一核——你上次那套对法挺清楚的。”",
                    },
                    options = new List<EventOption>
                    {
                        new EventOption{ label="放下手头的活，认真帮他对清楚",
                            effects=new Effects{ energy=-3, comm=1, rel=new List<RelDelta>{ new RelDelta{ id=nid2, familiar=2, evalv=1, memo="主动帮我对过口径" } } },
                            result="你们对了半个多小时。临走时他说了句“靠谱”——这种口碑，是攒出来的。" },
                        new EventOption{ label="实在抽不开身，给他指了个人",
                            effects=new Effects{ rel=new List<RelDelta>{ new RelDelta{ id=nid2, familiar=-1 } } },
                            result="他把材料抱去了别处。没什么对错——只是人情账上，这一页空着。" },
                    },
                };
            if (kind == NpcKind.Gossip.ToString())
                return new GameEvent
                {
                    id = "npc_initiative", type = "person", title = $"{nm}在走廊里把你叫住",
                    paras = new List<string>
                    {
                        $"{nm}压低声音：“给你递个信儿——最近上面在关注一件事，风向上你心里有数就行。”",
                    },
                    options = new List<EventOption>
                    {
                        new EventOption{ label="多问两句，把这个信儿接住",
                            effects=new Effects{ political=1, rel=new List<RelDelta>{ new RelDelta{ id=nid2, trust=1 } } },
                            result="你们在楼梯间又聊了五分钟。消息在机关里是硬通货——而你开始学会分辨哪些值钱、哪些有毒。" },
                        new EventOption{ label="笑笑带过，不往深里聊",
                            effects=new Effects{ },
                            result="你把话头轻轻挡了回去。不该知道的，知道了也是负担。" },
                    },
                };
            return new GameEvent
            {
                id = "npc_initiative", type = "person", title = $"{nm}约你中午吃饭",
                paras = new List<string>
                {
                    $"{nm}探过头来：“中午食堂还是楼下那家？一个人吃没意思。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="去。饭桌上的话，比会议室里真",
                        effects=new Effects{ morale=2, moneyDelta=-30, rel=new List<RelDelta>{ new RelDelta{ id=nid2, familiar=3, trust=1, memo="一起吃过几顿饭" } } },
                        result="一顿饭吃了四十分钟，工作只聊了一半——另一半是房子、物价和各自家里的事。关系就是这样一顿一顿吃出来的。" },
                    new EventOption{ label="推说手头有活，改天吧",
                        effects=new Effects{ energy=2 },
                        result="你把饭推了。“改天”说了三次之后，就不会再有人来约了。" },
                },
            };
        }

        private enum NpcKind { Ask, Gossip, Meal }


        /// <summary>每年9月的人事窗口：按总设定晋升年限与条件给出任命选项。</summary>
        private static GameEvent BuildPersonnel(GameState st)
        {
            int years = Career.GradeYears(st);
            var paras = new List<string>
            {
                "九月，全局人事窗口开启。任雪梅抱着档案袋挨个科室走——每年这个时候，走廊里的目光都比平时忙。",
                $"你的现任职级：{st.grade}（任职满 {years} 年）。" +
                (string.IsNullOrEmpty(st.seconded) ? "" : $"当前编制状态：{st.seconded}。"),
                $"同批的风声：{Npcs.Name(st.rival.id)}今年的势头不小（晋升竞争力 {st.rival.progress}/100）——人事窗口前，每个人都在跟时间赛跑。",
            };
            var opts = new List<EventOption>();
            if (Career.CanPromoteLi2(st))
            {
                paras.Add("周衡之在谈话表上签了字：“两年科员，考核都过得去。组织上想给你压担子了。”");
                opts.Add(new EventOption
                {
                    label = "接受晋升：吏二·副科",
                    effects = new Effects { gradeTo = "吏二·副科", morale = 6, rel = new List<RelDelta> { new RelDelta { id = "zhou", trust = 2, evalv = 2 } }, reputation = 1, logKind = "系统", logText = "晋升吏二·副科" },
                    result = "任前谈话、公示、任职文件——你成了副科级干部。工资条上多了一栏，责任栏里多了一行。"
                });
            }
            if (Career.CanPromoteLi1(st))
            {
                paras.Add("副局长马建国在你的考核表上画了个圈：“吏二满三年，又有优秀等次。局党组的意思——正科。”");
                opts.Add(new EventOption
                {
                    label = "接受晋升：吏一·正科",
                    effects = new Effects { gradeTo = "吏一·正科", morale = 6, reputation = 2, rel = new List<RelDelta> { new RelDelta { id = "ma", evalv = 2, memo = "晋升正科——组织上的认可" } }, logKind = "系统", logText = "晋升吏一·正科" },
                    result = "任命文件下来的那天，你把办公室搬到了靠窗的位置——正科，机关里真正的中坚一层。"
                });
            }
            if (st.grade.StartsWith("吏一"))
            {
                string reason;
                bool ok = Career.ExamEligible(st, out reason);
                paras.Add(ok
                    ? "组织科提醒你：州级转官考试的报考窗口开了——吏轨到官轨的那道门，今年对你敞开着。"
                    : $"转官考试的条件：{reason}。路还差几步，但每一步都算数。");
                if (ok)
                {
                    opts.Add(new EventOption
                    {
                        label = "报名州级转官考试",
                        effects = new Effects
                        {
                            setFlags = new List<string> { "exam_ready" }, professional = 2,
                            echoes = new List<EchoSpec> { new EchoSpec { eventId = "ch_exam_ticket", afterDays = 60 } },
                            logKind = "系统", logText = "报名州级转官考试"
                        },
                        result = "报考表、单位推荐函、基层履历证明——你把十年攒下的纸一张张交了上去。"
                    });
                }
            }
            opts.Add(new EventOption
            {
                label = "今年不谈进步，先把活干好",
                effects = new Effects { morale = 1 },
                result = "你笑了笑，把话题引回工作。人事年年有，急不来——稳住自己的节奏，机会到了自然接得住。"
            });
            return new GameEvent
            {
                id = "_dyn_personnel", type = "person", title = $"人事窗口 · {Today(st).Year}年9月",
                paras = paras, options = opts,
            };
        }

        /// <summary>每年1月的年度考核（评优评先制，Q3-02）。开场白按年变奏。</summary>
        private static GameEvent BuildAnnualEval(GameState st)
        {
            int year = Today(st).Year - 1;
            string opener;
            switch (year % 4)
            {
                case 1: opener = "考核办的灯提前亮了。走廊里的人脚步都比平时轻——一年的分量，最后落到这几张表上。"; break;
                case 2: opener = "考核季到了。有人盼着它，有人怕着它，更多人假装没看见公告栏上的通知。"; break;
                case 3: opener = "年度考核。茶水间的议论比会议室多：“听说今年优秀名额和去年一样。”一样少，大家都懂。"; break;
                default: opener = "新年第一件大事是考核。档案室的钥匙在任雪梅手里转了一圈，你一年的日子被调成了一叠纸。"; break;
            }
            var paras = new List<string>
            {
                opener,
                $"考核办调取了你{year}年的全部档案：{st.yearTaskCount} 项任务、评级分布、程序合规记录。",
                "考核等第：优秀／称职／基本称职／不称职。“优秀”名额有限，同批人都在盯着——评优评先，评的是一年的分量。",
            };
            string outcome = Career.EvaluateYear(st, true, Rng);
            string plain = Career.EvaluateYear(st, false, Rng);
            var opts = new List<EventOption>
            {
                new EventOption
                {
                    label = "实事求是，申报称职",
                    effects = new Effects { evalGrade = plain, clearYearStats = true, morale = plain == "优秀" ? 0 : 1 },
                    result = $"你的{year}年度考核等第：{plain}。踏实，是机关里最耐穿的鞋。"
                },
                new EventOption
                {
                    label = "申报优秀，参与评优竞争",
                    effects = new Effects { evalGrade = outcome, clearYearStats = true, morale = outcome == "优秀" ? 8 : -2,
                        reputation = outcome == "优秀" ? 2 : 0,
                        rel = new List<RelDelta> { new RelDelta { id = "zhou", evalv = outcome == "优秀" ? 1 : 0 } },
                        commend = outcome == "优秀" ? new CommendRecord { text = $"{year}年度考核优秀" } : null },
                    result = outcome == "优秀"
                        ? $"评优名单公示，你的名字在列——{year}年度考核优秀。同批人里，今年是你。"
                        : $"这一年你拼了，但优秀的名额给了别人（{outcome}）。竞争就是这样：差半口气，就差一个台阶——明年再来。"
                },
            };
            if (st.yearIntegrity >= 2)
                paras.Add("考核办的同志翻到你档案里的程序合规标记，眉头皱了皱——今年这几条记录，会是减分项。");
            return new GameEvent
            {
                id = "_dyn_annual", type = "politics", title = $"年度考核 · {year}年度",
                paras = paras, options = opts,
            };
        }

        /// <summary>2036年8月的十年总结（结局）。</summary>
        private static GameEvent BuildEnding(GameState st)
        {
            return new GameEvent
            {
                id = "_dyn_ending", type = "system", title = "十年之约",
                paras = new List<string>
                {
                    "2026年9月，你抱着档案袋走进综合科；2036年8月，你把十年的日子写成了几页鉴定。",
                    "城墙还是那道城墙，渭河还是那条渭河。变化的是你：职级、路线、能力、身边的同事与家人——以及你对“程序”二字的理解。",
                    "档案合上之前，组织上请你留下十年的答案。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="写下这十年的答案", effects=new Effects{ ending=true }, result="" },
                },
            };
        }

        // ---------------- 效果结算与检定 ----------------

        /// <summary>效果表克隆：共享注册事件的效果不得被单次执行修改（rel 增量、morale 累加等）。</summary>
        private static Effects CloneEffects(Effects e)
        {
            if (e == null) return new Effects();
            return new Effects
            {
                professional = e.professional, admin = e.admin, exec = e.exec,
                comm = e.comm, political = e.political,
                energy = e.energy, stress = e.stress, morale = e.morale,
                reputation = e.reputation, polCapital = e.polCapital, moneyDelta = e.moneyDelta,
                rel = new List<RelDelta>(e.rel ?? new List<RelDelta>()),
                setFlags = e.setFlags != null ? new List<string>(e.setFlags) : new List<string>(),
                setMarks = e.setMarks != null ? new List<string>(e.setMarks) : new List<string>(),
                clearMarks = e.clearMarks != null ? new List<string>(e.clearMarks) : new List<string>(),
                echoes = e.echoes != null ? new List<EchoSpec>(e.echoes) : new List<EchoSpec>(),
                clockOps = e.clockOps != null ? new List<ClockOp>(e.clockOps) : new List<ClockOp>(),
                task = e.task, document = e.document, integrity = e.integrity, commend = e.commend,
                logKind = e.logKind, logText = e.logText, gotoWork = e.gotoWork,
                gradeTo = e.gradeTo, route = e.route, seconded = e.seconded, partner = e.partner,
                baseExpDelta = e.baseExpDelta, evalGrade = e.evalGrade, clearYearStats = e.clearYearStats,
                ambition = e.ambition, housing = e.housing,
                ending = e.ending, resigned = e.resigned, underInvestigation = e.underInvestigation,
                marry = e.marry, child = e.child,
            };
        }

        private static void ApplyEffects(GameState st, Effects e, string grade)
        {
            if (e == null) return;
            var p = st.player;
            p.attrs.professional = Clamp(p.attrs.professional + e.professional, 0, 100);
            p.attrs.admin = Clamp(p.attrs.admin + e.admin, 0, 100);
            p.attrs.exec = Clamp(p.attrs.exec + e.exec, 0, 100);
            p.attrs.comm = Clamp(p.attrs.comm + e.comm, 0, 100);
            p.attrs.political = Clamp(p.attrs.political + e.political, 0, 100);
            p.energy = Clamp(p.energy + e.energy, 0, 100);
            p.stress = Clamp(p.stress + e.stress, 0, 100);
            p.morale = Clamp(p.morale + e.morale, 0, 100);
            p.reputation = Math.Max(0, p.reputation + e.reputation);
            p.polCapital = Math.Max(0, p.polCapital + e.polCapital);
            if (e.moneyDelta != 0) p.savings += e.moneyDelta;

            if (e.rel != null)
                foreach (var r in e.rel) Npcs.Mod(st, r);

            if (e.setFlags != null)
                foreach (var f in e.setFlags) st.SetFlag(f, true);

            if (e.setMarks != null)
                foreach (var m in e.setMarks) st.Mark(m);
            if (e.clearMarks != null)
                foreach (var m in e.clearMarks) st.Unmark(m);
            if (e.echoes != null)
                foreach (var s in e.echoes) ScheduleEcho(st, s.eventId, s.afterDays);
            if (e.clockOps != null)
                foreach (var c in e.clockOps) Clocks.Apply(st, c);

            if (e.task != null)
            {
                e.task.date = st.date;
                if (string.IsNullOrEmpty(e.task.grade)) e.task.grade = grade ?? "B";
                st.tasks.Add(e.task);
                st.week.tasks.Add(e.task);
                st.month.tasks.Add(e.task);
                st.yearGradePoints += 4 - GradeIndex(e.task.grade); // S=4..D=0
                st.yearTaskCount++;
            }
            if (e.document != null) { e.document.date = st.date; st.documents.Add(e.document); }
            if (e.integrity != null)
            {
                e.integrity.date = st.date;
                st.integrity.Add(e.integrity);
                st.violationCount++;
                st.yearIntegrity++;
                if (st.violationCount == 2) st.SetFlag("violation_2", true);
                if (st.violationCount == 3) st.SetFlag("violation_3", true);
                if (st.violationCount >= 4) st.SetFlag("violation_severe", true);
            }
            if (e.commend != null) { e.commend.date = st.date; st.commendations.Add(e.commend); }

            if (!string.IsNullOrEmpty(e.logText)) st.AddLog(string.IsNullOrEmpty(e.logKind) ? "系统" : e.logKind, e.logText);

            if (!string.IsNullOrEmpty(e.gradeTo))
            {
                st.grade = e.gradeTo;
                st.gradeSince = st.date;
                p.rank = RankOf(e.gradeTo);
                if (e.gradeTo == "十品·副处") st.SetFlag("became_pin10", true);
            }
            if (!string.IsNullOrEmpty(e.route)) { st.route = e.route; st.AddLog("系统", $"确立职业路线：{Career.RouteName(e.route)}"); }
            if (e.seconded != null) { st.seconded = e.seconded; st.AddLog("系统", string.IsNullOrEmpty(e.seconded) ? "结束借调（挂职），回局履职" : $"编制状态变动：{e.seconded}"); }
            if (!string.IsNullOrEmpty(e.ambition))
            {
                st.ambition = e.ambition;
                st.Mark("ambition_" + e.ambition);
                st.AddLog("系统", "你在心里给这十年定了个调子：" + AmbitionName(e.ambition));
            }
            if (!string.IsNullOrEmpty(e.housing)) { st.housing = e.housing; st.AddLog("生活", "住房状况：" + e.housing); }
            if (!string.IsNullOrEmpty(e.partner))
            {
                st.partner = e.partner;
                st.SetFlag("life_partner_set", true);
                st.SetFlag("life_marryable", true);
                st.AddLog("人物", $"你与{e.partner}确立了关系");
            }
            if (e.baseExpDelta != 0) st.baseExpMonths = Math.Max(0, st.baseExpMonths + e.baseExpDelta);
            if (e.clearYearStats)
            {
                int year = Today(st).Year - 1;
                st.evals.Add(new YearEval { year = year, grade = string.IsNullOrEmpty(e.evalGrade) ? "称职" : e.evalGrade });
                if (e.evalGrade == "优秀") st.outstandingYears++;
                st.yearGradePoints = 0;
                st.yearTaskCount = 0;
                st.yearIntegrity = 0;
                st.AddLog("政治", $"{year}年度考核等第：{e.evalGrade}");
            }
            if (e.ending)
            {
                st.endingData = Career.ComputeEnding(st);
                st.phase = Phase.Ending;
            }
            if (e.resigned)
            {
                st.resigned = true;
                st.endingData = Career.ComputeEnding(st);
                st.phase = Phase.Ending;
            }
            if (e.underInvestigation)
            {
                st.underInvestigation = true;
                st.endingData = Career.ComputeEnding(st);
                st.phase = Phase.Ending;
            }
            if (e.marry) { st.married = true; st.SetFlag("life_childable", true); st.AddLog("人物", $"你与{st.partner}登记结婚"); }
            if (e.child) { st.hasChild = true; st.AddLog("人物", "家里添了新成员"); }
            if (e.gotoWork) st.SetFlag("__goto_work", true);
        }

        private static string RankOf(string grade)
        {
            switch (grade)
            {
                case "吏二·副科": return "吏二·副科";
                case "吏一·正科": return "吏一·正科";
                case "十品·副处": return "十品·副处（副区长）";
            }
            return "吏三·科员";
        }

        /// <summary>志向显示名（P4.4）。</summary>
        public static string AmbitionName(string a)
        {
            switch (a)
            {
                case "做事": return "做点实际的事";
                case "晋升": return "往上走，走得更远";
                case "安稳": return "把日子过安稳";
                case "搞钱": return "让家里人过得宽裕些";
            }
            return string.IsNullOrEmpty(a) ? "尚未想清楚" : a;
        }

        private static string TaskCheck(GameState st, string main, float bonus)
        {
            var a = st.player.attrs;
            int attr;
            switch (main)
            {
                case "professional": attr = a.professional; break;
                case "admin": attr = a.admin; break;
                case "exec": attr = a.exec; break;
                case "comm": attr = a.comm; break;
                case "political": attr = a.political; break;
                default: attr = 30; break;
            }
            double score = attr * 0.6 + (st.player.energy / 100.0) * 20 + Rng.NextDouble() * 14.0 + bonus * 20.0
                         + st.plan.work * 0.1;   // 周计划·岗位投入：评级加成（Phase 4）
            if (score >= 78) return "S";
            if (score >= 65) return "A";
            if (score >= 52) return "B";
            if (score >= 40) return "C";
            return "D";
        }

        private static int GradeIndex(string g)
        {
            for (int i = 0; i < GradeNames.Length; i++) if (GradeNames[i] == g) return i;
            return 2; // B
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
