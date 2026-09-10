using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    public class RulebookTest
    {
        [Test]
        public void Grant_And_Open_Writes_State()
        {
            DossierEngine.Clear();
            Rulebook.ClearRegistry();
            ContentRulebook.Register();
            var st = State.NewGame();

            Assert.IsTrue(st.knownRules.Contains("rule_shuzi"), "NewGame 应授予数字勾稽口径");
            Assert.AreEqual("rule_shuzi", st.openRule, "应默认摊开数字勾稽");
            Assert.IsTrue(Rulebook.Open(st, "rule_xingwen"));
            Assert.AreEqual("rule_xingwen", st.openRule);
            Rulebook.CloseDesk(st);
            Assert.AreEqual("", st.openRule);
        }

        [Test]
        public void DeskHint_Hits_Matching_Issue()
        {
            DossierEngine.Clear();
            Rulebook.ClearRegistry();
            ContentRulebook.Register();
            ContentDossierM0.Register();
            var st = State.NewGame();
            DossierEngine.AssignWeek(st, new[] { "dz_t1" });
            Assert.IsTrue(DossierEngine.OpenNext(st));
            var d = DossierEngine.Current(st);
            Assert.IsNotNull(d);
            Assert.AreEqual("rule_shuzi", d.issues[0].ruleKey);

            string hint = Rulebook.DeskHint(st, d);
            StringAssert.Contains("3200", hint, "案头命中应给出 detectHint");

            Rulebook.Open(st, "rule_xingwen");
            string miss = Rulebook.DeskHint(st, d);
            StringAssert.Contains("暂未对上", miss, "口径不对时应提示未对上");
        }

        [Test]
        public void Check_Finds_Issue_On_Correct_Page()
        {
            DossierEngine.Clear();
            Rulebook.ClearRegistry();
            ContentRulebook.Register();
            ContentDossierM0.Register();
            var st = State.NewGame();
            DossierEngine.AssignWeek(st, new[] { "dz_t1" });
            DossierEngine.OpenNext(st);
            DossierEngine.TurnPage(st, +1); // 第2页
            var found = DossierEngine.CheckPage(st);
            Assert.IsNotNull(found);
            Assert.AreEqual("rule_shuzi", found.ruleKey);
        }

        [Test]
        public void Desk_Slots_Evict_Oldest()
        {
            DossierEngine.Clear();
            Rulebook.ClearRegistry();
            ContentRulebook.Register();
            var st = State.NewGame();
            // NewGame 已授 3 份：shuzi / xingwen / tudi
            Assert.AreEqual(3, st.deskRules.Count);
            Rulebook.Grant(st, "rule_suanfa");
            Rulebook.Grant(st, "rule_duishang"); // 第 5 份 → 顶掉最旧
            Assert.AreEqual(Rulebook.DeskSlots, st.deskRules.Count, "案头不超过 4");
            Assert.IsFalse(st.deskRules.Contains("rule_shuzi"), "最旧的数字勾稽应被顶下案头");
            Assert.IsTrue(st.knownRules.Contains("rule_shuzi"), "档案仍保留");
            // Grant 在 openRule 被顶空后会自动摊开新件
            Assert.AreEqual("rule_duishang", st.openRule);
            Rulebook.Open(st, "rule_shuzi"); // 从档案捞回案头
            Assert.IsTrue(Rulebook.OnDesk(st, "rule_shuzi"));
            Assert.AreEqual("rule_shuzi", st.openRule);
        }

        [Test]
        public void Search_Filters_By_Keyword()
        {
            DossierEngine.Clear();
            Rulebook.ClearRegistry();
            ContentRulebook.Register();
            var st = State.NewGame();
            Rulebook.Grant(st, "rule_suanfa"); // 上案头/入档案
            var hits = Rulebook.Search(st, "算法");
            Assert.Greater(hits.Count, 0, "应能检索到算法口径");
            Assert.IsTrue(hits.Contains("rule_suanfa"));
            var all = Rulebook.Search(st, "");
            Assert.AreEqual(st.knownRules.Count, all.Count);
            var num = Rulebook.Search(st, "数字");
            Assert.Greater(num.Count, 0);
        }
    }
}
