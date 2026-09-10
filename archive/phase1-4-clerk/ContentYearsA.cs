using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>第一卷（2026冬—2029）：站稳脚跟。科员→副科，确立路线，第一次借调。</summary>
    public static class ContentYearsA
    {
        public static void Register()
        {
            // ---------- 2026年冬 ----------
            Flow.Register(new GameEvent
            {
                id = "y26_talk", type = "person", title = "第一次被批评",
                when = new When { date = "2026-11-12" },
                paras = new List<string>
                {
                    "你整理的调研简报里，把两家企业的产值单位“万元”写成了“亿元”。周衡之把简报拍在你桌上，声音不大，脸很沉。",
                    "“单位错了三个数量级——这份东西要是报上去，全局都跟着丢人。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="立即改正，并自建“易错清单”",
                        effects=new Effects{ admin=2, stress=2, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1, memo="挨了批评会自省" } } },
                        result="当晚你整理出一份《易错清单：单位、年份、基数、人名》，贴在工位隔板上。三个月后，这份清单在科里流传开了。" },
                    new EventOption{ label="解释是原文抄错了",
                        effects=new Effects{ rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=-1 } }, political=1 },
                        result="周衡之听完，只回了一句：“交出去的东西，错的永远是最后的签名。”你把这句话嚼了很久。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y26_peers", type = "person", title = "同批人的第一年",
                when = new When { date = "2026-12-28" },
                paras = new List<string>
                {
                    "年底，同批新人小聚。饭桌上各自盘点：许飞进了政府工作报告起草组；苏晴的台账被市里转发；何斌认识半个市政府。",
                    "“明年这时候，”许飞举杯，“希望咱们中有人能上光荣榜。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="“先立个一年的小目标。”",
                        effects=new Effects{ morale=3, comm=1 },
                        result="你给自己定的目标是：明年，让周科长改你的稿子时“一字不改”。——这个目标，你后来用了很多年才做到。" },
                    new EventOption{ label="默默把目标写进本子",
                        effects=new Effects{ professional=1, political=1 },
                        result="你没说出来。目标写在心里是动力，说出口容易变酒话——机关教人的第一课，从饭桌上开始。" },
                },
            });

            // ---------- 2027 ----------
            Flow.Register(new GameEvent
            {
                id = "y27_ailaw", type = "politics", title = "《AI治理法》落地试点",
                when = new When { date = "2027-04-14" },
                paras = new List<string>
                {
                    "《AI治理法》正式施行，长安入选首批算法备案试点城市。市里成立专班，发改局要出一个产业影响评估——你被点名参与。",
                    "座谈会上，企业代表问得最凶的是一条：“算法可以提供意见，不得代替法定权力主体作出最终政治决定——那研发投入还敢不敢加？”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把企业的顾虑原样写进评估",
                        effects=new Effects{ political=2, professional=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1, memo="评估报告有胆识" } }, logKind="政治", logText="参与《AI治理法》落地评估" },
                        result="评估报告里那句“企业需要的是确定性，而不是空白”，被市领导在会上念了出来。确定性——从那天起你知道，政策语言里最贵的词。" },
                    new EventOption{ label="四平八稳，按模板交差",
                        check=new Check{ main="admin", bonus=0.05f },
                        effects=new Effects{ energy=-8, task=new TaskRecord{ title="《AI治理法》产业影响评估", note="模板化成稿", signature="参与" } },
                        result="报告交了，没出纰漏，也没激起水花。{grade}——有些活的价值就在于“没声音”。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y27_master", type = "person", title = "林晚的瓶颈",
                when = new When { date = "2027-07-08" },
                paras = new List<string>
                {
                    "林晚休假了——十年来第一次。科里传，她竞争副科材料撰写岗失败，需要缓缓。",
                    "赵姐叹气：“笔杆子都这样，笔快，路慢。她啊，把什么活都往身上揽。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="替林晚分担她手头的简报",
                        effects=new Effects{ exec=2, energy=-8, rel=new List<RelDelta>{ new RelDelta{ id="lin", trust=6, familiar=3, memo="我难的时候，是他接了我的活" } } },
                        result="你没说什么漂亮话，只是把她桌上的活悄悄分了一半。林晚销假回来，看见整整齐齐的归档，在工位上坐了很久。" },
                    new EventOption{ label="只做好自己的活",
                        effects=new Effects{ energy=0 },
                        result="机关里人人自顾不暇，你也不例外。只是那阵子路过林晚空着的工位，总觉得少了点什么。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y27_mafirst", type = "person", title = "马副局长的名字",
                when = new When { date = "2027-11-20" },
                paras = new List<string>
                {
                    "全局季度经济分析会。马建国副局长翻到“消费市场”一节，突然停下：“这一节的数是谁对的？”",
                    "会议室安静了两秒。周衡之报了你的名字。马副局长“嗯”了一声：“数是活的。”——这是他一整场会议里唯一的评价。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="会后被点名整理会议纪要",
                        effects=new Effects{ political=2, rel=new List<RelDelta>{ new RelDelta{ id="ma", familiar=5, evalv=2, memo="数是活的——记得这个新人" } }, comm=1 },
                        result="散会后周衡之提醒你：“马局记住你的名字了——记住名字，是器重，也是鞭子。”你把这句话连同那份纪要一起归了档。" },
                },
            });

            // ---------- 2028 ----------
            Flow.Register(new GameEvent
            {
                id = "y28_fusion", type = "society", title = "并网之夜",
                when = new When { date = "2028-05-20" },
                paras = new List<string>
                {
                    "长安核聚变示范堆实现稳定并网。新闻画面里，控制大厅的掌声像潮水——八年前，这里还只是图纸上的一个方框。",
                    "第二天上班，局长在全局会上说了一句话：“重大项目是城市的骨骼。骨骼要硬，血管要通，神经要灵——发改的工作，就是这三条。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把这句话记进本子",
                        effects=new Effects{ morale=3, political=1 },
                        result="“骨骼、血管、神经。”你后来无数次用这三个比喻向企业、向区县解释发改的职能——好框架是能复用的。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y28_midterm", type = "work", title = "五年规划中期评估",
                when = new When { date = "2028-09-15" },
                paras = new List<string>
                {
                    "“十五五”规划实施过半，中期评估启动。你负责“产业转型升级”一章的评估底稿——十八项指标，六项滞后。",
                    "林晚提醒你：“评估不是算账，是找原因。滞后指标背后，是政策错了、执行歪了，还是环境变了？”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="逐项访谈指标责任部门",
                        check=new Check{ main="comm", bonus=0.1f },
                        effects=new Effects{ energy=-16, comm=2, political=1, rel=new List<RelDelta>{ new RelDelta{ id="lin", evalv=2, memo="评估底稿扎实" } },
                            task=new TaskRecord{ title="“十五五”规划中期评估（产业章）", note="访谈式评估", signature="主笔" } },
                        result="你跑了七个部门，把六项滞后指标的病根分成三类：政策类两项、执行类三项、环境类一项——分类本身就是结论。{grade}。" },
                    new EventOption{ label="先做数据比对，快速成稿",
                        check=new Check{ main="admin", bonus=0.05f },
                        effects=new Effects{ energy=-12, task=new TaskRecord{ title="“十五五”规划中期评估（产业章）", note="数据比对", signature="主笔" } },
                        result="比对着完成稿，赶上了时限。评估会上有部门较真：“这个结论访谈过企业吗？”——你记住了这个漏洞。{grade}。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y28_pressure", type = "person", title = "第一次想辞职的夜晚",
                when = new When { date = "2028-12-15" },
                paras = new List<string>
                {
                    "连着三周加班，你把一份汇报材料改到第十一稿，领导只回了两个字：“再磨。”夜里十一点，办公室只剩你一盏灯。",
                    "手机屏亮了：大学同学晒出离职创业的融资喜报。你盯着屏幕，忽然问自己——图什么？",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="给父亲打个电话",
                        effects=new Effects{ morale=4, stress=-6 },
                        result="父亲没讲道理，只说：“你爷爷那辈修了三十年水渠。”你挂了电话，把第十二稿改完——有些答案，是熬出来的。" },
                    new EventOption{ label="下楼跑五公里",
                        effects=new Effects{ stress=-10, energy=-4, morale=3 },
                        result="你沿着环城北路跑了五公里，汗把西装后背浸透了。第二天，第十二稿一次通过——你不是唯一失眠的人，但你是恢复最快的人。" },
                    new EventOption{ label="把辞职念头写进备忘录",
                        effects=new Effects{ stress=-3, setFlags=new List<string>{ "resign_thought" },
                            setMarks=new List<string>{ "resign_seed_m" } },
                        result="你在备忘录里写下三个问号，锁上手机。有些念头不急着回答——留着，十年后回头看，它会变成坐标。" },
                },
            });

            // ---------- 2029 ----------
            Flow.Register(new GameEvent
            {
                id = "y29_games", type = "politics", title = "民族运动会 · 长安",
                when = new When { date = "2029-09-09" },
                paras = new List<string>
                {
                    "全国少数民族传统体育运动会在长安开幕。八州代表团依次入场——藏刀舞、安代舞、木卡姆、农乐舞，古城墙下是一场流动的多民族画卷。",
                    "“使其同政，不必使其同俗。”开幕式解说词里引用了帝国的老原则。你作为抽调的赛事保障联络员，在场馆里连轴转了九天。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="在时间轴与赛场之间连轴转",
                        effects=new Effects{ comm=2, political=2, morale=3, logKind="政治", logText="参与全国民族运动会保障工作" },
                        result="闭幕式那晚，各州代表团的旗子一起降下。你在工作手记里写：“统一这件事，不是消灭差异，是让差异安心。”——多民族共治的课，赛场比书本教得透。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y29_office", type = "work", title = "市府办的第一课",
                when = new When { date = "2029-11-06", flag = "" },
                paras = new List<string>
                {
                    "（若你在借调中，这是市府办教你的一课；若你在局里，这是传来的见闻。）",
                    "市政府办公室的老处长说：“市府办没有‘差不多’。一份纪要的歧义，可能就是两个部门三年的扯皮。”",
                    "“在这里写字，落笔之前先想三件事：谁执行？谁出钱？谁担责？”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把“三问”抄在工作手册扉页",
                        effects=new Effects{ political=2, professional=1 },
                        result="谁执行、谁出钱、谁担责——你后来发现，这三问适用于机关的一切文字，乃至一切人事。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "y29_district", type = "work", title = "第一次区县协调会",
                when = new When { date = "2029-06-18" },
                paras = new List<string>
                {
                    "为了一个跨区县的产业转移项目，你跟着科长连开三场协调会。两个区县各说各的理，火药味隔着桌子都能闻见。",
                    "散会后，科长教你：“协调会的本事不在会上，在会前——把两家能接受的下限先摸清楚。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="连夜做一份“双方底线对照表”",
                        check=new Check{ main="comm", bonus=0.1f },
                        effects=new Effects{ comm=2, political=1, energy=-10, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=2, evalv=1 } },
                            task=new TaskRecord{ title="跨区产业项目协调", note="底线对照法", signature="参与" } },
                        result="第四场会上，你把对照表摆在两家面前——争议从“要不要”变成了“怎么办”。科长在会后笑着说：“小同志，会前功大于会上功。”{grade}。" },
                },
            });
        }
    }
}
