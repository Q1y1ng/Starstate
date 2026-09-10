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
        }
    }
}
