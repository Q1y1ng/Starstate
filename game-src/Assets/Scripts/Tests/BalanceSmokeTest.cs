using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>平衡探针：十年自动通关后检查关键数值是否落在合理区间（真实游玩粗校准）。</summary>
    public class BalanceSmokeTest
    {
        [Test]
        public void Ten_Year_Stats_Stay_Sane()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("平衡测试员");
            Flow.Begin(st);

            int guard = 0;
            int highStressDays = 0, emptyEnergyDays = 0, sampleDays = 0;
            while (st.phase != Phase.Ending && !st.resigned && !st.underInvestigation && guard++ < 200000)
            {
                var scene = Flow.CurrentScene(st);
                Assert.IsNotEmpty(scene.options);

                bool dayFfable = st.phase == Phase.Day && !st.hasPending
                    && string.IsNullOrEmpty(st.currentEvent)
                    && st.queue.Count == 0
                    && (st.runtimeEvent == null || st.runtimeEvent.id == "_generic_day" || st.runtimeEvent.id == "_gen_task");
                if (dayFfable)
                {
                    sampleDays++;
                    if (st.player.stress >= 90) highStressDays++;
                    if (st.player.energy <= 5) emptyEnergyDays++;
                    Flow.FastForward(st);
                }
                else Flow.Choose(st, 0);
            }

            Assert.IsTrue(st.phase == Phase.Ending || st.resigned || st.underInvestigation,
                $"应走向结局，停在 {st.date}（{st.phase}）");
            Assert.GreaterOrEqual(st.tasks.Count, 15, "任务档案过少");
            Assert.GreaterOrEqual(st.evals.Count, 8, "年度考核过少");

            // 压力/精力不应长期顶格或见底（阈值放宽，防随机波动误报）
            if (sampleDays > 80)
            {
                Assert.Less(highStressDays * 1f / sampleDays, 0.45f,
                    $"高压日占比过高：{highStressDays}/{sampleDays}");
                Assert.Less(emptyEnergyDays * 1f / sampleDays, 0.45f,
                    $"空精力日占比过高：{emptyEnergyDays}/{sampleDays}");
            }

            Assert.GreaterOrEqual(st.player.savings, 0, "积蓄不应为负");
            Assert.LessOrEqual(st.player.stress, 100);
            Assert.LessOrEqual(st.player.energy, 100);

            // 能力总和应有成长（初始约 25 满分制条）
            int attrSum = st.player.attrs.professional + st.player.attrs.admin + st.player.attrs.exec
                + st.player.attrs.comm + st.player.attrs.political;
            Assert.Greater(attrSum, 10, "十年后能力总和应有明显成长");

            UnityEngine.Debug.Log($"[BALANCE] 结局={st.phase} 职级={st.grade} 储蓄={st.player.savings} " +
                $"能力和={attrSum} 压力={st.player.stress} 士气={st.player.morale} " +
                $"高压日占比={(sampleDays == 0 ? 0 : highStressDays * 1f / sampleDays):P0} " +
                $"任务={st.tasks.Count} 考核={st.evals.Count} 竞争者={st.rival.progress}");
        }
    }
}
