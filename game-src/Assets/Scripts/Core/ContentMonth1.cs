using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>2026年9月内容：入职月。16个脚本事件＋4个随机事件（M0 纵向切片）。</summary>
    public static class ContentMonth1
    {
        public static void Register()
        {
            // ---------- 9月1日（周二）：报到 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0901_report", type = "work", title = "报到第一天",
                when = new When { date = "2026-09-01" },
                paras = new List<string>
                {
                    "早上八点十分，长安市发展和改革局办公楼前。九月初的太阳已经有了点脾气，门卫室的小电视里放着早间新闻，正好提到《AI治理法》三审临近。",
                    "人事科在二楼。任雪梅（任姐）核对了你的公职资格证书、政审材料和报到证，抬眼打量了你一下：“翰林院大学的？今年分到咱们局的新人里，你成绩最靠前。”",
                    "她递给你一张工牌、一张食堂饭卡，和一句叮嘱：“综合科在三楼东，找你们科长——周衡之。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="提前二十分钟上三楼，先熟悉环境",
                        effects=new Effects{ energy=-4,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=3, memo="报到日提前到岗"}, new RelDelta{ id="ren", familiar=5 }},
                            logKind="人物", logText="入职第一天，提前到岗熟悉环境" },
                        result="三楼东还空着大半。你把综合科的门牌、科室分布图默记了一遍，又替窗台的绿萝浇了水。周衡之八点四十到，看见你，愣了一下：“新来的？”——他记住了你。" },
                    new EventOption{ label="准点到岗",
                        effects=new Effects{ energy=-2, rel=new List<RelDelta>{ new RelDelta{ id="ren", familiar=3 }} },
                        result="你踩着八点半进科。门开着，一位大姐抬头冲你笑笑：“新来的？坐，你科长马上到。”" },
                    new EventOption{ label="到岗先帮赵姐把饮水机的水换了",
                        effects=new Effects{ energy=-3, comm=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhao", familiar=8, trust=2, memo="第一天就帮我换水，实诚孩子" }, new RelDelta{ id="ren", familiar=3 }} },
                        result="“哎哟，这怎么好意思。”赵姐嘴上客气，已经把科里的规矩讲了一路。到岗第一天你就知道了谁的茶杯放在哪——三十年科员的经验，第一天就传给了你。" },
                },
            });

            // ---------- 9月2日（周三）：科长交代规矩 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0902_kezhang", type = "work", title = "科长的两条规矩",
                when = new When { date = "2026-09-02" },
                paras = new List<string>
                {
                    "周衡之的办公桌在科里最里侧，桌上一摞材料码得像刀切过。他把一份《上半年经济运行分析》推到你面前。",
                    "“综合科是干什么的？一句话：全局的笔杆子，数据的总闸。材料是发改局的脸面，数据是材料的命。规矩就这两条。”",
                    "他敲了敲那份材料：“先看去年的东西。什么叫分寸，看多了就有数了。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="认真做笔记，把规矩记进本子",
                        effects=new Effects{ admin=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=3, evalv=1, memo="新人肯下笨功夫" }} },
                        result="你在本子上写下两行字：材料是脸面，数据是命。周衡之瞥了一眼没说话，下午让赵姐给了你一份去年的全年数据合订本。" },
                    new EventOption{ label="当场请教：材料最常见的毛病是什么",
                        effects=new Effects{ comm=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=1, evalv=2, memo="敢问、会问" }} },
                        result="“毛病？”周衡之想了想，“最常见的是两种：一是把话写满，二是把话写死。写材料的分寸，就是给自己和别人都留余地。”你把这句话也记进了本子。" },
                    new EventOption{ label="只点头，先观察",
                        effects=new Effects{ political=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=1 }} },
                        result="你点了头，把情绪收进表情后面。周衡之低头继续改材料——红笔在纸上走得又快又稳。" },
                },
            });

            // ---------- 9月2日（周三）：认识科里人 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0902_colleagues", type = "person", title = "综合科的座位图",
                when = new When { date = "2026-09-02" },
                paras = new List<string>
                {
                    "下午，赵姐把科里的人给你挨个介绍。林晚从隔壁工位站起来冲你点头：“以后材料上有拿不准的，可以先问我——先说好，我只教方法，不代笔。”",
                    "赵姐压低声音：“咱科就咱们几个干活。马建国马副局长分管咱们科，你跟着大家叫马局就行。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把座位图记进本子最后一页",
                        effects=new Effects{ rel=new List<RelDelta>{ new RelDelta{ id="lin", familiar=5 }, new RelDelta{ id="zhao", familiar=3 }} },
                        result="你在本子最后一页画了个座位图：周科长、林晚、赵姐、你。机关的第一张地图，往往是这么画出来的。" },
                },
            });

            // ---------- 9月3日（周四）：同批新人 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0903_newbies", type = "person", title = "食堂：同批新人",
                when = new When { date = "2026-09-03" },
                paras = new List<string>
                {
                    "中午食堂，同批新人凑了一桌。规划科的许飞，就是那个笔试第一名；投资科的苏晴，安静，吃饭都很整齐；产业科的何斌，人没坐稳就先笑着绕桌打了个招呼。",
                    "“说说吧，都什么来头？”许飞先开的场。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="踏实型：老实交代学校与专业",
                        effects=new Effects{ comm=1, rel=new List<RelDelta>{ new RelDelta{ id="su", trust=4, familiar=4 }, new RelDelta{ id="xu", familiar=3 }} },
                        result="“翰林院大学，经济学。”你说得平淡。苏晴抬眼看了你一下：“学长好。”朴素的自我介绍像白开水——但在机关，白开水最解渴。" },
                    new EventOption{ label="进取型：顺带提到想干出点成绩",
                        effects=new Effects{ comm=2, rel=new List<RelDelta>{ new RelDelta{ id="xu", trust=5, familiar=4 }, new RelDelta{ id="he", trust=2, familiar=4 }, new RelDelta{ id="su", familiar=1 }},
                            setFlags=new List<string>{ "ambitious_reveal" } },
                        result="许飞眼睛一亮：“同行！”两双手握在一起，像两个连长在勘测各自的防区。苏晴低头扒饭，何斌笑着打圆场：“都是兄弟科室，以后多关照。”" },
                    new EventOption{ label="低调型：多问少说，把话题让给别人",
                        effects=new Effects{ political=2, rel=new List<RelDelta>{ new RelDelta{ id="su", trust=3, familiar=5 }, new RelDelta{ id="he", familiar=3 }} },
                        result="一顿饭下来，许飞讲了自己的备考史，何斌抖了三个“内幕”，苏晴一共说了两句话——其中一句是：“你挺会听人说话的。”你把这顿饭记进了心里的台账。" },
                },
            });

            // ---------- 9月4日（周五）：第一个任务 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0904_proof", type = "work", title = "第一个任务：校对会议纪要",
                when = new When { date = "2026-09-04" },
                paras = new List<string>
                {
                    "周衡之把一沓会议纪要复印件放在你桌上：“主任办公会的纪要，速记稿。你校一遍——错字、格式、数字，一个都别放过。”",
                    "这是你在综合科的第一件活。赵姐路过时小声说：“别慌，校对就是机关新人的开学第一课。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="快速校对一遍交差",
                        check=new Check{ main="exec" },
                        effects=new Effects{ energy=-5, task=new TaskRecord{ title="校对会议纪要", note="快速过稿" } },
                        result="你花了两个小时过了一遍，改出七处错。周衡之翻了翻，圈出你没发现的五处，没说话。{grade}——他心里有数了。" },
                    new EventOption{ label="逐字核对，把引用的数据对着台账查一遍",
                        check=new Check{ main="admin", bonus=0.1f },
                        effects=new Effects{ energy=-12, stress=2, admin=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1, memo="第一件活就肯对台账" }},
                            task=new TaskRecord{ title="校对会议纪要", note="数据核对到台账" } },
                        result="你核到第六页，发现纪要里引用的规上工业增加值和统计局月报对不上——差了0.3个百分点。你贴了便签。周衡之看到便签时停了两秒：“嗯。”这个字在后来很长时间里，都被你理解为嘉奖。{grade}。" },
                    new EventOption{ label="校对完请赵姐把把关再交",
                        check=new Check{ main="admin", bonus=0.05f },
                        effects=new Effects{ energy=-10,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhao", trust=3, familiar=2, memo="肯把活给我看，谦虚" }},
                            task=new TaskRecord{ title="校对会议纪要", note="师徒把关" } },
                        result="赵姐戴上老花镜看了一遍，指出你漏掉的一处：“年份写串了。这种错最要命——数字错是水平问题，年份错是态度问题。”{grade}。" },
                },
            });

            // ---------- 9月7日（周一）：科务会 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0907_weekmon", type = "work", title = "周一科务会",
                when = new When { date = "2026-09-07" },
                paras = new List<string>
                {
                    "周一上午的科务会。周衡之布置本周三条线：九月经济运行月报（数据从统计局来）、重点项目台账更新、以及“上面可能来调研AI产业，先做点案头准备”。",
                    "“综合科的活，三分写，七分备。”他看你一眼，“备，就是数据的来龙去脉都得清楚。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把三条线画成三列，标好节点",
                        effects=new Effects{ political=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=2 }} },
                        result="你在本子上把三条线画成三列。真正的机关工作，从这一页纸开始了。" },
                },
            });

            // ---------- 9月9日（周三）：第一次独立纪要 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0909_minutes", type = "work", title = "第一次独立写纪要",
                when = new When { date = "2026-09-09" },
                paras = new List<string>
                {
                    "“今天的全局季度工作推进会，你去记录。”周衡之把一支录音笔和一张座位图递给你，“会后出纪要，明早给我。”",
                    "会议室里，马建国副局长坐在主位。各部门负责人的发言有的长、有的短，有的话里有话。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="严格按模板记录：谁说、定了什么、谁落实",
                        check=new Check{ main="admin" },
                        effects=new Effects{ energy=-10, task=new TaskRecord{ title="季度推进会纪要", note="模板起步", signature="主笔" } },
                        result="你的纪要条理清楚。周衡之改了三处，都是语气——“部门协同”改成了“进一步加强部门协同”。你学到了：定性的词，重若千钧。{grade}。" },
                    new EventOption{ label="先请教赵姐纪要要点，再动笔",
                        check=new Check{ main="admin", bonus=0.1f },
                        effects=new Effects{ energy=-12, rel=new List<RelDelta>{ new RelDelta{ id="zhao", trust=2, familiar=1 }},
                            task=new TaskRecord{ title="季度推进会纪要", note="请教后成稿", signature="主笔" } },
                        result="赵姐就说了三句话：“纪要不是记录。领导反复说的，是重点；没人接话的，是难处；当场拍板的，是责任。”你按这三句话重组了纪要，周衡之这次只改了一处。{grade}。" },
                    new EventOption{ label="自己写完，再请林晚把关",
                        check=new Check{ main="admin", bonus=0.15f },
                        effects=new Effects{ energy=-14, stress=2, rel=new List<RelDelta>{ new RelDelta{ id="lin", trust=4, familiar=2, memo="第一次独立任务就肯请人把关" }},
                            task=new TaskRecord{ title="季度推进会纪要", note="林晚把关", signature="主笔" } },
                        result="林晚用铅笔在你的初稿上画了三道线：“这里把‘认为’换成‘指出’；这里，别替领导下结论；这里——”她顿了顿，“数据再核一遍，永远再核一遍。”改完交上去，周衡之这次什么都没改。{grade}。" },
                },
            });

            // ---------- 9月10日（周四）：林晚的点拨 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0910_lin", type = "person", title = "林晚的一句话",
                when = new When { date = "2026-09-10" },
                paras = new List<string>
                {
                    "傍晚，林晚收拾包准备下班，路过你工位时停了一下：“今天的纪要我看了，底子不错。”",
                    "“送你一句话——材料这东西，新手怕写错，熟手怕写浅。你现在怕错，是对的。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把这句话记在本子扉页",
                        effects=new Effects{ morale=2, rel=new List<RelDelta>{ new RelDelta{ id="lin", trust=2 }} },
                        result="她走了。你把这句话记在扉页。窗外长安的晚高峰正堵成一条河，写字楼和城墙在暮色里各站各的岗。" },
                },
            });

            // ---------- 9月15日（周二）：统计局对口径 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0915_tong", type = "person", title = "统计局：对口径",
                when = new When { date = "2026-09-15" },
                paras = new List<string>
                {
                    "月报要开工，数据得去统计局对口径。市统计局在三楼另一侧。你抱着一摞表格走过去，接洽的也是个年轻人，胸牌上写着：童远。",
                    "“发改局的？坐。”童远把一摞月度数据推过来，“先说好，规矩你都懂——规上规下、当月累计，别搞混。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="老实请教：两套口径最容易在哪儿出岔子",
                        effects=new Effects{ comm=1, political=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="tong", trust=4, familiar=5, memo="肯问口径，靠谱" }},
                            logKind="人物", logText="与统计局童远建立工作联络" },
                        result="童远来了精神，拉了块白板给你画了半小时：“最常出的岔子就一个——上月基数修订了，当月同比就变了。你们发改用数，最忌讳拿旧基数算新账。”你把白板抄了三页。临走他说：“以后对数直接找我，别走公文，快。”" },
                    new EventOption{ label="只交接数据，公事公办",
                        effects=new Effects{ rel=new List<RelDelta>{ new RelDelta{ id="tong", familiar=3 }} },
                        result="表格交接，签字确认。公事公办的第一次见面，像两个接口对上了协议——能用，但还不算联通。" },
                    new EventOption{ label="交接完顺口聊聊长安房价",
                        effects=new Effects{ comm=1, rel=new List<RelDelta>{ new RelDelta{ id="tong", trust=1, familiar=6 }},
                            setFlags=new List<string>{ "topic_housing" },
                            setMarks=new List<string>{ "topic_housing_m" } },
                        result="“房价？”童远笑了，“数据上，九月环比还在阴跌；但公租房摇号人数创了新高。这就是长安：数字冷静，人心火热。”你们聊到下班铃响。" },
                },
            });

            // ---------- 9月17日（周四）：节前 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0917_midautumn_prep", type = "society", title = "月饼票",
                when = new When { date = "2026-09-17" },
                paras = new List<string>
                {
                    "机关里的节日气息，是从行政科发月饼票开始的。每人两张票、一盒本地水晶饼，包装上印着“长安印月”。",
                    "赵姐把票分给你：“节前把活赶一赶——节前一周，全楼的打印机都比平时忙。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把月饼票夹进工牌套",
                        effects=new Effects{ morale=3, rel=new List<RelDelta>{ new RelDelta{ id="zhao", familiar=2 }} },
                        result="你把月饼票夹进工牌套。科里的节奏确实快了起来——月报进入倒计时。" },
                },
            });

            // ---------- 9月18日（周五）：月报初稿 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0918_draft", type = "work", title = "月报初稿",
                when = new When { date = "2026-09-18" },
                paras = new List<string>
                {
                    "月报任务正式落到你头上：九月经济运行月报的“综合经济运行情况”一节。GDP、规上工业、固定资产投资、消费、物价——五组数据，一页半篇幅。",
                    "周衡之只交代了一句：“初稿下周一给我。记住，月报是要给马副局长和局长看的。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="周末加班赶初稿，抢出质量",
                        check=new Check{ main="professional", bonus=0.1f },
                        effects=new Effects{ energy=-18, stress=5, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=1, evalv=2, memo="第一个月报肯下力气" }},
                            task=new TaskRecord{ title="九月经济运行月报·初稿", note="周末加班赶稿", signature="主笔" } },
                        result="你把周末几乎全搭了进去。周一早上，初稿放上周衡之的桌——他看了很久，久到你以为出了大问题。“开头这句结论，谁教你的？”“看人家年报……都这么写。”“以后不这么写。”他顿了顿，“但这份能用。”{grade}。" },
                    new EventOption{ label="按部就班，工作时间内完成",
                        check=new Check{ main="professional" },
                        effects=new Effects{ energy=-12, task=new TaskRecord{ title="九月经济运行月报·初稿", note="按期完成", signature="主笔" } },
                        result="你按节奏推进：数据、初稿、自校、成稿，交稿准时，质量平稳。周衡之批了四个字：“可用，再磨。”{grade}。" },
                    new EventOption{ label="先搭提纲，请林晚定调再写",
                        check=new Check{ main="professional", bonus=0.05f },
                        effects=new Effects{ energy=-14, admin=1, rel=new List<RelDelta>{ new RelDelta{ id="lin", trust=3, memo="提纲先送我看，稳" }},
                            task=new TaskRecord{ title="九月经济运行月报·初稿", note="提纲先行", signature="主笔" } },
                        result="林晚看完提纲，圈了两个数据点：“这两处要‘研判’，不要‘描述’。月报不是数字搬家，领导要看的是数字背后——稳不稳、有没有苗头、下一步怎么办。”你照这个调子写完。{grade}。" },
                },
            });

            // ---------- 9月22日（周二）：口径教训 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0922_koujing", type = "work", title = "基期修订",
                when = new When { date = "2026-09-22" },
                paras = new List<string>
                {
                    "上午十点，童远的电话直接打到你座机：“喂，你们月报里八月规上工业增加值增速，是不是用上月月报的基数？统计局这边上个月修订了基期，你们那个数得跟着调。”",
                    "你心里“咯噔”一下——初稿里那组增速，确实是拿旧基数算的。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="立即改数，向童远道谢，并向周科长报告修订出处",
                        effects=new Effects{ political=1, energy=-6,
                            rel=new List<RelDelta>{ new RelDelta{ id="tong", trust=3 }, new RelDelta{ id="zhou", trust=1, evalv=2, memo="数据出岔子第一时间报告修订出处" }},
                            setFlags=new List<string>{ "koujing_lesson" },
                            logKind="工作", logText="月报数据因基期修订同步调整" },
                        result="周衡之听完汇报看了你一眼：“知道错在哪就行——记住，数据的来路不清楚，材料写得再好也是空中楼阁。”童远在电话那头补了一句：“这才像话，以后咱俩对数省事了。”" },
                    new EventOption{ label="先辩解：数据来源是上期月报，不算错",
                        effects=new Effects{ political=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="tong", trust=-2 }, new RelDelta{ id="zhou", evalv=-2, memo="数据有争议先辩解——记下了" }},
                            setFlags=new List<string>{ "defensive_reveal" } },
                        result="电话那头沉默了两秒。“上期月报也是错的，”童远声音平平的，“修订公告上周就发了，你们没看。”你放下电话，后背有点发热——机关里最贵的东西之一，叫“我以为是”。" },
                    new EventOption{ label="不动声色，把全套数字重新核一遍再处理",
                        check=new Check{ main="admin", bonus=0.1f },
                        effects=new Effects{ political=1, admin=2, energy=-10,
                            rel=new List<RelDelta>{ new RelDelta{ id="tong", trust=2 }, new RelDelta{ id="zhou", evalv=1, memo="举一反三，稳" }},
                            setFlags=new List<string>{ "koujing_lesson" } },
                        result="你没先回话，而是把月报引用统计局的数据全部倒了一遍基期——除童远说的那组，又找出一处潜在偏差。你把两处修订连同出处一起报给周衡之。他这次多说了一个字：“好。”{grade}。" },
                },
            });

            // ---------- 9月24日（周四）：月报定稿 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0924_final", type = "work", title = "月报定稿",
                when = new When { date = "2026-09-24" },
                paras = new List<string>
                {
                    "九月经济运行月报定稿会。周衡之把改到第七稿的月报递给马建国副局长签批，你在旁边记录。",
                    "马副局长翻得很慢，在“运行总体平稳、结构稳中有进”那行停住：“这个‘稳中有进’，进在哪？”",
                    "周衡之答：“高技术制造业投资增速高于全部投资——数据在第三页。”马副局长“嗯”了一声，签了字。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把这一问一答记进本子",
                        effects=new Effects{ morale=3, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 }, new RelDelta{ id="ma", familiar=3, memo="九月月报初稿是新人写的——记得住了" }},
                            document=new DocRecord{ title="九月经济运行月报", signature="参与（初稿主笔）", note="定稿上报" } },
                        result="月报签批上报。从数据到定稿的十一天里，你第一次完整走完了一件“机关的事”。马副局长临走时问周衡之：“初稿谁写的？”——你在场，听得清清楚楚。" },
                },
            });

            // ---------- 9月25日（周五，中秋节） ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0925_midautumn", type = "society", title = "中秋节",
                when = new When { date = "2026-09-25" },
                paras = new List<string>
                {
                    "中秋节。长安下了点小雨，城墙在雨里变成一道剪影。这是你在长安的第一个传统节日——也是第一个没有安排工作的日子。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="到单位值班备勤",
                        effects=new Effects{ energy=-8, reputation=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=2, evalv=2, memo="中秋主动值班" }},
                            logKind="工作", logText="中秋节值班备勤" },
                        result="值班室里只有你和一个打瞌睡的保安。傍晚雨停，你在空荡荡的办公楼里给父母打了电话。晚上周衡之发来短信：“今天辛苦。节后科里聚餐，你来。”" },
                    new EventOption{ label="回父母家过节",
                        effects=new Effects{ morale=6, energy=5,
                            setFlags=new List<string>{ "family_midautumn" },
                            setMarks=new List<string>{ "family_close_m" },
                            logKind="人物", logText="中秋节回父母家" },
                        result="高铁四十分钟。母亲做了一桌菜，父亲问机关的事，你拣能说的说了几句。返程高铁上你靠着车窗想：所谓长安，大概就是从“回家”变成“回长安”。" },
                    new EventOption{ label="留在宿舍自习，把节后的调研提纲先读一遍",
                        effects=new Effects{ professional=2, energy=3,
                            setFlags=new List<string>{ "ai_law_interest" },
                            setMarks=new List<string>{ "ai_law_m" } },
                        result="你泡了壶茶，把《AI治理法（草案）》二审稿和局里的产业调研提纲对照着读。窗外烟花升起来的时候，你刚好读完最后一页。" },
                },
            });

            // ---------- 9月28日（周一）：授职礼 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0928_ceremony", type = "politics", title = "授职礼",
                when = new When { date = "2026-09-28" },
                paras = new List<string>
                {
                    "全市新录用公务员授职礼在国家大会堂侧厅举行。三百多名新人着正装列队，市人民政治委员会的领导逐一授职——轮到你时，你听见自己的名字在广播里响了一声。",
                    "礼成，全场起立奏国歌。你旁边的新人偷偷抹了一下眼角。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="庄严受礼",
                        effects=new Effects{ reputation=2, morale=5, political=1,
                            logKind="政治", logText="参加全市新录用公务员授职礼" },
                        result="散场时经过长廊，队伍里不知谁小声说：“咱们科员的档案，从今天起就是‘吏籍’了。”你摸了摸胸前的工牌——吏三，科员。三十岁的路，从今天起每一格都要自己走。" },
                },
            });

            // ---------- 9月29日（周二）：科里聚餐 ----------
            Flow.Register(new GameEvent
            {
                id = "ev_0929_dinner", type = "person", title = "中秋补的聚餐",
                when = new When { date = "2026-09-29" },
                paras = new List<string>
                {
                    "“中秋补的聚餐”——其实是赵姐张罗的便饭，科里几口人加你这个新人，就在局后街一家面馆。",
                    "周衡之难得话多，讲了讲他刚入职时把“调研”打成“掉研”的旧事，全桌笑作一团。林晚悄悄告诉你：“他一年就这一天话多。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="举杯，敬科里的各位老师",
                        effects=new Effects{ morale=4,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", familiar=4 }, new RelDelta{ id="lin", familiar=3 }, new RelDelta{ id="zhao", familiar=4, memo="席间懂事" }} },
                        result="散场时赵姐把找零硬塞给你：“拿着。科里规矩，新人第一年吃饭不掏钱。”你在心里把这碗面记进了另一本账——不是档案的那本。" },
                },
            });

            // ================= 随机事件池（工作日） =================

            Flow.Register(new GameEvent
            {
                id = "rnd_housing_lunch", type = "society", title = "午饭：公租房",
                when = new When { randomP = 0.15 },
                paras = new List<string>
                {
                    "午饭时苏晴和你拼桌，聊起公租房摇号：“我这批房源在浐河那边，通勤四十分钟。你的公租房呢？”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="吐槽各自的通勤",
                        effects=new Effects{ comm=1, morale=1, rel=new List<RelDelta>{ new RelDelta{ id="su", familiar=3 }} },
                        result="你们交换了通勤路线，得出一致结论：长安的地铁修得比房价跑得稳。" },
                    new EventOption{ label="认真聊公租房政策本身",
                        effects=new Effects{ political=1, rel=new List<RelDelta>{ new RelDelta{ id="su", trust=2 }} },
                        result="你把政策文件里的双轨制讲了一遍。苏晴认真听完：“你讲得像文件一样清楚。”不知是夸还是别的，但你记住了：知识用对场合，就是社交。" },
                    new EventOption{ label="笑笑，不接话",
                        effects=new Effects{ },
                        result="饭吃得安静。苏晴也没再说什么——机关里，不是每个话题都需要接住。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rnd_ai_news", type = "politics", title = "《AI治理法》二审",
                when = new When { randomP = 0.12 },
                paras = new List<string>
                {
                    "午间新闻推送：《AI治理法（草案）》完成二审，“算法可以提供意见，不得代替法定权力主体作出最终政治决定”的条款再次被顶上热搜。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="剪存报道，附上与本市产业相关的三行摘要",
                        effects=new Effects{ political=1, energy=-3, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } },
                            setFlags=new List<string>{ "ai_law_interest" },
                            setMarks=new List<string>{ "ai_law_m" },
                            logKind="政治", logText="剪存《AI治理法》二审报道并附摘要" },
                        result="你把摘要放在周衡之桌上。他扫了一眼：“三审通过前，把咱们高新区AI企业的底数摸一摸——回头调研用得上。”你的第一份“主动工作”，就这么有了回音。" },
                    new EventOption{ label="和同事讨论两句",
                        effects=new Effects{ comm=1, political=1 },
                        result="赵姐说：“管得对，机器不能替人拍板。”何斌说：“关键是谁的算法。”谁也没说服谁，但话题大家都记住了。" },
                    new EventOption{ label="看过就算",
                        effects=new Effects{ },
                        result="推送划走了。热点每天有，机关的日子按自己的钟摆走。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rnd_fusion_news", type = "society", title = "核聚变示范堆",
                when = new When { randomP = 0.10 },
                paras = new List<string>
                {
                    "晚间新闻：位于长安的下一代核聚变示范堆进入并网调试阶段。画面里，那座银灰色的大科学装置亮着常明灯。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="看完新闻",
                        effects=new Effects{ morale=2 },
                        result="你想起档案里那句国家战略。四百年前这个帝国决定“主抓科技进步”，今晚，它的灯光落在你的窗台外。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rnd_urgent", type = "work", title = "周五急件",
                when = new When { randomP = 0.15 },
                paras = new List<string>
                {
                    "周五下午四点五十，周衡之从会议室出来，径直把一份急件放在你桌上：“上面明天要的材料，今晚得报。你先顶一版，我改。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="留下加班，把急件顶下来",
                        check=new Check{ main="exec", bonus=0.1f },
                        effects=new Effects{ energy=-12, exec=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=2, evalv=2, memo="急件顶得住" }},
                            task=new TaskRecord{ title="急件：专项情况报告", note="当晚成稿" },
                            logKind="工作", logText="周五急件，加班成稿" },
                        result="晚上九点半，急件报出。周衡之走时在你桌边站了一下：“今天这活，叫‘救场’。机关里记住一个人的方式，往往就是这么一次。”{grade}。" },
                    new EventOption{ label="先理清口径再动笔，避免返工",
                        check=new Check{ main="admin", bonus=0.1f },
                        effects=new Effects{ energy=-10, admin=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 }},
                            task=new TaskRecord{ title="急件：专项情况报告", note="口径先行" } },
                        result="你花十分钟把三处口径跟赵姐对了一遍，然后一气呵成。周衡之只改了一个标点。“稳。”他说。{grade}。" },
                },
            });
        }
    }
}
