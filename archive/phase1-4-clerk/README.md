# Phase 1–4 科员线内容归档（未删除，仅下架）

> 状态：2026-09-10 Phase 5 v0.2.6 起，主玩法改为七品市长办公桌。  
> 科员十年线（长安发改局）内容**不再参与编译与注册**，源文件移入本目录保存，便于后续迁移或复活。

| 文件 | 原职责 |
| --- | --- |
| ContentPrologue.cs | 科员序章 |
| ContentCareer.cs | 吏三→十品晋升/考核事件 |
| ContentChains.cs | 数据造假案、AI 专班、房子、父亲、挖角、老康来信等链 |
| ContentClocks.cs | 考核冲刺/巡视/专项（科员口径） |
| ContentRival.cs | 许飞/苏晴/何斌同批里程碑 |
| ContentYearsA/B/C.cs | 2026–2036 年度大事件 |
| ContentLife.cs / ContentFlavor.cs / ContentMonth1.cs / ContentMonthly.cs | 生活、风味、首月、月度节律 |

## 恢复方式

1. 把对应 `.cs` 拷回 `game-src/Assets/Scripts/Core/`
2. `robocopy` 同步到 `game/`（**不要**直接覆盖 `.meta` GUID；新文件让 Unity 生成）
3. 在 `ContentRegistry.RegisterAll` 按需重新注册
4. 跑 `check-compile.sh` + EditMode

## 仍保留在主工程

- `ContentNews.cs` / `ContentNewsExtra.cs`（新闻流仍注册）
- `TaskGenerator.cs`（日常任务模板仍用）
- `Career.cs`（Phase 5 七品路径；旧吏轨 API 留桩）
- `Systems.cs` Clocks/NpcTick 引擎
