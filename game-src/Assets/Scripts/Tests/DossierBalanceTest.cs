using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>两把尺标定探针：漏查 vs 查全，合规/效率应朝预期方向动。</summary>
    public class DossierBalanceTest
    {
        static GameState Fresh()
        {
            DossierEngine.Clear();
            Rulebook.ClearRegistry();
            ContentRulebook.Register();
            ContentDossierM0.Register();
            ContentDossierY1.Register();
            var st = State.NewGame();
            Flow.Begin(st);
            return st;
        }

        static void ResolveAllPages(GameState st, bool doCheck, int optionIndex = 0)
        {
            var d = DossierEngine.Current(st);
            Assert.IsNotNull(d);
            if (doCheck)
            {
                for (int p = 1; p <= d.pages.Count; p++)
                {
                    int cur = st.activeDossier.page;
                    while (cur < p) { DossierEngine.TurnPage(st, +1); cur++; }
                    while (cur > p) { DossierEngine.TurnPage(st, -1); cur--; }
                    DossierEngine.CheckPage(st);
                }
            }
            var entry = DossierEngine.Resolve(st, optionIndex);
            Assert.IsNotNull(entry, "应能签批");
            st.hasPending = false;
            st.pendingParas.Clear();
            DossierEngine.CloseActive(st);
        }

        [Test]
        public void Missed_Issues_Drain_Compliance()
        {
            var st = Fresh();
            int c0 = st.compliance;
            DossierEngine.AssignWeek(st, new[] { "dz_t1" });
            // dz_t1 有雷；不核对直接照准
            Assert.IsTrue(DossierEngine.OpenNext(st));
            ResolveAllPages(st, doCheck: false);
            Assert.Less(st.compliance, c0, "漏查应扣合规");
            Assert.Greater(st.yearIntegrity, 0);
        }

        [Test]
        public void Full_Check_Preserves_Compliance()
        {
            var st = Fresh();
            int c0 = st.compliance;
            DossierEngine.AssignWeek(st, new[] { "dz_t1" });
            Assert.IsTrue(DossierEngine.OpenNext(st));
            // 查全后走「退回补正」（option 1，complianceDelta=+3），合规不应掉
            ResolveAllPages(st, doCheck: true, optionIndex: 1);
            Assert.GreaterOrEqual(st.compliance, c0, "查全+退回不应扣合规");
        }

        [Test]
        public void Overdue_Drains_Efficiency()
        {
            var st = Fresh();
            st.date = "2026-09-10"; // 晚于 dz_t1 deadline 09-04
            int e0 = st.efficiency;
            DossierEngine.AssignWeek(st, new[] { "dz_t1" });
            Assert.IsTrue(DossierEngine.OpenNext(st));
            ResolveAllPages(st, doCheck: true);
            Assert.Less(st.efficiency, e0, "逾期应扣效率");
        }

        [Test]
        public void Year_Imbalance_Keeps_Scores_In_Range()
        {
            var st = Fresh();
            // 连续 12 件：交替漏查/查全，分数应仍在 0-100 且未双双见底
            for (int i = 0; i < 12; i++)
            {
                st.activeDossier = null;
                DossierEngine.AssignWeek(st, new[] { i % 2 == 0 ? "dz_t1" : "dz_p_finance" });
                if (!DossierEngine.OpenNext(st)) continue;
                ResolveAllPages(st, doCheck: (i % 2 == 1));
            }
            Assert.GreaterOrEqual(st.compliance, 0);
            Assert.LessOrEqual(st.compliance, 100);
            Assert.GreaterOrEqual(st.efficiency, 0);
            Assert.LessOrEqual(st.efficiency, 100);
            // 纯漏查不会把合规打到 0（有下限与初始 100 缓冲）——至少不应为负逻辑错误
            Assert.Greater(st.compliance + st.efficiency, 20, "两把尺不应双双崩盘（12 件混合处置）");
        }
    }
}
