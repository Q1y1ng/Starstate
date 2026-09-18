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
| 测试 | EditMode **95 Passed / 0 Failed / 0 Skipped**（12 审计回归 ＋ 11 剧情线路 ＋ 11 内容量验收 ＋ 24 M2/M3 验收） |
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
| `ContentRoutesMayor` | **剧情线路（M1）**：路线确立＋四线（12 环）＋常委会/竞争/邀约链 | md 事件每年会重现 → 每环必须自带 `requireNotMarks` 完成标记；路线分叉一律用 marks（`When.route` 只对 date 触发生效） |
| `ContentDossierRoutes` | 四件路线专属卷宗 | 路线处置用 `whenMark` 锁，`lockReason` 写解锁条件 |
| `ContentDossierY2` | 手写卷宗第二辑 10 件（M1 内容量） | `generated` 只对模板件为 true（手写件勿标，否则内容量审计会漏计） |
| `ContentChainsRisk` | 安全事故瞒报压力链＋招商引资对赌链 | 同一节拍的分叉用不同事件 id ＋族标记（宽/严两条对赌线靠卷宗同时写具体标记与族标记） |
| `ContentTemplateChains` | **模板挂链**：模板件灰区标记的延迟回响（7 条） | 模板件也能有后果——标记名与 `DossierTemplatesY2` 一一对应，改名必须两边一起改 |
| `ContentChainsHome` | 旧链迁移：配偶从业与回避、医疗资源打招呼 | 两条链都写 `risk`/`integrity`，是立案审查结局的现实来路之一 |
| `DossierTemplate(s)` / `DossierTemplatesY2` | 模板结构＋两批模板数据（Y1 14 件 / Y2 20 件） | **总表顺序＝读档重建顺序**：新增模板只能往后追加 |
| `ContentNpcTalk` | NPC 台词池（10 人×8 句）＋ `StateLine` 状态感知台词（20 条） | 选项随熟悉度/信任解锁；效果仍走 mood 白名单 |
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
| `ContentRegistry.cs` | 唯一注册入口（启动时由 GameApp.Init / State.NewGame 调用） | 科员线暂不注册；RegisterAll 自愈可重入 |
| `GameLog.cs` | Core 的日志出口（Ui 层接线到 Debug.Log） | 让 Tests 不依赖 UnityEngine，可进独立编译门禁 |

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
- **两把尺**：漏雷照准扣合规（severity×4）；逾期扣效率；**办得干净（无漏查且未逾期）时谨慎处置不再倒扣效率**；月末回补（本月无漏查合规 +1、案头清空无逾期效率 +1）——防长线死亡螺旋。快进时引擎会**先翻页核对再办结**，并按 `DossierEngine.BestOptionIndex`（尽责启发式）选处置。
- **风险账本**（`st.riskLedger`）：不可见的历史存疑，灰区处置/逾期/漏查重大雷会记；**一个月没添新账只回落 1**。40 → 纪委谈话提醒（可主动交底降账）；**65 且程序违规 ≥2 → 立案审查结局**。
- **考核不拿终身累计**：`EvaluateYear` 按**年度** `yearIntegrity`＋两把尺＋风险账本判档；不称职**分级**（首次降级留任，再犯免职），累计三次基本称职→降级调离。
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
# ① 编译（**四程序集**：uGUI / Core / Ui / Editor / Tests——Tests 已进门禁）
"C:\Program Files\Git\bin\bash.exe" E:/Starstate/tools/check-compile.sh

# ② 同步（PowerShell）
robocopy E:\Starstate\game-src\Assets\Scripts E:\Starstate\game\Assets\Scripts /E /XF *.meta

# ③ EditMode（结果 XML 先写出，进程常挂——读 XML；只杀 -batchmode 实例）
# Unity: D:\pro\unity\Editor\Unity.exe -batchmode -nographics -projectPath E:/Starstate/game
#   -runTests -testPlatform EditMode -testResults <xml> -logFile <log>
# ④ 再跑下一轮前先确认上一轮实例已退出（否则报“工程已在另一实例中打开”）：
#   Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | ? { $_.CommandLine -like '*-batchmode*' } | % { Stop-Process -Id $_.ProcessId -Force }

# ⑤ 内容/平衡秒级探针（脱离 Unity 跑 Core）：改卷宗/链/数值后先跑这个
#   输出：卷宗注册数（手写/模板）、来文形态、第一年空周数、十年长跑结局与两把尺
"C:\Program Files\Git\bin\bash.exe" E:/Starstate/tools/core-probe.sh all
```

当前基线：**check-compile ALL_OK（5 段）+ EditMode 95/95 全绿**（2026-09-12）。

> Tests 为何单独一条编译链：NUnit 是 net35 档，与 netstandard 2.1 无法混引（CS0012/CS0518）。
> 因此 `GameLog.cs` 给 Core 提供日志出口，让 Tests **不引用 UnityEngine**，从而能进独立门禁。

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
13. `Effects` **已有** `compliance`/`efficiency`/`risk` 字段（M1 起）：剧情链可推动两把尺与风险账本；卷宗签批走 `DossierOption.complianceDelta`，两套并存。

### LLM
14. **严禁双 llama-server**（16GB 会卡死）；游戏会自动检测端口/已有进程，只等待不重拉。
15. 失败静默回退内置内容。
16. **结构化输出**：凡是游戏要**解析**的 AI 返回（交谈 `Talk`、小插曲 `Micro`）必须带 `response_format.json_schema`（`LlmSchemas.*`）。实测不带约束时合法 JSON 只有 25%~67%，带上后 100%（流式同样有效）；`{"type":"json_object"}` 在本机 b10343 上**无效**（不约束），只能用完整 schema。改 schema 时字段名必须与 `LlmJson.ParseTalk/ParseMicro` 的读取键一致，`LlmSchemas.Validate()` 与 `LlmPresetTest` 会拦住笔误。
17. **模型预设**（`LlmPresets.All`）：4B-Qwen（默认）/ 4B-Gemma / 9B-Ornith+LoRA。切换预设必须先 `LlamaServer.Kill()`，否则下次连接检测到端口已有服务就继续沿用旧模型（本游戏最容易踩的坑）。预设里的模型文件不存在时会回落到设置里的自定义路径。
18. **启动参数**：`-ngl 28 -c 16384 -ctk/-ctv q4_0 -ub 128 -fa on -t 16 --cpu-range 0-19 --jinja --reasoning off` ＋可选 `--lora`（RP-LoRA）＋可选 ngram-mod 自投机；失败时自动降档（ngl16/q8_0 → 纯 CPU），并按同档去自投机重试（兼容不认 `--spec-*` 的旧版 llama-server）。
19. **自投机（`ngram-mod`）在本游戏负载下无收益**（2026-09-13 交替 A/B 实测：三轮服务端 `draft` 打印次数均为 0，计时差异全在噪声内）——它是「代码/重复文本」场景的优化，保留开关供作者切换，不要把它当提速手段写进发布说明。

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
| 测试 | 95 过 / 0 跳过 |
| 存档 | saveVersion 5 |
