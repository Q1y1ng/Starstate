using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// Phase 5 · M1 剧情链补全（spec S2.10 的新链 2 与 4）：
    /// ① **安全事故瞒报压力**：涉险事件的口径一旦按下，压力才开始（软口径／如实上报／现场复查三条收束）；
    /// ② **招商引资对赌**：宽条款与严条款各自延展出「再谈一次」与「核数对账」两条线。
    ///
    /// 约定：md 事件每年重现 → 每条收束都带 `requireNotMarks = {本线完成标记}`；
    /// 同一节拍的分叉用不同事件 id + 互斥 marks（族标记由卷宗选项一并写入，见 ContentDossierY2）。
    /// 每个事件必须给足 options（0 个选项会在 VisibleOptions 里触发 options[0] 越界）。
    /// </summary>
    public static class ContentChainsRisk
    {
        public static void Register()
        {
            RegisterSafetyChain();
            RegisterTrustChain();
        }

        // =====================================================================
        // 一、安全事故瞒报压力（safety_soft / safety_honest / safety_probe）
        // =====================================================================

        static void RegisterSafetyChain()
        {
            // ① 软口径路线的回响：办结了，压力才来
            Flow.Register(new GameEvent
            {
                id = "ch_sc_pressure", type = "oversight", title = "再核一遍口径",
                when = new When
                {
                    md = "08-05", fromYear = 2027,
                    requireMarks = new[] { "safety_soft" },
                    requireNotMarks = new[] { "sc_pressure_done" },
                },
                paras = new List<string>
                {
                    "涉险事件按企业自报口径办结的第13天，企业董事长在区里领导陪同下来到市政府。",
                    "他带来一份“统一口径说明”，措辞客气，核心就一句：希望市里维持“无人员伤亡”的表述，理由是“企业正在申报省级龙头，不能有污点”。",
                    "周谨在你耳边补了一句：“三户家属昨天在厂门口坐了一下午，今天早上去了区医院调病历。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "当场改口：立即按条例补报，责任从市里查起",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "sc_pressure_done", "sc_corrected" },
                            compliance = 4, efficiency = -3, stress = 6, political = 1,
                            rel = new List<RelDelta>
                            {
                                new RelDelta { id = "shenyan", trust = 4, evalv = 3, memo = "涉险事故主动改口补报" },
                                new RelDelta { id = "shao", evalv = -3, memo = "涉险事故补报影响季度形象" },
                            },
                            logKind = "监察", logText = "涉险事故：压力下改口补报",
                        },
                        result = "补报材料送上去那天，企业董事长在楼道里没跟你握手。区里领导脸色很难看。但三天后省应急署的回复来了四个字：“知错即改。”——这四个字，值三个人的留观记录。",
                    },
                    new EventOption
                    {
                        label = "维持原口径，但让企业出钱妥善安置家属",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "sc_pressure_done", "sc_covered" },
                            compliance = -4, efficiency = 3, stress = 4,
                            integrity = new IntegrityRecord { tag = "安全生产", note = "涉险事故维持无伤亡口径，由企业私下安置家属" },
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", evalv = -4, memo = "涉险事故口径被家属事件顶翻" } },
                            logKind = "监察", logText = "涉险事故：维持口径+企业私下安置",
                        },
                        result = "家属签了协议，厂门口清了场。一年后省委巡视组的材料里有一行：“某企业事故善后由企业自行处理，政府未履行监管报告职责。”——这行字，落款处没有你的名字，但每个字都从你的签批栏里长出来。",
                    },
                    new EventOption
                    {
                        label = "不表态，让应急局和区里把事实先核实清楚",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "sc_pressure_done", "sc_delayed" },
                            political = 1, stress = 3, efficiency = -1,
                            logKind = "监察", logText = "涉险事故：不表态先核实",
                        },
                        result = "你没接那份“统一口径说明”。董事长临走时把它留在了茶几上——你让周谨把它登记进收文簿。不表态是一种慢，也是一种留痕。",
                    },
                },
            });

            // ② 现场复查路线：复查查出的比企业报的多
            Flow.Register(new GameEvent
            {
                id = "ch_sc_probe_echo", type = "oversight", title = "作业票上的第二个名字",
                when = new When
                {
                    md = "09-18", fromYear = 2027,
                    requireMarks = new[] { "safety_probe" },
                    requireNotMarks = new[] { "sc_probe_done" },
                },
                paras = new List<string>
                {
                    "专家组复查报告出来了：受限空间作业票上共有6个名字，其中3人不在企业的涉险名单里。",
                    "更麻烦的是，作业票的签字日期比气体检测记录晚了两天——补签的痕迹很清楚。",
                    "组长把报告推给你时说了一句：“这就不是涉险了。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按事故重新上报，并移交线索给监委",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "sc_probe_done", "sc_transferred" },
                            compliance = 5, efficiency = -3, stress = 5, political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 5, evalv = 4, memo = "事故线索主动移交监委" } },
                            logKind = "监察", logText = "涉险事件：按事故重报并移交线索",
                        },
                        result = "监委受理那天，区里三个人被叫去谈话。企业换了安全总监。你在批示里只写了一句：“别把安全生产交给运气。”",
                    },
                    new EventOption
                    {
                        label = "由市局按内部管理问题处理，督促企业整改",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "sc_probe_done", "sc_internal" },
                            compliance = -3, efficiency = 3, stress = 3,
                            integrity = new IntegrityRecord { tag = "安全生产", note = "作业票补签问题按内部管理处理未上报" },
                            logKind = "监察", logText = "涉险事件：按内部管理处理",
                        },
                        result = "整改报告写得很好：制度上墙、台账重做、责任人扣奖金。半年后同类补签在一次夜查中被发现——那一次，没有专家组替你先看一眼。",
                    },
                },
            });

            // ③ 如实上报路线：省里认可，市里挨骂
            Flow.Register(new GameEvent
            {
                id = "ch_sc_honest_echo", type = "oversight", title = "省里的四个字",
                when = new When
                {
                    md = "09-10", fromYear = 2027,
                    requireMarks = new[] { "safety_honest" },
                    requireNotMarks = new[] { "sc_honest_done" },
                },
                paras = new List<string>
                {
                    "省应急署通报：大同市涉险事件上报及时、口径准确，作为规范上报的正面案例。",
                    "通报抄送全省。同一天，区里的电话打到了周谨那里：辖区内企业正在申报省级龙头，这一通报“影响很不好”。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "坚持通报不变，另批复一次全区安全生产大检查",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "sc_honest_done", "sc_check_pushed" },
                            compliance = 3, efficiency = 1, stress = 3,
                            admin = 1,
                            rel = new List<RelDelta>
                            {
                                new RelDelta { id = "shenyan", trust = 3, memo = "顶住区里压力坚持通报" },
                                new RelDelta { id = "shao", evalv = -2, memo = "安全生产大检查影响招商进度" },
                            },
                            logKind = "监察", logText = "涉险事件：坚持通报+全区大检查",
                        },
                        result = "大检查查了四十家企业，停了三家。区里骂声一片，但那三家停产的车间里，有两条是受限空间作业——和出事那条一模一样。",
                    },
                    new EventOption
                    {
                        label = "给区里一个台阶：通报照发，另批技改资金帮企业补短板",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "sc_honest_done", "sc_gave_ladder" },
                            compliance = 1, efficiency = 1, comm = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "shao", familiar = 2, trust = 1, memo = "涉险通报后给区里补技改资金" } },
                            logKind = "监察", logText = "涉险事件：通报照发+技改资金台阶",
                        },
                        result = "企业拿到了四百万元安全技改资金，区里的电话再打来时语气软了。台阶给出去，通报的效力就打了一半折——值不值，看你更想要哪一个。",
                    },
                },
            });
        }

        // =====================================================================
        // 二、招商引资对赌（aitrust_weak / aitrust_hard 两个族标记）
        // =====================================================================

        static void RegisterTrustChain()
        {
            // ① 宽条款线：企业来谈“调整”
            Flow.Register(new GameEvent
            {
                id = "ch_ai_weak_2", type = "business", title = "行业周期",
                when = new When
                {
                    md = "12-06", fromYear = 2027,
                    requireMarks = new[] { "aitrust_weak" },
                    requireNotMarks = new[] { "ai_weak2_done" },
                },
                paras = new List<string>
                {
                    "华智提出第二份补充申请：受行业周期影响，申请将固定资产投资强度由每亩260万元下调至180万元，动工期限顺延一年。",
                    "商务局的初核意见写得很客气：“建议予以支持，以维护营商环境。”",
                    "周谨补了一句没写进材料的：“他们的对赌条款，本来就是我们自己写的。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "同意调整，换一份“亩均税收保底”承诺",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_weak2_done", "ai_weak2_traded" },
                            efficiency = 2, compliance = -1, comm = 1,
                            logKind = "卷宗", logText = "对赌：同意下调投资强度换税收保底",
                        },
                        result = "投资强度的事淡了，亩均税收保底写进了补充协议第3条。企业签字那天，法务问了一句：“保底从哪年算？”——这一问，把生效年份又往后推了一年。",
                    },
                    new EventOption
                    {
                        label = "不同意调整，按原协议催告动工",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_weak2_done", "ai_weak2_enforce" },
                            compliance = 3, efficiency = -2, stress = 3,
                            setFlags = new List<string> { "ai_contract_strict" },
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = -2, memo = "对赌催告影响招商氛围" } },
                            logKind = "卷宗", logText = "对赌：不同意调整，催告动工",
                        },
                        result = "催告函送出去，企业沉默了十天，然后动了工——只动了规划的那一半地。另一半长着草，草里插着一块“二期工程”的牌子，牌子的字是新漆的。",
                    },
                    new EventOption
                    {
                        label = "要求先做履约能力审计，再决定是否调整",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_weak2_done", "ai_weak2_audit" },
                            professional = 2, compliance = 2, efficiency = -1, stress = 2,
                            logKind = "卷宗", logText = "对赌：先做履约能力审计",
                        },
                        result = "审计发现企业账面现金只够撑十个月。调整申请于是变成了另一种东西——你手里第一次有了对它说“不”的底气，虽然这一声“不”还压在抽屉里。",
                    },
                },
            });

            // ② 严条款线：第三方核数进场（也挂在同年同日，与宽线互斥）
            Flow.Register(new GameEvent
            {
                id = "ch_ai_hard_2", type = "business", title = "核数进场",
                when = new When
                {
                    md = "12-06", fromYear = 2027,
                    requireMarks = new[] { "aitrust_hard" },
                    requireNotMarks = new[] { "ai_hard2_done" },
                },
                paras = new List<string>
                {
                    "第三方核数机构进场第五天，企业法务找到商务局，提出两件事：核数结论不对外披露；核数费用由市里承担一半。",
                    "机构负责人私下跟你说了一句：“他们的在建工程科目，有一笔两亿的凭证，附件只有一张收据。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "核数结论全文公开，费用由市财政单独承担",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_hard2_done", "ai_audit_open" },
                            compliance = 4, efficiency = -2, stress = 3, reputation = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "rensheng", trust = 2, memo = "核数结论全文公开" } },
                            logKind = "卷宗", logText = "对赌核数：结论全文公开",
                        },
                        result = "公开的核数报告里，那张两亿收据被写成了“凭证不完整”。企业第二天发来律师函，第三天又撤回了。公开的好处是：不用记住自己说过什么。",
                    },
                    new EventOption
                    {
                        label = "结论不公开，但报省发展和改革署备案",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_hard2_done", "ai_audit_report_up" },
                            compliance = 1, efficiency = 2, comm = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = 1, memo = "对赌核数结论报省备案" } },
                            logKind = "卷宗", logText = "对赌核数：报省备案不公开",
                        },
                        result = "省里收了备案，回了“已阅”。企业松了口气，商务局也松了口气。这份结论从此躺在一个更大的柜子里——柜子越深，将来取出来的时候越难看。",
                    },
                    new EventOption
                    {
                        label = "暂不下结论：先让企业补全凭证，下次核数前处理",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_hard2_done", "ai_audit_soften" },
                            efficiency = 3, compliance = -2,
                            integrity = new IntegrityRecord { tag = "招商引资", note = "核数发现凭证不完整，要求企业补正后未再核" },
                            logKind = "卷宗", logText = "对赌核数：令企业补正后暂结",
                        },
                        result = "凭证在两周内补了七份，那张两亿收据被换成了一份合同。核数机构在补充说明里写了四个字：“已补正。”——补正和真实的距离，是这行字测不出来的。",
                    },
                },
            });

            // ③ 宽条款线的收束：二次兑现（低完成率 + 再次延期）
            Flow.Register(new GameEvent
            {
                id = "ch_ai_weak_pay2", type = "oversight", title = "对赌第三年",
                when = new When
                {
                    md = "09-25", fromYear = 2029,
                    requireMarks = new[] { "aitrust_weak", "ai_weak2_done" },
                    requireNotMarks = new[] { "ai_weak_pay2_done" },
                },
                paras = new List<string>
                {
                    "对赌第三年评估：投资完成率58%，亩均税收完成率41%。按现行条款，收回上限30%，且触发条件需双方共同确认。",
                    "企业再次提出延期两年，并附了一句很少见的话：“如市里需要，可配合调整统计口径。”",
                    "方启年把评估表放在你桌上：“市长，这句话我念了三遍。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "终止协议，依法收回并公开评估全过程",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_weak_pay2_done", "ai_terminated" },
                            compliance = 4, efficiency = -3, stress = 5, political = 1,
                            reputation = 2,
                            logKind = "卷宗", logText = "对赌：终止协议并公开评估",
                        },
                        result = "终止公告发布的那天，市里少了一个“在谈大项目”，多了一份公开的评估报告。下一任招商局长谈项目时，会把这份报告放在手边——不是为了学习，是为了引用。",
                    },
                    new EventOption
                    {
                        label = "再延一年，但把土地按闲置处置、收回未动工部分",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_weak_pay2_done", "ai_half_reclaim" },
                            compliance = 2, efficiency = 1, stress = 3,
                            admin = 1,
                            logKind = "卷宗", logText = "对赌：延期一年+收回未动工土地",
                        },
                        result = "两宗未动工的地收了回来，重新挂牌成交价高出原价一成。企业保住了面子，市里拿回了里子——这种两全，通常意味着有一方还没算清账。",
                    },
                    new EventOption
                    {
                        label = "接受“配合调整统计口径”，换取补足投资",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_weak_pay2_done", "ai_soft_deal" },
                            efficiency = 3, compliance = -5, stress = 4,
                            integrity = new IntegrityRecord { tag = "招商引资", note = "对赌未达标，以调整统计口径换取补足投资" },
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", evalv = -5, memo = "对赌口径被企业牵着走" } },
                            logKind = "监察", logText = "对赌：以统计口径换补足投资",
                        },
                        result = "第二年报表上的完成率变成了89%。省里对账时，口径说明那一栏写着“双方共同确认”。方启年看完报表什么也没说，把计算器按了三遍。",
                    },
                },
            });

            // ④ 严条款线的收束：核数之后的对账
            Flow.Register(new GameEvent
            {
                id = "ch_ai_hard_pay", type = "oversight", title = "对赌对账",
                when = new When
                {
                    md = "09-25", fromYear = 2029,
                    requireMarks = new[] { "aitrust_hard", "ai_hard2_done" },
                    requireNotMarks = new[] { "ai_hard_pay_done" },
                },
                paras = new List<string>
                {
                    "对赌第三年。第三方核数：投资完成率96%，亩均税收完成率88%——接近达标，差的一点点正好卡在“投资强度口径”的争议上。",
                    "企业这次先开了口：“按原口径我们认。但请市里在下一轮用地指标上给个说法。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按条款执行，收回上限50%，并要求重新谈判二期",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_hard_pay_done", "ai_hard_collect" },
                            compliance = 4, efficiency = -1, political = 2, stress = 3,
                            polCapital = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", evalv = 2, memo = "大同对赌执行到位" } },
                            logKind = "卷宗", logText = "对赌：按硬条款收回上限50%",
                        },
                        result = "收回的一亿一千万进了专户。省里把这个案子写进了营商环境简报的“契约精神”栏——同一栏里，另一个市的案例是“三年四次延期”。",
                    },
                    new EventOption
                    {
                        label = "认定为达标，兑现政策并把它做成正面案例",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_hard_pay_done", "ai_hard_praise" },
                            efficiency = 3, reputation = 3, political = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = 2, memo = "对赌达标作正面案例" } },
                            logKind = "系统", logText = "对赌：认定为达标并作正面案例",
                        },
                        result = "现场会开在园区里，横幅拉了三道。会上记者问“亩均税收88%算什么水平”，邵志远接话说“全市第一”。——全市第一个履约的企业，同时也是全市唯一一个签约的。",
                    },
                    new EventOption
                    {
                        label = "达标但不宣传：兑现政策，另附一份履约评估底稿备查",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ai_hard_pay_done", "ai_hard_quiet" },
                            compliance = 3, efficiency = 1, professional = 1,
                            logKind = "卷宗", logText = "对赌：达标兑现+底稿备查",
                        },
                        result = "没有横幅，没有现场会，只有一份进柜的底稿。第二年省里交叉检查抽到这份底稿，翻了四十分钟，最后在意见栏写了两个字：“完整。”",
                    },
                },
            });
        }
    }
}
