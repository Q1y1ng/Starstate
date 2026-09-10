# STARSTATE 开发交接手册（HANDOFF）

> **给新接手的 Agent**：先读 [AGENTS.md](../AGENTS.md)，再读本文。Phase 5 规格：[papers-desk-overhaul.md](compose/spec/papers-desk-overhaul.md)。历史：[changelog/](../changelog/)。
> 最后更新：2026-09-10（Phase 5 · v0.2.3 市长办公桌）。

---

## 1. 项目一句话与当前状态

社会人生模拟：架空「中华帝国」（设定 `E:\AI\帝国\设定\` **只读**）。**当前玩法：七品·大同市市长沈砚舟（41 岁）卷宗签批**——翻页→核对→处置；两把尺（合规×效率）；常委会权力地形；口径手册（案头 4 槽）。Phase 1–4 科员十年线在仓库（吏轨 API 留桩，内容未注册）。

| 项 | 状态 |
| --- | --- |
| Phase 1-4 | 完成（2026-09-06） |
| Phase 5 | M0＋v0.2.x：卷宗引擎/七品开局/木纹桌面/口径/模板池/常委密谈/两条链（2026-09-10） |
| 测试 | EditMode **30 Passed / 0 Failed / 5 Skipped**（跳过=旧 ContentClockRival） |
| 存档 | **saveVersion 5**；旧档引导重开 |
| 编译 | `tools/check-compile.sh` → ALL_OK |

## 2. 代码地图（改 `game-src/` 再同步 `game/`）

### Core/（纯 C#，禁 UnityEngine）

| 文件 | 职责 | 接手须知 |
| --- | --- | --- |
| `DossierEngine.cs` | 登记/周分配/翻页/核对/签批/两把尺 | `Begin` 只 `ClearRuntime()`，**不要** `Clear()` 注册表 |
| `Rulebook.cs` | 案头 4 槽顶旧、检索、DeskHint | `knownRules` vs `deskRules` |
| `ContentDossierM0/Y1` | 教学3＋高光3＋池＋季节件 | 高光 marks 驱动链 |
| `ContentChainsMayor` | 对上报告 / 算法审批 | `requireMarks` 是 **AND**，分叉拆事件 id |
| `DossierGenerator` | 14 模板×变量，`SeedPool(36)` | id=`dz_g{seq}_{MMdd}` 唯一 |
| `ContentRulebook` | 6 条口径 | issue.ruleKey |
| `ContentNpcTalk` | 常委台词＋回退选项 | 无 AI 可玩 |
| `ContentPrologueMayor` | 序章三幕 | |
| `ContentCareerMayor` | 年度考核/结局 | |
| `Flow.cs` | 事件优先于卷宗；`DossierScene` 眉/铅笔痕/DeskHint | `dayFfable` 含 `queue.Count==0` |
| `Model.cs` | GameState 卷宗/口径/两把尺 | saveVersion 5；**禁 Dictionary** |
| `State.cs` | 七品 NewGame＋授 3 口径 | |
| `Career.cs` | 七品→六品双尺考核 | 旧吏轨留桩 |
| `Llm.cs` | 市长层提示词；周评=周谨 | 效果白名单 |
| `Npcs.cs` | 常委会权力地形 | |
| `ContentRegistry.cs` | 唯一注册入口 | 科员线暂不注册 |

### Ui/

| 文件 | 职责 |
| --- | --- |
| `GameApp.cs` | 组合根；OpenRule；密谈；LLM |
| `UiRoot.cs` | 木纹/灯晕/7 页签（含**口径**）/权力板/市长状态页 |
| 其余 | 同 Phase 4（Tween/LlmClient/LlamaServer…） |

### Tests/
`DossierEngineTest` · `RulebookTest` · `GeneratorPoolTest` · `FlowSmokeTest` · `TenYearSmokeTest` · `BalanceSmokeTest` · `GrowthSmokeTest` · `P4EngineTest` · `LlmSmokeTest`（选项 2–3）。

### 数据流
`RenderAll → Flow.CurrentScene → Scene{kind=dossier|result|week_plan|…} → UiRoot`。
卷宗选项序：上一页?/下一页?/核对/处置…（`Flow.ChooseDossier`）。

## 3. Phase 5 机制速查

- **周一** `AssignWeek`：showcase/deadline 优先，再抽池；budget 默认 5。
- **核对**：当前页有雷必中；案头口径 → DeskHint；错页 → 提示第 N 页；查出可补授口径。
- **两把尺**：漏雷照准扣合规（severity×4）；逾期扣效率；进考核与六品门槛。
- **口径**：`Grant` 入档案+上案头；满 4 顶最旧；`Open` 从档案捞回。
- **链 marks**：`pressed_report`/`honest_report`；`algo_auto`/`algo_pass`/`algo_veto`。
- **快进**：静默办结卷宗；队列非空必停。

## 4. 工程纪律（强制）

1. 改 `game-src/` → `robocopy src dst /E /XF *.meta`；**绝不删 `.meta`**。
2. 门禁：`check-compile.sh` ALL_OK → 同步 → EditMode。
3. 中文引号 `""`；ASCII `"` 会截断 C# 字符串。
4. 测试换注册：`DossierEngine.Clear()` + `Rulebook.ClearRegistry()`。
5. Boot 场景零脚本；`GameBootstrap` 自举。
6. Effects 走 `CloneEffects`；Attrs 默认 0；存档无 Dictionary。

## 5. 验证门禁

```bash
# ① 编译
"C:\Program Files\Git\bin\bash.exe" E:/Starstate/tools/check-compile.sh

# ② 同步（PowerShell）
robocopy E:\Starstate\game-src\Assets\Scripts E:\Starstate\game\Assets\Scripts /E /XF *.meta

# ③ EditMode（结果 XML 先写出，进程常挂——读 XML；只杀 -batchmode 实例）
# Unity: D:\pro\unity\Editor\Unity.exe -batchmode -nographics -projectPath E:/Starstate/game
#   -runTests -testPlatform EditMode -testResults <xml> -logFile <log>
```

Play 手测金路径：新局→序章三幕→周计划→第一周卷宗（摊开口径→翻页→核对→签批）→状态页两把尺/权力板→口径页签检索。

## 6. 坑清单（真实踩过）

### 工程
1. 绝不删 `game/Assets/**/*.meta`（GUID→missing script）。
2. Boot 场景零脚本。
3. 世界观 `E:\AI\帝国\设定\` 只读。

### Unity 批处理
4. 测试 XML 先写出、进程常挂——读 XML；只按 `-batchmode` 特征杀进程，**绝不按名字杀 Unity**。
5. 输出目录别用点开头名；用 `tmpbuild` 或直接 game/ 下 xml。

### C#
6. **Effects 是共享实例**：必须 `CloneEffects`。
7. Attrs 默认 0；初始值只在 `State.NewGame`。
8. 存档禁 Dictionary；新字段带默认值。
9. 内容文本用中文引号。
10. HorizontalLayoutGroup 的 childControl 必须 true。
11. Tween/延迟回调入口判空。
12. `requireMarks` 数组 AND；OR 要拆事件。
13. Effects **无** compliance/efficiency 字段——两把尺只走 DossierOption。

### LLM
14. **严禁双 llama-server**（16GB 会卡死）。
15. 失败静默回退内置内容。

## 7. 下一刀

| 优先 | 项 |
| --- | --- |
| 高 | T4 完整 DeskRoot 拆分；实机标定两把尺 |
| 高 | M1 手写 40+ 精修；恢复 ContentClockRival |
| 中 | 口径案头便签视觉；NewsPile 桌角化 |
| 低 | Windows 包；Unity 6 |

## 8. 规模速览（2026-09-10）

| 维度 | 数值 |
| --- | --- |
| 玩法起点 | 七品市长 · 大同 · 2026-09 |
| 卷宗 | 手写 12＋模板池 36＋Generator 14 套 |
| 口径 | 6 条 · 案头 4 槽 |
| 常委密谈 | 10 人 × 4 条 |
| 剧情链 | 对上报告、算法审批（分叉） |
| 测试 | 30 过 / 5 跳过 |
| 存档 | saveVersion 5 |
