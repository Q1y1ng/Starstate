using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 模板卷宗定义（手写数据的结构）。DossierTemplatesY1/Y2 各放一批，
    /// <see cref="DossierGenerator"/> 只负责按 (date, seq) 纯确定地填充变量并生成实例。
    /// </summary>
    internal class DossierTemplate
    {
        public string kind = "routine";
        public string form = "请示";
        public string titlePattern;          // {org} 可用
        public string docPrefix;
        public List<string> bodyParas;
        public string tablePattern;          // 可空；{n1}{n2}{n3}{sum}
        public DossierIssue issue;           // 可空；detectHint 用手写
        public string ruleKey = "";
        public int issueSeverity = 2;
        public List<DossierOption> options;
        public int checkBudget = 2;
        public string plant = "sum";     // sum=合计与分项不符；overfix=已整改>发现问题；none=只出程序类软刑
    }

    /// <summary>第一批模板（M0/M1：日常件与省交办、巡视整改）。</summary>
    internal static class DossierTemplatesY1
    {
        public static readonly DossierTemplate[] All = new DossierTemplate[]
        {
            new DossierTemplate
                {
                    form = "财政件", titlePattern = "关于追加{org}专项业务经费的请示", docPrefix = "同财",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "因年度工作任务调整，申请追加专项业务经费{sum}万元，用于设备更新与一线保障。",
                        "经费测算见附件。妥否，请批示。",
                    },
                    tablePattern = "项目|金额（万元）\n设备更新|{n1}\n一线保障|{n2}\n不可预见费|{n3}\n合计|{sum}",
                    issue = new DossierIssue { detectHint = "分项之和是否等于合计；不可预见费是否超比", pageRef = "2" },
                    ruleKey = "rule_shuzi",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "照准", complianceDelta = -1, efficiencyDelta = 3,
                            effects = new Effects { logKind = "卷宗", logText = "照准专项经费追加" },
                            result = "“同意，请按规定办理。”钱从你笔下过，责任也从你笔下过。" },
                        new DossierOption { label = "要求压减不可预见费后报", complianceDelta = 2, efficiencyDelta = -1,
                            effects = new Effects { admin = 1, logKind = "卷宗", logText = "压减不可预见费" },
                            result = "财政局半小时后回电：“按市长要求压到5%。”声音很稳——他们知道你在看表。" },
                    },
                },
                new DossierTemplate
                {
                    form = "请示", titlePattern = "关于举办{org}业务技能竞赛的请示", docPrefix = "同府办",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "拟于下月举办全市系统业务技能竞赛，参赛约{n1}人，预算{sum}万元（含场地与奖品）。",
                        "为展示队伍风貌，拟邀请媒体适度报道。妥否，请批示。",
                    },
                    issue = new DossierIssue { detectHint = "预算是否含媒体接待与奖品超标", pageRef = "1" },
                    ruleKey = "rule_xingwen",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "照准", complianceDelta = 0, efficiencyDelta = 2,
                            effects = new Effects { logKind = "卷宗", logText = "照准技能竞赛" },
                            result = "活动会很热闹。热闹的事，审计一般不盯——除非奖品清单出现在不该出现的地方。" },
                        new DossierOption { label = "同意办，砍掉媒体专列预算", complianceDelta = 2, efficiencyDelta = 0,
                            effects = new Effects { political = 1, logKind = "卷宗", logText = "竞赛去媒体专列预算" },
                            result = "你划掉媒体专列那一行。办公室的人说：“市长，宣传口会不会有意见？”你说：“竞赛是赛出来的，不是报出来的。”" },
                    },
                },
                new DossierTemplate
                {
                    kind = "cosign", form = "会议材料", titlePattern = "关于召开{org}重点工作推进会的请示", docPrefix = "同府办",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "拟召开重点工作推进会，会期半天，参会{n1}人。需市政府领导出席并讲话。",
                        "会议方案与讲话稿初稿附后。",
                    },
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "同意，届时出席", complianceDelta = 1, efficiencyDelta = 1,
                            effects = new Effects { energy = -4, comm = 1, logKind = "卷宗", logText = "出席重点工作推进会" },
                            result = "你会出席。台下的人会记住你讲了哪三句——以及你没讲的那一句。" },
                        new DossierOption { label = "请邵志远同志代为出席", complianceDelta = 0, efficiencyDelta = 1,
                            effects = new Effects { rel = new List<RelDelta> { new RelDelta { id = "shao", familiar = 2, evalv = 1, memo = "代出席推进会" } },
                                logKind = "卷宗", logText = "委托常务副市长出席" },
                            result = "邵志远应得很痛快。痛快里有分寸：会他开了，镜头里是他的侧脸。" },
                        new DossierOption { label = "压后，与督查事项合并开", complianceDelta = 1, efficiencyDelta = -1,
                            effects = new Effects { admin = 1, logKind = "卷宗", logText = "推进会与督查合并" },
                            result = "你批：“少开会，多解决问题。与季度督查一并。”办公厅把两份方案钉在了一起。" },
                    },
                },
                new DossierTemplate
                {
                    form = "人事单", titlePattern = "关于{org}部分科级干部调整备案的报告", docPrefix = "同组",
                    bodyParas = new List<string>
                    {
                        "市人民政府党组：",
                        "根据干部队伍建设需要，拟对{n1}名科级干部进行岗位调整，现予备案。",
                        "调整方案已经局党组会议研究。",
                    },
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "备案", complianceDelta = 1, efficiencyDelta = 1,
                            effects = new Effects { rel = new List<RelDelta> { new RelDelta { id = "han", familiar = 1, memo = "科级调整备案顺畅" } },
                                logKind = "卷宗", logText = "科级干部调整备案" },
                            result = "人事的章盖下去很轻。轻的东西堆多了，就是山。" },
                        new DossierOption { label = "要求补充廉政意见与回避说明", complianceDelta = 2, efficiencyDelta = -1,
                            effects = new Effects { political = 1, rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 1, memo = "人事调整要求廉政意见" } },
                                logKind = "卷宗", logText = "人事调整补廉政意见" },
                            result = "沈砚那边很快回了函。纪委的效率，永远用在别人最不想看见的地方。" },
                    },
                },
                new DossierTemplate
                {
                    form = "信访件", titlePattern = "关于{org}服务窗口投诉集中情况的通报", docPrefix = "同信",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "近期12345热线涉及{org}窗口投诉{n1}件，主要集中在排号时间长与材料重复提交。",
                        "已约谈相关科室，拟优化叫号系统并公开材料清单。",
                    },
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "阅知，督促整改", complianceDelta = 0, efficiencyDelta = 2,
                            effects = new Effects { logKind = "卷宗", logText = "督促窗口投诉整改" },
                            result = "你签了“阅”。窗口的事，群众体感最直接——也最容易被总结成“已整改”。" },
                        new DossierOption { label = "要求一周内报整改前后对比数据", complianceDelta = 1, efficiencyDelta = 1,
                            effects = new Effects { exec = 1, logKind = "卷宗", logText = "窗口整改要对比数据" },
                            result = "没有对比的整改，叫表态。你要的是数。" },
                    },
                },
                new DossierTemplate
                {
                    form = "省交办", titlePattern = "关于落实省署交办{org}领域专项核查任务的报告", docPrefix = "同府办",
                    plant = "overfix",
                    bodyParas = new List<string>
                    {
                        "省有关署：",
                        "我市高度重视交办事项，已组织{org}等部门开展核查。",
                        "现将阶段性情况报告如下：核查点位{n1}处，发现问题{sum}项，已完成整改{n2}项。",
                    },
                    tablePattern = "类别|数量\n核查点位|{n1}\n发现问题|{sum}\n完成整改|{n2}",
                    issue = new DossierIssue { detectHint = "完成整改数是否大于发现问题数；是否留有未完成清单", pageRef = "2" },
                    ruleKey = "rule_shuzi",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "按现稿报送", complianceDelta = -2, efficiencyDelta = 3,
                            effects = new Effects { setMarks = new List<string> { "province_report_loose" },
                                logKind = "卷宗", logText = "省交办核查按现稿报送" },
                            result = "报得快，省里高兴。数若打架，对账时就不高兴了。" },
                        new DossierOption { label = "核对无误后再报，并附未完成清单", complianceDelta = 3, efficiencyDelta = -1,
                            effects = new Effects { professional = 1, logKind = "卷宗", logText = "省交办核查核实后报送" },
                            result = "多花了一天。这一天买的是：省里下次交办还找你——因为你的数不用返工。" },
                    },
                },
                new DossierTemplate
                {
                    form = "巡视整改", titlePattern = "关于巡视反馈{org}相关问题整改销号的报告", docPrefix = "同巡整",
                    bodyParas = new List<string>
                    {
                        "市委巡视整改工作领导小组：",
                        "针对巡视反馈的{n1}个问题，已完成整改{sum}个，拟予销号。",
                        "整改台账及佐证材料附后。",
                    },
                    issue = new DossierIssue { detectHint = "销号是否以现场复查为准，还是以台账自报", pageRef = "1" },
                    ruleKey = "rule_shigu",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "同意销号", complianceDelta = -2, efficiencyDelta = 3,
                            effects = new Effects { setMarks = new List<string> { "xun_paper_close" },
                                logKind = "卷宗", logText = "巡视整改纸面销号" },
                            result = "台账很漂亮。巡视组若杀回马枪，漂亮就是罪证。" },
                        new DossierOption { label = "抽查两处现场后再销", complianceDelta = 3, efficiencyDelta = -2,
                            effects = new Effects { exec = 1, logKind = "卷宗", logText = "巡视整改抽查现场" },
                            result = "你点了两处点位。一处过关，一处还堆着建筑垃圾。销号章收回来了。" },
                    },
                },
                new DossierTemplate
                {
                    form = "协议", titlePattern = "关于{org}购买第三方服务合同续签的请示", docPrefix = "同财",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "拟与服务商续签一年合同，金额{sum}万元，服务范围与上年一致。",
                        "上年履约评价为“合格”。妥否，请批示。",
                    },
                    issue = new DossierIssue { detectHint = "“合格”是否等于不换供应商；是否履行重新采购程序", pageRef = "1" },
                    ruleKey = "rule_xingwen",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "照准续签", complianceDelta = -1, efficiencyDelta = 3,
                            effects = new Effects { logKind = "卷宗", logText = "照准第三方服务续签" },
                            result = "续签最省事。省事的路径上，往往长着利益的草。" },
                        new DossierOption { label = "要求履行重新采购或说明单一来源依据", complianceDelta = 3, efficiencyDelta = -2,
                            effects = new Effects { professional = 1, logKind = "卷宗", logText = "续签要求重新采购程序" },
                            result = "采购科的同志抱着文件夹在你门口站了十秒，才敲门。程序是慢，但程序会保护签字的人。" },
                    },
                },
                new DossierTemplate
                {
                    form = "请示", titlePattern = "关于{org}政务公开事项清单更新的报告", docPrefix = "同府办",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "根据政务公开条例，拟更新{org}主动公开事项清单，涉及事项{n1}项，新增{n2}项、调整{n3}项。",
                        "现报请审定。",
                    },
                    issue = new DossierIssue { detectHint = "调整项是否含应公开未公开的审批结果", pageRef = "1" },
                    ruleKey = "rule_xingwen",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "照准", complianceDelta = 1, efficiencyDelta = 2,
                            effects = new Effects { logKind = "卷宗", logText = "照准政务公开清单更新" },
                            result = "公开是最好的防腐剂——也是最省事的挡箭牌。清单报出去，接下来是真公开还是目录公开，看执行。" },
                        new DossierOption { label = "要求审批结果类事项全部纳入主动公开", complianceDelta = 3, efficiencyDelta = -1,
                            effects = new Effects { political = 1, logKind = "卷宗", logText = "审批结果纳入主动公开" },
                            result = "有科室打电话来问能不能“分步”。你说：能，从今天那一步开始。" },
                    },
                },
                new DossierTemplate
                {
                    form = "人事单", titlePattern = "关于{org}所属事业单位公开招聘方案的请示", docPrefix = "同人社",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "拟组织{org}所属事业单位公开招聘，计划{n1}个岗位，报名约{n2}人。",
                        "笔试面试委托第三方。妥否，请批示。",
                    },
                    issue = new DossierIssue { detectHint = "第三方是否具备资质；面试考官回避是否写明", pageRef = "1" },
                    ruleKey = "rule_xingwen",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "照准", complianceDelta = 0, efficiencyDelta = 3,
                            effects = new Effects { logKind = "卷宗", logText = "照准事业单位招聘" },
                            result = "招聘是给别人一条路，也是给自己埋一份将来有人翻的卷。" },
                        new DossierOption { label = "要求纪检全程监督并公示成绩", complianceDelta = 3, efficiencyDelta = -1,
                            effects = new Effects { rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 1, memo = "招聘要求纪检监督" } },
                                logKind = "卷宗", logText = "招聘：纪检监督+成绩公示" },
                            result = "沈砚那边回了两个字：“可以。”——纪委很少用三个字。" },
                    },
                },
                new DossierTemplate
                {
                    form = "财政件", titlePattern = "关于{org}政府购买服务年度评估的报告", docPrefix = "同财",
                    plant = "none",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "对{org}上年政府购买服务项目进行评估，涉及合同{sum}万元，履约评价“合格”及以上占{n1}%。",
                        "拟对评价末位的{n2}个项目不再续约。",
                    },
                    tablePattern = "项目数|{n1}\n金额（万元）|{sum}\n拟不续约|{n2}",
                    issue = new DossierIssue { detectHint = "末位淘汰是否执行；评价主体是否与服务商有关联", pageRef = "2" },
                    ruleKey = "rule_shuzi",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "照准执行", complianceDelta = 1, efficiencyDelta = 2,
                            effects = new Effects { exec = 1, logKind = "卷宗", logText = "购买服务末位不续约" },
                            result = "不续约比续约难——总有人来问“为什么是我”。评估表替你回答了一半。" },
                        new DossierOption { label = "要求评估底稿留档备查", complianceDelta = 2, efficiencyDelta = 0,
                            effects = new Effects { professional = 1, logKind = "卷宗", logText = "购买服务评估底稿留档" },
                            result = "底稿进了柜子。柜子不说话，但审计来的时候会开口。" },
                    },
                },
                new DossierTemplate
                {
                    kind = "cosign", form = "省交办", titlePattern = "关于配合省督查组开展{org}领域专项督查的通知", docPrefix = "同府办",
                    bodyParas = new List<string>
                    {
                        "各有关单位：",
                        "省督查组将于近日就{org}领域重点工作开展专项督查，请准备台账、点位与汇报材料。",
                        "督查期间实行日报告制度。",
                    },
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "照准部署，我参加汇报会", complianceDelta = 1, efficiencyDelta = 2,
                            effects = new Effects { energy = -5, political = 1, logKind = "卷宗", logText = "部署省督查配合" },
                            result = "汇报会你坐在中间。省里同志问得很细——细到某张表的某一格。你庆幸自己核过。" },
                        new DossierOption { label = "请邵志远同志牵头，我听汇报", complianceDelta = 0, efficiencyDelta = 2,
                            effects = new Effects { rel = new List<RelDelta> { new RelDelta { id = "shao", trust = 1, evalv = 1, memo = "省督查牵头授权" } },
                                logKind = "卷宗", logText = "省督查委托常务副牵头" },
                            result = "邵志远眼睛亮了一下。牵头省督查，是露脸，也是扛事——他两样都要。" },
                        new DossierOption { label = "要求先自查自纠，问题清单内部消化", complianceDelta = 2, efficiencyDelta = -1,
                            gray = true,
                            effects = new Effects { setMarks = new List<string> { "province_dd_selfcheck" },
                                integrity = new IntegrityRecord { tag = "督查配合", note = "省督查前组织自查，部分问题内部消化" },
                                logKind = "卷宗", logText = "省督查前自查自纠" },
                            result = "自查清单你看了三遍。有两处你划掉了——划掉的意思是：这两条，不能出现在省里桌上。为什么不能，你没写。" },
                    },
                },
                new DossierTemplate
                {
                    form = "信访件", titlePattern = "关于{org}系统职工待遇诉求的研判报告", docPrefix = "同信",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "{org}部分职工反映绩效发放滞后、加班补助未落实，涉及约{n1}人。",
                        "经研判，若月底前无明确方案，存在联名信风险。",
                    },
                    issue = new DossierIssue { detectHint = "财政方案是否回避编制内外差别", pageRef = "1" },
                    ruleKey = "rule_shuzi",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "责成财政与人社一周内出方案", complianceDelta = 1, efficiencyDelta = 3,
                            effects = new Effects { exec = 1, stress = 2, logKind = "卷宗", logText = "职工待遇诉求限时方案" },
                            result = "方案第三天到了你桌上。编制内外两列数字，你把笔停在第二列——那一列更长，也更难。" },
                        new DossierOption { label = "约谈单位主要负责人，压责任", complianceDelta = 2, efficiencyDelta = 1,
                            effects = new Effects { political = 1, logKind = "卷宗", logText = "约谈职工待遇责任人" },
                            result = "对方从你办公室出来时领带松了半格。责任压实的样子，有时候就是半格领带。" },
                    },
                },
                new DossierTemplate
                {
                    form = "协议", titlePattern = "关于{org}与企业合作建设实训基地的框架协议（送审稿）", docPrefix = "同人社",
                    bodyParas = new List<string>
                    {
                        "市人民政府：",
                        "拟与企业合作建设实训基地，企业出资{sum}万元，政府提供场地与政策支持。",
                        "协议约定：企业优先录用本地学员{n1}名/年。妥否，请批示。",
                    },
                    issue = new DossierIssue { detectHint = "“优先录用”是否可考核；场地作价是否经评估", pageRef = "1" },
                    ruleKey = "rule_tudi",
                    options = new List<DossierOption>
                    {
                        new DossierOption { label = "照准签约", complianceDelta = -2, efficiencyDelta = 4,
                            effects = new Effects { reputation = 2, logKind = "卷宗", logText = "照准实训基地框架协议" },
                            result = "签约照片会很好看。好看的东西，审计一般第二年才看。" },
                        new DossierOption { label = "要求录用指标写入合同并违约追责", complianceDelta = 3, efficiencyDelta = -1,
                            effects = new Effects { professional = 1, logKind = "卷宗", logText = "实训基地：录用指标入合同" },
                            result = "企业代表皱了下眉，又松开：“可以谈。”——能谈的条款，才是真条款。" },
                    },
                },
            };
    }
}
