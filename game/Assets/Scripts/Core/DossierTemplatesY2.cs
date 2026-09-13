using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 第二批模板（M2 上量：日常件的雷型与来文形态铺开）。
    /// 与 Y1 的区别：① 雷型更杂（比例超限、关联方、程序倒置、口径混淆）；
    /// ② 部分选项写**叙事标记**（`setMarks`）——模板件从此能挂链，
    ///    由 ContentTemplateChains 的延迟回响把「一次灰区处置」变成后续麻烦。
    /// </summary>
    internal static class DossierTemplatesY2
    {
        public static readonly DossierTemplate[] All = new DossierTemplate[]
        {
            new DossierTemplate
            {
                form = "突发事件", titlePattern = "关于{org}安全生产大检查情况的通报", docPrefix = "同应急",
                plant = "overfix",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "按照上级部署，我市开展安全生产大检查，共检查企业{n1}家，发现隐患{sum}项，已整改{n2}项。",
                    "拟对隐患严重的{n3}家企业挂牌督办。妥否，请批示。",
                },
                tablePattern = "项目|数量\n检查企业|{n1}\n发现隐患|{sum}\n已整改|{n2}\n挂牌督办|{n3}",
                issue = new DossierIssue { detectHint = "已整改数是否大于发现隐患数；挂牌企业是否附名单", pageRef = "2" },
                ruleKey = "rule_shigu", issueSeverity = 3,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "按现稿通报全市", complianceDelta = -2, efficiencyDelta = 3,
                        effects = new Effects { setMarks = new List<string> { "safety_loose" },
                            logKind = "卷宗", logText = "安全检查通报按现稿下发" },
                        result = "通报发得很快，各局都转发了。转发量很好看——直到下一处工地的脚手架开口说话。" },
                    new DossierOption { label = "要求重核整改数后再通报", complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects { exec = 1, logKind = "卷宗", logText = "安全通报要求重核整改数" },
                        result = "应急局的同志抱着台账回来时，脸色不太好：整改数里有三分之一是“承诺整改”。" },
                    new DossierOption { label = "同意通报，挂牌名单内部掌握", complianceDelta = 1, efficiencyDelta = 2,
                        effects = new Effects { political = 1, logKind = "卷宗", logText = "挂牌名单内部掌握" },
                        result = "名单没随通报下去。企业松了口气，区里也松了口气——松气的名单，往往就是下一次事故的候选。" },
                },
            },
            new DossierTemplate
            {
                form = "省交办", titlePattern = "关于{org}生态环境问题整改进展的报告", docPrefix = "同环",
                bodyParas = new List<string>
                {
                    "省有关署：",
                    "针对省署交办的{org}领域环境问题，我市已完成整改{sum}项，剩余{n1}项正在推进。",
                    "拟按此进度报送。",
                },
                tablePattern = "类别|数量\n已整改|{sum}\n在推进|{n1}\n其中跨年项目|{n2}",
                issue = new DossierIssue { detectHint = "“已完成整改”里是否含跨年未完工项目；佐证是否有现场照片", pageRef = "2" },
                ruleKey = "rule_shuzi", issueSeverity = 3,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "按现稿报送", complianceDelta = -2, efficiencyDelta = 3,
                        effects = new Effects { setMarks = new List<string> { "env_credit" },
                            logKind = "卷宗", logText = "环保整改进度按现稿报送" },
                        result = "数字报上去了，省里给了个“推进有力”。你想起那两个还在工地的项目——它们的进度条是画在纸上的。" },
                    new DossierOption { label = "把跨年项目单列并说明", complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects { professional = 1, logKind = "卷宗", logText = "环保整改跨年项目单列" },
                        result = "你把跨年那两栏单列出来。上报材料厚了一页，可信度却长了一截。" },
                },
            },
            new DossierTemplate
            {
                form = "财政件", titlePattern = "关于{org}医保基金专项检查处理情况的请示", docPrefix = "同医保",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "本次专项检查覆盖定点机构{n1}家，查处违规{sum}家，追回违规资金{n2}万元。",
                    "拟对违规机构以整改为主，不予行政处罚。妥否，请批示。",
                },
                tablePattern = "项目|数量\n检查机构（家）|{n1}\n查处违规（家）|{sum}\n追回资金（万元）|{n2}",
                issue = new DossierIssue { detectHint = "查处家数与追回金额是否匹配；“以整改为主”是否写在制度里", pageRef = "2" },
                ruleKey = "rule_shuzi", issueSeverity = 3,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "同意以整改为主", complianceDelta = -2, efficiencyDelta = 3,
                        gray = true,
                        effects = new Effects { setMarks = new List<string> { "yibao_soft" },
                            integrity = new IntegrityRecord { tag = "执法裁量", note = "医保违规以整改为主，未予行政处罚" },
                            logKind = "卷宗", logText = "医保违规以整改为主" },
                        result = "医院院长们很快知道了这条口径。基金的钱是大家的，处罚的刀是自己的——你选了后者。" },
                    new DossierOption { label = "对骗保情形一律移送", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { professional = 1, logKind = "卷宗", logText = "医保骗保线索移送" },
                        result = "移送函送出去三份。有人在电话里说“不至于吧”。你说：至不至于，问基金账户。" },
                    new DossierOption { label = "分清主观与过失后再定", complianceDelta = 1, efficiencyDelta = 1,
                        effects = new Effects { exec = 1, logKind = "卷宗", logText = "医保违规区分主客观" },
                        result = "你要求逐案写明主观故意与业务过失。这活儿很烦，但烦的活儿最经得起回头看。" },
                },
            },
            new DossierTemplate
            {
                form = "请示", titlePattern = "关于{org}校外培训机构治理情况的请示", docPrefix = "同教",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "我市已压减学科类培训机构{n1}家，现存{sum}家，涉及退费家长{n2}人。",
                    "对退费困难的机构，拟允许其“过渡期内继续消课”。妥否，请批示。",
                },
                tablePattern = "项目|数量\n已压减（家）|{n1}\n现存（家）|{sum}\n涉及退费家长（人）|{n2}",
                issue = new DossierIssue { detectHint = "现存家数与已压减数之和是否等于治理前总数；退费家长数与机构数是否对得上", pageRef = "2" },
                ruleKey = "rule_shuzi", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照准过渡期消课", complianceDelta = -1, efficiencyDelta = 3,
                        effects = new Effects { logKind = "卷宗", logText = "培训机构过渡期消课" },
                        result = "过渡期是缓冲，也是拖延的新名字。家长群里的消息你没有看，但你知道它们长什么样。" },
                    new DossierOption { label = "要求退费台账公开并设监管账户", complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects { setMarks = new List<string> { "edu_refund" },
                            logKind = "卷宗", logText = "培训机构退费台账公开" },
                        result = "监管账户三个字，让两家机构当晚就提交了退费计划——钱最怕见光。" },
                },
            },
            new DossierTemplate
            {
                form = "土地件", titlePattern = "关于{org}拟出让一宗国有建设用地使用权方案的请示", docPrefix = "同自然资",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "拟挂牌出让位于{n3}亩的地块，用途为工业，起始价{sum}万元，竞买保证金{n1}万元。",
                    "方案已经市自然资源局局务会研究。妥否，请批示。",
                },
                tablePattern = "项目|数值\n面积（亩）|{n3}\n起始价（万元）|{sum}\n保证金（万元）|{n1}",
                issue = new DossierIssue { detectHint = "起始价是否低于同期评估价；出让条件是否有排他性表述", pageRef = "1" },
                ruleKey = "rule_tudi", issueSeverity = 3,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照准挂牌", complianceDelta = -2, efficiencyDelta = 4,
                        effects = new Effects { reputation = 2, logKind = "卷宗", logText = "照准土地挂牌出让" },
                        result = "挂牌公告第二天就挂了。地是稀缺物，稀缺物前面的队伍里，总有人在看你手里的价。" },
                    new DossierOption { label = "要求重新评估并删去排他条款", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { professional = 1, logKind = "卷宗", logText = "土地出让重新评估删排他条款" },
                        result = "“产业定位相符”那一条被划掉了。招商的同志说这会影响拿地意愿。你说：影响的是拿地意愿，还是拿地的人？" },
                    new DossierOption { label = "同意出让，另设产业准入条件", complianceDelta = 0, efficiencyDelta = 2,
                        gray = true,
                        effects = new Effects { setMarks = new List<string> { "land_directed" },
                            logKind = "卷宗", logText = "土地出让另设产业准入" },
                        result = "准入条件写得体面：投资强度、亩均税收、开工时限。每一项都合法——每一项也都能筛人。" },
                },
            },
            new DossierTemplate
            {
                form = "协议", titlePattern = "关于{org}所属企业混合所有制改革职工安置方案的请示", docPrefix = "同国资",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "拟对{org}所属企业实施混改，涉及职工{n1}人，其中需安置{sum}人。",
                    "安置方案已经企业职代会审议。妥否，请批示。",
                },
                tablePattern = "项目|数量\n在册职工（人）|{n1}\n需安置（人）|{sum}\n职代会表决|通过",
                issue = new DossierIssue { detectHint = "需安置人数与在册人数是否吻合；职代会是否含劳务派遣人员", pageRef = "2" },
                ruleKey = "rule_xingwen", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照准实施", complianceDelta = -1, efficiencyDelta = 3,
                        effects = new Effects { logKind = "卷宗", logText = "照准混改职工安置方案" },
                        result = "方案通过得很快。职代会的掌声里，劳务派遣那几十个名字，没有出现在任何一页上。" },
                    new DossierOption { label = "要求把劳务派遣人员一并纳入", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { setMarks = new List<string> { "soe_dispatch" },
                            logKind = "卷宗", logText = "混改安置纳入劳务派遣" },
                        result = "企业负责人的表情很难形容。资产可以混改，人不能混过去——你把这句写在了批示里。" },
                },
            },
            new DossierTemplate
            {
                form = "对上报告", titlePattern = "关于{org}上半年主要经济指标完成情况的报告", docPrefix = "同统",
                bodyParas = new List<string>
                {
                    "省有关署：",
                    "上半年我市规模以上工业增加值增长{n1}%，固定资产投资增长{sum}%，社会消费品零售总额增长{n2}%。",
                    "预计可完成年度目标任务。",
                },
                tablePattern = "指标|增速（%）\n规上工业增加值|{n1}\n固定资产投资|{sum}\n社消零|{n2}",
                issue = new DossierIssue { detectHint = "增速与用电量、税收增速是否背离；是否注明口径调整", pageRef = "2" },
                ruleKey = "rule_shuzi", issueSeverity = 3,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "按现稿上报", complianceDelta = -2, efficiencyDelta = 4,
                        effects = new Effects { setMarks = new List<string> { "stat_fudge" },
                            logKind = "卷宗", logText = "经济指标按现稿上报" },
                        result = "数字报上去，省里点名表扬了句“稳中有进”。你知道那句表扬是押着明年的一笔账。" },
                    new DossierOption { label = "要求附用电量与税收对照", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { professional = 1, logKind = "卷宗", logText = "经济指标附用电税收对照" },
                        result = "表格多了一栏，两位统计局的同志连夜核对。数据不怕被问，怕的是没人问。" },
                    new DossierOption { label = "按口径调整后上报，注明基数变化", complianceDelta = 1, efficiencyDelta = 2,
                        effects = new Effects { admin = 1, logKind = "卷宗", logText = "指标注明口径调整" },
                        result = "一行“统计口径调整说明”加在末尾。加得老实，也加得聪明——回头有人较真，这一行就是桥。" },
                },
            },
            new DossierTemplate
            {
                kind = "cosign", form = "会议材料", titlePattern = "关于召开{org}防汛抗旱工作会议的请示", docPrefix = "同应急",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "拟召开防汛抗旱工作会议，通报汛前检查情况，部署下一阶段工作。",
                    "会议方案与责任清单附后，参会{n1}人。",
                },
                options = new List<DossierOption>
                {
                    new DossierOption { label = "同意，届时出席", complianceDelta = 1, efficiencyDelta = 1,
                        effects = new Effects { energy = -4, political = 1, logKind = "卷宗", logText = "出席防汛工作会议" },
                        result = "你出席并讲话。台下记笔记的人抬头看了你两次——他们想知道你会不会念到具体点位。" },
                    new DossierOption { label = "请邵志远同志出席，我要结果", complianceDelta = 0, efficiencyDelta = 2,
                        effects = new Effects { rel = new List<RelDelta> { new RelDelta { id = "shao", familiar = 2, memo = "代出席防汛会" } },
                            logKind = "卷宗", logText = "防汛会委托常务副出席" },
                        result = "邵志远去了。他回来后给你看的不是会议纪要，是一张整改清单——他懂你要什么。" },
                    new DossierOption { label = "改为现场点验，会议压后", complianceDelta = 2, efficiencyDelta = 0,
                        effects = new Effects { exec = 1, setMarks = new List<string> { "flood_onsite" },
                            logKind = "卷宗", logText = "防汛改现场点验" },
                        result = "会没开，人去了堤上。物资仓库里有两千条编织袋受潮，库管员说是“上一任留下的”。" },
                },
            },
            new DossierTemplate
            {
                form = "信访件", titlePattern = "关于{org}信访积案化解情况的报告", docPrefix = "同信",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "本季度交办信访积案{n1}件，已化解{sum}件，化解率{n2}%。",
                    "剩余案件均属历史遗留，拟稳步推进。",
                },
                tablePattern = "项目|数量\n交办积案（件）|{n1}\n已化解（件）|{sum}\n化解率（%）|{n2}",
                issue = new DossierIssue { detectHint = "化解是否以“程序性答复”计入；是否注明信访人是否签署息访", pageRef = "2" },
                ruleKey = "rule_shuzi", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照此销案", complianceDelta = -2, efficiencyDelta = 3,
                        effects = new Effects { setMarks = new List<string> { "petition_paper" },
                            logKind = "卷宗", logText = "信访积案按答复销案" },
                        result = "系统里的数字好看了。同一位上访人的名字，在另一个台账里，还躺着。" },
                    new DossierOption { label = "要求以群众签字为准销案", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { exec = 1, logKind = "卷宗", logText = "信访销案以签字为准" },
                        result = "半个月后，化解率从{n2}%掉到五成。掉下来的那一半，明年会以另一种方式回来。" },
                },
            },
            new DossierTemplate
            {
                form = "人事单", titlePattern = "关于{org}领导干部个人有关事项报告的备案意见", docPrefix = "同组",
                bodyParas = new List<string>
                {
                    "市人民政府党组：",
                    "本次集中申报{n1}人，其中{sum}人报告了配偶、子女从业情况。",
                    "经抽查，未发现与本人分管领域直接关联的情形。",
                },
                tablePattern = "项目|数量\n申报人数|{n1}\n报告配偶子女从业|{sum}\n抽查发现关联|0",
                issue = new DossierIssue { detectHint = "“未发现关联”的抽查比例是多少；是否含分管领域内企业", pageRef = "2" },
                ruleKey = "rule_xingwen", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "备案", complianceDelta = 0, efficiencyDelta = 2,
                        effects = new Effects { logKind = "卷宗", logText = "个人事项报告备案" },
                        result = "表格归档。归档的意思是：它们在那里，等到需要的时候。" },
                    new DossierOption { label = "要求对分管领域内从业情形逐人核", complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects { rel = new List<RelDelta> { new RelDelta { id = "han", familiar = 1, memo = "要求逐人核个人事项" } },
                            logKind = "卷宗", logText = "个人事项逐人核查" },
                        result = "组织部回话说要加班三天。你说：加三天，好过三年后有人替你加班。" },
                },
            },
            new DossierTemplate
            {
                form = "财政件", titlePattern = "关于{org}政府投资项目概算调整的请示", docPrefix = "同发改",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "因材料价格变动，{org}项目概算拟由{n1}万元调整为{sum}万元。",
                    "调整幅度在规定范围内，拟按程序报批。",
                },
                tablePattern = "项目|金额（万元）\n原概算|{n1}\n拟调整后|{sum}\n调增率|（见批注）",
                issue = new DossierIssue { detectHint = "调增率是否触及重新报批门槛；是否含未招标的暂列内容", pageRef = "2" },
                ruleKey = "rule_shuzi", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照准调整", complianceDelta = -2, efficiencyDelta = 3,
                        effects = new Effects { setMarks = new List<string> { "budget_split" },
                            logKind = "卷宗", logText = "照准项目概算调增" },
                        result = "概算调上去了。纸面上是“材料涨价”，工地上是“先干起来再说”。" },
                    new DossierOption { label = "要求重新评审后报批", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { professional = 1, logKind = "卷宗", logText = "概算要求重新评审" },
                        result = "评审中心的人来了，带着卷尺和清单。三天后他们的结论是：调增合理，但其中两项不该进来。" },
                },
            },
            new DossierTemplate
            {
                form = "土地件", titlePattern = "关于{org}城市更新项目房屋征收补偿方案的请示", docPrefix = "同住建",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "拟对{org}片区实施征收，涉及住户{n1}户，补偿总额{sum}万元。",
                    "评估机构已出具评估报告。妥否，请批示。",
                },
                tablePattern = "项目|数值\n征收住户（户）|{n1}\n补偿总额（万元）|{sum}\n评估机构|（见附件）",
                issue = new DossierIssue { detectHint = "评估机构是否与开发主体有关联；同一片区是否出现两套补偿标准", pageRef = "2" },
                ruleKey = "rule_tudi", issueSeverity = 3,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照准实施", complianceDelta = -2, efficiencyDelta = 4,
                        effects = new Effects { reputation = 2, logKind = "卷宗", logText = "照准征收补偿方案" },
                        result = "征收公告贴上墙那天，巷口站了很多老人。他们看告示的样子，像在看你。" },
                    new DossierOption { label = "要求更换评估机构并统一标准", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { exec = 1, logKind = "卷宗", logText = "征收更换评估机构" },
                        result = "新机构进场的第一件事是重测。重测结论比原报告低了 7%——差的那 7%，原来长在“关系”上。" },
                    new DossierOption { label = "先做两户试点再定", complianceDelta = 2, efficiencyDelta = -1,
                        effects = new Effects { admin = 1, logKind = "卷宗", logText = "征收先做两户试点" },
                        result = "两户签了，也两户闹了。试点把问题提前一个月摆到了你桌上——这一个月，值。" },
                },
            },
            new DossierTemplate
            {
                form = "信访件", titlePattern = "关于{org}营商环境投诉办理情况的通报", docPrefix = "同营",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "本季度受理企业投诉{n1}件，已办结{sum}件，企业满意度{n2}%。",
                    "投诉集中在审批环节多、材料重复提交。",
                },
                tablePattern = "项目|数值\n受理（件）|{n1}\n办结（件）|{sum}\n满意度（%）|{n2}",
                issue = new DossierIssue { detectHint = "满意度是否由被投诉单位自行回访采集；办结是否等于问题解决", pageRef = "2" },
                ruleKey = "rule_xingwen", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "阅知，按现口径通报", complianceDelta = 0, efficiencyDelta = 2,
                        effects = new Effects { logKind = "卷宗", logText = "营商投诉按现口径通报" },
                        result = "满意度{n2}%是个漂亮数。漂亮的数一般由被投诉的人自己算出来。" },
                    new DossierOption { label = "改为第三方回访并公开不满意件", complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects { setMarks = new List<string> { "yshj_open" },
                            logKind = "卷宗", logText = "营商投诉第三方回访" },
                        result = "第一次回访，满意度掉到六成。六成是真的，而真的东西才修得动。" },
                },
            },
            new DossierTemplate
            {
                form = "协议", titlePattern = "关于{org}政府采购项目质疑答复的请示", docPrefix = "同财",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "某项目中标结果公示后收到质疑{sum}件，采购人答复为“质疑不成立”。",
                    "拟维持原中标结果。妥否，请批示。",
                },
                tablePattern = "项目|数量\n质疑件|{sum}\n成立|0\n维持原结果|是",
                issue = new DossierIssue { detectHint = "“质疑不成立”是否逐条对应答复；评分表是否随答复一并附上", pageRef = "2" },
                ruleKey = "rule_xingwen", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "维持原结果", complianceDelta = -2, efficiencyDelta = 3,
                        effects = new Effects { setMarks = new List<string> { "procure_hold" },
                            logKind = "卷宗", logText = "采购质疑维持原结果" },
                        result = "维持最省事。质疑的企业会去投诉，投诉会来信，信会到你桌上——那时候你还会是“维持”。" },
                    new DossierOption { label = "要求重新组织评审", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { professional = 1, logKind = "卷宗", logText = "采购质疑重新评审" },
                        result = "复评结论：质疑有三条成立，中标结果变更。采购人代表的电话打到了你办公室，你没接。" },
                    new DossierOption { label = "让采购人与质疑人自行协商", complianceDelta = 0, efficiencyDelta = 1,
                        gray = true,
                        effects = new Effects { political = 1, logKind = "卷宗", logText = "采购质疑自行协商" },
                        result = "“自行协商”四个字写下去很容易。后来他们确实协商了——协商的结果是质疑人撤诉，和一份补充协议。" },
                },
            },
            new DossierTemplate
            {
                form = "财政件", titlePattern = "关于{org}医疗设备集中采购计划的请示", docPrefix = "同卫健",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "拟为市属医院采购医疗设备{n1}台（套），预算{sum}万元。",
                    "其中{n2}台（套）属单一来源采购。妥否，请批示。",
                },
                tablePattern = "项目|数值\n设备数量（台套）|{n1}\n预算（万元）|{sum}\n单一来源（台套）|{n2}",
                issue = new DossierIssue { detectHint = "单一来源理由是否充分；同型号历史采购单价是否可比", pageRef = "2" },
                ruleKey = "rule_shuzi", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照准采购", complianceDelta = -1, efficiencyDelta = 3,
                        effects = new Effects { logKind = "卷宗", logText = "照准医疗设备采购" },
                        result = "设备会很好用。好用和好价，中间隔着一个“只有它能满足需求”的说法。" },
                    new DossierOption { label = "要求比价并重填单一来源理由", complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects { professional = 1, logKind = "卷宗", logText = "医疗设备要求比价" },
                        result = "比价表回来后，有四种设备换了型号，省下三百多万。医院方面说“型号差一点”。你说：差一点的钱，够买两台。" },
                },
            },
            new DossierTemplate
            {
                form = "请示", titlePattern = "关于{org}义务教育招生划片调整方案的请示", docPrefix = "同教",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "因新建成小区交付，拟调整{n1}个片区划片范围，涉及学位{sum}个。",
                    "调整方案已征求街道意见。妥否，请批示。",
                },
                tablePattern = "项目|数量\n调整片区|{n1}\n涉及学位|{sum}\n新增小区|{n2}",
                issue = new DossierIssue { detectHint = "方案是否已公示并留出答问期；新增小区与学位数是否匹配", pageRef = "1" },
                ruleKey = "rule_xingwen", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照准实施", complianceDelta = 0, efficiencyDelta = 3,
                        effects = new Effects { logKind = "卷宗", logText = "照准招生划片调整" },
                        result = "划片图发下去，家长群炸了两天。第三天安静了——安静不代表接受，只代表他们找到了别的门。" },
                    new DossierOption { label = "要求公示并安排现场答问", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { comm = 1, setMarks = new List<string> { "edu_hearing" },
                            logKind = "卷宗", logText = "招生划片公示并答问" },
                        result = "答问会来了三百多人，话筒传了十七次。散场时有人对你说“谢谢”，也有人什么都没说——两种都算收获。" },
                },
            },
            new DossierTemplate
            {
                form = "突发事件", titlePattern = "关于{org}超限超载专项整治情况的报告", docPrefix = "同交通",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "专项整治期间查处超限车辆{n1}台次，罚款{sum}万元，已上缴财政{n2}万元。",
                    "重点货运源头企业已签订承诺书。",
                },
                tablePattern = "项目|数值\n查处（台次）|{n1}\n罚款（万元）|{sum}\n已上缴（万元）|{n2}",
                issue = new DossierIssue { detectHint = "罚款数与上缴数是否一致；是否执行罚缴分离", pageRef = "2" },
                ruleKey = "rule_shuzi", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照准结案", complianceDelta = -1, efficiencyDelta = 3,
                        effects = new Effects { logKind = "卷宗", logText = "治超整治照准结案" },
                        result = "路上确实安静了一阵。货车司机们说：“这阵子查得紧。”——紧的那阵子，就是全部的意思。" },
                    new DossierOption { label = "要求核查罚缴是否分离", complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects { setMarks = new List<string> { "zhichao_check" },
                            logKind = "卷宗", logText = "治超核查罚缴分离" },
                        result = "账对上了大半，有几笔“当场收取”没有票据。执法队长说那是“便民”。你说：便民的钱，进的是哪个口袋？" },
                },
            },
            new DossierTemplate
            {
                form = "突发事件", titlePattern = "关于{org}网络舆情情况的处置建议", docPrefix = "同宣",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "某短视频反映我市{org}相关问题，播放量已达{n1}万，评论区出现关联讨论{sum}条。",
                    "建议启动舆情应对，尽快降温。",
                },
                tablePattern = "项目|数值\n播放量（万）|{n1}\n评论（条）|{sum}\n转载|持续增长",
                issue = new DossierIssue { detectHint = "处置建议是“回应问题”还是“减少可见”；事实是否已核实", pageRef = "1" },
                ruleKey = "rule_xingwen", issueSeverity = 3,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "按建议降温，联系平台", complianceDelta = -3, efficiencyDelta = 3,
                        gray = true,
                        effects = new Effects { setMarks = new List<string> { "yq_delete" },
                            integrity = new IntegrityRecord { tag = "舆情处置", note = "要求平台对反映问题的视频降热" },
                            logKind = "卷宗", logText = "舆情要求平台降热" },
                        result = "热度确实降了。三天后，同一个问题被另一个人用另一种方式发了出来——播放量翻倍。" },
                    new DossierOption { label = "先核实事实，再公开回应", complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects { comm = 1, setMarks = new List<string> { "yq_respond" },
                            logKind = "卷宗", logText = "舆情核实后公开回应" },
                        result = "核实用了两天，回应用了三百字。评论区有人说“终于有人说话”，也有人说“说得好听”。两种都对。" },
                    new DossierOption { label = "请宣传部按既有口径统一答复", complianceDelta = 1, efficiencyDelta = 2,
                        effects = new Effects { political = 1, logKind = "卷宗", logText = "舆情交宣传部统一答复" },
                        result = "口径统一了，问题还在。统一的东西一般解决不了具体的事——它只解决具体的人。" },
                },
            },
            new DossierTemplate
            {
                form = "土地件", titlePattern = "关于{org}开发区闲置标准厂房处置方案的请示", docPrefix = "同开发",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "开发区现有闲置标准厂房{sum}万平方米，涉及企业{n1}家。",
                    "拟对长期闲置企业按协议收回，或协商变更用途。妥否，请批示。",
                },
                tablePattern = "项目|数值\n闲置面积（万平方米）|{sum}\n涉及企业（家）|{n1}\n其中享受过招商优惠|{n2}",
                issue = new DossierIssue { detectHint = "闲置企业是否享受过招商优惠与补贴；收回条款是否写明在协议里", pageRef = "2" },
                ruleKey = "rule_tudi", issueSeverity = 3,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "协商变更用途", complianceDelta = -1, efficiencyDelta = 3,
                        effects = new Effects { logKind = "卷宗", logText = "闲置厂房协商变更用途" },
                        result = "用途一变，闲置就有了新名字：转型。转型的账，等下一任审计来算。" },
                    new DossierOption { label = "按协议收回并追缴优惠款", complianceDelta = 3, efficiencyDelta = -2,
                        effects = new Effects { setMarks = new List<string> { "idle_reclaim" }, reputation = -1,
                            logKind = "卷宗", logText = "闲置厂房按协议收回" },
                        result = "收回通知送出去，有两家企业当天就找了市里领导“说明情况”。你把两份说明都批给了开发区：按协议办。" },
                    new DossierOption { label = "先约谈，给三个月整改期", complianceDelta = 2, efficiencyDelta = 0,
                        effects = new Effects { admin = 1, logKind = "卷宗", logText = "闲置厂房约谈给整改期" },
                        result = "三个月后会有企业开工，也会有企业再来找你。整改期是善意，善意需要账本。" },
                },
            },
            new DossierTemplate
            {
                form = "人事单", titlePattern = "关于{org}退役军人安置任务分解的请示", docPrefix = "同退役",
                bodyParas = new List<string>
                {
                    "市人民政府：",
                    "本年度需安置退役军人{n1}人，拟分解到{org}等{sum}个单位。",
                    "其中事业编制{n2}个。妥否，请批示。",
                },
                tablePattern = "项目|数值\n安置任务（人）|{n1}\n分解单位（个）|{sum}\n事业编制（个）|{n2}",
                issue = new DossierIssue { detectHint = "分解数与接收单位实际编制是否对得上；是否含已满编单位", pageRef = "2" },
                ruleKey = "rule_xingwen", issueSeverity = 2,
                options = new List<DossierOption>
                {
                    new DossierOption { label = "照此分解", complianceDelta = 0, efficiencyDelta = 3,
                        effects = new Effects { logKind = "卷宗", logText = "退役军人安置照此分解" },
                        result = "任务分下去，几家单位负责人当天就来“汇报困难”。困难是真的，编制也是真的。" },
                    new DossierOption { label = "要求逐单位核定编制再分解", complianceDelta = 3, efficiencyDelta = -1,
                        effects = new Effects { rel = new List<RelDelta> { new RelDelta { id = "han", familiar = 1, memo = "安置核定编制" } },
                            logKind = "卷宗", logText = "安置逐单位核定编制" },
                        result = "核定后，有两个单位确实满编，任务重新摊了。被摊到的单位不高兴——不过程的盘，最后都不好端。" },
                },
            },
        };
    }
}
