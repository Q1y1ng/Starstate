using System;
using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 时钟系统：机会与威胁的可见进度条（Citizen Sleeper 式）。
    /// 满格触发 onFullEventId（由 Flow.CollectDue 检查入队）；dailyRate 为自然推进/天。
    /// </summary>
    public static class Clocks
    {
        /// <summary>按效果指令建立/更新/推进/移除时钟。</summary>
        public static void Apply(GameState st, ClockOp op)
        {
            if (op == null || string.IsNullOrEmpty(op.id)) return;
            if (op.remove) { Remove(st, op.id); return; }
            var c = st.clocks.Find(x => x.id == op.id);
            if (c == null)
            {
                c = new ClockState { id = op.id, max = op.max > 0 ? op.max : 5 };
                st.clocks.Add(c);
            }
            if (!string.IsNullOrEmpty(op.label)) c.label = op.label;
            if (!string.IsNullOrEmpty(op.kind)) c.kind = op.kind;
            if (!string.IsNullOrEmpty(op.onFull)) c.onFullEventId = op.onFull;
            if (op.max > 0) c.max = op.max;
            c.dailyRate = op.dailyRate;
            if (op.delta != 0) Advance(st, op.id, op.delta);
        }

        public static void Advance(GameState st, string id, int delta)
        {
            if (string.IsNullOrEmpty(id) || delta == 0) return;
            var c = st.clocks.Find(x => x.id == id);
            if (c == null) return;
            c.value = Math.Max(0, Math.Min(c.max, c.value + delta));
        }

        public static void Remove(GameState st, string id)
        {
            st.clocks.RemoveAll(x => x.id == id);
        }

        /// <summary>每日自然推进（Flow.CollectDue 开头调用；日期变更的各路径都会经过）。</summary>
        public static void DailyTick(GameState st)
        {
            for (int i = 0; i < st.clocks.Count; i++)
            {
                var c = st.clocks[i];
                if (c.dailyRate != 0 && c.value < c.max)
                    c.value = Math.Min(c.max, c.value + c.dailyRate);
            }
        }
    }

    /// <summary>
    /// NPC 周 tick：人物随时间自然变动（每周一由 Flow.ResetWeek 调用）。
    /// 骨架：久未互动淡忘 + 偶发主动找你钩子（npc_initiative 动态事件在 P4.2 注册）。
    /// </summary>
    public static class NpcTick
    {
        public static void WeeklyTick(GameState st)
        {
            // 1) 久未互动的淡忘：最近一条记忆超过 28 天 → 熟悉度 -1（有记忆的人不会淡到零以下）
            foreach (var r in st.relations)
            {
                if (r.familiar <= 0 || r.memories.Count == 0) continue;
                var last = r.memories[r.memories.Count - 1];
                if (string.IsNullOrEmpty(last.date)) continue;
                try
                {
                    if (GameClock.Parse(st.date).Subtract(GameClock.Parse(last.date)).TotalDays > 28)
                        r.familiar = Math.Max(0, r.familiar - 1);
                }
                catch { /* 日期异常跳过 */ }
            }

            // 2) 偶发：有人主动来找你（10%／周；未注册时静默跳过）
            if (Flow.RngNextDouble() < 0.10)
                Flow.TryEnqueueIfRegistered(st, "npc_initiative");
        }
    }
}
