using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>Phase 5 M0：教学卷宗 3 件＋高光卷宗 3 件（手写，文风圣经）。</summary>
    public static class ContentDossierM0
    {
        public static void Register()
        {
            RegisterTeaching();
            RegisterShowcase();
            RegisterPoolSamples();
        }

        static void RegisterTeaching()
        {
            // —— 教学件 1：数字勾稽 ——
            DossierEngine.Register(new Dossier
            {
                id = "dz_t1",
                kind = "showcase",
                form = "财政件",
                title = "关于追加云中老工业区搬迁专项资金的请示",
                docNo = "同财〔2026〕87号",
                org = "市财政局",
                deadline = "2026-09-04",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "根据《云中老工业区搬迁改造实施方案》，现就第三批搬迁专项资金申请追加如下：",
                            "一、项目概况。老工业区涉及企业11户、职工安置约2400人，搬迁周期2026—2028年。",
                            "二、资金需求。第三批申请追加人民币4800万元，用于职工安置补偿与厂区拆除。",
                            "三、已到位资金。第一批、第二批合计已拨付1.12亿元。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：资金测算表",
                        table = "项目|金额（万元）|备注\n职工安置补偿|3200|按2400人测算\n厂区拆除|1200|中标价\n不可预见费|400|—\n合计|4800|—",
                        paras = new List<string>
                        {
                            "（测算表由市财政局经济建设科编制，科长已核。）",
                        },
                    },
                    new DossierPage
                    {
                        title = "签批栏",
                        paras = new List<string>
                        {
                            "来文单位意见：情况属实，请市政府审批。",
                            "分管副秘书长拟办：请财政局再压一压不可预见费。呈市长阅批。",
                            "——空白——",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_t1_i1",
                        pageRef = "2",
                        detectHint = "对合计与分项：3200+1200+400",
                        ruleKey = "rule_shuzi",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准，转财政局执行",
                        complianceDelta = -2,
                        efficiencyDelta = 4,
                        effects = new Effects
                        {
                            exec = 1,
                            setFlags = new List<string>{ "t1_passed_as_is" },
                            logKind = "卷宗", logText = "照准搬迁专项资金请示",
                        },
                        result = "你签了“同意，请按规定办理”。笔尖离开纸面时很轻。第2页那处加法错误——如果你没看见——已经变成了市政府的意志。",
                    },
                    new DossierOption
                    {
                        label = "退回财政局：测算表合计有误，重报",
                        complianceDelta = 3,
                        efficiencyDelta = -1,
                        effects = new Effects
                        {
                            professional = 1,
                            setMarks = new List<string>{ "careful_on_numbers" },
                            rel = new List<RelDelta>{ new RelDelta{ id="fang", evalv = -2, memo="市长核出了测算表加法错误" } },
                            logKind = "卷宗", logText = "退回：资金测算合计有误",
                        },
                        result = "你在签批栏写：“合计与分项不符，请复核后重报。”方启年半小时后打来电话，声音很稳，也很慢：“市长，我们马上改。”——他知道你看了第2页。",
                    },
                    new DossierOption
                    {
                        label = "请示岑主席后再定",
                        complianceDelta = 1,
                        efficiencyDelta = -2,
                        gray = true,
                        effects = new Effects
                        {
                            political = 1,
                            rel = new List<RelDelta>{ new RelDelta{ id="cen", familiar = 2, memo="资金件请示了主席" } },
                            setMarks = new List<string>{ "deferred_to_cen" },
                            logKind = "卷宗", logText = "搬迁资金件请示主席",
                        },
                        result = "岑伯衡听完只说：“政府职权范围内的事，你定。定之前，把数看对。”——他没有帮你兜，他只要你别把他也拖进错的数里。",
                    },
                },
            });

            // —— 教学件 2：程序瑕疵 ——
            DossierEngine.Register(new Dossier
            {
                id = "dz_t2",
                kind = "routine",
                form = "请示",
                title = "关于举办“云中·智算”产业推介会的请示",
                docNo = "同发改〔2026〕31号",
                org = "市发展和改革局",
                deadline = "",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "为落实省委、市委关于人工智能产业培育的部署，拟于9月下旬举办“云中·智算”产业推介会。",
                            "一、时间地点。9月26日，云中国际会展中心。",
                            "二、规模。拟邀请企业80家、媒体20家。",
                            "三、经费。会务经费约85万元，由市发改局部门预算列支。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：经费明细",
                        table = "项目|金额（万元）\n场地|18\n餐饮|22\n宣传|30\n其他|15\n合计|85",
                        paras = new List<string>{ "（无领导签批页。）" },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_t2_i1",
                        pageRef = "2",
                        detectHint = "越级行文：应经分管副市长或办公厅核转；且缺签批页",
                        ruleKey = "rule_xingwen",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准",
                        complianceDelta = -3,
                        efficiencyDelta = 3,
                        effects = new Effects { logKind="卷宗", logText="照准推介会请示（未补程序）" },
                        result = "“同意。”两个字落下去。程序的洞不会立刻见光——但办公厅的收文登记里，会留下一笔：直报市长。",
                    },
                    new DossierOption
                    {
                        label = "退回：按程序经分管副市长核转，并补签批页",
                        complianceDelta = 4,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            admin = 1,
                            setMarks = new List<string>{ "insisted_on_chain" },
                            rel = new List<RelDelta>{ new RelDelta{ id="shao", trust = 2, memo="市长坚持行文链" } },
                            logKind="卷宗", logText="退回：行文程序瑕疵",
                        },
                        result = "你在文头空白处批：“请按程序核转。”邵志远后来在楼道里说了一句：“新市长懂规矩。”——懂规矩的人，要么被保护，要么被绕开。",
                    },
                },
            });

            // —— 教学件 3：人情件 ——
            DossierEngine.Register(new Dossier
            {
                id = "dz_t3",
                kind = "risk",
                form = "土地件",
                title = "关于云中新区两宗国有建设用地使用权出让方案的请示",
                docNo = "同自然资〔2026〕156号",
                org = "市自然资源局",
                deadline = "2026-09-11",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "拟对云中新区2026-07、2026-08两宗国有建设用地使用权实施挂牌出让。",
                            "一、2026-07：商业用地，起始价1.85亿元。",
                            "二、2026-08：商住混合，起始价2.40亿元。竞买人资格：具备房地产开发一级资质。",
                            "三、建议挂牌时间：9月18日至9月28日。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：竞买条件说明",
                        paras = new List<string>
                        {
                            "（附件第2段）为保障项目品质与建设进度，建议对2026-08宗地设置“产业导入承诺”：竞得人须引入一家人工智能领域企业区域总部，并实现三年内税收落地。",
                            "（附件第4段，字号略小）竞买保证金为起始价的20%。联合竞买的，牵头方须为一级资质企业。",
                        },
                    },
                    new DossierPage
                    {
                        title = "签批栏",
                        paras = new List<string>
                        {
                            "任慎拟办：方案可行，建议按期挂牌。呈市长。",
                            "——空白——",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_t3_i1",
                        pageRef = "2",
                        detectHint = "“产业导入承诺”是否具备可考核条款与违约责任——还是只写在附件里的漂亮话",
                        ruleKey = "rule_tudi",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准挂牌",
                        complianceDelta = -2,
                        efficiencyDelta = 4,
                        gray = true,
                        effects = new Effects
                        {
                            setMarks = new List<string>{ "land_fast" },
                            logKind="卷宗", logText="照准新区两宗地挂牌",
                        },
                        result = "“同意挂牌。”你签得很顺。任慎的司机当晚多绕了两圈路——不是你的事，但节奏是你的节奏。",
                    },
                    new DossierOption
                    {
                        label = "要求补违约责任与考核指标后再报",
                        complianceDelta = 3,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            professional = 1, admin = 1,
                            setMarks = new List<string>{ "land_strict" },
                            rel = new List<RelDelta>{ new RelDelta{ id="rensheng", evalv = -3, memo="挂牌方案被要求补违约条款" } },
                            logKind="卷宗", logText="要求土地方案补违约责任",
                        },
                        result = "你批：“产业导入须写明考核指标、年度核查与违约责任，不得以附件意向代替合同条款。”任慎沉默了五秒：“市长，时间……”“时间我担。条款你补。”",
                    },
                    new DossierOption
                    {
                        label = "口头让局里“把握”，书面照准",
                        complianceDelta = -4,
                        efficiencyDelta = 5,
                        gray = true,
                        effects = new Effects
                        {
                            setMarks = new List<string>{ "gray_favor" },
                            logKind="卷宗", logText="口头把握、书面照准土地件",
                        },
                        result = "你把任慎叫来，说了句“产业导入要真，不要写在纸上的假”。然后签了“同意”。口头的刀，切不开合同。审计的时候，纸上只有你的名字。",
                    },
                },
            });
        }

        static void RegisterShowcase()
        {
            // 高光：对上报告美化压力
            DossierEngine.Register(new Dossier
            {
                id = "dz_s1",
                kind = "showcase",
                form = "对上报告",
                title = "关于报送2026年前三季度人工智能产业招商完成情况的报告",
                docNo = "同政报〔2026〕12号",
                org = "市政府办公室（汇总）",
                deadline = "2026-10-15",
                checkBudget = 4,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "省人民政府：",
                            "根据省署关于重点产业招商的部署，现将我市前三季度人工智能产业招商情况报告如下：",
                            "一、新签约项目37个，总投资额214.6亿元，完成省下达年度目标的107%。",
                            "二、开工项目21个，开工率56.8%。",
                            "三、存在的问题：部分项目用地手续仍在办理；个别县区统计口径需统一。",
                            "四、下一步工作：……",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：项目清单（摘要）",
                        table = "序号|项目|投资额（亿）|状态\n1|智算中心二期|28.0|开工\n2|矿区装备智能改造|6.2|开工\n3|某某区域总部|45.0|签约\n…|…|…|…",
                        paras = new List<string>
                        {
                            "（清单共37行。第14行“某某区域总部”状态为“签约”，备注空白。）",
                            "（统计科便签：口径若含框架协议，目标完成率可至112%。）",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_s1_i1",
                        pageRef = "2",
                        detectHint = "框架协议是否计入“新签约”；107% 是硬签约还是含意向",
                        ruleKey = "rule_duishang",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "按现稿报省（数字好看）",
                        complianceDelta = -5,
                        efficiencyDelta = 6,
                        gray = true,
                        effects = new Effects
                        {
                            reputation = 3,
                            setMarks = new List<string>{ "pressed_report" },
                            rel = new List<RelDelta>{ new RelDelta{ id="shao", evalv = 3, memo="招商数字按现稿上报" } },
                            logKind="卷宗", logText="对上报告：按现稿（含口径水分）上报",
                        },
                        result = "报告报出去的第三天，省署回了两个字：“已阅。”数字在表格里很漂亮。漂亮的东西，通常经不起第二年对账。",
                    },
                    new DossierOption
                    {
                        label = "要求剔除框架协议，如实反映",
                        complianceDelta = 5,
                        efficiencyDelta = -3,
                        effects = new Effects
                        {
                            professional = 1, political = 1,
                            setMarks = new List<string>{ "honest_report" },
                            logKind="卷宗", logText="对上报告：剔除水分如实上报",
                        },
                        result = "完成率从107%落到89%。邵志远皱了眉：“省里会不会觉得我们慢？”你说：“慢，可以追。假，追不回来。”岑伯衡后来在走廊上看了你一眼——那一眼很长。",
                    },
                },
            });

            // 高光：算法审批
            DossierEngine.Register(new Dossier
            {
                id = "dz_s2",
                kind = "showcase",
                form = "协议",
                title = "关于“AI辅助审批”市级试点购买服务协议的请示",
                docNo = "同政数〔2026〕9号",
                org = "市城市管理局（牵头）",
                deadline = "",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "为提高审批效率，拟购买“AI辅助审批”服务，覆盖户外广告设置、临时占道等6类高频事项。",
                            "一、服务商：华智算法公司（采购程序已完成）。",
                            "二、服务期限3年，合同金额每年1860万元。",
                            "三、系统将自动出具“审批通过”意见，窗口人员盖章确认。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：服务范围与权责",
                        paras = new List<string>
                        {
                            "第3条：系统基于历史审批数据训练，自动作出通过/退回建议。",
                            "第5条：最终法律责任由市城市管理局承担。",
                            "（页脚小字）系统日志保存6个月。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_s2_i1",
                        pageRef = "2",
                        detectHint = "“自动出具审批通过”是否架空法定权力主体；日志仅6个月是否够审计",
                        ruleKey = "rule_suanfa",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准采购，按协议试行",
                        complianceDelta = -4,
                        efficiencyDelta = 5,
                        effects = new Effects
                        {
                            setMarks = new List<string>{ "algo_auto" },
                            logKind="卷宗", logText="照准AI自动审批试点",
                        },
                        result = "效率会很好看。直到某一天，有人问：是谁批的？卷宗上写的是“系统通过，窗口盖章”。法定权力主体不会是服务器。",
                    },
                    new DossierOption
                    {
                        label = "原则同意采购，但改为“算法辅助、人工终审”，日志存3年",
                        complianceDelta = 4,
                        efficiencyDelta = 0,
                        effects = new Effects
                        {
                            professional = 2, political = 1,
                            setMarks = new List<string>{ "algo_pass" },
                            logKind="卷宗", logText="AI试点：辅助意见、人工终审",
                        },
                        result = "你在第5条旁批：“算法可提供意见，不得代替法定权力主体作最终决定。审批通过，必须有人签字，有人负责。”这笔字后来被写进市里的操作手册——你并不知道会写进去。",
                    },
                    new DossierOption
                    {
                        label = "否决试点",
                        complianceDelta = 2,
                        efficiencyDelta = -4,
                        effects = new Effects
                        {
                            rel = new List<RelDelta>{ new RelDelta{ id="shao", evalv = -4, memo="否决AI审批试点" } },
                            setMarks = new List<string>{ "algo_veto" },
                            logKind="卷宗", logText="否决AI辅助审批试点",
                        },
                        result = "邵志远的脸沉了半秒：“效率是竞争力。”你说：“责任也是。”会开完了。产业园的灯还亮着，像很多只不肯闭上的眼睛。",
                    },
                },
            });

            // 高光：信访/民生
            DossierEngine.Register(new Dossier
            {
                id = "dz_s3",
                kind = "deadline",
                form = "信访件",
                title = "关于平城区老旧小区加装电梯群体性诉求的情况报告",
                docNo = "同信〔2026〕44号",
                org = "市信访局",
                deadline = "2026-09-20",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市政府：",
                            "近期，平城区12个老旧小区居民联名反映加装电梯推进缓慢，涉及住户约2100户，其中60岁以上占比41%。",
                            "主要诉求：一是简化审批；二是提高财政补贴比例；三是明确低层住户补偿标准。",
                            "经研判，若9月底前无明确答复，存在集体到省走访风险。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：财政局意见",
                        paras = new List<string>
                        {
                            "提高补贴比例将增加年度支出约2300万元。建议维持现行比例，通过优化流程回应诉求。",
                            "（信访局批注）流程优化无法回应“低层补偿”核心矛盾。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_s3_i1",
                        pageRef = "2",
                        detectHint = "补贴比例与低层补偿：财政方案是否回避了核心矛盾",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "按财政意见答复：维持补贴，优化流程",
                        complianceDelta = 0,
                        efficiencyDelta = 2,
                        effects = new Effects
                        {
                            stress = 3,
                            setMarks = new List<string>{ "elevator_soft" },
                            logKind="卷宗", logText="加装电梯：按财政意见答复",
                        },
                        result = "答复件发出去的第二天，平城区又报来一份联名信，字迹比上次工整——人们把诉求抄了第二遍。抄第二遍的人，是准备去省里的。",
                    },
                    new DossierOption
                    {
                        label = "召开专题会：适度提补贴＋出台低层补偿指导价",
                        complianceDelta = 2,
                        efficiencyDelta = -1,
                        effects = new Effects
                        {
                            admin = 1, reputation = 2, morale = 1,
                            setMarks = new List<string>{ "helped_petition" },
                            logKind="卷宗", logText="加装电梯：提补贴并出补偿指导价",
                        },
                        result = "你让财政、住建、平城区坐到一间屋里，定了两件事：补贴比例提5个百分点，低层补偿给指导区间。钱是问题，但楼道里那些上不去的老人，也是问题。",
                    },
                    new DossierOption
                    {
                        label = "压下，等省里口径",
                        complianceDelta = -2,
                        efficiencyDelta = -4,
                        gray = true,
                        effects = new Effects
                        {
                            setMarks = new List<string>{ "elevator_delay" },
                            logKind="卷宗", logText="加装电梯件压下待省口径",
                        },
                        result = "“再等等。”你说。文件躺在抽屉里。9月20日的时限像墙上的钉子，你假装没看见它。",
                    },
                },
            });
        }

        static void RegisterPoolSamples()
        {
            // 池：例行财政追加（低风险手感）
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_p_finance",
                kind = "routine",
                form = "财政件",
                title = "关于调整市级机关差旅费管理办法部分条款的请示",
                docNo = "同财〔2026〕92号",
                org = "市财政局",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "根据省署统一部署，拟对我市机关差旅费住宿标准分档进行微调，自2026年10月1日起执行。",
                            "调整幅度：一类地区住宿限额上调8%，伙食补助维持不变。",
                            "妥否，请批示。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准",
                        complianceDelta = 1,
                        efficiencyDelta = 2,
                        effects = new Effects { logKind="卷宗", logText="照准差旅费标准微调" },
                        result = "“同意。”日常件。城市就是这样被无数个小“同意”推动的。",
                    },
                    new DossierOption
                    {
                        label = "要求同步压减一般性支出后报",
                        complianceDelta = 2,
                        efficiencyDelta = -1,
                        effects = new Effects { admin = 1, logKind="卷宗", logText="差旅费调整并压减支出" },
                        result = "你多批了一句：“标准可调，一般性支出须同比压减。”方启年会记住这句——好的财政局长，记性都用在这种地方。",
                    },
                },
            });

            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_p_report",
                kind = "routine",
                form = "请示",
                title = "关于报送全市安全生产专项整治阶段总结的报告",
                docNo = "同应急〔2026〕61号",
                org = "市应急管理局",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "全市安全生产专项整治开展以来，检查企业1240家，整改隐患3562项，责令停产整顿17家。",
                            "下一步将持续盯紧煤矿、化工、建筑施工三个重点行业。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_p_report_i1",
                        pageRef = "1",
                        detectHint = "“责令停产整顿17家”是否有复查销号清单",
                        ruleKey = "rule_shigu",
                        severity = 1,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准报送",
                        complianceDelta = 0,
                        efficiencyDelta = 2,
                        effects = new Effects { logKind="卷宗", logText="照准安全生产阶段总结" },
                        result = "总结报出去了。安全这根弦，松一次，就可能再也紧不回来——但总结本身，往往只负责“已报送”。",
                    },
                    new DossierOption
                    {
                        label = "要求附停产整顿企业复查销号清单",
                        complianceDelta = 2,
                        efficiencyDelta = -1,
                        effects = new Effects { exec = 1, logKind="卷宗", logText="要求附停产复查清单" },
                        result = "你批：“请附17家企业复查销号清单。”应急局局长在电话里顿了两秒：“好的市长，我们连夜整。”",
                    },
                },
            });

            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_p_hr",
                kind = "cosign",
                form = "人事单",
                title = "关于推荐市统计局局长人选征求意见的函",
                docNo = "同组〔2026〕23号",
                org = "市委组织部",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府党组：",
                            "根据工作需要，拟对市统计局局长岗位进行补充。现征求政府党组意见。",
                            "人选一：现任市财政局副局长甲，基层经历完整，数据口径熟。",
                            "人选二：现任市发改委副主任乙，项目协调能力强，统计专业背景一般。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "同意人选一（专业对口）",
                        complianceDelta = 1,
                        efficiencyDelta = 0,
                        effects = new Effects
                        {
                            professional = 1,
                            rel = new List<RelDelta>{ new RelDelta{ id="han", trust = 1, memo="统计局长：赞成专业对口" } },
                            logKind="卷宗", logText="统计局长人选：赞成甲",
                        },
                        result = "韩清收函时微微点头。专业对口是最不容易出错的答案——也是最不锋利的答案。",
                    },
                    new DossierOption
                    {
                        label = "同意人选二（协调能力）",
                        complianceDelta = 0,
                        efficiencyDelta = 1,
                        effects = new Effects
                        {
                            comm = 1,
                            rel = new List<RelDelta>{ new RelDelta{ id="shao", trust = 2, memo="统计局长：支持发改委副主任" } },
                            logKind="卷宗", logText="统计局长人选：赞成乙",
                        },
                        result = "邵志远会心一笑。人选背后是部门平衡，不是岗位说明书。你已经学会在同意里选边。",
                    },
                    new DossierOption
                    {
                        label = "建议扩大比选范围",
                        complianceDelta = 2,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            political = 1,
                            rel = new List<RelDelta>{ new RelDelta{ id="han", evalv = -1, memo="建议扩大比选" } },
                            logKind="卷宗", logText="统计局长：建议扩大比选",
                        },
                        result = "韩清把函收进文件夹，语气温和：“程序上可以。时间上，请政府这边把握。”——温和的提醒也是提醒。",
                    },
                },
            });
        }
    }
}
