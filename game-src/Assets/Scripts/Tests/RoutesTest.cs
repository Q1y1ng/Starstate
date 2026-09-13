using System.Collections.Generic;
using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>
    /// Phase 5 · M1 剧情线路测试：
    /// 路线确立 / 路线门控（只对对应路线开门）/ 链环一次性 / 路线专属卷宗选项锁定 /
    /// 下海结局可达 / 剧情可推动两把尺 / 四条路线各自连通无断档。
    /// </summary>
    public class RoutesTest
    {
        static GameState Fresh()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("线路测试");
            Flow.Begin(st);
            return st;
        }

        /// <summary>把指定事件摆到屏上（绕过调度门槛，只验呈现与选项）。</summary>
        static Scene SceneOf(GameState st, string id)
        {
            st.phase = Phase.Day;
            st.hasPending = false;
            st.queue.Clear();
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.activeDossier = null;
            st.queue.Add(id);
            return Flow.CurrentScene(st);
        }

        /// <summary>走真实的 NextDay → CollectDue 路径推进一天，返回次日入队的事件 id。</summary>
        static List<string> AdvanceOneDay(GameState st)
        {
            st.phase = Phase.Day;
            st.hasPending = false;
            st.queue.Clear();              // 倒空积压（序章 mp1/mp2 等），否则 Choose 只消费队首、日期不动
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.activeDossier = null;
            st.pendingDossierIds.Clear();
            st.week.reviewed = true;
            Flow.Choose(st, 0);
            return new List<string>(st.queue);
        }

        /// <summary>从当前日期自动玩到 untilIso（每日场景必须有选项，否则算断档）。</summary>
        static void AutoPlayTo(GameState st, string untilIso)
        {
            int guard = 0;
            while (string.CompareOrdinal(st.date, untilIso) < 0 && guard++ < 20000)
            {
                if (st.phase == Phase.Ending) return;
                var scene = Flow.CurrentScene(st);
                Assert.IsNotEmpty(scene.options, "场景无选项（断档）：" + scene.title + " @ " + st.date + " / " + st.phase);
                bool dayFfable = st.phase == Phase.Day && !st.hasPending
                    && string.IsNullOrEmpty(st.currentEvent) && st.queue.Count == 0
                    && (st.runtimeEvent == null || st.runtimeEvent.id == "_generic_day" || st.runtimeEvent.id == "_gen_task");
                if (dayFfable) Flow.FastForward(st);
                else Flow.Choose(st, 0);
            }
        }

        static void PlayToRoute(GameState st, int routeOptionIndex)
        {
            st.date = "2027-03-09";
            var queued = AdvanceOneDay(st);
            Assert.Contains("ch_route_open", queued, "2027-03-10 应触发路线确立（实际队列：" + string.Join(",", queued.ToArray()) + "）");
            Flow.Choose(st, routeOptionIndex);
        }

        // ---------------- 路线确立 ----------------

        [Test]
        public void Route_Choice_Sets_Route_And_Mark()
        {
            string[] keys = { "industry", "people", "project", "uplink" };
            for (int i = 0; i < keys.Length; i++)
            {
                var st = Fresh();
                var scene = SceneOf(st, "ch_route_open");
                Assert.AreEqual(4, scene.options.Count, "路线确立应有 4 个选项");
                Flow.Choose(st, i);
                Assert.AreEqual(keys[i], st.route, "选项 " + i + " 应确立路线 " + keys[i]);
                Assert.IsTrue(st.HasMark("route_" + keys[i]), "应写入路线标记 route_" + keys[i]);
                Assert.IsTrue(st.HasMark("route_set"), "应写入 route_set（保证不会再问一次）");
            }
        }

        [Test]
        public void Route_Open_Only_Fires_Once()
        {
            var st = Fresh();
            PlayToRoute(st, 0);
            Assert.IsTrue(st.HasMark("route_set"));

            // 次年同日不应再次触发
            st.date = "2028-03-09";
            var queued = AdvanceOneDay(st);
            Assert.IsFalse(queued.Contains("ch_route_open"), "路线确立不应在次年重复触发");
        }

        // ---------------- 路线门控 ----------------

        [Test]
        public void Route_Chain_Is_Gated_By_Route_Mark()
        {
            // 未确立路线：产业线第一环不应入队
            var a = Fresh();
            a.date = "2027-05-19";
            var q1 = AdvanceOneDay(a);
            Assert.IsFalse(q1.Contains("rte_ind_1"), "未选产业路线时不应触发产业线");

            // 确立产业路线后：同日应入队
            var b = Fresh();
            b.Mark("route_set");
            b.Mark("route_industry");
            b.date = "2027-05-19";
            var q2 = AdvanceOneDay(b);
            Assert.Contains("rte_ind_1", q2, "确立产业路线后应触发产业线第一环");
        }

        [Test]
        public void Route_Chain_Does_Not_Repeat_After_Answered()
        {
            var st = Fresh();
            st.Mark("route_set");
            st.Mark("route_industry");
            st.date = "2027-05-19";
            var q = AdvanceOneDay(st);
            Assert.Contains("rte_ind_1", q);

            // 作答（写入 *_done）
            st.queue.Clear();
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.hasPending = false;
            st.queue.Add("rte_ind_1");
            Flow.CurrentScene(st);
            Flow.Choose(st, 0);
            Assert.IsTrue(st.HasMark("rte_ind_1_done"), "作答后应写入本环完成标记");

            // 次年同日不应重复（md 事件每年都会重新满足条件，靠 requireNotMarks 掐断）
            st.date = "2028-05-19";
            var q2 = AdvanceOneDay(st);
            Assert.IsFalse(q2.Contains("rte_ind_1"), "已作答的链环不应在次年重复触发");
        }

        [Test]
        public void Route_Dossier_Option_Locks_Without_Route()
        {
            var st = Fresh();
            DossierEngine.AssignWeek(st, new[] { "dz_rt_ind" });
            Assert.IsTrue(DossierEngine.OpenNext(st));
            var d = DossierEngine.Current(st);
            Assert.IsNotNull(d);

            var routeOpt = d.options.Find(o => o.whenMark == "route_industry");
            Assert.IsNotNull(routeOpt, "路线专属卷宗应有 route_industry 门控选项");

            string reason;
            Assert.IsFalse(DossierEngine.OptionAvailable(st, routeOpt, out reason), "未确立路线时路线处置应锁定");
            Assert.IsNotEmpty(reason, "锁定应给出原因");

            st.Mark("route_industry");
            Assert.IsTrue(DossierEngine.OptionAvailable(st, routeOpt, out reason), "确立路线后应解锁");
        }

        // ---------------- 下海结局 ----------------

        [Test]
        public void Offer_Chain_Reaches_Resignation_Ending()
        {
            var st = Fresh();
            st.Mark("offer_met");
            var scene = SceneOf(st, "ch_offer_2");
            int resignIdx = -1;
            for (int i = 0; i < scene.options.Count; i++) if (scene.options[i].Contains("签")) resignIdx = i;
            Assert.GreaterOrEqual(resignIdx, 0, "应有接受邀约的选项");
            Assert.AreEqual(scene.options.Count - 1, resignIdx,
                "不可逆的辞职选项必须排在最后（自动推进/连点不应误触结局）");

            Flow.Choose(st, resignIdx);
            Assert.IsTrue(st.resigned, "接受邀约应进入辞职结局");
            Assert.AreEqual(Phase.Ending, st.phase, "应立即进入结局阶段");
            Assert.IsNotNull(st.endingData);
            Assert.AreEqual("结局 · 转身离开", st.endingData.title);
        }

        [Test]
        public void Offer_Report_Branch_Keeps_Playing()
        {
            var st = Fresh();
            st.Mark("offer_met");
            var scene = SceneOf(st, "ch_offer_2");
            int reportIdx = -1;
            for (int i = 0; i < scene.options.Count; i++) if (scene.options[i].Contains("报告")) reportIdx = i;
            Assert.GreaterOrEqual(reportIdx, 0);
            Flow.Choose(st, reportIdx);
            Assert.IsFalse(st.resigned, "拒绝邀约不应结束游戏");
            Assert.IsTrue(st.HasMark("offer_reported"));
            Assert.Greater(Npcs.Get(st, "shenyan").trust, 0, "如实报告应换来纪委信任");
        }

        // ---------------- 纪委谈话的两条分叉（靠 has_violation 标记区分干净/有瑕疵） ----------------

        [Test]
        public void Oversight_Talk_Fork_By_Integrity_History()
        {
            // 无程序违规 → 干净谈话
            var clean = Fresh();
            clean.Mark("pb_1_done");
            clean.date = "2028-09-19";
            var qc = AdvanceOneDay(clean);
            Assert.Contains("ch_pb_3_clean", qc);
            Assert.IsFalse(qc.Contains("ch_pb_3_dirty"), "无违规记录时不应触发带回甘的谈话");

            // 有程序违规 → 另一种谈话（且不能再触发干净版）
            var dirty = Fresh();
            dirty.Mark("pb_1_done");
            Flow.ApplyEffects(dirty, new Effects { integrity = new IntegrityRecord { tag = "产业用地", note = "先签后补" } });
            Assert.IsTrue(dirty.HasMark("has_violation"), "写入程序违规记录时应同步写 mark（否则内容层的门槛永远不生效）");
            dirty.date = "2028-09-19";
            var qd = AdvanceOneDay(dirty);
            Assert.Contains("ch_pb_3_dirty", qd);
            Assert.IsFalse(qd.Contains("ch_pb_3_clean"), "有违规记录时不应再触发干净版谈话");
        }

        // ---------------- 剧情可推动两把尺（Phase 5 · M1 新增引擎能力） ----------------

        [Test]
        public void Chain_Effects_Can_Move_The_Two_Rulers()
        {
            var st = Fresh();
            int c0 = st.compliance, e0 = st.efficiency;

            Flow.ApplyEffects(st, new Effects { compliance = -5, efficiency = 4 });
            Assert.AreEqual(c0 - 5, st.compliance);
            Assert.AreEqual(e0 + 4, st.efficiency);

            // 上下限钳制
            Flow.ApplyEffects(st, new Effects { compliance = -999, efficiency = 999 });
            Assert.AreEqual(0, st.compliance);
            Assert.AreEqual(100, st.efficiency);
        }

        // ---------------- 四条路线连通性（无断档） ----------------

        [Test]
        public void All_Four_Route_Lines_Are_Playable()
        {
            for (int route = 0; route < 4; route++)
            {
                var st = Fresh();
                PlayToRoute(st, route);
                AutoPlayTo(st, "2029-12-31");
                Assert.AreNotEqual(Phase.Ending, st.phase,
                    "路线 " + route + " 不应在 2029 年前进入结局（@ " + st.date + "）");
            }
        }

        [Test]
        public void Route_Chain_Events_Are_All_Registered()
        {
            string[] ids =
            {
                "ch_route_open", "ch_route_echo",
                "rte_ind_1", "rte_ind_2", "rte_ind_3",
                "rte_ppl_1", "rte_ppl_2", "rte_ppl_3", "ch_ppl_fund_back",
                "rte_prj_1", "rte_prj_2", "rte_prj_3",
                "rte_upl_1", "rte_upl_2", "rte_upl_3",
                "ch_pb_1", "ch_pb_2", "ch_pb_3_clean", "ch_pb_3_dirty",
                "ch_rival_1", "ch_rival_2", "ch_rival_3",
                "ch_offer_1", "ch_offer_2",
            };
            ContentRegistry.RegisterAll();
            foreach (var id in ids)
                Assert.IsTrue(Flow.IsRegistered(id), "剧情事件未注册：" + id);

            string[] dossiers = { "dz_rt_ind", "dz_rt_ppl", "dz_rt_prj", "dz_rt_upl" };
            foreach (var id in dossiers)
                Assert.IsTrue(DossierEngine.IsRegistered(id), "路线卷宗未注册：" + id);

            Assert.IsEmpty(DossierEngine.ValidationErrors,
                "路线卷宗自检未通过：" + string.Join("；", DossierEngine.ValidationErrors.ToArray()));
        }
    }
}
