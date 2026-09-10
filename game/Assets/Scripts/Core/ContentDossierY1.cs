using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>Phase 5 第一年手写池卷宗（2026-09 → 2027-08 季节感），补足模板之外的质感。</summary>
    public static class ContentDossierY1
    {
        public static void Register()
        {
            // 供暖季前
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_heat",
                kind = "deadline",
                form = "请示",
                title = "关于采暖季供热管网检修资金安排的请示",
                docNo = "同住建〔2026〕118号",
                org = "市住房和城乡建设局",
                deadline = "2026-10-15",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "采暖季临近，全市需检修老旧管网约47公里，涉及小区89个。申请资金1260万元。",
                            "若10月底前资金不到位，存在延迟开栓风险。妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：风险清单",
                        paras = new List<string>
                        {
                            "平城区：22个小区，管网14公里，其中8个小区上年曾低温投诉。",
                            "云冈区：15个小区，管网9公里。",
                            "（住建局批注）建议优先保障上年投诉小区。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准，优先保障投诉小区",
                        complianceDelta = 1, efficiencyDelta = 3,
                        effects = new Effects
                        {
                            exec = 1, morale = 1,
                            setMarks = new List<string> { "heat_first" },
                            logKind = "卷宗", logText = "供热检修资金优先投诉小区",
                        },
                        result = "云中的冬天不和人商量。你批了优先序——开栓那天，投诉热线会告诉你批得对不对。",
                    },
                    new DossierOption
                    {
                        label = "要求同步报分户改造方案，检修与改造一并统筹",
                        complianceDelta = 2, efficiencyDelta = 0,
                        effects = new Effects
                        {
                            professional = 1, stress = 2,
                            logKind = "卷宗", logText = "供热资金并分户改造",
                        },
                        result = "住建局多跑了一周。多跑的一周，是明年少挨的骂——前提是今年别冻着人。",
                    },
                },
            });

            // 预算季
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_budget",
                kind = "routine",
                form = "财政件",
                title = "关于编制2027年市级部门预算控制数的通知（代拟稿）",
                docNo = "同财〔2026〕156号",
                org = "市财政局",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "根据省署要求，2027年一般性支出原则上零增长；“三公”经费只减不增。",
                            "拟按上年预算压减3%下达控制数，请审定。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：压减测算",
                        table = "类别|上年（万元）|拟下达（万元）\n人员经费|—|与上年持平\n公用经费|—|压减3%\n项目支出|—|分类压减2%-5%\n三公|—|压减5%",
                        paras = new List<string>
                        {
                            "（财政局说明）民生与安全类项目不压减；产业引导基金另议。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_y1_budget_i1",
                        pageRef = "2",
                        detectHint = "“产业引导基金另议”是否为超预算开口子；压减是否只压了公用经费",
                        ruleKey = "rule_shuzi",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准下达",
                        complianceDelta = -1, efficiencyDelta = 3,
                        effects = new Effects { logKind = "卷宗", logText = "照准预算控制数" },
                        result = "控制数下去了。真正的戏在“另议”两个字里——明年这时候你就知道了。",
                    },
                    new DossierOption
                    {
                        label = "产业引导基金单列专题报市政府常务会",
                        complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects
                        {
                            political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = -2, memo = "产业基金要上常务会" } },
                            logKind = "卷宗", logText = "产业基金单列上会",
                        },
                        result = "邵志远没当场表态。把“另议”变成“上会”，是把口袋里的权拿到桌面上——有人喜欢，有人不喜欢。",
                    },
                },
            });

            // 汛期
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_flood",
                kind = "deadline",
                form = "突发事件",
                title = "关于御河城区段汛情及应急处置情况的报告",
                docNo = "同应急〔2027〕22号",
                org = "市应急管理局",
                deadline = "2027-07-20",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "7月18日夜间，御河城区段水位超警戒0.4米。已转移低洼地带群众312人，无人员伤亡。",
                            "请求：一是拨付应急抢险资金200万元；二是同意动用市级物资储备。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "立即同意，并赴现场",
                        complianceDelta = 1, efficiencyDelta = 4,
                        effects = new Effects
                        {
                            energy = -8, stress = 4, reputation = 3, exec = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "cen", evalv = 2, memo = "汛情第一时间到场" } },
                            logKind = "卷宗", logText = "御河汛情：资金与物资即批即办",
                        },
                        result = "雨衣里的手电光晃得人睁不开眼。你站在堤上没说几句话——群众记住的是市长在，不是市长讲了什么。",
                    },
                    new DossierOption
                    {
                        label = "同意资金，委托邵志远同志现场指挥",
                        complianceDelta = 1, efficiencyDelta = 3,
                        effects = new Effects
                        {
                            energy = -2,
                            rel = new List<RelDelta> { new RelDelta { id = "shao", familiar = 2, trust = 2, memo = "汛情现场指挥授权" } },
                            logKind = "卷宗", logText = "汛情：委托常务副现场指挥",
                        },
                        result = "邵志远半夜接到电话，声音很清醒：“我马上到。”搭档的成色，只在夜里见。",
                    },
                    new DossierOption
                    {
                        label = "要求先报详细损失清单再拨付",
                        complianceDelta = 2, efficiencyDelta = -3,
                        effects = new Effects
                        {
                            stress = 3, morale = -2,
                            logKind = "卷宗", logText = "汛情资金要求损失清单",
                        },
                        result = "清单三天后才齐。这三天里，转移群众住在体育馆。程序是对的——体育馆的味道也是真的。",
                    },
                },
            });

            // 煤电转型（大同压力源）
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_coal",
                kind = "showcase",
                form = "请示",
                title = "关于云冈矿区三对矿井关闭退出职工安置方案的请示",
                docNo = "同人社〔2026〕91号",
                org = "市人力资源和社会保障局",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "按去产能部署，拟关闭三对矿井，涉及职工4120人。安置渠道：内部转岗1860人、提前退休640人、自主创业补贴其余。",
                            "申请专项资金8900万元。妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：安置测算",
                        table = "渠道|人数|人均（万元）|小计（万元）\n内部转岗|1860|1.2|2232\n提前退休|640|2.0|1280\n创业补贴|1620|3.0|4860\n合计|4120|—|8372",
                        paras = new List<string>
                        {
                            "（正文申请8900万，附件合计8372万。差额528万，附件备注空白。）",
                            "（人社局副局长口头：差额拟作“应急周转”，未入附件。）",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_y1_coal_i1",
                        pageRef = "2",
                        detectHint = "正文8900万与附件合计8372万差528万；“应急周转”是否入账",
                        ruleKey = "rule_shuzi",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准8900万",
                        complianceDelta = -4, efficiencyDelta = 4,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "coal_slush" },
                            integrity = new IntegrityRecord { tag = "资金口径", note = "安置资金正文与附件差额未说明" },
                            logKind = "卷宗", logText = "矿井关闭安置：照准含差额资金",
                        },
                        result = "钱下去了，人也稳住了。差额那528万像煤渣——不起眼，但审计的铲子会翻到。",
                    },
                    new DossierOption
                    {
                        label = "按附件8372万核拨，差额另报",
                        complianceDelta = 4, efficiencyDelta = -1,
                        effects = new Effects
                        {
                            professional = 1, exec = 1,
                            setMarks = new List<string> { "coal_clean" },
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = -2, memo = "安置资金卡了差额" } },
                            logKind = "卷宗", logText = "矿井安置：按附件核拨",
                        },
                        result = "邵志远说：“安置如救火。”你说：“救火也不能往油桶里泼。”三天后，附件补全了528万的用途——写得很细。",
                    },
                },
            });

            // AI 产业园招商对赌
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_aitrust",
                kind = "risk",
                form = "协议",
                title = "关于华智算法公司区域总部对赌协议的请示",
                docNo = "同商务〔2026〕77号",
                org = "市商务局",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "拟与华智算法公司签订区域总部对赌：企业三年落地投资15亿元、税收2.4亿元；政府给予产业用地优惠及人才公寓配套。",
                            "若未达标，按比例收回优惠。妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：对赌条款摘录",
                        paras = new List<string>
                        {
                            "第4条：投资完成情况以企业自报为准，年度抽查比例不低于10%。",
                            "第7条：未达标时，优惠收回上限为已兑现金额的30%。",
                            "（商务局批注）同类协议他市收回上限通常为50%以上。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_y1_aitrust_i1",
                        pageRef = "2",
                        detectHint = "自报为准+收回上限30%是否形同虚设",
                        ruleKey = "rule_tudi",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准签约",
                        complianceDelta = -3, efficiencyDelta = 5,
                        effects = new Effects
                        {
                            reputation = 3,
                            setMarks = new List<string> { "aitrust_weak" },
                            logKind = "卷宗", logText = "华智对赌：照准（弱条款）",
                        },
                        result = "签约仪式会有合影。合影里你看不出第4条和第7条——它们在附件里，像两处事先留好的退水闸。",
                    },
                    new DossierOption
                    {
                        label = "要求改为第三方核数、收回上限50%",
                        complianceDelta = 4, efficiencyDelta = -2,
                        effects = new Effects
                        {
                            political = 1, professional = 1,
                            setMarks = new List<string> { "aitrust_hard" },
                            logKind = "卷宗", logText = "华智对赌：硬化条款",
                        },
                        result = "企业代表打了个电话，会开了延长四十分钟。最后条款改了两处——谈判桌上，慢的那方往往在看合同，快的那方在看表。",
                    },
                },
            });

            // 巡视整改纸面销号
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_xun",
                kind = "deadline",
                form = "巡视整改",
                title = "关于省委巡视反馈“项目审批超时”问题整改销号的请示",
                docNo = "同巡整〔2027〕12号",
                org = "市行政审批服务局",
                deadline = "2027-03-31",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市委巡视整改办：",
                            "针对“项目审批平均超时15个工作日”问题，已上线并联审批，现平均时限压至9个工作日。",
                            "申请销号。附件为系统截图与台账。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：抽查样本",
                        paras = new List<string>
                        {
                            "样本20件，系统显示均在时限内。",
                            "（备注）其中6件为“补录”，补录时间集中在销号前一周。",
                            "（审批局说明）补录系历史件归档，不影响真实时限。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_y1_xun_i1",
                        pageRef = "2",
                        detectHint = "销号前一周集中补录6件，是否纸面达标",
                        ruleKey = "rule_shigu",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "同意销号",
                        complianceDelta = -4, efficiencyDelta = 4,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "xun_paper" },
                            integrity = new IntegrityRecord { tag = "巡视整改", note = "审批超时问题以补录样本申请销号" },
                            logKind = "卷宗", logText = "巡视整改：纸面销号",
                        },
                        result = "销号文件很漂亮。巡视组若回头看补录时间戳，漂亮会变成另一种材料。",
                    },
                    new DossierOption
                    {
                        label = "随机抽10件非补录件复核后再销",
                        complianceDelta = 4, efficiencyDelta = -2,
                        effects = new Effects
                        {
                            exec = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", evalv = 2, memo = "巡视销号要求真实样本" } },
                            logKind = "卷宗", logText = "巡视整改：非补录样本复核",
                        },
                        result = "复核抽了三件超时。你把销号请示退了回去。沈砚后来在电梯里说了一句：“这样对。”",
                    },
                },
            });

            // 春运/文旅
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_tour",
                kind = "routine",
                form = "请示",
                title = "关于举办云中古城新春灯会的请示",
                docNo = "同文旅〔2026〕201号",
                org = "市文化和旅游局",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "拟于春节期间举办古城新春灯会，预算680万元，预计客流45万人次。",
                            "需协调公安、城管、卫健保障。妥否，请批示。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准，按大型活动保障",
                        complianceDelta = 1, efficiencyDelta = 3,
                        effects = new Effects { reputation = 2, energy = -3, logKind = "卷宗", logText = "照准新春灯会" },
                        result = "灯会那晚你去了半小时。人潮里没人认出市长——这半小时，比十份简报更像调研。",
                    },
                    new DossierOption
                    {
                        label = "同意办，要求安全预案与人流上限报我",
                        complianceDelta = 2, efficiencyDelta = 1,
                        effects = new Effects { exec = 1, logKind = "卷宗", logText = "灯会要安全预案" },
                        result = "预案第三页写了单小时入园上限。你把那个数字圈了出来——圈数字的习惯，从第一份测算表带到了灯会。",
                    },
                },
            });

            // 医疗资源
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_hosp",
                kind = "cosign",
                form = "请示",
                title = "关于市三医院与省医科大学合作办医的请示",
                docNo = "同卫健〔2027〕19号",
                org = "市卫生健康局",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "拟与省医科大学合作共建区域医疗中心，对方输出管理与专家，我市投入基建1.2亿元。",
                            "合作期15年。妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：权责摘录",
                        paras = new List<string>
                        {
                            "第6条：人事任免由合作理事会决定，我市占4席、校方占5席。",
                            "第9条：争议提交省里协调。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_y1_hosp_i1",
                        pageRef = "2",
                        detectHint = "理事会我市4席校方5席，是否丧失主导权",
                        ruleKey = "rule_xingwen",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准",
                        complianceDelta = -2, efficiencyDelta = 3,
                        effects = new Effects { reputation = 2, logKind = "卷宗", logText = "照准合作办医" },
                        result = "签字时笔尖顿了一下。第6条很轻，轻得像附则——附则往往才是正文。",
                    },
                    new DossierOption
                    {
                        label = "要求我市席位不少于对方，重大事项一票否决",
                        complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects
                        {
                            political = 2,
                            logKind = "卷宗", logText = "合作办医：争主导权",
                        },
                        result = "校方代表沉吟良久。最终我市5席、对方4席，外加一条“涉及资产处置须我市书面同意”。慢的那两周，买的是十五年的方向盘。",
                    },
                },
            });

            // 教育双减/学位
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_school",
                kind = "routine",
                form = "请示",
                title = "关于平城区新建两所小学学区划分方案的报告",
                docNo = "同教育〔2027〕33号",
                org = "市教育局",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "两所新建小学拟于秋季招生，学区划分方案已听证。涉及对口小区11个、预计学位2160个。",
                            "现报请审定。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准公布",
                        complianceDelta = 1, efficiencyDelta = 2,
                        effects = new Effects { logKind = "卷宗", logText = "照准学区划分" },
                        result = "学区图一公布，热线会热三天。教育口的件，批的不是地图，是家长的后半生焦虑。",
                    },
                    new DossierOption
                    {
                        label = "要求同步公布学位余量与调剂规则",
                        complianceDelta = 2, efficiencyDelta = 1,
                        effects = new Effects { admin = 1, logKind = "卷宗", logText = "学区：公布余量与调剂" },
                        result = "余量表让一部分家长安静了，让另一部分更不安——但不安有了数字，就好过有了谣言。",
                    },
                },
            });

            // 交通拥堵
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_road",
                kind = "deadline",
                form = "请示",
                title = "关于迎宾街快速化改造交通导改方案的报告",
                docNo = "同交通〔2027〕58号",
                org = "市交通运输局",
                deadline = "2027-05-10",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "迎宾街改造将于下月进场，需导改18条公交线路，预计高峰延误增加12分钟。",
                            "请审定导改方案并授权发布通告。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准，授权发布",
                        complianceDelta = 1, efficiencyDelta = 3,
                        effects = new Effects { exec = 1, logKind = "卷宗", logText = "照准迎宾街导改" },
                        result = "通告发出那晚，导航软件上的红线会变深。城市改造的代价，先由通勤者预付。",
                    },
                    new DossierOption
                    {
                        label = "要求分阶段施工，保住两条主干公交",
                        complianceDelta = 2, efficiencyDelta = -1,
                        effects = new Effects { political = 1, logKind = "卷宗", logText = "导改：保主干公交" },
                        result = "工期拉长了四十天。邵志远没说什么——他家也住城西，那两条公交他也坐过。",
                    },
                },
            });

            // 市场监管·食品安全
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_food",
                kind = "routine",
                form = "请示",
                title = "关于开展校园周边食品安全专项整治的报告",
                docNo = "同市监〔2027〕71号",
                org = "市市场监督管理局",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "拟对全市中小学周边200米内食品经营单位开展专项整治，检查覆盖率100%，问题单位公开曝光。",
                            "请审定。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准",
                        complianceDelta = 1, efficiencyDelta = 2,
                        effects = new Effects { reputation = 1, logKind = "卷宗", logText = "照准校园食安整治" },
                        result = "曝光名单会有人来打招呼。你让周谨记下每一个电话——名单还在，电话也还在。",
                    },
                    new DossierOption
                    {
                        label = "照准，并要求抽检结果同步家长群可见",
                        complianceDelta = 2, efficiencyDelta = 1,
                        effects = new Effects { comm = 1, logKind = "卷宗", logText = "食安整治：抽检公开" },
                        result = "家长群里的截图比公文跑得快。透明是最便宜的公信力——也最考验执行。",
                    },
                },
            });

            // 环保·大气
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_air",
                kind = "risk",
                form = "请示",
                title = "关于重污染天气应急响应级别的请示",
                docNo = "同生态〔2026〕144号",
                org = "市生态环境局",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "预测未来72小时将出现重度污染过程，建议启动橙色响应：部分企业限产、工地停工。",
                            "企业反映：正值订单交付高峰，恳请评估黄色响应。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：企业名单与产值",
                        paras = new List<string>
                        {
                            "涉及重点企业14户，预计影响产值约2.3亿元/周。",
                            "（生态环境局意见）科学上应橙色；经济上企业压力大。",
                            "（页边铅笔字，似领导口头）“再研究研究。”",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_y1_air_i1",
                        pageRef = "2",
                        detectHint = "“再研究研究”是否构成口头指示干预科学判定",
                        ruleKey = "rule_xingwen",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "按科学建议启动橙色响应",
                        complianceDelta = 3, efficiencyDelta = -3,
                        effects = new Effects
                        {
                            political = 1, stress = 3,
                            setMarks = new List<string> { "air_orange" },
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = -3, memo = "橙色响应压了企业订单" } },
                            logKind = "卷宗", logText = "启动橙色应急响应",
                        },
                        result = "企业老板的电话打到了邵志远那里。邵志远只转达了一句：“市长按监测数据定的。”——转达，也是一种立场。",
                    },
                    new DossierOption
                    {
                        label = "降为黄色，兼顾企业交付",
                        complianceDelta = -4, efficiencyDelta = 4,
                        gray = true,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "air_yellow" },
                            integrity = new IntegrityRecord { tag = "应急响应", note = "重污染过程降级响应，兼顾企业订单" },
                            logKind = "卷宗", logText = "降级为黄色响应",
                        },
                        result = "那三天的天空是黄的。学校停了户外课。你在第2页那行铅笔字旁，看见了自己的影子。",
                    },
                    new DossierOption
                    {
                        label = "启动橙色，但对保供企业“一企一策”",
                        complianceDelta = 1, efficiencyDelta = 0,
                        gray = true,
                        effects = new Effects
                        {
                            exec = 1, stress = 2,
                            setMarks = new List<string> { "air_oneco" },
                            logKind = "卷宗", logText = "橙色响应+一企一策",
                        },
                        result = "“一企一策”四个字很好听。执行的同志会来问：策是什么？你说：策是每一家都要有人签字负责。",
                    },
                },
            });

            // 年终走访/老同志
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_visit",
                kind = "routine",
                form = "会议材料",
                title = "关于春节前走访慰问老干部活动安排的请示",
                docNo = "同府办〔2027〕8号",
                org = "市委老干部局（代拟）",
                checkBudget = 1,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "拟于春节前走访慰问地市级以上老干部23人，安排车辆4台、随行记者2名。",
                            "请市领导分组带队。妥否，请批示。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准，我带一组",
                        complianceDelta = 1, efficiencyDelta = 1,
                        effects = new Effects
                        {
                            energy = -4, morale = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "laokang", familiar = 3, trust = 2, memo = "春节前走访老同志" } },
                            logKind = "卷宗", logText = "走访老干部：市长带队",
                        },
                        result = "老康的字还是那么正。他说：“替我看看城东那条路修好没有。”你记在了随身的本子上——比慰问金更难忘记的，是一句具体的话。",
                    },
                    new DossierOption
                    {
                        label = "请办公厅分组，我参加集中座谈",
                        complianceDelta = 0, efficiencyDelta = 2,
                        effects = new Effects { energy = -1, logKind = "卷宗", logText = "老干部走访：集中座谈" },
                        result = "座谈会上你听了四十分钟。老同志们讲的不是待遇，是“当年我们怎么干”。听进去的人，会把“当年”变成明天的尺子。",
                    },
                },
            });

            // 审计进点（年末）
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_audit",
                kind = "deadline",
                form = "省交办",
                title = "关于配合省审计厅开展市长任期经济责任审计的通知",
                docNo = "同审〔2027〕9号",
                org = "市审计局（转）",
                deadline = "2027-11-20",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "省审计厅将对你市开展经济责任审计，范围为2026年9月以来重大决策、财政收支与政府债务。",
                            "请准备台账并配合进点。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：需提供材料清单",
                        paras = new List<string>
                        {
                            "1. 常委会/常务会纪要；2. 重大合同；3. 专项资金拨付；4. 政府购买服务；5. 对上报告底稿。",
                            "（清单第5项旁有铅笔勾——若你签过含水分的对上报告，这一项会很烫。）",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "全力配合，台账如实",
                        complianceDelta = 2, efficiencyDelta = 1,
                        effects = new Effects
                        {
                            stress = 4,
                            setMarks = new List<string> { "audit_clean" },
                            logKind = "监察", logText = "省审计进点：如实提供",
                        },
                        result = "进点会开了两小时。审计组的同志翻纪要翻得很慢——慢，说明在看。你庆幸有些页自己也看过。",
                    },
                    new DossierOption
                    {
                        label = "先内部自查补正再交",
                        complianceDelta = -1, efficiencyDelta = 2,
                        gray = true,
                        effects = new Effects
                        {
                            stress = 6, exec = 2,
                            setMarks = new List<string> { "audit_scramble" },
                            logKind = "监察", logText = "省审计前内部自查补正",
                        },
                        result = "三夜。补正说明写了十七页。审计组收材料时说：“自查是好的。但时间戳很诚实。”",
                    },
                },
            });

            // 常委会前的会签件
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_huiqian",
                kind = "cosign",
                form = "会议材料",
                title = "关于提请常委会审议《大同市AI产业发展若干措施》的请示",
                docNo = "同发改〔2027〕41号",
                org = "市发展和改革局",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市委常委会：",
                            "拟出台AI产业若干措施：设立引导基金20亿元、算力补贴、场景开放。",
                            "已经市政府常务会研究，提请常委会审议。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：基金条款",
                        paras = new List<string>
                        {
                            "第2条：引导基金由市财政出资首期5亿元，社会资本募集15亿元。",
                            "第5条：基金投决会7人，政府方3人、社会方4人。",
                            "（发改局说明）与华智对赌协议中的“区域总部”条款有交叉。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_y1_huiqian_i1",
                        pageRef = "2",
                        detectHint = "投决会政府3席社会4席是否失控；与对赌协议交叉是否重复让利",
                        ruleKey = "rule_tudi",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "同意上会",
                        complianceDelta = -1, efficiencyDelta = 3,
                        effects = new Effects
                        {
                            logKind = "卷宗", logText = "AI若干措施：同意上常委会",
                        },
                        result = "岑伯衡会前看了你一眼：“政府先过了？”你说过了。他说：“过了就好——上会是确认，不是重新谈判。”",
                    },
                    new DossierOption
                    {
                        label = "要求改投决席位后再上会",
                        complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects
                        {
                            political = 2, professional = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = -2, memo = "基金席位卡了上会" } },
                            logKind = "卷宗", logText = "AI措施：改投决席位",
                        },
                        result = "发改局改了三稿。最终政府4席、社会3席。邵志远说你“抠”。你说：二十亿的盘子，席位就是方向盘。",
                    },
                },
            });

            // 基层减负
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_burden",
                kind = "routine",
                form = "请示",
                title = "关于开展“基层减负”专项督查的方案",
                docNo = "同府办〔2027〕66号",
                org = "市政府办公厅",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "拟督查各部门向区县摊派报表、APP打卡、留痕过度问题，抽查台账并通报。",
                            "请审定方案。",
                        },
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准，我任组长",
                        complianceDelta = 2, efficiencyDelta = 1,
                        effects = new Effects
                        {
                            reputation = 2, energy = -3,
                            logKind = "卷宗", logText = "基层减负：市长任督查组长",
                        },
                        result = "通报发出去那天，有三个局的办公室主任同时给周谨打电话。减负减的是表，疼的是填表的人。",
                    },
                    new DossierOption
                    {
                        label = "请周谨同志牵头即可",
                        complianceDelta = 1, efficiencyDelta = 2,
                        effects = new Effects
                        {
                            rel = new List<RelDelta> { new RelDelta { id = "zhoujin", trust = 1, evalv = 2, memo = "减负督查授权" } },
                            logKind = "卷宗", logText = "减负：办公厅牵头",
                        },
                        result = "周谨应得很稳。办公厅牵头减负，减的是别人加给基层的负——自己加的那部分，他会先撕。",
                    },
                },
            });

            // 突发：煤矿安全
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_y1_mine",
                kind = "deadline",
                form = "突发事件",
                title = "关于云冈区某矿井涉险事故的紧急报告",
                docNo = "同应急〔2027〕103号",
                org = "市应急管理局",
                deadline = "2027-08-08",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "8月6日，云冈区某矿井发生顶板涉险，8人被困，已升井5人，救援进行中。",
                            "请求：市长赴现场；开通绿色通道；准备后续通报口径。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：初报口径（草稿）",
                        paras = new List<string>
                        {
                            "“企业安全管理存在漏洞，已责令停产整顿。”",
                            "（页边铅笔）是否提“监管责任”？——待定。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_y1_mine_i1",
                        pageRef = "2",
                        detectHint = "通报是否回避监管责任；停产整顿是否附销号清单要求",
                        ruleKey = "rule_shigu",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "立即赴现场，口径含监管责任自查",
                        complianceDelta = 3, efficiencyDelta = 4,
                        effects = new Effects
                        {
                            energy = -10, stress = 8, reputation = 4, exec = 2,
                            setMarks = new List<string> { "mine_own" },
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", evalv = 3, memo = "涉险事故口径含监管自查" } },
                            logKind = "监察", logText = "矿井涉险：现场+含监管责任口径",
                        },
                        result = "你在井口站到凌晨。3人仍未升井。通报你改了七遍，最后留下“监管责任同步自查”——沈砚看完只说：可以。",
                    },
                    new DossierOption
                    {
                        label = "委托分管同志现场，口径先聚焦救援",
                        complianceDelta = 1, efficiencyDelta = 3,
                        effects = new Effects
                        {
                            energy = -4, stress = 5,
                            logKind = "监察", logText = "矿井涉险：委托现场",
                        },
                        result = "救援是第一位的。第二位的，是将来有人问：市长那天在哪。答案会写进某份巡视底稿。",
                    },
                    new DossierOption
                    {
                        label = "要求企业自行处置，政府只做通报",
                        complianceDelta = -4, efficiencyDelta = -2,
                        gray = true,
                        effects = new Effects
                        {
                            stress = 3, morale = -3,
                            setMarks = new List<string> { "mine_hands_off" },
                            integrity = new IntegrityRecord { tag = "安全生产", note = "涉险事故交企业自行处置" },
                            logKind = "监察", logText = "矿井涉险：交企业自行处置",
                        },
                        result = "第二天舆情起来了。标题里有你的城市名，没有你的名字——暂时。",
                    },
                },
            });
        }
    }
}
