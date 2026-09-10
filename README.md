# STARSTATE

> 在一个真实运行的社会中，经历自己的人生。
>
> **当前版本：v0.2.6 Preview**（Phase 5 市长办公桌，EditMode 37/37 全绿）

Starstate 是一款社会人生模拟游戏：玩家通过一个具体的人，观察并参与一个持续运转的社会——工作、学习、家庭、财富、人际关系、社会地位、职业发展、政治经历都会随时间变化；政府、企业、学校、政党与城市也在同步运行，并与玩家的经历相互交织。

**世界观基底**：架空世界——1700 年建立的"中华帝国"（制度化贤能帝制），经历自主工业化与 1978—1986 大革命（"第二次建国"），2026 年进入 AI 时代的宪制帝国。**这不是现实中国**，一切设定以世界观资料为准（见文末）。

## 当前状态

**Phase 1 ✅ → Phase 2 ✅ → Phase 3 ✅ → Phase 4 ✅ → Phase 5 进行中**（2026-09-19，EditMode **23/23** 全绿）。Phase 5「市长办公桌」把体验对标 Papers Please：七品·大同市市长（41 岁）卷宗签批、两把尺（合规/效率）、常委会权力地形（见 [changelog/PHASE-5.md](changelog/PHASE-5.md)、[docs/compose/spec/papers-desk-overhaul.md](docs/compose/spec/papers-desk-overhaul.md)）。

「政府职业成长」当前形态（v0.2 Preview · Phase 5 M0）：

- **七品开局**：41 岁沈砚舟就任大同市人民政府市长（正厅·常委会副主席）；最快合规轨三次特批写入 marks——荣光也是巡视靶子；
- **卷宗签批**（Papers Please 本体）：周一办公厅送达卷宗包；翻页阅读→有限次核对（数字勾稽/程序瑕疵/人情土地）→照准/退回/请示/会签/压下/特事特办；埋雷可查可漏；
- **两把尺**：合规分（程序与实体合法性）× 效率分（时限、积压、省里交办）——进年度考核、届中鉴定、巡视底稿；
- **常委会权力地形**：岑伯衡/韩清/沈砚/邵志远/周谨……信任冷热可见；口头指示与书面规则冲突是刀；
- **周计划五槽**：签批专注 / 调研摸底 / 会商协调 / 关系走动 / 家庭休整；
- **办公桌美学**（零美术资产，全程序生成）：木纹桌面＋台灯晕、红头文件＋朱砂「签批」章、台历大字日期、纸纹与程序音效；
- **序章三幕**：省城谈话 → 第一次常委会 → 第一份带雷卷宗；
- **十年骨架**：七品→六品竞争、年度考核、审查/辞职/平稳/灰色/十年一日等结局分支（内容量仍在 M1 填充）。

> Phase 1–4 的科员十年完整版逻辑仍在仓库中（吏轨 API 留桩，内容待迁到市长层）；主菜单从 v0.2 起默认走市长办公桌。

## 大模型增强（可选，默认开启）

游戏可在 NPC 交谈、日常"小插曲"、**科长周评**上接入大模型，**架构上保证不影响可玩性**：AI 只产文本与情绪分类，所有数值效果走白名单；关闭/失败自动回退内置内容。

- **本地模式（默认）**：自动拉起 `llama-server`（OpenAI 兼容），默认模型 `Ornith-1.5-9B-Heretic-Q4_K_M` ＋ RP-LoRA（可在设置中更换/留空）；启动参数按 `D:\AI` 标定报告执行（`-ngl 28` 固定＋KV 双 q4_0 量化＋`-ub 128 -fa on -t 16 --cpu-range 0-19`），显存极端不足时自动降档；
- **外部 API 模式**：任何 OpenAI 兼容 `…/v1/chat/completions` 端点＋密钥；
- **AI 周评**：周五例会结算后，AI 以科长口吻生成一句引用本周真实计划与事件的评语，追加在点评页（【纯文本】协议，失败静默）；
- **设置页**：总开关、模式切换、地址/密钥/模型名/路径/LoRA、测试连接、音效开关；顶栏徽标实时显示 AI 状态；
- 本机实测（RTX 3060 Laptop 6GB）：交谈生成约 13—18 秒/次；三模型横评与选型结论见 [changelog/PHASE-3.md](changelog/PHASE-3.md) 附录。

## 快速开始

1. 安装 **Unity 2022.3.57f1c2**（Hub 添加本机编辑器 `D:\pro\unity`，升级路径 Unity 6 LTS）；
2. 打开工程 `game/`，进入 `Assets/Scenes/Boot.unity`，按 **Play**；
3. Boot 场景**不挂任何脚本**：`GameBootstrap` 由 `RuntimeInitializeOnLoadMethod` 运行时自举（根治场景脚本引用剥落问题）；首次会进主菜单——输入名字开始新局；
4. AI 增强（可选）：保持默认即可自动唤醒本地模型（首次约 1—2 分钟）；或到"设置 → AI 接入"改外部 API / 关闭。

## 工程结构

| 位置 | 内容 |
| --- | --- |
| `game/` | Unity 工程（Assembly：Starstate.Core / Ui / Editor / Tests） |
| `game-src/` | 代码暂存区（Assets 镜像，先改这里再同步；**绝不删除 game/Assets 下的 .meta**） |
| `docs/` | Phase 1 报告、待确认清单、Phase 2/3/4 技术设计 |
| `changelog/` | 各 Phase 开发历程（Phase 3 含九补丁，Phase 4 为叙事升级全程） |
| `tools/check-compile.sh` | 独立编译检查（编辑器自带 Roslyn，四程序集零错误门禁） |

**分层纪律**：`Core/` 纯 C# 不依赖 UnityEngine（可测试可迁移）；`content` 只向 `Flow.Register` 注册数据；`Ui/` 只读状态、只调 `Flow.Choose`；LLM 增强层的提示词/解析/效果白名单在 Core（`Core/Llm.cs`），HTTP 与进程管理在 Ui。

**测试**：EditMode 17 项——序章＋九月通关、十年自动通关（结局/考核/档案断言）、任务评级边界、LLM 容错解析/白名单/回退、属性成长回归、P4 引擎五项（回响到点/池不枯竭/选项锁定/时钟满格/NPC 淡忘）。

## 文档导航

- [AGENTS.md](AGENTS.md) — Agent/开发者第一阅读文件：项目共识、资料位置与使用规则（含接手流程）
- [docs/HANDOFF.md](docs/HANDOFF.md) — **开发交接手册**：代码地图、核心机制速查、常用配方、验证门禁、坑清单
- [docs/FOUNDATION_REPORT.md](docs/FOUNDATION_REPORT.md) — Phase 1 主报告：世界观概要 → 玩法定义 → 核心循环 → 核心数据 → 对应表 → 原型 → 确认状态
- [docs/OPEN_QUESTIONS.md](docs/OPEN_QUESTIONS.md) — 待确认事项（玩法项已全部批复确认；世界观遗留 Q1 不阻塞）
- [docs/PHASE2-DESIGN.md](docs/PHASE2-DESIGN.md) / [docs/PHASE3-DESIGN.md](docs/PHASE3-DESIGN.md) / [docs/PHASE4-DESIGN.md](docs/PHASE4-DESIGN.md) — Phase 2/3/4 技术设计
- [changelog/](changelog/) — 开发历程（Phase 3 含九个补丁＋模型横评；Phase 4 为叙事升级全程）

## 世界观资料

位于 `E:\AI\帝国\设定\`（只读，项目内不存副本）：《中华帝国世界观总设定v2.0》（最高依据，【核心正式设定】冻结）、《中华帝国编年史1700-2026》、《世界近代史推演》、《中华帝国行政区地图》。辅助参考：《华北风暴·正文》及其修订说明（官场质感与 2026 高层人事口径）。
