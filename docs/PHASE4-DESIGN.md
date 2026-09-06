# Phase 4 设计文档 — 从"事件轮播"到"活的人生"

> 对应历程：[changelog/PHASE-4.md](../changelog/PHASE-4.md)。本文件沉淀升级方案的**设计逻辑**（为什么这么改）与**系统规格**（改成什么样），供后续阶段扩展时对齐。

## 1. 问题定义（升级前的量化诊断）

| 病灶 | 量化事实 | 玩家感受 |
| --- | --- | --- |
| 内容倒挂 | 一周目 3500-4000 屏，独特内容约 110 屏（3%）；46 条日常文案×50-60 次、28 任务模板共用 1 套选项×1250 次、4 段周末文本×130 次、中秋 9 年一字不差 | "剧情重复，像是流水线" |
| 随机池枯竭 | 19 个 randomP 事件触发即永久删除，2028 年后八年无新随机内容 | "越玩越空" |
| 选择不可积累 | Effects 无延迟字段；16 个 flag 只写不读；NPC 记忆只展示不消费；选项无关系门槛 | "选什么都不重要" |
| 互动退化 | `_generic_day` 单选项屏占工作日一半；晋升纯数值闸门无竞争者 | "可玩性极差" |
| 表现无身份 | 零美术资产、单一圆角矩形词汇、单一字体、动效只有淡入 | "风格极其普通" |

## 2. 他山之石（研究与取材）

- **中国式家长**：日程槽位让玩家成为"一周的作者"→ 周计划；期望/面子＝对手与比较 → 竞争者。
- **Citizen Sleeper**：行动点稀缺 + **时钟**（机会与威胁双面倒计时）→ 精力分配 + Clocks 系统。
- **Suzerain**：决策写入档案、经官方渠道回响 → marks/回响 + 结局个人史。
- **Papers, Please**：把重复事务做成主题、档案拟物 UI 与题材强绑定 → 档案公文美学。
- **BitLife**：俏皮文本 + 随机性让循环文本不腻 → 文案扩容 + 官场腔调。
- **Crusader Kings 3 / 太吾绘卷**：NPC 有目标、有记忆、自己会动 → NpcTick + NPC 主动事件。
- **Disco Elysium**：锁定的选项显示解锁条件 → 叙事门槛"锁定可见"。
- **I Was a Teenage Exocolonist / Sir Brante**：选择跨年回响、章节制人生 → EchoScheduler + md 主题。

赛道：官场职业模拟近无现代竞品（最接近为古董桌游"升官图"），无对标包袱，拼执行。

## 3. 系统规格

### 3.1 叙事引擎（Core）
- **marks**（`st.marks`，持久化）：叙事记号。写入：`Effects.setMarks/clearMarks`；读取：`When.requireMarks/requireNotMarks`（事件级）、`OptionWhen.mark/notMark`（选项级）、结局个人史。
- **回响（Echo）**：`Effects.echoes = [{eventId, afterDays}]` → `Flow.ScheduleEcho` → 到期 CollectDue 入队。回响事件 `when=null`；带 flag/marks 门槛的回响未达标即**耗散**（一次性消耗，不滞留）。
- **加权池**：`randomP>0` 即入池；每日 0.45 概率按 `weight` 加权抽一件。`maxFires`（默认 1=一生一次；>1 可复现，fired 表记 `id#n`）、`cooldownDays`（poolLog 记 `id@iso`）、`ambition` 门槛。**池永不枯竭**。
- **选项双轨门槛**：结构性（grade/route/flag/notFlag/年限/考核）不满足→隐藏（旧行为）；叙事性（mark/notMark/relNpc+minFamiliar/minTrust）不满足→**锁定可见**，Scene.optionLocks 平行数组给原因，UI 置灰禁点。
- **时钟（Clocks）**：`ClockState{id,label,value,max,kind,onFullEventId,dailyRate}`；`Effects.clockOps` 建立/推进/移除；满格触发事件（每时钟一次，`clockfull_<id>` 去重）；DailyTick 随 CollectDue 自然推进。
- **NPC 周 tick**（NpcTick，周一 ResetWeek 调用）：>28 天无互动熟悉度 -1；10% 触发 `npc_initiative`（动态构建：对口径/递消息/约饭，按 familiar≥8 选人）。

### 3.2 核心循环
- **周计划**：100 点精力 → 岗位/学习/人际/家庭/休整（±10 步进，总量≤100，UI 编辑器 + "开始这一周"）。
  - 结算（PlanGrowth，周五例会与快进静默周共用）：exec+work/20、admin+work/40、professional+study/10、comm+social/15、political+social/30、stress+work/15+study/20+social/30−rest/4、morale+family/10、人际槽随机 NPC familiar+。
  - 任务评级加成：`TaskCheck` 中 `+plan.work×0.1`。
  - 日常耗损：Drift 按 rest 档降耗（rest≥30 压力+1/天）。
  - 主导槽派生 `week.focus` 兼容日常文案池（family 并入 rest）。
- **任务五骨架**（TaskGenerator.BuildByKind，按 55/15/12/10/8 抽签）：例行（认真/交差）、限时交办（赶工/卡点/申请延期−科长评价）、协作（独扛/拉同批/请赵姐）、风险（留痕/口径变通→程序合规标记/上报请示）、露脸（接住/让功）。模板只提供主题（title/para/note/main），骨架提供决策结构。
- **快进新语义**：AutoPlan 沿用玩家上周计划（energy<45 或 stress>70 时强制休整倾斜 50）；排期/回响/时钟到点/危机必停；结构阶段代选默认项。
- **竞争者**：`st.rival`（默认许飞），每周 +1~2（0-100）；人事窗口文案引用；P4.4 起与同批晋升叙事咬合。

### 3.3 档案公文美学（Ui，零美术资产）
| 母题 | 实现 |
| --- | --- |
| 红头文件 | 事件场景（入职后）文头：局名朱红居中＋文号（`DocNoFor`：事件 id 稳定哈希→"长发改〔年〕第 N 号"）＋双朱线；文件标题居中加粗；正文仿宋首行缩进"　　" |
| 朱砂印章 | RingSprite（程序圆环）＋印文（已阅/核毕/归档），-12° 倾斜，1.8→1 OutBack 砸落＋程序音效 |
| 报纸剪报 | 新闻卡加"长 安 日 报"报头（分类色）＋TornSprite 撕边底缘 |
| 干部履历卡 | NPC 卡加姓氏白文印头像（BarSprite 染朱红＋姓）＋熟悉度关系条 |
| 台历 | 顶栏左端今日大字（朱红加粗）＋竖朱线分隔 |
| 纸纹 | PaperTex：128²颗粒噪声＋随机纤维，Tiled 平铺页面底 |
| 双字体 | MakeDocFont：FangSong→仿宋→GB2312→KaiTi→楷体→雅黑回退；公文正文/剪报/印文用公文字体，界面 chrome 用雅黑 |
| 音效 | SoundFx：印章（52Hz 冲击+噪声）、翻纸（低通噪声+包络抖动）、轻点（880Hz 衰减）；设置页开关 |

### 3.4 内容架构（P4.4）
- **内容链模式**：`md+requireMarks`（年度循环门）或 `date+flag`（一次性门）触发 → 选项 `setMarks` + `echoes` → 分支回响事件（when=null 或带门槛）→ 再回响。回响间隔 25-1825 天。
- **死 flag 兑现清单**（16→0）：origin_×3（10-20 科长深谈）、uni_×3（3-16 专业首秀）、prologue_×3（11-10，村庄线含老康二次回响）、ambitious/defensive_reveal（11-03）、koujing_lesson（12-08）、dream_ambition（9-30 循环＋1825 天回响）、ai_law_interest（AI 专班三段链）、topic_housing（房子三择＋420 天回响）、family_midautumn（父亲体检）、resign_thought（挖角→**中途辞职结局**）、flv_want_marathon（马拉松）、proactive/passive_wait（巡视回头看）、punished（翻篇）、exam_ready（准考证回响）。
- **月度十二节律**（ContentMonthly）：1 总结季 / 2 开工 / 3 规划月 / 4 调研季 / 5 竞赛 / 6 双过半 / 7 汛期 / 8 年假 / 9 人事季 / 10 预算季 / 11 审计进点 / 12 收官；md 循环 fromYear 2027。
- **体量**：日常文案 46→106、任务模板 28→50、新闻 69→109、剧情链事件 30+。
- **志向**：序章 p_ambition 四选 → `st.ambition` + mark；池事件门槛 + 结局引用。
- **结局个人史**：Career.AppendMarkEchoes 按 marks 装配"十年回响"段。
- **AI 周评**：LlmPrompt.WeekReviewUser（引用周计划/评级/本周事件，【纯文本】协议）→ GameApp.RequestWeeklyReview → ui.AppendBodyPara；失败静默。

## 4. 兼容与测试
- 存档 **saveVersion 4**；旧档读入由 GameApp 引导重开。
- EditMode 17 项：原有 12 ＋ P4 引擎 5（Echo_Fires_When_Due / Pool_Respects_MaxFires_And_Repeats / Option_Lock_Gates_On_Mark_And_Relation / Clock_Full_Triggers_Event / NpcTick_Decays_Stale_Relations）。
- 十年自动通关测试在全部新内容启用下保持通过（快进新契约：计划沿用＋节点必停）。

## 5. 已知取舍与后续候选
- 时钟系统已在引擎层完备，但常驻剧情时钟（巡视倒计时/考核冲刺仪表）留待 Phase 5 接内容（引擎测试覆盖满格触发）。
- 竞争者目前是"势头仪表＋人事季文案"，未做独立晋升竞争事件（rival 里程碑链为候选）。
- NPC 头像为印章式占位；若未来引入美术资产，履历卡结构已预留槽位。
- AI 家信/微信、AI 剧情链润色为 LLM 层候选（周评已落地，复用同一纯文本协议即可）。
