using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>Phase 5 卷宗引擎：照准/退回/逾期/埋雷未查/埋雷查出 五路径。</summary>
    public class DossierEngineTest
    {
        [SetUp]
        public void Setup()
        {
            ContentRegistry.RegisterAll();
            ContentDossierM0.Register();
            ContentCareerMayor.Register();
        }

        [Test]
        public void Register_And_Assign_Week()
        {
            var st = State.NewGame("卷宗测试员");
            DossierEngine.AssignWeek(st);
            Assert.Greater(st.pendingDossierIds.Count, 0, "应装配待办卷宗");
            Assert.IsTrue(DossierEngine.HasWork(st));
        }

        [Test]
        public void Open_Check_Resolve_Missed_Issue_Loses_Compliance()
        {
            var st = State.NewGame("漏查测试");
            DossierEngine.Clear();
            ContentDossierM0.Register();
            DossierEngine.AssignWeek(st, new[] { "dz_t1" });
            Assert.IsTrue(DossierEngine.OpenNext(st));
            Assert.IsNotNull(st.activeDossier);

            int c0 = st.compliance;
            // 不核对，直接照准（第一个选项）
            var d = DossierEngine.Current(st);
            int opt = d.options.FindIndex(o => o.label.Contains("照准"));
            Assert.GreaterOrEqual(opt, 0);
            var entry = DossierEngine.Resolve(st, opt);
            Assert.IsNotNull(entry);
            Assert.AreEqual(1, entry.issuesMissed, "未核对应漏查 1 处数字雷");
            Assert.Less(st.compliance, c0, "漏查应扣合规分");
        }

        [Test]
        public void Check_Discovers_Issue_Then_Return_Preserves_Compliance()
        {
            var st = State.NewGame("查雷测试");
            DossierEngine.Clear();
            ContentDossierM0.Register();
            DossierEngine.AssignWeek(st, new[] { "dz_t1" });
            DossierEngine.OpenNext(st);

            // 翻到第 2 页（测算表）并核对
            DossierEngine.TurnPage(st, +1);
            var found = DossierEngine.CheckPage(st);
            Assert.IsNotNull(found, "第2页应可查出合计错误");
            Assert.IsTrue(st.activeDossier.foundIssues.Contains(found.id));

            int c0 = st.compliance;
            var d = DossierEngine.Current(st);
            int opt = d.options.FindIndex(o => o.label.Contains("退回"));
            Assert.GreaterOrEqual(opt, 0);
            var entry = DossierEngine.Resolve(st, opt);
            Assert.IsNotNull(entry);
            Assert.AreEqual(0, entry.issuesMissed);
            Assert.AreEqual(1, entry.issuesFound);
            Assert.GreaterOrEqual(st.compliance, c0, "查出后退回不应因漏雷扣分");
        }

        [Test]
        public void Option_WhenMark_Locks_Without_Mark()
        {
            var st = State.NewGame("锁定测试");
            DossierEngine.Clear();
            DossierEngine.Register(new Dossier
            {
                id = "dz_lock_test",
                kind = "routine",
                form = "请示",
                title = "测试件",
                pages = { new DossierPage { title = "正文", paras = { "内容" } } },
                options =
                {
                    new DossierOption { label = "普通处置", effects = new Effects() },
                    new DossierOption { label = "特殊处置", whenMark = "never_marked", lockReason = "还缺一个契机", effects = new Effects() },
                },
            });
            DossierEngine.AssignWeek(st, new[] { "dz_lock_test" });
            DossierEngine.OpenNext(st);
            var d = DossierEngine.Current(st);
            string reason;
            Assert.IsTrue(DossierEngine.OptionAvailable(st, d.options[0], out reason));
            Assert.IsFalse(DossierEngine.OptionAvailable(st, d.options[1], out reason));
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void NewGame_Is_Qipin_Mayor()
        {
            var st = State.NewGame("沈砚舟");
            Assert.AreEqual(5, st.saveVersion);
            Assert.AreEqual("七品·正厅", st.grade);
            Assert.IsTrue(st.grade.StartsWith("七品"));
            Assert.AreEqual(1985, st.player.birthYear);
            Assert.IsTrue(st.HasMark("fast_track"));
            Assert.IsTrue(st.examPassed && st.academyDone);
            Assert.AreEqual(100, st.compliance);
            Assert.Greater(st.efficiency, 0);
            Assert.IsTrue(st.married && st.hasChild);
        }
    }
}
