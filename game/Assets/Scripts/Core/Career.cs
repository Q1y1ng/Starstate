using System;
using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 职业生涯系统：吏轨晋升与转官规则全部依据总设定（冻结版晋升年限表）。
    /// 吏三→吏二≥2年＋考核合格；吏二→吏一≥3年＋考核优秀；
    /// 转官：吏一满一届5年（优秀破格满3年，须特批）＋基层公共事务履历≥24个月
    ///      ＋州级转官考试＋省政治学院1年 → 十品·副处。
    /// </summary>
    public static class Career
    {
        public const int PromoteYearsLi2 = 2;     // 吏三→吏二
        public const int PromoteYearsLi1 = 3;     // 吏二→吏一
        public const int ExamYearsStandard = 5;   // 吏一满一届
        public const int ExamYearsFast = 3;       // 优秀破格
        public const int ExamFastOutstanding = 2; // 破格所需优秀次数
        public const int BaseExpRequired = 24;    // 基层履历（月）

        public static int GradeYears(GameState st)
        {
            var since = GameClock.Parse(st.gradeSince);
            var now = GameClock.Parse(st.date);
            return (int)((now - since).TotalDays / 365.25);
        }

        public static bool CanPromoteLi2(GameState st)
        {
            return st.grade.StartsWith("吏三") && GradeYears(st) >= PromoteYearsLi2 && HasPassingEval(st);
        }

        public static bool CanPromoteLi1(GameState st)
        {
            return st.grade.StartsWith("吏二") && GradeYears(st) >= PromoteYearsLi1 && st.outstandingYears >= 1;
        }

        /// <summary>是否已具备参加州级转官考试的条件（优秀破格或届满）。</summary>
        public static bool ExamEligible(GameState st, out string reason)
        {
            reason = "";
            if (!st.grade.StartsWith("吏一")) { reason = "需先晋升吏一（正科）"; return false; }
            if (st.examPassed) { reason = "已通过考试"; return false; }
            int years = GradeYears(st);
            bool standard = years >= ExamYearsStandard;
            bool fast = years >= ExamYearsFast && st.outstandingYears >= ExamFastOutstanding;
            if (!standard && !fast)
            {
                reason = standard ? "" : (years < ExamYearsFast
                    ? $"任吏一满{ExamYearsFast}年且考核优秀{ExamFastOutstanding}次可申请优秀破格，满{ExamYearsStandard}年按届满报考"
                    : $"优秀破格需考核优秀{ExamFastOutstanding}次（现有{st.outstandingYears}次）");
                return false;
            }
            if (st.baseExpMonths < BaseExpRequired)
            {
                reason = $"基层公共事务履历不足（{st.baseExpMonths}/{BaseExpRequired} 个月）";
                return false;
            }
            return true;
        }

        public static bool HasPassingEval(GameState st)
        {
            for (int i = st.evals.Count - 1; i >= 0; i--)
                if (st.evals[i].grade == "称职" || st.evals[i].grade == "优秀") return true;
            return st.evals.Count == 0; // 首年未考核视为可晋升
        }

        /// <summary>年度考核评定（评优评先：竞争性）。评优失败降为称职；严重程序问题封顶基本称职。</summary>
        public static string EvaluateYear(GameState st, bool compete, System.Random rng)
        {
            if (st.yearIntegrity >= 2) return "基本称职";
            int score = st.yearGradePoints
                      + Math.Max(-10, Math.Min(15, RelEvalSum(st) / 5))
                      + (compete ? 2 : 0)
                      + (rng != null ? rng.Next(0, 12) : 6);
            if (compete && score >= 68) return "优秀";
            return "称职";
        }

        private static int RelEvalSum(GameState st)
        {
            int s = 0;
            var r = st.relations.Find(x => x.id == "zhou");
            if (r != null) s += r.evalv;
            var m = st.relations.Find(x => x.id == "ma");
            if (m != null) s += m.evalv / 2;
            return s;
        }

        /// <summary>十年结局计算：多结局（Q4-04）。被审查调查 / 辞职 / 十品 / 吏一骨干 / 平稳行者 / 十年一日。</summary>
        public static EndingData ComputeEnding(GameState st)
        {
            var e = new EndingData();
            var p = st.player;
            int age = 2036 - p.birthYear;
            string family = st.hasChild ? "已婚有孩，家里有了新的牵挂"
                          : st.married ? "已婚，两个人把日子过成了同盟"
                          : string.IsNullOrEmpty(st.partner) ? "孑然一身，把机关当家" : "恋爱中，人生多了半个人的分量";

            if (st.underInvestigation)
            {
                e.title = "结局 · 接受审查调查";
                e.paras.Add($"2036年，{p.name}因涉嫌严重违反程序被留置审查。档案上那些“当年觉得没什么”的签名，此刻一页页摊在桌面上。");
                e.paras.Add("监察条例写得清楚：程序的每一次让步，都是欠给制度的一笔债。总有一天，连本带息。");
            }
            else if (st.resigned)
            {
                e.title = "结局 · 转身离开";
                e.paras.Add($"{p.name}递辞呈那天，长安在下小雨。工牌交还人事科的那一刻，十年机关生涯归档。");
                e.paras.Add($"有人惋惜，有人不解。但只有你自己知道：{family}，今后的路，换一种走法。");
            }
            else if (st.grade.StartsWith("十品"))
            {
                e.title = "结局 · 十品副处";
                e.paras.Add($"{age}岁的{p.name}，从科员到副处——用十年走完了许多人半辈子的路。组织鉴定上写着：“善于学习，作风扎实，程序意识强。”");
                e.paras.Add($"上任那天，你路过综合科的窗，看见一个新的年轻人正在给你的绿萝浇水。{family}。");
            }
            else if (st.grade.StartsWith("吏一"))
            {
                e.title = st.outstandingYears >= 2 ? "结局 · 正科骨干，转官在即" : "结局 · 机关中坚";
                e.paras.Add(st.outstandingYears >= 2
                    ? $"考核优秀的次数攒够了，州级转官考试的准考证就在抽屉里。{age}岁的正科，前路清晰。"
                    : $"{p.name}成了局里公认的熟手：材料把关、数据口径、部门协调，样样离不得。{family}。");
            }
            else if (st.grade.StartsWith("吏二"))
            {
                e.title = "结局 · 平稳行者";
                e.paras.Add($"十年，从吏三到吏二。没有火箭式的速度，但每一份材料都经得起翻阅。{family}。");
                e.paras.Add("赵姐退休那天说过：“平平稳稳，也是一种本事。”你如今懂了这句话的分量。");
            }
            else
            {
                e.title = "结局 · 十年一日";
                e.paras.Add($"十年科员。你把最普通的位置坐成了钉子——局里谁都说你“可靠”，只是档案上的职级没什么变化。{family}。");
            }

            // —— 个人史回响（Phase 4）：结局由你一路留下的叙事标记装配 ——
            AppendMarkEchoes(st, e);

            // 数据段
            e.paras.Add($"—— 十年档案 ——");
            e.paras.Add($"职级：{st.grade}｜路线：{RouteName(st.route)}｜考核优秀 {st.outstandingYears} 次｜基层履历 {st.baseExpMonths} 个月");
            e.paras.Add($"经手任务 {st.tasks.Count} 项｜材料 {st.documents.Count} 份｜奖惩：表扬 {st.commendations.Count} 次、程序标记 {st.integrity.Count} 条");
            e.paras.Add($"积蓄 {p.savings} 元｜住房：{st.housing}｜社会声望 {p.reputation}｜政治资本 {p.polCapital}");
            return e;
        }

        public static string RouteName(string r)
        {
            switch (r)
            {
                case "笔杆子": return "综合文秘（笔杆子）";
                case "产业经济": return "产业经济";
                case "投资项目": return "投资项目";
                case "区域协调": return "区域协调";
            }
            return string.IsNullOrEmpty(r) ? "未定" : r;
        }

        /// <summary>结局个人史：按叙事标记装配“十年回响”段落——选择过的东西，结局时都回来。</summary>
        private static void AppendMarkEchoes(GameState st, EndingData e)
        {
            var lines = new List<string>();
            if (st.ambition == "做事") lines.Add("你当初想“做点实际的事”——经手的台账与项目，替你守住了这句话。");
            else if (st.ambition == "晋升") lines.Add($"你当初想往上走——{st.grade}，是你给自己的回执。");
            else if (st.ambition == "安稳") lines.Add("你当初想把日子过安稳——如今回头看，安稳确实是一种需要本事的东西。");
            else if (st.ambition == "搞钱") lines.Add($"你当初想让家里宽裕些——积蓄 {st.player.savings} 元，是你十年的另一种答卷。");

            if (st.HasMark("ledger_refused")) lines.Add("2027年那份台账，你顶住了——多年后每当有人问起“程序值不值”，你都有底气。");
            else if (st.HasMark("ledger_complicit")) lines.Add("2027年那份台账，你调了数——这些年它像一枚软钉子，偶尔在你的梦里硌一下。");
            else if (st.HasMark("ledger_reported")) lines.Add("2027年那份台账，你选择上报——科长当时没说什么，但后来的很多事说明他记住了。");
            if (st.HasMark("ai_seconded")) lines.Add("算法备案专班的那一年，让你在AI时代的档案里留下了自己的名字。");
            if (st.HasMark("house_owned_m")) lines.Add("那套咬着牙买下的房子，从“负担”慢慢变成了“底气”。");
            else if (st.HasMark("house_family_m")) lines.Add("房子首付里有父母的存折——你一直记得那份重量的利息该怎么还。");
            if (st.HasMark("family_care_m")) lines.Add("陪父亲复查的那个秋天，你请的假在考勤表上留过痕——但你从没后悔过一天。");
            if (st.HasMark("laokang_friend")) lines.Add("青溪乡的老康一直记得你。基层一年的回报，有时要等很多年才到账。");
            if (st.HasMark("dream_kept_m")) lines.Add("抽屉里那句入职时写下的话，你留到了最后——它没白被写下。");
            if (st.HasMark("stay_clean_m")) lines.Add("同学递来的橄榄枝你接了又放下——不是不心动，是你确认了自己要什么。");
            if (st.HasMark("marathon_m")) lines.Add("那场马拉松的奖牌还在书柜里：42.195公里教会你的事，机关里一样用得上。");
            if (lines.Count == 0) return;
            e.paras.Add("—— 十年回响 ——");
            foreach (var l in lines) e.paras.Add(l);
        }
    }
}
