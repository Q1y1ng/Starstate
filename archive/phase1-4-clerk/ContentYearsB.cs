using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>第二卷（2030—2032）：中层岁月。借调归位、吏一晋升、下沉挂职、AI政务与老友。</summary>
    public static class ContentYearsB
    {
        public static void Register()
        {
            // ---------- 2030 ----------
            Flow.Register(new GameEvent
            {
                id = "y30_census", type = "society", title = "人口普查发布日",
                when = new When { date = "2030-03-20" },
                paras = new List<string>
                {
                    "第七次全国人口普查数据发布。新闻发布会上念出的数字里，长安都市圈人口突破七千万——老龄化率也悄然爬升了一个百分点。",
                    "发布会上有记者问：“人口结构变了，规划怎么变？”全场看向发改系统的席位。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="会后连夜整理人口专题分析",
                        check=new Check{ main="professional", bonus=0.1f },
                        effects=new Effects{ energy=-12, professional=1, political=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } },
                            task=new TaskRecord{ title="人口结构变化影响分析", note="普查数据专题", signature="主笔" } },
                        result="你的分析结论只有一句话被上级画了线：“人口不是数字，是二十年后这座城市的年龄。”——这句话后来进了规划说明书的引言。{grade}。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y30_taskforce", type = "work", title = "专班的成果",
                when = new When { date = "2030-06-25" },
                paras = new List<string>
                {
                    "“十五五”规划编制专班阶段性成果汇报。两年间，你从数据员做到了产业篇主笔——规划文本里有一段是你的原话。",
                    "市领导批示：“编制规划的人，要经得起规划落地时的检验。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="在批示件前拍照留念",
                        effects=new Effects{ morale=5, reputation=2, professional=2 },
                        result="你给批示拍了照，设成了加班时才看的屏保。规划还有十年才到期——十年后检验今天的话，想想就让人笔下发抖，又发亮。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y30_shuqing", type = "work", title = "述职答辩",
                when = new When { date = "2030-12-14" },
                paras = new List<string>
                {
                    "局里试行“评优述职”：想争优秀等次的干部，上台八分钟，晒全年工作，接受评委问询。",
                    "轮到你时，评委席上坐着马副局长和人事科——你的十二分钟材料里，藏着这一年的全部心跳。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="用数据说话，少谈苦劳",
                        check=new Check{ main="professional", bonus=0.1f },
                        effects=new Effects{ energy=-10, comm=2, political=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="ma", evalv=2, familiar=3, memo="述职有干货" } } },
                        result="你没讲加班，只讲了三组数：抓了几项、落地几项、纠正几项。马副局长听完点头：“评优材料就照这个改。”{grade}。" },
                    new EventOption{ label="讲情怀，讲成长",
                        check=new Check{ main="comm", bonus=0.05f },
                        effects=new Effects{ energy=-8, comm=1 },
                        result="你讲得很动情，评委听得很安静。分数不低，但马副局长的批语一针见血：“情怀可以有，台账更要有。”{grade}。" },
                },
            });

            // ---------- 2031 ----------
            Flow.Register(new GameEvent
            {
                id = "y31_opening", type = "society", title = "新规划开局 · 集中开工",
                when = new When { date = "2031-04-10" },
                paras = new List<string>
                {
                    "新五年规划开局之年，全市重大项目集中开工。主会场的桩机同时启动，声音像大地的心跳。",
                    "你在会场负责核签项目清单——每一个数字背后，都是过去两年专班里的一个深夜。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="核签清单",
                        effects=new Effects{ morale=3, exec=2, reputation=1 },
                        result="清单核签完毕，桩声隆隆。规划从纸面走进大地的那一天，编制者有种隐秘的自豪——也有隐秘的紧张：落地时，是要对账的。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y31_mentor", type = "person", title = "你带的新人",
                when = new When { date = "2031-11-18" },
                paras = new List<string>
                {
                    "科里来了新人——一个戴眼镜的年轻人，像极了十年前的你：名校、认真、把“口径”两个字写在笔记本第一页。",
                    "周衡之把带教任务交给你：“当年赵姐怎么带你的，你就怎么带他。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把《易错清单》抄一份给他",
                        effects=new Effects{ comm=1, morale=3, rel=new List<RelDelta>{ new RelDelta{ id="zhao", familiar=2, memo="你的清单传下去了——传帮带" } } },
                        result="你把入职第一年整理的《易错清单》重新誊了一遍，添上这五年新踩的坑，交到他手里。传承这件事，就这样在纸页间完成了。" },
                    new EventOption{ label="让他自己先摔两个跟头",
                        effects=new Effects{ political=1 },
                        result="你忍住了。有些坑必须自己踩过才算数——你只在坑边插了面小旗：那页写着“此处曾有前人”。" },
                },
            });

            // ---------- 2032 ----------
            Flow.Register(new GameEvent
            {
                id = "y32_aigov", type = "politics", title = "AI政务助手上线",
                when = new When { date = "2032-06-20" },
                paras = new List<string>
                {
                    "AI政务助手覆盖全市机关：材料初稿、数据校验、会议纪要——效率飙升，焦虑也在飙升：“它会取代我们吗？”",
                    "法规处墙上挂着的还是那条铁律：“算法可以提供意见，不得代替法定权力主体作出最终政治决定。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把AI当学徒用，把责任留给自己",
                        effects=new Effects{ professional=2, political=2, energy=-4, admin=1,
                            task=new TaskRecord{ title="AI政务助手使用规范建议", note="人机边界", signature="主笔" } },
                        result="你起草了科里的AI使用规范：三查（数据、口径、依据）、一签（责任自负）。周衡之转发全科：“这才叫会用机器。”" },
                    new EventOption{ label="抵触新工具，坚持手写",
                        effects=new Effects{ professional=1, stress=3 },
                        result="你坚持了一周，手速跟不上时代了。最终你在AI初稿上改出了自己的版本——工具无罪，关键看谁在掌舵。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y32_tong", type = "person", title = "童远的十年",
                when = new When { date = "2032-11-11" },
                paras = new List<string>
                {
                    "统计局的童远提了副科。庆祝的方式照旧：一瓶汽水，一次加班后的深夜对数。",
                    "“还记得十年前你第一次来对口径吗？”他笑，“那时候你连‘规上规下’都问。”",
                    "“现在，”他把一份最新的基期修订文件推给你，“轮到我请教你问题了。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="“咱们谁也不许在数字上糊弄谁。”",
                        effects=new Effects{ morale=4, comm=1, rel=new List<RelDelta>{ new RelDelta{ id="tong", trust=3, familiar=4 } } },
                        result="你们碰了碰汽水瓶。十年里，你们对过的数据摞起来比人高——这种友谊，机关里叫“战友”。" },
                },
            });
        }
    }
}
