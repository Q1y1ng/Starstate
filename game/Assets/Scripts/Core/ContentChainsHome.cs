using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// M2 · 旧链迁移（spec S2.10）：把科员线的两条生活链语义升到市长层。
    ///
    /// ① **周转房 / 家属院与配偶经商**：妻子林晚在市设计院（事业单位）。机构改革转企改制之后，
    ///    她所在单位会与政府发生业务往来——于是每年那份《配偶从业情况》报表，就从“填个表”
    ///    变成了“要不要在同一张纸上写出利益冲突”。
    /// ② **医疗资源打招呼**：父亲体检查出肺部结节。市一院院长是熟人，专家号、加急、单间都能安排。
    ///    人情不是当场结账的——它会在某一份设备采购请示上找回来。
    ///
    /// 两条链都直通风险账本（risk），并且都能写成程序合规记录（integrity → 程序违规），
    /// 因此它们不只是情感戏，也是 M3「立案审查」结局的现实来路之一。
    /// </summary>
    public static class ContentChainsHome
    {
        public static void Register()
        {
            RegisterSpouseChain();
            RegisterMedicalChain();
        }

        // =====================================================================
        // 一、周转房 / 家属院与配偶经商
        // =====================================================================

        static void RegisterSpouseChain()
        {
            // ① 年度报表：一次小小的笔误选择
            Flow.Register(new GameEvent
            {
                id = "ch_home_1", type = "family", title = "配偶从业情况申报表",
                when = new When { md = "10-09", fromYear = 2026, requireNotMarks = new[] { "home_chain_done" } },
                paras = new List<string>
                {
                    "组织部送来一份表：《领导干部配偶、子女及其配偶从业情况申报表》。每年都填，今年多了两栏。",
                    "林晚那个设计院下个月完成转企改制，名字要改成“云中设计集团”。新集团已经出现在市里的市政设计服务采购名单上。",
                    "她在电话里说得很平静：“按实际的写就行，我又不管投标。”——她确实不管。但表上那一栏问的不是她管不管。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "逐栏写实：转企、业务范围、可能涉及市政采购",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "home_declared", "home_honest" },
                            compliance = 3, risk = -4, morale = -3, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "linwan", trust = 3, familiar = 1, memo = "配偶从业如实申报" } },
                            logKind = "家风", logText = "配偶从业情况如实申报（含转企与业务范围）",
                        },
                        result = "表交上去，组织部同志多看了两眼：“写这么细？”你说：“细一点，以后省事。”那天晚上林晚没提这事，只把台灯调暗了一格——家里那股气，从来不明着来。",
                    },
                    new EventOption
                    {
                        label = "写“事业单位技术人员”，避开改制字样",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "home_declared", "home_vague" },
                            compliance = -1, risk = 6, morale = 2,
                            integrity = new IntegrityRecord { tag = "个人事项申报", note = "配偶从业情况申报未写明转企改制" },
                            logKind = "家风", logText = "配偶从业情况按“事业单位技术人员”申报",
                        },
                        result = "表填得很干净，四个字就把改制挡在了外面。林晚那晚做了两个菜。你知道这两个菜是给谁做的——也知道自己刚才省下的是什么。",
                    },
                    new EventOption
                    {
                        label = "先给老康打个电话，问问有没有办法调岗",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "home_declared", "home_vague" },
                            compliance = -1, risk = 4, morale = 1,
                            political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "laokang", familiar = 2, trust = 1, memo = "为配偶岗位打过电话" } },
                            logKind = "家风", logText = "就配偶岗位向组织部老康打招呼",
                        },
                        result = "老康在电话里停了三秒：“老沈，这种事你让我怎么说呢。”最后他说“我问问”，然后就没了下文。电话挂断后你坐了一会儿——你刚才打的这个电话，本身就是一条记录。",
                    },
                },
            });

            // ② 回响：抽查 / 家里的对话 / 转企后的第一标
            Flow.Register(new GameEvent
            {
                id = "ch_home_2", type = "family", title = "转企后的第一标",
                when = new When { md = "01-22", fromYear = 2027, requireMarks = new[] { "home_vague" }, requireNotMarks = new[] { "home_chain_done" } },
                paras = new List<string>
                {
                    "云中设计集团成立四个月，中标了市里一个片区改造的设计服务，金额不大，程序走得很正。",
                    "组织部在这个月做了个人事项抽查，抽到 12 人，其中包括你。抽查通知上写着：请就配偶从业情况作进一步说明。",
                    "林晚把中标公告打印出来放在餐桌上：“你要不要看一下？我们公司昨天中的。”她的语气像是在说别人的事。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "补报说明，并主动申请回避该集团相关事项",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "home_chain_done", "home_clean" },
                            compliance = 4, risk = -10, morale = 1, political = 1,
                            integrity = new IntegrityRecord { tag = "回避事项", note = "就配偶所在企业申请回避相关事项" },
                            rel = new List<RelDelta> { new RelDelta { id = "linwan", trust = 4, familiar = 2, memo = "主动申请回避设计集团事项" } },
                            logKind = "家风", logText = "补报配偶从业并主动申请回避",
                        },
                        result = "回避申请由市政府办转给了分管副市长：凡涉及云中设计集团及其关联企业的事项，你一律不签。林晚看到那份备案时说：“这样也好，我不用每天想你会不会看到我们的标书。”——这句话比表上任何一栏都重。",
                    },
                    new EventOption
                    {
                        label = "补报说明，但注明“未参与、不知情”",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "home_chain_done" }, compliance = 1, risk = 2, morale = 1,
                            logKind = "家风", logText = "补报配偶从业（注明未参与）",
                        },
                        result = "说明写完了，抽查这一关过去了。凡事只要“未参与、不知情”六个字写得上，程序上就过得去。至于那六个字背后的日子怎么过，只有你和林晚知道。",
                    },
                    new EventOption
                    {
                        label = "请组织部的同志“把这条压一压”",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "home_chain_done" }, risk = 14, political = -1,
                            integrity = new IntegrityRecord { tag = "个人事项申报", note = "请托对个人事项抽查线索不予深查" },
                            logKind = "家风", logText = "请托压下个人事项抽查线索",
                        },
                        result = "对方答应了“按程序办”——这四个字有很多种意思，这一次的意思你听懂了。抽查结论是“已说明清楚”。你把那张纸夹进文件夹时，夹得比平时用力。",
                    },
                },
            });

            // ③ 结局回响（走灰区且后续再出问题）
            Flow.Register(new GameEvent
            {
                id = "ch_home_3", type = "oversight", title = "一张标书封面",
                when = new When { md = "11-05", fromYear = 2029, requireMarks = new[] { "home_vague" }, requireNotMarks = new[] { "home_chain_done", "home_late_done" } },
                paras = new List<string>
                {
                    "省审计组抽查市政设计服务采购，调阅了云中设计集团近三年的投标文件。",
                    "其中一份标书封面的项目负责人一栏，是林晚的名字。那是一个她确实做过技术负责的项目——你说不知情，但你的签批在另一份关联材料上。",
                    "审计组的问询函只有一句话：请说明是否存在应当回避而未回避的情形。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "如实说明当时未申报完整，接受组织处理",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "home_late_done" },
                            compliance = 2, risk = -6, political = -2, morale = -2,
                            integrity = new IntegrityRecord { tag = "回避事项", note = "未按期申报配偶从业完整信息，接受组织处理" },
                            logKind = "家风", logText = "未完整申报配偶从业，接受组织处理",
                        },
                        result = "处理是“谈话提醒、责令作出说明”。林晚陪你写说明写到后半夜，写完她说：“以后每年那张表，我来盯着你填。”",
                    },
                    new EventOption
                    {
                        label = "解释为技术负责不涉及经营决策",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "home_late_done" }, risk = 12,
                            integrity = new IntegrityRecord { tag = "回避事项", note = "就配偶参与投标情形作出解释，未主动回避" },
                            logKind = "家风", logText = "就配偶参与投标作解释说明",
                        },
                        result = "解释写得很专业：技术负责、无经营决策权、金额占比极小。审计组收了材料，没有结论。这份没有结论的问询函，被归进了省审计厅的底稿里。",
                    },
                },
            });
        }

        // =====================================================================
        // 二、医疗资源打招呼（父亲体检）
        // =====================================================================

        static void RegisterMedicalChain()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_med_1", type = "family", title = "父亲的体检报告",
                when = new When { md = "03-14", fromYear = 2027, requireNotMarks = new[] { "med_chain_done" } },
                paras = new List<string>
                {
                    "父亲的体检报告出来了：右肺上叶结节，1.4 厘米，建议进一步检查。报告上“建议”两个字后面印着“尽快”。",
                    "母亲在电话里说得很慢，把每一句都说了两遍。你一边听一边翻手机通讯录——市第一医院的院长方正，三年前市里给他解决过院区用地的事。",
                    "门诊专家号最快是三周后。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按普通流程排队，三周后看专家",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "med_wait" },
                            compliance = 3, risk = -3, stress = 6, morale = -2,
                            rel = new List<RelDelta> { new RelDelta { id = "linwan", familiar = 1, memo = "公公看病没有打招呼" } },
                            logKind = "家风", logText = "父亲就医按普通流程排队",
                        },
                        result = "那三周里你在办公室坐着，会忽然想起报告上那两个字。号挂上了，专家看完说：“一期，切掉就行。”——你在走廊里给母亲打电话，说了两遍“没事”。",
                    },
                    new EventOption
                    {
                        label = "给方正打个电话，只问一句“能不能加急”",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "med_favor" },
                            compliance = -1, risk = 5, stress = -3, morale = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "fang", familiar = 3, trust = 2, memo = "为父亲加急看过病" } },
                            logKind = "家风", logText = "为父亲就医向市一院院长打招呼",
                        },
                        result = "方正说得极干脆：“明天上午来，我安排。”第二天父亲进了特需门诊，专家、加急、单人病房一路顺畅。母亲逢人就说“儿子有办法”。你站在病房外的走廊上，第一次觉得“有办法”三个字这么刺耳。",
                    },
                    new EventOption
                    {
                        label = "走医院正常加急通道，费用全自付并要求留据",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "med_paid" },
                            compliance = 2, risk = -1, stress = 2, moneyDelta = -1,
                            rel = new List<RelDelta> { new RelDelta { id = "fang", familiar = 1, memo = "父亲就医走正规加急并全自付" } },
                            logKind = "家风", logText = "父亲就医走正规加急并全额自付",
                        },
                        result = "特需服务的费用你按全价交了，收据夹在笔记本里。方正听说后笑了一下：“老沈，你这也太仔细。”你说：“仔细点，睡得着。”",
                    },
                },
            });

            // 回响：人情在某一份请示上找回来
            Flow.Register(new GameEvent
            {
                id = "ch_med_2", type = "oversight", title = "方正院长的第三份请示",
                when = new When { md = "08-19", fromYear = 2027, requireMarks = new[] { "med_favor" }, requireNotMarks = new[] { "med_chain_done" } },
                paras = new List<string>
                {
                    "市一院送来今年第三份设备采购请示：一台 3.0T 磁共振，单一来源，预算比同类医院上年的采购价高出一成。",
                    "随文附了一张便签，是方正的字：“父亲术后复查，随时可来，不必挂号。”便签折得很小，压在第一页下面。",
                    "这台设备在你父亲做检查的那个科室。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按程序办：要求比价并重新论证单一来源",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "med_chain_done", "med_clean" },
                            compliance = 4, risk = -8, political = -1,
                            rel = new List<RelDelta> { new RelDelta { id = "fang", trust = -3, evalv = -2, memo = "设备采购未予照顾" } },
                            logKind = "家风", logText = "医院设备采购要求比价与重新论证",
                        },
                        result = "比价后省下四百多万，单一来源改成公开招标。方正没有再提复查的事，也没有再递便签。你父亲术后复查那年是在普通门诊排的队——你自己陪他去的，排了两个小时。",
                    },
                    new EventOption
                    {
                        label = "同意采购，但把单一来源改为竞争性谈判",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "med_chain_done" }, compliance = 1, risk = 3, political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "fang", familiar = 1, memo = "设备采购改竞争性谈判放行" } },
                            logKind = "家风", logText = "医院设备采购改竞争性谈判放行",
                        },
                        result = "程序上说得过去：谈判也是竞争。方正很满意，复查的号一直给你留着。便签你夹进了文件夹，没扔——留着的意思，你心里清楚。",
                    },
                    new EventOption
                    {
                        label = "照准单一来源，签字了事",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "med_chain_done" }, compliance = -2, risk = 12,
                            integrity = new IntegrityRecord { tag = "采购审批", note = "医院设备单一来源采购未予比价即照准" },
                            logKind = "家风", logText = "医院设备单一来源采购照准",
                        },
                        result = "签字很快，快得像在还一笔早就该还的账。设备装好了，科室换牌那天方正给你发了张照片，你回了个“好”。",
                    },
                },
            });
        }
    }
}
