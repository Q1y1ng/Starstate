#!/bin/bash
# STARSTATE 独立编译检查：绕开 Unity 编辑器，用编辑器自带 Roslyn 按程序集编译。
# 用法：bash tools/check-compile.sh
set -e

# ⚠️ 路径全部**从脚本自身位置推导**（2026-09-18 E 盘格式化重建时改为位置无关）：
#    以前硬编码 E:/Starstate 与 com.unity.ugui@1.0.0 这类版本号，换目录/升包即断。
# 注意：Git Bash 的 pwd 给的是 /e/... 形式，csc 会误解析成 <当前盘>:\e\...，
#      所以必须用 `pwd -W` 取回盘符形式（E:/AI/Starstate）。
SELF_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -W)"
ROOT="$(dirname "$SELF_DIR")"

DATA="D:/pro/unity/Editor/Data"
DOTNET="$DATA/NetCoreRuntime/dotnet.exe"
CSC="$DATA/DotNetSdkRoslyn/csc.dll"
NETSTD="$DATA/NetStandard/ref/2.1.0/netstandard.dll"
UMGD="$DATA/Managed/UnityEngine"
SRC="$ROOT/game-src/Assets/Scripts"
# 包缓存用通配匹配：Unity 首次打开工程时才会生成 Library/PackageCache，且版本号会随包升级变化
UGUI="$(ls -d "$ROOT"/game/Library/PackageCache/com.unity.ugui@* 2>/dev/null | head -1)"
NUNIT="$(ls "$ROOT"/game/Library/PackageCache/com.unity.ext.nunit@*/net35/unity-custom/nunit.framework.dll 2>/dev/null | head -1)"
MSCORLIB="$DATA/MonoBleedingEdge/lib/mono/2.0-api/mscorlib.dll"
SYS20="$DATA/MonoBleedingEdge/lib/mono/2.0-api/System.dll $DATA/MonoBleedingEdge/lib/mono/2.0-api/System.Core.dll"
OUT="$ROOT/.tmpbuild"
mkdir -p "$OUT"

if [ -z "$UGUI" ] || [ -z "$NUNIT" ]; then
  echo "✗ 未找到 Unity 包缓存（Library/PackageCache）——请先用 Unity 打开一次工程 game/（首次导入会生成）。"
  echo "  期望位置：$ROOT/game/Library/PackageCache/com.unity.ugui@* 与 com.unity.ext.nunit@*"
  exit 2
fi
if [ ! -d "$SRC" ]; then echo "✗ 源码目录不存在：$SRC"; exit 2; fi

WARN="-nowarn:CS0169,CS0649,CS0414,CS0219,CS0168,CS0108,CS0114,CS0618,CS0436,CS1701,CS1702"
COMMON="-noconfig -nostdlib -target:library -langversion:9.0 -define:UNITY_2022_3_OR_NEWER $WARN"

UMG_REFS=""
for d in "$UMGD"/UnityEngine*.dll; do UMG_REFS="$UMG_REFS -r:$d"; done
EDI_REFS=""
for d in "$UMGD"/UnityEditor*.dll; do EDI_REFS="$EDI_REFS -r:$d"; done

echo "== [1/5] uGUI (com.unity.ugui 源码) =="
{
  echo "$COMMON -out:$OUT/ugui.dll -r:$NETSTD $UMG_REFS"
  find "$UGUI/Runtime" -name "*.cs"
} > "$OUT/ugui.rsp"
"$DOTNET" exec "$CSC" @"$OUT/ugui.rsp"

echo "== [2/5] Starstate.Core =="
{
  echo "$COMMON -out:$OUT/Starstate.Core.dll -r:$NETSTD"
  ls "$SRC"/Core/*.cs
} > "$OUT/core.rsp"
"$DOTNET" exec "$CSC" @"$OUT/core.rsp"

echo "== [3/5] Starstate.Ui =="
{
  echo "$COMMON -out:$OUT/Starstate.Ui.dll -r:$NETSTD -r:$OUT/ugui.dll -r:$OUT/Starstate.Core.dll $UMG_REFS"
  ls "$SRC"/Ui/*.cs
} > "$OUT/ui.rsp"
"$DOTNET" exec "$CSC" @"$OUT/ui.rsp"

echo "== [4/5] Starstate.Editor =="
{
  echo "$COMMON -out:$OUT/Starstate.Editor.dll -r:$NETSTD -r:$OUT/ugui.dll -r:$OUT/Starstate.Core.dll -r:$OUT/Starstate.Ui.dll $UMG_REFS $EDI_REFS"
  ls "$SRC"/Editor/*.cs
} > "$OUT/editor.rsp"
"$DOTNET" exec "$CSC" @"$OUT/editor.rsp"

echo "== [5/5] Starstate.Tests =="
# Tests 走 mscorlib 独立链：NUnit 是 net35 档，与 netstandard 2.1 无法混引（会报 CS0012/CS0518）。
# Core 本就不依赖 UnityEngine，所以再用 4.7.1-api 编一份等价 Core 供测试引用。
API="$DATA/MonoBleedingEdge/lib/mono/4.7.1-api"
BCL4="-r:$API/mscorlib.dll -r:$API/System.dll -r:$API/System.Core.dll"
{
  echo "$COMMON -out:$OUT/Starstate.Core.ForTests.dll $BCL4"
  ls "$SRC"/Core/*.cs
} > "$OUT/core4.rsp"
"$DOTNET" exec "$CSC" @"$OUT/core4.rsp"
{
  echo "$COMMON -out:$OUT/Starstate.Tests.dll $BCL4 -r:$OUT/Starstate.Core.ForTests.dll -r:$NUNIT"
  ls "$SRC"/Tests/*.cs
} > "$OUT/tests.rsp"
"$DOTNET" exec "$CSC" @"$OUT/tests.rsp"

echo "ALL_OK"
