using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// Phase 5 · M1 内容量补充（第二辑）：手写卷宗 10 件，覆盖 spec S2.6 的全部来文形态。
    /// 其中两件与两条新链挂钩：
    /// ① `dz_sc_safety` 涉险事故初报 → 安全事故瞒报压力链（ContentChainsRisk）；
    /// ② `dz_ai_terms` 对赌补充条款 → 招商引资对赌链（同文件）。
    /// 写法约定：routine/cosign 进随机池；deadline 件必须给 releaseFrom，否则会被优先队列一次性排空。
    /// </summary>
    public static class ContentDossierY2
    {
        public static void Register()
        {
            RegisterSafetyReport();      // 突发事件（链：瞒报压力）
            RegisterTrustTerms();        // 协议（链：招商对赌）
            RegisterRelocationPetition();// 信访件
            RegisterBudgetAdjust();      // 财政件
            RegisterCadrePost();         // 人事单
            RegisterSoeReform();         // 协议（国企混改）
            RegisterMeetingItem();       // 会议材料
            RegisterXunLookback();       // 巡视整改（回头看）
            RegisterQuarterReport();     // 对上报告
            RegisterIdleLand();          // 土地件
        }

        // ---------------- 1. 涉险事故初报（→ 瞒报压力链） ----------------

        static void RegisterSafetyReport()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_sc_safety",
                kind = "risk",
                form = "突发事件",
                title = "关于云冈区某化工厂受限空间作业涉险事件的初步报告",
                docNo = "同应急〔2027〕64号",
                org = "市应急管理局",
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
                            "2027年7月22日，云冈区某化工厂污水处理车间发生受限空间作业涉险事件。企业报告：无人员伤亡，直接经济损失约48万元。",
                            "现场已由区应急局责令停止作业，企业自行组织整改。",
                            "按《生产安全事故报告和调查处理条例》，是否构成一般事故，待进一步核实。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：核实情况与关联材料",
                        table = "项目|企业报告|区应急局核实\n受伤人数|0|3（其中1人留观）\n直接损失（万元）|48|待评估\n是否上报|否|未上报",
                        paras = new List<string>
                        {
                            "（注：区应急局7月24日到市第三医院调取的就诊记录显示，当日在厂区急救车送医3人。）",
                            "（企业报告落款日期：7月23日。）",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_sc_safety_i1",
                        pageRef = "2",
                        detectHint = "企业自报伤亡口径与就诊记录是否一致；应该上报而未上报",
                        ruleKey = "rule_shigu",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照企业自报口径办结，交区里督办整改",
                        complianceDelta = -3,
                        efficiencyDelta = 3,
                        gray = true,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "safety_soft" },
                            exec = 1,
                            integrity = new IntegrityRecord { tag = "安全生产", note = "涉险事件按企业自报“无伤亡”办结" },
                            logKind = "卷宗", logText = "涉险事件：按企业自报口径办结",
                        },
                        result = "批了六个字：“请区里督办落实。”——那三个人的留观记录，躺在医院系统里，和这份卷宗各归各的柜子。",
                    },
                    new DossierOption
                    {
                        label = "指令：由市局牵头核伤亡口径，按条例上报",
                        complianceDelta = 3,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "safety_honest" },
                            exec = 1, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 2, memo = "涉险事故要求按条例上报" } },
                            logKind = "卷宗", logText = "涉险事件：核口径后按条例上报",
                        },
                        result = "市局连夜核到第三医院。上报文本里写的是“3人受伤（1人留观）”。区里有人骂了句“小题大做”——这五个字，将来会替三个人作证。",
                    },
                    new DossierOption
                    {
                        label = "派专家现场复查，先固化证据再定性",
                        complianceDelta = 2,
                        efficiencyDelta = -1,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "safety_probe" },
                            professional = 1,
                            logKind = "卷宗", logText = "涉险事件：派专家现场复查固化证据",
                        },
                        result = "专家组第三天进场，先封了作业票和监控硬盘。证据这东西，早一天拿在手里是证据，晚一天就是“已损坏”。",
                    },
                },
            });
        }

        // ---------------- 2. 对赌补充条款（→ 招商对赌链） ----------------

        static void RegisterTrustTerms()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_ai_terms",
                kind = "routine",
                form = "协议",
                title = "关于华智项目投资协议补充条款（第二轮）的请示",
                docNo = "同商务〔2027〕19号",
                org = "市商务局",
                deadline = "",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "经两轮谈判，企业对投资协议提出补充条款：",
                            "一、投资强度考核口径由“固定资产投资额”调整为“项目总投资（含流动资金）”；",
                            "二、亩均税收指标以企业年度审计报告为准，市方不再另行核定；",
                            "三、政策兑现与考核指标完成情况由双方共同确认后执行。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：条款变化对照",
                        table = "指标|原条款|补充条款\n投资强度口径|固定资产投资|含流动资金的总投资\n税收核定|市方核定|企业审计报告\n争议解决|市方认定|双方共同确认",
                        paras = new List<string> { "（对照表由市商务局外资科编制。）" },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_ai_terms_i1",
                        pageRef = "1",
                        detectHint = "考核口径放宽与核定权让渡：谁定标准、谁说了算",
                        ruleKey = "rule_tudi",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准，以促成签约",
                        complianceDelta = -3,
                        efficiencyDelta = 3,
                        gray = true,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "aitrust_weak2", "aitrust_weak" },
                            efficiency = 2,
                            logKind = "卷宗", logText = "对赌补充条款：照准放宽考核口径",
                        },
                        result = "“含流动资金的总投资”六个字，把投资强度的分母放大了近一倍。签约仪式的横幅已经印好了，字很大，看不清小字。",
                    },
                    new DossierOption
                    {
                        label = "退回：坚守原口径，核定权留在市方",
                        complianceDelta = 3,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "aitrust_double_down", "aitrust_hard" },
                            professional = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "rensheng", trust = 1, memo = "对赌条款坚守核定权" } },
                            logKind = "卷宗", logText = "对赌补充条款：退回坚守原口径",
                        },
                        result = "商务局带着修改意见又跑了两趟。第三趟回来时，企业只在“核定”两个字上让了一步——让一步，是因为你也在合同里让了一步：用地价格按工业下限。",
                    },
                    new DossierOption
                    {
                        label = "折中：口径可宽，但核定权交第三方，且写入退出条款",
                        whenMark = "route_industry",
                        lockReason = "需先确立「产业转型」路线",
                        complianceDelta = 2,
                        efficiencyDelta = 1,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "aitrust_third", "aitrust_hard" },
                            admin = 1, political = 1,
                            logKind = "卷宗", logText = "对赌条款：第三方核数+退出条款",
                        },
                        result = "第三方机构名单由双方各出三家、抽签定。企业法务盯着抽签过程看了很久，最后说了句：“这个我们认。”——认的是规则，不是人。",
                    },
                },
            });
        }

        // ---------------- 3. 棚改回迁逾期联名信 ----------------

        static void RegisterRelocationPetition()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_v_relocate",
                kind = "routine",
                form = "信访件",
                title = "关于北关棚改项目回迁逾期问题的联名信访件",
                docNo = "同信〔2027〕52号",
                org = "市信访局",
                deadline = "",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "北关棚改项目回迁安置房原定2026年12月交付，现逾期7个月。信访人共216户，联名信中提出三项诉求：明确交付日期、补发过渡期安置费、公布工程进度。",
                            "市住建局答复口径：因施工方资金链问题停工，正协调续建。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：项目与资金情况",
                        paras = new List<string>
                        {
                            "项目共4栋，其中2栋主体完工、2栋停工。",
                            "过渡期安置费已发放至2026年12月，欠发7个月，测算需追加1240万元。",
                            "施工方与建设单位存在工程款纠纷，涉诉金额约3600万元。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_v_relocate_i1",
                        pageRef = "1",
                        detectHint = "答复口径只讲施工方责任，是否回避监管与资金监管账户问题",
                        ruleKey = "rule_xingwen",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "按现有口径答复，责成协调续建",
                        complianceDelta = -1,
                        efficiencyDelta = 2,
                        effects = new Effects { logKind = "卷宗", logText = "棚改回迁：责成协调续建" },
                        result = "答复函发下去，第217封信在一个月后寄到了省里。信里多了一句：“市长批示只写了‘协调’两个字。”",
                    },
                    new DossierOption
                    {
                        label = "先补发过渡费，并公开工程进度与资金监管账户",
                        complianceDelta = 2,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            reputation = 3, stress = 3,
                            rel = new List<RelDelta> { new RelDelta { id = "fang", familiar = 2, memo = "棚改过渡费应急补发" } },
                            logKind = "卷宗", logText = "棚改回迁：补发过渡费并公开进度",
                        },
                        result = "1240万从预备费里挤出来，公示栏里贴上了资金监管账户的流水。贴出去那天，信访局的人说：“今天没来人。”——没来人，就是来人少了。",
                    },
                    new DossierOption
                    {
                        label = "由市属平台公司接盘续建，同步追偿施工方",
                        whenMark = "route_project",
                        lockReason = "需先确立「项目攻坚」路线",
                        complianceDelta = 2,
                        efficiencyDelta = 2,
                        effects = new Effects
                        {
                            exec = 2, stress = 2,
                            setMarks = new List<string> { "prj_relocate_takeover" },
                            integrity = new IntegrityRecord { tag = "政府债务", note = "棚改项目由市属平台接盘续建" },
                            logKind = "卷宗", logText = "棚改回迁：平台公司接盘续建",
                        },
                        result = "平台公司接盘，两栋停工楼在四十天后复工。接盘的代价记在平台公司的负债表上——四年后做政府债务审计时，这张表会被人翻开。",
                    },
                },
            });
        }

        // ---------------- 4. 年中预算调整 ----------------

        static void RegisterBudgetAdjust()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_v_budget",
                kind = "routine",
                form = "财政件",
                title = "关于2027年年中预算调整方案的请示",
                docNo = "同财〔2027〕141号",
                org = "市财政局",
                deadline = "",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "受土地出让收入下滑影响，上半年一般公共预算收入完成年度预算的41%。拟调整方案：",
                            "一、调减政府性基金收入8.6亿元；二、压减一般性支出3.2亿元；三、动用预算稳定调节基金2.4亿元弥补缺口。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：调整明细表",
                        table = "项目|年初预算|调整后|增减\n一般公共预算收入|86.0|82.8|-3.2\n政府性基金收入|42.0|33.4|-8.6\n一般性支出|31.0|27.8|-3.2\n稳定调节基金动用|0.0|2.4|+2.4",
                        paras = new List<string> { "（明细表由市财政局预算科编制。）" },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_v_budget_i1",
                        pageRef = "2",
                        detectHint = "压减支出是否涉及民生与“三保”；动用稳定调节基金的后续补充来源",
                        ruleKey = "rule_shuzi",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准调整方案",
                        complianceDelta = -1,
                        efficiencyDelta = 2,
                        effects = new Effects { efficiency = 1, logKind = "卷宗", logText = "年中预算调整：照准" },
                        result = "方案过了。压减的3.2亿里，会议费和培训费占了四成——能压的部分，通常也是最容易压过头的部分。",
                    },
                    new DossierOption
                    {
                        label = "要求单列“三保”支出清单，民生项目不参与压减",
                        complianceDelta = 3,
                        efficiencyDelta = -1,
                        effects = new Effects
                        {
                            admin = 1, professional = 1,
                            compliance = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "fang", evalv = -1, memo = "预算调整被要求单列三保清单" } },
                            logKind = "卷宗", logText = "预算调整：单列三保清单",
                        },
                        result = "清单报上来，民生支出被划在红线外的部分比预想的多1.1亿。方启年在门口站了一会儿：“市长，这样压不到8.6亿。”你说：“那就少压1.1亿。”",
                    },
                },
            });
        }

        // ---------------- 5. 区局一把手调整（含回避） ----------------

        static void RegisterCadrePost()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_v_cadre",
                kind = "cosign",
                form = "人事单",
                title = "关于云冈区自然资源分局局长调整的备案报告",
                docNo = "同组〔2027〕36号",
                org = "市自然资源局党组",
                deadline = "",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府党组：",
                            "经局党组研究，拟调整云冈区自然资源分局局长：现任局长另有任用，拟由现任副局长（主持工作二年）接任。",
                            "该同志任现职期间考核均为称职以上。现予备案。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：干部情况说明",
                        paras = new List<string>
                        {
                            "拟任人选：男，48岁，本科学历，2019年起任现职。",
                            "（备注：其配偶在云冈区某房地产开发企业任职，任财务负责人。）",
                            "分局近三年承办土地出让事项47宗。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_v_cadre_i1",
                        pageRef = "2",
                        detectHint = "配偶从业与岗位回避：是否属于应当回避的情形，有无书面声明",
                        ruleKey = "rule_xingwen",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "备案",
                        complianceDelta = -2,
                        efficiencyDelta = 2,
                        effects = new Effects
                        {
                            rel = new List<RelDelta> { new RelDelta { id = "han", familiar = 1, memo = "区局长调整备案通过" } },
                            integrity = new IntegrityRecord { tag = "人事备案", note = "拟任人选配偶在辖区房企任职未附回避说明" },
                            logKind = "卷宗", logText = "区局长调整：直接备案",
                        },
                        result = "章盖下去，一切照旧。三年后一份巡察报告里会有一行字：“个别岗位回避制度执行不到位”——那行字，会有人替你解释。",
                    },
                    new DossierOption
                    {
                        label = "要求补充履职回避声明与配偶从业情况报告",
                        complianceDelta = 3,
                        efficiencyDelta = -1,
                        effects = new Effects
                        {
                            political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 1, memo = "人事调整要求补回避声明" } },
                            logKind = "卷宗", logText = "区局长调整：补回避声明",
                        },
                        result = "两天后补来的材料里，多了一页配偶签字的承诺书。局长打电话来解释，语气里带点委屈——他委屈得有道理，但你签的字会跟他一起进档案。",
                    },
                    new DossierOption
                    {
                        label = "建议调整拟任方向，由市局下派交流",
                        complianceDelta = 2,
                        efficiencyDelta = -1,
                        effects = new Effects
                        {
                            political = 2, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "han", evalv = -1, memo = "人事方案被要求改交流" } },
                            logKind = "卷宗", logText = "区局长调整：改由市局下派",
                        },
                        result = "韩清听完只说了句：“理由呢？”你答：“同一个岗位待了七年，七年里她单位的财务都在那边。”她记下了这句，也记下了你。",
                    },
                },
            });
        }

        // ---------------- 6. 国企混改框架协议 ----------------

        static void RegisterSoeReform()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_v_soe",
                kind = "routine",
                form = "协议",
                title = "关于市属城投集团引入战略投资者的框架协议（送审稿）",
                docNo = "同国资〔2028〕27号",
                org = "市国资委",
                deadline = "",
                releaseFrom = "2028-03-01",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "为化解城投集团存量债务，拟引入战略投资者增资扩股，出让不超过35%股权，募集资金不低于18亿元。",
                            "框架协议约定：投资者入股后三年内不得转让；我方承诺其参与城市更新项目。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：估值与承诺事项",
                        table = "项目|金额/内容\n投前估值|52亿元\n拟出让股权|≤35%\n募集资金|≥18亿元\n我方承诺|优先参与城市更新项目",
                        paras = new List<string>
                        {
                            "（估值依据：2027年末净资产45亿元，双方协商确定。未列评估机构。）",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_v_soe_i1",
                        pageRef = "2",
                        detectHint = "国有股权作价是否经法定评估；“优先参与城市更新”是否构成变相承诺收益",
                        ruleKey = "rule_tudi",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "原则同意，按框架推进",
                        complianceDelta = -3,
                        efficiencyDelta = 2,
                        gray = true,
                        effects = new Effects
                        {
                            efficiency = 2,
                            integrity = new IntegrityRecord { tag = "国资交易", note = "混改估值未列法定评估即原则同意" },
                            logKind = "卷宗", logText = "城投混改：未评估即原则同意",
                        },
                        result = "18亿在半年内到账，平台公司的负债率降了9个百分点。审计进场时问的第一个问题是：“投前估值52亿，出自哪份评估报告？”",
                    },
                    new DossierOption
                    {
                        label = "要求先做法定评估与职工代表大会程序",
                        complianceDelta = 3,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            professional = 2, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "fang", familiar = 1, memo = "混改要求先评估与职代会" } },
                            logKind = "卷宗", logText = "城投混改：先评估与职代会",
                        },
                        result = "评估报告出来，投前估值是47亿——比协商价低了5亿。投资方代表在会议室沉默了很久，最后说：“那我们把募集额提到20亿。”",
                    },
                    new DossierOption
                    {
                        label = "同意估值，但删除“优先参与城市更新”的承诺条款",
                        whenMark = "route_project",
                        lockReason = "需先确立「项目攻坚」路线",
                        complianceDelta = 2,
                        efficiencyDelta = 1,
                        effects = new Effects
                        {
                            political = 1, admin = 1,
                            setMarks = new List<string> { "soe_no_promise" },
                            logKind = "卷宗", logText = "城投混改：删除收益性承诺条款",
                        },
                        result = "条款删掉那天，投资方问：“那我们的保障是什么？”你说：“按股比说话。”——按股比说话的意思是：不保本，但也不算计。",
                    },
                },
            });
        }

        // ---------------- 7. 常务会议题 ----------------

        static void RegisterMeetingItem()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_v_meeting",
                kind = "cosign",
                form = "会议材料",
                title = "关于提请市政府常务会议审议《大同市数据要素市场培育三年行动方案》的请示",
                docNo = "同府办〔2027〕88号",
                org = "市政府办公厅",
                deadline = "",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "拟提请常务会议审议《数据要素市场培育三年行动方案》。方案共4章18条，明确三年内建成市级数据交易平台1个、培育数商企业100家。",
                            "方案已征求12个部门意见，其中市司法署提出“数据确权条款与上位法衔接需细化”。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：部门意见汇总",
                        table = "部门|意见|采纳情况\n市司法署|确权条款需与上位法衔接|未采纳\n市财政署|未明确经费来源|部分采纳\n市监署|数商认定标准缺失|未采纳",
                        paras = new List<string> { "（汇总表由起草组整理。）" },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_v_meeting_i1",
                        pageRef = "2",
                        detectHint = "未采纳的法制意见是否说明理由；方案有无经费与认定标准两个“空手条款”",
                        ruleKey = "rule_xingwen",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "原则同意，提请常务会议审议",
                        complianceDelta = -2,
                        efficiencyDelta = 3,
                        effects = new Effects
                        {
                            exec = 1,
                            integrity = new IntegrityRecord { tag = "会议材料", note = "法治审查意见未采纳未写明理由即上会" },
                            logKind = "卷宗", logText = "数据要素方案：未说明理由即上会",
                        },
                        result = "会上通过了。半年后，方案第7条被省里要求重新解释——解释不了的地方，正是当初“未采纳”的那一格。",
                    },
                    new DossierOption
                    {
                        label = "退回起草组：未采纳意见须逐条写明理由，经费与标准补实",
                        complianceDelta = 3,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            professional = 2, admin = 1,
                            logKind = "卷宗", logText = "数据要素方案：退回补理由与经费",
                        },
                        result = "起草组加了四页附注，把“未采纳”的理由一条条写清楚。常务会上没人再问——问不出来，比问出来更省时间。",
                    },
                },
            });
        }

        // ---------------- 8. 巡视“回头看” ----------------

        static void RegisterXunLookback()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_v_xun2",
                kind = "deadline",
                form = "巡视整改",
                title = "关于省委巡视“回头看”反馈意见整改方案的请示",
                docNo = "同巡整〔2028〕9号",
                org = "市委巡视整改工作领导小组办公室",
                deadline = "2028-06-30",
                releaseFrom = "2028-05-10",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市委：",
                            "省委巡视“回头看”反馈涉及政府系统共9个问题，其中3个属于上轮巡视已“销号”后再次出现的问题。",
                            "整改方案：逐条明确责任单位与完成时限，2028年6月底前完成。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：问题与责任分解（摘录）",
                        table = "序号|问题|上轮状态|本轮定性\n1|政府投资项目超概算|已销号|再次出现\n2|专项资金滞留|已销号|再次出现\n3|征地补偿程序|已销号|再次出现\n4|公车使用台账|整改中|—",
                        paras = new List<string> { "（注：3个“销号后再次出现”问题的上轮销号材料，均为台账自报。）" },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_v_xun2_i1",
                        pageRef = "2",
                        detectHint = "“纸面销号”的复发：本轮整改是否仍以台账自报作为验收依据",
                        ruleKey = "rule_shigu",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准方案，按期完成销号",
                        complianceDelta = -3,
                        efficiencyDelta = 3,
                        gray = true,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "xun2_paper" },
                            efficiency = 2,
                            logKind = "卷宗", logText = "巡视回头看：照准台账销号方案",
                        },
                        result = "方案过了，9个问题在6月底前全部“完成整改”。第三次巡视时，前3个问题的名字还会出现——它们已经习惯了这份表格。",
                    },
                    new DossierOption
                    {
                        label = "要求：3个复发问题一律现场复查后销号，并倒查上轮销号责任",
                        complianceDelta = 4,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            political = 2, stress = 3,
                            setMarks = new List<string> { "xun2_onsite" },
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 3, memo = "巡视整改要求现场复查并倒查" } },
                            logKind = "卷宗", logText = "巡视回头看：现场复查+倒查责任",
                        },
                        result = "倒查倒出了上轮的两个经办人。夜里有人给你打电话，说了很多“不容易”——你听完，把复查名单又加了一处。",
                    },
                },
            });
        }

        // ---------------- 9. 季度经济形势报告 ----------------

        static void RegisterQuarterReport()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_v_report2",
                kind = "routine",
                form = "对上报告",
                title = "关于2027年三季度经济运行情况的报告（送审稿）",
                docNo = "同发改〔2027〕132号",
                org = "市发展和改革局",
                deadline = "",
                checkBudget = 2,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "省发展和改革署：",
                            "三季度全市地区生产总值同比增长5.8%，规上工业增加值增长7.2%，固定资产投资增长6.4%。",
                            "新兴产业投资增长24%，占固定资产投资比重提升至21%。",
                            "现将有关情况报告如下。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：主要指标口径说明",
                        table = "指标|数值|口径说明\nGDP|+5.8%|—\n规上工业增加值|+7.2%|含新投产企业当年全口径\n新兴产业投资|+24%|含框架协议意向投资\n固投|+6.4%|—",
                        paras = new List<string> { "（口径说明由市发改局综合科提供。）" },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_v_report2_i1",
                        pageRef = "2",
                        detectHint = "意向投资是否可计入“新兴产业投资”；新投产企业口径与统计制度是否一致",
                        ruleKey = "rule_duishang",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "签发报送",
                        complianceDelta = -2,
                        efficiencyDelta = 3,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "q3_report_loose" },
                            logKind = "卷宗", logText = "季度形势报告：按现有口径签发",
                        },
                        result = "报告上去，省里在简报里点了大同的名。同一个“24%”，统计口径一旦被对账，名字会从表扬栏挪到另一栏。",
                    },
                    new DossierOption
                    {
                        label = "退回：剔除意向投资，按统计制度重算后再报",
                        complianceDelta = 3,
                        efficiencyDelta = -1,
                        effects = new Effects
                        {
                            professional = 2,
                            setMarks = new List<string> { "q3_report_clean" },
                            logKind = "卷宗", logText = "季度形势报告：剔意向投资重算",
                        },
                        result = "重算后新兴产业投资增速是16%，占比18%。数字降了，发改局长的脸色也降了——但省里下一轮回访抽查时，这两个数他们核得出来。",
                    },
                },
            });
        }

        // ---------------- 10. 闲置土地处置 ----------------

        static void RegisterIdleLand()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_v_idleland",
                kind = "routine",
                form = "土地件",
                title = "关于高新区三宗闲置土地处置方案的请示",
                docNo = "同自然资〔2028〕61号",
                org = "市自然资源局",
                deadline = "",
                releaseFrom = "2028-01-20",
                checkBudget = 3,
                pages = new List<DossierPage>
                {
                    new DossierPage
                    {
                        title = "正文",
                        paras = new List<string>
                        {
                            "市人民政府：",
                            "高新区GX-02、GX-05、GX-11三宗土地自2024年出让后闲置超过两年，拟按《闲置土地处置办法》处置：",
                            "方案一：延长动工开发期限一年；方案二：协议有偿收回；方案三：置换其他地块。",
                            "经征询权利人意见，三家均申请“延长动工期限”。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：宗地情况",
                        table = "宗地|受让人|闲置原因|拟处置\nGX-02|某置业公司|资金不足|延期一年\nGX-05|某科技公司|规划调整|延期一年\nGX-11|某置业公司|资金不足|延期一年",
                        paras = new List<string>
                        {
                            "（注：GX-02与GX-11受让人为同一实际控制人。三宗地均未收取闲置费。）",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_v_idleland_i1",
                        pageRef = "2",
                        detectHint = "闲置费是否应收未收；“延长一年”是否成为变相长期占地的通行做法",
                        ruleKey = "rule_tudi",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准三宗全部延期一年",
                        complianceDelta = -3,
                        efficiencyDelta = 2,
                        gray = true,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "idle_all_extend" },
                            efficiency = 1,
                            integrity = new IntegrityRecord { tag = "土地闲置", note = "三宗闲置土地全部同意延期且未收闲置费" },
                            logKind = "卷宗", logText = "闲置土地：三宗全部延期",
                        },
                        result = "三宗地继续闲着。明年这个时候，同一份请示会再来一次，只改一个年份。",
                    },
                    new DossierOption
                    {
                        label = "不同意延期：一宗有偿收回、两宗收闲置费并限期动工",
                        complianceDelta = 3,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            professional = 2, stress = 2,
                            setMarks = new List<string> { "idle_enforce" },
                            rel = new List<RelDelta> { new RelDelta { id = "rensheng", trust = 2, memo = "闲置土地依法收回与收费" } },
                            logKind = "卷宗", logText = "闲置土地：收回一宗+收取闲置费",
                        },
                        result = "收回的那宗地，两个月后重新挂牌，成交价高出原价21%。任慎在成交确认书上签字时说：“早该这么干。”——早该的事情，通常要等一个愿意当恶人的市长。",
                    },
                },
            });
        }
    }
}
