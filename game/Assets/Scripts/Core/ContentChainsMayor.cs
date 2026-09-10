using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// Phase 5 市长层剧情链：对上报告、算法审批（承接高光卷宗 marks 的完整分叉）。
    /// </summary>
    public static class ContentChainsMayor
    {
        public static void Register()
        {
            RegisterReportChain();
            RegisterAlgoChain();
            RegisterRiskPayoffs();
        }

        /// <summary>高风险卷宗 marks 的延迟回响（煤电差额 / 大气降级 / 对赌弱条款）。</summary>
        static void RegisterRiskPayoffs()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_coal_slush", type = "oversight", title = "安置资金抽查",
                when = new When { requireMarks = new[] { "coal_slush" }, md = "06-20", fromYear = 2027 },
                paras = new List<string>
                {
                    "省审计组延伸审计专项资金：云冈矿区安置资金。528万差额在凭证里对不上用途。",
                    "方启年在你办公室坐了二十分钟，没喝完那杯茶。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "主动说明并追回差额",
                        effects = new Effects
                        {
                            stress = 5, political = 1,
                            integrity = new IntegrityRecord { tag = "专项资金", note = "安置差额528万主动说明并追回" },
                            logKind = "监察", logText = "安置差额：主动说明追回",
                        },
                        result = "追回手续办了两个月。矿区的人不知道那528万——他们只知道安置金到账了。有些补救没有掌声，只有账本。",
                    },
                    new EventOption
                    {
                        label = "以“应急周转”补一份说明材料",
                        effects = new Effects
                        {
                            stress = 3,
                            integrity = new IntegrityRecord { tag = "专项资金", note = "安置差额事后补说明" },
                            logKind = "监察", logText = "安置差额：事后补说明",
                        },
                        result = "说明材料写得很圆。审计组收了，没表扬，也没再问。圆的东西，经不起第二次打开。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "ch_air_yellow", type = "oversight", title = "约谈",
                when = new When { requireMarks = new[] { "air_yellow" }, md = "12-05", fromYear = 2026 },
                paras = new List<string>
                {
                    "省生态环境署约谈：重污染过程降级响应，被点名“响应级别与监测数据不符”。",
                    "约谈室的茶是凉的。你面前的材料里，夹着那页有铅笔字的附件复印件。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "如实说明决策过程，接受通报",
                        effects = new Effects
                        {
                            stress = 6, morale = -2,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", evalv = -2, memo = "应急响应被省里约谈" } },
                            logKind = "监察", logText = "大气响应约谈：如实说明",
                        },
                        result = "通报会抄送全市。岑伯衡在走廊上没停步——不停步，有时候比批评更冷。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "ch_air_orange", type = "work", title = "回头看",
                when = new When { requireMarks = new[] { "air_orange" }, md = "05-12", fromYear = 2027 },
                paras = new List<string>
                {
                    "省里空气质量“回头看”：大同橙色响应执行到位，作为正面案例简报。",
                    "邵志远把简报放在你桌上，没说话。那半格领带，已经系正了。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "批示：固化应急标准，不搞一阵风",
                        effects = new Effects
                        {
                            professional = 1, reputation = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = 2, memo = "橙色响应经得起回头看" } },
                            logKind = "卷宗", logText = "大气应急：固化标准",
                        },
                        result = "标准文件印发那天，天是蓝的。蓝不常有——所以才要写成纸。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "ch_aitrust_weak", type = "work", title = "兑现率",
                when = new When { requireMarks = new[] { "aitrust_weak" }, md = "09-10", fromYear = 2028 },
                paras = new List<string>
                {
                    "华智区域总部第三年评估：投资完成率71%，税收完成率64%。按对赌，收回上限30%。",
                    "企业法务拿着第4条和第7条，一条一条念。你听得很清楚——条款是他们起草的版本。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按上限收回，并公开评估报告",
                        effects = new Effects
                        {
                            political = 1, stress = 3,
                            logKind = "卷宗", logText = "华智对赌：按上限收回并公开",
                        },
                        result = "收回的数字不大，公开的动作不小。下一任市长谈判时，会把这份报告放在手边。",
                    },
                    new EventOption
                    {
                        label = "协商延期，不公开报告",
                        effects = new Effects
                        {
                            comm = 1,
                            integrity = new IntegrityRecord { tag = "招商引资", note = "对赌未达标协商延期且不公开" },
                            logKind = "卷宗", logText = "华智对赌：协商延期",
                        },
                        result = "新闻稿用了“深化合作、共克时艰”。附件里那两个完成率，没有出现在任何通稿里。",
                    },
                },
            });
        }

        // ---------------- 对上报告：pressed_report / honest_report ----------------

        static void RegisterReportChain()
        {
            // 如实上报 → 省里对账表扬 + 岑伯衡认可（延迟 45 天）
            Flow.Register(new GameEvent
            {
                id = "ch_rep_honest", type = "oversight", title = "省署的对账电话",
                when = new When { requireMarks = new[] { "honest_report" }, md = "11-20", fromYear = 2026 },
                paras = new List<string>
                {
                    "省发展和改革署投资处的电话：“大同同志，你们九月那份招商报告，我们按硬签约重新对了一遍——89%。数字不好看，但对得上。”",
                    "电话那头顿了顿：“有些市报的是110%，我们正在一家一家对。你们这种，省心。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把这份“省心”记进案头",
                        effects = new Effects
                        {
                            morale = 3, reputation = 2, political = 2,
                            setMarks = new List<string> { "rep_honest_paid" },
                            rel = new List<RelDelta> { new RelDelta { id = "cen", evalv = 3, memo = "对上数字经得起省里对账" } },
                            logKind = "卷宗", logText = "省署对账：大同数字经得起查",
                        },
                        result = "岑伯衡在楼道里碰到你，只说了一句：“数，要对得上。”——他很少表扬人，这已经是表扬。",
                    },
                },
            });

            // 含水分上报 → 年末省审计抽查（延迟触发）
            Flow.Register(new GameEvent
            {
                id = "ch_rep_pressed", type = "oversight", title = "抽查通知",
                when = new When { requireMarks = new[] { "pressed_report" }, md = "12-10", fromYear = 2026 },
                paras = new List<string>
                {
                    "市审计局转来省审计组通知：将抽查三个设区市“新签约项目”口径，大同在列。",
                    "方启年站在你办公室门口，手里捏着那份清单——第14行“某某区域总部”，状态仍是“签约”，备注空白。",
                    "省里要的是合同原件。框架协议锁在招商局抽屉里，锁不住审计。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "连夜组织补材料，口径统一到“以正式合同为准”",
                        effects = new Effects
                        {
                            stress = 6, exec = 2,
                            setMarks = new List<string> { "rep_pressed_scramble" },
                            integrity = new IntegrityRecord { tag = "对上口径", note = "招商完成率含框架协议，省审计抽查前补正" },
                            logKind = "监察", logText = "省审计抽查：补正招商口径",
                        },
                        result = "三天三夜。最终报上去的是92%。省审计组的同志合上底稿说：“以后一开始就按这个口径。”——语气平静，像在教你做题。",
                    },
                    new EventOption
                    {
                        label = "请示岑主席，争取从轻口径",
                        effects = new Effects
                        {
                            stress = 4, political = -2,
                            rel = new List<RelDelta> { new RelDelta { id = "cen", evalv = -4, memo = "招商数字被抽查才来找我" } },
                            integrity = new IntegrityRecord { tag = "对上口径", note = "招商数字水分问题上报前寻求口径庇护" },
                            logKind = "监察", logText = "对上报告问题请示主席",
                        },
                        result = "岑伯衡听完，把茶杯放下：“政府报出去的数，主席能改吗？”——他没有骂你。不骂，比骂更冷。",
                    },
                },
            });

            // 补正/被查后的回响：同类报告找你把关（两条路径各挂一次，互斥 marks）
            RegisterRepGate("ch_rep_gate", "rep_honest_paid");
            RegisterRepGate("ch_rep_gate2", "rep_pressed_scramble");
        }

        static void RegisterRepGate(string id, string mark)
        {
            Flow.Register(new GameEvent
            {
                id = id, type = "work", title = "再来一份对上报告",
                when = new When { requireMarks = new[] { mark }, md = "03-15", fromYear = 2027 },
                paras = new List<string>
                {
                    "又到季度报送。邵志远把初稿放在你桌上：“这次产业口的数，你亲自看一眼。”",
                    "你知道他在看你——看的是你上一次怎么处理口径。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按硬签约口径核到底，该多少是多少",
                        effects = new Effects
                        {
                            professional = 2, stress = 3,
                            setMarks = new List<string> { "rep_gate_clean" },
                            logKind = "卷宗", logText = "季度报告：坚持硬签约口径",
                        },
                        result = "邵志远签了字，没再争。有些规矩立过一次，后面就省很多口舌。",
                    },
                    new EventOption
                    {
                        label = "给产业口留一点“弹性表述”",
                        effects = new Effects
                        {
                            comm = 1, stress = 1,
                            setMarks = new List<string> { "rep_gate_soft" },
                            integrity = new IntegrityRecord { tag = "对上口径", note = "季度报告保留弹性表述" },
                            logKind = "卷宗", logText = "季度报告：弹性表述",
                        },
                        result = "省里没追问。没追问不等于没事——弹性这东西，会回弹。",
                    },
                },
            });
        }

        // ---------------- 算法审批：algo_auto / algo_pass / algo_veto ----------------

        static void RegisterAlgoChain()
        {
            // 人工终审 → 操作手册收录（半年后）
            Flow.Register(new GameEvent
            {
                id = "ch_algo_manual", type = "work", title = "操作手册上的批注",
                when = new When { requireMarks = new[] { "algo_pass" }, md = "03-01", fromYear = 2027 },
                paras = new List<string>
                {
                    "市城市管理局报送《AI辅助审批操作手册（试行）》。翻开第2页，你去年在协议第5条旁的批注被印成了黑体：",
                    "「算法可提供意见，不得代替法定权力主体作最终决定。审批通过，必须有人签字，有人负责。」",
                    "下面还有一行小字：借鉴大同市人民政府领导批示。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "批示：照此执行，季度抽查人工复核台账",
                        effects = new Effects
                        {
                            professional = 2, reputation = 2, political = 1,
                            setMarks = new List<string> { "algo_manual_guard" },
                            logKind = "卷宗", logText = "AI操作手册：人工终审+季度抽查",
                        },
                        result = "邵志远在常务会上说：“大同这套，省里在看。”效率与责任，终于站在了同一张桌子上——暂时。",
                    },
                },
            });

            // 自动审批 → 出事（次年春）
            Flow.Register(new GameEvent
            {
                id = "ch_algo_blow", type = "oversight", title = "是谁批的",
                when = new When { requireMarks = new[] { "algo_auto" }, md = "04-08", fromYear = 2027 },
                paras = new List<string>
                {
                    "临时占道审批出了一起纠纷：系统自动“通过”了一份本应退回的申请，商户把摊子支到了消防通道上。",
                    "市监委的问询函放在你桌上，只有一行核心问题：请说明该事项法定审核责任人。",
                    "卷宗上的签字栏是窗口人员的章。系统日志——只有6个月，刚好覆盖不到训练数据偏差那个月。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "主动报告：试点设计责任在市政府，启动整改",
                        effects = new Effects
                        {
                            stress = 8, political = 1,
                            setMarks = new List<string> { "algo_blow_own" },
                            integrity = new IntegrityRecord { tag = "算法审批", note = "自动审批试点出现责任真空，主动担责整改" },
                            logKind = "监察", logText = "AI试点责任事故：主动报告",
                        },
                        result = "沈砚在反馈上写了四个字：“态度端正。”——端正不能抵处分，但能决定处分的深浅。系统改为人工终审，你亲自改的协议。",
                    },
                    new EventOption
                    {
                        label = "按协议由城管局承担，市政府只负监管之责",
                        effects = new Effects
                        {
                            stress = 5,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", evalv = -5, memo = "算法事故责任下移" } },
                            integrity = new IntegrityRecord { tag = "算法审批", note = "试点事故责任推给牵头部门" },
                            logKind = "监察", logText = "AI事故：责任归城管局",
                        },
                        result = "城管局局长在你办公室坐了四十分钟，走的时候背是直的。协议第5条写得清楚——清楚，有时候是刀。",
                    },
                },
            });

            // 否决 → 邻市对比压力
            Flow.Register(new GameEvent
            {
                id = "ch_algo_veto_pay", type = "work", title = "邻市的效率榜",
                when = new When { requireMarks = new[] { "algo_veto" }, md = "06-15", fromYear = 2027 },
                paras = new List<string>
                {
                    "省政务简报：邻市试点“智能审批”，高频事项办理时限压缩60%。大同未列其中。",
                    "邵志远把简报复印件放在你桌上，没说话。复印件边缘有指甲掐出的印子。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "启动“辅助审批、人工终审”本地版，不买全自动",
                        effects = new Effects
                        {
                            professional = 1, exec = 2,
                            setMarks = new List<string> { "algo_veto_rebuild" },
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = 2, memo = "否决后仍推进可控智能化" } },
                            logKind = "卷宗", logText = "启动辅助审批本地版（人工终审）",
                        },
                        result = "慢了半年，但签字栏还是人名。省里来调研时，你把责任条款单独打了一份给他们看。",
                    },
                    new EventOption
                    {
                        label = "坚持不搞，把精力放在流程本身",
                        effects = new Effects
                        {
                            admin = 1, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = -3, memo = "拒绝任何智能化试点" } },
                            logKind = "卷宗", logText = "否决智能化跟进",
                        },
                        result = "效率榜上大同继续靠后。靠后的名次有人议论，落下的责任没有人替你扛。",
                    },
                },
            });
        }
    }
}
