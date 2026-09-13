#!/bin/bash
# STARSTATE Core 探针：脱离 Unity 跑模拟核心（内容体检 + 十年长跑）。
# 用途：改内容/平衡后秒级验证，不必等 Unity 导入与 EditMode（后者 1 分钟起步）。
# 用法：bash tools/core-probe.sh [all|content|long]
set -e

ROOT="E:/Starstate"
DATA="D:/pro/unity/Editor/Data"
API="$DATA/MonoBleedingEdge/lib/mono/4.7.1-api"
CSC="$DATA/DotNetSdkRoslyn/csc.dll"
DOTNET="$DATA/NetCoreRuntime/dotnet.exe"
MONO="$DATA/MonoBleedingEdge/bin/mono.exe"
OUT="$ROOT/.tmpbuild"
CORE="$OUT/Starstate.Core.ForTests.dll"

MODE="${1:-all}"

# ① Core 必须先编过（复用主门禁的第 5 段产物）
if [ ! -f "$CORE" ]; then
  echo "== 未找到 Core 产物，先跑 check-compile.sh =="
  bash "$ROOT/tools/check-compile.sh" > /dev/null
fi

# ② 编译探针（与 Tests 同一条 mscorlib 链：Core 不依赖 UnityEngine）
mkdir -p "$OUT/probe"
{
  echo "-noconfig -nostdlib -langversion:9.0 -nowarn:CS0169,CS0649,CS0414,CS0219,CS0168,CS0436,CS1701,CS1702"
  echo "-out:$OUT/probe/Probe.exe -r:$API/mscorlib.dll -r:$API/System.dll -r:$API/System.Core.dll -r:$CORE"
  echo "$ROOT/tools/probe/Probe.cs"
} > "$OUT/probe.rsp"
"$DOTNET" exec "$CSC" @"$OUT/probe.rsp"
cp "$CORE" "$OUT/probe/" 2>/dev/null || true

# ③ 运行（Mono 需要程序集在 exe 同目录）
cd "$OUT/probe"
"$MONO" Probe.exe "$MODE"
