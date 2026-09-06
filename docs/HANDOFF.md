# STARSTATE 开发交接手册（HANDOFF）

> **给新接手的 Agent**：先读 [AGENTS.md](../AGENTS.md)（项目共识与纪律），再读本文（怎么干活）。历史决策查 [changelog/](../changelog/)，设计逻辑查 [docs/PHASE4-DESIGN.md](PHASE4-DESIGN.md)。目标：**30 分钟内安全提交第一处改动**。
> 最后更新：2026-09-06（Phase 4 收口后）。

---

## 1. 项目一句话与当前状态

政府职业成长模拟游戏：架空世界"中华帝国"（设定以 `E:\AI\帝国\设定\` 为唯一事实来源，**只读**），玩家 23 岁入职长安市发展和改革局，2026-2036 十年科员→（转官）十品副处。

| 项 | 状态 |
| --- | --- |
| Phase 1-4 | 全部完成；Phase 4 = 回响引擎＋周计划＋档案公文美学＋内容大填充（2026-09-06） |
| 测试 | EditMode **17/17** 全绿；独立编译四程序集零错误（`tools/check-compile.sh`） |
| 存档 | JsonUtility，**saveVersion 4**（GameApp.SaveVersion 门槛，旧档引导重开） |
| 规模 | 37 个 C# 文件 / 约 11000 行；151 处事件注册（运行时唯一事件 130+）；任务模板 50；新闻 118；LLM 增强层（本地 llama-server 标定参数 / 外部 OpenAI 兼容） |

## 2. 代码地图（E:\Starstate\game-src\Assets\Scripts\，改这里再同步到 game\）

### Core/（纯 C#，无 UnityEngine 依赖——保持！）

| 文件 | 职责 | 接手须知 |
| --- | --- | --- |
| `Flow.cs` | 引擎心脏：事件调度、场景状态机、效果结算、周月节奏、快进、动态事件（人事/考核/结局/NPC 主动） | 改调度逻辑前先读 §6 坑清单；`CollectDue` 是每日事件入口 |
| `Model.cs` | 全部 POCO：GameState/Effects/GameEvent/When/OptionWhen/EchoJob/ClockState/WeekPlanState/RivalState… | JsonUtility 存档：**禁止 Dictionary**；新字段必须给默认值 |
| `State.cs` | NewGame 工厂（玩家初始属性唯一赋值点） | Attrs 默认值必须为 0（它也是增量容器） |
| `Career.cs` | 晋升年限/年度考核评定/结局计算＋**结局个人史（AppendMarkEchoes）** | 晋升规则=总设定冻结版，别动数值语义 |
| `GameClock.cs` | 日历：节假日表（2028 起为【占位推演】）、周/月边界 | |
| `Npcs.cs` | NPC 定义表＋关系系统（familiar/trust/evalv/memories） | 9 个默认人物 |
| `Systems.cs` | `Clocks`（时钟仪表）＋`NpcTick`（每周淡忘/主动找你） | 时钟满格触发由 Flow.CheckClocks 完成 |
| `TaskGenerator.cs` | 50 个任务模板 × **五种决策骨架**（例行/限时/协作/风险/露脸） | 模板只管主题，骨架提供选项结构 |
| `Llm.cs` | LLM 全部 Core 侧：LlmPrompt（JSON schema 与【纯文本】协议）、MiniJson、LlmJson 容错解析、LlmGameplay 白名单效果 | AI 只产文本，数值效果必须走白名单 |
| `Content*.cs` | 内容数据（见 §4 配方）；`ContentRegistry.RegisterAll` 是唯一注册入口 | ContentChains=剧情链，ContentMonthly=月度十二节律 |

### Ui/（UGUI 全代码构建，零美术资产）

| 文件 | 职责 | 接手须知 |
| --- | --- | --- |
| `GameApp.cs` | 组合根＋页面数据流＋存档＋LLM 编排；`GameBootstrap` 运行时自举 | Boot 场景**零脚本**是纪律；**存档管线=主线程序列化＋线程池原子落盘（WriteSaveAtomic）**，新增写盘一律走它；字体静态缓存 |
| `UiRoot.cs` | 约 2000 行全部 UI：主题色/程序纹理/红头文件/印章/剪报/履历卡/台历/周计划编辑器/设置/交谈/引导 | `Build(font, docFont, scale)` 整树重建是换肤管线；**RenderMain 有场景签名去重**（同签名跳过重建）——改重建逻辑时注意 `lastSceneSig` 重置时机；周计划 ± 走 `UpdatePlanEditor` 局部刷新 |
| `LlamaServer.cs` | llama-server 生命周期＋**标定启动参数**（ngl28+KV q4_0+ub128+fa+t16 绑核，降级阶梯兜底） | 参数有实测依据（changelog/PHASE-4 补丁十），别随手改；Kill 含 Dispose |
| `LlmClient.cs` | HTTP（UnityWebRequest），Chat 支持取消谓词（会话作废即 Abort） | |
| `Tween.cs` / `ButtonFx.cs` / `TextBuilders.cs` / `SoundFx.cs` | 补间库 / 按钮微反馈 / 纯字符串面板文本 / 程序合成音效 | 协程入口必须判空（销毁竞态） |

### Tests/（EditMode，NUnit）
`FlowSmokeTest`（序章九月）· `TenYearSmokeTest`（十年自动通关——**任何引擎改动的回归底线**）· `TaskCheckGrades`（在 FlowSmokeTest 内）· `LlmSmokeTest` · `GrowthSmokeTest` · `P4EngineTest`（回响/池不枯竭/锁定/时钟/淡忘）。

### 数据流（只读视图模式）
`GameApp.RenderAll → Flow.CurrentScene(st) → Scene{kind,title,paras,options,optionLocks,planValues,docNo} → UiRoot 渲染`；输入：`ui.OnOptionChosen/OnPlanAdjusted/… → GameApp → Flow.Choose/SetPlan`。**Ui 不改状态，Core 不碰 UnityEngine。**

## 3. 核心机制速查（改内容前必读）

- **Phase 状态机**：Prologue → WeekPlan（周一）→ Day×4 → WeekEnd（周五例会）→ Weekend → GotoMonday…月末最后工作日 SettleMonth；年度节点=动态事件 `sys_annual_eval`(1/15)、`sys_personnel`(9/20)；2036-08 结局。
- **每日事件收集**（Flow.CollectDue，按序）：① 回响队列（到期 echo，带 flag/marks 门槛的未达标即耗散）→ ② date 事件 / md 循环事件（+requireMarks） / flag 事件 → ③ 随机池**加权单抽**（0.45/工作日）→ ④ 时钟满格。
- **marks**（叙事标记，持久化）：`Effects.setMarks/clearMarks` 写；`When.requireMarks/requireNotMarks`（事件级）与 `OptionWhen.mark/notMark`（选项级）读；结局个人史消费。这是"选择的长影子"的载体。
- **回响（Echo）**：`Effects.echoes=[{eventId,afterDays}]`；回响事件 `when=null`（或带门槛做条件回响）。
- **随机池**：`randomP>0` 入池；`weight` 权重、`maxFires`（默认 1=一生一次）、`cooldownDays`、`ambition` 门槛。**池永不枯竭**（有测试保证）。
- **选项双轨门槛**：结构性（grade/route/flag/notFlag/年限/优秀次数/基层月数）不满足→**隐藏**；叙事性（mark/notMark/relNpc+minFamiliar/minTrust）不满足→**锁定可见**（Scene.optionLocks 给原因，UI 自动置灰）。
- **周计划**：100 点精力五槽位（岗位/学习/人际/家庭/休整）；结算=`Flow.PlanGrowth`（例会与快进静默周共用）；岗位投入→`TaskCheck` +plan.work×0.1 评级加成；快进=AutoPlan 沿用上周计划。
- **任务五骨架**：TaskGenerator.BuildByKind 按 55/15/12/10/8 抽签，同一模板主题套不同决策结构。
- **结局个人史**：`Career.AppendMarkEchoes` 按 marks 装配"十年回响"段——加新链时记得在这里补一行。
- **LLM 协议**：JSON 任务走 `LlmJson.ExtractFirstJson` 容错解析；纯文本任务在 user prompt 标注【纯文本】；效果一律 LlmGameplay 白名单；任何 AI 失败**静默回退**内置内容。交谈请求带取消谓词（弹层关闭即 Abort 在途请求）。
- **存档管线**：`Save()`=主线程序列化（紧凑 JSON）→ 线程池 `WriteSaveAtomic`（临时文件→长度校验→`File.Replace` 原子替换，带写入锁）；`OnDestroy` 同步兜底。**新增持久化写盘一律走 WriteSaveAtomic**，不要在主线程直接 WriteAllText。
- **渲染去重**：`RenderMain` 按场景签名跳过同场景重建（去闪烁/去 GC 尖峰）；周计划 ± 走 `UpdatePlanEditor` 局部刷新，不走 RenderAll。
- **内存上限**：log 600 条（裁最旧 100）、NPC memories 8 条、poolLog 500 条；任务/材料档案**有意**全量保留（十年工作档案是内容本体）。

## 4. 常用配方（照抄即可）

**加一个脚本事件**：在合适的 `Content*.cs` 的 `Register()` 里 `Flow.Register(new GameEvent{ id="ev_xxx", type="work|person|society|politics|oversight|system", title=…, when=new When{ … }, paras={…}, options={…} })`。`When` 四种触发：`date:"2027-05-04"`（一次性）／`md:"05-04",fromYear:2027`（每年）／`flag:"xxx"`（置位次日）／`randomP:0.1`（入池，配 weight/maxFires/cooldownDays）。`when=null`=只由回响/队列触发。**id 全局唯一**；入职前的内容不要给 docNo（引擎已按 phase 判定）。

**加一条剧情链**：触发事件选项 `effects = new Effects{ setMarks={"xxx_m"}, echoes={new EchoSpec{ eventId="ev_xxx_b", afterDays=90 }} }`；回响事件 `when=null`（无条件）或 `when=new When{ flag/mark 门槛 }`（条件回响，未达标耗散）；分支结局用多个同日事件各自 `requireMarks` 不同标记。参考 `ContentChains.RegisterLedgerChain()`（数据造假案三幕）。

**加任务模板**：`TaskGenerator.Pool` 加 `new Template{ title,note,para,main,routes }`；五骨架自动套用；风险骨架的"口径变通"选项自带程序合规标记——新增模板不要破坏这一结构。

**加日常/新闻**：日常文案进 `ContentRegistry.GenericDayPools` 四池之一；新闻在 `ContentNewsExtra`（或新文件）里 `News.Add("2031-06-18","国内","标题","正文")` 并在 ContentRegistry 注册你的 Register()。

**加锁定选项**：`when = new OptionWhen{ relNpc="zhou", minTrust=5, mark="xxx_m" }`——UI 自动显示"🔒 需要周衡之的信任（3/5）"并禁点。

**加 LLM 能力**：`LlmPrompt` 加 User 提示词（JSON schema 或【纯文本】）→ GameApp 编排协程 → 失败静默。参考 `WeekReviewUser`＋`RequestWeeklyReview`。

**调数值**：周计划结算 `Flow.PlanGrowth`；任务评级 `Flow.TaskCheck`（阈值 78/65/52/40）；晋升年限 `Career` 常量；体力耗损 `Flow.Drift`。改完必须跑十年回归测试。

## 5. 验证门禁（每个改动都要过，顺序执行）

```bash
# ① 独立编译（秒级，四程序集零错误）
bash E:/Starstate/tools/check-compile.sh          # 期望末行 ALL_OK

# ② 同步到 Unity 工程（只增改；绝不删除 game/Assets 下任何 .meta）
cp -r E:/Starstate/game-src/Assets/Scripts/. E:/Starstate/game/Assets/Scripts/

# ③ Unity EditMode 测试（数分钟；先确认无残留实例占工程锁）
wmic process where "name='Unity.exe'" get ProcessId,CommandLine 2>/dev/null | tr -d '\r' | grep -i '\-batchmode'
#   有输出=先按上面查到的 PID 精确 taskkill（只准杀 -batchmode 实例，绝不准按名字杀 Unity）
"D:/pro/unity/Editor/Unity.exe" -batchmode -nographics -projectPath E:/Starstate/game \
  -runTests -testPlatform EditMode -testResults E:/Starstate/tmpbuild/testresults.xml \
  -logFile E:/Starstate/tmpbuild/testlog.txt
#   结果 XML 会先写出，进程可能挂起——等通知后直接读 XML；总期望 17/17
#   目录名别用 .tmpbuild（点开头目录 Unity 拒绝）
```

UI 表现与 LLM 链路自动测不到：Play 模式手测要点——主菜单开新局走完序章（含"你为什么来"）、周一周计划拖拽、事件红头/印章、新闻剪报、人物履历卡、设置页音效；AI 开启时测试连接＋一次交谈。

## 6. 坑清单（血泪问题提醒——每一条都真实踩过）

### 工程与同步
1. **game-src 先改、同步只增改、绝不删除 game/Assets 下的 .meta**（GUID 删了=场景引用断裂"missing script"）。
2. **Boot 场景零脚本**：入口是 GameBootstrap 的 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` 自举。不要往场景挂 MonoBehaviour——编辑器脚本重载窗口期保存场景会剥落引用。
3. 世界观资料 `E:\AI\帝国\设定\` 只读不复制；一切产出放 `E:\Starstate`；需要作者拍板的登记 `docs/OPEN_QUESTIONS.md`（默认方案先行，可否决）。

### Unity 批处理
4. 结果 XML 先写出、**Unity 进程经常挂起不退**——读 XML 即可，清理只按 `-batchmode` 命令行特征，**绝不按进程名杀 Unity**（用户编辑器可能开着）。
5. `wmic` 输出带 CRLF：**必须 `tr -d '\r'`** 再 grep/正则，否则静默失配（曾致僵尸实例误判）。
6. "another instance is running with this project open"=有残留批处理实例占工程锁，先查先杀（同第 4 条纪律）。
7. 测试输出目录别用点开头名（`.tmpbuild` 会被 Unity 拒绝），用 `tmpbuild`。

### C# / Unity 运行时
8. **Effects 是注册表共享实例**：Choose 里必须走 `CloneEffects`，否则 rel/morale 跨次执行累积污染（已修，别回退）。
9. **Attrs 字段默认值必须为 0**（它同时是增量容器）；玩家初始属性只在 `State.NewGame` 赋——违反会复发"五维全 100"。
10. 存档类（GameState 及嵌套）**不能有 Dictionary**（JsonUtility）；新增字段必须带默认值；`when=null` 的事件进 MarksMet 等判定前要防 null（已修，别回退）。
11. 内容文本里的引号一律用中文引号""——**ASCII 双引号会截断 C# 字符串**（批量替换/脚本生成内容后必须过编译门禁）。
12. 布局：HorizontalLayoutGroup 的 childControl 必须 true，否则 LayoutElement.preferred 尺寸不落实（"巨大色块"教训）。
13. 所有 Tween 协程与延迟回调入口**判空再动**（Delayed 恢复时目标可能已被整树重建销毁 → MissingReferenceException）。
14. 修改用户存档（调试时）：**临时文件→校验→原子替换**，绝不直接覆写（曾有截断丢档事故）。

### Python（环境里是 Python 2）
15. `io.open(p, encoding=…)` 会报错、print 是语句、json 句柄有坑——脚本加 `# -*- coding: utf-8 -*-`，写文件用 `open(path,'wb').write(s.encode('utf-8'))`。
16. 批量改文本后**先 grep 验证再编译**（全角/半角引号、CRLF 都是真实事故来源）。

### llama-server / LLM
17. **严禁双实例**：16GB RAM 装不下两个 9B 模型，并跑会卡死整机（真实事故）。测试前按进程名清点必须=0、测完按名清理并复核。用户自己可能常驻 8081 的服务——**起测试实例前先看**。
18. Git Bash 里 **`$!` 不是 Windows PID**，不能作为 taskkill 依据；用 `wmic process where "name='llama-server.exe'" get ProcessId | tr -d '\r'` 查真实 PID。
19. 启动参数已按 `D:\AI` 报告标定（`-ngl 28 -ctk q4_0 -ctv q4_0 -ub 128 -fa on -t 16 --cpu-range 0-19`，见 changelog/PHASE-4 补丁十），改动前先读报告；曾把"双实例争抢"误判为"-fa on 中毒"——**排除性能问题必须单实例对照**。
20. Git Bash 的 curl 直接发中文 JSON 会转码毁掉（server 报 ill-formed UTF-8）——写 UTF-8 文件用 `--data-binary @file`。
21. 长提示测速必须走 llama-server（llama-cli 对 64K+ prefill 会静默死锁，D:\AI\README 实测结论）。

## 7. 已知短板与下一步候选（接手后可做的事）

- **时钟内容**：引擎层完备（建立/推进/满格触发/测试覆盖），但尚无常驻剧情时钟（候选：巡视组倒计时、年度考核冲刺仪表、专项 deadline）。
- **竞争者系统**：目前是"势头仪表＋人事季文案"，可做独立里程碑链（许飞晋升事件、同批分化）。
- **NPC 头像**为印章式占位；履历卡已预留槽位，未来接美术资产即可替换。
- **AI 家信/微信**、AI 剧情润色：复用【纯文本】协议即可（周评已落地，照抄 RequestWeeklyReview 模式）。
- 音频目前是程序合成三件套（章/纸/点）；立绘、BGM、打包发布（脱离 Unity 编辑器运行）均未做。
- 升级路径：Unity 6 LTS（AGENTS 技术栈一节）；发布形态未定。

## 8. 规模速览（2026-09-06）

| 维度 | 数值 |
| --- | --- |
| C# 文件 / 行数 | 37 / ≈11000（Core 19 · Ui 7 · Editor 1 · Tests 5＋asmdef） |
| 事件注册（Flow.Register） | 151 处（运行时唯一 id 130+；md 循环事件按年展开） |
| 剧情链 | 数据造假案三幕、AI 专班、房子、父亲体检、挖角（中途辞职）、老康来信、初心对账 等 |
| 任务模板 / 日常文案 / 新闻 | 50（×5 骨架）/ 117 / 118 |
| 月度主题 | 12 节律 × md 循环 |
| 测试 | EditMode 17 项全绿（含十年自动通关回归） |
| 存档 | saveVersion 4（marks/echoQueue/clocks/plan/rival/ambition/poolLog） |
