# Phase 5 · 市长办公桌（Papers Please 压力驱动）

> 规格：[docs/compose/spec/papers-desk-overhaul.md](../docs/compose/spec/papers-desk-overhaul.md)  
> 起点：七品·大同市市长（41 岁，最快合规轨），2026-09  
> **当前版本：STARSTATE v0.2.9 Preview** · EditMode **95/95** · 存档 saveVersion **5** · main `4491a87`

## v0.2.9（2026-09-12）· M2 内容量与 M3 收口

> 规格依据：`docs/compose/spec/papers-desk-overhaul.md` S2.13（M2：DossierGenerator 上量＋氛围 LLM＋旧链迁移；M3：六品晋升与全部结局＋平衡＋Windows 包）。

| 轴 | 内容 |
| --- | --- |
| **模板上量 14 → 34** | 模板定义从引擎里拆出（`DossierTemplate.cs` 放结构 + Y1 的 14 件，`DossierTemplatesY2.cs` 新增 20 件），`DossierGenerator` 只管按 (date, seq) 纯确定填充。新雷型：比例超限、关联方评估、程序倒置、口径混淆；来文形态铺到 11 种。**模板总表顺序即读档重建顺序**，只能往后追加。 |
| **模板挂链** | 新增 `ContentTemplateChains.cs`：模板件的灰区处置写下的标记（`stat_fudge`/`yq_delete`/`safety_loose`/`budget_split`/`procure_hold`/`petition_paper`/`yibao_soft`）各自有一条延迟回响——省统计局倒查、舆情反弹、同类事故再发、审计抽查、质疑信到省里、上访人赴省。每次“压一压”都会在未来某个月找回来。 |
| **旧链迁移** | 新增 `ContentChainsHome.cs`：① **周转房/家属院与配偶经商**（设计院转企改制 → 每年那张《配偶从业情况》表 → 抽查/回避/请托三条线，带一份晚报的标书封面）；② **医疗资源打招呼**（父亲体检 → 市一院院长安排加急 → 回响是同一院长递来的第三份设备采购请示）。两条链都直通风险账本与程序合规记录。 |
| **氛围与批示 LLM** | `LlmPrompt.AmbienceUser`（当日一句氛围）与 `RemarkUser`（批示涓色）；`LlmGameplay.SanitizeRemark/SanitizeAmbience` 严格回退（去代码块、去引号、去“批语：”前缀、限长）。签批结果页**先落一条确定性批语**（无 AI 也有），有 AI 时异步涓色后重绘。 |
| **M3 风险账本** | 新增 `GameState.riskLedger`（0-100，不可见的历史存疑）+ `Effects.risk`：灰区处置 +2、逾期 +1、漏查重大雷 +2+severity，一个月没添新账回落 1。阀值 40 → 纪委谈话提醒（每年一次，可主动交底降账）；**65 且程序违规 ≥2 → 立案审查**（新结局入口——旧实现里 `underInvestigation` 只能由内容选项置位，事实上不可达）。 |
| **M3 全部结局** | 新增两个结局：**免职待查**（第二次不称职）、**降级调离**（累计三次基本称职）；考核新增不称职档。**分级、不偷袭**：第一次不称职只降级留任（扣合规分 + 警告标记），再犯才结局。 |
| **平衡修正（探针抓到的三个真 bug）** | ① 考核不能拿**终身** `violationCount` 判不称职——第一年几次失误就终身背，十年局在第一年结束；改按年度 `yearIntegrity` 计。② 快进/自动推进的“**合规优先**”启发式会系统性挑中**灰区选项**（它们面子上合规分更高），18 个月攒下 10 条程序违规；改为 `DossierEngine.BestOptionIndex`（先避程序违规与灰区，再比合规分）。③ 年度程序问题阈值按**卷宗量**（≈5 件/周）重新标定。 |
| **平衡探针** | `tools/core-probe.sh`＋`tools/probe/Probe.cs`：四策略（稳健/灰度/照准/摆烂）× 3 seed 跑十年，控制台乱码时自动写 UTF-8。**校准后分布**：稳健→六品副省、灰度→接受审查调查/免职、照准→免职、摆烂→降级调离。 |
| **测试与包** | 新增 `Tests/M2M3Test.cs` 24 条；**95 Passed / 0 Failed / 0 Skipped**；Windows 包重打（`E:/Starstate/builds/Starstate`）。 |

## v0.2.8（2026-09-12）· M1 内容量达成（卷宗 40 件＋两条新链＋NPC 对话扩容）

> 规格依据：`docs/compose/spec/papers-desk-overhaul.md` S2.13（M1 验收：第一年手写卷宗 40＋核心 NPC 对话＋2 条完整链＋十月内无断档）与 S2.10（新链 2、4）。

| 轴 | 内容 |
| --- | --- |
| **卷宗 40 件** | 新增 `ContentDossierY2.cs` 十件（涉险事故初报／对赌补充条款／棚改回迁联名信／年中预算调整／区局一把手调整（含回避）／国企混改框架协议／常务会议题／巡视回头看／季度形势报告／闲置土地处置）；十一种来文形态齐备；`No_Dossier_Week_Is_Ever_Empty_In_First_Year` 断言第一年 52 周不断档。另修正 M0 三件**手写**池样件被误标 `generated=true`（内容量审计因此少计 3 件）。 |
| **安全事故瞒报压力链** | `dz_sc_safety`（企业自报“无伤亡” vs 医院就诊记录 3 人）三条出口（如实上报／按自报办结／派专家复查）→ 三条互斥回响：压力上门（企业＋区里要求统一口径）、省里四个字“知错即改”、作业票补签被查出 6 人。 |
| **招商引资对赌链** | 宽/严两族标记驱动两条完整线：宽线（变更投资强度→二次兑现 58%，可以“调整统计口径”换补足）／严线（第三方核数进场→对账 96%，按条款收回或做成典型）。新卷宗选项**同时写具体标记与族标记**，链才能接上。 |
| **NPC 对话** | 常委会 10 人台词池 4→8 句；新增 `StateLine` **状态感知台词 20 条**（合规低时纪委变冷、效率低时常务副搬进度表、路线不同则主席说法不同、同批先晋则组织部的口径变、压力大则配偶提白头发……）；交谈选项随信任≥25／熟悉≥50 各解锁一个。 |
| **两把尺的长期经济** | 十年长跑探针发现：快进静默办结取队首选项（照准灰区）且**从不翻页核对**→ 合规必跌到 0、两把尺退化成一把、结局被锁死在“灰色着陆”。三项修正：① 快进办结前**模拟翻页核对**；② **办得干净时谨慎处置不再倒扣效率**（效率正增长只能来自敢拍板）；③ 月末小回补（无漏查合规 +1／案头清空无逾期效率 +1）。修正后十年长跑跑出**六品副省／平稳主官／转身离开**多种结局，且六品晋升路径真正走通。 |
| **测试** | 新增 `Tests/ContentVolumeTest.cs` 11 条；新增 `DossierEngine.RegisteredIds()` 供内容审计；**71 Passed / 0 Failed / 0 Skipped**。另搭了**脱离 Unity 的 Core 探针**（Unity 自带 Mono 跑编译产物）用于内容/平衡秒级迭代。 |

## v0.2.7（2026-09-12）· M1 剧情线路（主干分叉＋三条权力/命运链）

> 规格依据：`docs/compose/spec/papers-desk-overhaul.md` S2.10（剧情链迁移与新链）与 S2.13（M1 第一年）。

| 轴 | 内容 |
| --- | --- |
| **主干分叉** | `ch_route_open`（2027-03-10 务虚会）：**路线确立**——产业转型／民生兜底／项目攻坚／向上争取。此前 `st.route` 全程无人赋值（悬空机制），现在由玩家选择写入，并驱动后续链、日常任务池与结局回响。 |
| **四条路线链** | 产业转型（配套用地→本地配套率→承接产业转移示范区）／民生兜底（供暖投诉→老旧小区资金缺口＋专项回响→接诉即办）／项目攻坚（征地苗头→补偿标准→开工缺口）／向上争取（保供指标→进省答辩→专项债落地）；共 13 环，每环 2–3 个真取舍。 |
| **常委会权力链** | `ch_pb_1` 三十亿产业基金被“缓议”（上门讲清／原样重提／委托常务副吹风，三条路后期代价不同）→ `ch_pb_2` 组织部长的暗示 → **`ch_pb_3_clean` / `ch_pb_3_dirty`**：按有无程序违规记录分叉的两场纪委谈话。 |
| **同批竞争链** | 许飞先晋六品 → 他向大同要一份底数（正式报送／先给好看的版本／压一压）→ 六品窗口前夜（只摆实绩／老同志递话／主动退出），与 2031 的人事窗口互相扣。 |
| **企业邀约链** | 2029 老同学来电 → 开出条件：**签＝中途下海结局**（复用 `resigned` 结局「转身离开」）／划掉关系协调条款／拒绝并如实报告。不可逆选项**排在最后**，避免连点误触结局。 |
| **路线专属卷宗** | 4 件手写件（配套产业用地／供暖管网追加／征地补偿方案／省专项要件）：基础处置人人可点，**路线处置用 `whenMark` 锁**，未立路线时锁定并显示解锁条件。 |
| **引擎小改** | ① `Effects` 新增 `compliance`/`efficiency`——**剧情链也能推动两把尺**（此前只有卷宗签批能改），带上下限钳制与克隆；② 写入程序违规记录时同步 `Mark("has_violation")`（mark 与 flag 是两套存储，而 md 事件调度只认 mark——旧写法 `requireNotMarks=["violation_severe"]` 是空转的）；③ 日常任务池的路线偏好改为“六成概率抽路线相关”，不再硬过滤（路线的中文旧标签→Phase 5 键做映射）。 |
| **结局回响** | `Career.AppendMarkEchoes` 新增 10 条：四条路线各一句、示范区牌子、接诉即办、村里的大爷、牛皮纸袋、同批体面等。 |
| **测试** | 新增 `Tests/RoutesTest.cs` 11 条：路线确立写值/路线门控/链环一次性不跨年重发/路线卷宗选项锁定/下海结局可达/拒绝分支/纪委谈话双分叉靠 `has_violation`/剧情可推动两把尺并钳制/四线各自连通到 2029 无断档/事件 id 全部注册。**60 Passed / 0 Failed / 0 Skipped**。 |
| **测试抓到的两个真问题** | ① 我第一版把“接受邀约辞职”放在选项 0，十年冒烟测试（一律选第一项）因此在 2029 就结局——暴露了不可逆选项的排序风险，已改到末位；② `violation_severe` 只写了 flag 没写 mark，导致纪检谈话分叉的两个事件会在同一天双双入队，已修并对齐存储。 |

## v0.2.6-auditfix（2026-09-12）· 全面审计修复

> 全量审计报告与逐条修复记录：[docs/AUDIT-2026-09-12.md](../docs/AUDIT-2026-09-12.md)

| 轴 | 内容 |
| --- | --- |
| **P0 修复** | ① 内容注册从未接线（运行时序章/卷宗/考核/结局全空）→ `GameApp.Init`＋`State.NewGame` 双保险，`RegisterAll` 改自愈可重入；② `sys_personnel` 未注册 → 七品→六品晋升整条死掉 → 注册＋市长层晋升选项；③ 卷宗总量 62 件只够 13 周 → `AssignWeek` 积压/档期/按需生成四段重构 |
| **P1 修复** | ④ 事件与卷宗输入路由错配（点事件被卷宗吃掉）→ 显示/输入共用 `PendingChoiceEvent`/`DossierOnScreen`；⑤ 模板件 id 依赖进程级计数器 → 读档后「继续」死按钮 → `Respawn` 按 id 原样重建；⑥ 存档写入乱序 → 每文件单调序号 + 快照入锁；⑦ 埋雷提示与数据不自洽 → `Template.plant`（sum/overfix/none）＋注册期自检；⑧ 季节档期形同虚设 → `releaseFrom`（优先队列＋随机池两处都卡） |
| **P2 修复** | `CloneEffects` 深拷贝档案记录、`MiniJson` 实例化（去静态游标）、`LlamaServer.starting` 超时失效、`SceneSig` 全内容散列、密钥输入框 Password、科员线残留清理（周评/周末/文号/幽灵 NPC）、`DocNoFor` 稳定哈希、考核去 save-scum、字体候选链判定等 |
| **测试** | 新增 `Tests/RegressionTest.cs` 12 条；**49 Passed / 0 Failed / 0 Skipped**；`check-compile.sh` 增加第 5 段编译 Tests（Core 另编 mscorlib 档，`GameLog` 让 Tests 不依赖 UnityEngine） |
| **验证** | check-compile ALL_OK（5 段）＋ EditMode 49/49；`game/` 与 `game-src/` 非 .meta 差异为空 |
| **未修（有意）** | 漏雷惩罚对所有处置一视同仁（平衡取向）；十年长线内容多样性仍待 M1；`UiRoot` 绘制细节未重构；LLM 配置仍明文存 PlayerPrefs（仅做不回显） |

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
