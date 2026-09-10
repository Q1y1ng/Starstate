using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>人物定义表与关系系统。Phase 5：市级权力地形（常委会 11 席核心＋家庭）。</summary>
    public static class Npcs
    {
        public class Def
        {
            public string name, title, grade, traits, desc;
            public int age;
        }

        public static readonly Dictionary<string, Def> Defs = new Dictionary<string, Def>
        {
            ["cen"] = new Def {
                name = "岑伯衡", title = "市人民政治委员会常务委员会主席", grade = "七品·正厅", age = 54,
                traits = "沉稳、护盘、记仇",
                desc = "大同政治口的定盘星。说话慢，批人准。最烦年轻干部“不讲程序的聪明”。" },
            ["han"] = new Def {
                name = "韩清", title = "市委组织部部长（常委会副主席）", grade = "七品·正厅", age = 50,
                traits = "精密、中立、档案癖",
                desc = "管档案的人。谁的履历干净、谁的特批过多，她比当事人还清楚。" },
            ["shenyan"] = new Def {
                name = "沈砚", title = "市纪委（监委）主持工作的领导", grade = "七品·正厅", age = 52,
                traits = "冷、程序正义",
                desc = "同姓不同宗。他手里的线索不响，响的时候已经晚了。可盟可敌，取决于你桌上的卷宗干不干净。" },
            ["shao"] = new Def {
                name = "邵志远", title = "市委常委、常务副市长", grade = "八品·副厅", age = 49,
                traits = "能干、分权、好名",
                desc = "抓落实的一把好手，也爱把功劳先写进自己的纪要里。搭档还是对手，看季度。" },
            ["zhoujin"] = new Def {
                name = "周谨", title = "市政府办公厅主任（常委会委员）", grade = "八品·副厅", age = 46,
                traits = "闸门、周到、嘴严",
                desc = "决定你桌上先出现哪一份文件的人。挡驾与放行，都是艺术。" },
            ["xu"] = new Def {
                name = "许飞", title = "邻市市长（同批竞争者）", grade = "七品·正厅", age = 42,
                traits = "锐、快、爱比较",
                desc = "你们同一年过帝国考试。他把“比你早进六品”写在每一次调研的路线选择里。" },
            ["laokang"] = new Def {
                name = "康慎行（老康）", title = "退居二线的老同志、前任班子成员", grade = "七品·正厅（退休待遇）", age = 67,
                traits = "通透、书信",
                desc = "写得一手好字。退了仍有人找他递话。他的信，比文件难批。" },
            ["linwan"] = new Def {
                name = "林晚", title = "市属国企中层（配偶）", grade = "吏一·正科", age = 40,
                traits = "清醒、克制",
                desc = "她比你更怕“打招呼”三个字。家里那盏灯，是你签批之外的另一套审查。" },
            ["fang"] = new Def {
                name = "方启年", title = "市财政局局长", grade = "九品·正处", age = 48,
                traits = "算盘精、怕事",
                desc = "数字从他手里过一遍会变薄。你要真数，得亲自去局里坐。" },
            ["rensheng"] = new Def {
                name = "任慎", title = "市自然资源局局长", grade = "九品·正处", age = 51,
                traits = "土地、话少",
                desc = "批地的章在他抽屉里。开发商叫他任叔，他叫开发商“企业朋友”。" },
            // —— 旧科员阵容保留定义，供迁移内容/回响引用 ——
            ["zhou"] = new Def {
                name = "周衡之", title = "（旧线）综合科科长", grade = "吏一·正科", age = 45,
                traits = "严谨、护短", desc = "旧职业线人物。" },
            ["lin"] = new Def {
                name = "林晚（旧线）", title = "（旧线）", grade = "吏二·副科", age = 33,
                traits = "—", desc = "与配偶同名占位，迁移时废弃。" },
        };

        public static readonly string[] Order =
        {
            "cen", "han", "shenyan", "shao", "zhoujin", "xu", "laokang", "linwan", "fang", "rensheng",
        };

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
            if (r.memories.Count > 8) r.memories.RemoveRange(0, r.memories.Count - 8);
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
