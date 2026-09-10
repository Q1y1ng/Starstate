using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 常驻时钟内容：把 Clocks 引擎接上真实剧情节点。
    /// 三类：年度考核冲刺（机会）／巡视组倒计时（威胁）／专项 deadline（机会）。
    /// 满格走 onFullEventId；推进靠 clockOps（选项/事件效果）与 dailyRate。
    /// </summary>
    public static class ContentClocks
    {
        public static void Register()
        {
            RegisterEvalSprint();
            RegisterInspection();
            RegisterProjectDeadline();
        }

        // ---------------- 年度考核冲刺（每年 11/1 建立，1/15 sys_annual_eval 前满格可提前拿分） ----------------

        private static void RegisterEvalSprint()
        {
            Flow.Register(new GameEvent
            {
                id = "clk_eval_sprint_start",
                type = "work",
                title = "考核冲刺季",
                when = new When { md = "11-01", fromYear = 2027 },
                paras = new List<string>
                {
                    "十一月的综合科进入冲刺：个人总结、处室台账、评优推荐材料，三线并进。",
                    "周衡之把日历上 1 月 15 日的考核节点圈了红：“冲刺这六十天，决定你全年档案的颜色。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "立军令状：把冲刺台账接过来",
                        effects = new Effects
                        {
                            energy = -6,
                            admin = 1,
                            setMarks = new List<string> { "eval_sprint_taken" },
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_eval", label = "考核冲刺", max = 10, kind = "opportunity",
                                    onFull = "clk_eval_full", dailyRate = 1, delta = 2,
                                },
                            },
                            logKind = "工作", logText = "主动接下年度考核冲刺台账",
                        },
                        result = "你把四类材料分色建档。冲刺仪表亮了起来——接下来每一天都是进度。",
                    },
                    new EventOption
                    {
                        label = "按分工推进，不额外揽活",
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
                        result = "你守好自己那份。仪表也亮了，只是走得慢些——机关里，不揽活也是一种节奏。",
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
                    "考核冲刺仪表满格。你把全部材料按“可核、可查、可述”三关过了一遍，提前两周交卷。",
                    "人事科的同志翻到你这摞时停了两秒：“这本可以当样本。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "记下这次提前量",
                        effects = new Effects
                        {
                            admin = 1, political = 1, morale = 3, reputation = 1,
                            setMarks = new List<string> { "eval_sprint_done" },
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_eval", remove = true } },
                            rel = new List<RelDelta> { new RelDelta { id = "ren", evalv = 1, memo = "考核材料提前交卷，可当样本" } },
                        },
                        result = "提前清零的仪表从侧栏消失了，你心里那根弦却松了半格。年度考核那天，你会想起今天。",
                    },
                },
            });

            // 冲刺期可主动加班推进（每月可重复，冷却 20 天）
            Flow.Register(new GameEvent
            {
                id = "clk_eval_push",
                type = "work",
                title = "夜里加一班",
                when = new When
                {
                    randomP = 0.08, weight = 28, maxFires = 6, cooldownDays = 20,
                    requireMarks = new[] { "eval_sprint_taken" },
                },
                paras = new List<string>
                {
                    "下班后你留下核对考核附件。走廊灯次第熄灭，只剩你这排和打印机的呼吸声。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把这份附件磨到无懈可击",
                        effects = new Effects
                        {
                            energy = -8, stress = 2, admin = 1,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp { id = "clk_eval", delta = 2 },
                            },
                        },
                        result="多推了两格。付出是实的，进度条不会骗人。",
                    },
                    new EventOption
                    {
                        label = "差不多就收工，别熬垮",
                        effects = new Effects
                        {
                            energy = -2, stress = -2,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp { id = "clk_eval", delta = 1 },
                            },
                        },
                        result = "你关灯走人。冲刺是长跑，不是百米。",
                    },
                },
            });
        }

        // ---------------- 巡视组倒计时（2028 / 2031 / 2034 秋启动，威胁时钟） ----------------

        private static void RegisterInspection()
        {
            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_start",
                type = "oversight",
                title = "巡视组要来了",
                when = new When { date = "2028-09-10" },
                paras = new List<string>
                {
                    "局办转发通知：州委巡视组下月进驻本市，重点看项目审批与数据质量。",
                    "综合科被点名配合材料调阅。马副局长在走廊里只说了一句：“别让巡视组在咱们口子上捡到东西。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把近五年材料过一遍底账",
                        effects = new Effects
                        {
                            energy = -6, stress = 3,
                            setMarks = new List<string> { "xuncha_prep" },
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_xuncha", label = "巡视组进驻", max = 14, kind = "threat",
                                    onFull = "clk_xuncha_full", dailyRate = 1, delta = 1,
                                },
                            },
                            logKind = "监察", logText = "开始配合巡视组材料底账核查",
                        },
                        result = "倒计时开始。你知道自己经得起查——但“经得起查”也要花时间证明。",
                    },
                    new EventOption
                    {
                        label = "照常干活，按调阅单再准备",
                        effects = new Effects
                        {
                            stress = 1,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_xuncha", label = "巡视组进驻", max = 14, kind = "threat",
                                    onFull = "clk_xuncha_full", dailyRate = 1, delta = 2,
                                },
                            },
                        },
                        result = "仪表还是亮了。威胁不会因为你不看日历就消失。",
                    },
                },
            });

            // 后续两轮巡视（每年错开，可重复叙事）
            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_start_b",
                type = "oversight",
                title = "巡视“回头看”",
                when = new When { date = "2031-10-08" },
                paras = new List<string>
                {
                    "巡视“回头看”进驻。通知比上一次薄，要求比上一次细——重点看上次反馈的整改闭环。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把整改台账再对一遍口径",
                        effects = new Effects
                        {
                            energy = -5, stress = 2,
                            setMarks = new List<string> { "xuncha_prep_b" },
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_xuncha_b", label = "巡视回头看", max = 10, kind = "threat",
                                    onFull = "clk_xuncha_full_b", dailyRate = 1,
                                },
                            },
                        },
                        result = "回头看的仪表开始走。有过一次经验的人，不会把巡视当成偶发事件。",
                    },
                    new EventOption
                    {
                        label = "让业务口先报，综合科只做汇总",
                        effects = new Effects
                        {
                            stress = 1, comm = 1,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_xuncha_b", label = "巡视回头看", max = 10, kind = "threat",
                                    onFull = "clk_xuncha_full_b", dailyRate = 1, delta = 1,
                                },
                            },
                        },
                        result = "汇总也是责任。仪表照样走。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_start_c",
                type = "oversight",
                title = "专项巡察预通知",
                when = new When { date = "2034-09-18" },
                paras = new List<string>
                {
                    "专项巡察预通知：聚焦“数字政绩”与基层报表负担。你所在的综合科正好卡在数据与材料的咽喉上。",
                    "这次没人说“别捡到东西”，大家都在心里过自己的账。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "主动做一份自查清单交给科长",
                        effects = new Effects
                        {
                            energy = -6, political = 2, stress = 2,
                            setMarks = new List<string> { "xuncha_prep_c" },
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_xuncha_c", label = "专项巡察", max = 12, kind = "threat",
                                    onFull = "clk_xuncha_full_c", dailyRate = 1, delta = 1,
                                },
                            },
                            rel = new List<RelDelta> { new RelDelta { id = "zhou", trust = 1, memo = "巡察前主动自查" } },
                        },
                        result = "周衡之看完清单，把“责任边界”那页折了个角。倒计时开始。",
                    },
                    new EventOption
                    {
                        label = "等正式方案，不抢跑",
                        effects = new Effects
                        {
                            stress = 1,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_xuncha_c", label = "专项巡察", max = 12, kind = "threat",
                                    onFull = "clk_xuncha_full_c", dailyRate = 1, delta = 2,
                                },
                            },
                        },
                        result = "不抢跑是稳妥，也是把时间交出去。仪表走得更快了。",
                    },
                },
            });

            // 巡视推进：随机池（有 prep 标记时可降低威胁）
            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_push",
                type = "oversight",
                title = "调阅单",
                when = new When { randomP = 0.1, weight = 32, maxFires = 5, cooldownDays = 25 },
                paras = new List<string>
                {
                    "巡视组调阅单来了：要近三年项目审批链条和会议纪要的对应关系。",
                    "这不是走过场——每一页都可能被问到经办人和日期。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "逐条对齐，宁可多附说明",
                        check = new Check { main = "admin", bonus = 0.05f },
                        effects = new Effects
                        {
                            energy = -7, admin = 1,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp { id = "clk_xuncha", delta = -2 },
                                new ClockOp { id = "clk_xuncha_b", delta = -2 },
                                new ClockOp { id = "clk_xuncha_c", delta = -2 },
                            },
                        },
                        result = "{grade}。说明附得厚，追问就薄了。",
                    },
                    new EventOption
                    {
                        label = "按标准格式打包上报",
                        effects = new Effects
                        {
                            energy = -4,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp { id = "clk_xuncha", delta = -1 },
                                new ClockOp { id = "clk_xuncha_b", delta = -1 },
                                new ClockOp { id = "clk_xuncha_c", delta = -1 },
                            },
                        },
                        result = "标准动作完成。威胁仪表缓了一格。",
                    },
                },
            });

            // 满格：巡视谈话（三次文案不同）
            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_full",
                type = "oversight",
                title = "巡视谈话",
                when = null,
                paras = new List<string>
                {
                    "巡视组请你到谈话室。桌上一杯白水，一支录音笔，一份你自己经手过的项目目录。",
                    "“请从你负责的第一项说起。”语气平和，问题却刀刀见骨——日期、经办、口径、谁批的。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "如实陈述，能记的日期都报出来",
                        effects = new Effects
                        {
                            stress = 3, political = 2, morale = 2,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_xuncha", remove = true } },
                            setMarks = new List<string> { "xuncha_talked_clean" },
                        },
                        result = "谈话结束。你后背是潮的，但档案是干的。经得起问，本身就是一种政治资本。",
                    },
                    new EventOption
                    {
                        label = "谨慎回忆，不确定的说“需再核”",
                        effects = new Effects
                        {
                            stress = 4, political = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_xuncha", remove = true } },
                            setMarks = new List<string> { "xuncha_talked_careful" },
                        },
                        result = "“需再核”说了三次。谨慎不是错，但对方记下了你的谨慎。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_full_b",
                type = "oversight",
                title = "回头看反馈",
                when = null,
                paras = new List<string>
                {
                    "回头看反馈会。上次指出的问题，这次要见台账与销号单。",
                    "你准备的那摞整改材料被传到主桌，没有人表扬，也没有人挑刺——这在巡视语境里，接近满分。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把销号单归档，写进工作笔记",
                        effects = new Effects
                        {
                            admin = 1, morale = 3,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_xuncha_b", remove = true } },
                            setMarks = new List<string> { "xuncha_b_clean" },
                        },
                        result = "仪表熄灭。你知道这种平静有多贵。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "clk_xuncha_full_c",
                type = "oversight",
                title = "专项巡察核实",
                when = null,
                paras = new List<string>
                {
                    "专项巡察核实环节。有人问：这些报表数字，基层是不是被“赶”出来的？",
                    "问题不在你一个人身上，但回答会记在你个人的谈话记录里。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "讲清流程与减负措施，不护短",
                        effects = new Effects
                        {
                            stress = 2, political = 3, reputation = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_xuncha_c", remove = true } },
                            setMarks = new List<string> { "xuncha_c_clean" },
                        },
                        result = "你既没当替罪羊，也没当橡皮图章。这是老机关的窄门。",
                    },
                    new EventOption
                    {
                        label = "只陈述事实，不评价机制",
                        effects = new Effects
                        {
                            stress = 3, political = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_xuncha_c", remove = true } },
                            setMarks = new List<string> { "xuncha_c_quiet" },
                        },
                        result = "事实很干净，评价留白。有人觉得你稳，有人觉得你滑——两种评价都会进耳朵。",
                    },
                },
            });
        }

        // ---------------- 专项 deadline（随机启动，机会时钟） ----------------

        private static void RegisterProjectDeadline()
        {
            Flow.Register(new GameEvent
            {
                id = "clk_proj_kick",
                type = "work",
                title = "专项材料压过来",
                when = new When { randomP = 0.12, weight = 30, maxFires = 4, cooldownDays = 90, weekdaysOnly = true },
                paras = new List<string>
                {
                    "科里接到市府办急件：一份跨部门专项材料，四周内要出初稿，再报州里备案。",
                    "周衡之扫了一圈，目光停在你身上：“你主笔。要人给人，要口径找林晚。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "接下：立一份四周作战图",
                        effects = new Effects
                        {
                            energy = -8, stress = 2, exec = 1,
                            setMarks = new List<string> { "proj_deadline_on" },
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_proj", label = "专项初稿", max = 12, kind = "opportunity",
                                    onFull = "clk_proj_full", dailyRate = 1, delta = 1,
                                },
                            },
                            rel = new List<RelDelta> { new RelDelta { id = "zhou", trust = 1, memo = "接下市府办专项主笔" } },
                            task = new TaskRecord { title = "市府办专项材料", note = "四周初稿", signature = "主笔" },
                        },
                        result = "作战图贴在隔板上。十二格仪表亮起——四周，刚好。",
                    },
                    new EventOption
                    {
                        label = "推一半给业务口，自己做汇总",
                        effects = new Effects
                        {
                            energy = -5, comm = 1, stress = 1,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp
                                {
                                    id = "clk_proj", label = "专项初稿", max = 12, kind = "opportunity",
                                    onFull = "clk_proj_full", dailyRate = 1, delta = 2,
                                },
                            },
                        },
                        result = "分工清楚，压力也分了。仪表亮着，节奏略紧。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "clk_proj_full",
                type = "work",
                title = "专项初稿杀青",
                when = null,
                paras = new List<string>
                {
                    "专项初稿在第十二格清零。你把终稿打印三份：科长、分管、市府办各一份。",
                    "林晚路过时看了一眼目录结构：“条理是对的。数字再抠一遍，就交得出去。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按意见抠完最后一遍数字",
                        effects = new Effects
                        {
                            energy = -4, professional = 1, reputation = 2, polCapital = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_proj", remove = true } },
                            setMarks = new List<string> { "proj_done" },
                            rel = new List<RelDelta>
                            {
                                new RelDelta { id = "zhou", evalv = 1 },
                                new RelDelta { id = "lin", familiar = 1, evalv = 1 },
                            },
                        },
                        result = "交出去的那天没有掌声。但你把作战图从隔板上撕下来时，心里是满的。",
                    },
                    new EventOption
                    {
                        label = "现在就交，不过度打磨",
                        effects = new Effects
                        {
                            reputation = 1, morale = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_proj", remove = true } },
                            setMarks = new List<string> { "proj_done_fast" },
                        },
                        result = "先交再改是机关生存智慧。至少，deadline 死在了你前面。",
                    },
                },
            });

            // 专项进行中的加急件（可推进 deadline）
            Flow.Register(new GameEvent
            {
                id = "clk_proj_mid",
                type = "work",
                title = "市府办催了一次",
                when = new When
                {
                    randomP = 0.15, weight = 40, maxFires = 3, cooldownDays = 14,
                    requireMarks = new[] { "proj_deadline_on" },
                },
                paras = new List<string>
                {
                    "市府办来电话催进度，说州里节点提前了三天。",
                    "你看了眼作战图：还剩五格。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "今晚把中段写完",
                        effects = new Effects
                        {
                            energy = -10, stress = 3, exec = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_proj", delta = 3 } },
                        },
                        result = "键盘敲到十一点。进度条往前蹦了三格，眼睛里全是血丝。",
                    },
                    new EventOption
                    {
                        label = "要林晚帮审提纲，再动手",
                        effects = new Effects
                        {
                            energy = -5, comm = 1,
                            clockOps = new List<ClockOp> { new ClockOp { id = "clk_proj", delta = 2 } },
                            rel = new List<RelDelta> { new RelDelta { id = "lin", familiar = 1 } },
                        },
                        result = "提纲先过目，后面省两晚。协作不是示弱，是算账。",
                    },
                },
            });
        }
    }
}
