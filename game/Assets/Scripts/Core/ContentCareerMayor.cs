using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>Phase 5 系统动态事件：年度考核与十年结局（七品路径）。</summary>
    public static class ContentCareerMayor
    {
        public static void Register()
        {
            Flow.Register(new GameEvent
            {
                id = "sys_personnel", type = "person", title = "人事窗口", dynamic = true,
                // 七品满一届（2026-09 就任 → 2031 年满 5 年）之后每年 9 月开窗
                when = new When { md = "09-20", fromYear = 2031 },
                paras = new List<string>(),
                options = new List<EventOption> { new EventOption { label = "继续" } },
            });
            Flow.Register(new GameEvent
            {
                id = "sys_annual_eval", type = "politics", title = "年度考核", dynamic = true,
                when = new When { md = "01-15", fromYear = 2027 },
                paras = new List<string>(),
                options = new List<EventOption> { new EventOption { label = "继续" } },
            });
            Flow.Register(new GameEvent
            {
                id = "sys_ending", type = "system", title = "十年之约", dynamic = true,
                when = new When { date = "2036-08-27" },
                paras = new List<string>(),
                options = new List<EventOption> { new EventOption { label = "继续" } },
            });
            // —— M3：风险后果链。旧实现里 underInvestigation 只能由内容选项置位，没有任何系统入口，
            //    意味着“接受审查调查”这条结局事实上不可达。现由风险账本驱动。
            Flow.Register(new GameEvent
            {
                id = "sys_risk_talk", type = "politics", title = "谈话提醒", dynamic = true,
                when = new When { md = "11-20", fromYear = 2028, requireMarks = new[] { "risk_watch" } },   // 风险账本 ≥ 40 时由 RefreshRisk 挂上
                paras = new List<string>(),
                options = new List<EventOption> { new EventOption { label = "继续" } },
            });
            Flow.Register(new GameEvent
            {
                id = "sys_investigation", type = "politics", title = "立案审查", dynamic = true,
                when = new When { flag = "risk_investigation" },   // 风险 ≥ 65 且程序违规 ≥ 2
                paras = new List<string>(),
                options = new List<EventOption> { new EventOption { label = "继续" } },
            });
        }
    }
}
