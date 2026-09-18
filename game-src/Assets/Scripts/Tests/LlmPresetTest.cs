using System.Collections.Generic;
using NUnit.Framework;
using Starstate.Core;

namespace Starstate.Tests
{
    /// <summary>
    /// 本地模型接入验收（2026-09-16 本机新增 4B 级模型后的横向评测结论落地）：
    /// ① 预设表完整可解析；② 默认配置指向实测最优的 4B 预设；
    /// ③ 结构化输出 schema 自检（改 schema 忘改字段名会在这里挂掉）。
    /// </summary>
    public class LlmPresetTest
    {
        [Test]
        public void Presets_Are_WellFormed()
        {
            Assert.GreaterOrEqual(LlmPresets.All.Length, 3, "至少要收录 4B 两档与 9B 一档");
            var ids = new HashSet<string>();
            foreach (var p in LlmPresets.All)
            {
                Assert.IsNotEmpty(p.id, "预设缺 id");
                Assert.IsNotEmpty(p.label, "预设缺显示名：" + p.id);
                Assert.IsNotEmpty(p.modelPath, "预设缺模型路径：" + p.id);
                Assert.IsTrue(ids.Add(p.id), "预设 id 重复：" + p.id);
                Assert.Greater(p.ctx, 2048, p.id + " 上下文过小");
                Assert.LessOrEqual(p.ctx, 262144, p.id + " 上下文超出模型上限");
                Assert.Greater(p.ngl, 0, p.id + " 至少要卸载部分层到 GPU");
                Assert.IsNotEmpty(p.note, p.id + " 缺取舍说明（设置面板要显示给作者看）");
                Assert.IsTrue(p.modelPath.EndsWith(".gguf"), p.id + " 模型路径应指向 gguf 文件");
            }
        }

        [Test]
        public void Preset_Lookup_By_Id()
        {
            Assert.IsNotNull(LlmPresets.Find(LlmPresets.Qwen4B));
            Assert.IsNotNull(LlmPresets.Find(LlmPresets.Gemma4B));
            Assert.IsNotNull(LlmPresets.Find(LlmPresets.Ornith9B));
            Assert.IsNull(LlmPresets.Find(""), "空 id = 自定义，应返回 null");
            Assert.IsNull(LlmPresets.Find("no-such-preset"));
        }

        [Test]
        public void Default_Config_Uses_The_Measured_Fastest_Preset()
        {
            var cfg = new LlmConfig();
            Assert.AreEqual(LlmPresets.Qwen4B, cfg.preset, "默认应指向实测最快的 4B 预设（44~51 t/s）");
            var p = LlmPresets.Find(cfg.preset);
            Assert.IsNotNull(p);
            Assert.AreEqual(p.modelPath, cfg.modelPath, "默认 modelPath 与预设不一致（换预设时要同步）");
            Assert.AreEqual(p.ctx, cfg.ctx, "默认 ctx 与预设不一致");
            Assert.IsFalse(cfg.noSpec, "自投机默认开启（作者要求接入）；本游戏负载下实测无收益，一行开关可关");
        }

        [Test]
        public void Only_The_9B_Preset_Mounts_A_Lora()
        {
            // 4B Deckard 是「消融+RP 同权重」，另挂 LoRA 反而可能破坏；9B 档才需要 RP-LoRA
            foreach (var p in LlmPresets.All)
            {
                if (p.id == LlmPresets.Ornith9B) Assert.IsNotEmpty(p.loraPath, "9B 档应挂 RP-LoRA");
                else Assert.IsEmpty(p.loraPath, p.id + " 不应挂 LoRA（权重内已含 RP 微调）");
            }
        }

        [Test]
        public void Json_Schemas_Are_Valid_And_Match_Parser_Keys()
        {
            string err;
            Assert.IsTrue(LlmSchemas.Validate(out err), "schema 自检失败：" + err);
        }

        [Test]
        public void Schema_Field_Names_Match_What_Parsers_Read()
        {
            // ParseTalk 读 greeting/lines/options（选项 label/mood/reply）；ParseMicro 读 title/paras/options
            StringAssert.Contains("\"greeting\"", LlmSchemas.Talk);
            StringAssert.Contains("\"lines\"", LlmSchemas.Talk);
            StringAssert.Contains("\"reply\"", LlmSchemas.Talk);
            StringAssert.Contains("\"title\"", LlmSchemas.Micro);
            StringAssert.Contains("\"paras\"", LlmSchemas.Micro);
            StringAssert.Contains("\"result\"", LlmSchemas.Micro);
            StringAssert.Contains("\"ok\"", LlmSchemas.Ping);
        }
    }
}
