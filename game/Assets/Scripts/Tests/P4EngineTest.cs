using System.Collections.Generic;
using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>Phase 4 叙事引擎测试：回响调度、加权池不枯竭、选项锁定、时钟满格、NPC 淡忘。</summary>
    public class P4EngineTest
    {
        private static GameState ToDay()
        {
            var st = State.NewGame("P4测试员");
            Flow.Begin(st);
            int guard = 0;
            while (st.phase == Phase.Prologue && guard++ < 40) Flow.Choose(st, 0);
            if (st.phase == Phase.WeekPlan) Flow.Choose(st, 0);   // 定重心 → 进入 Day
            return st;
        }

        [Test]
        public void Echo_Fires_When_Due()
        {
            ContentRegistry.RegisterAll();
            Flow.Register(new GameEvent
            {
                id = "t_echo_source", type = "work", title = "回响源头",
                when = new When { randomP = 0.1, weight = 50 },   // 加权池很快抽到
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "埋下种子",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "mark_source" },
                            echoes = new List<EchoSpec> { new EchoSpec { eventId = "t_echo_target", afterDays = 3 } },
                        },
                        result = "有些事当时没有回音。",
                    },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "t_echo_target", type = "work", title = "回响抵达",
                paras = new List<string> { "当初种下的因，今天结果了。" },
                options = new List<EventOption> { new EventOption { label = "接住", effects = new Effects { morale = 3 }, result = "" } },
            });

            var st = ToDay();
            bool seen = false;
            int guard = 0;
            while (guard++ < 80)
            {
                var scene = Flow.CurrentScene(st);
                if (scene.title == "回响源头")
                {
                    Flow.Choose(st, 0);
                    Assert.IsTrue(st.HasMark("mark_source"), "选择应写入叙事标记");
                    continue;
                }
                if (scene.title == "回响抵达") { seen = true; break; }
                Flow.Choose(st, 0);
            }
            Assert.IsTrue(seen, "回响事件应在数日后入队并被呈现");
        }

        [Test]
        public void Pool_Respects_MaxFires_And_Repeats()
        {
            ContentRegistry.RegisterAll();
            Flow.Register(new GameEvent
            {
                id = "t_pool_repeat", type = "society", title = "循环事件",
                when = new When { randomP = 0.1, weight = 60, maxFires = 3 },
                paras = new List<string> { "这件事又发生了。" },
                options = new List<EventOption> { new EventOption { label = "应付过去", effects = new Effects { morale = 1 }, result = "" } },
            });

            var st = ToDay();
            int seen = 0, guard = 0;
            while (guard++ < 400 && seen < 3)
            {
                if (st.phase == Phase.WeekPlan) Flow.Choose(st, 0);
                var scene = Flow.CurrentScene(st);
                if (scene.title == "循环事件") seen++;
                Flow.Choose(st, 0);
            }
            Assert.GreaterOrEqual(seen, 3, "重复触发应实际被呈现（池不枯竭）");
            Assert.GreaterOrEqual(st.FireCount("t_pool_repeat"), 3, "maxFires>0 的池事件应可重复触发");
        }

        [Test]
        public void Option_Lock_Gates_On_Mark_And_Relation()
        {
            ContentRegistry.RegisterAll();
            Flow.Register(new GameEvent
            {
                id = "t_lock_opt", type = "work", title = "门槛测试",
                when = new When { flag = "t_lock_flag" },
                paras = new List<string> { "有一件只有熟人才好办的事。" },
                options = new List<EventOption>
                {
                    new EventOption { label = "普通做法", effects = new Effects { exec = 1 }, result = "" },
                    new EventOption
                    {
                        label = "托关系加急",
                        when = new OptionWhen { relNpc = "zhou", minTrust = 5, mark = "t_lock_mark" },
                        effects = new Effects { morale = 2 },
                        result = "一个电话的事。",
                    },
                },
            });

            var st = ToDay();
            st.SetFlag("t_lock_flag", true);
            Npcs.Mod(st, new RelDelta { id = "zhou", trust = 10 });   // 信任够，但 mark 未立

            int guard = 0;
            while (guard++ < 30)
            {
                var scene = Flow.CurrentScene(st);
                if (scene.title == "门槛测试") break;
                Flow.Choose(st, 0);
            }
            var ev = Flow.CurrentScene(st);
            Assert.AreEqual(2, ev.options.Count, "锁定选项应可见");
            StringAssert.Contains("🔒", ev.options[1], "锁定选项应带锁定标记");
            Assert.IsNotNull(ev.optionLocks, "应提供锁定原因列表");

            int moraleBefore = st.player.morale;
            Flow.Choose(st, 1);   // mark 未立 → 不可点，应无效果
            Assert.AreEqual(moraleBefore, st.player.morale, "锁定选项不应生效");
            Assert.AreEqual("门槛测试", Flow.CurrentScene(st).title, "锁定选择后事件应仍在屏");

            st.Mark("t_lock_mark");
            Flow.Choose(st, 1);
            Assert.AreEqual(moraleBefore + 2, st.player.morale, "解锁后选择应正常生效");
        }

        [Test]
        public void Clock_Full_Triggers_Event()
        {
            ContentRegistry.RegisterAll();
            Flow.Register(new GameEvent
            {
                id = "t_clock_full", type = "work", title = "调研窗口开启",
                paras = new List<string> { "钟满了。" },
                options = new List<EventOption> { new EventOption { label = "抓住", effects = new Effects { reputation = 2 }, result = "" } },
            });

            var st = ToDay();
            st.clocks.Add(new ClockState { id = "t_clock", label = "专项调研", value = 4, max = 5, kind = "opportunity", onFullEventId = "t_clock_full", dailyRate = 1 });

            bool seen = false;
            int guard = 0;
            while (guard++ < 30)
            {
                var scene = Flow.CurrentScene(st);
                if (scene.title == "调研窗口开启") { seen = true; break; }
                Flow.Choose(st, 0);
            }
            Assert.IsTrue(seen, "时钟满格应触发事件");
            Assert.IsTrue(st.Fired("clockfull_t_clock"), "满格事件应只触发一次");
        }

        [Test]
        public void NpcTick_Decays_Stale_Relations()
        {
            var st = State.NewGame("淡忘员");
            st.date = "2026-09-01";
            var r = Npcs.Get(st, "zhou");
            r.familiar = 10;
            r.memories.Add(new MemoryEntry { date = "2026-07-20", text = "一个月前的事" });
            var r2 = Npcs.Get(st, "lin");
            r2.familiar = 10;
            r2.memories.Add(new MemoryEntry { date = "2026-08-30", text = "刚互动过" });

            NpcTick.WeeklyTick(st);

            Assert.AreEqual(9, r.familiar, "超过28天未互动应淡忘1点熟悉度");
            Assert.AreEqual(10, r2.familiar, "近期互动过的不应淡忘");
        }
    }
}
