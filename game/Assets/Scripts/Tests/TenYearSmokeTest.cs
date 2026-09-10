using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>十年冒烟测试：全自动通关 2026-09 → 2036-08，验证晋升、考核、结局全链路。</summary>
    public class TenYearSmokeTest
    {
        [Test]
        public void Ten_Years_Completes_With_Evaluations()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("十年测试员");
            Flow.Begin(st);

            int guard = 0;
            while (st.phase != Phase.Ending && !st.resigned && !st.underInvestigation && guard++ < 200000)
            {
                var scene = Flow.CurrentScene(st);
                Assert.IsNotEmpty(scene.options, $"场景无选项：{scene.title}（{st.date}）");
                // 可推进判定与 GameApp.UpdateFfButton 一致：日常/任务可代选，剧情与动态抉择必须 Choose
                bool dayFfable = st.phase == Phase.Day && !st.hasPending
                    && string.IsNullOrEmpty(st.currentEvent)
                    && st.queue.Count == 0
                    && (st.runtimeEvent == null || st.runtimeEvent.id == "_generic_day" || st.runtimeEvent.id == "_gen_task");
                if (dayFfable)
                {
                    Flow.FastForward(st);   // 平日自动推进到下一个安排
                }
                else
                {
                    Flow.Choose(st, 0);     // 关键节点一律选第一个选项（稳妥路线）
                }
            }

            Assert.IsTrue(st.phase == Phase.Ending || st.resigned || st.underInvestigation,
                $"十年应走向结局，实际停在 {st.date}（{st.phase}）");
            Assert.GreaterOrEqual(st.evals.Count, 9, "十年应有9次以上年度考核记录");
            Assert.GreaterOrEqual(st.tasks.Count, 15, "十年应积累足够的任务档案");
            Assert.Less(st.player.savings, int.MaxValue - 100000, "财务不应溢出");

            // 年度考核记录应按年递增
            for (int i = 1; i < st.evals.Count; i++)
                Assert.Greater(st.evals[i].year, st.evals[i - 1].year, "考核年份应严格递增");
        }
    }
}
