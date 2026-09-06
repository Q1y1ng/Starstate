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
            sb.AppendLine($"编制状态：{(string.IsNullOrEmpty(st.seconded) ? "在局履职" : st.seconded)}");
            sb.AppendLine($"职业路线：{CareerSys.RouteName(st.route)}");
            sb.AppendLine();
            sb.AppendLine("—— 晋升轨道（总设定·冻结版） ——");
            sb.AppendLine("吏三·科员 →(≥2年+考核合格) 吏二·副科");
            sb.AppendLine("吏二·副科 →(≥3年+考核优秀) 吏一·正科");
            sb.AppendLine("吏一·正科 →(满5年或优秀破格3年＋基层履历24月＋州级考试＋省政治学院1年) 十品·副处");
            sb.AppendLine($"基层履历：{st.baseExpMonths} / {CareerSys.BaseExpRequired} 个月");
            sb.AppendLine();
            sb.AppendLine("—— 年度考核（评优评先） ——");
            if (st.evals.Count == 0) sb.AppendLine("（尚未考核）");
            foreach (var ev in st.evals) sb.Append($"{ev.year}:{ev.grade}　");
            sb.AppendLine();
            sb.AppendLine($"考核优秀累计 {st.outstandingYears} 次｜程序标记 {st.violationCount} 条");
            sb.AppendLine();
            sb.AppendLine("—— 生活 ——");
            string fam = st.hasChild ? "已婚有孩" : st.married ? "已婚" : string.IsNullOrEmpty(st.partner) ? "单身" : $"与{st.partner}恋爱中";
            sb.AppendLine($"婚姻家庭：{fam}");
            sb.AppendLine($"住房：{st.housing}");
            return sb.ToString();
        }

        public static string TopBar(GameState st)
        {
            var d = GameClock.Parse(st.date);
            var p = st.player;
            return $"{GameClock.FmtFull(d)}   ｜   第 {st.week.index} 周   ｜   {PhaseLabel(st.phase)}\n" +
                   $"精力 {Bar(p.energy)} {p.energy}    压力 {Bar(p.stress)} {p.stress}    士气 {p.morale}";
        }

        public static string Status(GameState st)
        {
            var sb = new StringBuilder();
            var p = st.player;
            sb.AppendLine($"姓名：{p.name}（{DateTime.Now.Year - p.birthYear}岁 · 2003年生）");
            sb.AppendLine($"母校：{p.school} · {p.major}");
            sb.AppendLine($"单位：{p.unit}");
            sb.AppendLine($"科室/岗位：{p.post}");
            sb.AppendLine($"职级：{p.rank}");
            if (p.probationMonths < 12) sb.AppendLine($"试用期：{p.probationMonths} / 12 个月");
            sb.AppendLine();
            sb.AppendLine("—— 能力 ——");
            sb.AppendLine($"专业能力   {Bar(p.attrs.professional)} {p.attrs.professional}");
            sb.AppendLine($"行政能力   {Bar(p.attrs.admin)} {p.attrs.admin}");
            sb.AppendLine($"执行能力   {Bar(p.attrs.exec)} {p.attrs.exec}");
            sb.AppendLine($"沟通能力   {Bar(p.attrs.comm)} {p.attrs.comm}");
            sb.AppendLine($"政治敏感度 {Bar(p.attrs.political)} {p.attrs.political}");
            sb.AppendLine();
            sb.AppendLine("—— 资源与状态 ——");
            sb.AppendLine($"精力 {p.energy} / 压力 {p.stress} / 士气 {p.morale}");
            sb.AppendLine($"社会声望 {p.reputation}（速率随职级递增）");
            sb.AppendLine($"政治资本 {p.polCapital}");
            sb.AppendLine($"积蓄 {p.savings} 元（月结余约 {p.monthlyIn - p.monthlyOut} 元）");
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
