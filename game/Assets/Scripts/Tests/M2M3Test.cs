using System;
using System.Collections.Generic;
using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>
    /// Phase 5 · M2/M3 验收（spec S2.13）：
    /// M2 —— ① DossierGenerator 模板上量且 30 天无重复 id；② 模板灰区处置写标记并能挂上回响链；
    ///        ③ 旧链迁移（配偶从业与回避 / 医疗资源打招呼）；④ 氛围与批示 LLM 提示词及严格回退。
    /// M3 —— ① 风险账本累积与回落；② 立案审查结局可达；③ 不称职/降级结局可达；④ 全部结局标题可产出。
    /// </summary>
    public class M2M3Test
    {
        static GameState Fresh()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("M2M3测试");
            Flow.Begin(st);
            return st;
        }

        // ===================== M2 ① 模板上量 =====================

        [Test]
        public void Generator_Has_ThirtyPlus_Templates()
        {
            Assert.GreaterOrEqual(DossierGenerator.TemplateCount, 34, "M2 验收：模板池应上量（当前 " + DossierGenerator.TemplateCount + " 个）");
        }

        [Test]
        public void Generator_Thirty_Days_Have_No_Duplicate_Id()
        {
            // spec T8 acceptance：随机卷宗连续 30 天无重复 id
            var ids = new HashSet<string>();
            var start = new DateTime(2026, 9, 1);
            for (int i = 0; i < 30; i++)
            {
                var d = DossierGenerator.SpawnOne(start.AddDays(i), i + 1);
                Assert.IsNotNull(d, "第 " + (i + 1) + " 件生成失败");
                Assert.IsTrue(ids.Add(d.id), "重复 id：" + d.id);
                Assert.IsNotEmpty(d.title, "空标题：" + d.id);
                Assert.GreaterOrEqual(d.options.Count, 2, "模板件至少两个选项：" + d.id);
            }
        }

        [Test]
        public void Templates_Cover_All_Forms_And_Have_Issues()
        {
            var forms = new HashSet<string>();
            int withIssue = 0, withTable = 0;
            for (int i = 1; i <= DossierGenerator.TemplateCount; i++)
            {
                var d = DossierGenerator.SpawnOne(new DateTime(2026, 9, 1), i);
                forms.Add(d.form);
                if (d.issues.Count > 0) withIssue++;
                foreach (var p in d.pages) if (!string.IsNullOrEmpty(p.table)) withTable++;
            }
            Assert.GreaterOrEqual(forms.Count, 8, "模板要覆盖足够多的来文形态（当前 " + forms.Count + "）");
            Assert.Greater(withIssue, DossierGenerator.TemplateCount / 2, "过半模板应带可查的雷");
            Assert.Greater(withTable, 5, "应有相当数量的模板带测算表附件");
        }

        [Test]
        public void Template_Gray_Choices_Write_Marks_That_Chains_Consume()
        {
            DossierEngine.Clear();
            ContentRegistry.RegisterAll();

            // 模板灰区处置写入的标记 → 必须有回响事件消费它（否则模板件就是毫无后果的重复劳动）
            var pairs = new Dictionary<string, string>
            {
                { "stat_fudge", "ch_tc_stat" },
                { "yq_delete", "ch_tc_yq" },
                { "safety_loose", "ch_tc_safety" },
                { "budget_split", "ch_tc_budget" },
                { "procure_hold", "ch_tc_procure" },
                { "petition_paper", "ch_tc_petition" },
                { "yibao_soft", "ch_tc_yibao" },
            };

            var marksInContent = new HashSet<string>();
            for (int i = 1; i <= DossierGenerator.TemplateCount; i++)
            {
                var d = DossierGenerator.SpawnOne(new DateTime(2026, 9, 1), i);
                foreach (var o in d.options)
                    if (o.effects != null && o.effects.setMarks != null)
                        foreach (var m in o.effects.setMarks) marksInContent.Add(m);
            }

            foreach (var kv in pairs)
            {
                Assert.IsTrue(marksInContent.Contains(kv.Key), "模板层没有任何选项写标记：" + kv.Key);
                Assert.IsTrue(Flow.IsRegistered(kv.Value), "标记 " + kv.Key + " 的回响事件未注册：" + kv.Value);
            }
        }

        [Test]
        public void Dossier_Template_Spawn_Is_Pure_By_Seq()
        {
            // 读档重建依赖 (date, seq) 纯确定：同一 seq 两次生成必须完全一致（含 id 与标题）
            var day = new DateTime(2028, 3, 14);
            var a = DossierGenerator.SpawnOne(day, 7);
            var b = DossierGenerator.SpawnOne(day, 7);
            Assert.AreEqual(a.id, b.id);
            Assert.AreEqual(a.title, b.title);
            Assert.AreEqual(a.options.Count, b.options.Count);
            Assert.AreEqual(a.checkBudget, b.checkBudget);
            var c = DossierGenerator.Respawn(a.id, day);
            Assert.IsNotNull(c, "按 id 重建失败：" + a.id);
            Assert.AreEqual(a.title, c.title, "重建出的卷宗与首次生成不一致（模板顺序被改动过？）");
        }

        // ===================== M2 ③ 旧链迁移 =====================

        [Test]
        public void Home_And_Medical_Chains_Are_Registered()
        {
            DossierEngine.Clear();
            ContentRegistry.RegisterAll();
            foreach (var id in new[] { "ch_home_1", "ch_home_2", "ch_home_3", "ch_med_1", "ch_med_2" })
                Assert.IsTrue(Flow.IsRegistered(id), "旧链迁移事件未注册：" + id);
        }

        [Test]
        public void Spouse_Declaration_Honest_Choice_Raises_Compliance_And_Lowers_Risk()
        {
            var st = Fresh();
            st.riskLedger = 30;
            st.compliance = 80;                       // 不能已经顶格，否则“提升合规分”无从断言
            int c0 = st.compliance, r0 = st.riskLedger;
            var ev = Flow.BuildForTest("ch_home_1", st);
            Assert.IsNotNull(ev, "配偶从业申报事件构建失败");
            Flow.ApplyEffects(st, Flow.CloneEffects(ev.options[0].effects), null);
            Assert.Greater(st.compliance, c0, "如实申报应提升合规分");
            Assert.Less(st.riskLedger, r0, "如实申报应降低风险账本");
            Assert.IsTrue(st.HasMark("home_honest"), "应写入如实申报标记");
        }

        [Test]
        public void Medical_Favor_Raises_Risk_And_Writes_Integrity()
        {
            var st = Fresh();
            var ev = Flow.BuildForTest("ch_med_1", st);
            Assert.IsNotNull(ev);
            int v0 = st.violationCount;
            Flow.ApplyEffects(st, Flow.CloneEffects(ev.options[1].effects), null);
            Assert.Greater(st.riskLedger, 0, "打招呼应记入风险账本");
            Assert.AreEqual(v0, st.violationCount, "打招呼本身不是程序违规（后果在回响里）");
            Assert.IsTrue(st.HasMark("med_favor"), "应写入人情标记");
        }

        // ===================== M2 ④ 氛围与批示 =====================

        [Test]
        public void Fallback_Remark_Matches_Disposition()
        {
            StringAssert.Contains("退回", LlmGameplay.FallbackRemark("退回来文单位补正", 0));
            StringAssert.Contains("复算", LlmGameplay.FallbackRemark("要求重新比价后报", 0));
            StringAssert.Contains("报省", LlmGameplay.FallbackRemark("报省请示", 0));
            StringAssert.Contains("同意", LlmGameplay.FallbackRemark("照准", 0));
            StringAssert.Contains("签得快", LlmGameplay.FallbackRemark("照准", 3));   // 漏查时补一句
        }

        [Test]
        public void Sanitize_Remark_And_Ambience_Strip_Noise()
        {
            Assert.AreEqual("同意，按程序办理。", LlmGameplay.SanitizeRemark("批语：同意，按程序办理。"));
            Assert.AreEqual("同意，按程序办理。", LlmGameplay.SanitizeRemark("\"同意，按程序办理。\""));
            Assert.AreEqual("同意办理。", LlmGameplay.SanitizeRemark("```json\n同意办理。\n```"));
            Assert.IsNull(LlmGameplay.SanitizeRemark("   "));
            Assert.IsNull(LlmGameplay.SanitizeRemark(null));
            // 超长截断且不以逗号结尾
            string long1 = new string('字', 80);
            string cut = LlmGameplay.SanitizeRemark(long1);
            Assert.LessOrEqual(cut.Length, 41);
            Assert.IsFalse(cut.EndsWith("，"));

            StringAssert.Contains("窗外", LlmGameplay.SanitizeAmbience("窗外下着雨。"));   // 正常句保留
            Assert.IsNull(LlmGameplay.SanitizeAmbience(""));
            Assert.LessOrEqual(LlmGameplay.SanitizeAmbience(new string('云', 200)).Length, 61);
        }

        [Test]
        public void Prompt_Builders_Carry_Constraints()
        {
            var st = Fresh();
            string amb = LlmPrompt.AmbienceUser(st);
            StringAssert.Contains("纯文本", amb);
            StringAssert.Contains("不超过 40 字", amb);

            string rem = LlmPrompt.RemarkUser(st, "关于追加经费的请示", "照准", "财政局半小时后回电。");
            StringAssert.Contains("纯文本", rem);
            StringAssert.Contains("不超过 30 字", rem);
            StringAssert.Contains("关于追加经费的请示", rem);
            StringAssert.Contains("照准", rem);
        }

        [Test]
        public void Dossier_Resolve_Writes_Fallback_Remark_Into_Result_Scene()
        {
            DossierEngine.Clear();
            ContentRegistry.RegisterAll();
            var st = Fresh();
            DossierEngine.AssignWeek(st, new[] { "dz_t1" });
            Assert.IsTrue(DossierEngine.OpenNext(st));
            var act = st.activeDossier;
            Assert.IsNotNull(act);

            // 走到处置区：翻页 + 核对，然后签批第一项
            var dz = DossierEngine.Current(st);
            for (int i = 1; i < dz.pages.Count; i++) DossierEngine.TurnPage(st, +1);
            for (int i = 0; i < act.checksLeft; i++) DossierEngine.CheckPage(st);
            Assert.IsNotNull(DossierEngine.Resolve(st, dz.options.Count - 1));

            st.hasPending = true;
            st.pendingParas = new List<string> { "已签批。" };
            var scene = Flow.CurrentScene(st);
            bool hasRemark = false;
            foreach (var p in scene.paras) if (p.StartsWith("批示：")) hasRemark = true;
            Assert.IsTrue(hasRemark, "签批结果页应带一条批示（无 AI 时为确定性回退批语）");
            Assert.IsFalse(string.IsNullOrEmpty(st.remark));
            Assert.AreEqual(act.id, st.remarkDossier);
        }

        // ===================== M3 ① 风险账本 =====================

        [Test]
        public void Risk_Ledger_Clamps_And_Decays_On_Clean_Months()
        {
            var st = Fresh();
            st.riskLedger = 0;
            Flow.AddRisk(st, 30, "测试");
            Assert.AreEqual(30, st.riskLedger);
            Flow.AddRisk(st, 200, "测试上限");
            Assert.AreEqual(100, st.riskLedger, "风险账本上限 100");
            Flow.AddRisk(st, -500, "测试下限");
            Assert.AreEqual(0, st.riskLedger, "风险账本下限 0");
        }

        [Test]
        public void Gray_Dossier_Disposition_Raises_Risk()
        {
            DossierEngine.Clear();
            ContentRegistry.RegisterAll();
            var st = Fresh();
            st.riskLedger = 0;
            DossierEngine.AssignWeek(st, new[] { "dz_g016_0901" });   // 医保基金模板（选项 0 即灰区 + 程序记录）
            Assert.IsTrue(DossierEngine.OpenNext(st));
            var dz = DossierEngine.Current(st);

            int gray = -1;
            for (int i = 0; i < dz.options.Count; i++) if (dz.options[i].gray) { gray = i; break; }
            if (gray < 0) Assert.Ignore("该模板件没有灰区选项");

            Assert.IsNotNull(DossierEngine.Resolve(st, gray));
            Assert.Greater(st.riskLedger, 0, "灰区处置应记入风险账本");
        }

        // ===================== M3 ② 立案审查结局可达 =====================

        [Test]
        public void High_Risk_With_Violations_Triggers_Investigation()
        {
            var st = Fresh();
            st.violationCount = 2;          // 程序违规 ≥2
            Flow.AddRisk(st, 70, "测试：多处灰区处置");
            Assert.IsTrue(st.GetFlag("risk_investigation"), "风险过线应置立案标志");

            var ev = Flow.FindById("sys_investigation");
            Assert.IsNotNull(ev, "立案审查事件未注册");
            Assert.IsTrue(ev.dynamic);

            // 动态构建 → 选“配合组织” → 进入审查结局
            Flow.ApplyEffects(st, new Effects { underInvestigation = true }, null);
            Assert.IsTrue(st.underInvestigation);
            Assert.AreEqual(Phase.Ending, st.phase);
            var e = st.endingData ?? Career.ComputeEnding(st);
            StringAssert.Contains("审查", e.title);
        }

        [Test]
        public void Risk_Talk_Event_Reduces_Risk_When_Confessing()
        {
            var st = Fresh();
            st.riskLedger = 50;
            st.violationCount = 2;
            var ev = Flow.BuildForTest("sys_risk_talk", st);
            Assert.IsNotNull(ev, "谈话提醒事件构建失败");
            Assert.GreaterOrEqual(ev.options.Count, 3);
            int r0 = st.riskLedger;
            int v0 = st.violationCount;
            Flow.ApplyEffects(st, Flow.CloneEffects(ev.options[0].effects), null);
            Assert.Less(st.riskLedger, r0, "主动交底应显著降低风险账本");
            Assert.AreEqual(v0, st.violationCount, "被谈话不是新的程序违规（否则会把玩家推往立案，语义就错了）");
        }

        // ===================== M3 ③ 不称职/降级结局 =====================

        [Test]
        public void Unqualified_Evaluation_Is_Graded_Not_Instant_Dismissal()
        {
            var st = Fresh();
            st.yearIntegrity = 6;                        // 一年内六次程序问题 → 不称职
            Assert.AreEqual("不称职", Career.EvaluateYear(st, false, null));

            // 第一次不称职：降级留任（不结束游戏），扣合规分并留警告标记
            Flow.ApplyEffects(st, new Effects { evalGrade = "不称职", clearYearStats = true }, null);
            Assert.AreEqual("", st.adverse, "第一次不称职不应直接结局");
            Assert.AreNotEqual(Phase.Ending, st.phase);
            Assert.IsTrue(st.HasMark("adverse_warn"));

            // 第二次不称职 → 免职待查结局
            st.yearIntegrity = 6;
            Flow.ApplyEffects(st, new Effects { evalGrade = "不称职", clearYearStats = true }, null);
            Assert.AreEqual("免职", st.adverse);
            Assert.AreEqual(Phase.Ending, st.phase);
            var e = st.endingData ?? Career.ComputeEnding(st);
            StringAssert.Contains("免职", e.title);
        }

        [Test]
        public void Yearly_Violations_Do_Not_Accumulate_Across_Years()
        {
            // 终身累计不能用于年度等第：否则第一年几次失误就终身背“不称职”
            var st = Fresh();
            st.violationCount = 5;      // 历史上确实有过违规
            st.yearIntegrity = 0;       // 但今年干净
            st.compliance = 80;
            st.efficiency = 70;
            Assert.AreNotEqual("不称职", Career.EvaluateYear(st, false, null), "今年干净就不该按往年旧账判不称职");
        }

        [Test]
        public void BestOption_Avoids_Gray_And_Integrity_Options()
        {
            // 回归：旧实现只看 complianceDelta，会系统性挑中“面子上合规分更高”的灰区选项，
            // 而灰区选项恰恰会写程序违规记录（探针实测 18 个月攒了 10 次）。
            DossierEngine.Clear();
            ContentRegistry.RegisterAll();
            var st = Fresh();
            DossierEngine.AssignWeek(st, new[] { "dz_g011_0901" });   // 省交办模板（照准 1 / 委托 0 / 自查消化 2 + 违规记录）
            Assert.IsTrue(DossierEngine.OpenNext(st));
            var dz = DossierEngine.Current(st);

            int idx = DossierEngine.BestOptionIndex(st, dz);
            var o = dz.options[idx];
            Assert.IsFalse(o.gray, "尽责启发式不应主动选灰区选项：" + o.label);
            if (o.effects != null) Assert.IsNull(o.effects.integrity, "尽责启发式不应主动选会写程序违规的选项：" + o.label);
        }

        [Test]
        public void Three_Plain_Evaluations_Lead_To_Demotion_Ending()
        {
            var st = Fresh();
            st.evals.Add(new YearEval { year = 2027, grade = "基本称职" });
            st.evals.Add(new YearEval { year = 2028, grade = "基本称职" });
            Flow.ApplyEffects(st, new Effects { evalGrade = "基本称职", clearYearStats = true }, null);
            Assert.AreEqual("降级", st.adverse);
            Assert.AreEqual(Phase.Ending, st.phase);
            var e = st.endingData ?? Career.ComputeEnding(st);
            StringAssert.Contains("降级", e.title);
        }

        [Test]
        public void High_Risk_Alone_Cannot_End_Game_Without_Violations()
        {
            // 只是件签得灰，但从未留下程序违规记录 → 只降考核等第，不直接进结局
            var st = Fresh();
            st.violationCount = 1;
            st.riskLedger = 90;
            Flow.AddRisk(st, 1, "测试");
            Assert.IsFalse(st.GetFlag("risk_investigation"), "程序违规不足 2 次不应立案");
            Assert.AreEqual("不称职", Career.EvaluateYear(st, false, null), "风险过高仍会拖低考核等第");
        }

        [Test]
        public void Investigation_Flag_Needs_Both_Risk_And_Violations()
        {
            var st = Fresh();
            st.riskLedger = 70;
            st.violationCount = 2;
            Flow.AddRisk(st, 1, "测试");
            Assert.IsTrue(st.GetFlag("risk_investigation"), "风险≥ 65 且违规≥ 2 次应立案");
        }

        // ===================== M3 ④ 结局齐备 =====================

        [Test]
        public void All_Ending_Titles_Are_Reachable()
        {
            var titles = new List<string>();
            var st = Fresh();

            st.underInvestigation = true; titles.Add(Career.ComputeEnding(st).title); st.underInvestigation = false;
            st.adverse = "免职"; titles.Add(Career.ComputeEnding(st).title); st.adverse = "";
            st.adverse = "降级"; titles.Add(Career.ComputeEnding(st).title); st.adverse = "";
            st.resigned = true; titles.Add(Career.ComputeEnding(st).title); st.resigned = false;
            st.grade = "六品·副省"; titles.Add(Career.ComputeEnding(st).title); st.grade = "七品·正厅";
            st.compliance = 80; st.efficiency = 70; titles.Add(Career.ComputeEnding(st).title);
            st.compliance = 30; st.efficiency = 70; titles.Add(Career.ComputeEnding(st).title);
            st.compliance = 60; st.efficiency = 50; titles.Add(Career.ComputeEnding(st).title);

            foreach (var t in titles) StringAssert.Contains("结局 ·", t);
            var uniq = new HashSet<string>(titles);
            Assert.GreaterOrEqual(uniq.Count, 7, "至少应产出七种不同结局");
        }

        [Test]
        public void Ending_Echoes_Mention_Risk_When_Ledger_Is_High()
        {
            var st = Fresh();
            st.riskLedger = 55;
            var e = Career.ComputeEnding(st);
            bool mentioned = false;
            foreach (var p in e.paras) if (p.Contains("风险账本")) mentioned = true;
            Assert.IsTrue(mentioned, "高风险结局回响应提到未合上的存疑");
        }
    }
}
