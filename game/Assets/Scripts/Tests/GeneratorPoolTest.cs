using System;
using System.Collections.Generic;
using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    public class GeneratorPoolTest
    {
        [Test]
        public void SeedPool_Ids_Are_Unique()
        {
            DossierEngine.Clear();
            Rulebook.ClearRegistry();
            ContentRulebook.Register();
            var today = new DateTime(2026, 9, 1);
            var ids = new HashSet<string>();
            for (int i = 0; i < 40; i++)
            {
                var d = DossierGenerator.SpawnOne(today);
                Assert.IsFalse(string.IsNullOrEmpty(d.id));
                Assert.IsTrue(ids.Add(d.id), "重复 id: " + d.id);
                Assert.Greater(d.options.Count, 0, "必须有处置选项");
                Assert.IsFalse(string.IsNullOrEmpty(d.org));
            }
        }

        [Test]
        public void Standing_Committee_Have_Fallback_Lines()
        {
            string[] seats = { "cen", "han", "shenyan", "shao", "zhoujin" };
            foreach (var id in seats)
            {
                Assert.IsTrue(ContentNpcTalk.Lines.ContainsKey(id), id + " 缺内置台词");
                Assert.Greater(ContentNpcTalk.Lines[id].Length, 1);
            }
            var st = State.NewGame();
            var dto = ContentNpcTalk.FallbackTalk(st, "cen");
            Assert.IsNotNull(dto);
            Assert.IsFalse(string.IsNullOrEmpty(dto.greeting));
            Assert.GreaterOrEqual(dto.options.Length, 2);
            // 工作向选项应能写关系（warm）
            bool hasWarm = false;
            foreach (var o in dto.options) if (o.mood == "warm") hasWarm = true;
            Assert.IsTrue(hasWarm, "常委交谈应含 warm 选项");
        }
    }
}
