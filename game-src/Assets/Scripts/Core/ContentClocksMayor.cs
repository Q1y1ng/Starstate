using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// Phase 5 市长层常驻时钟：年度考核冲刺（机会）／巡视组进驻（威胁）。
    /// 引擎同 Clocks；内容换到市政府案头与双尺考核。
    /// </summary>
    public static class ContentClocksMayor
    {
        public static void Register()
        {
            RegisterEvalSprint();
            RegisterInspection();
        }

        static void RegisterEvalSprint()
        {
            Flow.Register(new GameEvent
            {
                id = "clk_eval_sprint_start",
                type = "work",
                title = "考核冲刺季",
                when = new When { md = "11-01", fromYear = 2026 },
                paras = new List<string>
                {
                    "十一月，办公厅开始收年度考核材料：双尺台账、项目完成率、巡视整改销号率，三线并进。",
                    "周谨把 1 月 15 日的考核节点圈了红：“老板，冲刺这六十天，决定您档案的颜色——也决定六品名单上有没有大同。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "立军令状：亲自盯双尺台账",
                        effects = new Effects
                        {
                            energy = -6, admin = 1,
                            setMarks = new List<string> { "eval_sprint_taken" },
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_eval", label = "考核冲刺", max = 10, kind = "opportunity",
                                    onFull = "clk_eval_full", dailyRate = 1, delta = 2,
                                },
                            },
                            logKind = "工作", logText = "市长亲自盯年度考核双尺台账",
                        },
                        result = "你把合规、效率、项目三本台账分色建档。冲刺仪表亮了——接下来每一天都是进度。",
                    },
                    new EventOption
                    {
                        label = "按分工推进，办公厅汇总",
                        effects = new Effects
                        {
                            energy = -2, morale = 1,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_eval", label = "考核冲刺", max = 10, kind = "opportunity",
                                    onFull = "clk_eval_full", dailyRate = 1,
                                },
                            },
                        },
                        result = "周谨应了一声。仪表也亮了，只是走得慢些——不揽活也是一种节奏。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "clk_eval_full",
                type = "work",
                title = "冲刺台账提前清零",
                when = null,
                paras = new List<string>
                {
                    "考核冲刺仪表满格。双尺台账、销号率、省里交办完成率，三关都过了。",
                    "材料交上去的那天，韩清的秘书打来电话：“部长说，大同的表，不用返工。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把功劳记在班子账上",
                        effects = new Effects
                        {
                            morale = 3, political = 2, reputation = 1,
                            rel = new List<RelDelta>
                            {
                                new RelDelta { id = "han", evalv = 3, memo = "考核材料不用返工" },
                                new RelDelta { id = "shao", trust = 1, evalv = 2, memo = "冲刺功劳归班子" },
                            },
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_eval", remove = true } },
                            logKind = "工作", logText = "年度考核冲刺台账提前清零",
                        },
                        result = "邵志远在常务会上多看了你一眼。有些表扬不需要奖状——名单会替你说话。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "clk_eval_push",
                type = "work",
                title = "冲刺加一把",
                when = new When { requireMarks = new[] { "eval_sprint_taken" }, md = "12-10", fromYear = 2026 },
                paras = new List<string>
                {
                    "冲刺过半。周谨报：销号率 78%，省里交办还差三件。",
                    "要不要再压一压？",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "压：三件交办限时办结",
                        effects = new Effects
                        {
                            energy = -8, stress = 4, exec = 2,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_eval", delta = 2 } },
                            logKind = "工作", logText = "冲刺：省交办限时清零",
                        },
                        result = "三件在一周内结了。效率分在往上走——压力也是。",
                    },
                    new EventOption
                    {
                        label = "稳：按程序推进，不赶工",
                        effects = new Effects
                        {
                            morale = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_eval", delta = 1 } },
                            logKind = "工作", logText = "冲刺：按程序推进",
                        },
                        result = "慢一点，但每一笔都能翻。考核办的人说：大同的材料，厚度刚好。",
                    },
                },
            });
        }

        static void RegisterInspection()
        {
            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_start",
                type = "oversight",
                title = "巡视组要来了",
                when = new When { md = "09-10", fromYear = 2027 },
                paras = new List<string>
                {
                    "省委巡视组将于月底进驻，重点：重大决策程序、专项资金、政府债务与招商引资。",
                    "沈砚发来一条短信，只有四个字：“材料干净。”——干净是最好准备，也是唯一准备。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "全面自查：卷宗与合同逐件过",
                        effects = new Effects
                        {
                            energy = -8, stress = 5, professional = 1,
                            setMarks = new List<string> { "xuncha_selfcheck" },
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_xuncha", label = "巡视组进驻", max = 14, kind = "threat",
                                    onFull = "clk_xuncha_full", dailyRate = 1, delta = 2,
                                },
                            },
                            logKind = "监察", logText = "巡视前全面自查",
                        },
                        result = "自查清单列了六页。你圈了两处要补正的——圈的时候笔很重。",
                    },
                    new EventOption
                    {
                        label = "按台账准备，不过度加码",
                        effects = new Effects
                        {
                            energy = -3, stress = 2,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_xuncha", label = "巡视组进驻", max = 14, kind = "threat",
                                    onFull = "clk_xuncha_full", dailyRate = 1, delta = 1,
                                },
                            },
                        },
                        result = "台账是现成的。现成的东西经不经得起翻，取决于平时——不取决于这十四天。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_full",
                type = "oversight",
                title = "巡视反馈",
                when = null,
                paras = new List<string>
                {
                    "巡视反馈会。问题清单里有你签过的字，也有你没签过的程序。",
                    "沈砚念到第三条时顿了一下——那一条，和某份框架协议的附件页码对得上。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "全盘接受，立行立改",
                        effects = new Effects
                        {
                            stress = 6, political = 1,
                            integrity = new IntegrityRecord { tag = "巡视", note = "巡视反馈全盘接受并整改" },
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_xuncha", remove = true } },
                            setMarks = new List<string> { "xuncha_accepted" },
                            logKind = "监察", logText = "巡视反馈：立行立改",
                        },
                        result = "整改方案你亲自改了三稿。岑伯衡在常委会上说了一句：“态度是好的。”——在这间会议室，这四个字很重。",
                    },
                    new EventOption
                    {
                        label = "对个别条目提出说明",
                        effects = new Effects
                        {
                            stress = 4, comm = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_xuncha", remove = true } },
                            setMarks = new List<string> { "xuncha_explain" },
                            logKind = "监察", logText = "巡视反馈：部分说明",
                        },
                        result = "说明写了五页。巡视组收了，没当场表态。没表态，说明还在看。",
                    },
                },
            });

            // 巡视压力期：可主动推进/缓释
            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_push",
                type = "oversight",
                title = "进驻前的最后一次碰头",
                when = new When { md = "09-20", fromYear = 2027 },
                paras = new List<string>
                {
                    "进驻前三天。周谨问：要不要把两份“边补边报”的材料再压一压口径？",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按原始口径，不修饰",
                        effects = new Effects
                        {
                            stress = 3,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_xuncha", delta = -2 } },
                            logKind = "监察", logText = "巡视前：坚持原始口径",
                        },
                        result = "原始数不好看，但对得上。对得上，就是最好的修饰。",
                    },
                    new EventOption
                    {
                        label = "技术性统一表述",
                        effects = new Effects
                        {
                            stress = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_xuncha", delta = 1 } },
                            integrity = new IntegrityRecord { tag = "巡视配合", note = "进驻前技术性统一表述" },
                            logKind = "监察", logText = "巡视前：统一表述",
                        },
                        result = "表述统一了。时间戳没统一——时间戳从不说谎。",
                    },
                },
            });
        }
    }
}
