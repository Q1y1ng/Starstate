using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>属性成长回归：防"五维全 100"复发（Attrs 默认值泄漏 + 共享效果表污染）。</summary>
    public class GrowthSmokeTest
    {
        [Test]
        public void Attrs_Defaults_Are_Zero()
        {
            // Attrs 也用作 WeekEndData.growth 增量容器——非零默认会把初始值当每周增量重复发放
            var a = new Attrs();
            Assert.AreEqual(0, a.professional);
            Assert.AreEqual(0, a.admin);
            Assert.AreEqual(0, a.exec);
            Assert.AreEqual(0, a.comm);
            Assert.AreEqual(0, a.political);
        }

        [Test]
        public void NewGame_Uses_Initial_Attrs()
        {
            var st = State.NewGame("测试");
            Assert.AreEqual(75, st.player.attrs.professional);
            Assert.AreEqual(35, st.player.attrs.admin);
            Assert.AreEqual(50, st.player.attrs.exec);
            Assert.AreEqual(55, st.player.attrs.comm);
            Assert.AreEqual(30, st.player.attrs.political);
        }

        [Test]
        public void WeekEnd_WorkFocus_Applies_Only_Its_Growth()
        {
            var st = State.NewGame("测试");
            st.phase = Phase.WeekEnd;
            st.week.focus = "work";
            st.weekEndData = new WeekEndData();   // growth 全零（修复点：未赋值字段不得携带初始值）
            st.weekEndData.growth.exec = 2;       // 复现 BeginWeekEnd 对工作周的赋值
            st.weekEndData.growth.admin = 1;
            int p0 = st.player.attrs.professional, c0 = st.player.attrs.comm, l0 = st.player.attrs.political;

            Flow.Choose(st, 0);

            Assert.AreEqual(p0, st.player.attrs.professional);   // 不得把初始值 75 当增量
            Assert.AreEqual(c0, st.player.attrs.comm);           // 不得 +55
            Assert.AreEqual(l0, st.player.attrs.political);      // 不得 +30
            Assert.AreEqual(50 + 2, st.player.attrs.exec);       // 工作周：执行 +2
            Assert.AreEqual(35 + 1, st.player.attrs.admin);      // 行政 +1
        }

        [Test]
        public void Choose_Does_Not_Mutate_Shared_Event_Effects()
        {
            var st = State.NewGame("测试");
            st.phase = Phase.Day;
            st.date = "2026-09-09"; // 周三：结算链不触发周末/月末
            var shared = new Effects
            {
                morale = 1,
                rel = new System.Collections.Generic.List<RelDelta> { new RelDelta { id = "zhou", evalv = 1 } },
            };
            var ev = new GameEvent
            {
                id = "_t_clone", type = "work", title = "克隆测试",
                paras = new System.Collections.Generic.List<string> { "x" },
                options = new System.Collections.Generic.List<EventOption>
                {
                    new EventOption { label = "a", effects = shared, result = "ok" },
                },
            };

            st.runtimeEvent = ev;
            Flow.Choose(st, 0);
            st.hasPending = false;                // 吞掉结果页
            st.runtimeEvent = ev;
            Flow.Choose(st, 0);                   // 同一事件再次执行

            // 共享效果表必须保持原样（老实现会在其上累积/改写）
            Assert.AreEqual(1, shared.morale);
            Assert.AreEqual(1, shared.rel.Count);
            Assert.AreEqual(2, Npcs.Get(st, "zhou").evalv);   // 每次执行各生效一次
        }
    }
}
