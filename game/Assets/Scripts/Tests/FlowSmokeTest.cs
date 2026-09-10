using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>冒烟测试：自动玩完序章＋2026年9月（永远选第一个选项），断言月末结算正确。</summary>
    public class FlowSmokeTest
    {
        [Test]
        public void Prologue_And_September_Completes()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("测试员");
            Flow.Begin(st);

            int steps = 0;
            while (string.CompareOrdinal(st.date, "2026-10-01") < 0 && steps < 8000)
            {
                var scene = Flow.CurrentScene(st);
                Assert.IsNotEmpty(scene.options, $"场景无选项：{scene.title}（{st.date}）");
                bool dayFfable = st.phase == Phase.Day && !st.hasPending
                    && string.IsNullOrEmpty(st.currentEvent)
                    && st.queue.Count == 0
                    && (st.runtimeEvent == null || st.runtimeEvent.id == "_generic_day" || st.runtimeEvent.id == "_gen_task");
                if (dayFfable) Flow.FastForward(st);
                else Flow.Choose(st, 0);
                steps++;
            }

            Assert.GreaterOrEqual(string.CompareOrdinal(st.date, "2026-10-01"), 0,
                $"应至少进入10月（实际 {st.date}）");
            Assert.IsTrue(st.grade.StartsWith("七品"), "应为七品市长");
            Assert.GreaterOrEqual(st.dossierLog.Count + st.tasks.Count, 3, "九月应有卷宗或任务档案");
            Assert.GreaterOrEqual(st.player.probationMonths, 1, "月结应推进1个月");
            Assert.AreNotEqual("2026-08", st.month.key, "月状态应离开开局月");
            Assert.IsNotNull(Npcs.Get(st, "cen"), "主席关系应已建立");
        }

        [Test]
        public void TaskCheck_Grades_Are_Bounded()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("检定员");
            Flow.Begin(st);
            // 强行走完序章（前置剧情共8个事件，每个至多两拍）
            int guard = 0;
            while (st.phase == Phase.Prologue && guard++ < 40) Flow.Choose(st, 0);
            Assert.AreEqual(Phase.WeekPlan, st.phase, "序章全部选择后应进入第一周计划");

            // 极端精力下的检定不越界
            for (int i = 0; i < 50; i++)
            {
                st.player.energy = 100;
                string g = TaskCheckViaScene(st);
                CollectionAssert.Contains(new[] { "S", "A", "B", "C", "D" }, g);
            }
        }

        private static string TaskCheckViaScene(GameState st)
        {
            // 直接构造一个带检定的运行时事件（须处于 Day 阶段才走事件分支）；
            // 每次调用前重置 pending，保证可重复调用而不推进流程。
            var ev = new GameEvent
            {
                id = "_test_check", type = "work", title = "检定测试",
                options = new System.Collections.Generic.List<EventOption>
                {
                    new EventOption
                    {
                        label = "执行",
                        check = new Check { main = "admin" },
                        effects = new Effects { task = new TaskRecord { title = "检定测试任务" } },
                        result = "{grade}"
                    }
                }
            };
            st.phase = Phase.Day;
            st.queue.Clear();
            st.currentEvent = null;
            st.hasPending = false;
            st.pendingParas.Clear();
            int before = st.tasks.Count;
            st.runtimeEvent = ev;
            Flow.Choose(st, 0);
            st.hasPending = false; // 吞掉结果
            Assert.Greater(st.tasks.Count, before, "带检定的事件应产生一条任务档案");
            var rec = st.tasks[st.tasks.Count - 1];
            st.tasks.RemoveAt(st.tasks.Count - 1);
            st.week.tasks.Remove(rec);
            st.month.tasks.Remove(rec);
            return rec.grade;
        }
    }
}
