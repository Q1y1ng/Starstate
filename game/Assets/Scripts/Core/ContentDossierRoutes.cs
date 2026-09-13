using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// Phase 5 · M1 路线专属卷宗：四条职业路线各一件。
    /// 共同写法：基础处置人人可点；**路线专属处置**用 `whenMark` 锁在对应路线标记后
    /// （未确立路线时锁定可见并给原因——Disco 式可读性，而不是直接隐藏）。
    /// </summary>
    public static class ContentDossierRoutes
    {
        public static void Register()
        {
            RegisterIndustryLand();
            RegisterHeatingFund();
            RegisterRequisition();
            RegisterProvinceFund();
        }

        // ---------------- 产业转型：配套产业用地 ----------------

        static void RegisterIndustryLand()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_rt_ind",
                kind = "routine",
                form = "土地件",
                title = "关于高新区配套产业用地挂牌方案的请示",
                docNo = "同自然资〔2027〕41号",
                org = "市自然资源局",
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
                            "为保障华智能源区域基地配套需求，拟将高新区东区GX-07地块（工业用地，面积186亩）挂牌出让，起始价按工业用地基准地价的70%执行。",
                            "方案已征求市工信局意见，工信局回复“无不同意见”。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：地块情况表",
                        table = "地块编号|面积（亩）|规划用途|起始价（万元）|备注\nGX-07|186|工业|3910|拟定向配套\nGX-08|94|工业|1980|—",
                        paras = new List<string>
                        {
                            "（情况表由市自然资源局土地利用科编制。）",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_rt_ind_i1",
                        pageRef = "1",
                        detectHint = "“定向配套”是否有产业目录认定依据；起始价70%的执行口径有无文件出处",
                        ruleKey = "rule_tudi",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准挂牌",
                        complianceDelta = -2,
                        efficiencyDelta = 3,
                        gray = true,
                        effects = new Effects { exec = 1, logKind = "卷宗", logText = "照准配套用地挂牌（含定向条款）" },
                        result = "“同意，请按程序办理。”任慎拿到批示时看了一眼那行“拟定向配套”，什么都没说——他抽屉里那份没编号的纪要，还压着。",
                    },
                    new DossierOption
                    {
                        label = "退回：先补产业目录认定与价格依据",
                        complianceDelta = 3,
                        efficiencyDelta = -1,
                        effects = new Effects
                        {
                            professional = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "rensheng", evalv = -1, memo = "供地方案被退回补依据" } },
                            logKind = "卷宗", logText = "退回配套用地挂牌方案",
                        },
                        result = "你在签批栏写：“70%的依据是什么？定向的法律依据在哪一页？”——两个问号递上去，两天后回来的是两个文号。",
                    },
                    new DossierOption
                    {
                        label = "按产业目录认定后定向供地，方案同步报省备案",
                        whenMark = "route_industry",
                        lockReason = "需先确立「产业转型」路线",
                        complianceDelta = 3,
                        efficiencyDelta = 1,
                        effects = new Effects
                        {
                            admin = 1, political = 1,
                            setMarks = new List<string> { "ind_land_route" },
                            rel = new List<RelDelta> { new RelDelta { id = "rensheng", trust = 1, memo = "按产业目录定向供地并报省备案" } },
                            logKind = "卷宗", logText = "产业目录认定后定向供地（报省备案）",
                        },
                        result = "方案加了两页：目录认定结果，与省里的备案回执。企业少等了半个月，任慎把那份没编号的纪要扔进了碎纸机——有些纸，有了出处就不必留着。",
                    },
                },
            });
        }

        // ---------------- 民生兜底：供暖管网改造 ----------------

        static void RegisterHeatingFund()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_rt_ppl",
                kind = "routine",
                form = "财政件",
                title = "关于西城区供暖管网改造追加投资的请示",
                docNo = "同财〔2027〕116号",
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
                            "西城区三个小区供暖管网末端水力失衡，需改造一次管网4.2公里、换热站2座。申请追加投资1860万元。",
                            "资金来源：拟从年度预算预备费中列支。",
                            "妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：投资构成表",
                        table = "项目|金额（万元）\n一次管网|620\n换热站|480\n路面恢复|350\n不可预见费|280\n合计|1860",
                        paras = new List<string> { "（投资构成表由市财政局经济建设科编制，科长已核。）" },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_rt_ppl_i1",
                        pageRef = "2",
                        detectHint = "对合计与分项：620+480+350+280；不可预见费占直接费比例是否超8%",
                        ruleKey = "rule_shuzi",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准，从预备费列支",
                        complianceDelta = -1,
                        efficiencyDelta = 3,
                        effects = new Effects { exec = 1, logKind = "卷宗", logText = "照准供暖管网改造追加" },
                        result = "钱批下去，管子换上来。西城区那个拎保温杯的老大爷，今年冬天没来市政府门口——他不需要来了。",
                    },
                    new DossierOption
                    {
                        label = "要求压减不可预见费并重新测算",
                        complianceDelta = 2,
                        efficiencyDelta = -1,
                        effects = new Effects { admin = 1, logKind = "卷宗", logText = "供暖改造：压减不可预见费" },
                        result = "压到6%后重报，省下92万。方启年打电话来的时候语气很稳：“市长，我们改好了。”——稳的语气，往往意味着他们早就准备好了两套数。",
                    },
                    new DossierOption
                    {
                        label = "与老旧小区改造打包立项，一次改到位",
                        whenMark = "route_people",
                        lockReason = "需先确立「民生兜底」路线",
                        complianceDelta = 2,
                        efficiencyDelta = 1,
                        effects = new Effects
                        {
                            admin = 1, reputation = 2,
                            setMarks = new List<string> { "ppl_heat_pack" },
                            rel = new List<RelDelta> { new RelDelta { id = "fang", familiar = 1, memo = "供暖与老旧小区打包立项" } },
                            logKind = "卷宗", logText = "供暖与老旧小区改造打包立项",
                        },
                        result = "两份材料合成一本。挖开的路面只挖一次，居民的骂声也只挨一次。多花的报批时间，换来的是不再重复施工的三年。",
                    },
                },
            });
        }

        // ---------------- 项目攻坚：征地补偿方案 ----------------

        static void RegisterRequisition()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_rt_prj",
                kind = "routine",
                form = "土地件",
                title = "关于云冈区项目用地征地补偿安置方案的请示",
                docNo = "同自然资〔2027〕73号",
                org = "市自然资源局（转云冈区）",
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
                            "云冈区某村集体土地312亩拟征收，用于重点项目用地。补偿安置方案已与被征地村协商两次。",
                            "区片综合地价执行标准：每亩8.6万元；青苗及地上附着物据实补偿。",
                            "方案已于村务公开栏公示。妥否，请批示。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：协商记录摘要",
                        paras = new List<string>
                        {
                            "第一次协商：村民提出按邻区标准（每亩9.4万元）执行，区里答复“按现行区片价”。",
                            "第二次协商：到会村民代表19人，签字同意11人，不同意见8人。",
                            "听证：区里认为“不属于必须听证的情形”，未组织。",
                        },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_rt_prj_i1",
                        pageRef = "2",
                        detectHint = "协商未全部同意时的法定程序；是否属于应当组织听证的情形",
                        ruleKey = "rule_tudi",
                        severity = 3,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "照准实施，按现行区片价",
                        complianceDelta = -3,
                        efficiencyDelta = 4,
                        effects = new Effects
                        {
                            exec = 1,
                            integrity = new IntegrityRecord { tag = "征地补偿", note = "协商未一致即照准实施，未组织听证" },
                            logKind = "卷宗", logText = "照准征地补偿方案（未听证）",
                        },
                        result = "项目当月进场。三个月后，那8户中的3户把材料寄到了省里——寄材料的那天，村里正在量地。",
                    },
                    new DossierOption
                    {
                        label = "退回：补听证程序，并就区片价依据做书面说明",
                        complianceDelta = 3,
                        efficiencyDelta = -3,
                        effects = new Effects
                        {
                            professional = 2, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "rensheng", evalv = -2, memo = "征地方案被退回补听证" } },
                            logKind = "卷宗", logText = "退回征地补偿方案（补听证）",
                        },
                        result = "区里骂了三句“不懂基层”，然后组织了听证。听证会开了四个小时，最后区片价没动，但多了一条“困难户专项帮扶”——这一条，是那四个小时买来的。",
                    },
                    new DossierOption
                    {
                        label = "按市里统筹原则平衡两区标准，报省备案后实施",
                        whenMark = "route_project",
                        lockReason = "需先确立「项目攻坚」路线",
                        complianceDelta = 2,
                        efficiencyDelta = 0,
                        effects = new Effects
                        {
                            political = 2, admin = 1,
                            setMarks = new List<string> { "prj_std_balance" },
                            logKind = "卷宗", logText = "征地补偿：统筹两区标准报省备案",
                        },
                        result = "两个区的标准并到了一张表上。并表的那天，任慎在你办公室站了很久：“市长，这一并，全市以后的征地都要按这个数来。”你说：“那就按这个数来。”",
                    },
                },
            });
        }

        // ---------------- 向上争取：省级专项资金要件 ----------------

        static void RegisterProvinceFund()
        {
            DossierEngine.RegisterPool(new Dossier
            {
                id = "dz_rt_upl",
                kind = "routine",
                form = "对上报告",
                title = "关于申报省级产业转型专项资金要件情况的报告",
                docNo = "同发改〔2027〕98号",
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
                            "按照《省级产业转型专项资金管理办法》，现将我市申报要件报送如下：",
                            "一、项目情况：拟申报项目7个，总投资46.8亿元；二、前期进展：已完成可研6个、用地预审5个、环评4个；三、资金需求：申请省级支持3.2亿元；四、完成时限：2028年底。",
                            "妥否，请审核。",
                        },
                    },
                    new DossierPage
                    {
                        title = "附件：要件清单",
                        table = "要件|应报（项）|已报（项）\n可研批复|7|6\n用地预审|7|5\n环评批复|7|4\n配套资金承诺|7|7",
                        paras = new List<string> { "（清单由市发改局投资科编制，标注“缺件部分正在补办”。）" },
                    },
                },
                issues = new List<DossierIssue>
                {
                    new DossierIssue
                    {
                        id = "dz_rt_upl_i1",
                        pageRef = "2",
                        detectHint = "要件缺件率与“已全部具备申报条件”的表述是否矛盾；承诺函是否经财政确认",
                        ruleKey = "rule_duishang",
                        severity = 2,
                    },
                },
                options = new List<DossierOption>
                {
                    new DossierOption
                    {
                        label = "按现稿报送，缺件部分注明“正在补办”",
                        complianceDelta = -2,
                        efficiencyDelta = 3,
                        effects = new Effects
                        {
                            setMarks = new List<string> { "upl_report_loose" },
                            logKind = "卷宗", logText = "省专项资金：按现稿报送（含缺件）",
                        },
                        result = "报得快。省里初审退回一次，要求“要件齐备后再报”——退回的那一周，别的市把额度占了三成。",
                    },
                    new DossierOption
                    {
                        label = "补齐要件后再报，附缺件清单与补办时限",
                        complianceDelta = 3,
                        efficiencyDelta = -2,
                        effects = new Effects
                        {
                            professional = 1, admin = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "zhoujin", familiar = 1, memo = "专项资金要件补齐后报送" } },
                            logKind = "卷宗", logText = "省专项资金：补齐要件后报送",
                        },
                        result = "晚报十二天，一次过审。省里经办人在电话里说：“你们这份清单做得像台账。”——像台账的东西，通常也能进下一轮的口袋。",
                    },
                    new DossierOption
                    {
                        label = "由市长带队进省汇报，当面争取额度",
                        whenMark = "route_uplink",
                        lockReason = "需先确立「向上争取」路线",
                        complianceDelta = 2,
                        efficiencyDelta = 2,
                        effects = new Effects
                        {
                            political = 2, polCapital = 2, stress = 2,
                            setMarks = new List<string> { "upl_face_to_face" },
                            logKind = "卷宗", logText = "省专项资金：市长带队进省汇报",
                        },
                        result = "汇报二十分钟，主评人问了三个问题，第三个是“钱什么时候能形成实物工作量”。你答“三季度”。他记下了——记下的数字，是回来要还的。",
                    },
                },
            });
        }
    }
}
