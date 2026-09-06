using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>婚恋家庭副线（Q3-06：副线）。三条感情线（苏晴/林依/陈曦）+ 结婚 + 生育，全部可选可拒。</summary>
    public static class ContentLife
    {
        public static void Register()
        {
            // 2028-10 与苏晴的默契
            Flow.Register(new GameEvent
            {
                id = "life_su", type = "person", title = "加班夜的一碗面",
                when = new When { date = "2028-10-17" },
                paras = new List<string>
                {
                    "晚上九点，投资科只剩你和苏晴。她收拾包时多带了一份面，放在你桌角：“楼下面馆的，加班餐——别多想，顺路。”",
                    "你抬头，她已经背起包走向电梯，耳根有点红。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="追上去说声谢谢，顺便约了周末",
                        effects=new Effects{ morale=4, setFlags=new List<string>{ "life_path_su" }, logKind="人物", logText="与苏晴开始走近" },
                        result="周末你们去了城墙根的老面馆。她讲台账，你讲材料——两个“数字洁癖”的约会，聊得像对数据。" },
                    new EventOption{ label="道谢，把关系留在战友层面",
                        effects=new Effects{ morale=1, rel=new List<RelDelta>{ new RelDelta{ id="su", trust=2, familiar=2 } } },
                        result="那碗面你吃得踏实。有些关系停在战友的位置，反而能走一辈子——你选择了这个版本。" },
                },
            });

            // 2029-03 相亲：林依
            Flow.Register(new GameEvent
            {
                id = "life_linyi", type = "person", title = "母亲安排的相亲",
                when = new When { date = "2029-03-16" },
                paras = new List<string>
                {
                    "母亲的电话带着不容拒绝的温度：“人家姑娘是小学老师，性格稳当，你们见一见。”你答应了——顺便也是给家里一个交代。",
                    "咖啡厅里，林依比想象中健谈。她讲班上的孩子，你讲写字楼和报表——两个世界的交集，是都想把日子过踏实。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="再见一面试试",
                        effects=new Effects{ setFlags=new List<string>{ "life_path_linyi" }, morale=2, logKind="人物", logText="与林依开始来往" },
                        result="第二次见面你迟到了十分钟——审查会拖堂。她没恼，只说：“看得出你忙得有内容。”" },
                    new EventOption{ label="坦白说：事业刚起步，暂不考虑",
                        effects=new Effects{ political=1 },
                        result="林依大方地笑了：“直说挺好。”回家你汇报“不合适”，母亲叹了口气，转头开始催下一个候选人。" },
                },
            });

            // 2031-03 产业线：陈曦（需产业经济路线）
            Flow.Register(new GameEvent
            {
                id = "life_chenxi", type = "person", title = "企业联络人陈曦",
                when = new When { date = "2031-03-18", route = "产业经济" },
                paras = new List<string>
                {
                    "高新区的产业对接会上，企业的政府事务经理陈曦条理清晰、进退有度——散会后她递来名片：“陈曦。以后对接，请多关照。”",
                    "此后半年，你们在工作里一来一回：她懂市场，你懂政策——对话总是很省力。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把工作关系处成朋友",
                        effects=new Effects{ setFlags=new List<string>{ "life_path_chenxi" }, comm=1, logKind="人物", logText="与陈曦熟络起来" },
                        result="你们约定：只聊行业，不谈具体项目——边界立得住，朋友才做得长。" },
                    new EventOption{ label="保持标准距离",
                        effects=new Effects{ political=2 },
                        result="“多关照”你是当着办公室七八个人回的：“按规矩办，都好说。”企业联络人，距离就是专业。" },
                },
            });

            // 2032-05 确定关系（三条线各自成立）
            Flow.Register(new GameEvent
            {
                id = "life_confirm_su", type = "person", title = "答案",
                when = new When { date = "2032-05-20", flag = "life_path_su" },
                paras = new List<string>
                {
                    "相识第四年，你约苏晴去了当年的面馆。你把一句话在心里演练了一路：“苏晴，处对象吧——正式的那种。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="说出口",
                        effects=new Effects{ partner="苏晴", morale=8, rel=new List<RelDelta>{ new RelDelta{ id="su", trust=5, familiar=6, memo="从战友到爱人" } } },
                        result="她低头笑了一下，用她确认台账的语气说：“核实无误，同意归档。”——全长安最浪漫的批复。" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "life_confirm_linyi", type = "person", title = "答案",
                when = new When { date = "2032-05-20", flag = "life_path_linyi" },
                paras = new List<string>
                {
                    "来往三年，林依陪你熬过了你妈生病住院的那个冬天——她排的陪护表比你的材料还工整。",
                    "这年五月，你买了枚戒指。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="求她做你的人生同伴",
                        effects=new Effects{ partner="林依", morale=8, logKind="人物", logText="与林依确立关系" },
                        result="她红着眼圈点头：“我等你这句话，等了三年——不过你求错了人，应该先求你妈，是她撮合的。”" },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "life_confirm_chenxi", type = "person", title = "答案",
                when = new When { date = "2032-05-20", flag = "life_path_chenxi" },
                paras = new List<string>
                {
                    "“只聊行业，不谈项目”的边界守了三年，终于被一次长谈打破——你们都意识到，边界外面站着的，一直是彼此。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="越过那条界线",
                        effects=new Effects{ partner="陈曦", morale=8, political=1, logKind="人物", logText="与陈曦确立关系（各自申报了利益关系）" },
                        result="你们做的第一件事，是分别向单位和公司申报了关系——她懂合规，你懂程序，这是你们能开始的原因。" },
                },
            });

            // 2034-06 结婚（有伴侣者）
            Flow.Register(new GameEvent
            {
                id = "life_marry", type = "person", title = "婚事",
                when = new When { date = "2034-06-08", flag = "life_marryable" },
                paras = new List<string>
                {
                    "两家人把婚事提上了日程。按机关的规矩，婚宴从简、申报在先——喜糖倒是备得足足的。",
                    "（若是与体制内对象成婚，双方的财产申报与回避事项都已依规办理。）",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="简办婚礼，宴请亲友同事",
                        effects=new Effects{ marry=true, morale=8, reputation=1, logKind="人物", logText="举行婚礼，机关同事到场祝贺" },
                        result="婚礼在咸阳老家办的长桌宴，周衡之代表科室致辞，就一句：“过日子和写材料一样——留余地，守分寸。”" },
                    new EventOption{ label="旅行结婚，一切从简",
                        effects=new Effects{ marry=true, morale=8, moneyDelta=-3000, logKind="人物", logText="旅行结婚" },
                        result="你们去了海边。没有司仪的排场，只有两个人的誓言和一场日出——回来时工位上的喜糖，被同事们一抢而空。" },
                },
            });

            // 2035-06 生育抉择（已婚者）
            Flow.Register(new GameEvent
            {
                id = "life_child", type = "person", title = "新生命的抉择",
                when = new When { date = "2035-06-01", flag = "life_childable" },
                paras = new List<string>
                {
                    "结婚一年后，“要不要孩子”成了家里最认真讨论的话题。你们俩都在事业上升期，也都明白：有些事，永远等不到“最合适的时候”。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="迎接新生命",
                        effects=new Effects{ child=true, morale=8, stress=4, logKind="人物", logText="家里迎来新生命" },
                        result="孩子出生那晚你正在加班，接到电话冲出办公楼时，鞋都穿反了。护士说：“爸爸的鞋反了。”——那是2035年你听过的最好笑、也最好哭的一句话。" },
                    new EventOption{ label="再等一等，先把两个人过好",
                        effects=new Effects{ morale=2 },
                        result="你们商量好：再给彼此一点时间。这个决定无关对错——人生的排序，只有自己能定。" },
                },
            });
        }
    }
}
