using System;
using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 模板卷宗生成：变量（局名/数字/企业）填充，保证十年池不枯。
    /// 高光与教学件仍手写；本类只出 routine/cosign 日常件。
    /// </summary>
    public static class DossierGenerator
    {
        const string IdPrefix = "dz_g";
        static int toolSeq;   // 仅 SpawnOne(today) 测试/工具重载使用；不参与存档

        /// <summary>测试/工具重载：进程内自增序号。正式运行时序号由 GameState.dossierSeq 提供。</summary>
        public static Dossier SpawnOne(DateTime today) { return SpawnOne(today, ++toolSeq); }

        static readonly string[] Orgs =
        {
            "市财政局", "市住建局", "市教育局", "市卫生健康局", "市交通运输局",
            "市市场监管局", "市文化旅游局", "市退役军人局", "市医保局", "市审计局",
            "市应急管理局", "市自然资源局", "市发展和改革局", "市人力资源和社会保障局",
            "市民政局", "市司法局", "市生态环境局", "市水务局", "市农业农村局", "市商务局",
        };

        static readonly string[] Forms =
        {
            "请示", "财政件", "会议材料", "人事单", "信访件", "省交办", "巡视整改", "协议",
        };

        /// <summary>全部模板（Y1 日常件 + Y2 上量件）；索引由 seq 取模决定，因此顺序改动会让读档重建错位。</summary>
        static readonly DossierTemplate[] Templates = DossierTemplates.All();

        /// <summary>模板池规模（内容审计/测试用：M2 要求 ≥34）。</summary>
        public static int TemplateCount { get { return Templates.Length; } }

        /// <summary>从模板件 id 反解生成序号（dz_g007_0901 → 7）；非模板件返回 false。</summary>
        public static bool TryParseSeq(string id, out int seq)
        {
            seq = 0;
            if (string.IsNullOrEmpty(id) || !id.StartsWith(IdPrefix)) return false;
            int us = id.IndexOf('_', IdPrefix.Length);
            if (us < 0) return false;
            return int.TryParse(id.Substring(IdPrefix.Length, us - IdPrefix.Length), out seq) && seq > 0;
        }

        /// <summary>
        /// 按 id 原样重建模板卷宗（读档后运行时实例丢失 / 跨会话注册表错位时用）。
        /// 生成过程对 (date, seq) 纯确定，且日期从 id 后缀反解——重建结果与首次生成完全一致。
        /// </summary>
        public static Dossier Respawn(string id, DateTime today)
        {
            int seq;
            if (!TryParseSeq(id, out seq)) return null;
            var when = today;
            int us = id.IndexOf('_', IdPrefix.Length);
            if (us >= 0 && id.Length >= us + 5)
            {
                int mm, dd;
                if (int.TryParse(id.Substring(us + 1, 2), out mm) && int.TryParse(id.Substring(us + 3, 2), out dd))
                {
                    try { when = new DateTime(today.Year, mm, dd); }
                    catch { when = today; }
                }
            }
            return SpawnOne(when, seq);
        }

        /// <summary>生成一件模板卷宗（纯函数：同一 (today, seq) 必得同一件，含 id）。</summary>
        public static Dossier SpawnOne(DateTime today, int seq)
        {
            if (seq <= 0) seq = 1;
            var t = Templates[seq % Templates.Length];
            var rng = new Random(20260901 + seq * 17);
            string org = Orgs[rng.Next(Orgs.Length)];
            int n1 = rng.Next(8, 60);
            int n2 = rng.Next(3, n1 + 5);
            int n3 = rng.Next(5, 40);
            int sum = n1 + n2 + n3;

            // 埋雷：按模板声明的方式制造**能被该模板 detectHint 指出的真实矛盾**
            // （旧实现一律 sum+7，遇到“整改数是否大于问题数”这类提示时数据上不可能出现，玩家照提示永远查不到）
            bool plant = (seq % 3) == 0;
            bool mathPlant = plant && t.issue != null && t.plant != "none";
            int sumShown = sum;
            int n2Shown = n2;
            if (mathPlant && t.plant == "sum") sumShown = sum + 7;        // 合计 ≠ 分项之和
            if (mathPlant && t.plant == "overfix") n2Shown = sum + 3;     // 已整改 > 发现问题

            string id = IdPrefix + seq.ToString("000") + "_" + today.ToString("MMdd");
            var d = new Dossier
            {
                id = id,
                kind = t.kind,
                form = t.form,
                title = t.titlePattern.Replace("{org}", org),
                docNo = t.docPrefix + "〔" + today.Year + "〕" + (100 + seq % 800) + "号",
                org = org,
                deadline = "",
                checkBudget = t.checkBudget,
                generated = true,
            };

            var body = new List<string>();
            foreach (var p in t.bodyParas)
                body.Add(p.Replace("{org}", org).Replace("{n1}", n1.ToString())
                    .Replace("{n2}", n2Shown.ToString()).Replace("{n3}", n3.ToString())
                    .Replace("{sum}", sumShown.ToString()));
            d.pages.Add(new DossierPage { title = "正文", paras = body });

            if (!string.IsNullOrEmpty(t.tablePattern))
            {
                d.pages.Add(new DossierPage
                {
                    title = "附件：测算表",
                    table = t.tablePattern
                        .Replace("{n1}", n1.ToString()).Replace("{n2}", n2Shown.ToString())
                        .Replace("{n3}", n3.ToString()).Replace("{sum}", sumShown.ToString()),
                    paras = new List<string> { "（附件由" + org + "相关科室编制。）" },
                });
            }

            if (t.issue != null && (plant || (seq % 2) == 0))
            {
                d.issues.Add(new DossierIssue
                {
                    id = id + "_i1",
                    pageRef = t.issue.pageRef,
                    detectHint = t.issue.detectHint,
                    ruleKey = t.ruleKey,
                    // plant=true → 実埋了可指出的矛盾；否则只出程序类软雷（扣分更轻）
                    severity = plant ? t.issueSeverity : Math.Max(1, t.issueSeverity - 1),
                });
            }

            foreach (var o in t.options)
            {
                d.options.Add(new DossierOption
                {
                    label = o.label,
                    result = o.result,
                    complianceDelta = o.complianceDelta,
                    efficiencyDelta = o.efficiencyDelta,
                    gray = o.gray,
                    effects = o.effects != null ? Flow.CloneEffects(o.effects) : new Effects(),
                });
            }
            return d;
        }

        /// <summary>向池中注入 n 件模板卷宗（启动用）。序号固定从 1 开始：
        /// id 只由序号决定，不受本进程内其它 SpawnOne 调用影响（否则跑过测试再进游戏会整体错位）。</summary>
        public static void SeedPool(int n, DateTime today)
        {
            for (int i = 1; i <= n; i++)
            {
                var d = SpawnOne(today, i);
                DossierEngine.RegisterPool(d);
            }
        }
    }
}
