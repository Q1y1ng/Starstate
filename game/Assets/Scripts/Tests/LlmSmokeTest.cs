using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>大模型增强层单元测试：容错解析、效果白名单、无 AI 回退。</summary>
    public class LlmSmokeTest
    {
        private static GameState NewState()
        {
            var st = State.NewGame("测试员");
            Flow.Begin(st);
            return st;
        }

        [Test]
        public void LlmJson_Extracts_Talk_From_Fenced_Output()
        {
            const string raw = "好的，以下是内容：\n```json\n{\"greeting\":\"周衡之抬起头，“有事？”\",\"lines\":[\"他指了指椅子。\"],\"options\":[{\"label\":\"递上材料\",\"mood\":\"warm\",\"reply\":\"他接过去翻了翻。\"},{\"label\":\"转身就走\",\"mood\":\"bad\",\"reply\":\"\"}]}\n```";
            var dto = LlmJson.ParseTalk(raw);
            Assert.IsNotNull(dto);
            Assert.IsTrue(dto.greeting.Contains("周衡之"));
            Assert.AreEqual(2, dto.options.Length);
            Assert.AreEqual("递上材料", dto.options[0].label);
            Assert.AreEqual("warm", dto.options[0].mood);
            Assert.AreEqual("neutral", dto.options[1].mood); // 非法 mood 归一化
        }

        [Test]
        public void LlmJson_Tolerates_Garbage()
        {
            Assert.IsNull(LlmJson.ParseTalk("模型不想说话。"));
            Assert.IsNull(LlmJson.ParseTalk(""));
            Assert.IsNull(LlmJson.ParseMicro("{\"title\": 未闭合"));
        }

        [Test]
        public void Talk_Effects_Are_Whitelisted()
        {
            var st = NewState();
            int energy0 = st.player.energy;
            var opt = new TalkOptionDto { label = "请教材料", mood = "warm", reply = "他讲得很细。" };
            string summary = LlmGameplay.ApplyTalk(st, "zhou", opt);
            var r = Npcs.Get(st, "zhou");
            Assert.AreEqual(2, r.familiar);           // warm 上限 +2
            Assert.AreEqual(2, r.trust);              // warm 上限 +2
            Assert.AreEqual(1, r.memories.Count);     // 写入记忆
            Assert.AreEqual(energy0 - 2, st.player.energy);
            Assert.IsTrue(summary.Contains("熟悉+2"));

            // 非法 mood 走 neutral；模型给不了任何数字之外的效果
            LlmGameplay.ApplyTalk(st, "zhou", new TalkOptionDto { mood = "hacked" });
            Assert.AreEqual(3, r.familiar);           // neutral +1
            Assert.AreEqual(2, r.trust);              // 不再增加
        }

        [Test]
        public void Micro_Event_Uses_Whitelist_Effects()
        {
            const string raw = "{\"title\":\"打印机又卡了\",\"paras\":[\"第一段。\",\"第二段。\"],\"options\":[{\"label\":\"帮忙修好\",\"mood\":\"good\",\"result\":\"大家谢谢你。\"},{\"label\":\"绕着走\",\"mood\":\"tough\",\"result\":\"\"}]}";
            var st = NewState();
            var ev = LlmGameplay.BuildMicroEvent(st, LlmJson.ParseMicro(raw));
            Assert.IsNotNull(ev);
            Assert.AreEqual(2, ev.paras.Count);
            Assert.AreEqual(2, ev.options.Count);
            Assert.IsTrue(ev.title.StartsWith("小插曲 · "));
            Assert.AreEqual(2, ev.options[0].effects.morale);   // good：士气+2 精力-2
            Assert.AreEqual(-2, ev.options[0].effects.energy);
            Assert.AreEqual(2, ev.options[1].effects.stress);   // tough：压力+2 政敏+1
            Assert.AreEqual(1, ev.options[1].effects.political);
            Assert.AreEqual("大家谢谢你。", ev.options[0].result);
            Assert.IsFalse(string.IsNullOrEmpty(ev.options[1].result)); // 空结果回退到默认尾巴
        }

        [Test]
        public void Fallback_Talk_Works_Without_Llm()
        {
            var st = NewState();
            foreach (var id in Npcs.Order)
            {
                var dto = ContentNpcTalk.FallbackTalk(st, id);
                Assert.IsNotNull(dto);
                Assert.GreaterOrEqual(dto.options.Length, 2, id);
                // 选项随熟悉度/信任解锁：3 基础 + “说给他听”(信任≥25) + “把关材料”(熟悉≥50)
                Assert.LessOrEqual(dto.options.Length, 5, id);
                Assert.IsFalse(string.IsNullOrEmpty(dto.greeting), id);
            }
        }
    }
}
