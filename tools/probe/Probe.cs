using System;
using System.Collections.Generic;
using Starstate.Core;

/// <summary>
/// STARSTATE Core 探针（脱离 Unity 跑模拟核心）。
/// 用途：改内容/平衡后秒级验证——不必等 Unity 导入与 EditMode（那要 1 分钟起步）。
/// 由 tools/core-probe.sh 编译并运行；依赖 check-compile.sh 先产出 Starstate.Core.ForTests.dll。
///
/// 用法：Probe.exe [all|content|long]
///   content —— 内容体检（卷宗量/形态/自检/第一年空周）
///   long    —— 十年长跑 × 三种策略（稳健 / 灰度 / 摆烂），观测结局分布与两把尺
/// </summary>
public static class Probe
{
    public static void Main(string[] args)
    {
        var log = new System.Text.StringBuilder();
        Console.SetOut(new System.IO.StringWriter(log));
        string mode = args != null && args.Length > 0 ? args[0] : "all";
        if (mode == "all" || mode == "content") Content();
        if (mode == "all" || mode == "long") LongRun();
        Console.Out.Flush();
        try { System.IO.File.WriteAllText("probe-out.txt", log.ToString(), System.Text.Encoding.UTF8); } catch { }
        Console.Error.Write(log.ToString());
    }

    // ---------------- 内容体检 ----------------

    static void Content()
    {
        DossierEngine.Clear();
        ContentRegistry.RegisterAll();

        var ids = DossierEngine.RegisteredIds();
        int handwritten = 0, generated = 0;
        var forms = new HashSet<string>();
        foreach (var id in ids)
        {
            var d = DossierEngine.Clone(id);
            if (d == null) continue;
            if (d.generated) { generated++; continue; }
            handwritten++;
            forms.Add(d.form);
        }

        Console.WriteLine("== 内容体检 ==");
        Console.WriteLine("卷宗注册数 = " + ids.Count + "（手写 " + handwritten + " / 模板实例 " + generated
                          + "；模板池 " + DossierGenerator.TemplateCount + " 个）");
        Console.WriteLine("来文形态 = " + string.Join(" / ", new List<string>(forms).ToArray()));
        Console.WriteLine("卷宗注册期自检错误 = " + DossierEngine.ValidationErrors.Count);
        foreach (var e in DossierEngine.ValidationErrors) Console.WriteLine("  ! " + e);

        int dup = 0;
        var seen = new HashSet<string>();
        for (int i = 1; i <= 30; i++)
        {
            var d = DossierGenerator.SpawnOne(new DateTime(2026, 9, 1), i);
            if (!seen.Add(d.id)) dup++;
        }
        Console.WriteLine("模板件 30 天重复 id 数 = " + dup + "（应为 0）");

        Flow.SeedRng(20260901);
        var st = State.NewGame("探针");
        int emptyWeeks = 0;
        for (int w = 0; w < 52; w++)
        {
            st.date = GameClock.Iso(GameClock.AddDays(GameClock.Parse(st.date), 7));
            DossierEngine.AssignWeek(st);
            if (st.pendingDossierIds.Count == 0) emptyWeeks++;
        }
        Console.WriteLine("第一年空周数 = " + emptyWeeks + "（应为 0）");
    }

    // ---------------- 十年长跑（三种策略） ----------------

    const int StrategySteady = 0;   // 稳健：每件核到底 + 选合规分最高的处置（≈一个尽责的市长）
    const int StrategyGray = 1;     // 灰度：不核对 + 灰区优先（风险账本应累积 → 审查/免职）
    const int StrategyLazy = 2;     // 照准：不核对 + 永远选第一项（最省力，也最积账）
    const int StrategyLast = 3;     // 摆烂：不核对 + 永远选最后一项

    static void LongRun()
    {
        Console.WriteLine();
        Console.WriteLine("== 十年长跑（策略 × seed）==");
        for (int s = 0; s < 4; s++)
        {
            for (int seed = 1; seed <= 3; seed++) Run(seed, s);
            Console.WriteLine();
        }
    }

    static void Run(int seed, int strategy)
    {
        Flow.SeedRng(seed);
        DossierEngine.Clear();
        ContentRegistry.RegisterAll();
        var st = State.NewGame("长跑");
        Flow.Begin(st);

        int guard = 0, dossierSteps = 0, grayPicks = 0;
        while (st.phase != Phase.Ending && !st.resigned && !st.underInvestigation
               && string.IsNullOrEmpty(st.adverse) && guard++ < 200000)
        {
            var sc = Flow.CurrentScene(st);
            if (sc.options == null || sc.options.Count == 0)
            {
                Console.WriteLine("!! 断档 " + st.date + " " + sc.title);
                return;
            }

            if (sc.kind == "dossier")
            {
                dossierSteps++;
                var dz = DossierEngine.Current(st);
                if (dz != null && (strategy == StrategySteady || strategy == StrategyLast))
                {
                    // 稳健/摆烂策略：翻页核到底（用核对次数换信息）
                    var act = st.activeDossier;
                    int sweep = 0;
                    while (act != null && act.checksLeft > 0 && sweep++ < 32)
                    {
                        var f = DossierEngine.CheckPage(st);
                        if (f == null)
                        {
                            if (act.page >= dz.pages.Count) break;
                            DossierEngine.TurnPage(st, +1);
                        }
                    }
                    sc = Flow.CurrentScene(st);
                }

                int pick = PickIndex(st, dz, strategy);
                if (dz != null && pick >= 0 && pick < dz.options.Count && dz.options[pick].gray) grayPicks++;
                string want = dz != null && pick >= 0 && pick < dz.options.Count ? dz.options[pick].label : null;
                int idx = want != null ? sc.options.IndexOf(want) : -1;
                Flow.Choose(st, idx >= 0 ? idx : Math.Max(0, sc.options.Count - 1));
                continue;
            }

            bool ff = st.phase == Phase.Day && !st.hasPending && string.IsNullOrEmpty(st.currentEvent)
                      && st.queue.Count == 0
                      && (st.runtimeEvent == null || st.runtimeEvent.id == "_generic_day"
                          || st.runtimeEvent.id == "_gen_task");
            if (ff) { Flow.FastForward(st); continue; }

            int choice = strategy == StrategyGray ? 1 : 0;
            if (choice >= sc.options.Count) choice = 0;
            Flow.Choose(st, choice);
        }

        string ending = st.endingData != null ? st.endingData.title
                      : !string.IsNullOrEmpty(st.adverse) ? "结局 · " + st.adverse
                      : st.phase.ToString();
        Console.WriteLine(string.Format(
            "  seed={0} {1,-4} → {2,-14} @{3} | 考核{4} 卷宗步{5} 灰区{6} 合规{7} 效率{8} 风险{9} 违规{10} 职级{11}",
            seed, Name(strategy), ending, st.date, st.evals.Count, dossierSteps, grayPicks,
            st.compliance, st.efficiency, st.riskLedger, st.violationCount, st.grade));
        if (st.integrity.Count > 0)
        {
            var tags = new List<string>();
            foreach (var r in st.integrity) tags.Add(r.tag + "@" + r.date);
            Console.WriteLine("      违规记录：" + string.Join("，", tags.ToArray()));
        }
    }

    static string Name(int s)
    {
        switch (s)
        {
            case StrategyGray: return "灰度";
            case StrategyLazy: return "照准";
            case StrategyLast: return "摆烂";
            default: return "稳健";
        }
    }

    /// <summary>处置选取：稳健＝引擎的“尽责市长”启发式（避程序违规/避灰区）；灰度＝优先灰区项；照准＝第一项；摆烂＝最后一项。</summary>
    static int PickIndex(GameState st, Dossier dz, int strategy)
    {
        if (dz == null || dz.options.Count == 0) return 0;
        if (strategy == StrategyGray)
        {
            for (int i = 0; i < dz.options.Count; i++) if (dz.options[i].gray) return i;
            return dz.options.Count > 1 ? dz.options.Count - 2 : 0;
        }
        if (strategy == StrategyLast) return dz.options.Count - 1;
        if (strategy == StrategyLazy) return 0;
        return DossierEngine.BestOptionIndex(st, dz);
    }
}
