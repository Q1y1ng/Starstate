using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>口径手册内容：开局常备 3 条；教学件查出问题时补授对应口径。</summary>
    public static class ContentRulebook
    {
        public static void Register()
        {
            Rulebook.Register(new RuleDef
            {
                id = "rule_shuzi",
                title = "财政资金测算：合计与分项勾稽",
                category = "数字",
                source = "市财政局《专项资金审核要点》〔2025〕内部",
                body = "一、附件合计必须等于分项之和；正文金额与附件不一致时，以附件明细为准并书面说明。\n二、不可预见费一般不超过直接费的 8%；超出须另附测算依据。\n三、签批前至少核一遍加法——错一个数，市政府就背一个数。",
            });
            Rulebook.Register(new RuleDef
            {
                id = "rule_xingwen",
                title = "公文处理：行文关系与要素",
                category = "程序",
                source = "《党政机关公文处理工作条例》· 市政府办公厅翻印",
                body = "一、不得越级行文；确需越级的，须同时抄送被越过的上级。\n二、请示应当一文一事；须有签批页或明确的拟办意见。\n三、附件、印章、成文日期缺一不可；缺件退回补正，不得以电话记录替代。",
            });
            Rulebook.Register(new RuleDef
            {
                id = "rule_tudi",
                title = "土地与项目：人情件识别",
                category = "土地",
                source = "市自然资源局纪律提示 · 纪委抄送",
                body = "一、涉及征地、用途变更、容积率调整的，须核对是否经法定程序与听证。\n二、材料中出现特定企业反复「协调」「加快」的，核对经办人与亲属关系声明。\n三、领导口头指示不得替代书面依据；可请示，但卷宗要留痕。",
            });
            Rulebook.Register(new RuleDef
            {
                id = "rule_suanfa",
                title = "算法辅助：法定权力主体不可让渡",
                category = "算法",
                source = "《AI治理法》施行口径 · 省司法署解读（2026）",
                body = "一、算法可以提供意见、排序、预警，不得代替法定权力主体作出最终决定。\n二、自动审批类系统须保留人工复核入口与完整日志；日志保存期一般不少于五年。\n三、「系统通过、窗口盖章」不能成为责任真空——签字栏仍须是人。",
            });
            Rulebook.Register(new RuleDef
            {
                id = "rule_duishang",
                title = "对上报告：统计口径与「新签约」",
                category = "统计",
                source = "省统计局《投资统计常见问题解答》",
                body = "一、「新签约项目」一般指正式合同；框架协议、意向书不得计入，除非省里书面放宽。\n二、完成率分母调整须报备；含水分的口径一经审计对账，签字人连带。\n三、对上材料宁可慢，不可假——假的追不回来。",
            });
            Rulebook.Register(new RuleDef
            {
                id = "rule_shigu",
                title = "安全生产：停产整顿与销号",
                category = "安全",
                source = "市应急管理局督办单样式 · 附复查销号清单要求",
                body = "一、「责令停产整顿」须有企业清单、整改期限、复查责任人。\n二、销号以现场复查记录为准，不得以企业自报替代。\n三、未销号即恢复生产的，倒查审批链条。",
            });
        }
    }
}
