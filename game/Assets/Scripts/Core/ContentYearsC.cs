using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>第三卷（2033—2036）：转官与十年之约。赵姐退休、政治学院、十品就任、同批人散场。</summary>
    public static class ContentYearsC
    {
        public static void Register()
        {
            // ---------- 2033 ----------
            Flow.Register(new GameEvent
            {
                id = "y33_zhao", type = "person", title = "赵姐退休",
                when = new When { date = "2033-04-28" },
                paras = new List<string>
                {
                    "赵桂芳的退休手续办完了。她在科里待了整整三十一年——从手写油印到数字元报销，她见证了三个时代，职级停在了吏三。",
                    "散伙饭上她喝了两杯，红着脸说：“我这辈子没当上官，但我经手的档案，没有一页是糊涂账。”",
                    "她把那本翻烂的《公文格式手册》留给了科里——扉页写着：“给后来人。字是脸面，账是良心。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="接过手册，敬她一杯",
                        effects=new Effects{ morale=-2, professional=1, rel=new List<RelDelta>{ new RelDelta{ id="zhao", trust=5, familiar=6, memo="把手册留给了我" } }, commend=new CommendRecord{ text="获赵桂芳赠《公文格式手册》（传帮带）" } },
                        result="你站起来敬了三杯。第三杯敬的是三十年科员——职级丈量不出的东西，档案记得，人心也记得。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y33_district_work", type = "work", title = "区里的“小事”",
                when = new When { date = "2033-10-16" },
                paras = new List<string>
                {
                    "（挂职/下沉的同志都懂：区里没有小事。）一户居民的加装电梯申请，卡在两个科室之间三个月了。老人爬楼的喘息声，就是这件事的紧急程度。",
                    "在市里，这样的问题只是报表上的一行；在区里，它是七个楼层、二十八户人家的日常。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把两个科室叫到一起，当场拍板流程",
                        effects=new Effects{ comm=2, exec=2, reputation=1, political=1,
                            task=new TaskRecord{ title="加装电梯审批流程协调", note="基层实事实办", signature="主笔" } },
                        result="十分钟协调会，流程打通。电梯动工那天，老人送来一袋自家的橘子——基层履历的分量，原来是甜的。{grade}。" },
                },
            });

            // ---------- 2034：政治学院年 ----------
            Flow.Register(new GameEvent
            {
                id = "y34_debate", type = "politics", title = "学院辩论：效率与程序",
                when = new When { date = "2034-10-15", flag = "exam_passed" },
                paras = new List<string>
                {
                    "政治学院经典辩题：“重大公共项目中，当效率与程序冲突时，孰先？”你被分到“程序优先”一方——而你的对手，恰好是观点犀利的某国企挂职干部。",
                    "台下坐着的是未来十年的处级干部们。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="引用1978年大革命的历史教训",
                        effects=new Effects{ political=3, professional=1, reputation=1 },
                        result="你的陈词只有一段：“1970年代的帝国什么都有，就是没有边界——所以有了大革命。程序不是效率的敌人，程序是效率不翻车的轨道。”全场安静了三秒，然后是掌声。" },
                    new EventOption{ label="以退为进，承认对方的部分合理性",
                        check=new Check{ main="comm", bonus=0.15f },
                        effects=new Effects{ comm=2, political=1 },
                        result="你说：“在招标截止日这种具体场景，我方承认效率有优先性——但那是程序内部的裁量，不是对程序的豁免。”评委给了最高分：立场坚定，进退有据。{grade}。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y34_crisis", type = "politics", title = "危机模拟：能源断供",
                when = new When { date = "2034-12-08", flag = "exam_passed" },
                paras = new List<string>
                {
                    "期末危机模拟：极端气候导致外部能源通道中断四十八小时，你扮演市发改系统决策者——限电顺序、储备调度、舆论口径，每一个决定都在倒计时里。",
                    "“反方政府”训练组的教员在旁边不断施压：企业断电投诉、家属院供暖告急、上级要口径——三线同时起火。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="先保民生，再保重点，最后保形象",
                        check=new Check{ main="exec", bonus=0.15f },
                        effects=new Effects{ exec=2, political=2, stress=4, reputation=1 },
                        result="你把限电顺序排在民生、医院、重点企业之后才是形象工程——教员点评：“顺序对了，世界就简单了。”模拟结束后你在座位上坐了很久：真实的四十八小时，比这残酷一百倍。{grade}。" },
                },
            });

            // ---------- 2035 ----------
            Flow.Register(new GameEvent
            {
                id = "y35_takeoffice", type = "work", title = "上任第一站",
                when = new When { date = "2035-10-16", flag = "became_pin10" },
                paras = new List<string>
                {
                    "新岗位的第一个月。你发现“副处”两个字最先改变的不是别人对你的称呼，而是你批文的姿势——笔尖悬在纸上，比科员时代重了一百克。",
                    "老领导托人带话：“签字之前，把当年的《易错清单》再翻一遍——现在你的‘错’，是别人的‘灾’。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把清单升级成“签批三问”：依据、程序、后果",
                        effects=new Effects{ political=2, admin=2, reputation=1, logKind="系统", logText="确立签批三问工作法" },
                        result="你把“签批三问”贴在办公桌正对面。第一份经你手签发的文件流转出去了——它合规、准确、可追溯。这就够了，这就是全部。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y35_reform", type = "work", title = "营商环境改革攻坚",
                when = new When { date = "2035-12-10" },
                paras = new List<string>
                {
                    "你牵头的营商环境改革项到了验收关口：审批时限压缩60%的目标卡在最后两项——一项涉及跨部门数据壁垒，一项涉及某个“历史遗留问题”。",
                    "企业主在座谈会上说了句糙话：“别跟我们讲蓝图，讲讲我的执照哪天能下来。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="啃硬骨头：把壁垒清单晒在台面上",
                        check=new Check{ main="exec", bonus=0.1f },
                        effects=new Effects{ exec=2, political=1, reputation=2, stress=5, rel=new List<RelDelta>{ new RelDelta{ id="ma", evalv=2, memo="改革攻坚有章法" } },
                            task=new TaskRecord{ title="营商环境改革攻坚验收", note="壁垒清单销号", signature="主笔" } },
                        result="你把数据壁垒的“断点”逐个列给责任部门，当着分管领导的面逐个销号。年底验收：时限压缩61%——那多出的1%，是你陪企业跑的第七趟。{grade}。" },
                },
            });

            // ---------- 2036 ----------
            Flow.Register(new GameEvent
            {
                id = "y36_reunion", type = "person", title = "十年之约 · 同批人",
                when = new When { date = "2036-07-18" },
                paras = new List<string>
                {
                    "入职十年，同批人凑齐了一次——苏晴（或来信）、许飞、何斌，还有去了企业、去了江南、去了监察一线的老同学。",
                    "包间里加了三把椅子才坐得下。许飞举杯：“十年前咱们说‘让有人上光荣榜’——现在看看，咱们谁没上过几回？”",
                    "何斌接话：“我查了查，咱们这批二十三个人：副处以上七个，正科九个，离开体制的四个——还有俩，问就是‘保密’。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="“下一个十年，再约。”",
                        effects=new Effects{ morale=6, comm=2, rel=new List<RelDelta>{ new RelDelta{ id="xu", familiar=4 }, new RelDelta{ id="su", familiar=4 }, new RelDelta{ id="he", familiar=4 } } },
                        result="合影时，窗外长安的晚霞正好。十年前你们互报家门，十年后你们互道珍重——这个帝国把“让每一代人都有改变命运的机会”写进国训，而你们，就是这句话的注脚。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y36_lookback", type = "society", title = "城墙下的少年",
                when = new When { date = "2036-08-10" },
                paras = new List<string>
                {
                    "下班路上，你绕到了城墙根。红底白字的标语重新刷过了：“让每一代人都有改变自己命运的机会”。",
                    "十年前报到那天，你在这行字下面站过。今天，一个拖着行李箱的年轻人正在标语前拍照——他的胸牌上印着“长安市发展和改革局”。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="过去帮他拍了张照",
                        effects=new Effects{ morale=5 },
                        result="“谢谢哥！”年轻人冲你笑。你摆摆手走进暮色——十年，从被这座城接纳，到把接力棒递下去。墙内是四百年，墙外，是每一个人的十年。" },
                },
            });
        }
    }
}
