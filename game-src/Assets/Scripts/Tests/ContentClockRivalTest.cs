using System.Collections.Generic;
using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>补丁十三：常驻时钟内容 + 竞争者里程碑链。</summary>
    public class ContentClockRivalTest
    {
        private static GameState ToDay()
        {
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
            ContentRegistry.RegisterAll();
            var st = ToDay();
            // 直接调度到 11/1
            st.date = "2027-10-31";
            st.phase = Phase.Day;
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.queue.Clear();

            int guard = 0;
            bool sawSprint = false;
            while (guard++ < 15)
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
            Assert.IsTrue(sawSprint, "11 月应出现考核冲刺季事件");
            Assert.IsTrue(st.clocks.Exists(c => c.id == "clk_eval"), "选择后应建立考核冲刺时钟");
            Assert.IsTrue(st.HasMark("eval_sprint_taken"), "接台账选项应写叙事标记");
        }

        [Test]
        public void Inspection_Start_Creates_Threat_Clock()
        {
            ContentRegistry.RegisterAll();
            var st = ToDay();
            st.date = "2028-09-09";
            st.phase = Phase.Day;
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.queue.Clear();

            int guard = 0;
            bool saw = false;
            while (guard++ < 12)
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
            Assert.IsTrue(saw, "2028-09-10 应出现巡视组事件");
            var clk = st.clocks.Find(c => c.id == "clk_xuncha");
            Assert.IsNotNull(clk, "应建立巡视威胁时钟");
            Assert.AreEqual("threat", clk.kind);
        }

        [Test]
        public void Rival_Milestones_Are_Registered()
        {
            ContentRegistry.RegisterAll();
            // 关键 id 必须在注册表（十年回归会路过它们的日期）
            string[] ids =
            {
                "rv_xu_report", "rv_xu_dinner", "rv_xu_projoffice", "rv_xu_merit", "rv_xu_exam",
                "rv_su_seconded", "rv_su_vice", "rv_he_vice", "rv_he_wedding",
                "clk_eval_sprint_start", "clk_xuncha_start", "clk_proj_kick",
            };
            foreach (var id in ids)
            {
                Assert.IsTrue(Flow.IsRegistered(id), "缺少事件注册：" + id);
            }
        }

        [Test]
        public void Xufly_Report_Event_Presents_Options()
        {
            ContentRegistry.RegisterAll();
            var st = ToDay();
            st.date = "2027-03-13";
            st.phase = Phase.Day;
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.queue.Clear();

            int guard = 0;
            bool saw = false;
            while (guard++ < 10)
            {
                var scene = Flow.CurrentScene(st);
                if (scene.title == "深夜的办公楼")
                {
                    saw = true;
                    Assert.GreaterOrEqual(scene.options.Count, 2, "许飞事件应有分支选项");
                    Flow.Choose(st, 0);
                    Assert.IsTrue(st.HasMark("rival_xu_watch") || st.HasMark("rival_xu_ignore") || st.HasMark("rival_xu_friend"));
                    break;
                }
                Flow.Choose(st, 0);
            }
            Assert.IsTrue(saw, "2027-03-14 应触发许飞报告事件");
        }
    }
}
