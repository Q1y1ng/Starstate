using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>
    /// 修复回归集（对应 docs/AUDIT-2026-09-12.md）：
    /// A1 运行时内容注册、A3 卷宗池补货、B1 事件/卷宗输入路由、B2 卷宗按 id 重建、
    /// B4 内容自检、B5 季节档期、C1 效果记录深拷贝。
    /// </summary>
    public class RegressionTest
    {
        // ---------------- A1：内容注册必须由引擎入口自动完成 ----------------

        [Test]
        public void NewGame_Registers_All_Content_Without_Explicit_Call()
        {
            // 模拟“换了一个进程/域”：注册表全空，然后只走正常开局路径
            DossierEngine.Clear();
            Rulebook.ClearRegistry();

            var st = State.NewGame("注册测试");
            Assert.IsNotNull(st);

            Assert.IsTrue(Flow.IsRegistered("mp0"), "序章未注册：Play 时会直接跳过序章");
            Assert.IsTrue(Flow.IsRegistered("sys_annual_eval"), "年度考核未注册：十年没有考核");
            Assert.IsTrue(Flow.IsRegistered("sys_ending"), "结局事件未注册：游戏永不结局");
            Assert.IsTrue(Flow.IsRegistered("sys_personnel"), "人事窗口未注册：七品→六品晋升不可达");
            Assert.IsTrue(DossierEngine.IsRegistered("dz_t1"), "教学卷宗未注册：案头永远没有卷宗");
            Assert.IsTrue(Rulebook.IsRegistered("rule_shuzi"), "口径未注册：口径页与案头提示全空");
            Assert.IsNotEmpty(st.knownRules, "开局应已授 3 份口径");
            Assert.AreEqual(3, st.deskRules.Count, "开局案头应有 3 份口径");
        }

        // ---------------- B4：内容自检（页号越界 / 口径未注册 / 无选项） ----------------

        [Test]
        public void All_Registered_Dossiers_Are_Wellformed()
        {
            DossierEngine.Clear();
            ContentRegistry.RegisterAll();
            Assert.IsEmpty(DossierEngine.ValidationErrors,
                "卷宗自检未通过：" + string.Join("；", DossierEngine.ValidationErrors.ToArray()));
        }

        // ---------------- A3：卷宗池必须能撑过十年，而不是 13 周就枯竭 ----------------

        [Test]
        public void Dossier_Pool_Replenishes_Across_Years()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("补货测试");

            for (int week = 0; week < 130; week++)   // 130 周（约 2.5 年），远超旧实现的总量 62 件
            {
                st.date = GameClock.Iso(GameClock.AddDays(GameClock.Parse(st.date), 7));
                DossierEngine.AssignWeek(st);
                Assert.Greater(st.pendingDossierIds.Count, 0, "第 " + week + " 周案头为空（池已枯竭）");
                // 视作当周全部办结
                foreach (var id in st.pendingDossierIds) if (!st.dossierFired.Contains(id)) st.dossierFired.Add(id);
                st.pendingDossierIds.Clear();
            }
        }

        // ---------------- B5：季节档期（2027 年的件不应在 2026 年 9 月就发下来） ----------------

        [Test]
        public void Seasonal_Dossiers_Respect_ReleaseFrom()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("档期测试");
            st.weekDossierBudget = 12;          // 放宽件数，排除“没排上”的干扰
            st.date = "2026-09-01";

            DossierEngine.AssignWeek(st);
            Assert.IsFalse(st.pendingDossierIds.Contains("dz_y1_flood"),
                "2027-07 的防汛件不应在 2026-09 投放");
            Assert.IsFalse(st.pendingDossierIds.Contains("dz_y1_xun"),
                "2027-03 的巡视整改件不应在 2026-09 投放");
            Assert.IsFalse(st.pendingDossierIds.Contains("dz_y1_audit"),
                "2027-11 的审计件不应在 2026-09 投放");

            st.date = "2027-07-01";
            st.pendingDossierIds.Clear();
            st.dossierFired.Clear();
            DossierEngine.AssignWeek(st);
            Assert.IsTrue(st.pendingDossierIds.Contains("dz_y1_flood"),
                "进入 releaseFrom(2027-06-15) 之后防汛件应可投放");
        }

        // ---------------- B1：事件在屏时，点击不能被卷宗吃掉 ----------------

        static GameState ToDayWithDossier()
        {
            var st = State.NewGame("路由测试");
            int guard = 0;
            while (guard++ < 300)
            {
                if (st.phase == Phase.Day && st.activeDossier != null && !st.activeDossier.resolved) return st;
                var sc = Flow.CurrentScene(st);
                Assert.IsNotEmpty(sc.options, "场景无选项：" + sc.title + " / " + st.date);
                Flow.Choose(st, 0);
            }
            Assert.Fail("300 步内未进入「工作日 + 已打开卷宗」状态");
            return st;
        }

        [Test]
        public void Event_On_Screen_Wins_Over_Dossier()
        {
            var st = ToDayWithDossier();
            int checks0 = st.activeDossier.checksLeft;
            int page0 = st.activeDossier.page;

            // 塞入一件必然可构建的动态事件（年度考核）
            st.queue.Clear();
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.queue.Add("sys_annual_eval");

            var scene = Flow.CurrentScene(st);
            Assert.AreEqual("event", scene.kind, "屏上应是事件");
            Assert.IsFalse(Flow.DossierOnScreen(st), "事件在屏时不应判为卷宗场景");
            Assert.IsTrue(Flow.PendingChoiceEvent(st));

            Flow.Choose(st, 0);

            Assert.AreEqual(checks0, st.activeDossier.checksLeft, "点事件选项不应消耗卷宗的核对次数");
            Assert.AreEqual(page0, st.activeDossier.page, "点事件选项不应翻卷宗的页");
            Assert.IsFalse(st.activeDossier.resolved, "点事件选项不应签批掉卷宗");
        }

        [Test]
        public void Dossier_On_Screen_Wins_When_No_Event_Pending()
        {
            var st = ToDayWithDossier();
            st.queue.Clear();
            st.currentEvent = null;
            st.runtimeEvent = null;

            Assert.AreEqual("dossier", Flow.CurrentScene(st).kind);
            Assert.IsTrue(Flow.DossierOnScreen(st));
            Assert.IsFalse(Flow.PendingChoiceEvent(st));
        }

        // ---------------- B2：模板卷宗必须能按 id 原样重建（否则读档后“继续”是死按钮） ----------------

        [Test]
        public void Generated_Dossier_Survives_Registry_Loss()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("重建测试");
            st.pendingDossierIds.Clear();
            st.pendingDossierIds.Add("dz_g007_0824");   // 模板件 id：序号可反解
            Assert.IsTrue(DossierEngine.OpenNext(st), "应能打开未登记的模板件");
            var d1 = DossierEngine.Current(st);
            Assert.IsNotNull(d1);
            Assert.AreEqual("dz_g007_0824", d1.id);

            // 模拟读档/换会话：运行时实例与注册表都没了
            DossierEngine.ClearRuntime();
            DossierEngine.Clear();

            var d2 = DossierEngine.Current(st);
            Assert.IsNotNull(d2, "注册表清空后应能按 id 重建，而不是返回 null（旧版会让“继续”失效）");
            Assert.AreEqual(d1.id, d2.id);
            Assert.AreEqual(d1.title, d2.title);
            Assert.AreEqual(d1.pages.Count, d2.pages.Count);
            Assert.AreEqual(d1.options.Count, d2.options.Count);
        }

        // ---------------- C1：效果表里的档案记录必须深拷贝 ----------------

        [Test]
        public void CloneEffects_DeepCopies_Archive_Records()
        {
            var src = new Effects
            {
                integrity = new IntegrityRecord { tag = "督查配合", note = "自查" },
                task = new TaskRecord { title = "某件" },
                document = new DocRecord { title = "某文" },
                commend = new CommendRecord { text = "嘉奖" },
            };
            var a = Flow.CloneEffects(src);
            var b = Flow.CloneEffects(src);

            Assert.AreNotSame(src.integrity, a.integrity);
            Assert.AreNotSame(a.integrity, b.integrity, "integrity 深拷贝失败：多条合规档案会指向同一对象");
            Assert.AreNotSame(a.task, b.task);
            Assert.AreNotSame(a.document, b.document);
            Assert.AreNotSame(a.commend, b.commend);
            Assert.AreEqual("督查配合", a.integrity.tag);
            Assert.AreEqual("某件", a.task.title);
        }

        // ---------------- A2：七品→六品晋升路径真的走得通 -------------

        [Test]
        public void Personnel_Window_Offers_Promotion_When_Eligible()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("晋升测试");
            st.grade = "七品·正厅";
            st.gradeSince = "2026-09-01";
            st.compliance = 80;
            st.efficiency = 70;
            st.date = "2031-09-20";          // 满一届（5 年）后的首个开窗日
            st.phase = Phase.Day;
            st.hasPending = false;
            st.queue.Clear();
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.activeDossier = null;
            st.queue.Add("sys_personnel");

            var scene = Flow.CurrentScene(st);
            Assert.AreEqual("event", scene.kind);
            Assert.AreEqual("人事窗口 · 2031年9月", scene.title);
            bool hasPromotion = false;
            foreach (var o in scene.options) if (o.Contains("六品")) hasPromotion = true;
            Assert.IsTrue(hasPromotion, "满一届且两把尺达标时，人事窗口必须给出六品酝酿选项（旧版永远是空的）");

            Flow.Choose(st, 0);   // 接受酝酿
            Assert.IsTrue(st.grade.StartsWith("六品"), "应晋升为六品，实际：" + st.grade);
        }

        [Test]
        public void Personnel_Window_Blocks_Promotion_When_Below_Threshold()
        {
            ContentRegistry.RegisterAll();
            var st = State.NewGame("晋升门槛测试");
            st.grade = "七品·正厅";
            st.gradeSince = "2026-09-01";
            st.date = "2031-09-20";
            st.compliance = 30;               // 合规分过低：御史会压住名单
            st.phase = Phase.Day;
            st.hasPending = false;
            st.queue.Clear();
            st.currentEvent = null;
            st.runtimeEvent = null;
            st.activeDossier = null;
            st.queue.Add("sys_personnel");

            var scene = Flow.CurrentScene(st);
            foreach (var o in scene.options) Assert.IsFalse(o.Contains("六品"), "不达标时不应给出晋升选项");
            bool toldWhy = false;
            foreach (var p in scene.paras) if (p.Contains("条件还没攒齐")) toldWhy = true;
            Assert.IsTrue(toldWhy, "应告知为何还不能进六品酝酿");
        }

        [Test]
        public void Six_Pin_Ending_Is_Reachable()
        {
            var st = State.NewGame("结局测试");
            st.grade = "六品·副省";
            var e = Career.ComputeEnding(st);
            Assert.AreEqual("结局 · 六品副省", e.title, "六品结局必须可达（旧版 grade 永远变不成六品）");
        }

        // ---------------- B4：生成的模板件，埋雷必须真的能看到 ----------------

        [Test]
        public void Generated_Planted_Issue_Is_Actually_Visible()
        {
            // 序号 3/6/9… 会埋雷；对“整改数>问题数”类模板，提示必须与数据一致
            for (int seq = 3; seq <= 24; seq += 3)
            {
                var d = DossierGenerator.SpawnOne(new System.DateTime(2026, 9, 1), seq);
                Assert.IsNotEmpty(d.pages, "模板件应有页面：seq=" + seq);
                foreach (var iss in d.issues)
                {
                    int page;
                    int.TryParse(iss.pageRef, out page);
                    Assert.GreaterOrEqual(page, 1);
                    Assert.LessOrEqual(page, d.pages.Count, "埋雷指向了不存在的页：seq=" + seq);
                }
                // 埋了雷的件必须能通过“翻到该页 + 核对”查出来
                if (d.issues.Count > 0)
                {
                    var st = State.NewGame("埋雷测试");
                    st.pendingDossierIds.Clear();
                    st.pendingDossierIds.Add(d.id);
                    DossierEngine.Register(d);
                    Assert.IsTrue(DossierEngine.OpenNext(st));
                    int page;
                    int.TryParse(d.issues[0].pageRef, out page);
                    if (page > 1) DossierEngine.TurnPage(st, page - 1);
                    var found = DossierEngine.CheckPage(st);
                    Assert.IsNotNull(found, "该页的埋雷应可被核对查出：seq=" + seq);
                }
            }
        }
    }
}
