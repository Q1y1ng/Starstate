using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// M2 · **模板挂链**：随机生成的日常卷宗一旦选了灰区处置，就会写下一个叙事标记
    /// （`stat_fudge` / `yq_delete` / `safety_loose` / `budget_split` / `procure_hold` / `petition_paper`
    /// / `yibao_soft` / `land_directed` / `avoid_soft`），本文件负责给这些标记派后续代价。
    ///
    /// 设计意图：模板件可以无限生成，但代价不是无限的——**账本只有一本**（风险账本 riskLedger），
    /// 每一次“照准/压下/打招呼”都会在某个未来的月份以一次约谈、一次倒查、一次反弹的形式回来。
    /// 若无此文件，模板件就只是重复的签字劳动，长线就会“机制不枯竭但意义枯竭”。
    ///
    /// 约定：md 事件逐年重现 → 每条线都带 `requireNotMarks = {本线完成标记}`；选项尽量给三条
    /// （认账/补程序/找人打招呼），让玩家在“钱、面子、账”之间选。
    /// </summary>
    public static class ContentTemplateChains
    {
        public static void Register()
        {
            RegisterStatAudit();
            RegisterOpinionBacklash();
            RegisterSafetyRepeat();
            RegisterBudgetAudit();
            RegisterProcureComplaint();
            RegisterPetitionEscalate();
            RegisterInsuranceSoft();
        }

        // ① 统计口径“技术处理” → 省统计局倒查（用电量说不圆）
        static void RegisterStatAudit()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_tc_stat", type = "oversight", title = "省统计局要三张表",
                when = new When { md = "03-20", fromYear = 2027, requireMarks = new[] { "stat_fudge" }, requireNotMarks = new[] { "tc_stat_done" } },
                paras = new List<string>
                {
                    "省统计局来了三个人，不查台账，只要三张表：规上工业增加值、工业用电量、工业增值税。",
                    "“不用准备材料，”带队的处长很客气，“我们对一下增速就行。”",
                    "三张表摆在一张桌上，增速差了两个百分点——那两点，是去年秋天你在报表上“技术处理”过的地方。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "当场认账，同步修正上年数据并向上说明",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_stat_done", "stat_corrected" },
                            compliance = 4, efficiency = -3, risk = -8, political = -1,
                            integrity = new IntegrityRecord { tag = "统计整改", note = "主动修正上年统计口径并说明原因" },
                            logKind = "统计", logText = "统计数据认账修正",
                        },
                        result = "你当着处长的面给统计局局长打电话：“按实际改，谁的责任谁写说明。”省里三天后回了函：不予追究，但要“举一反三”。修正数据那年大同没拿上先进——第二年拿了真的。",
                    },
                    new EventOption
                    {
                        label = "解释为口径调整，补一份说明",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_stat_done" }, compliance = 1, efficiency = 1, risk = 2,
                            logKind = "统计", logText = "统计口径以说明书应对",
                        },
                        result = "说明书交上去了，处长收下时说了句“我们会带回去”。数据没改，账也没清——只是把它们往后挪了一年。",
                    },
                    new EventOption
                    {
                        label = "先请省局同志吃饭，事后再谈",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_stat_done" }, risk = 9,
                            integrity = new IntegrityRecord { tag = "统计倒查", note = "统计倒查期间安排省局人员接待" },
                            logKind = "统计", logText = "统计倒查期间安排接待",
                        },
                        result = "饭吃了，处长始终没动那瓶酒。临走他说：“我们回去按程序写。”——程序写出来的东西，比饭桌上的话硬得多。",
                    },
                },
            });
        }

        // ② 舆情“降温” → 反弹（另一个账号，播放量翻倍）
        static void RegisterOpinionBacklash()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_tc_yq", type = "society", title = "同一个问题，换了个说法",
                when = new When { md = "04-18", fromYear = 2027, requireMarks = new[] { "yq_delete" }, requireNotMarks = new[] { "tc_yq_done" } },
                paras = new List<string>
                {
                    "一个月前你要求平台降热的那个视频，昨天被另一家媒体用另一种方式发了出来：不是拍现场，是拍现场旁边那条没修完的路。",
                    "播放量是上次的三倍。评论区第一句话是：“上次那个视频，我找不到了。”",
                    "市委宣传部的同志把舆情单送到你桌上，站着没走：这次压不动了。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "公开回应：把整件事连未修完的路一起说清",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_yq_done", "yq_public" },
                            compliance = 4, risk = -9, comm = 2, efficiency = -2,
                            logKind = "舆情", logText = "舆情：公开回应并未修完的路",
                        },
                        result = "回应稿只有四百字，把工期、资金、征地各写了一段，连“上次删帖”也写了一句“做法欠妥”。热度两天后落了。有人在评论区留了句：“终于像个政府了。”",
                    },
                    new EventOption
                    {
                        label = "继续协调平台降热并请媒体“顾全大局”",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_yq_done" }, risk = 12,
                            integrity = new IntegrityRecord { tag = "舆情处置", note = "二次要求平台降热并约谈自媒体" },
                            logKind = "舆情", logText = "舆情：二次降热并约谈媒体",
                        },
                        result = "热度又降了。宣传部同志走的时候回头看了一眼，什么也没说。三个月后，同一件事出现在省里的内参上——署名是“群众来信”。",
                    },
                    new EventOption
                    {
                        label = "不回应，让时间去冲",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_yq_done" }, risk = 4, reputation = -1,
                            logKind = "舆情", logText = "舆情：以沉默应对",
                        },
                        result = "七天后热度自然落了。问题没解决，只是大家说腻了。你桌上的舆情单压在第二份下面，半个月后归档。",
                    },
                },
            });
        }

        // ③ 安全检查“已整改”注水 → 同类事故再发（脚手架）
        static void RegisterSafetyRepeat()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_tc_safety", type = "oversight", title = "另一处脚手架",
                when = new When { md = "05-08", fromYear = 2027, requireMarks = new[] { "safety_loose" }, requireNotMarks = new[] { "tc_safety_done" } },
                paras = new List<string>
                {
                    "通报过的那批隐患里，有一家企业的“已整改”是承诺整改。今天上午，那家企业的二期工地掉下来一根钢管，砸穿了一间活动板房。",
                    "万幸，板房里当时没人。企业负责人已经在应急局门口等了两个小时。",
                    "应急局的同志把上次的通报稿和今天的现场照片并排放在你桌上——同一条脚手架通道，同一个位置。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按事故上报，倒查“承诺整改”是怎么签的字",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_safety_done", "safety_hard" },
                            compliance = 5, efficiency = -3, risk = -6, stress = 5,
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = -2, memo = "安全事故如实上报" } },
                            logKind = "监察", logText = "同类事故如实上报并倒查整改",
                        },
                        result = "上报材料里附了那张并排的照片。应急局连夜重新验了全市 47 处“承诺整改”。这次查出 9 处——比上次通报的整改数，少了 6 处，多了 9 条命。",
                    },
                    new EventOption
                    {
                        label = "作为一般险情处理，先赔付再内部整改",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_safety_done" }, risk = 13,
                            integrity = new IntegrityRecord { tag = "事故定性", note = "钢管坠落未按事故上报，作一般险情处理" },
                            logKind = "监察", logText = "同类事故作一般险情处理",
                        },
                        result = "企业很配合，赔偿当天到位。没上报的事故在这座城市里一年有很多起——不上报的每一件，都在等下一根钢管。",
                    },
                    new EventOption
                    {
                        label = "全市停工一天，重新验一遍",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_safety_done", "safety_hard" },
                            compliance = 4, efficiency = -6, risk = -4, exec = 2, reputation = -1,
                            logKind = "监察", logText = "全市停工一日重验安全",
                        },
                        result = "停工损失算得出，事故损失算不出。有开发商在电话里骂了句“乱来”。你说：乱来的是那根钢管。",
                    },
                },
            });
        }

        // ④ 概算拆分规避审批 → 审计抽查
        static void RegisterBudgetAudit()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_tc_budget", type = "oversight", title = "两笔钱，一个项目",
                when = new When { md = "06-12", fromYear = 2027, requireMarks = new[] { "budget_split" }, requireNotMarks = new[] { "tc_budget_done" } },
                paras = new List<string>
                {
                    "市审计局在抽查中把两笔概算调整放在了同一张纸上：同一项目，相隔 4 个月，分别调增 9.8% 与 9.6%。",
                    "按单次看都在权限内，合起来超了重新报批的门槛。审计意见写得很技术：“建议明确此类情形是否属于一个事项。”",
                    "写这句话的人，其实已经知道答案。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "主动说明：承认拆分成两个事项规避了报批",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_budget_done", "budget_honest" },
                            compliance = 4, risk = -7, political = -1,
                            integrity = new IntegrityRecord { tag = "概算调整", note = "主动说明概算拆分成两个事项" },
                            logKind = "审计", logText = "概算调整主动说明并补报批",
                        },
                        result = "你在说明上签字：“是一个事项，由我签字拆分，责任在我。”补报批程序走了两个月，项目慢了一个季度——但你之后的每一份概算，发改局都会自己先算一遍合计。",
                    },
                    new EventOption
                    {
                        label = "补一个程序说明，往后按此口径执行",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_budget_done" }, compliance = 1, risk = 3, admin = 1,
                            logKind = "审计", logText = "概算拆分补程序说明",
                        },
                        result = "说明写得很体面，把“同一项目分阶段实施”解释透了。审计报告上写了“已整改”。项目继续，规矩也继续模糊着。",
                    },
                    new EventOption
                    {
                        label = "跟审计局沟通，这条不入报告",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_budget_done" }, risk = 11,
                            integrity = new IntegrityRecord { tag = "审计沟通", note = "就概算拆分问题与审计局沟通不入报告" },
                            logKind = "审计", logText = "概算拆分问题不入审计报告",
                        },
                        result = "这条确实没进报告。审计局的老同志把稿子收起来时说：“市长，我们明年还来。”——明年他还来，这条就还在。",
                    },
                },
            });
        }

        // ⑤ 采购质疑“维持原结果” → 投诉到省里
        static void RegisterProcureComplaint()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_tc_procure", type = "society", title = "质疑信到了省里",
                when = new When { md = "07-15", fromYear = 2027, requireMarks = new[] { "procure_hold" }, requireNotMarks = new[] { "tc_procure_done" } },
                paras = new List<string>
                {
                    "采购质疑的企业没有再找市里，直接把材料寄到了省财政厅：原件、评分表复印件、以及一份“质疑答复前后对照”。",
                    "省厅转了回来，附一句话：请说明评分表中“服务方案”一项的评分依据。",
                    "那份评分表你上次只看了结论——总分、排名、中标人。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "调阅全部评分底稿，逐项回复省厅",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_procure_done", "procure_honest" },
                            compliance = 4, efficiency = -2, risk = -6, professional = 1,
                            logKind = "采购", logText = "采购质疑调阅底稿逐项回复",
                        },
                        result = "底稿调上来，问题比质疑的多：两位评委的打分表是同一支笔写的。省厅回复“按规定处理”。你重新组织了评审——中标结果变了，采购中心主任换了岗。",
                    },
                    new EventOption
                    {
                        label = "按采购人答复口径回省厅",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_procure_done" }, risk = 5, compliance = -1,
                            logKind = "采购", logText = "采购质疑按原答复口径上报",
                        },
                        result = "回复发出去了，省厅没有再问。这件事在纸面上结束了——质疑人后来去了外地，材料在省厅留了档。",
                    },
                },
            });
        }

        // ⑥ 信访“程序性销案” → 同一人赴省访
        static void RegisterPetitionEscalate()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_tc_petition", type = "society", title = "他去了省城",
                when = new When { md = "09-12", fromYear = 2027, requireMarks = new[] { "petition_paper" }, requireNotMarks = new[] { "tc_petition_done" } },
                paras = new List<string>
                {
                    "上季度按“已答复”销案的那位上访人，昨天在省信访接待大厅排了七个小时的队。",
                    "省里把件转回来，附了两个字：“属实。”——他反映的安置房漏水，确实是三年前就该修的那栋楼。",
                    "信访局的同志说：“当时答复也没错，程序走完了。”你说：“程序走完了，房子没修。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "亲自见他一面，把三年前的答复重开",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_petition_done", "petition_met" },
                            compliance = 3, efficiency = -2, risk = -5, comm = 2, morale = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "zhoujin", familiar = 2, memo = "市长亲自接访重开旧案" } },
                            logKind = "信访", logText = "市长接访重开三年前旧案",
                        },
                        result = "他来了，带了一个旧保温杯。你说“答复是错的”，他愣了半分钟，然后说：“我不要赔偿，我要那栋楼修好。”三个月后楼顶防水做完了，他又来了一次——这次是送一面锦旗，旗子不大。",
                    },
                    new EventOption
                    {
                        label = "批给区里限期办结并报结果",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_petition_done" }, compliance = 2, efficiency = 1, exec = 1,
                            logKind = "信访", logText = "省转信访件批区里限期办结",
                        },
                        result = "区里第八天报来结果：已组织维修。你让周谨给那位上访人打了个电话核实——电话通了，他说“来了几个工人，看了，说材料要等”。",
                    },
                    new EventOption
                    {
                        label = "维持原答复，另按困难救助处理",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_petition_done" }, risk = 6, moneyDelta = -1,
                            integrity = new IntegrityRecord { tag = "信访处置", note = "省转信访件维持原答复，以困难救助结案" },
                            logKind = "信访", logText = "省转信访以困难救助结案",
                        },
                        result = "救助款批了，案子结了，房子还漏。信访系统里的数字是干净的——干净的数字后面，站着一位还在等材料的老人。",
                    },
                },
            });
        }

        // ⑦ 医保“以整改为主” → 上级检查时同类问题再现
        static void RegisterInsuranceSoft()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_tc_yibao", type = "oversight", title = "同样的三张单子",
                when = new When { md = "10-16", fromYear = 2027, requireMarks = new[] { "yibao_soft" }, requireNotMarks = new[] { "tc_yibao_done" } },
                paras = new List<string>
                {
                    "省医保署交叉检查抽到了上次“以整改为主”的那家医院。同样的三张单子：挂床、过度检查、超量开药。",
                    "检查组组长把两次检查的记录并排放在你面前：“上一次你们处理的是整改。这一次，我们按骗保报。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "支持按骗保处理，并倒查上次为何从轻",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_yibao_done", "yibao_hard" },
                            compliance = 5, efficiency = -2, risk = -7,
                            logKind = "医保", logText = "医保同类问题按骗保处理",
                        },
                        result = "医院院长被免职，两名科室主任移送。你在党组会上讲了上次从轻的理由：怕影响医院运转。这句话说完，会议室里安静了很久。",
                    },
                    new EventOption
                    {
                        label = "配合省检查组，市里不再另行处理",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "tc_yibao_done" }, compliance = 2, risk = -2, political = 1,
                            logKind = "医保", logText = "医保问题交省检查组处理",
                        },
                        result = "把责任交出去最省事，也最简单：上面的处理决定会下来，市里只需执行。你在签批栏写了两个字：“配合”。",
                    },
                },
            });
        }
    }
}
