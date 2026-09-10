using System;
using System.Collections.Generic;

namespace Starstate.Core
{
    public enum Phase { Prologue, Day, WeekPlan, WeekEnd, Weekend, MonthEnd, Ending }

    // ---------------- 玩家与状态 POCO（JsonUtility 友好：无 Dictionary） ----------------

    [Serializable]
    public class Attrs
    {
        // 注意：默认值必须为 0——本类也用作 WeekEndData.growth 等增量容器，
        // 玩家初始值由 State.NewGame 显式赋予（否则每周五例会把初始值当增量重复发放→五维全 100）。
        public int professional;        // 专业能力
        public int admin;               // 行政能力
        public int exec;                // 执行能力
        public int comm;                // 沟通能力
        public int political;           // 政治敏感度
    }

    [Serializable]
    public class PlayerState
    {
        public string name = "沈砚舟";
        public int birthYear = 1985;
        public string school = "国立中央翰林院大学";
        public string major = "经济学（社会科学方向）";
        public Attrs attrs = new Attrs();
        public int energy = 70, stress = 35, morale = 60;
        public int reputation = 40;     // 社会声望（七品起点）
        public int polCapital = 25;     // 政治资本
        public string unit = "大同市人民政府";
        public string post = "市长";
        public string rank = "七品·正厅";
        public int probationMonths = 0;
        public int savings = 280000;    // 市级主官量级占位（清白工资积累）
        public int monthlyIn = 18000;   // 正厅工资+补贴口径占位
        public int monthlyOut = 9500;   // 家庭开支占位
    }

    [Serializable]
    public class TaskRecord
    {
        public string date = "", title = "", grade = "B", note = "";
        public string signature = "";   // 材料署名：主笔/参与/挂名
        public bool flagged;            // 程序合规标记（监察线数据基础）
    }

    [Serializable]
    public class DocRecord
    {
        public string date = "", title = "", signature = "", note = "";
    }

    [Serializable]
    public class IntegrityRecord
    {
        public string date = "", tag = "", note = "";
    }

    [Serializable]
    public class CommendRecord
    {
        public string date = "", text = "";
    }

    [Serializable]
    public class MemoryEntry
    {
        public string date = "", text = "";
    }

    [Serializable]
    public class RelationEntry
    {
        public string id = "";
        public int familiar;            // 0-100
        public int trust;               // -100~100
        public int evalv;               // -100~100（对玩家的评价）
        public List<MemoryEntry> memories = new List<MemoryEntry>();
    }

    [Serializable]
    public class LogEntry
    {
        public string date = "", kind = "", text = "";
    }

    [Serializable]
    public class FlagEntry
    {
        public string key = "";
        public bool value;
    }

    [Serializable]
    public class WeekState
    {
        public int index = 1;
        public string focus;            // work/study/social/rest
        public List<TaskRecord> tasks = new List<TaskRecord>();
        public bool reviewed;
    }

    [Serializable]
    public class YearEval
    {
        public int year;
        public string grade = "称职";
    }

    /// <summary>
    /// 选项条件。结构性门槛（grade/route/flag/notFlag/年限/考核）不满足时选项隐藏；
    /// 叙事性门槛（mark/notMark/relNpc+好感信任）不满足时选项锁定可见并给原因（Disco 式可读性）。
    /// </summary>
    [Serializable]
    public class OptionWhen
    {
        public string grade;            // 需要的职级（前缀匹配，如 "吏一"）
        public string route;            // 需要的路线
        public string flag;             // 需要的标记
        public string notFlag;          // 需要没有的标记
        public int minGradeYears;       // 现职级任职年限（整年）
        public int minOutstanding;      // 考核优秀次数下限
        public int minBaseExpMonths;    // 基层履历月数下限
        // —— 叙事性门槛（Phase 4：锁定可见）——
        public string mark;             // 需要的叙事标记（st.marks）
        public string notMark;          // 需要没有的叙事标记
        public string relNpc;           // 关系门槛对象（NPC id）
        public int minFamiliar;         // 熟悉度下限（>0 生效）
        public int minTrust;            // 信任下限（>0 生效）
    }

    // ---------------- Phase 4 新状态结构（存档 v4）----------------

    [Serializable]
    public class EchoJob
    {
        public string eventId = "";
        public string dueOn = "";       // ISO 日期；到期由 CollectDue 入队（选择的长影子）
    }

    [Serializable]
    public class ClockState
    {
        public string id = "";
        public string label = "";       // 仪表显示名（如"专项调研""巡视组"）
        public int value;
        public int max = 5;
        public string kind = "opportunity"; // opportunity/threat
        public string onFullEventId = "";   // 满格触发的事件 id
        public int dailyRate;               // 自然推进速率/天（可为负）
    }

    [Serializable]
    public class WeekPlanState
    {
        // 精力槽位分配（P4.2 周计划系统；和≈100）
        public int work = 40, study = 15, social = 15, family = 10, rest = 20;
        public bool hasPlan;            // 本周是否已定计划
    }

    [Serializable]
    public class RivalState
    {
        public string id = "xu";        // 竞争者 NPC id
        public int progress;            // 晋升竞争力 0-100（P4.2 仪表化）
        public int stage;               // 里程碑阶段（ContentRival 推进）
    }

    [Serializable]
    public class EchoSpec
    {
        public string eventId = "";
        public int afterDays = 7;
    }

    [Serializable]
    public class ClockOp
    {
        public string id = "";
        public string label = "";
        public int max = 5;
        public string kind = "opportunity";
        public string onFull = "";
        public int delta;               // 推进量（0=只建立/更新）
        public int dailyRate;
        public bool remove;
    }

    [Serializable]
    public class EndingData
    {
        public string title = "";
        public List<string> paras = new List<string>();
    }

    [Serializable]
    public class MonthState
    {
        public string key = "2026-08";
        public List<TaskRecord> tasks = new List<TaskRecord>();
    }

    [Serializable]
    public class GameState
    {
        public int saveVersion = 5;                                 // 存档结构版本（v5=Phase 5 七品市长+卷宗）
        public Phase phase = Phase.Prologue;
        public string date = "2026-09-01";
        public List<string> queue = new List<string>();      // 今日待触发事件 id
        public string currentEvent;                          // 呈现中的事件 id（可断点续显）
        public bool hasPending;                              // 结果等待“继续”
        public string pendingTitle = "";
        public List<string> pendingParas = new List<string>();

        public PlayerState player = new PlayerState();
        public List<TaskRecord> tasks = new List<TaskRecord>();              // 工作档案·任务
        public List<DocRecord> documents = new List<DocRecord>();            // 工作档案·材料
        public List<IntegrityRecord> integrity = new List<IntegrityRecord>();// 工作档案·程序合规
        public List<CommendRecord> commendations = new List<CommendRecord>();// 工作档案·奖惩
        public List<RelationEntry> relations = new List<RelationEntry>();
        public List<FlagEntry> flags = new List<FlagEntry>();
        public List<LogEntry> log = new List<LogEntry>();
        public WeekState week = new WeekState();
        public MonthState month = new MonthState();
        public List<string> fired = new List<string>();      // 已触发事件 id

        // —— 职业生涯（Phase 5：七品市长）——
        public string grade = "七品·正厅";                    // 七品·正厅 → 六品·副省
        public string gradeSince = "2026-09-01";
        public string route = "";                             // 产业转型/民生兜底/项目攻坚/向上争取
        public string seconded = "";
        public int baseExpMonths = 240;                       // 最快轨履历（已具备，非科员积累）
        public bool examPassed;                               // 帝国考试（高等级）——就任时已完成
        public bool academyDone;                              // 政治学院结业——就任时已完成
        public int outstandingYears;                          // 考核优秀次数
        public List<YearEval> evals = new List<YearEval>();   // 年度考核记录
        public int violationCount;                            // 程序违规累计
        public int termYears;                                 // 现届已任年数（5 年一届）

        // —— 生活副线 ——
        public string partner = "林晚";
        public bool married = true, hasChild = true;
        public string housing = "市政府周转房";

        // —— 结局 ——
        public bool resigned, underInvestigation;
        public EndingData endingData;

        // —— 年度统计（次年1月年度考核用，考核后清零）——
        public int yearGradePoints, yearTaskCount, yearIntegrity;

        // —— AI 增强（LlmGameplay）：当日已生成过 AI 小事件的日期，防止重复 ——
        public string lastAiMicro = "";
        public string lastLetterMonth = "";  // AI 家信：上次生成的年月 "yyyy-MM"

        // —— Phase 4：叙事引擎状态 ——
        public List<string> marks = new List<string>();              // 叙事标记（跨事件记忆，持久化）
        public List<EchoJob> echoQueue = new List<EchoJob>();        // 延迟回响队列
        public List<ClockState> clocks = new List<ClockState>();     // 机会/威胁时钟仪表
        public WeekPlanState plan = new WeekPlanState();             // 周计划（P4.2）
        public RivalState rival = new RivalState();                  // 同批竞争者（P4.2）
        public string ambition = "";                                 // 开局志向（P4.4：做事/晋升/安稳/搞钱）
        public List<string> poolLog = new List<string>();            // 随机池触发流水 "id@iso"（冷却用，截断保留）

        // —— Phase 5：卷宗与两把尺 ——
        public int compliance = 100;                           // 合规分 0-100
        public int efficiency = 70;                            // 效率分 0-100
        public List<string> pendingDossierIds = new List<string>();   // 本周待办卷宗 id
        public List<string> openDossierIds = new List<string>();      // 仍打开（未办结）
        public ActiveDossier activeDossier;                    // 正在办理
        public List<DossierLogEntry> dossierLog = new List<DossierLogEntry>();
        public List<string> dossierFired = new List<string>(); // 已使用卷宗实例 id
        public int weekDossierBudget = 5;                      // 本周件数上限
        public List<string> knownRules = new List<string>();   // 已获口径（档案全集）
        public List<string> deskRules = new List<string>();    // 案头槽位（上限 4，新件顶掉最旧）
        public string openRule = "";                           // 案头摊开的口径（空=未摊开）

        // —— 结算数据（参与存档，防止读档空引用）；runtimeEvent 为单回合临时事件，不入档 ——
        [NonSerialized] public GameEvent runtimeEvent;
        public WeekEndData weekEndData;
        public MonthEndData monthEndData;

        // ---- 便捷访问 ----
        public bool GetFlag(string key)
        {
            for (int i = 0; i < flags.Count; i++) if (flags[i].key == key) return flags[i].value;
            return false;
        }

        public void SetFlag(string key, bool value)
        {
            for (int i = 0; i < flags.Count; i++)
                if (flags[i].key == key) { flags[i].value = value; return; }
            flags.Add(new FlagEntry { key = key, value = value });
        }

        public bool PopFlag(string key)
        {
            bool v = GetFlag(key);
            if (v) SetFlag(key, false);
            return v;
        }

        public bool Fired(string id) => fired.Contains(id);
        public void MarkFired(string id) { if (!fired.Contains(id)) fired.Add(id); }

        // —— 叙事标记（st.marks）——
        public bool HasMark(string m) => !string.IsNullOrEmpty(m) && marks.Contains(m);
        public void Mark(string m) { if (!string.IsNullOrEmpty(m) && !marks.Contains(m)) marks.Add(m); }
        public void Unmark(string m) { if (!string.IsNullOrEmpty(m)) marks.Remove(m); }

        // —— 池事件可重复触发计数：fired 里存 "id"（首次）与 "id#1"、"id#2"…（重复） ——
        public int FireCount(string id)
        {
            int c = 0;
            for (int i = 0; i < fired.Count; i++)
            {
                var f = fired[i];
                if (f == id) c++;
                else if (f.Length > id.Length && f[id.Length] == '#' && f.StartsWith(id)) c++;
            }
            return c;
        }

        public void MarkFireAdditional(string id)
        {
            int c = FireCount(id);
            fired.Add(c == 0 ? id : id + "#" + c);
        }

        public void AddLog(string kind, string text)
        {
            log.Add(new LogEntry { date = date, kind = kind, text = text });
            // 日志上限：十年长档防无限增长（周末回顾/月度大事记只读最近若干条，裁剪无感）
            if (log.Count > 600) log.RemoveRange(0, 100);
        }
    }

    // ---------------- 效果与事件 ----------------

    [Serializable]
    public class RelDelta
    {
        public string id;
        public int familiar, trust, evalv;
        public string memo;             // 写入 NPC 记忆
    }

    [Serializable]
    public class Effects
    {
        public int professional, admin, exec, comm, political;   // 五维增量
        public int energy, stress, morale;
        public int reputation, polCapital;
        public int moneyDelta;
        public List<RelDelta> rel = new List<RelDelta>();
        public List<string> setFlags = new List<string>();
        public TaskRecord task;                                  // 有值＝产生任务档案
        public DocRecord document;
        public IntegrityRecord integrity;
        public CommendRecord commend;
        public string logKind, logText;
        public bool gotoWork;                                    // 序章结束→进入入职
        public string gradeTo;                                   // 职级变更（吏二·副科 等）
        public string route;                                     // 路线选择
        public string seconded;                                  // 借调/挂职/学习变动（""=回局）
        public string partner;                                   // 确立恋爱/婚姻对象
        public int baseExpDelta;                                 // 基层履历月数增量
        public string ambition;                                  // 志向确立（P4.4：做事/晋升/安稳/搞钱）
        public string housing;                                   // 住房变动（P4.4 房子链）
        public string evalGrade;                                 // 年度考核等第（配合clearYearStats）
        public bool clearYearStats;                              // 推入考核记录并清零年度统计
        public bool ending;                                      // 进入结局画面
        public bool resigned;                                    // 主动辞职（结局）
        public bool underInvestigation;                          // 立案审查（结局）
        public bool marry;                                       // 结婚
        public bool child;                                       // 生育
        // —— Phase 4：叙事效果 ——
        public List<string> setMarks = new List<string>();       // 写入叙事标记
        public List<string> clearMarks = new List<string>();     // 清除叙事标记
        public List<EchoSpec> echoes = new List<EchoSpec>();     // 延迟回响（N 天后入队事件）
        public List<ClockOp> clockOps = new List<ClockOp>();     // 时钟建立/推进/移除

        public bool IsEmpty()
        {
            return professional == 0 && admin == 0 && exec == 0 && comm == 0 && political == 0
                && energy == 0 && stress == 0 && morale == 0 && reputation == 0 && polCapital == 0
                && moneyDelta == 0 && (rel == null || rel.Count == 0)
                && (setFlags == null || setFlags.Count == 0)
                && (setMarks == null || setMarks.Count == 0)
                && (clearMarks == null || clearMarks.Count == 0)
                && (echoes == null || echoes.Count == 0)
                && (clockOps == null || clockOps.Count == 0)
                && task == null && document == null && integrity == null && commend == null
                && string.IsNullOrEmpty(logText) && !gotoWork;
        }
    }

    [Serializable]
    public class Check
    {
        public string main;         // professional/admin/exec/comm/political
        public float bonus;         // -0.5 ~ 0.5 加成
    }

    [Serializable]
    public class When
    {
        public string date;         // 精确日期触发（ISO）
        public string md;           // 每年循环触发（"MM-DD"，配合fromYear）
        public int fromYear;        // 循环触发的起始年（默认2027）
        public string flag;         // 标记触发（flag 为 true 且未触发过）
        public string route;        // 路线门槛（date 触发时附加）
        public double randomP;      // >0 = 进入工作日随机池（Phase 4 起为候选池，按 weight 抽取）
        public bool weekdaysOnly = true;

        // —— Phase 4：随机池由“触发即删”改为“加权抽取 + 次数 + 冷却 + 标记门槛” ——
        public double weight;           // 抽取权重（>0 生效，默认 1）
        public int maxFires;            // 最大触发次数（默认 1 = 一生一次；>1 可复现）
        public int cooldownDays;        // 两次触发最小间隔（0 = 不冷却）
        public string[] requireMarks;   // 需要的全部叙事标记
        public string[] requireNotMarks;// 需要没有的叙事标记
        public string ambition;         // 志向门槛（P4.4）
    }

    [Serializable]
    public class EventOption
    {
        public string label;
        public OptionWhen when;     // 可选：出现条件
        public Check check;         // 可选：能力检定（结果填入 task.grade 并替换 result 中 {grade}）
        public Effects effects;
        public string result;       // 结果文本，支持 {grade} 占位
    }

    [Serializable]
    public class GameEvent
    {
        public string id, type, title;
        public bool dynamic;        // 触发时由引擎按当前状态动态构建（人事窗口/年度考核/结局）
        public When when;
        public List<string> paras = new List<string>();
        public List<EventOption> options = new List<EventOption>();
    }

    /// <summary>呈现层场景（只读视图）。optionLocks 与 options 平行；非空项为锁定原因（UI 应置灰）。</summary>
    public class Scene
    {
        public string kind, title;
        public List<string> paras = new List<string>();
        public List<string> options = new List<string>();
        public List<string> optionLocks;                    // null = 全部可点（Phase 4 锁定选项）
        public int[] planValues;                            // week_plan 场景：五槽位当前值（岗位/学习/人际/家庭/休整）
        public string docNo;                                // 红头文件文号（非空=按公文样式渲染事件）
    }

    // ---------------- 结算数据 ----------------

    [Serializable]
    public class WeekEndData
    {
        public List<TaskRecord> tasks = new List<TaskRecord>();
        public string avgGrade = "—";
        public int zhouDelta;
        public Attrs growth = new Attrs();
        public int stressDelta;
        public int moraleDelta;         // 周计划·家庭槽带来的士气（Phase 4）
        public string peerLine = "";
    }

    [Serializable]
    public class MonthEndData
    {
        public string monthLabel;
        public int tasksTotal, best, worst;
        public int netIncome;
        public int probation;
        public int integrityCount;
        public string flavor;
        public string monthDigest;      // 本月大事记（Phase 4 上下文化）
    }

    // ---------------- Phase 5：卷宗（Papers Please）----------------

    [Serializable]
    public class DossierIssue
    {
        public string id = "";
        public string pageRef = "";     // 藏在哪一页（1 起）
        public string detectHint = "";  // 口径/关联线索（不直接剧透结论）
        public string ruleKey = "";     // 对应 Rulebook 条目 id（案头摊开可加成）
        public int severity = 1;        // 1-3
        public bool discovered;         // 玩家已核出
    }

    [Serializable]
    public class DossierPage
    {
        public string title = "";
        public List<string> paras = new List<string>();
        public string table = "";       // 可选：程序表格文本（制表符/竖线分隔）
    }

    [Serializable]
    public class DossierOption
    {
        public string label = "";
        public string whenMark = "";    // 叙事门槛（空=总是）
        public string whenNotMark = "";
        public string lockReason = "";  // 锁定时显示
        public Effects effects;
        public string result = "";
        public int complianceDelta;     // 该处置对合规分的直接修正（主雷另算）
        public int efficiencyDelta;
        public bool gray;               // 灰区选项
    }

    [Serializable]
    public class Dossier
    {
        public string id = "";
        public string kind = "routine"; // routine|deadline|cosign|risk|showcase
        public string form = "请示";    // 请示|会议材料|人事单|财政件|土地件|信访件|突发事件|省交办|巡视整改|协议|对上报告
        public string title = "";
        public string docNo = "";
        public string org = "";         // 来文单位
        public string deadline = "";    // ISO；空=本周内
        public List<DossierPage> pages = new List<DossierPage>();
        public List<DossierIssue> issues = new List<DossierIssue>();
        public List<DossierOption> options = new List<DossierOption>();
        public int checkBudget = 3;     // 核对次数
        public bool generated;          // 模板生成（非手写）
    }

    [Serializable]
    public class DossierLogEntry
    {
        public string date = "";
        public string dossierId = "";
        public string title = "";
        public string disposition = ""; // 照准/退回/请示/会签/压下/特事特办
        public int issuesMissed;        // 未查出的雷数
        public int issuesFound;
        public int complianceDelta;
        public int efficiencyDelta;
        public bool overdue;
    }

    [Serializable]
    public class ActiveDossier
    {
        public string id = "";
        public int page = 1;            // 当前页（1 起）
        public int checksLeft = 3;
        public List<string> foundIssues = new List<string>();
        public bool resolved;
        public string pendingResultTitle = "";
        public List<string> pendingResultParas = new List<string>();
    }
}
