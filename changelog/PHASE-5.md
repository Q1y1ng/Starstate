# Phase 5 · 市长办公桌（Papers Please 压力驱动）

> 规格：[docs/compose/spec/papers-desk-overhaul.md](../docs/compose/spec/papers-desk-overhaul.md)  
> 起点：七品·大同市市长（41 岁，最快合规轨），2026-09  
> **当前版本：STARSTATE v0.2.6 Preview** · EditMode **37/37** · 存档 saveVersion **5** · main `4491a87`

## 状态快照（v0.2.6）

| 轴 | 状态 |
| --- | --- |
| 玩法 | 卷宗签批（翻页/核对/处置）＋两把尺＋口径案头＋常委密谈＋考核/巡视时钟 |
| 内容 | 手写 Y1 约 16 件＋教学/高光 6＋Generator 模板池 36；对上报告/算法审批链＋风险回响 |
| UI | 木纹桌面（DeskSurface）/红头卷宗/签批章/常委会权力板/口径页签；版本 v0.2.6 Preview |
| 工程 | 独立编译 ALL_OK；Windows 包已重打；科员线归档 `archive/phase1-4-clerk/` |
| 待定 | **平衡等作者实机 Play 后再标定**；DeskRoot 完整拆分；内容精修 |

### 作者拍板（2026-09-10）

1. **Git**：授权直接在 main commit（不 push）→ `c76c612` / `4491a87`
2. **Windows 包**：删旧打新 → `E:/Starstate/builds/Starstate/Starstate.exe`
3. **科员线**：压缩归档，不删除，视后续再迁
4. **平衡**：等 Play 反馈后再动两把尺/晋升门槛

---

## v0.2.6 · 市长时钟恢复＋DeskSurface 拆分＋手写再补＋归档打包（2026-09-10）

| 模块 | 内容 |
| --- | --- |
| 时钟 | `ContentClocksMayor`：考核冲刺／巡视进驻／巡视前碰头 |
| 测试 | `ContentClockRivalTest` 去 Ignore；**37 Passed / 0 Failed / 0 Skipped** |
| UI | `Ui/DeskSurface.cs` 纹理与桌底拆出；菜单版本 **v0.2.6 Preview** |
| 手写卷宗 | Y1 +3：常委会 AI 措施会签（席位雷）、基层减负督查、矿井涉险（监管责任口径） |
| 归档 | 科员线 12 个 Content*.cs 移入 `archive/phase1-4-clerk/`（不编译，可恢复） |
| 提交 | main `c76c612`（作者授权直接 commit，未 push） |
| Windows 包 | 删旧包后重打：`E:/Starstate/builds/Starstate/Starstate.exe`（Build Successful） |
| 验证 | ALL_OK；EditMode 37/37 |

### 工程备忘

- EventOption **无** gray 字段（DossierOption 才有）
- md 日期事件单测可直接 `queue.Add(id)` 跳过 CollectDue

---

## v0.2.5 · 第一年手写补密＋两把尺探针＋风险回响（2026-09-10）

| 模块 | 内容 |
| --- | --- |
| 手写卷宗 | `ContentDossierY1` +10 件：煤电安置（差额雷）、AI 招商对赌、巡视销号、灯会、合作办医（席位雷）、学区、导改、食安、大气应急（口头指示）、走访老干部、省审计进点 |
| 风险回响 | `ch_coal_slush` 安置差额抽查；`ch_air_yellow` 省约谈；`ch_air_orange` 回头看正面；`ch_aitrust_weak` 对赌兑现率 |
| 平衡探针 | `DossierBalanceTest` 4 项：漏查扣合规 / 查全+退回保合规 / 逾期扣效率 / 12 件混合不崩盘 |
| 验证 | ALL_OK；EditMode **34 Passed / 0 Failed / 5 Skipped** |

### 标定结论（自动化）

- 照准 option 自带 complianceDelta（如 dz_t1 照准 −2）；漏雷再按 severity×4 叠加——「查全仍照准」仍会小幅掉合规，设计如此。
- 逾期另扣效率 −8；查全+退回可回补。

---

## v0.2.4 · LLM 市长层＋卷宗铅笔痕＋市长案头日常＋HANDOFF（2026-09-10）

| 模块 | 内容 |
| --- | --- |
| LLM | `System`/`MicroUser`/`WeekReviewUser` 改市长口径；周评叙事人=周谨；微事件=市政府案头 |
| 卷宗 UI | 已查出问题以「铅笔痕」列入案头眉 |
| 日常池 | `GenericDayPools["mayor"]` 12 条案头日常；七品开局优先 |
| 文档 | `docs/HANDOFF.md` 整份重写为 Phase 5 |
| 验证 | ALL_OK；EditMode **30 Passed / 0 Failed / 5 Skipped** |

### 工程备忘

- Effects **无** compliance/efficiency 字段——两把尺只走 DossierOption
- `requireMarks` 数组 AND；分叉拆事件 id
- 测试换注册前 `DossierEngine.Clear()` + `Rulebook.ClearRegistry()`

---

## v0.2.3 · 常委密谈＋模板池上量（2026-09-10）

| 模块 | 内容 |
| --- | --- |
| 密谈 | `ContentNpcTalk` 重写：岑伯衡/韩清/沈砚/邵志远/周谨/老康/林晚/方启年/任慎/许飞 各 4 条市长层台词 |
| 回退选项 | 常委类 3 选项（请教卷宗 warm / 寒暄 / 告辞）；关系经 `LlmGameplay.ApplyTalk` 写档 |
| 模板池 | `DossierGenerator` 模板 8→14（政务公开/招聘/购买服务/省督查/职工待遇/实训基地）；`SeedPool(36)` |
| 测试 | `GeneratorPoolTest`（40 件 id 唯一＋常委台词）；EditMode **30 Passed / 0 Failed / 5 Skipped** |

### 验证

- `tools/check-compile.sh` → **ALL_OK**
- Unity EditMode → **30 Passed / 0 Failed / 5 Skipped**

### 工程备忘

- `requireMarks` 数组是 **AND**；分叉路径要拆成多个事件 id
- Effects 无 compliance/efficiency 字段——两把尺只走 DossierOption
- 测试换注册前：`DossierEngine.Clear()` + `Rulebook.ClearRegistry()`
- 常委回退选项从 2 改为 2–3，`LlmSmokeTest` 断言放宽为区间

---

## v0.2.2 · 口径槽位＋第一年内容骨架（2026-09-10）

| 模块 | 内容 |
| --- | --- |
| 口径案头 | `deskRules` 上限 4；新件顶掉最旧；档案全集 `knownRules` 可检索捞回 |
| 口径 UI | 检索框（标题/类别/正文/出处）；「案头」与「档案」分区；上案头/摊开/收起 |
| 模板池 | `DossierGenerator`：8 套模板×局名/数字变量，启动注入 18 件；1/3 埋合计雷 |
| 第一年手写 | `ContentDossierY1`：供热检修、预算控制数、御河汛情（季节 deadline） |
| 剧情链 | `ContentChainsMayor`：对上报告（如实/含水分→省对账/审计抽查→再报把关）；算法审批（人工终审→手册收录 / 自动→责任真空 / 否决→邻市对比） |
| 测试 | `RulebookTest` 5 项；EditMode **28 Passed / 0 Failed / 5 Skipped** |

### 验证

- `tools/check-compile.sh` → **ALL_OK**
- Unity EditMode → **28 Passed / 0 Failed / 5 Skipped**

### 工程备忘

- `requireMarks` 数组是 **AND**；分叉路径要拆成多个事件 id
- Effects 无 compliance/efficiency 字段——两把尺只走 DossierOption
- 测试换注册前：`DossierEngine.Clear()` + `Rulebook.ClearRegistry()`

---

## v0.2.1 · 口径手册（2026-09-10）

| 模块 | 内容 |
| --- | --- |
| Core | `Rulebook` + `ContentRulebook`（6 条手写口径：数字勾稽/行文程序/土地人情/算法法定权限/对上统计/安全销号） |
| 存档 | `knownRules` / `openRule`；`DossierIssue.ruleKey` |
| 开局 | NewGame 授予三份常备口径，默认摊开《财政资金测算：合计与分项勾稽》 |
| 核对 | 案头口径命中时卷宗页眉给 DeskHint；核错页时提示「更常出现在附件或签批栏·第 N 页」；查出问题可补授口径 |
| UI | 侧栏新页签「口径」：列表/摊开到案头/收起；摊开卡显示可执行正文；状态页显示案头口径 |
| 序章 | mp2 周谨台词点明桌角口径手册 |
| 测试 | `RulebookTest` 3 项；EditMode **26 Passed / 0 Failed / 5 Skipped** |

### 验证

- `tools/check-compile.sh` → **ALL_OK**
- Unity EditMode → **26 Passed / 0 Failed / 5 Skipped**

### 工程备忘

- `Flow.Begin` 只 `DossierEngine.ClearRuntime()`，**不要** `Clear()` 注册表
- 事件队列优先于卷宗：`CurrentScene` 先判 queue/currentEvent
- `dayFfable` 必须含 `queue.Count == 0`，否则 FF 空转日期不动
- 测试里换注册内容前：`DossierEngine.Clear()` + `Rulebook.ClearRegistry()`

---

## v0.2 Preview（2026-09-10）

版本定名：**STARSTATE v0.2 Preview**（主菜单右下角同步）。相对 v0.1 的体验差：

| 模块 | 改动 |
| --- | --- |
| 桌面 | 全局底由米纸改为**程序木纹**（胡桃色年轮＋节疤）＋左上台灯暖晕；主卡/侧栏加投影浮在木上 |
| 状态页 | 重写为市长案头：任职 / **两把尺**（合规·效率阈值着色）/ **常委会权力地形**（五席信任冷热）/ 身心 / 资源 / 同批；去掉科员试用期与恋爱字段 |
| 职业页 | 七品→六品轨道文案（届中/届终＋巡视干净＋省里推荐）；本届年限、帝国考试、政治学院状态 |
| 卷宗眉 | 紧凑一行：形态标签＋来文＋文号＋时限（逾期标红语）＋页码＋核对剩余/已查出；去掉与顶栏重复的两把尺 |
| 主菜单 | 副题改「七品·大同市市长 · 卷宗签批 · 2026—2036」；红头默认「大同市人民政府」 |

文档：README 当前状态改写为 Phase 5 形态；本文件升为版本化 changelog。

### 验证

- `tools/check-compile.sh` → **ALL_OK**
- Unity EditMode → **23 Passed / 0 Failed / 5 Skipped**（跳过=旧时钟内容；v0.2 复跑同结果）

### 工程备忘

- `Flow.Begin` 只 `DossierEngine.ClearRuntime()`，**不要** `Clear()` 注册表
- 事件队列优先于卷宗：`CurrentScene` 先判 queue/currentEvent
- `dayFfable` 必须含 `queue.Count == 0`，否则 FF 空转日期不动

---

## M0 垂直切片（2026-09-10）

**目标**：卷宗签批循环可玩，十年自动回归不炸。

### 已落地

| 模块 | 内容 |
| --- | --- |
| Core | `DossierEngine`（登记/周分配/翻页/核对/签批结算/两把尺）；`Model` 卷宗与 `ActiveDossier`；`saveVersion 5` |
| 开局 | `State.NewGame`：七品市长沈砚舟、41 岁、最快轨三次特批 marks、家庭/债务/常委会关系 |
| 职业 | `Career`：七品→六品竞争条件、双尺年度考核、结局（审查/辞职/六品/平稳/灰色/十年一日）＋十年回响 |
| NPC | 市级权力地形：岑伯衡/韩清/沈砚/邵志远/周谨/许飞/老康/林晚/方启年/任慎 |
| 序章 | 三幕：省城谈话 → 第一次常委会 → 第一份卷宗（`ContentPrologueMayor`） |
| 卷宗 | 教学 3（数字勾稽/程序瑕疵/人情土地）＋高光 3（对上报告/AI 试点/加装电梯）＋池 3 |
| 引擎钩子 | 周一 `AssignWeek`；事件优先于卷宗；快进静默办结卷宗；`sys_annual_eval`/`sys_ending` |
| UI | 卷宗走红头文件态＋「签批」章；周计划五槽改名签批/调研/会商/关系/家庭；顶栏显示合规/效率/待办件数 |
| 测试 | `DossierEngineTest` 5 项；旧吏轨冒烟改七品断言；`ContentClockRivalTest` 整夹具 Ignore（M1 恢复） |

### 验证

- `tools/check-compile.sh` → **ALL_OK**
- Unity EditMode → **23 Passed / 0 Failed / 5 Skipped**（跳过=旧时钟内容）

### 已知缺口（M0 时点；多数已在 v0.2.x 关闭）

1. ~~办公桌壳未拆 `UiRoot`~~ → v0.2 木纹/灯晕；v0.2.6 DeskSurface 已拆；完整 DossierView/PowerBoard 仍并存 UiRoot
2. ~~第一年内容密度不足~~ → v0.2.5–0.2.6 手写 16＋池 36
3. ~~旧 ContentClocks 未迁移~~ → ContentClocksMayor＋测试 37/37；科员线整体归档
4. 平衡未标定（两把尺扣分与晋升门槛**待作者实机**）

### 工程备忘

- `Flow.Begin` 只 `DossierEngine.ClearRuntime()`，**不要** `Clear()` 注册表
- 事件队列优先于卷宗：`CurrentScene` 先判 queue/currentEvent
- `dayFfable` 必须含 `queue.Count == 0`，否则 FF 空转日期不动
