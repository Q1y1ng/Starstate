# game-src — Unity 工程暂存区（Assets 镜像）

> 本目录是 Unity 工程的**代码暂存区**，镜像最终 `Assets/` 结构。等 Unity 工程在 `E:\Starstate\game` 创建后，把 `Assets/Scripts` 整体复制进 `game/Assets/` 即可（.meta 由 Unity 自动生成）。

## 复制命令（工程创建后执行）

```bash
cp -r /e/Starstate/game-src/Assets/Scripts /e/Starstate/game/Assets/
```

## 结构

```
Assets/Scripts/
├─ Starstate.Core.asmdef      # 纯C#模拟核心（不引用 UnityEngine）
├─ Core/                      # GameClock / Model / Npcs / ContentRegistry / ContentPrologue / ContentMonth1 / Flow
├─ Starstate.Ui.asmdef
├─ Ui/                        # UiRoot（代码构建UGUI）/ GameApp（组合根+存档）/ TextBuilders
├─ Starstate.Editor.asmdef
├─ Editor/                    # BootSceneBuilder：菜单 STARSTATE→创建 Boot 场景
├─ Starstate.Tests.asmdef
└─ Tests/                     # FlowSmokeTest：EditMode 冒烟测试
```

## 工程就绪后的验证清单

1. 菜单 **STARSTATE → 创建 Boot 场景**
2. Test Runner（EditMode）跑 `FlowSmokeTest` 两项测试 → 全绿
3. 按 Play：主菜单 → 新游戏 → 序章三选一 → 9月1日报到 → 正常推进到 9/30 月度结算
