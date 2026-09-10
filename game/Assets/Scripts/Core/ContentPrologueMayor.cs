using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// Phase 5 序章三幕（七品市长就任）：
    /// ① 省城谈话 ② 第一次常委会 ③ 第一份带雷卷宗。
    /// 文风：冷、准、具体；一个感官细节＋一处权力暗示。
    /// </summary>
    public static class ContentPrologueMayor
    {
        public static void Register()
        {
            Flow.Register(new GameEvent
            {
                id = "mp0", type = "system", title = "序章 · 省城谈话",
                paras = new List<string>
                {
                    "2026年8月24日，山西省署大楼。空调出风口对着你的后颈，吹了四十分钟。",
                    "送你上任的人只说了半句：“大同的账，面上过得去。”另外半句，他用杯盖刮了刮茶叶，没说。",
                    "调令上的字很硬：大同市人民政府市长，七品正厅。你今年四十一岁。档案里有三次“优秀破格”的特批——组织部管这叫“年轻化”，巡视组管这叫“关注对象”。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "问清楚：账，面下过不过得去？",
                        effects = new Effects
                        {
                            political = 2, stress = 2,
                            setMarks = new List<string>{ "probed_truth" },
                            logKind = "人物", logText = "就任前：追问省里口风",
                        },
                        result = "对方放下杯盖，看了你三秒。“你去了就知道。记住一条——签字要能过夜。”你出门时后颈已经不凉了，是汗。",
                    },
                    new EventOption
                    {
                        label = "不问。表态：把大同的事办好。",
                        effects = new Effects
                        {
                            admin = 2, morale = 2,
                            setMarks = new List<string>{ "took_charge" },
                            logKind = "人物", logText = "就任前：明确表态",
                        },
                        result = "他点点头，第一次露出一点笑意。“年轻干部，态度是好的。态度不能当饭吃，但能当开门砖。”",
                    },
                    new EventOption
                    {
                        label = "只谈工作交接，不碰“账”字。",
                        effects = new Effects
                        {
                            exec = 1, political = 1,
                            setMarks = new List<string>{ "played_safe" },
                            logKind = "人物", logText = "就任前：回避敏感话题",
                        },
                        result = "谈话提前八分钟结束。走廊很长，你的皮鞋声比平时响。有些门，是你自己没推开的。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "mp1", type = "system", title = "序章 · 第一次常委会",
                paras = new List<string>
                {
                    "8月31日，市委常委会会议室。长桌十一个位置，你的牌在主席右手第二——《席位表》写着：副主席、市长。",
                    "岑伯衡主持。他开场没看你，看的是茶杯：“新同志来了，班子还是这个班子。大同的事，急不得，也拖不得。”",
                    "韩清念了你的简历。念到“三次提前考核”时，她的笔尖在纸上顿了一下。沈砚坐在斜对面，像在听，又像在记。",
                    "散会前，岑伯衡终于看你：“市长同志，政府那边，你先熟悉文件。签字——要负得起责。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "表态：先当学生，再当先生。",
                        effects = new Effects
                        {
                            comm = 1,
                            setMarks = new List<string>{ "first_pc_humble" },
                            rel = new List<RelDelta>
                            {
                                new RelDelta{ id="cen", evalv = 3, memo="第一次常委会：姿态放低" },
                                new RelDelta{ id="han", familiar = 2 },
                            },
                            logKind = "人物", logText = "第一次常委会：谦逊表态",
                        },
                        result = "岑伯衡“嗯”了一声，不置可否。韩清合上简历的速度慢了半拍——她在心里又记了一笔：会做人，未必会做官。",
                    },
                    new EventOption
                    {
                        label = "表态：尽快进入状态，对发展负责。",
                        effects = new Effects
                        {
                            exec = 2,
                            setMarks = new List<string>{ "first_pc_eager" },
                            rel = new List<RelDelta>
                            {
                                new RelDelta{ id="shao", evalv = -2, memo="新市长急着表态" },
                                new RelDelta{ id="cen", evalv = 1 },
                            },
                            logKind = "人物", logText = "第一次常委会：积极表态",
                        },
                        result = "邵志远翻了一页材料，没抬头。岑伯衡说：“好。那就看文件。”——在这间会议室，“看文件”既是鼓励，也是圈禁。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "mp2", type = "system", title = "序章 · 第一份卷宗",
                paras = new List<string>
                {
                    "9月1日，市长办公室。台历是新的，茶是周谨亲手泡的，文件堆得比你的水杯高。",
                    "最上面一份是市财政局的请示：关于追加云中老工业区搬迁专项资金。文号、印章、签批栏齐全。第3页的表格很干净——干净得像擦过。",
                    "周谨站在门口：“老板，今天五件。这件标了‘急’。桌角那几份口径，是办公厅给您备的——数字勾稽那本，最好先摊开。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "先看这份。（进入卷宗办理）",
                        effects = new Effects
                        {
                            setFlags = new List<string>{ "tutorial_dossier_ready" },
                            logKind = "系统", logText = "开始办理第一份卷宗",
                        },
                        result = "你把烟（你戒了三年，桌上还是摆着烟灰缸——前任留下的）往里推了推，翻开第一页。\n\n签字之前，先学会看纸。",
                    },
                },
            });

            // 教学卷宗由 DossierEngine 在入职后投放；此处勾子确保标记落地
            Flow.Register(new GameEvent
            {
                id = "mp_hook", type = "system", title = "序章 · 落地",
                when = new When { flag = "tutorial_dossier_ready" },
                paras = new List<string>
                {
                    "窗外，云中的天灰得很有层次：底下是老城的煤色，上面是产业园玻璃的反光。",
                    "你的十年，从这摞纸开始。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "开始第一周",
                        effects = new Effects
                        {
                            gotoWork = true,
                            setMarks = new List<string>{ "prologue_done" },
                            morale = 2,
                            logKind = "系统", logText = "就任大同市市长",
                        },
                        result = "办公厅的铃响了。周谨的声音很稳：“老板，周计划表。还有——下午有企业想来坐坐。”",
                    },
                },
            });
        }
    }
}
