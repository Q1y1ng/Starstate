# STARSTATE — Phase 2 技术设计（M0 基础构建 · Unity）

> 版本：v0.2（2026-09-06，引擎定为 Unity 6 LTS 后重写）
> 蓝图来源：[FOUNDATION_REPORT.md](FOUNDATION_REPORT.md) §4 核心循环、§5 核心数据、§7 第一版原型
> 引擎决策：**Unity 6 LTS + C#**（作者确认；否决 Web 与 UE5，理由见 changelog/PHASE-2.md）
> 本期交付：**序章＋2026年9月的可玩纵向切片**

---

## 1. 工程策略

| 项 | 决定 |
| --- | --- |
| 引擎 | **Unity 2022.3.57f1c2**（本机现有，`D:\pro\unity\Editor\Unity.exe`，M0 直接使用）；**升级路径：Unity 6 LTS**（作者定案，工程就绪后随时升级） |
| 工程位置 | `E:\Starstate\game`（手工创建最小工程：ProjectVersion＋manifest＋Assets，Unity 首次打开自动补全） |
| 代码暂存 | `E:\Starstate\game-src\Assets\`（镜像 Assets 结构；已复制进工程） |
| 无头验证 | `Unity.exe -batchmode -nographics -projectPath … -runTests -testPlatform EditMode` |

## 2. 目录结构（暂存区 = 最终 Assets 结构）

```
game-src/Assets/Scripts/
├─ Starstate.Core.asmdef        # noEngineReferences=true（纯C#）
├─ Core/
│  ├─ GameClock.cs              # 日历：日期/星期/节假日/周与月边界（System.DateTime）
│  ├─ Model.cs                  # GameState/PlayerState/记录POCO/Effects/GameEvent/Scene
│  ├─ Npcs.cs                   # 人物定义表、关系（熟悉/信任/评价＋记忆）
│  ├─ ContentRegistry.cs        # 内容注册入口＋周计划/日常/月末文案池
│  ├─ ContentPrologue.cs        # 序章事件（实践年三选一→报到）
│  ├─ ContentMonth1.cs          # 2026年9月：16个脚本事件＋4个随机事件
│  └─ Flow.cs                   # 流程状态机：事件调度/效果结算/任务检定/周月节奏
├─ Starstate.Ui.asmdef          # 引用 Core + UnityEngine.UI
├─ Ui/
│  ├─ GameBootstrap.cs          # 场景引导：代码构建 Canvas/面板/EventSystem/字体
│  ├─ GameApp.cs                # 组合根：菜单→游戏主循环→自动存档
│  └─ TextBuilders.cs           # 状态/档案/人物/日志 的文本渲染（读 Core 模型）
├─ Starstate.Editor.asmdef
├─ Editor/
│  └─ BootSceneBuilder.cs       # 菜单 STARSTATE/创建Boot场景（一键生成可玩场景）
├─ Starstate.Tests.asmdef       # EditMode + nunit
└─ Tests/
   └─ FlowSmokeTest.cs          # 冒烟测试（与原终端版同断言）
```

## 3. 核心循环（对齐报告 §4，与引擎无关）

```
周一 周计划（埋头工作/学习充电/走动人际/休整调整）
  → 周二至周五逐日推进：日常描写 → 事件卡（2~3选项，任务事件=能力检定）
  → 周五：科长周点评（任务评级汇总 + 重心成长 + 同批新人对比）
  → 周末：一个周末场景（覆盖周六日）
  → 月末：月度结算（工资入账/评级统计/试用期进度/档案摘要）
  节假日为独立日流程（事件可安排在节假日）；周一点评规则让节假日周五也能正常收束
```

- 起点 2026-08-24（序章周一）→ 入职 2026-09-01 → Demo 首切片覆盖至 2026-09-30。
- 任务检定：`得分 = 主属性×0.6 + 精力/100×20 + 随机(0~14) + 加成×20`；S≥78 / A≥65 / B≥52 / C≥40 / D。
- 评级自动换算：科长评价（S+4 / A+3 / B+1 / C−1 / D−3）＋士气（S+3…D−4），并写入工作档案。
- 月度结算：结余 = 月薪8500＋补贴800 − 房租900 − 生活3800 ≈ **+4600 元/月**（占位）。

## 4. 事件模型（content 向 Flow 注册的协议）

```csharp
Flow.Register(new GameEvent{
  id="ev_0904_proof", type="work", title="第一个任务：校对会议纪要",
  when=new When{ date="2026-09-04" },          // 或 flag= / randomP=0.15
  paras=new List<string>{ "……" },
  options=new List<EventOption>{
    new EventOption{ label="逐字核对并查数据口径",
      check=new Check{ main="admin", bonus=0.1f },
      effects=new Effects{ energy=-12, stress=3, admin=2,
        rel=new List<RelDelta>{ new RelDelta{ id="zhao", trust=2, memo="主动请教材料"}},
        setFlags=new List<string>{"koujing_aware"},
        task=new TaskRecord{ title="校对会议纪要", note="口径核对" }},
      result="……评级：{grade}。"}},              // {grade} 由检定结果替换
});
```

效果键：五维属性增量 / energy / stress / morale / reputation / polCapital / moneyDelta / rel / setFlags / task / document / integrity / commend / log / gotoWork。
**工作档案（责任记录）**：tasks／documents／integrity（程序合规标记，监察线数据基础）／commendations——一切任务型选择留痕。

## 5. 数据与存档

- `GameState`：玩家（五维/精力/压力/士气/声望/政治资本/职级/财务）、四类档案、九人关系（含记忆列表）、flags、日志、周/月状态、已触发事件表。
- 存档：JsonUtility → `persistentDataPath/starstate_save.json`；每回合自动存档＋主菜单"继续"。

## 6. 占位数值（不违背已确认口径，可调参）

| 项 | 占位值 |
| --- | --- |
| 月薪/补贴/房租/生活 | 8500 / 800 / 900 / 3800 元（锚点：科员月薪≈市收入中位数，小康偏中产） |
| 初始积蓄 | 12000 元 |
| 精力/压力日漂移 | 工作日 −8 / +2（休整重心 +1）；周末日 +12 / −6 |
| 检定阈值 | S≥78 / A≥65 / B≥52 / C≥40 |
| 节假日占位 | 开国纪念日 10-08、宪法日 3-11、元旦 1-1 等为推演占位（Q2-12）；中秋 2026-09-25、清明 2027-04-05 为真实历法 |

## 7. 集成与验证流程（当前阶段）

1. ✅ winget 安装 Unity Hub
2. ✅ 发现并启用本机现有编辑器 Unity 2022.3.57f1c2（`D:\pro\unity\Editor\Unity.exe`）
3. ✅ 手工创建最小工程 `E:\Starstate\game`（ProjectVersion＋manifest＋Assets），代码从 `game-src/` 集成
4. ✅ batchmode 无头编译：零错误
5. ✅ EditMode 冒烟测试 2/2 通过（`FlowSmokeTest`）
6. ⬜ 编辑器内人工试玩：**用 Hub 打开（Add project → E:\Starstate\game），或直接运行 `D:\pro\unity\Editor\Unity.exe -projectPath E:\Starstate\game` → 菜单 STARSTATE → 创建 Boot 场景 → 按 Play**

## 8. 里程碑

| 里程碑 | 内容 | 状态 |
| --- | --- | --- |
| **M0 基础构建** | 核心系统＋序章＋2026年9月可玩切片＋冒烟测试 | ✅ 完成（编译零错误，EditMode 测试 2/2 通过） |
| M1 秋冬 | 10—12月内容、年度考核（评优评先） | ⬜ |
| M2 春季 | 春节抉择、专项调研随行、宪法日、副局长线 | ⬜ |
| M3 收束 | 数据虚高核心抉择（监察伏笔）、试用期考核、转正与新机会、结局系统数据预留 | ⬜ |
