namespace Starstate.Core
{
    /// <summary>游戏状态工厂：新档默认值集中于此（Attrs 类默认值为 0，玩家初始属性在这里显式赋予）。</summary>
    public static class State
    {
        public static GameState NewGame(string name)
        {
            var st = new GameState();
            st.player.name = string.IsNullOrEmpty(name) ? "沈知行" : name;
            st.player.attrs = new Attrs { professional = 75, admin = 35, exec = 50, comm = 55, political = 30 };
            return st;
        }
    }
}
