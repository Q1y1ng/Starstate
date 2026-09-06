using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>人物定义表与关系系统。M0 默认阵容（FOUNDATION_REPORT §7.8），姓名细节为设计草案。</summary>
    public static class Npcs
    {
        public class Def
        {
            public string name, title, grade, traits, desc;
            public int age;
        }

        public static readonly Dictionary<string, Def> Defs = new Dictionary<string, Def>
        {
            ["zhou"] = new Def {
                name = "周衡之", title = "综合科科长", grade = "吏一·正科", age = 45,
                traits = "严谨、护短",
                desc = "材料出身的老机关，在这层楼待了二十年。嘴上厉害，笔下护人，最恨数据口径出岔子。" },
            ["lin"] = new Def {
                name = "林晚", title = "综合科副科长", grade = "吏二·副科", age = 33,
                traits = "业务能手、温和",
                desc = "全局有名的笔杆子，写材料讲究“分寸”二字。对新人不藏私，但也不替人扛活。" },
            ["zhao"] = new Def {
                name = "赵桂芳（赵姐）", title = "综合科科员", grade = "吏三·科员（老科员）", age = 52,
                traits = "热心、通透",
                desc = "干了三十年科员，明年在退休线上了。科里的活字典，谁的茶杯放在哪她都知道。" },
            ["ma"] = new Def {
                name = "马建国（马副局长）", title = "副局长（分管综合科）", grade = "九品·正处", age = 49,
                traits = "威严、务实",
                desc = "开会是“我简单说两句”能说四十分钟的人。记性极好，谁的材料好他心里有账。" },
            ["ren"] = new Def {
                name = "任雪梅（任姐）", title = "人事科干部", grade = "吏二·副科", age = 41,
                traits = "流程通、口风紧",
                desc = "全员的档案都从她手里过。她笑的时候在办事，她不笑的时候也在办事。" },
            ["xu"] = new Def {
                name = "许飞", title = "发展规划科科员（同批新人）", grade = "吏三·科员", age = 24,
                traits = "外向、好胜",
                desc = "同批笔试第一名，逢人便提。目标明确：三年内让分管副局长记住全名。" },
            ["su"] = new Def {
                name = "苏晴", title = "固定资产投资科科员（同批新人）", grade = "吏三·科员", age = 23,
                traits = "踏实、细心",
                desc = "话不多，台账做得比老科员还齐整。你入职那天她帮你搬的箱子。" },
            ["he"] = new Def {
                name = "何斌", title = "产业发展科科员（同批新人）", grade = "吏三·科员", age = 25,
                traits = "圆滑、消息灵通",
                desc = "入职两周已能报出全局领导的籍贯。消息来源成谜，本人乐于此道。" },
            ["tong"] = new Def {
                name = "童远", title = "市统计局综合科科员", grade = "吏三·科员", age = 26,
                traits = "直爽、口径活字典",
                desc = "兄弟部门的同龄人。全长安最清楚“规上规下、当月累计”区别的人之一，且乐于科普。" },
        };

        public static readonly string[] Order = { "zhou", "lin", "zhao", "ma", "ren", "xu", "su", "he", "tong" };

        public static string Name(string id) => Defs.ContainsKey(id) ? Defs[id].name : id;
        public static string Title(string id) => Defs.ContainsKey(id) ? Defs[id].title : "";

        public static void Ensure(GameState st)
        {
            foreach (var id in Order)
            {
                if (st.relations.Find(r => r.id == id) == null)
                    st.relations.Add(new RelationEntry { id = id });
            }
        }

        public static RelationEntry Get(GameState st, string id)
        {
            var r = st.relations.Find(x => x.id == id);
            if (r == null) { r = new RelationEntry { id = id }; st.relations.Add(r); }
            return r;
        }

        /// <summary>应用关系增量并写记忆。</summary>
        public static void Mod(GameState st, RelDelta d)
        {
            var r = Get(st, d.id);
            r.familiar = Clamp(r.familiar + d.familiar, 0, 100);
            r.trust = Clamp(r.trust + d.trust, -100, 100);
            r.evalv = Clamp(r.evalv + d.evalv, -100, 100);
            if (!string.IsNullOrEmpty(d.memo))
                r.memories.Add(new MemoryEntry { date = st.date, text = d.memo });
            // 记忆上限：只留最近 8 条（提示词取最后一条、UI 显示最后一条，裁剪无感）
            if (r.memories.Count > 8) r.memories.RemoveRange(0, r.memories.Count - 8);
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
