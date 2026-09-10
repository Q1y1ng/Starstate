using System;
using System.Collections.Generic;
using System.Text;
using Starstate.Core;
using CareerSys = Starstate.Core.Career;

namespace Starstate.Ui
{
    /// <summary>状态/档案/人物/日志面板的文本渲染（纯字符串构建）。</summary>
    public static class TextBuilders
    {
        public static string Bar(int v)
        {
            int n = v / 10;
            return new string('█', n) + new string('░', 10 - n);
        }

        public static string PhaseLabel(Phase p)
        {
            switch (p)
            {
                case Phase.Prologue: return "序章";
                case Phase.Day: return "工作日";
                case Phase.WeekPlan: return "周计划";
                case Phase.WeekEnd: return "周点评";
                case Phase.Weekend: return "周末";
                case Phase.MonthEnd: return "月度结算";
                case Phase.Ending: return "结局";
            }
            return "";
        }

        public static string CareerPanel(GameState st)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"职级：{st.grade}（自 {st.gradeSince}，任职 {CareerSys.GradeYears(st)} 年）");
            sb.AppendLine($"编制状态：{(string.IsNullOrEmpty(st.seconded) ? "在市履职" : st.seconded)}");
            sb.AppendLine($"职业路线：{(string.IsNullOrEmpty(st.route) ? "尚未选定主攻" : CareerSys.RouteName(st.route))}");
            sb.AppendLine();
            sb.AppendLine("—— 晋升轨道（总设定·冻结版） ——");
            sb.AppendLine("七品·市长 →(届中/届终考核+巡视干净+省里推荐) 六品·副省");
            sb.AppendLine("四品及以上禁破格；七品进高级干部序列。");
            sb.AppendLine($"本届已任：{CareerSys.GradeYears(st)} / 5 年（五年一届，十年＝两届）");
            sb.AppendLine($"帝国考试：{(st.examPassed ? "已通过（高等级）" : "未通过")}　政治学院：{(st.academyDone ? "已结业" : "未结业")}");
            sb.AppendLine();
            sb.AppendLine("—— 年度考核（双尺复合） ——");
            if (st.evals.Count == 0) sb.AppendLine("（尚未考核）");
            foreach (var ev in st.evals) sb.Append($"{ev.year}:{ev.grade}　");
            sb.AppendLine();
            sb.AppendLine($"考核优秀累计 {st.outstandingYears} 次｜程序违规累计 {st.violationCount} 条");
            sb.AppendLine();
            sb.AppendLine("—— 生活 ——");
            string fam = st.hasChild ? "已婚有孩" : st.married ? "已婚" : string.IsNullOrEmpty(st.partner) ? "—" : $"与{st.partner}";
            sb.AppendLine($"婚姻家庭：{fam}");
            sb.AppendLine($"住房：{st.housing}");
            return sb.ToString();
        }

        public static string TopBar(GameState st)
        {
            var d = GameClock.Parse(st.date);
            var p = st.player;
            int pending = st.pendingDossierIds.Count + (st.activeDossier != null && !st.activeDossier.resolved ? 1 : 0);
            return $"{GameClock.FmtFull(d)}   ｜   第 {st.week.index} 周   ｜   {PhaseLabel(st.phase)}   ｜   待办卷宗 {pending}\n" +
                   $"合规 {Bar(st.compliance)} {st.compliance}    效率 {Bar(st.efficiency)} {st.efficiency}    精力 {p.energy}    压力 {p.stress}";
        }

        public static string Status(GameState st)
        {
            var sb = new StringBuilder();
            var p = st.player;
            int age = 2026 - p.birthYear;
            sb.AppendLine($"姓名：{p.name}（{age}岁 · {p.birthYear}年生）");
            sb.AppendLine($"学历：{p.school} · {p.major}");
            sb.AppendLine($"职务：{p.unit} {p.post}");
            sb.AppendLine($"品级：{p.rank}");
            sb.AppendLine($"本届起任：{st.gradeSince}（已任 {Career.GradeYears(st)} 年）");
            if (!string.IsNullOrEmpty(st.route)) sb.AppendLine($"主攻方向：{st.route}");
            sb.AppendLine();
            sb.AppendLine("—— 能力 ——");
            sb.AppendLine($"专业能力   {Bar(p.attrs.professional)} {p.attrs.professional}");
            sb.AppendLine($"行政能力   {Bar(p.attrs.admin)} {p.attrs.admin}");
            sb.AppendLine($"执行能力   {Bar(p.attrs.exec)} {p.attrs.exec}");
            sb.AppendLine($"沟通能力   {Bar(p.attrs.comm)} {p.attrs.comm}");
            sb.AppendLine($"政治敏感度 {Bar(p.attrs.political)} {p.attrs.political}");
            sb.AppendLine();
            sb.AppendLine("—— 两把尺 ——");
            sb.AppendLine($"合规分 {Bar(st.compliance)} {st.compliance}　（程序与实体合法性）");
            sb.AppendLine($"效率分 {Bar(st.efficiency)} {st.efficiency}　（时限、积压、交办）");
            sb.AppendLine($"本年程序问题 {st.yearIntegrity} 次　累计违规 {st.violationCount} 次");
            sb.AppendLine();
            sb.AppendLine("—— 资源与状态 ——");
            sb.AppendLine($"精力 {p.energy} / 压力 {p.stress} / 士气 {p.morale}");
            sb.AppendLine($"社会声望 {p.reputation}　政治资本 {p.polCapital}");
            sb.AppendLine($"积蓄 {p.savings} 元（月结余约 {p.monthlyIn - p.monthlyOut} 元）");
            sb.AppendLine($"家庭：{(st.married ? st.partner : "—")}{(st.hasChild ? "，一子女" : "")}　住房：{st.housing}");
            return sb.ToString();
        }

        public static string Records(GameState st)
        {
            var sb = new StringBuilder();
            sb.AppendLine("—— 任务档案 ——");
            if (st.tasks.Count == 0) sb.AppendLine("（暂无）");
            int start = MathfMax(0, st.tasks.Count - 12);
            for (int i = start; i < st.tasks.Count; i++)
            {
                var t = st.tasks[i];
                sb.AppendLine($"{t.date}  [{t.grade}]  {t.title}（{t.note}）");
            }
            sb.AppendLine();
            sb.AppendLine("—— 材料档案 ——");
            if (st.documents.Count == 0) sb.AppendLine("（暂无）");
            foreach (var d in st.documents)
                sb.AppendLine($"{d.date}  {d.title}（署名：{d.signature}；{d.note}）");
            sb.AppendLine();
            sb.AppendLine("—— 程序合规标记（监察线数据） ——");
            if (st.integrity.Count == 0) sb.AppendLine("（无）");
            foreach (var r in st.integrity)
                sb.AppendLine($"{r.date}  [{r.tag}] {r.note}");
            sb.AppendLine();
            sb.AppendLine("—— 奖惩 ——");
            if (st.commendations.Count == 0) sb.AppendLine("（无）");
            foreach (var c in st.commendations)
                sb.AppendLine($"{c.date}  {c.text}");
            return sb.ToString();
        }

        public static string NpcPanel(GameState st)
        {
            var sb = new StringBuilder();
            foreach (var id in Npcs.Order)
            {
                var def = Npcs.Defs[id];
                var r = Npcs.Get(st, id);
                sb.AppendLine($"【{def.name}】{def.title}（{def.grade}）");
                sb.AppendLine($"  熟悉 {Bar(r.familiar)}  信任 {r.trust:+0;-0;0}  评价 {r.evalv:+0;-0;0}");
                if (r.memories.Count > 0)
                {
                    var m = r.memories[r.memories.Count - 1];
                    sb.AppendLine($"  最近记忆：{m.text}");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string LogPanel(GameState st)
        {
            var sb = new StringBuilder();
            sb.AppendLine("—— 经历时间线 ——");
            int start = MathfMax(0, st.log.Count - 40);
            for (int i = start; i < st.log.Count; i++)
            {
                var e = st.log[i];
                sb.AppendLine($"{e.date} [{e.kind}] {e.text}");
            }
            if (st.log.Count == 0) sb.AppendLine("（暂无记录）");
            return sb.ToString();
        }

        private static int MathfMax(int a, int b) { return a > b ? a : b; }
    }
}
