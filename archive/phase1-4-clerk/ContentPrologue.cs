using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 序章前置剧情链（依总设定推演）：
    /// 出身（2003-2021）→ 大学时代（2021-2025）→ 实践年动员与实践（2025-2026）→
    /// 公职资格考试与放榜（2026春）→ 夏天与进城（2026夏）→ 档案 → 报到前夜。
    /// 时间线口径：2000-2010互联网时代／2020疫情高效应对／2026 AI治理法·核聚变示范堆（编年史）。
    /// </summary>
    public static class ContentPrologue
    {
        public static void Register()
        {
            // ---------- 序·出身（2003—2021） ----------
            Flow.Register(new GameEvent
            {
                id = "pre0", type = "system", title = "序章 · 出身",
                paras = new List<string>
                {
                    "公元2003年，你出生了。这一年，帝国正处于互联网时代的浪潮中——皇家理工大学的机房里彻夜亮灯，网络正在进入寻常人家，而你只会哇哇大哭。",
                    "你在一个普通家庭长大。十八年里，你见过这个帝国最寻常的样子：免费的基础教育、按期到账的全民基本补贴、新闻里换了又换的五年规划，以及城墙下那条刷了很多年的标语——",
                    "“让每一代人，都有改变自己命运的机会。”",
                    "高考那年，你报了国立中央翰林院大学。放榜那晚，父亲把成绩单看了三遍，没说话；母亲在厨房里抹眼泪。皇家典籍馆的正统传人——帝国最高的人文社科学府——收下了他们的小孩。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="你家在关中·渭南的一个县城——父亲是县水利局科员",
                        effects=new Effects{ admin=2, setFlags=new List<string>{ "origin_guanzhong" }, logKind="人物", logText="出身：关中县城体制内家庭" },
                        result="小城体制人家的孩子，从小听惯了两个词：编制，和分寸。饭桌上的话题永远是“谁家孩子考出去了”。你对机关的想象，最早来自父亲的公文包——那种牛皮纸的味道，你一辈子都记得。" },
                    new EventOption{ label="你家在江南州·苏州——家里做丝绸外贸",
                        effects=new Effects{ comm=2, professional=1, setFlags=new List<string>{ "origin_jiangnan" }, logKind="人物", logText="出身：江南州外贸商人家庭" },
                        result="饭桌上谈的是订单、汇率和共荣圈里的客户。2008年金融海啸那年，家里的生意先缩后稳——你十岁，记住了父亲一句话：“国家稳，生意才稳。”商业家庭的孩子，早熟地懂得供需与人心。" },
                    new EventOption{ label="你家在华北州·唐山——钢厂三代",
                        effects=new Effects{ exec=2, setFlags=new List<string>{ "origin_huabei" }, logKind="人物", logText="出身：华北州产业工人家庭" },
                        result="祖父炼过铁，父亲管过车间。你见过产业转型的阵痛怎么落在一个具体的饭碗上——后来你在规划文本里读到“产业梯度转移”六个字，脑子里全是厂区公告栏前的人群。" },
                },
            });

            // ---------- 序·大学时代（2021—2025） ----------
            Flow.Register(new GameEvent
            {
                id = "pre1", type = "system", title = "序章 · 大学时代",
                paras = new List<string>
                {
                    "2021年秋，你拖着行李箱走进翰林院大学。校园里最老的那座楼叫“典籍馆”——前身是1700年太祖设的皇家典籍馆，比这座校园的所有树都老。",
                    "四年，说长不长。你在经济学系读书，也在这座城市里读书——长安的高新区在城西南日夜生长，政务区的城墙安安静静地趴在城中心，像一头收起爪子的老兽。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="四年埋头读书：图书馆、国奖、专业第一",
                        effects=new Effects{ professional=3, setFlags=new List<string>{ "uni_studious" }, logKind="系统", logText="大学四年：潜心学业" },
                        result="你在典籍馆的旧阅览室里坐穿了四个冬天。毕业时你是系里的专业第一，答辩那天教授说：“底子干净。”——这是翰林院大学能给出的最高表扬之一。" },
                    new EventOption{ label="四年纵横社团：辩论队、学生会、支教团",
                        effects=new Effects{ comm=2, political=1, setFlags=new List<string>{ "uni_social" }, logKind="系统", logText="大学四年：社团与实践" },
                        result="你在辩论队学会了把一句话拆成三种说法，在学生会学会了把十件事排成先后，在支教团第一次看见“政策”落到一个具体孩子身上是什么样子。" },
                    new EventOption{ label="跟导师做“数字政府”课题",
                        effects=new Effects{ professional=2, admin=1, setFlags=new List<string>{ "uni_research" }, logKind="系统", logText="大学四年：跟随导师研究数字政府" },
                        result="你的导师研究政务数字化。大二起你跟着跑数据、做访谈，第一次知道一份“红头文件”从起草到下发要经过多少双手——大学没教完的，实践年会接着教。" },
                },
            });

            // ---------- 序·最后一课（2025夏） ----------
            Flow.Register(new GameEvent
            {
                id = "pre_grand", type = "system", title = "序章 · 最后一课",
                paras = new List<string>
                {
                    "2025年6月，大四的最后一节课。辅导员抱着一摞表格走进来，教室里少见地安静。",
                    "“毕业之前，还有一年。”她说，“按《国民实践教育令》——1780年就立下的规矩——你们要去基层完成一年的国民实践。工科下厂，农科下乡，你们社科的，去政府、去乡里、去车间，自己选。”",
                    "“我不管你们将来当教授还是当干部，”她顿了顿，“这一年别糊弄。这个国家最大的一门课，在田野里。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="在选发表上郑重签下名字",
                        effects=new Effects{ political=1, morale=2, logKind="系统", logText="确定国民实践年去向" },
                        result="你在选发表上签了字。窗外的蝉声很响——那一年你22岁，还不知道这张表会把你带向哪里。" },
                },
            });

            // ---------- 序·实践年（2025—2026，三选一） ----------
            Flow.Register(new GameEvent
            {
                id = "p1", type = "system", title = "序章 · 实践年",
                paras = new List<string>
                {
                    "《国民实践教育令》——自1780年《国民实践教育令》起，帝国的大学生毕业前必须完成一年基层实践。干部选拔尤其看重基层治理经历。",
                    "你的实践年，是在哪里度过的？",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="经开区装备制造厂——工厂技术员见习",
                        effects=new Effects{ exec=3, logKind="人物", logText="实践年：经开区装备制造厂技术员见习",
                            setFlags=new List<string>{ "prologue_factory" } },
                        result="一年车间。你学会了看图纸、盯工序、跟三班倒的师傅们蹲在设备旁吃盒饭。机器不跟你讲客气——这段日子给你的回报是：把一件事真正做成的手感。" },
                    new EventOption{ label="秦岭北麓的青溪乡——农村基层治理见习",
                        effects=new Effects{ comm=2, political=1, logKind="人物", logText="实践年：秦岭北麓青溪乡基层治理见习",
                            setFlags=new List<string>{ "prologue_village" } }, // 老康的记忆钩子留给后续月份
                        result="一年乡土。走访、台账、防返贫监测、帮村里的合作社跑手续。街道办康主任教你第一课：“老乡不看文件，看水通不通、路平不平。”——有个叫老康的人，欠你一顿没吃的饭。" },
                    new EventOption{ label="区发改局——综合科跟班见习",
                        effects=new Effects{ admin=3, logKind="工作", logText="实践年：区发改局综合科跟班见习",
                            setFlags=new List<string>{ "prologue_district" } },
                        result="一年案头。你在区发改局综合科跟着抄了一年的材料，学会了公文格式、会议纪要和数据台账。带你的老科员说：你这起点，比大多数新人早了半年。" },
                },
            });

            // ---------- 序·公职资格考试（2026春） ----------
            Flow.Register(new GameEvent
            {
                id = "pre_exam", type = "system", title = "序章 · 资格考试",
                paras = new List<string>
                {
                    "2026年春，帝国公职资格考试（行政资格）。这是现代的科举：法律、行政、财政、历史、政治理论、基础数学、公文写作、地方治理——中央统一命题，各省组织考试。",
                    "笔试、面试、政审，一道不缺。放榜那天，你在名单上找到了自己：综合成绩，第九名。",
                    "按成绩排名，没有选调、没有例外——你被分配到：长安市发展和改革局，综合科。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把成绩单仔细折好，夹进笔记本",
                        effects=new Effects{ morale=3, professional=1, logKind="系统", logText="公职资格考试综合成绩第九名" },
                        result="第九名。你把成绩单的折痕压得很平——很多年后你还会记得这个下午：阳光、榜单、和一个即将开始的十年。" },
                },
            });

            // ---------- 序·夏天与进城（2026夏） ----------
            Flow.Register(new GameEvent
            {
                id = "pre_city", type = "system", title = "序章 · 夏天与进城",
                paras = new List<string>
                {
                    "那个夏天很长。毕业典礼上校长说：“翰林院教你们的是判断力，判断力要用在真问题上。”台下的你们不知道，真问题马上就来了。",
                    "整个七月，新闻里都是两件事：《AI治理法》草案三审在即；长安的下一代核聚变示范堆进入并网调试。时代在头顶轰隆隆地过，你在家里等一纸录用通知。",
                    "八月末，通知到了：9月1日，长安市发展和改革局，报到。母亲连夜收拾行李，父亲把一支用旧的钢笔塞进你的包：“写材料用的。”",
                    "高铁进站，城垣从楼群的缝隙里浮出来。你终于看清了这座城：南边是大学与科研的灯火，西南是高新区的塔吊，而城墙之内，是安静得近乎肃穆的中央政务区。",
                    "你的十年，要从城墙外的一间办公室开始了。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="安顿下来，准备报到",
                        effects=new Effects{ morale=4, logKind="系统", logText="抵达长安，准备入职报到" },
                        result="你在单位附近的小旅馆住下，把工牌要用的照片、资格证书、政审材料在桌上排成一列。长安的第一夜，你睡得很浅——梦里全是表格。" },
                },
            });

            // ---------- 档案（承前启后的小结） ----------
            Flow.Register(new GameEvent
            {
                id = "p0", type = "system", title = "序章 · 档案",
                paras = new List<string>
                {
                    "把前情写进档案，只需要几行字——",
                    "{你}，2003年生，23岁。国立中央翰林院大学·经济学；一年基层实践（国民实践年）；帝国公职资格考试·行政资格，综合成绩全市第九。",
                    "分配单位：长安市发展和改革局综合科。职级：吏三，科员。试用期：一年。",
                    "档案很薄，人生很长。按成绩排名，没有选调、没有关系、没有例外——从这个秋天起，你要在这座帝国的首都，一格一格地走完属于你的阶梯。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="翻开档案，走向第一个工作日",
                        effects=new Effects{ },
                        result="档案合上了。窗外，长安的秋天刚刚开始。" },
                },
            });

            // ---------- 报到前夜 ----------
            Flow.Register(new GameEvent
            {
                id = "p2", type = "system", title = "序章 · 报到前夜",
                paras = new List<string>
                {
                    "8月31日夜。你把明天的东西检查了第三遍：报到证、身份证、资格证书、政审回执、照片八张。",
                    "父亲打来电话，只说了三句话：“早点睡。”“别迟到。”“多干活。”你一一应了。",
                    "关灯之前，你最后看了一眼手机日历——2026年9月1日，星期二。十年后你会明白，有些日子的分量，是后来才慢慢长出来的。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="睡觉。明天，八点半。",
                        effects=new Effects{ gotoWork=true, energy=5, logKind="系统", logText="启程赴长安市发展和改革局报到" },
                        result="——2026年9月1日，你的机关生涯开始了。" },
                },
            });
        }
    }
}
