using System;

namespace Starstate.Core
{
    /// <summary>
    /// 职业生涯（Phase 5）：七品市长 → 六品副省 / 其他结局。
    /// 依据总设定：一届＝5 年；七品晋升六品需中央年度考核累计优良＋履历完整。
    /// 四品及以上禁破格——六品起满届说话。
    /// </summary>
    public static class Career
    {
        public const int TermYears = 5;
        public const int PromoteYearsTo6 = 5;   // 七品满一届方可竞争六品
        public const int PromoteFast = 3;       // 理论破格线
        public const int FastTrackMarks = 3;    // 开局特批次数（巡视质疑源）
        public const int BaseExpRequired = 24;  // 旧吏轨兼容常量

        public static int GradeYears(GameState st)
        {
            var since = GameClock.Parse(st.gradeSince);
            var now = GameClock.Parse(st.date);
            return (int)((now - since).TotalDays / 365.25);
        }

        // —— 旧吏轨 API 桩：Phase 5 已改为七品路径；保留签名供 Flow 旧动态事件编译通过 ——
        public static bool CanPromoteLi2(GameState st) => false;
        public static bool CanPromoteLi1(GameState st) => false;
        public static bool ExamEligible(GameState st, out string reason) { reason = "Phase 5：七品路径不走州级转官考试"; return false; }
        public static string RouteName(string route)
        {
            switch (route)
            {
                case "industry": return "产业转型";
                case "people": return "民生兜底";
                case "project": return "项目攻坚";
                case "uplink": return "向上争取";
                default: return string.IsNullOrEmpty(route) ? "（未定）" : route;
            }
        }

        public static bool HasPassingEval(GameState st)
        {
            for (int i = st.evals.Count - 1; i >= 0; i--)
                if (st.evals[i].grade == "称职" || st.evals[i].grade == "优秀") return true;
            return st.evals.Count == 0;
        }

        /// <summary>是否具备竞争六品（副省级市主官/省厅正职）的基本年限条件。</summary>
        public static bool CanCompete6(GameState st, out string reason)
        {
            reason = "";
            if (!st.grade.StartsWith("七品")) { reason = "需在七品任上"; return false; }
            int years = GradeYears(st);
            if (years < PromoteYearsTo6)
            {
                reason = $"现届已任 {years} 年，满 {PromoteYearsTo6} 年（一届）方可进入六品酝酿";
                return false;
            }
            if (st.compliance < 55)
            {
                reason = $"合规分偏低（{st.compliance}），御史评价会压住名单";
                return false;
            }
            if (st.efficiency < 45)
            {
                reason = $"效率分偏低（{st.efficiency}），省里交办完成率不够看";
                return false;
            }
            return true;
        }

        /// <summary>年度考核：合规/效率双尺＋关系＋程序问题＋风险账本。
        /// 注意：违规要**按年度**计（yearIntegrity），不能用终身累计（violationCount）——
        /// 否则第一年四次小失误就终身背“不称职”，十年长局直接在第一年结束（探针实测过）。</summary>
        public static string EvaluateYear(GameState st, bool compete, Random rng)
        {
            // 阈值标定（十年长跑实测）：卷宗量 ≈5 件/周，即使尽责玩家也会漏掉个别雷，
            // 所以“年度程序问题”按**次数**卡时要放宽；主要判据仍是两把尺与风险账本。
            int bad = st.yearIntegrity;
            if (bad >= 6 || st.riskLedger >= 85 || (bad >= 3 && st.compliance < 50)) return "不称职";
            if (bad >= 3 || st.compliance < 40 || st.riskLedger >= 60) return "基本称职";
            int score = st.yearGradePoints
                      + (st.compliance - 70) / 2
                      + (st.efficiency - 60) / 3
                      + (compete ? 2 : 0)
                      + (rng != null ? rng.Next(0, 10) : 5);
            if (compete && score >= 62 && st.compliance >= 70 && st.efficiency >= 55) return "优秀";
            if (score < 20) return "基本称职";
            return "称职";
        }

        public static EndingData ComputeEnding(GameState st)
        {
            var e = new EndingData();
            var p = st.player;
            int age = 2036 - p.birthYear; // 51
            string family = st.hasChild ? "孩子已经能读懂你签批里那些欲言又止的句子"
                          : st.married ? "林晚把家里那盏灯一直留着"
                          : "你把市政府当成了家";

            if (st.underInvestigation)
            {
                e.title = "结局 · 接受审查调查";
                e.paras.Add($"2036年秋，{p.name}被宣布接受审查调查。办公室的门从里面锁上，再打开时，桌上只剩一盆没人浇水的文竹。");
                e.paras.Add("卷宗柜里那些“当时觉得没什么”的批示，一页页被翻出来对时。制度的债，从来不会因为你签得快就消失。");
                e.paras.Add($"{family}。只是这一次，灯下等的人等来的不是归期。");
            }
            else if (st.adverse == "免职")
            {
                e.title = "结局 · 免职待查";
                e.paras.Add($"年度考核等第公布那天，{p.name}的档案袋上多了一张纸：不称职。一个月后，免职通知到了市政府。");
                e.paras.Add("没有立案，也没有追究——组织给了体面，让你“待安排”。你收拾办公桌时才发现，抽屉里最厚的是两年没看的文件。");
                e.paras.Add($"{family}。往后的日子，你终于有空读懂那些被自己签“照准”的句子。");
            }
            else if (st.adverse == "降级")
            {
                e.title = "结局 · 降级调离";
                e.paras.Add($"三次基本称职之后，{p.name}被调任省里一个清闲署的副职——职级降了半格，份量也降了半格。");
                e.paras.Add("十年大同，账面上没有大事，流程上没有硬伤。可“没有硬伤”不是政绩，只是没什么可写的。");
                e.paras.Add($"{family}。你说这半格降得不冤，只是这话只有在你自己的桌子前说得出口。");
            }
            else if (st.resigned)
            {
                e.title = "结局 · 转身离开";
                e.paras.Add($"{p.name}递辞呈那天，云中在下第一场雪。七品正厅的工牌交还办公厅，周谨接过去时手指顿了一下。");
                e.paras.Add($"有人惋惜，有人松了口气。{family}。往后的路，换一种走法——至少，签字只对自己负责。");
            }
            else if (st.grade.StartsWith("六品"))
            {
                e.title = "结局 · 六品副省";
                e.paras.Add($"{age}岁的{p.name}离开大同时，车窗外的矸石山被夕阳切出一道金边。中央组织委员会的备案函在公文包里，轻得像一张纸，重得像一座城。");
                e.paras.Add("组织鉴定写着：“政治上成熟，驾驭复杂局面能力较强。”——“驾驭复杂局面”，是你十年签批里最贵的六个字。");
                e.paras.Add($"{family}。新的办公桌上，文件已经码好了第一摞。");
            }
            else if (st.compliance >= 75 && st.efficiency >= 60)
            {
                e.title = "结局 · 平稳主官";
                e.paras.Add($"两届任满，{p.name}没有去更好的地方，也没有出事。大同的财政窟窿补上了一半，AI 产业园的灯亮了一半。");
                e.paras.Add("有人说你保守，有人说你干净。在七品这个位子上，干净本身就是政绩。");
                e.paras.Add($"{family}。你终于可以准点下班，把签批的速度，放慢到一笔一画。");
            }
            else if (st.compliance < 45)
            {
                e.title = "结局 · 灰色着陆";
                e.paras.Add($"没有立案，也没有嘉奖。{p.name}被调任省里一个清闲署的巡视员——档案袋上盖着“工作需要”。");
                e.paras.Add("你知道那些漏查的雷没有炸完，只是被挪到了别人够不着的抽屉。");
                e.paras.Add($"{family}。夜里还是会醒，听楼道有没有脚步声。");
            }
            else
            {
                e.title = "结局 · 十年一日";
                e.paras.Add($"2036年，{p.name}仍在大同。城市不大不小，文件不多不少，你的批语越来越短。");
                e.paras.Add("十年市长，说不上功，说不上过。云中的风还是从北边来，卷着煤尘和一点新时代的电弧味。");
                e.paras.Add($"{family}。这样的一生，在这座城里，已经比多数人完整。");
            }

            e.paras.Add("—— 十年回响 ——");
            AppendMarkEchoes(st, e);
            return e;
        }

        public static void AppendMarkEchoes(GameState st, EndingData e)
        {
            void Line(string mark, string text)
            {
                if (st.HasMark(mark)) e.paras.Add(text);
            }
            Line("fast_track", "档案里那三次“优秀破格”的特批，在每一次巡视谈话里都被轻轻翻过——荣光与靶子，本就是同一枚印章的两面。");
            Line("origin_huabei", "你出身华北的产业工人家庭。签字时，你总想起厂区公告栏前的人群——有些数字背后是饭碗。");
            Line("origin_jiangnan", "你出身商贾之家。有人说你懂市场；只有你知道，你更懂“国家稳，生意才稳”。");
            Line("origin_guanzhong", "你出身关中吏员家庭。编制与分寸，是饭桌上最早学会的两个词。");
            Line("patron_province", "省里那条线一直托着你。托举的另一面，是随时可以松手。");
            Line("patron_none", "你没有靠山，只好把每一份材料都做得让人挑不出刺——干净，是无依无靠者唯一的派系。");
            Line("pressed_report", "那份被你压下又改过的对上报告，像一根细刺，十年后仍在某份巡视底稿里。");
            Line("algo_pass", "你签过“算法辅助、人工终审”。后来《AI治理法》专条落地，你的那一页批示被当作正面案例——或反面，取决于谁在念。");
            Line("helped_petition", "你接过一次群众的门。十年后还有人记得市长办公室的灯。");
            Line("gray_favor", "你特事特办过一次。就一次。档案却不会写“就一次”。");
            // —— Phase 5 · M1 剧情线路回响 ——
            Line("route_industry", "这十年你押在产业上。牌子挂上了，配套率爬上去了，煤城的骨头换了一半——另一半，留给了下一任。");
            Line("route_people", "这十年你押在民生上。暖气热了，老楼改了，接诉即办的数字先掉后涨。这些事不进大材料，但进老百姓的冬天。");
            Line("route_project", "这十年你押在项目上。工地从玉米地长成了厂房，你的名字出现在每一份开工报告的落款处。");
            Line("route_uplink", "这十年你押在向上争取上。省里的门你敲热了，专项债的额度翻了一倍——风口过去后，那些门也许会重新变冷。");
            Line("ind_zone_won", "“承接产业转移示范区”那块牌子在墙上挂了八年。责任状锁在抽屉里，钥匙你一直带着走。");
            Line("ppl_jiesu", "您建起的“接诉即办”，后任者保留了它——只是把满意率那一栏，改成了“力争”。");
            Line("prj_village_mayor", "云冈区那个村的老大爷，在你的告别会上托人送了一袋小米。他说的“我记账”，记的是你来过。");
            Line("pb_3_dirty_own", "纪委那个牛皮纸袋里的三份件，你在十年里又想起过很多次。承认过的事，反而不会在半夜里敲门。");
            Line("rival_grace", "许飞比你早两年进六品。你们后来在省里的会上又见过几面，彼此都客气——客气里，有那一年的体面。");
            if (st.route == "industry" || st.route == "people" || st.route == "project" || st.route == "uplink")
                e.paras.Add("组织鉴定里的那一句，写的是你做成了什么；而你知道，这十年真正改变你的，是你选了哪条路。");
            if (st.dossierLog.Count > 0)
            {
                int missed = 0;
                foreach (var d in st.dossierLog) missed += d.issuesMissed;
                if (missed > 3)
                    e.paras.Add($"十年里，你漏查过至少 {missed} 处材料问题。大多没有炸。大多，不等于全部。");
                else
                    e.paras.Add("十年里，你桌上的雷，多数被你自己拆掉了。这在七品里，已属难得。");
            }
            if (st.riskLedger >= 40)
                e.paras.Add($"纪委监委的卷宗里，关于你的那份存疑始终没有合上（风险账本 {st.riskLedger}）。它不写在鉴定上，但它在。");
        }
    }
}
