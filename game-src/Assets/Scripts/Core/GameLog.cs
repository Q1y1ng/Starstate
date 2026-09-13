using System;

namespace Starstate.Core
{
    /// <summary>
    /// 极简日志出口：Core 是纯 C#（禁止 UnityEngine），但模拟层与测试都需要一条输出通道。
    /// Ui 层启动时把 <see cref="Sink"/> 接到 Debug.Log；未接线时静默（测试环境不产生噪声）。
    /// </summary>
    public static class GameLog
    {
        /// <summary>日志接收器（Ui 层接线；null = 丢弃）。</summary>
        public static Action<string> Sink;

        public static void Info(string msg)
        {
            var s = Sink;
            if (s != null) s(msg);
        }

        public static void Warn(string msg)
        {
            var s = Sink;
            if (s != null) s("[WARN] " + msg);
        }
    }
}
