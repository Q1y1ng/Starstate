using System.Collections.Generic;
using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>
    /// Phase 5 · M1 内容量验收（spec S2.13）：
    /// ① 手写卷宗 ≥40 件、来文形态齐全、全部通过自检；
    /// ② 核心 NPC 台词池扩容 + 状态感知开场白 + 关系解锁选项；
    /// ③ 两条新链（安全事故瞒报压力 / 招商引资对赌）的门控与收束。
    /// </summary>
    public class ContentVolumeTest
    {
        static GameState Fresh()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("内容量测试");
            Flow.Begin(st);
            return st;
        }

        static List<string> AdvanceOneDay(GameState st)
        {
            st.phase = Phase.Day;
            st.hasPending = false;
            st.queue.Clear();
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.activeDossier = null;
            st.pendingDossierIds.Clear();
            st.week.reviewed = true;
            Flow.Choose(st, 0);
            return new List<string>(st.queue);
        }

        // ---------------- ① 手写卷宗量与形态 ----------------

        [Test]
        public void Handwritten_Dossiers_Reach_M1_Target()
        {
            DossierEngine.Clear();
            ContentRegistry.RegisterAll();

            int handwritten = 0;
            var forms = new HashSet<string>();
            foreach (var id in DossierEngine.RegisteredIds())
            {
                var d = DossierEngine.Clone(id);
                if (d == null || d.generated) continue;   // 模板件不计入手写量
                handwritten++;
                forms.Add(d.form);
            }
            Assert.GreaterOrEqual(handwritten, 40, "M1 验收：第一年手写卷宗应 ≥40 件（当前 " + handwritten + "）");

            // spec S2.6 的来文形态应基本齐备
            string[] required = { "请示", "财政件", "会议材料", "人事单", "信访件", "省交办", "巡视整改", "协议", "对上报告", "土地件", "突发事件" };
            foreach (var f in required)
                Assert.IsTrue(forms.Contains(f), "来文形态缺失：" + f);

            Assert.IsEmpty(DossierEngine.ValidationErrors,
                "卷宗自检未通过：" + string.Join("；", DossierEngine.ValidationErrors.ToArray()));
        }

        [Test]
        public void No_Dossier_Week_Is_Ever_Empty_In_First_Year()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("断档测试");
            // 第一年 52 周，每周都必须装配到件（M1 验收：十月内无断档）
            for (int week = 0; week < 52; week++)
            {
                st.date = GameClock.Iso(GameClock.AddDays(GameClock.Parse(st.date), 7));
                DossierEngine.AssignWeek(st);
                Assert.Greater(st.pendingDossierIds.Count, 0, "第 " + week + " 周案头为空");
            }
        }

        // ---------------- ② NPC 台词与状态感知 ----------------

        [Test]
        public void Every_Standing_Committee_Member_Has_Deep_Fallback_Pool()
        {
            foreach (var id in Npcs.Order)
            {
                Assert.IsTrue(ContentNpcTalk.Lines.ContainsKey(id), id + " 缺内置台词池");
                Assert.GreaterOrEqual(ContentNpcTalk.Lines[id].Length, 6, id + " 的台词池应 ≥6 句（避免一周内重复）");

                var st = Fresh();
                var dto = ContentNpcTalk.FallbackTalk(st, id);
                Assert.IsNotNull(dto);
                Assert.IsNotEmpty(dto.greeting);
                Assert.GreaterOrEqual(dto.options.Length, 2, id + " 的回退交谈应 ≥2 个选项");
                bool warm = false;
                foreach (var o in dto.options) if (o.mood == "warm") warm = true;
                Assert.IsTrue(warm, id + " 的回退交谈应含 warm 选项（可写关系）");
            }
        }

        [Test]
        public void State_Lines_React_To_Two_Rulers_Route_And_Records()
        {
            // ① 有程序违规记录 → 纪委台词变味
            var st = Fresh();
            Flow.ApplyEffects(st, new Effects { integrity = new IntegrityRecord { tag = "产业用地", note = "先签后补" } });
            StringAssert.Contains("标了记号", ContentNpcTalk.StateLine(st, "shenyan"));

            // ② 效率过低 → 常务副搬出进度表
            var slow = Fresh();
            slow.efficiency = 30;
            StringAssert.Contains("进度表", ContentNpcTalk.StateLine(slow, "shao"));

            // ③ 路线不同 → 主席的说法不同
            var people = Fresh();
            people.route = "people";
            StringAssert.Contains("民生", ContentNpcTalk.StateLine(people, "cen"));
            var project = Fresh();
            project.route = "project";
            StringAssert.Contains("开工率", ContentNpcTalk.StateLine(project, "cen"));

            // ④ 同批先晋 → 组织部的材料口径跟着变
            var han = Fresh();
            han.Mark("rival_1_done");
            StringAssert.Contains("同批", ContentNpcTalk.StateLine(han, "han"));

            // ⑤ 家庭压力 → 配偶台词转向身体
            var tired = Fresh();
            tired.player.stress = 90;
            StringAssert.Contains("白头发", ContentNpcTalk.StateLine(tired, "linwan"));

            // ⑥ 没有特别状态时不硬编台词（走基础池）
            var calm = Fresh();
            Assert.IsNull(ContentNpcTalk.StateLine(calm, "rensheng"));
        }

        [Test]
        public void Talk_Options_Unlock_By_Relationship()
        {
            var st = Fresh();
            var r = Npcs.Get(st, "cen");
            r.familiar = 0;   // 先归零，否则 cen 的初始熟悉度(70) 会让门槛已经满足
            r.trust = 0;
            int baseCount = ContentNpcTalk.FallbackTalk(st, "cen").options.Length;
            Assert.AreEqual(3, baseCount, "零关系时应只有 3 个基础选项");

            r.trust = 40;
            int trustCount = ContentNpcTalk.FallbackTalk(st, "cen").options.Length;
            Assert.Greater(trustCount, baseCount, "信任到 25 以上应解锁“说给他听”");

            r.familiar = 60;
            int famCount = ContentNpcTalk.FallbackTalk(st, "cen").options.Length;
            Assert.Greater(famCount, trustCount, "熟悉到 50 以上应解锁“帮着把关材料”");
        }

        // ---------------- 两把尺的长线稳定性（防死亡螺旋） ----------------

        [Test]
        public void FastForward_Does_Not_Death_Spiral_The_Two_Rulers()
        {
            ContentRegistry.RegisterAll();
            Flow.SeedRng(20260901);
            var st = State.NewGame("长跑稳定性");
            Flow.Begin(st);

            int guard = 0;
            while (st.phase != Phase.Ending && guard++ < 200000
                   && string.CompareOrdinal(st.date, "2031-01-01") < 0)
            {
                var sc = Flow.CurrentScene(st);
                Assert.IsNotEmpty(sc.options, "断档：" + sc.title);
                bool ff = st.phase == Phase.Day && !st.hasPending && string.IsNullOrEmpty(st.currentEvent)
                          && st.queue.Count == 0
                          && (st.runtimeEvent == null || st.runtimeEvent.id == "_generic_day" || st.runtimeEvent.id == "_gen_task");
                if (ff) Flow.FastForward(st); else Flow.Choose(st, 0);
            }

            // 旧实现：快进不核对 → 每件满额漏雷扣分 → 合规必然见底、两把尺退化成一把、结局被锁死
            Assert.Greater(st.compliance, 20, "快进四年后合规分不应见底（@" + st.date + "：" + st.compliance + "）");
            Assert.Greater(st.efficiency, 20, "快进四年后效率分不应见底（@" + st.date + "：" + st.efficiency + "）");
        }

        [Test]
        public void Clean_Handling_Never_Drains_Efficiency()
        {
            var st = Fresh();
            DossierEngine.Clear();
            ContentRegistry.RegisterAll();
            DossierEngine.AssignWeek(st, new[] { "dz_t1" });
            Assert.IsTrue(DossierEngine.OpenNext(st));
            int e0 = st.efficiency;

            // 不核对（会漏雷）→ 效率不得上涨；核出后“退回”→ 至少不下降
            DossierEngine.TurnPage(st, +1);
            Assert.IsNotNull(DossierEngine.CheckPage(st));
            DossierEngine.Resolve(st, 1 + 1 + 1);   // 处置区第一项（退回类）
            Assert.GreaterOrEqual(st.efficiency, e0, "办得干净时，谨慎处置不应继续倒扣效率");
        }

        // ---------------- ③ 两条新链 ----------------

        [Test]
        public void Safety_Chain_Branches_By_Report_Choice()
        {
            // 软口径 → 压力事件
            var soft = Fresh();
            soft.Mark("safety_soft");
            soft.date = "2027-08-04";
            var qs = AdvanceOneDay(soft);
            Assert.Contains("ch_sc_pressure", qs);
            Assert.IsFalse(qs.Contains("ch_sc_honest_echo"));
            Assert.IsFalse(qs.Contains("ch_sc_probe_echo"));

            // 现场复查 → 复查回响；且与软口径互斥
            var probe = Fresh();
            probe.Mark("safety_probe");
            probe.date = "2027-09-17";
            var qp = AdvanceOneDay(probe);
            Assert.Contains("ch_sc_probe_echo", qp);
            Assert.IsFalse(qp.Contains("ch_sc_pressure"));

            // 如实上报 → 省里认可（但其触发日 09-10 要走到那天）
            var honest = Fresh();
            honest.Mark("safety_honest");
            honest.date = "2027-09-09";
            var qh = AdvanceOneDay(honest);
            Assert.Contains("ch_sc_honest_echo", qh);

            // 卷宗侧的三个出口标记确实存在（内容与链对齐）
            var st = Fresh();
            DossierEngine.AssignWeek(st, new[] { "dz_sc_safety" });
            Assert.IsTrue(DossierEngine.OpenNext(st));
            var d = DossierEngine.Current(st);
            Assert.AreEqual(3, d.options.Count);
            Assert.Contains("safety_soft", d.options[0].effects.setMarks);
            Assert.Contains("safety_honest", d.options[1].effects.setMarks);
            Assert.Contains("safety_probe", d.options[2].effects.setMarks);
        }

        [Test]
        public void Safety_Pressure_Is_One_Shot_And_Moves_Compliance()
        {
            var st = Fresh();
            st.Mark("safety_soft");
            st.date = "2027-08-05";
            st.phase = Phase.Day;
            st.hasPending = false;
            st.queue.Clear();
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.activeDossier = null;
            st.queue.Add("ch_sc_pressure");

            int c0 = st.compliance = 60;   // 合规分先降下来，否则起点就是上限 100，涨不动
            Flow.CurrentScene(st);
            Flow.Choose(st, 0);           // 当场改口补报
            Assert.Greater(st.compliance, c0, "主动补报应提高合规分");
            Assert.IsTrue(st.HasMark("sc_pressure_done"));

            st.date = "2028-08-04";
            var q = AdvanceOneDay(st);
            Assert.IsFalse(q.Contains("ch_sc_pressure"), "收束后的链不应在次年重发");
        }

        [Test]
        public void Trust_Chain_Has_A_Weak_And_A_Hard_Line()
        {
            // 宽条款族：变更 → 二次兑现
            var weak = Fresh();
            weak.Mark("aitrust_weak");
            weak.date = "2027-12-05";
            var q1 = AdvanceOneDay(weak);
            Assert.Contains("ch_ai_weak_2", q1);
            Assert.IsFalse(q1.Contains("ch_ai_hard_2"), "宽/严两条线必须互斥");

            weak.Mark("ai_weak2_done");
            weak.date = "2029-09-24";
            var q2 = AdvanceOneDay(weak);
            Assert.Contains("ch_ai_weak_pay2", q2);
            Assert.IsFalse(q2.Contains("ch_ai_hard_pay"));

            // 严条款族：核数 → 对账
            var hard = Fresh();
            hard.Mark("aitrust_hard");
            hard.date = "2027-12-05";
            var q3 = AdvanceOneDay(hard);
            Assert.Contains("ch_ai_hard_2", q3);

            hard.Mark("ai_hard2_done");
            hard.date = "2029-09-24";
            var q4 = AdvanceOneDay(hard);
            Assert.Contains("ch_ai_hard_pay", q4);
            Assert.IsFalse(q4.Contains("ch_ai_weak_pay2"));
        }

        [Test]
        public void New_Trust_Dossier_Writes_Both_Specific_And_Family_Mark()
        {
            var st = Fresh();
            DossierEngine.AssignWeek(st, new[] { "dz_ai_terms" });
            Assert.IsTrue(DossierEngine.OpenNext(st));

            Flow.Choose(st, 0);   // 卷宗场景：选项 0 = 照准
            // 直接按卷宗选项结算更稳：走引擎
            var d = DossierEngine.Current(st);
            if (d != null && !st.activeDossier.resolved)
            {
                DossierEngine.Resolve(st, 0);
            }
            Assert.IsTrue(st.HasMark("aitrust_weak2"), "应写入本条的具体标记");
            Assert.IsTrue(st.HasMark("aitrust_weak"), "应同时写入族标记，否则宽/严两条链都接不上");
        }
    }
}
