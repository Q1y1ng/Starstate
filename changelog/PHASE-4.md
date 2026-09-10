# Phase 4 · 从"事件轮播"到"活的人生"（2026-09-06）— 历程记录

> 起因：玩家实测反馈——**剧情重复、可玩性差、像流水线；风格普通、没有玩下去的兴趣**。
> 应对：对现状做量化审计＋研究同品类优秀作品后，制定并实施五期全面升级方案。
> 设计文档：[docs/PHASE4-DESIGN.md](../docs/PHASE4-DESIGN.md)。状态：✅ 完成（EditMode 17/17 全绿）。

---

## 2026-09-06 · 诊断（升级方案的依据）

对代码做只读审计，把"流水线感"量化为五个可执行的病灶：

1. **内容倒挂**：一周目约 3500-4000 屏，独特内容约 110 屏（**3%**）；其余 >90% 在轮播约 100 条循环文本（46 条日常文案×50-60 次、28 个任务模板×40+ 次共用同一对选项、4 段周末文本×130 次、中秋 9 年一字不差）。
2. **随机池枯竭**：19 个 randomP 事件触发即永久删除，2028 年起后八年再无新随机内容。
3. **选择不可积累**：Effects 无延迟字段；**16 个 flag 只写不读**（序章出身/大学/实践年/性格揭示全是死代码）；NPC 记忆只展示不消费；选项门槛没有关系字段。
4. **互动退化成点击**：`_generic_day` 单选项"继续"屏占工作日约一半；晋升是纯数值闸门，无竞争者。
5. **表现层无身份**：零美术资产、形状词汇只有圆角矩形、单一微软雅黑、无纹理/图标/印章，动效只有淡入+轻缩放。

同品类研究结论（详见设计文档"他山之石"一节）：中国式家长（日程槽位）、Citizen Sleeper（时钟=机会/威胁双面倒计时）、Suzerain（决策档案与官方渠道回响）、Papers Please（档案拟物 UI＝题材强绑定）、BitLife（俏皮文本+随机性）、Crusader Kings 3/太吾绘卷（NPC 自己会动）、Disco Elysium（锁定选项显示解锁条件）。赛道本身近无现代竞品。

作者批复：美术方向＝**档案公文美学**；核心循环＝**完整周计划**；范围＝**五期全做**。

## 2026-09-06 · 五期实施（当日完成）

**P4.1 引擎内核（存档 v4）**
- `GameState` 新增：`marks`（叙事标记）、`echoQueue`（延迟回响）、`clocks`（时钟仪表）、`plan`（周计划）、`rival`（竞争者）、`ambition`（志向）、`poolLog`（池冷却流水）；`FireCount/MarkFireAdditional` 支持池事件可复现计数。
- `When` 新增 `weight/maxFires/cooldownDays/requireMarks/requireNotMarks/ambition`；`Effects` 新增 `setMarks/clearMarks/echoes/clockOps`；`OptionWhen` 新增 `mark/notMark/relNpc/minFamiliar/minTrust`（叙事门槛=锁定可见给原因，结构性门槛=隐藏，向后兼容）。
- 随机池改**加权单抽**（0.45/工作日），事件不再"触发即删"；回响队列随 CollectDue 清算，带 flag/marks 门槛的回响未达标即耗散。
- 新文件 `Core/Systems.cs`：`Clocks`（建立/推进/满格触发/日推进）与 `NpcTick`（久未互动淡忘＋偶发主动找你）。
- 测试 +5：回响到点、池不枯竭（maxFires>1 重复触发）、选项锁定门、时钟满格触发、NPC 淡忘。

**P4.2 循环重塑**
- **周计划**：100 点精力五槽位（岗位/学习/人际/家庭/休整），Flow.SetPlan/AdjustPlan/AutoPlan；计划质量→任务评级加成（plan.work×0.1）、成长与压力结算（PlanGrowth，周五例会与快进静默周共用口径）、日常耗损（Drift 按 rest 档降耗）。
- **任务五骨架**：TaskGenerator 按 55/15/12/10/8 抽签 例行/限时交办/协作/有程序风险/当众露脸，各有独立选项结构与代价回报（变通口径=程序合规标记是"明码标价"的风险）。
- **快进重定义**：沿用玩家上周计划自动推进，疲惫强制休整倾斜；排期/回响/时钟到点必停。
- **上下文化**：周末插入"这一周"真实事件回顾；月结附"本月大事记"；人事窗口提及竞争者势头（rival.progress 每周+1~2）。
- **NPC 主动事件** `npc_initiative`（动态构建：对口径/递消息/约饭，按好感选人）。
- UI：周计划编辑器（五槽位 ±/进度条/总量提示）+ GameApp.OnPlanAdjusted 接线。

**P4.3 档案公文美学（零美术资产，全部程序生成）**
- **红头文件**：事件场景切换为公文态——文头"长安市发展和改革局"朱红居中＋文号（由事件 id 稳定派生）＋双朱线，文件标题居中、正文仿宋首行缩进；序章（入职前）保持便签态。
- **朱砂印章**：结果页"已阅"/月结"核毕"/结局"归档"——程序圆环章＋印文，1.8→1 OutBack 砸落＋闷响。
- **报纸剪报**：新闻卡加"长 安 日 报"报头＋程序撕边底缘。
- **干部履历卡**：人物页 NPC 加姓氏白文印头像＋熟悉度关系条。
- **台历顶栏**：左端今日大字＋朱线分隔。
- **双字体**：OS 仿宋/楷体（公文正文/剪报/印文）→雅黑（界面 chrome），带回退链（FangSong→KaiTi→雅黑）。
- **纸纹**：128×128 程序颗粒+纤维纹理平铺页面底。
- **音效**：程序 PCM 合成（印章闷响/翻纸/轻点），设置页四钮行加"音效"开关。
- 锁定选项置灰禁点并显示解锁原因。

**P4.4 内容大填充**
- **死 flag 全部兑现**（16→0）：出身三选→10-20 与科长深谈三分支；大学三选→3-16 专业首秀；实践年三选→11-10（含"老康来信"→420 天后"老康进城"回响）；ambitious/defensive_reveal→11-03 压担子/敲打；koujing_lesson→12-08 便签传帮带；dream_ambition→2028-09-30 初心重温→1825 天后"十年过半对账"；ai_law_interest→2027 施行日条款解读→专班借调→结项表彰（二段回响）；topic_housing→2028 房子三择→420 天后三分支回响；family_midautumn→2029 父亲体检→陪诊/寄钱两结局；resign_thought→2029 挖角→二次橄榄枝（**中途辞职结局**入口）；marathon→2028 马拉松；proactive/passive_wait（监察表态）→2030 巡视"回头看"两分支；punished→次日"翻篇"；exam_ready→报名后 60 天"准考证与冲刺"回响。
- **数据造假案**（监察主线三幕）：2027-11 台账三择（拒绝留痕/照办/上报）→90 天后各自回响→2028-05 审计抽查三分支结局，与 violation/巡视系统自然咬合。
- **月度主题 12 节律**：总结季/开工/规划月/调研季/竞赛/双过半/汛期/年假/人事季/预算季/审计进点/收官，md 循环错峰（fromYear 2027），一年 12 个主题事件。
- **年度变体**：年度考核开场白按年轮换（4 套）；系统事件与动态构建逐步差异化。
- **体量**：日常文案池 46→117（四池扩容：work30/study31/social31/rest25）、任务模板 28→50（覆盖节能/信用/专项债/飞地/双碳等业务域）、新闻 69→109（含 2036 城市总规公示等收束期条目）。
- **志向系统**：序章"你为什么来"四选（做事/晋升/安稳/搞钱）→写入 st.ambition＋mark，供池事件门槛（When.ambition）与结局引用。
- **结局个人史**：Career.AppendMarkEchoes 按 marks 装配"十年回响"段（台账的影子/专班的名字/房子的底气/父亲的秋天/老康的核桃……）。
- **AI 周评**：周五点评结算后，本地/远程模型生成一句引用本周真实计划与事件的科长评语，追加到结果页（失败静默，LlmPrompt.WeekReviewUser＋【纯文本】协议）。
- 新手引导重写为四页：欢迎与志向→周计划与锁定选项→选择的回声与印章→推进与 AI。

**P4.5 收口**
- 全量回归 17/17 全绿（含修复：MarksMet 空 when 防护、测试脚本两处自身缺陷）。
- 本文件与 docs/PHASE4-DESIGN.md、README、AGENTS 同步更新。

## 验证状态

- 独立编译四程序集零错误（tools/check-compile.sh）。
- Unity batchmode EditMode **17/17 通过**：序章九月冒烟、检定有界、十年全自动通关（晋升/考核/结局全链路）、AI 协议、成长回归、P4 引擎五项（回响/池不枯竭/锁定/时钟/淡忘）。

## 事故与更正记录（按纪律留痕）

- **测试脚本自身缺陷（2 处）**：Echo 测试源事件漏写触发条件；Pool 测试循环条件用 FireCount 提前退出导致第三次触发未呈现。均已修正——非引擎缺陷。
- **全角引号污染**：一次批量替换把 C# 字符串定界符写成全角引号（ContentChains 两事件），独立编译门禁当场拦截，重写该块修复。
- **MarksMet 空引用**：回响事件 `when=null` 时 MarksMet 未做防护导致十年回归 NRE；补 `if (w == null) return true;`。
- **语义纠正**：`proactive_confession/passive_wait` 实为监察线表态（非恋爱线），已按正确语义接"巡视回头看"回响，未按误判实施。
- **挂起批处理实例**：两次按 `-batchmode` 命令行特征清理自己启动的 Unity（PID 14784/14424），未触碰用户实例。

## 补丁十 · 本地模型启动参数标定化（2026-09-06）

**起因**：作者指出 llama-server 启动参数未开 KV 量化、未固定 ngl，要求按 `D:\AI` 的标定报告调整。

**依据**（D:\AI\benchmark_2026-08-31.md / benchmark_报告_2026-08-31.md / quant_IQ4vsQ3_测试报告.md）：
- 9B 模型本机标定命令行：`-ngl 28 -c 65536 -ctk/ctv q4_0 -ub 128 -fa on -rea off -t 16`（`--cpu-range 0-19` 为 35B 生产参数，9B 行未带；经单实例实测对本模型无副作用，保留）；
- KV 双 q4_0 量化：缓存显存减半，是 6GB 显存下 ngl 28 稳定放下的前提；
- `-t 16`：20 逻辑核下优于 t20（+2~3%，quant 报告第九节）。

**改动**（Ui/LlamaServer.cs）：降级阶梯从 `ngl 99/28/16/0`（无 KV 量化）改为**标定配置优先**——
1. 标定档：`-ngl 28 -ctk q4_0 -ctv q4_0 -ub 128 -fa on -t 16 --cpu-range 0-19`（ctx 全量 65536）；
2. 降档：`-ngl 16 -ctk/ctv q8_0`（ctx 32K）；
3. 纯 CPU：`-ngl 0 -ctk/ctv q8_0`（ctx 16K）。
`--jinja --reasoning off` 与 LoRA 挂载保持不变。

**实测验证**（单实例、测试口 8899、ctx 4096、Heretic-Q4_K_M＋RP-LoRA）：
- 标定档（KV q4_0，无 fa）：prompt 85.0 t/s / gen 18.8 t/s / 3.7s 完答；
- 标定档（含 fa on）：prompt 66.1 t/s / gen 19.2 t/s / 2.2s 完答；
- 均远优于旧参数实测的 12.8-13 t/s 生成速度（约 +50%），且 64K 上下文一次性加载成功。

**事故记录**：验证过程中两次出现双实例并跑（Git Bash 下 `taskkill //PID $!` 杀不到 Windows 真实 PID，前一实例未被清掉；叠加作者自己的常驻模型），导致内存打满、机器卡顿。已即时全部清理（按进程名），并确立纪律：**任何本地模型测试前先按进程名清点确认零实例、测完按名清理复核；Git Bash 下 `$!` 不是 Windows PID，不可作为 taskkill 依据**。初步误判的"-fa on 中毒"经单实例对照实验排除，实为内存争抢。

## 补丁十一 · 全面审计与深度优化（2026-09-06）

**审计范围**：热路径性能、内存增长点、逻辑边角、资源占用（纹理/音频/字体/存档/LLM 进程）。

**发现与修复**：

| # | 发现 | 影响 | 修复 |
| --- | --- | --- | --- |
| 1 | **每次点击同步写盘**：`File.WriteAllText(ToJson(st, true))` 在主线程阻塞（pretty JSON 全量序列化+同步 IO） | 每次交互一次卡顿源；十年长档文件变大 | `Save()` 改为**主线程紧凑序列化 → 线程池落盘**；写临时文件→长度校验→`File.Replace` 原子替换；加写入锁防并发；`OnDestroy` 同步兜底保证最后一次状态落盘 |
| 2 | **UI 全树重建无去重**：`RenderMain` 对同一场景重复重建正文/选项（锁定选项点击、外部触发的 RenderAll），每段 Text+CanvasGroup+协程＋两次 ForceUpdateCanvases | 闪烁与 GC 尖峰 | 引入**场景签名**（kind+标题+段数+选项数+前两段+首选项+文号），同签名直接跳过重建；`Build()` 时重置签名 |
| 3 | **周计划每次 ± 全树重建**：PlanAdjusted 调 RenderAll，正文/选项/动画全部重跑 | 周计划操作卡顿、闪烁 | 编辑器缓存五组数值 Text/进度条 Image/合计行，`UpdatePlanEditor` **局部刷新**（含合计行配色），GameApp 不再 RenderAll |
| 4 | **NPC 记忆无上限**：Llm.cs:472 只在提示词路径裁剪到 6，真实列表随十年增长 | 存档体积与遍历成本 | 裁剪移到源头 `Npcs.Mod`（保留最近 8 条） |
| 5 | **游戏日志无上限**：st.log 十年累积（周回顾/月大事记只需近期条目） | 存档体积 | `AddLog` 超 600 条裁掉最早 100 条 |
| 6 | **交谈在途请求不取消**：关闭交谈弹层后请求继续跑满 60s 超时 | 无谓占用本地模型推理队列 | `LlmClient.Chat` 增加可选取消谓词（会话号失效即 `web.Abort()`），交谈生成接入 |
| 7 | **字体实例泄漏**：字号切换（SetFontScale）每次重建 UI 都 `CreateDynamicFontFromOSFont` 新建两个 Font | 重复切换累积内存 | GameBootstrap 静态缓存 UI/公文两枚 Font |
| 8 | **llama-server 进程句柄不释放**：Kill 只 Kill 不 Dispose | 每次会话一个句柄泄漏 | Kill 中补 `proc.Dispose()` |
| 9 | **死代码**：`ContentRegistry.WeekPlanOptions`（周计划改槽位制后无引用） | 干扰接手者 | 删除 |
| 10 | **存档版本降级静默**：旧档读入直接无提示开新局，玩家不知进度为何消失 | UX | `UiRoot.SetMenuHint`：主菜单留痕"检测到旧版本存档（格式已升级到 v4）…" |

**资源占用审计结论**（无需改动项）：程序纹理共约 0.1MB（纸纹 64KB＋印章/撕边各数 KB）；程序音频三段共约 40KB；UI/公文两枚动态字体由 Unity 托管图集（仿宋仅用于公文体）；`fired` 列表十年约 2600 条字符串（去重必需，~50KB）；`poolLog` 已有 500 条上限；任务/材料档案按设计**有意**全量保留（十年工作档案是内容本体）。llama-server 显存/内存占用已由标定参数约束（见补丁十）。

**验证**：独立编译零错误；EditMode 17/17 全绿（分两批验证：批一=存档/日志/记忆/签名/局部刷新，批二=LLM 取消/字体缓存/句柄释放）。

**接手者注意**（已同步 docs/HANDOFF.md）：新增持久化写盘一律走 `WriteSaveAtomic`；手动改 UI 场景重建逻辑时记得 `lastSceneSig` 的重置时机（Build 内已重置）。

## 版本定名 · v0.1 Preview（2026-09-07）

- 自 Phase 4 收口＋补丁十/十一起，项目进入版本化命名：**STARSTATE v0.1 Preview**（主菜单右下角版本号同步更新）。
- 版本基线评审报告：[docs/PROJECT-REVIEW.md](../docs/PROJECT-REVIEW.md)——十维度评分（综合 7.2/10）、系统完成度矩阵、风险清单与 v0.2 路线。最优先事项：git 版本控制与独立可执行打包。

## 补丁十二 · LLM 启动降载与防双开（2026-09-10）

**起因**：作者反馈「LLM 层每次游戏启动资源占用体感异常大」，要求检查启动逻辑。

**排查结论**：不是逻辑写崩，而是**默认配置过重＋两处护栏缺口**。

1. **主因——启动即全量拉模型**：`GameApp.Init` 在 `enabled && local && autoStart` 时立刻 `KickServer`；默认三者皆真，且 `ctx=65536`。llama-server 启动时按满 ctx 预留 KV，叠 5.6GB 权重＋LoRA＋Unity，16GB 机器易被打满。编辑器每次进 Play（OnDestroy→Kill→再 Play）等于反复全量加载。
2. **参数与真实用法不匹配**：NPC 交谈/微事件/周评均为单次短 prompt（max_tokens≤460、无跨轮历史），64K 纯属浪费。补丁十标定命令行写的是 `-c 65536`，但**游戏内复验实际用的是 ctx 4096**——64K 未按真实启动路径在 6GB 卡压测。
3. **`starting` 竞态**：置位在 `Process.Start` **之后**；自动拉起与「测试连接」几乎同时进入时可能双开（正是补丁十记录的内存打满事故形态）。并发等待还挂在可能为空/过期的 `proc` 上。
4. **设置页未暴露 `autoStart` / `ctx`**：用户无法在 UI 关掉开机自启或把上下文降到够用档。

**改动**：

| 项 | 文件 | 内容 |
| --- | --- | --- |
| 默认懒加载 | `Core/Llm.cs` | `autoStart=false`——不启动即加载；首次交谈/测试连接时现场唤醒（`WakeThenTalk` / `TestFlow` 路径已存在） |
| 默认上下文 | `Core/Llm.cs` | `ctx` 65536→**16384**（单次短 prompt 足够；设置可改回） |
| 防双开 | `Ui/LlamaServer.cs` | `starting` 在 `Process.Start` **前**置位；循环内旧的 `if (starting)` 改为循环前统一走 `WaitForInFlightStart`（健康即成功，starting 结束仍未就绪则失败）；ctx 做 1024–65536 钳制 |
| 设置页 | `Ui/UiRoot.cs` | 新增「启动时自动加载」开关＋「上下文 tokens」输入；卡片 680→720；`MakeSettingsInput` 支持行内半宽 |
| 配置键 | `Ui/GameApp.cs` | `starstate_llm_cfg2`→**v3**（新默认对旧存档生效；自定义路径需在设置里重填） |

**行为**：启动不再自动拉 llama-server；NPC 交谈/测试连接会现场唤醒；也可在设置打开自动加载。微事件/周评仍仅在 `llmReady` 时触发（不主动唤醒）——符合「AI 坏了照常玩」。

**验证**：独立编译 `ALL_OK`；同步 `game/`（未动 `.meta`）；EditMode **17/17 Passed**。

**接手者注意**：改 LLM 默认值时记得升 `LlmKey` 后缀，否则旧 PlayerPrefs 会盖住新默认；`starting` 必须保持「Start 前置位、所有出口复位」。

## 补丁十三 · P1/P2 内容接线与工程打包（2026-09-10）

**范围**（作者指定：先做 P1 与 P2，测通后再 git + Windows build）。

### P1-3 常驻时钟接内容（`Core/ContentClocks.cs`）

| 时钟 | 类型 | 启动 | 满格 |
| --- | --- | --- | --- |
| `clk_eval` 考核冲刺 | opportunity | 每年 11-01 | `clk_eval_full` 提前清零台账（admin/political/ren 评价） |
| `clk_xuncha` 巡视组 | threat | 2028-09-10 | 巡视谈话（干净/谨慎两分支） |
| `clk_xuncha_b` 回头看 | threat | 2031-10-08 | 反馈会销号 |
| `clk_xuncha_c` 专项巡察 | threat | 2034-09-18 | 核实环节 |
| `clk_proj` 专项初稿 | opportunity | 随机池（四周作战图） | 杀青交稿 |

推进路径：事件 `clockOps` 建立/增减；随机池「调阅单 / 夜里加一班 / 市府办催了一次」按 marks 门槛可重复。威胁时钟负 delta 缓解、机会时钟正 delta 加速。

### P1-4 竞争者里程碑链（`Core/ContentRival.cs`）

许飞主线五节点（2027 报告 / 2029 饭局 / 2031 调重点项目办 / 2033 三等功 / 2034 转官考试）＋苏晴两节点（借调、提副科）＋何斌两节点（升副科、婚礼）＋人事季前「同批风声」循环与「差距感」池事件。选项写 marks 与关系记忆；侧栏「同批」卡片显示 `rival.progress` 与年度风声摘要。`RivalState.stage` 预留里程碑阶段。

### P1-5 多存档槽（`Ui/GameApp.cs` + `Ui/UiRoot.cs`）

- 路径：`starstate_save.json`=自动档（slot 0），`starstate_save_s1..3.json`=手动槽。
- 主菜单按槽列出「继续」（标签含日期/姓名/职级）；新局优先写入空手动槽，全满覆盖自动档。
- 手动槽每次 Save **镜像写自动档**（继续总能回到最新进度）。
- 设置页新增「快照槽1/2/3」；重置只清当前槽。

### P1-6 平衡探针（`Tests/BalanceSmokeTest.cs`）

十年自动通关后检查：结局可达、任务/考核下限、高压/空精力日占比 <45%、积蓄非负、能力和成长。Debug.Log 输出关键指标。

### P2 实现

| 项 | 内容 |
| --- | --- |
| LLM 流式 | `LlmClient.ChatStream`（SSE + `DownloadHandlerScript`）；服务端忽略 stream 时自动回退整包解析；交谈加载中显示「已 N 字」 |
| AI 家信/微信 | `LlmPrompt.FamilyLetterUser` / `WeChatUser`（【纯文本】）；每月上旬周末生成家信进日志（结果页可追加正文）；35% 概率同事微信进日志 |
| 侧栏卡片化 | 状态页改公文卡片：基本信息/能力/身心/资源/同批/时钟 |
| 内容审计脚本 | `tools/content-audit.py` → `tmpbuild/content-audit.json`（独特长字符串占比 0.973） |

### 验证与产物

- 独立编译 `ALL_OK`；EditMode **22/22 Passed**（原 17 ＋ ContentClockRivalTest×4 ＋ BalanceSmokeTest×1）。
- LLM 实测：本地 llama-server 单实例（ctx=16K）非流式 + SSE 流式均连通（见下文补记）。
- Windows 包：`Editor/BuildPlayer.BuildWindows`（菜单 STARSTATE/构建 Windows 包；batchmode `-executeMethod` 可用）。
- Git：本补丁作为规范提交落库（含 `.gitignore` 覆盖 Library/tmpbuild）。

**接手者注意**：同步 `game-src`→`game` 时用 robocopy/cp 到 `Scripts/` 目录内容，**不要**把 `Scripts` 整夹拷成 `Scripts/Scripts`（会重复 asmdef 直接炸编译）；新事件 id 全局唯一；时钟 `onFull` 事件必须 `when=null`。
