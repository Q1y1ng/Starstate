using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// Phase 4 剧情链：把序章/基期的每一次选择变成数月乃至数年后的回响。
    /// 结构：md+requireMarks（或 date+flag）触发 → 选项写 marks → EchoSpec 延迟回响 → 分支兑现。
    /// 另含开局志向（p_ambition）与全部死 flag 的下游兑现。
    /// </summary>
    public static class ContentChains
    {
        public static void Register()
        {
            RegisterAmbition();
            RegisterOriginPayoffs();
            RegisterPracPayoffs();
            RegisterUniPayoffs();
            RegisterRevealPayoffs();
            RegisterLedgerChain();
            RegisterAiChain();
            RegisterHouseChain();
            RegisterFamilyChain();
            RegisterResignChain();
            RegisterSmallArcs();
        }

        // ---------------- 数据造假案（监察主线，2027冬 → 2028春 → 长影） ----------------

        private static void RegisterLedgerChain()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_ledger_a", type = "oversight", title = "十一月的台账",
                when = new When { md = "11-15", fromYear = 2027 },
                paras = new List<string>
                {
                    "年终数据汇总。隔壁口子的一位老熟人凑过来压低声音：“规上增速那栏，你把四季度基数再核核——调一调，全区都好看，又不是你一个部门的数。”",
                    "纸笔就在手边。改，只要一行；不改，只要摇头。十年后的档案，就在这一行和这一摇之间。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="摇头：口径是口径，人情是人情",
                        effects=new Effects{ stress=2, morale=2, political=1, setMarks=new List<string>{ "ledger_refused" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_ledger_b1", afterDays=90 } },
                            logKind="监察", logText="拒绝调改年终台账口径" },
                        result="他悻悻走了。你把这次谈话记在工作笔记上——日期、原话、在场者。留痕不是防他，是防自己将来想不起今天为什么摇头。" },
                    new EventOption{ label="照办：反正是“技术性调整”",
                        effects=new Effects{ stress=3, polCapital=2, exec=1, setMarks=new List<string>{ "ledger_complicit" },
                            integrity=new IntegrityRecord{ tag="数据口径", note="年终汇总未按原始基数调整，技术性修饰" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_ledger_b2", afterDays=90 } },
                            logKind="监察", logText="年终台账做了“技术性调整”" },
                        result="数字改完的那晚你睡得不算差——只是第二天路过公告栏时，脚步快了一点。这件事，你谁也没说。" },
                    new EventOption{ label="把这件事报给周衡之",
                        effects=new Effects{ stress=-1, political=1, setMarks=new List<string>{ "ledger_reported" },
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=2, evalv=1, memo="台账的事第一时间报了我" } },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_ledger_b3", afterDays=90 } } },
                        result="周衡之听完只说了两个字：“知道了。”然后你看见他拿起了内线电话。有些靠山不须表态——他接了电话，就都算了表态。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_ledger_b1", type = "oversight", title = "不点名的表扬",
                when = null,
                paras = new List<string>
                {
                    "年终数据质量通报会上，分管副局长念了一句话：“今年综合科的台账，经得起翻。”没点你的名。",
                    "但周衡之翻材料的手，在那一页停了两秒。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把这两秒收进心里",
                        effects=new Effects{ morale=3, admin=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } } },
                        result="会散了，雪没下。你摸了摸笔记本里那页谈话记录——有些坚持的回报是慢的，但它按时到了。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_ledger_b2", type = "oversight", title = "漂亮的总表",
                when = null,
                paras = new List<string>
                {
                    "年终汇总上报，数字很漂亮，通报表扬里有了你们口的姓名。那位老熟人特意路过你工位，挤了挤眼。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="笑着应付过去",
                        effects=new Effects{ morale=-1, stress=2 },
                        result="表扬是大家的，坑是你一个人的——从今天起，你多了一件永远不能说的事，和一个永远要对齐的口径。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_ledger_b3", type = "oversight", title = "“这事到我为止”",
                when = null,
                paras = new List<string>
                {
                    "那位老熟人被口子负责人找去谈了话。他再没来找过你——路过你工位时，脚步比从前规矩。",
                    "周衡之后来在科务会上说过一句：“口径上的事，科里解决不了的，第一时间报我。”没人知道这句话从哪儿来。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把这句科务会的话抄下来",
                        effects=new Effects{ morale=2, political=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=1 } } },
                        result="你抄的时候忽然明白：领导的意义，就是让“上报”两个字有地方落。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_ledger_c_refused", type = "oversight", title = "台账抽查",
                when = new When { md = "05-20", fromYear = 2028, requireMarks = new[] { "ledger_refused" } },
                paras = new List<string>
                {
                    "市审计局抽查四季度汇总数据，从原始基数一路核到上报口径。你的名字出现在三个签字栏里。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="配合抽查，一切以留痕为准",
                        effects=new Effects{ admin=1, morale=2, stress=-1,
                            commend=new CommendRecord{ text="年度数据抽查全程无差错（审计组反馈）" } },
                        result="抽查结论两个字：无差。审计组长临走时多问了一句：“当时基数的原始底稿在谁手里？”——在你手里，一直都在。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_ledger_c_complicit", type = "oversight", title = "差额问询",
                when = new When { md = "05-20", fromYear = 2028, requireMarks = new[] { "ledger_complicit" } },
                paras = new List<string>
                {
                    "审计抽查发现四季度增速与原始基数存在无法解释的差额，调阅清单直接开到了你的经手件。",
                    "谈话室的白炽灯很白。你的解释准备了三个版本，出口时只剩最短的那个。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="如实说明，把责任领下来",
                        effects=new Effects{ stress=6, morale=-2, political=1,
                            integrity=new IntegrityRecord{ tag="数据口径", note="审计问询中如实说明台账调整事项" },
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=1, memo="问询时没有推责" } } },
                        result="谈话记录按了手印。走出大楼时天正在下雨——难看是难看了点，但比被拆穿那天，你提前了三个月。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_ledger_c_reported", type = "oversight", title = "有据可查",
                when = new When { md = "05-20", fromYear = 2028, requireMarks = new[] { "ledger_reported" } },
                paras = new List<string>
                {
                    "审计抽查涉及那笔“调整未遂”——因为当年即报，处置记录、谈话纪要一应俱全，抽查组在两小时内就签了字。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="补交一份情况说明归档",
                        effects=new Effects{ polCapital=1, morale=1, admin=1 },
                        result="你的档案里从此多了一页“主动报告、处置及时”。纸很薄，分量不轻。" },
                },
            });
        }

        // ---------------- 开局志向 ----------------

        private static void RegisterAmbition()
        {
            Flow.Register(new GameEvent
            {
                id = "p_ambition", type = "system", title = "序章 · 你为什么来",
                paras = new List<string>
                {
                    "报到前夜的最后一问，不是考官问的，是你自己问的。",
                    "十年很长，长到足以让一个人变成另一个人；十年也很短，短到来不及假装。你在心里，把这十年定了个调子——",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="做点实际的事——项目、数据、落地的政策",
                        effects=new Effects{ ambition="做事", morale=2, exec=1 },
                        result="你想的是“至少经我手的活，别烂在纸上”。这个念头不响，但结实。" },
                    new EventOption{ label="往上走——职级、平台、更大的事",
                        effects=new Effects{ ambition="晋升", morale=1, political=1 },
                        result="你不否认自己想要那个台阶。想往上走不丢人——丢人的是又想要又不认。" },
                    new EventOption{ label="把日子过安稳——规律的、体面的、可预期的生活",
                        effects=new Effects{ ambition="安稳", morale=3, stress=-2 },
                        result="父母那辈人常说，安稳是福。你半信半疑，但决定先信着试试。" },
                    new EventOption{ label="让家里人过得宽裕些——钱不是全部，但钱是底气",
                        effects=new Effects{ ambition="搞钱", morale=1, comm=1 },
                        result="你想起家里那张饭桌。有些账，从你领到第一个月工资那天就要开始算了。" },
                },
            });
        }

        // ---------------- 序章·出身 兑现（2026-10-20，与科长的第一次深谈） ----------------

        private static void RegisterOriginPayoffs()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_origin_gz", type = "person", title = "科长问起了你的老家",
                when = new When { date = "2026-10-20", flag = "origin_guanzhong" },
                paras = new List<string>
                {
                    "午休时周衡之端着茶杯路过你工位，忽然问：“听任姐说，你是渭南县里的？父亲在水利局？”",
                    "“县里出来的孩子懂分寸，”他说完这句就走了。你听懂了——在他的字典里，这是句好话。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把这些年饭桌上听来的机关门道，挑两句说给他听",
                        effects=new Effects{ rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=3, trust=1, memo="聊过县城机关的家常" } }, political=1 },
                        result="他听完难得笑了一下：“记性好。机关就是靠这些家常话连起来的。”" },
                    new EventOption{ label="谦虚几句，把话题还回去",
                        effects=new Effects{ rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=1 } } },
                        result="你把话头轻轻接住又放下。他点点头走了——这一页翻得不坏，也不算出彩。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_origin_jn", type = "person", title = "外贸世家的饭桌本事",
                when = new When { date = "2026-10-20", flag = "origin_jiangnan" },
                paras = new List<string>
                {
                    "局里要接待一批江南州来的客商。座谈会上对方报出一串行业术语，科里一时没人接得上。",
                    "你凭着家里饭桌上熏出来的那点语感，把对方的意思复述了三句——鞭辟入里。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="会后把客商的关注点整理成一页纸交给科长",
                        effects=new Effects{ admin=1, professional=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1, memo="江南客商那场，他接得住" } },
                            document=new DocRecord{ title="客商座谈要点", signature="主笔" } },
                        result="周衡之把那页纸看了两遍：“家里做生意的？”你点头。他说：“那你比我们懂他们要什么。”" },
                    new EventOption{ label="只在会上出力，会后不多事",
                        effects=new Effects{ comm=1 },
                        result="会开完了，事也就过去了。你的名字在科长那里轻轻记了一笔，不重。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_origin_hb", type = "person", title = "钢厂三代的手感",
                when = new When { date = "2026-10-20", flag = "origin_huabei" },
                paras = new List<string>
                {
                    "市里讨论传统产业转型，材料里一句“坚决淘汰落后产能”，在你眼里变成了厂区公告栏前一张张具体的脸。",
                    "你忍不住在讨论时说了一句：“产能背后是人。转岗安置的口径，建议先于指标落地。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把这句话展开，写成一份内部建议",
                        effects=new Effects{ political=1, comm=1, rel=new List<RelDelta>{ new RelDelta{ id="lin", familiar=2, evalv=1, memo="转型讨论里的话有分量" } },
                            document=new DocRecord{ title="转型期职工安置口径建议", signature="主笔" } },
                        result="林晚看完说：“你这话有来处。”你知道她指的不是文献，是生活。" },
                    new EventOption{ label="点到为止，不多说",
                        effects=new Effects{ morale=-1 },
                        result="话说了半截，你把它咽了回去。散会后有点后悔——机关里，后悔是常客。" },
                },
            });
        }

        // ---------------- 序章·实践年 兑现（2026-11-10） ----------------

        private static void RegisterPracPayoffs()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_prac_factory", type = "work", title = "车间那一年的手感",
                when = new When { date = "2026-11-10", flag = "prologue_factory" },
                paras = new List<string>
                {
                    "科里在核一家装备制造企业的补贴申报材料。别人看报表，你先看了工序单——车间那一年教会你的：报表会喘气，工序不会。",
                    "你发现申报材料里的产能数据和检修记录对不上。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把疑点列出来，附上工序依据",
                        effects=new Effects{ admin=1, exec=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1, memo="企业材料核得出真东西" } },
                            task=new TaskRecord{ title="企业补贴申报复核", note="查出产能口径疑点", grade="A", signature="主笔" } },
                        result="企业后来补了说明——数据没错，是设备大修停了两个月。虽是虚惊，但周衡之记住了：这小子核得出真东西。" },
                    new EventOption{ label="按流程报统计科复核",
                        effects=new Effects{ exec=1 },
                        result="流程走了，事清了。你在其中起的作用不大——但你学会了把专业的事交给专业的口。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_prac_village", type = "person", title = "老康来信",
                when = new When { date = "2026-11-10", flag = "prologue_village" },
                paras = new List<string>
                {
                    "一张明信片，字迹用力得几乎划破纸背：“小沈：青溪的路修通了，你那年帮着跑的手续，没白跑。老康。”",
                    "你把明信片压在工位玻璃板下面。有些账，乡里人不记账，但认。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="回一张明信片，附上局里惠农新政策的剪报",
                        effects=new Effects{ morale=3, comm=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=1 } },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_laokang_visit", afterDays=420 } } },
                        result="你在剪报边上写了一行字：路通了，下一步是货。寄出去的时候，你觉得这十年没那么抽象。" },
                    new EventOption{ label="把明信片收进抽屉",
                        effects=new Effects{ morale=1 },
                        result="玻璃板下多了一张纸。偶尔抬头看见，心里某处会软一下。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_laokang_visit", type = "person", title = "老康进城",
                when = null,
                paras = new List<string>
                {
                    "门卫打来电话：楼下有位康主任找。你跑下去，老康拎着一袋核桃站在阳光里，完全不像一个会对局长点头哈腰的人。",
                    "“来市里办合作社的事，”他把核桃塞给你，“顺便看看，你小子胖没胖。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="领他去食堂吃饭，把合作社的材料帮他捋一遍",
                        effects=new Effects{ morale=4, comm=1, political=1, moneyDelta=-45, energy=-4,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=1, memo="基层来人，接待得体" } },
                            logKind="人物", logText="老康进城，你陪他跑了半天手续" },
                        result="你带他跑了两个科、盖了三个章。送他上公交时他说：“你现在说话有干部样了。”你分不清是夸还是损——大概都是。" },
                    new EventOption{ label="工作太忙，给他指了办事窗口",
                        effects=new Effects{ morale=-2 },
                        result="他摆摆手说理解。你看着他的背影排队取号，忽然想起那年乡里的月亮。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_prac_district", type = "work", title = "区发改局那年的底子",
                when = new When { date = "2026-11-10", flag = "prologue_district" },
                paras = new List<string>
                {
                    "市局要汇总各区县的月度数据。别人对着口径说明发懵，你一眼认出这张表的祖宗——区发改局那张你抄了一年的表。",
                    "顺手的事，你把三处常见口径错误提前标了出来。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把口径要点整理成一页“防坑指南”发给各联络员",
                        effects=new Effects{ admin=2, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=2, memo="口径防坑指南，省了多少来回" } },
                            task=new TaskRecord{ title="月度数据口径指引", note="提前堵住三处常见错误", grade="A", signature="主笔" } },
                        result="那个月各区的返工率明显降了。任雪梅在走廊里说：“综合科这个新人，上道得很早。”" },
                    new EventOption{ label="留着自己用",
                        effects=new Effects{ admin=1 },
                        result="你自己少加了几天班。本事留在手里，没变成关系——也不亏，只是慢。" },
                },
            });
        }

        // ---------------- 大学 兑现（2027-03-16） ----------------

        private static void RegisterUniPayoffs()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_uni_studious", type = "work", title = "专业第一的旧账",
                when = new When { date = "2027-03-16", flag = "uni_studious" },
                paras = new List<string>
                {
                    "局里请了党校教授讲宏观形势。互动环节教授抛出一个模型问题，会议室安静了十秒。",
                    "你把那套供给冲击的分析框架在脑子里过了一遍——典籍馆旧阅览室那四个冬天，没有白坐。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="举手，把模型讲清楚",
                        effects=new Effects{ professional=2, reputation=1, rel=new List<RelDelta>{ new RelDelta{ id="ma", evalv=1, memo="党校课上答得漂亮" } } },
                        result="教授说：“机关里有这个底子的不多。”马建国坐在后排，抬眼看了你一下——记性极好的人，又记了一笔。" },
                    new EventOption{ label="会后把分析写成简报呈科长",
                        effects=new Effects{ professional=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } },
                            document=new DocRecord{ title="宏观形势学习要点", signature="主笔" } },
                        result="周衡之把简报转给了副局长，署名处是你的名字。藏在纸后面的功夫，终于从纸后面露了一下头。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_uni_research", type = "work", title = "数字政府课题的续篇",
                when = new When { date = "2027-03-16", flag = "uni_research" },
                paras = new List<string>
                {
                    "《AI治理法》施行在即，局里在讨论算法备案的口径。你大学跟导师做的“数字政府”课题，忽然有了用武之地。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把课题结论整理成三条备案口径建议",
                        effects=new Effects{ professional=1, admin=1, setMarks=new List<string>{ "ai_expert" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_ai_taskforce", afterDays=25 } },
                            document=new DocRecord{ title="算法备案口径三条建议", signature="主笔" } },
                        result="三条建议里有一条被科长原文引用进汇报稿。有些伏笔，大学时代就埋下了。" },
                    new EventOption{ label="会上提了两句，没往深里走",
                        effects=new Effects{ comm=1 },
                        result="你点了题，没做文章。枪打出头鸟，可不出头的鸟，也打不着食。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_uni_social", type = "person", title = "社团攒下的人脉",
                when = new When { date = "2027-03-16", flag = "uni_social" },
                paras = new List<string>
                {
                    "市统计局来函要一份联合调研数据。函件走流程要两周——你想起辩论队的老队友童远就在统计局综合科。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="直接打电话给童远，约个饭把事定了",
                        effects=new Effects{ energy=-3, comm=1, moneyDelta=-60,
                            rel=new List<RelDelta>{ new RelDelta{ id="tong", familiar=3, trust=1, memo="一个电话省了两周流程" } },
                            task=new TaskRecord{ title="跨局数据协调", note="人情跑出来的效率", grade="B", signature="参与" } },
                        result="童远在电话那头笑：“你小子，毕业了还使唤队友。”周五的火锅局上，两周的流程变成了一句话。" },
                    new EventOption{ label="按流程走函",
                        effects=new Effects{ admin=1 },
                        result="两周后数据到了，规规矩矩。你偶尔会想：规矩之外，本来还有一条近路。" },
                },
            });
        }

        // ---------------- 基期事件 兑现（性格揭示的下游） ----------------

        private static void RegisterRevealPayoffs()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_reveal_amb", type = "person", title = "周衡之给你压了担子",
                when = new When { date = "2026-11-03", flag = "ambitious_reveal" },
                paras = new List<string>
                {
                    "基期谈话那天你说想做点“能留下痕迹的事”。周衡之一直没接话——今天他把一份材料放在你桌上：",
                    "“全局务虚会的交流发言，给我写初稿。写得好不好，副局长们都看着。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="接下来，熬两个晚上",
                        check=new Check{ main="professional", bonus=0.05f },
                        effects=new Effects{ energy=-12, reputation=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=1, evalv=2, memo="敢接大材料" } },
                            document=new DocRecord{ title="务虚会交流发言（初稿）", signature="主笔" } },
                        result="{grade}。稿子过了，一字未改。周衡之只说了句“别翘尾巴”——他护短的方式，就是给你更难的活。" },
                    new EventOption{ label="请求林晚把一道关",
                        check=new Check{ main="professional", bonus=0.1f },
                        effects=new Effects{ energy=-10, rel=new List<RelDelta>{ new RelDelta{ id="lin", familiar=2, trust=1 }, new RelDelta{ id="zhou", evalv=1 } },
                            document=new DocRecord{ title="务虚会交流发言（初稿）", signature="主笔" } },
                        result="林晚删了你三百个字，留下的都是筋。{grade}——你学会了分寸这个词的第一笔。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_reveal_def", type = "person", title = "科长的敲打",
                when = new When { date = "2026-11-03", flag = "defensive_reveal" },
                paras = new List<string>
                {
                    "基期谈话那天你先辩解了几句，周衡之当场没说什么。今天他路过你工位，放下一份返工的材料：",
                    "“数据和口径，先认再查。查清了再辩，是本事；上来就辩，是本能。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把这句话抄在便签上，贴在显示器边",
                        effects=new Effects{ admin=1, stress=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=1, evalv=1 } } },
                        result="便签很小，字很用力。有些课，别人用嘴上，你用墙。" },
                    new EventOption{ label="晚上加班把返工材料重新核一遍",
                        effects=new Effects{ energy=-8, admin=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } },
                            task=new TaskRecord{ title="返工材料复核", note="认错认得快", grade="B", signature="主笔" } },
                        result="第二天材料放上他桌时，他“嗯”了一声。在周衡之那里，这一声顶别人三句表扬。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_koujing", type = "work", title = "工位上的口径便签",
                when = new When { date = "2026-12-08", flag = "koujing_lesson" },
                paras = new List<string>
                {
                    "那次口径教训之后，你在工位边贴了张便签：“先对基期，再对本期，最后对口径。”",
                    "今天新来的借调生盯着你的便签看了半天。你忽然意识到——你也开始传帮带了。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把便签背后的三个坑，讲给他听",
                        effects=new Effects{ comm=1, morale=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=1 } } },
                        result="你讲得比想象中顺。教别人一遍，自己那道坎才算真迈过去。" },
                    new EventOption{ label="笑笑说“慢慢就懂了”",
                        effects=new Effects{ },
                        result="你想起自己当年问遍全科的窘。有些弯路，大概注定要每个人自己走一遍。" },
                },
            });
        }

        // ---------------- AI 专班链（ai_law_interest / 数字政府课题 → 借调专班） ----------------

        private static void RegisterAiChain()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_ai_class", type = "work", title = "《AI治理法》施行首日",
                when = new When { date = "2027-03-11", flag = "ai_law_interest" },
                paras = new List<string>
                {
                    "《AI治理法》今日施行。全局组织集体学习，条线上的同志逐条过文本，会议室的空调嗡嗡作响。",
                    "你去年就对这部法上过心——条款背后那套“算法只评分、人来定”的边界逻辑，你想再往前多走一步。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="主动请缨：牵头写一版条款解读",
                        check=new Check{ main="professional", bonus=0.1f },
                        effects=new Effects{ energy=-10, professional=1, setMarks=new List<string>{ "ai_expert" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_ai_taskforce", afterDays=40 } },
                            document=new DocRecord{ title="《AI治理法》条款解读（综合科版）", signature="主笔" } },
                        result="{grade}。解读印发各科室，落款是综合科——但谁执笔的，科里都知道。" },
                    new EventOption{ label="认真学习，做好笔记",
                        effects=new Effects{ professional=1, political=1 },
                        result="笔记记了七页。风口的事，先看风向，再决定上不上船。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_ai_taskforce", type = "person", title = "算法备案专班点将",
                when = null,
                paras = new List<string>
                {
                    "市里启动首批算法备案试点，局里要抽人进专班。分管副局长在会上问：“谁平时研究这个？”",
                    "会议室安静了三秒。周衡之咳了一声，没看你，但话是冲你说的：“我们科有个年轻人，写过条款解读。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="接下专班（借调半年，活硬，露脸也硬）",
                        effects=new Effects{ seconded="局算法备案专班", energy=-8, reputation=1, political=1,
                            setMarks=new List<string>{ "ai_seconded" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_ai_close", afterDays=200 } },
                            logKind="系统", logText="借调入局算法备案专班" },
                        result="专班的活比科里硬十倍：备案口径、企业座谈、和技术公司掰扯术语。累——但你在风口里面。" },
                    new EventOption{ label="婉拒：科里的活也离不开人",
                        effects=new Effects{ morale=-1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=1 } },
                            setMarks=new List<string>{ "ai_declined" } },
                        result="周衡之眼里闪过一丝“可惜”。机会这种东西，让出去一次，就不一定再等你了。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_ai_close", type = "person", title = "专班结项",
                when = null,
                paras = new List<string>
                {
                    "算法备案专班结项。首批十二家企业完成备案，口径手册印成了小册子——扉页的起草人名单里，有你。",
                    "分管副局长在总结会上点了你的名。散会后周衡之说：“回来吧，科里给你留着位置——留着值得的位置。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="带着专班的成果回科里",
                        effects=new Effects{ seconded="", morale=3, polCapital=3, reputation=1,
                            commend=new CommendRecord{ text="算法备案专班工作表现突出（专班通报表扬）" },
                            logKind="系统", logText="专班结项，回局履职" },
                        result="你回到综合科的工位，绿萝活着，台灯还是那盏。但有些东西不一样了——你的名字，进了另一个层级的视野。" },
                },
            });
        }

        // ---------------- 房子链（topic_housing_m → 2028 抉择 → 2031 回响） ----------------

        private static void RegisterHouseChain()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_house_a", type = "society", title = "房子这道题",
                when = new When { md = "04-10", fromYear = 2028, requireMarks = new[] { "topic_housing_m" } },
                paras = new List<string>
                {
                    "公租房合同快到期了。同批的何斌在看盘，苏晴打算继续排队，母亲在电话里说“该有个自己的窝了”。",
                    "长安的房价在政策微调中缓慢企稳——售楼处的沙盘锃亮，你的存折硌手。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="咬牙上车：掏空积蓄，背上房贷",
                        effects=new Effects{ moneyDelta=-68000, stress=4, morale=2, housing="自有住房（贷款）",
                            setMarks=new List<string>{ "house_owned_m" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_house_b_own", afterDays=420 } },
                            logKind="生活", logText="咬牙买房，背上房贷" },
                        result="签约那天你的手心全是汗。三十年的债务，和一个再也不用搬的家——成年人的世界，安全感是分期付款的。" },
                    new EventOption{ label="继续攒钱，排队等公租房转正式配租",
                        effects=new Effects{ morale=1, stress=-1, setMarks=new List<string>{ "house_wait_m" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_house_b_wait", afterDays=420 } } },
                        result="你把心里的算盘拨得噼啪响：钱不够，就先让脚步等一等收入。不体面吗？机关大院里，这叫稳。" },
                    new EventOption{ label="开口向家里求助，凑个首付",
                        effects=new Effects{ moneyDelta=-34000, morale=1, stress=2, housing="自有住房（家里帮衬）",
                            setMarks=new List<string>{ "house_family_m" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_house_b_family", afterDays=420 } },
                            logKind="生活", logText="父母帮衬，凑首付上了车" },
                        result="母亲把存折的密码报给你时，语气轻得像怕惊动什么。你告诉自己：这份重量，往后要一寸一寸还回去。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_house_b_own", type = "society", title = "房贷与房价",
                when = null,
                paras = new List<string>
                {
                    "两年过去了。长安的房价稳中有升，你那套房账面上“赚”了——但你的工资条还是那条工资条。",
                    "同事聚会聊起房子，有人问你“后悔吗”。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="不后悔。房子是住的，日子是过的",
                        effects=new Effects{ morale=3, stress=-1 },
                        result="你说完这句话，忽然发现自己信了。每月还贷的日子紧，但每天回家推开门，灯是你自己的。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_house_b_wait", type = "society", title = "配租名单",
                when = null,
                paras = new List<string>
                {
                    "配租名单公示，你的名字排在第 41 位——比去年前进了 20 位，离能分到的位次还差着一截。",
                    "何斌在旁边啧啧摇头：“这队排的。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="笑笑：好饭不怕晚，反正公租房住着也不憋屈",
                        effects=new Effects{ morale=2, stress=-1, moneyDelta=2000 },
                        result="你算了一笔账：这两年省下的月供差价，加上积蓄的利息——稳，有时候也是种盈利。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_house_b_family", type = "society", title = "帮衬的重量",
                when = null,
                paras = new List<string>
                {
                    "父母来长安看你，第一次走进“你的”房子。母亲摸了摸门框，父亲在阳台上站了很久。",
                    "临走时父亲说：“房子的事，别老挂着。把工作干好，比什么都强。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="送他们上高铁，把补品塞进他们的背包",
                        effects=new Effects{ morale=3, stress=-2, moneyDelta=-600 },
                        result="车开了，你在站台上站了一会儿。这房子的一半是他们给的——另一半，你得用往后的日子挣回来。" },
                },
            });
        }

        // ---------------- 家庭链（family_close_m → 2029 体检异常 → 回响） ----------------

        private static void RegisterFamilyChain()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_family_checkup", type = "society", title = "父亲的体检报告",
                when = new When { md = "10-30", fromYear = 2029, requireMarks = new[] { "family_close_m" } },
                paras = new List<string>
                {
                    "母亲深夜来电，声音压得很低：“你爸体检查出个指标，医生说……要复查。你别太担心啊。”",
                    "最后半句，恰恰说明该担心。高铁票三小时，假条一张，你握着手机在阳台上站了很久。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="连夜请假回家，亲自陪他复查",
                        effects=new Effects{ energy=-14, stress=-2, morale=2, setMarks=new List<string>{ "family_care_m" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_family_ok", afterDays=28 } },
                            logKind="人物", logText="请假回家陪父亲复查" },
                        result="医院走廊的长椅上，父亲忽然说：“你小时候发烧，我背你去县医院，也是这样的椅子。”你们都没再说话。" },
                    new EventOption{ label="工作上实在走不开，转五千块回家，电话里盯着",
                        effects=new Effects{ moneyDelta=-5000, stress=3, morale=-2,
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_family_late", afterDays=35 } },
                            logKind="人物", logText="父亲复查，你只寄回了钱" },
                        result="你说“工作忙”，父亲说“公家的事要紧”。挂了电话，你盯着天花板到两点。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_family_ok", type = "society", title = "虚惊一场",
                when = null,
                paras = new List<string>
                {
                    "复查结果出来了：良性，定期观察即可。医生说来得早。父亲在诊室外咧嘴笑：“没事，走，爸请你吃面。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="吃那碗面。把假再延一天",
                        effects=new Effects{ morale=5, energy=2, stress=-4 },
                        result="一碗八块钱的面，父亲吃得郑重其事。你后来常想：那天多请的假，是十年里回报率最高的一笔投资。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_family_late", type = "society", title = "慢性病",
                when = null,
                paras = new List<string>
                {
                    "复查结果是慢性病，要长期吃药、忌口、定期监测。母亲在电话里絮絮地说，你隔着一千二百公里点头。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="给家里装个视频摄像头，教会母亲用",
                        effects=new Effects{ moneyDelta=-8000, stress=-1, morale=1,
                            logKind="人物", logText="给家里装了摄像头，每周固定视频" },
                        result="你调试摄像头那晚，父亲别扭地在镜头前坐直了。从那以后，每周日晚八点，是你的“例会”。" },
                },
            });
        }

        // ---------------- 辞职诱惑链（resign_seed_m → 2029 挖角 → 可中途辞职） ----------------

        private static void RegisterResignChain()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_headhunt", type = "person", title = "老同学的橄榄枝",
                when = new When { md = "07-05", fromYear = 2029, requireMarks = new[] { "resign_seed_m" } },
                paras = new List<string>
                {
                    "大学室友创办的政策咨询公司拿了融资，专程来长安请你吃饭。“急缺懂政府的人，”他把杯子推过来，“薪资翻三倍，明天就能入职。”",
                    "你想起那句“别糊弄”。也想起房租、房价，和同学群里晒的年终奖。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="婉拒。你走的这条路，不在别处",
                        effects=new Effects{ morale=2, polCapital=1, setMarks=new List<string>{ "stay_clean_m" } },
                        result="你说“谢谢，再看看”。回单位的公交上你问自己：是真不动心，还是已经走不开了？你没敢深想。" },
                    new EventOption{ label="留了联系方式：世事无绝对",
                        effects=new Effects{ stress=2, setMarks=new List<string>{ "exit_rising_m" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_headhunt2", afterDays=40 } } },
                        result="名片放进钱包夹层。从此那层钱包，偶尔会硌到你。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_headhunt2", type = "person", title = "第二次橄榄枝",
                when = null,
                paras = new List<string>
                {
                    "室友又来了，这次带着合同模板。“考虑得怎么样？这轮结束，下一个坑位就没了。”",
                    "窗外的长安华灯初上。体制内的十年你走了三分之一——剩下的三分之二，押在哪一边？",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把名片还给他：谢谢你，但我的路在这边",
                        effects=new Effects{ morale=2, polCapital=1, stress=-1, setMarks=new List<string>{ "stay_late_m" } },
                        result="他耸耸肩，说佩服。你走出包厢，晚风一吹，忽然觉得肩上的工牌轻了半克。" },
                    new EventOption{ label="递上辞呈——十年之约，到此为止",
                        effects=new Effects{ resigned=true, logKind="系统", logText="接受企业邀约，辞去公职" },
                        result="笔尖落下的时候，你听见自己心跳如鼓。工牌、门禁、公文系统——一样一样交回去，像交还一段人生。" },
                },
            });
        }

        // ---------------- 小弧线：马拉松 / 情人节 / 初心 / 处分翻篇 / 考证冲刺 ----------------

        private static void RegisterSmallArcs()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_marathon", type = "society", title = "长安城市马拉松",
                when = new When { date = "2028-04-16", requireMarks = new[] { "marathon_m" } },
                paras = new List<string>
                {
                    "你说过的那句“想跑一次马拉松”，今天是兑现的日子——长安城市马拉松，三万人的城池狂欢。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="参赛！跑完半程",
                        effects=new Effects{ energy=-20, morale=5, stress=-4,
                            rel=new List<RelDelta>{ new RelDelta{ id="he", familiar=2, memo="马拉松终点线见的都是真交情" } },
                            logKind="生活", logText="完成长安马拉松半程" },
                        result="最后三公里你是骂着街跑完的。冲线那一刻，何斌给你挂上奖牌：“行啊你，科员里的耐力型选手。”" },
                    new EventOption{ label="报了名却伤病退赛，去当志愿者",
                        effects=new Effects{ energy=-6, reputation=1, comm=1, morale=2 },
                        result="你在补给站递了两千杯水。跑不成的那口气，换成了另一种参与——成年人的遗憾，大多这样消化。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_confess_echo", type = "oversight", title = "巡视整改·回头看",
                when = new When { date = "2030-06-15", requireMarks = new[] { "confess_m" } },
                paras = new List<string>
                {
                    "巡视整改“回头看”，当年那份主动说明的书面材料被再次调阅。回访组的结论里有八个字：“主动纠错，态度端正。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把整改台账亲手交上去",
                        effects=new Effects{ morale=3, polCapital=2, stress=-2,
                            commend=new CommendRecord{ text="巡视整改主动作为（回访组通报肯定）" } },
                        result="当年按手印时汗湿的纸，如今成了档案里的加分项。程序不会忘记你做对的事——只是它记得比较慢。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_wait_echo", type = "oversight", title = "档案里那一页",
                when = new When { date = "2030-06-15", requireMarks = new[] { "wait_m" } },
                paras = new List<string>
                {
                    "“回头看”的谈话提前通知了你。坐下之后对方翻开的，是当年那份《情况说明通知书》——它和你的档案躺在一起，安静得像一句没说出口的话。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把整改情况一字一句讲清楚",
                        effects=new Effects{ stress=-1, political=1, morale=1 },
                        result="这次你讲得很细——上一次的教训教会你：主动交代永远比被动解释便宜。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_dream_revisit", type = "person", title = "入职两周年的抽屉",
                when = new When { md = "09-30", fromYear = 2028, requireMarks = new[] { "dream_m" } },
                paras = new List<string>
                {
                    "整理抽屉，你翻出入职那晚写下的那句话——两年来你几乎忘了它的存在。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把它重新钉在软木板上",
                        effects=new Effects{ morale=3, setMarks=new List<string>{ "dream_kept_m" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_dream_2033", afterDays=1825 } } },
                        result="那张纸条颜色已经发黄。你把它钉在最显眼的位置——不是为了看它，是为了偶尔抬头的那个瞬间。" },
                    new EventOption{ label="放进碎纸机——人要向前看",
                        effects=new Effects{ stress=-1 },
                        result="纸屑簌簌落下。有些东西碎了才是解脱——你也说不清这句话是不是安慰。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_dream_2033", type = "person", title = "十年过半，初心对账",
                when = null,
                paras = new List<string>
                {
                    "2033年9月30日，入职第七年。软木板上那张发黄的纸条又一年按时出现在视野里。",
                    "你把七年经手的事在心里过了一遍账：离那句话，是近了，还是远了？",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="近了。再走三年看看",
                        effects=new Effects{ morale=5, stress=-2, logKind="系统", logText="十年过半，初心对账：未完待续" },
                        result="你给自己倒了杯茶。初心这种东西，每年对一次账，就不算辜负。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_punished_after", type = "person", title = "翻篇",
                when = new When { flag = "punished" },
                paras = new List<string>
                {
                    "处分的事过去几天了。走廊里遇到科长，他脚步没停，只丢下一句：“评优的名单今年没你——但明年的路还长。”",
                },
            options = new List<EventOption>
                {
                    new EventOption{ label="把这句话记在本子上",
                        effects=new Effects{ morale=1, stress=-2, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=1 } } },
                        result="翻篇不是遗忘，是允许自己继续往前走。你合上本子，午休还剩二十分钟。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_exam_ticket", type = "work", title = "准考证与冲刺",
                when = null,
                paras = new List<string>
                {
                    "转官考试的准考证下来了。照片上的你穿着三年前的衬衫，考场信息一栏写着州府。",
                    "距离开考还有两个月——吏轨到官轨的那道门，钥匙在你自己手里。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="立个冲刺计划，每周固定两晚复习",
                        check=new Check{ main="professional", bonus=0.1f },
                        effects=new Effects{ energy=-10, professional=2, morale=2, setMarks=new List<string>{ "exam_sprint_m" },
                            echoes=new List<EchoSpec>{ new EchoSpec{ eventId="ch_exam_sprint_done", afterDays=56 } } },
                        result="{grade}。日历上圈出的那些晚上，台灯为你亮到十一点——有些考试，考前三个月就开始了。" },
                    new EventOption{ label="吃老本，考前突击一下",
                        check=new Check{ main="professional" },
                        effects=new Effects{ energy=-6 },
                        result="老本吃了几年还没空。{grade}——但你自己知道，有一道分析题答得心虚。" },
                },
            });
            Flow.Register(new GameEvent
            {
                id = "ch_exam_sprint_done", type = "work", title = "考后的一晚",
                when = null,
                paras = new List<string>
                {
                    "考完出考场，天擦黑。你给科长发了条短信：“考完了。”他回了三个字：“等你消息。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="给自己放一晚假",
                        effects=new Effects{ morale=4, stress=-5, energy=3 },
                        result="你沿着渭河走了很久。成绩还没出，但你已经把该做的做完了——这种感觉，比录取通知书先到。" },
                },
            });
        }
    }
}
