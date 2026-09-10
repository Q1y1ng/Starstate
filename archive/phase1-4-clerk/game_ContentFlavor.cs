using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>生活/社会/机关日常随机事件池（Phase 3 扩容）。均为一次性低概率事件，选项温和（Q5-03）。</summary>
    public static class ContentFlavor
    {
        public static void Register()
        {
            // ---------- 引擎动态事件占位（NpcTick 触发，Flow.BuildNpcInitiative 动态构建） ----------
            Flow.Register(new GameEvent { id = "npc_initiative", type = "person", dynamic = true, title = "" });

            // ---------- 机关日常 ----------
            Flow.Register(new GameEvent
            {
                id = "flv_canteen", type = "society", title = "食堂的学问",
                when = new When { randomP = 0.05 },
                paras = new List<string>
                {
                    "食堂十二点开饭，十一点五十就有人下楼——机关的钟表有两套，一套在墙上，一套在胃里。",
                    "你端着餐盘找座，科里老同志们占着固定的“老位置”，新人们聚在窗边。今天两个区都剩了座。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="坐到老同志那桌去",
                        effects=new Effects{ comm=1, political=1, rel=new List<RelDelta>{ new RelDelta{ id="zhao", familiar=2 } } },
                        result="老同志们聊的是八十年代的粮票和今年的养老金。你插不上话，但听懂了什么叫“单位的记忆”。" },
                    new EventOption{ label="和新人们拼桌",
                        effects=new Effects{ morale=2, rel=new List<RelDelta>{ new RelDelta{ id="xu", familiar=2 }, new RelDelta{ id="su", familiar=2 } } },
                        result="一桌子的吐槽和抱负。年轻真好——你把这句感慨就着米饭咽了下去。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_printer", type = "work", title = "打印机危机",
                when = new When { randomP = 0.05 },
                paras = new List<string>
                {
                    "四楼唯一的彩色打印机在今天下午集体罢工——而全市现场会的材料四点要装订。",
                    "行政科的电话被打爆了。你路过时被赵姐一把拉住：“你会修！上！”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="拆开机器，取出卡了半天的纸",
                        effects=new Effects{ exec=1, morale=2, energy=-3, rel=new List<RelDelta>{ new RelDelta{ id="zhao", trust=1, familiar=3 } } },
                        result="一张对折的A4卡在滚轴深处。你用镊子和耐心把它请了出来——四点整，材料准时装订。英雄不问出处，但会问会不会修打印机。" },
                    new EventOption{ label="建议去楼下文印店应急",
                        effects=new Effects{ exec=1, energy=-2 },
                        result="你联系了巷口文印店，老板骑电驴亲自送货上楼。从此全局多了个应急预案，叫做“巷口老李”。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_studyclass", type = "politics", title = "机关学习会",
                when = new When { randomP = 0.04 },
                paras = new List<string>
                {
                    "周五下午的全局学习会。今天学的文件和你的业务严丝合缝——领导点了几处“要结合我市实际”的地方，笔尖快的人已经在纸上列提纲了。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="结合业务写三条落实建议",
                        effects=new Effects{ political=2, professional=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } } },
                        result="你把三条建议交给周衡之。一周后，其中一条出现在了全局的工作要点里——学习会的正确打开方式。" },
                    new EventOption{ label="认真听，记好笔记",
                        effects=new Effects{ political=1 },
                        result="你记了四页笔记。学习这件事，一分投入一分货，利息按年头算。" },
                },
            });

            // ---------- 城市生活 ----------
            Flow.Register(new GameEvent
            {
                id = "flv_subway", type = "society", title = "早高峰的地铁二号线",
                when = new When { randomP = 0.05 },
                paras = new List<string>
                {
                    "早高峰的二号线上，人和人之间的距离被压缩到物理极限。你旁边的大爷拎着鸟笼，鸟笼里两只画眉淡定得像退休领导。",
                    "到站广播响起，你被人流稳稳地“运”出了车门——长安的早高峰，教会每个新市民什么叫秩序中的柔性。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="挤着，但今天心情不错",
                        effects=new Effects{ morale=1, comm=1 },
                        result="出站时你帮一位抱孩子的妈妈抬了下婴儿车。她连声道谢——一天的运气，从早上就存好了。" },
                    new EventOption{ label="决定以后骑车通勤",
                        effects=new Effects{ energy=3, morale=2 },
                        result="你办了共享单车季卡。从此早高峰的风是你的，迟到风险也是你的。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_rain", type = "society", title = "暴雨中的长安",
                when = new When { randomP = 0.04 },
                paras = new List<string>
                {
                    "一场暴雨把晚高峰泡在了水里。有的路段积水过膝，朋友圈里一半在怨，一半在晒“北海观潮”。",
                    "单位的应急通知随即就到：相关处室今晚值守，重点盯易涝点。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="留下参与值守",
                        effects=new Effects{ energy=-8, reputation=1, exec=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } } },
                        result="你在办公室盯着易涝点的实时照片，把三条积水信息转给了应急局。深夜雨停，城市的排水系统赢了——多数时候，没人注意这种赢。" },
                    new EventOption{ label="冒雨回家，路上帮邻居推了辆车",
                        effects=new Effects{ morale=3, comm=1, energy=-4 },
                        result="你和小区的邻居们把一辆熄火的面包车推过积水段。素不相识的人因为一场雨成了战友。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_marathon", type = "society", title = "马拉松封路",
                when = new When { randomP = 0.035 },
                paras = new List<string>
                {
                    "长安国际马拉松开跑，主城区限行。你站在警戒线外，看着三万名跑者从城门楼下涌过——古城与现代，在同一条赛道上。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="给跑者加油",
                        effects=new Effects{ morale=3 },
                        result="你冲一位白发跑者竖了大拇指，他回你一个标准的军礼——后来你才知道那是位退休的老将军。" },
                    new EventOption{ label="想着“明年我也跑”",
                        effects=new Effects{ energy=2, morale=2, setFlags=new List<string>{ "flv_want_marathon" },
                            setMarks=new List<string>{ "marathon_m" } },
                        result="你在手机上记下：明年报名。flag立在了长安的春天里。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_market", type = "society", title = "菜市场经济学",
                when = new When { randomP = 0.04 },
                paras = new List<string>
                {
                    "周末早市。你听着摊主们报价：猪肉涨了两块，桃子因丰产跌了——CPI的微观形态，活生生摆在眼前。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="跟摊主聊聊今年的行情",
                        effects=new Effects{ comm=1, professional=1 },
                        result="卖桃的大姐比你懂供需：“去年价好，今年种的人多，就贱了。”你在心里补了一句：这就是教科书的第一章。" },
                    new EventOption{ label="只买菜，不调研",
                        effects=new Effects{ morale=2 },
                        result="你拎着菜回家，决定周末就当个普通市民——分析狂魔也需要休息。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_health", type = "society", title = "年度体检报告",
                when = new When { randomP = 0.035 },
                paras = new List<string>
                {
                    "单位组织的年度体检报告出了。你的各项指标大体正常，除了“轻度脂肪肝倾向”和“建议减少久坐”两条灰字。",
                    "同办公室的赵姐看了一眼：“三十岁的身体，五十岁的腰椎——来，跟我学工间操。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="开始每天工间操＋走楼梯",
                        effects=new Effects{ energy=5, morale=2, stress=-4 },
                        result="你坚持了两周，爬六楼不喘了。健康是1，职级是后面的0——体检报告每年都这么提醒你。" },
                    new EventOption{ label="看完收起来，继续久坐",
                        effects=new Effects{ stress=2 },
                        result="你把报告塞进抽屉最底层，和“明天开始锻炼”的决心放在一起。" },
                },
            });

            // ---------- 人物互动 ----------
            Flow.Register(new GameEvent
            {
                id = "flv_hebin_info", type = "person", title = "何斌的“情报”",
                when = new When { randomP = 0.045 },
                paras = new List<string>
                {
                    "何斌神神秘秘地凑过来：“重磅——听说局里要来个新副局长，上面空降的。”你问哪听来的，他神秘一笑：“你要信就信，不信就当我没说。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="听完存档，不传播",
                        effects=new Effects{ political=2 },
                        result="一周后消息应验了一半：确实有新人，但是内部提任。你给何斌的消息打了七十五分——他的情报和天气预报一个准头，但都值得参考。" },
                    new EventOption{ label="“你的消息渠道要守法。”",
                        effects=new Effects{ political=1, rel=new List<RelDelta>{ new RelDelta{ id="he", trust=1 } } },
                        result="何斌嘿嘿一笑：“放心，我这是公开渠道＋逻辑推理。”你发现他最大的本事是把八卦做成分析题。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_lin_book", type = "person", title = "林晚的书单",
                when = new When { randomP = 0.04 },
                paras = new List<string>
                {
                    "林晚借给你一本书——《措辞的艺术》，扉页写着1989年的赠言，书页黄得像档案。",
                    "“看完写读后感，”她说，“不超过两百字。写不出来，说明没看懂。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="一周后交了篇一百八十字的读后感",
                        effects=new Effects{ professional=2, rel=new List<RelDelta>{ new RelDelta{ id="lin", trust=3, familiar=2, memo="读后感写得透" } } },
                        result="她看完只回了一句：“可以。”——从林晚嘴里说出的“可以”，比多数人的“很好”值钱。" },
                    new EventOption{ label="把书供在工位上天天看封面",
                        effects=new Effects{ morale=1 },
                        result="书在工位立了一个月。某天林晚路过瞥了一眼：“书立得挺直，翻开了吗？”你决定今晚就翻开第一章。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_zhou_tea", type = "person", title = "周科长的一杯茶",
                when = new When { randomP = 0.04 },
                paras = new List<string>
                {
                    "周衡之难得地给你泡了杯茶——他最好的泾阳茯砖。茶汤红亮。他说：“尝尝，比你的工资先到嘴边。”",
                    "你受宠若惊。上次他给人泡茶，还是三年前林晚评上副科的时候。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="喝完茶，等他开口",
                        effects=new Effects{ political=2 },
                        result="茶过三巡他才说话：“最近科里的材料，你担了大头。我年纪大了，往后有些事你要多担些。”——这杯茶，是交棒的味道。" },
                    new EventOption{ label="先谢谢，把话题引到工作上",
                        effects=new Effects{ comm=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=2 } } },
                        result="你们就着茶把科里的台账理了一遍。茶凉了，事清了——周科长的茶从来不是白喝的。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_tong_ball", type = "person", title = "和童远打球",
                when = new When { randomP = 0.04 },
                paras = new List<string>
                {
                    "统计局和发改局的篮球友谊赛，你和童远分在对位。他个子不高，出手却贼——三节打完，你们俩各得十二分。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="全力打完，赛后撸串",
                        effects=new Effects{ energy=-10, morale=4, comm=1, rel=new List<RelDelta>{ new RelDelta{ id="tong", trust=3, familiar=4 } } },
                        result="比分定格在58:58——两边的计分员都饿了。烤串摊上你们约定：球场上不认兄弟，数据里不认朋友，球场下和数据外，都是兄弟。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_rencall", type = "person", title = "任姐的提醒",
                when = new When { randomP = 0.035 },
                paras = new List<string>
                {
                    "人事科任姐把你叫到走廊尽头，声音压得很低：“你的档案里缺一张实践年的鉴定表复印件，这周内补齐。”",
                    "“别小看一张纸，”她说，“档案上的窟窿，要用十年去补。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="当天补齐，并顺手整理了自己的全部档案",
                        effects=new Effects{ admin=1, political=1, rel=new List<RelDelta>{ new RelDelta{ id="ren", trust=3, familiar=2, memo="档案意识强" } } },
                        result="你把档案按时间线自查了一遍，补齐了那张纸，还发现了两处信息更新遗漏。任姐难得地笑了：“孺子可教。”" },
                    new EventOption{ label="下周才想起来补",
                        effects=new Effects{ stress=2 },
                        result="任姐收下材料时看了眼日期，什么都没说。但你知道，她在心里给你记了一笔——人事科的账，最长久。" },
                },
            });

            // ---------- 职业感悟 ----------
            Flow.Register(new GameEvent
            {
                id = "flv_dream", type = "person", title = "深夜的自我对话",
                when = new When { randomP = 0.03 },
                paras = new List<string>
                {
                    "加班到深夜，整层楼只剩你。你盯着屏幕上自己的倒影，忽然想起入职第一天那个抱着一摞表格的自己。",
                    "“你后悔吗？”你问倒影。倒影没说话，只是把材料又翻过了一页。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="“不后悔。但我要走得更快。”",
                        effects=new Effects{ morale=3, exec=1, professional=1, setFlags=new List<string>{ "dream_ambition" },
                            setMarks=new List<string>{ "dream_m" } },
                        result="你把这句话写在了笔记本的最后一页。野心不可耻——可耻的是浪费了让它实现的机会。" },
                    new EventOption{ label="“不后悔。这就是我想要的生活。”",
                        effects=new Effects{ morale=4, stress=-3, political=1 },
                        result="你合上电脑，关灯，走进长安的夜色里。有些满足不需要向任何人证明——包括向自己。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "flv_greenrookie", type = "person", title = "又一批新人",
                when = new When { randomP = 0.035 },
                paras = new List<string>
                {
                    "局里来了新一批公务员。培训会上，一个年轻人举手问：“请问怎样才能快速成长？”台下的老科员们相视一笑。",
                    "散会后你路过综合科——你的老工位上，坐着一张崭新的面孔，正紧张地整理着工牌。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="像当年赵姐对你那样，请他吃顿食堂",
                        effects=new Effects{ comm=1, morale=3, rel=new List<RelDelta>{ new RelDelta{ id="zhao", familiar=2 } } },
                        result="你把《易错清单》和一句“不懂就问，别自己扛”一起给了他。轮回这件事，在机关是温暖的那一种。" },
                    new EventOption{ label="远远看一眼，继续赶路",
                        effects=new Effects{ political=1 },
                        result="你在走廊尽头回头看了一眼——每个人都曾是新人，每个人也都会成为“老同志”。时间的规矩，谁也绕不开。" },
                },
            });
        }
    }
}
