# Phase 2 · 系统设计与实现（进行中）— 历程记录

> 目标：实现第一个玩法「政府职业成长」的可玩原型。
> 蓝图：[docs/FOUNDATION_REPORT.md](../docs/FOUNDATION_REPORT.md) §4-§7；技术设计：[docs/PHASE2-DESIGN.md](../docs/PHASE2-DESIGN.md)。
> 里程碑：**M0 基础构建**（本期）→ M1 秋冬内容与年度考核 → M2 春季专项调研线 → M3 责任抉择与转正收束。

---

## 2026-09-06 · M0 基础构建完成 ✅

**事项**：序章＋2026年9月可玩纵向切片全部实现并通过自动化验证。

**验证结果**：

- Unity 2022.3.57f1c2 batchmode 无头编译：**零错误**（Core / Ui / Editor / Tests 四程序集全部通过；此前用 Unity 自带 Roslyn 逐程序集定位并修复了 asmdef 位置冲突、字段错置、缺引用等 10+ 处问题）；
- EditMode 冒烟测试 **2/2 通过**：
  - `Prologue_And_September_Completes`——自动玩完序章＋九月：日期推进至 2026-10-01、任务档案 ≥3 条、试用期 1/12、月度结算积蓄 16600 元、报到事件与人物关系全部正确建立；
  - `TaskCheck_Grades_Are_Bounded`——任务检定评级 S/A/B/C/D 边界正确。

**M0 内容清单**：日历与节假日系统（含占位节假日 Q2-12）／玩家状态与五维能力／九人关系与记忆／事件引擎（日期/标记/随机触发＋效果结算）／周计划→日事件→周五点评→周末→月结算循环／16 个脚本事件＋4 个随机事件＋序章三选一／代码构建的 UGUI（公文纸风格、OS 中文字体）／JSON 自动存档／菜单继续或新开。

**待办（下阶段）**：编辑器内人工试玩体验（STARSTATE→创建 Boot 场景→Play）；M1 秋冬内容与年度考核；Unity 6 LTS 升级（可选，不阻塞）。

## 2026-09-06 · 关键发现：本机已有 Unity 编辑器，M0 立即开工

**事项**：安装 Unity Hub 后检索开始菜单，发现本机**已装有 Unity 2022.3.57f1c2 编辑器**（Unity 中国版，`D:\pro\unity\Editor\Unity.exe`）——此前的盘符搜索遗漏。

**决定与理由**：M0 直接用现有编辑器做**无头（batchmode）编译＋EditMode 冒烟测试**，零下载、零登录等待；手工创建最小 Unity 工程（ProjectVersion＋manifest＋Assets）。作者选定的 Unity 6 LTS 作为**升级路径**保留（简单工程升级顺畅），不阻塞 M0。所有模拟核心为纯 C#，跨版本无风险。

**产出**：`game/`（Unity 工程：ProjectSettings/ProjectVersion.txt 2022.3.57f1c2、Packages/manifest.json（ugui＋test-framework）、Assets/Scripts 全部 M0 代码、.gitignore）；`game-src/` 保留为暂存镜像。

## 2026-09-06 · 技术选型升级：定 Unity 6 LTS

**事项**：作者否决 Web 方案；在拟定 Python 终端原型后，作者进一步要求"正式游戏级"，在 Unity 与 UE5 之间征询意见。

**决定与理由**：定 **Unity 6 LTS + C#**。Starstate 是重 UI、重数据、重事件流的文字型人生模拟——Unity 的 C# 数据驱动＋UGUI 正好匹配、中文资料与生态最厚；UE5 的优势在高保真 3D，本作用不上，30-60G 体积与更慢的迭代是净负担。本机此前无任何引擎：已用 winget 安装 Unity Hub（作者登录激活 Personal 授权并安装 Unity 6 LTS 到 E 盘）；**模拟核心（core）保持纯 C#、不依赖 UnityEngine**，保证可测试与可迁移。

**产出**：docs/PHASE2-DESIGN.md 重写为 Unity 版；`game-src/`（Assets 镜像暂存区，含完整 M0 C# 代码，待工程创建后集成验证）。

## 2026-09-06 · Phase 2 启动：M0 基础构建

**事项**：开始第一个可玩纵向切片（序章＋2026年9月）。

**决定与理由**（原始记录，技术选型后被上述条目取代，流程与范围设计仍有效）：

- **M0 范围**：核心系统（游戏状态与存档／日历与节假日／人物与关系／事件引擎与效果结算／核心循环：周计划→日事件→周点评→周末→月结算）＋ 序章与九月全部脚本事件 ＋ 冒烟测试。
- **占位数值**：科员月薪 8500＋基本补贴 800／月；公租房 900＋生活开支 3800／月（净结余 4600）；节假日中元旦、开国纪念日（10-08）、宪法日（3-11）等具体日期为占位推演（登记 Q2-12）。

**产出**：changelog 体系建立；docs/PHASE2-DESIGN.md。
