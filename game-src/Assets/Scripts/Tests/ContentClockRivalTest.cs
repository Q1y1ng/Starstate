using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>市长层常驻时钟＋同批竞争（Phase 5 适配）。</summary>
    public class ContentClockRivalTest
    {
        private static GameState ToDay()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("时钟测试员");
            Flow.Begin(st);
            int guard = 0;
            while (st.phase == Phase.Prologue && guard++ < 40) Flow.Choose(st, 0);
            if (st.phase == Phase.WeekPlan) Flow.Choose(st, 0);
            return st;
        }

        [Test]
        public void Eval_Sprint_Start_Creates_Clock()
        {
            var st = ToDay();
            st.date = "2026-11-02";
            st.phase = Phase.Day;
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.queue.Clear();
            st.fired.Remove("clk_eval_sprint_start");
            // 直接入队（md 事件由 CollectDue 触发；单测跳过日推进）
            st.queue.Add("clk_eval_sprint_start");

            int guard = 0;
            bool sawSprint = false;
            while (guard++ < 10)
            {
                var scene = Flow.CurrentScene(st);
                if (scene.title == "考核冲刺季")
                {
                    sawSprint = true;
                    Flow.Choose(st, 0);
                    break;
                }
                Flow.Choose(st, 0);
            }
            Assert.IsTrue(sawSprint, "应出现考核冲刺季事件");
            Assert.IsTrue(st.clocks.Exists(c => c.id == "clk_eval"), "选择后应建立考核冲刺时钟");
            Assert.IsTrue(st.HasMark("eval_sprint_taken"), "亲自盯台账选项应写叙事标记");
        }

        [Test]
        public void Inspection_Start_Creates_Threat_Clock()
        {
            var st = ToDay();
            st.date = "2027-09-11";
            st.phase = Phase.Day;
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.queue.Clear();
            st.fired.Remove("clk_xuncha_start");
            st.fired.Remove("clk_xuncha_push");
            st.queue.Add("clk_xuncha_start");

            int guard = 0;
            bool saw = false;
            while (guard++ < 10)
            {
                var scene = Flow.CurrentScene(st);
                if (scene.title == "巡视组要来了")
                {
                    saw = true;
                    Flow.Choose(st, 0);
                    break;
                }
                Flow.Choose(st, 0);
            }
            Assert.IsTrue(saw, "应出现巡视组事件");
            var clk = st.clocks.Find(c => c.id == "clk_xuncha");
            Assert.IsNotNull(clk, "应建立巡视威胁时钟");
            Assert.AreEqual("threat", clk.kind);
        }

        [Test]
        public void Rival_Progress_Grows_Over_Weeks()
        {
            var st = ToDay();
            int p0 = st.rival.progress;
            int guard = 0;
            while (guard++ < 40 && st.phase != Phase.Ending)
            {
                bool dayFfable = st.phase == Phase.Day && !st.hasPending
                    && string.IsNullOrEmpty(st.currentEvent)
                    && st.queue.Count == 0
                    && (st.runtimeEvent == null || st.runtimeEvent.id == "_generic_day" || st.runtimeEvent.id == "_gen_task");
                if (dayFfable) Flow.FastForward(st);
                else Flow.Choose(st, 0);
            }
            Assert.Greater(st.rival.progress, p0, "同批竞争者势头应随周增长");
        }
    }
}
