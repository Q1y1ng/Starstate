using System.Collections.Generic;

namespace Starstate.Core
{
    public static class State
    {
        public static GameState NewGame(string playerName = "沈砚舟", string origin = "origin_huabei", string patron = "patron_merit")
        {
            var st = new GameState
            {
                saveVersion = 5,
                phase = Phase.Prologue,
                date = "2026-08-24",
                grade = "七品·正厅",
                gradeSince = "2026-09-01",
                termYears = 0,
                examPassed = true,
                academyDone = true,
                baseExpMonths = 240,
                outstandingYears = 3,
                partner = "林晚",
                married = true,
                hasChild = true,
                housing = "市政府周转房",
                compliance = 100,
                efficiency = 70,
                weekDossierBudget = 5,
                ambition = "做事",
            };
            st.player = new PlayerState
            {
                name = string.IsNullOrEmpty(playerName) ? "沈砚舟" : playerName,
                birthYear = 1985,
                school = "国立中央翰林院大学",
                major = "经济学（社会科学方向）",
                energy = 70,
                stress = 35,
                morale = 60,
                reputation = 40,
                polCapital = 25,
                unit = "大同市人民政府",
                post = "市长",
                rank = "七品·正厅",
                savings = 280000,
                monthlyIn = 18000,
                monthlyOut = 9500,
            };
            // 七品市长画像：执行与政治敏感突出，专业为经济底子
            st.player.attrs.admin = 62;
            st.player.attrs.professional = 55;
            st.player.attrs.exec = 58;
            st.player.attrs.comm = 50;
            st.player.attrs.political = 55;

            // 最快轨三次特批——荣光也是巡视靶子
            st.Mark("fast_track");
            st.SetFlag("fast_track", true);
            st.evals.Add(new YearEval { year = 2023, grade = "优秀" });
            st.evals.Add(new YearEval { year = 2024, grade = "优秀" });
            st.evals.Add(new YearEval { year = 2025, grade = "优秀" });

            ApplyOrigin(st, origin);
            ApplyPatron(st, patron);

            Npcs.Ensure(st);
            foreach (var kv in MayorNpcBonds)
            {
                var r = st.relations.Find(x => x.id == kv.Key);
                if (r != null)
                {
                    r.familiar = kv.Value.familiar;
                    r.trust = kv.Value.trust;
                    r.evalv = kv.Value.evalv;
                }
            }

            // 案头口径手册：就任时办公厅给的三份常备件；数字勾稽先摊开（教学）
            Rulebook.Grant(st, "rule_shuzi");
            Rulebook.Grant(st, "rule_xingwen");
            Rulebook.Grant(st, "rule_tudi");
            Rulebook.Open(st, "rule_shuzi");

            st.week.index = 1;
            st.month.key = "2026-08";
            Flow.Begin(st);
            return st;
        }

        static void ApplyOrigin(GameState st, string origin)
        {
            switch (origin)
            {
                case "origin_guanzhong":
                    st.Mark("origin_guanzhong");
                    st.SetFlag("origin_guanzhong", true);
                    st.player.attrs.admin += 3;
                    st.player.attrs.political += 2;
                    break;
                case "origin_jiangnan":
                    st.Mark("origin_jiangnan");
                    st.SetFlag("origin_jiangnan", true);
                    st.player.attrs.comm += 3;
                    st.player.attrs.professional += 2;
                    break;
                default:
                    st.Mark("origin_huabei");
                    st.SetFlag("origin_huabei", true);
                    st.player.attrs.exec += 3;
                    st.player.attrs.professional += 1;
                    break;
            }
        }

        static void ApplyPatron(GameState st, string patron)
        {
            switch (patron)
            {
                case "patron_province":
                    st.Mark("patron_province");
                    st.SetFlag("patron_province", true);
                    st.player.polCapital += 8;
                    break;
                case "patron_none":
                    st.Mark("patron_none");
                    st.SetFlag("patron_none", true);
                    st.player.polCapital -= 5;
                    st.player.reputation += 5;
                    break;
                default:
                    st.Mark("patron_merit");
                    st.SetFlag("patron_merit", true);
                    break;
            }
        }

        struct Bond { public int familiar, trust, evalv; }
        static readonly Dictionary<string, Bond> MayorNpcBonds = new Dictionary<string, Bond>
        {
            ["cen"] = new Bond { familiar = 70, trust = 20, evalv = 10 },   // 主席
            ["han"] = new Bond { familiar = 55, trust = 15, evalv = 5 },    // 组织部部长
            ["shenyan"] = new Bond { familiar = 40, trust = 0, evalv = 0 }, // 纪委
            ["shao"] = new Bond { familiar = 65, trust = 25, evalv = 15 },  // 常务副市长
            ["zhoujin"] = new Bond { familiar = 80, trust = 40, evalv = 20 }, // 办公厅主任
            ["xu"] = new Bond { familiar = 45, trust = 5, evalv = 5 },      // 同批许飞
            ["laokang"] = new Bond { familiar = 30, trust = 20, evalv = 10 },
            ["linwan"] = new Bond { familiar = 95, trust = 70, evalv = 0 },
        };
    }
}
