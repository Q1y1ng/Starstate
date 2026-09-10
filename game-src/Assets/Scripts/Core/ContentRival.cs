using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 同批竞争者里程碑链：把 RivalState.progress 从“仪表数字”推进为可感知的剧情。
    /// 许飞为主线，苏晴/何斌为支线；节点与 ContentRegistry.Years.rival 口径对齐。
    /// </summary>
    public static class ContentRival
    {
        public static void Register()
        {
            RegisterXuChain();
            RegisterSuChain();
            RegisterHeChain();
            RegisterPressureBeats();
        }

        // ---------------- 许飞：主竞争者 ----------------

        private static void RegisterXuChain()
        {
            // 2027：被抽去写政府工作报告产业部分
            Flow.Register(new GameEvent
            {
                id = "rv_xu_report",
                type = "person",
                title = "深夜的办公楼",
                when = new When { date = "2027-03-14" },
                paras = new List<string>
                {
                    "听说许飞被抽去写政府工作报告的产业部分了。他朋友圈发了一张深夜的办公楼，定位是市府招待所。",
                    "同批群安静了三分钟，然后炸出一串“牛”。你盯着那张照片看了很久——玻璃幕墙里，有一格灯是他的。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把他的提纲要点记进自己的笔记",
                        effects = new Effects
                        {
                            energy = -3, professional = 1, morale = -1,
                            setMarks = new List<string> { "rival_xu_watch" },
                            logKind = "人物", logText = "关注许飞参与政府工作报告起草",
                        },
                        result = "笔记写完，你发现自己并没有嫉妒到发疯——只是清楚了一件事：赛道已经不一样宽了。",
                    },
                    new EventOption
                        {
                        label = "放下手机，继续改自己的材料",
                        effects = new Effects
                        {
                            energy = -2, morale = 1,
                            setMarks = new List<string> { "rival_xu_ignore" },
                        },
                        result = "有些差距不是靠刷朋友圈抹平的。你关掉屏幕，改到了第九稿。",
                    },
                    new EventOption
                    {
                        label = "私聊他：产业那块有什么需要帮忙的",
                        effects = new Effects
                        {
                            energy = -4, comm = 1, morale = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = 2, trust = 1, memo = "报告起草期主动问过要不要帮忙" } },
                            setMarks = new List<string> { "rival_xu_friend" },
                        },
                        result = "他回得很快：“谢了，暂时不用——不过回头请你喝一杯。”同批之间，竞争和交情可以并行，只要心眼不坏。",
                    },
                },
            });

            // 2029：何斌升副科后的同批聚餐（许飞段）——放在 He 链里也行，这里做许飞表态
            Flow.Register(new GameEvent
            {
                id = "rv_xu_dinner",
                type = "person",
                title = "那顿饭之后",
                when = new When { date = "2029-06-22" },
                paras = new List<string>
                {
                    "何斌升副科的饭局散场。许飞在停车场多站了一会儿，烟没点着。",
                    "“三年了。”他像是对你说，又像是对自己说，“有人上去了，有人还在写材料。你说，咱们这种人图什么？”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "“图把事做成。位置是结果，不是目标。”",
                        effects = new Effects
                        {
                            morale = 2, political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", trust = 2, memo = "停车场那晚：把事做成" } },
                            setMarks = new List<string> { "rival_xu_doer" },
                        },
                        result = "他笑了一下，把烟收回盒里。“你跟别人不一样。”这句话，后来你从他嘴里再没听过第二次。",
                    },
                    new EventOption
                    {
                        label = "“图走得稳。风口上的事，轮不轮得到我们另说。”",
                        effects = new Effects
                        {
                            stress = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = 1 } },
                            setMarks = new List<string> { "rival_xu_steady" },
                        },
                        result = "他点点头，没接话。你们各自回家。那年夏天，机关里开始有人把“稳”说成一种缺点。",
                    },
                    new EventOption
                    {
                        label = "“我不知道。但我不会停下来问这种问题。”",
                        effects = new Effects
                        {
                            energy = -1, stress = 2, exec = 1,
                            setMarks = new List<string> { "rival_xu_hard" },
                        },
                        result = "他盯着你看了两秒，然后笑了：“行，狠人。”有些同批关系，是从真话开始变质的。",
                    },
                },
            });

            // 2031：许飞调重点项目办
            Flow.Register(new GameEvent
            {
                id = "rv_xu_projoffice",
                type = "person",
                title = "重点项目办的人事风",
                when = new When { date = "2031-04-09" },
                paras = new List<string>
                {
                    "任雪梅在走廊里叫住你，压低声音：“许飞调重点项目办了，下周报到。你知道就行。”",
                    "重点项目办是硬骨头，也是镀金炉。同批的格局，又一次被人事科的铅字轻轻改写。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "恭喜他，并问清楚他手头项目的节点",
                        effects = new Effects
                        {
                            energy = -3, comm = 1, professional = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = 2, memo = "调岗时当面恭喜并了解项目" } },
                            setMarks = new List<string> { "rival_xu_close" },
                        },
                        result = "他说得兴奋，你听得认真。竞争者最危险的时候，不是他升上去，而是你对他的世界一无所知。",
                    },
                    new EventOption
                    {
                        label = "在心里把自己的路线再画一遍",
                        effects = new Effects
                        {
                            energy = -2, admin = 1, morale = 1,
                            setMarks = new List<string> { "rival_xu_focus" },
                        },
                        result = "别人的路是别人的。你在笔记本上写下下季度要攻的三件事，把页角折了起来。",
                    },
                },
            });

            // 2033：许飞三等功
            Flow.Register(new GameEvent
            {
                id = "rv_xu_merit",
                type = "person",
                title = "三等功公示",
                when = new When { date = "2033-09-26" },
                paras = new List<string>
                {
                    "局公示栏贴出三等功名单，许飞的名字在第二行。",
                    "同批群里又是一串“恭喜”。你想起 2027 年那张深夜办公楼的照片——六年，他把一格灯，走成了一枚奖章。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "真诚地给他发一条消息",
                        effects = new Effects
                        {
                            morale = 2, comm = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", trust = 1, memo = "三等功时发了真诚的祝贺" } },
                        },
                        result = "他回了一个抱拳表情，后面跟了句：“也快轮到你了。”你不确定这是客套还是预言。",
                    },
                    new EventOption
                    {
                        label = "把公示栏前那口气，咽进下一份材料",
                        effects = new Effects
                        {
                            energy = -4, stress = 3, professional = 2,
                            setMarks = new List<string> { "rival_xu_pushed" },
                        },
                        result = "当晚你把一份积压材料改到了发表水准。嫉妒可以是燃料，只要你记得控制火候。",
                    },
                    new EventOption
                    {
                        label = "对比自己的档案，默默补短板",
                        effects = new Effects
                        {
                            energy = -3, admin = 1, political = 1,
                        },
                        result = "你发现自己缺的不是能力，是被看见的节点。有些短板，要主动去够。",
                    },
                },
            });

            // 2034：许飞报考州级转官考试
            Flow.Register(new GameEvent
            {
                id = "rv_xu_exam",
                type = "person",
                title = "转官考试报名表",
                when = new When { date = "2034-03-04" },
                paras = new List<string>
                {
                    "许飞在食堂跟你说他报了州级转官考试。他把报名回执折了两折，像收起一张船票。",
                    "“咱们这一批，到了该分流的年纪。”他说这话时没看你看他，而是在看窗外的梧桐。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "“我也在准备。考场上见。”",
                        effects = new Effects
                        {
                            stress = 2, political = 1, morale = 1,
                            setMarks = new List<string> { "rival_xu_exam_rival" },
                            rel = new List<RelDelta> { new RelDelta { id = "xu", trust = 1, evalv = 1, memo = "转官考试：考场见" } },
                        },
                        result = "他终于看了你一眼，那一眼里有意外，也有棋逢对手的痛快。“好，考场见。”",
                    },
                    new EventOption
                    {
                        label = "“祝你上岸。我走我自己的路。”",
                        effects = new Effects
                        {
                            morale = 1,
                            setMarks = new List<string> { "rival_xu_exam_bless" },
                        },
                        result = "分流不是背叛。有人过河，有人修桥，有人守渡口——都是渡人。",
                    },
                    new EventOption
                    {
                        label = "问他要复习笔记",
                        effects = new Effects
                        {
                            energy = -2, professional = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = 1, memo = "索要转官复习笔记" } },
                            setMarks = new List<string> { "rival_xu_exam_notes" },
                        },
                        result = "他第二天真把笔记拍给你了，页边写满批注。对手给的资料，往往比朋友的更硬。",
                    },
                },
            });
        }

        // ---------------- 苏晴：踏实线 ----------------

        private static void RegisterSuChain()
        {
            Flow.Register(new GameEvent
            {
                id = "rv_su_seconded",
                type = "person",
                title = "苏晴借调市府办",
                when = new When { date = "2030-05-12" },
                paras = new List<string>
                {
                    "投资科给苏晴开了个简单的欢送会。瓜子花生，纸杯红茶。她要借调去市府办，暂定一年。",
                    "“档案是会跟人一辈子的。”她走之前只跟你说了一句这个。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "请她把市府办的材料规范寄一份回来",
                        effects = new Effects
                        {
                            energy = -2, admin = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "su", trust = 2, memo = "要了市府办材料规范" } },
                            setMarks = new List<string> { "rival_su_bond" },
                        },
                        result = "一周后你收到一份扫描件，页眉写着“仅供参考”。你把它打印出来，放进了自己的方法库。",
                    },
                    new EventOption
                    {
                        label = "祝她顺利，不多打听",
                        effects = new Effects
                        {
                            morale = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "su", familiar = 1 } },
                        },
                        result = "有些同批关系，保持恰当距离反而走得更远。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rv_su_vice",
                type = "person",
                title = "苏晴提了副科",
                when = new When { date = "2033-03-16" },
                paras = new List<string>
                {
                    "苏晴在市府办提了副科。消息传回来时，综合科正在对一份台账。",
                    "赵姐感慨：“踏实人有踏实人的福。”周衡之没抬头，只说了一句：“把自己的活干好。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "对照她的路径，检查自己缺哪一环",
                        effects = new Effects
                        {
                            energy = -3, admin = 2, political = 1,
                            setMarks = new List<string> { "rival_su_mirror" },
                        },
                        result = "你缺的不是努力，是“被更高平台验证过”的那一步。这件事，得自己去够。",
                    },
                    new EventOption
                    {
                        label = "发条消息恭喜，顺便约顿饭",
                        effects = new Effects
                        {
                            energy = -2, comm = 1, morale = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "su", familiar = 2, trust = 1, memo = "提副科后约饭" } },
                        },
                        result = "饭桌上她说了句掏心窝的：“别看谁先上去，看谁摔不下来。”",
                    },
                },
            });
        }

        // ---------------- 何斌：圆滑线 ----------------

        private static void RegisterHeChain()
        {
            Flow.Register(new GameEvent
            {
                id = "rv_he_vice",
                type = "person",
                title = "何斌升副科",
                when = new When { date = "2029-06-18" },
                paras = new List<string>
                {
                    "何斌升了副科，请全批人吃饭。他敬酒时说：“咱们这批人，该轮着往前走了。”",
                    "桌上有人起哄，有人沉默。你注意到许飞把杯子攥得很紧。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "跟着敬一杯，记下他的晋升材料写法",
                        effects = new Effects
                        {
                            energy = -2, political = 1, admin = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "he", familiar = 2 } },
                            setMarks = new List<string> { "rival_he_watch" },
                        },
                        result = "你发现他的晋升材料里，每一条成绩都有“市领导批示/兄弟处室好评”作证。会干活，也要会证明自己干了活。",
                    },
                    new EventOption
                    {
                        label = "安静吃完，不多说话",
                        effects = new Effects
                        {
                            morale = -1, stress = 1,
                        },
                        result = "饭局散场。你骑车回家，想的是自己下个月的节点——别人的喜酒，喝多了会醉自己的节奏。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rv_he_wedding",
                type = "person",
                title = "何斌的婚礼",
                when = new When { date = "2032-10-02" },
                paras = new List<string>
                {
                    "同批里第一个结婚的是何斌。婚礼上他把“新郎”两个字说得像“正科”——开玩笑的，但大家都笑了。",
                    "你随了份子，看见许飞也来了，西装笔挺，像来述职。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "敬酒时聊起同批这些年的分化",
                        effects = new Effects
                        {
                            comm = 2, morale = 1,
                            rel = new List<RelDelta>
                            {
                                new RelDelta { id = "he", trust = 1 },
                                new RelDelta { id = "xu", familiar = 1 },
                                new RelDelta { id = "su", familiar = 1 },
                            },
                            setMarks = new List<string> { "rival_cohort_dinner" },
                        },
                        result = "酒过三巡，有人提副科，有人还在科员，有人准备离开。一张餐桌坐得下十年，坐不散的是各自的选择。",
                    },
                    new EventOption
                    {
                        label = "礼到人到，早走一步",
                        effects = new Effects
                        {
                            energy = 2, stress = -1,
                        },
                        result = "你把份子钱塞进红包就走了。成年人的社交，完成比完美重要。",
                    },
                },
            });
        }

        // ---------------- 压力对照：人事季前的同批风声（动态，可重复） ----------------

        private static void RegisterPressureBeats()
        {
            Flow.Register(new GameEvent
            {
                id = "rv_pressure",
                type = "person",
                title = "同批的风声",
                when = new When
                {
                    md = "09-05", fromYear = 2028,
                },
                paras = new List<string>
                {
                    "人事季前夕，同批的动静又起来了。有人材料被抽借，有人名字出现在考察预告的边角。",
                    "你看了眼自己的档案袋——厚度是够的，亮点还差一截。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把最亮的三件事重新写进年度总结",
                        effects = new Effects
                        {
                            energy = -5, admin = 1, political = 1,
                            setMarks = new List<string> { "rival_polish_sum" },
                        },
                        result = "总结不是流水账，是个人品牌的年检。你把三件事改成了“可被引用的表述”。",
                    },
                    new EventOption
                    {
                        label = "按兵不动，等组织谈话",
                        effects = new Effects
                        {
                            stress = 2,
                        },
                        result = "你告诉自己：该来的会来。焦虑像办公室的绿萝，不浇水也会自己长。",
                    },
                },
            });

            // 竞争者进度很高时的对照事件（flag 由年结/人事触发也可；这里用 md 重复池）
            Flow.Register(new GameEvent
            {
                id = "rv_gap_felt",
                type = "person",
                title = "差距感",
                when = new When
                {
                    randomP = 0.07, weight = 18, maxFires = 5, cooldownDays = 120, weekdaysOnly = true,
                },
                paras = new List<string>
                {
                    "食堂里听见有人拿你和许飞比：“一个材料硬，一个路子野。”",
                    "你端着餐盘站了两秒——被比较本身不可怕，可怕的是你发现自己在意。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "听完就散，不往心里去",
                        effects = new Effects
                        {
                            stress = -1, morale = 1,
                        },
                        result = "机关里的比较是背景音。你把它调小了。",
                    },
                    new EventOption
                    {
                        label = "今晚加练一项短板",
                        effects = new Effects
                        {
                            energy = -6, stress = 2, professional = 1, exec = 1,
                        },
                        result = "短板不会因为在意就变长，只会因为练习变长。",
                    },
                },
            });
        }
    }
}
