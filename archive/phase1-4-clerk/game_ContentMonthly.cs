using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// Phase 4 月度主题：一年十二种“节律”，让每个月有自己的性格与事件。
    /// md 循环触发（fromYear 错峰），选项写短弧效果——月月不同，年复一年有变奏。
    /// </summary>
    public static class ContentMonthly
    {
        public static void Register()
        {
            // 1月：总结季（从2027起；年度考核另有 sys_annual_eval 在 1/15）
            Flow.Register(new GameEvent
            {
                id = "mo_jan", type = "work", title = "一月 · 总结季",
                when = new When { md = "01-08", fromYear = 2027 },
                paras = new List<string>
                {
                    "一月的主旋律是总结。科室总结、个人总结、党小组总结——把上一年的三百六十五天，压缩成几页纸的功劳簿。",
                    "笔杆子们的旺季到了，走廊里打印机就没停过。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="帮科里消化两份总结初稿",
                        check=new Check{ main="professional", bonus=0.05f },
                        effects=new Effects{ energy=-8, professional=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } },
                            task=new TaskRecord{ title="年度总结把关", note="一月总结季", signature="主笔" } },
                        result="{grade}。你把三份千篇一律的稿子改出了三副面孔——写总结的最高境界，是让平凡的一年听起来非同寻常。" },
                    new EventOption{ label="只写好自己的那份",
                        effects=new Effects{ energy=-4, admin=1 },
                        result="你自己的总结写了改、改了写，最后删掉所有形容词交上去。{grade}。" },
                },
            });

            // 2月：开工季（春节后）
            Flow.Register(new GameEvent
            {
                id = "mo_feb", type = "work", title = "二月 · 收心与开工",
                when = new When { md = "02-20", fromYear = 2027 },
                paras = new List<string>
                {
                    "年味还没散尽，收心会先开了。周衡之在会上念完全年工作要点，末了加了一句：“元宵之前，把心思从饺子上挪回来。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="开工第一周就拿出状态，把新年第一份材料打成样",
                        check=new Check{ main="exec", bonus=0.05f },
                        effects=new Effects{ energy=-8, exec=1, reputation=1,
                            task=new TaskRecord{ title="年度首份急件", note="开年立标杆", signature="主笔" } },
                        result="{grade}。开年第一炮，全科的年都顺了三分——机关里的“开门红”，信的人多了就成了真的。" },
                    new EventOption{ label="按部就班进入节奏",
                        effects=new Effects{ energy=-4, morale=1 },
                        result="你给自己留了半个月的缓冲。年还长，不急在一周。{grade}。" },
                },
            });

            // 3月：规划月
            Flow.Register(new GameEvent
            {
                id = "mo_mar", type = "work", title = "三月 · 规划月",
                when = new When { md = "03-05", fromYear = 2027 },
                paras = new List<string>
                {
                    "三月的机关属于规划。五年规划的中期评估、年度计划的分解下达、专项方案的征求意见——每一份文件后面都是一串会议。",
                    "你负责的那摊子里，有一场跨部门协调会要组织。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把协调会开出效率：议程精确到分钟",
                        check=new Check{ main="admin", bonus=0.05f },
                        effects=new Effects{ energy=-9, admin=1, reputation=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="lin", familiar=1, evalv=1 } },
                            task=new TaskRecord{ title="跨部门协调会组织", note="三月规划月", signature="主笔" } },
                        result="{grade}。四十分钟的会开出了四十分钟的成效，与会处室的评价是：“综合科的会，不磨叽。”" },
                    new EventOption{ label="让各方先私下通气，会上只过纸面",
                        check=new Check{ main="comm", bonus=0.1f },
                        effects=new Effects{ energy=-7, comm=1, political=1,
                            task=new TaskRecord{ title="跨部门协调会组织", note="会下功夫", signature="参与" } },
                        result="{grade}。真正的会从来不在会议室里开。你把分歧解决在了饭桌上和电话里——这是林晚教的，管用。" },
                },
            });

            // 4月：调研季
            Flow.Register(new GameEvent
            {
                id = "mo_apr", type = "work", title = "四月 · 下调研",
                when = new When { md = "04-08", fromYear = 2027 },
                paras = new List<string>
                {
                    "春天适合往下跑。局里的调研计划排到了六月，你跟队去区县看重点项目——大巴车、安全帽、和一路的Excel表。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="多往车间和工地钻，少坐会议室",
                        check=new Check{ main="exec", bonus=0.05f },
                        effects=new Effects{ energy=-12, exec=1, professional=1,
                            task=new TaskRecord{ title="重点项目实地调研", note="一线跑出来的记录", signature="参与" } },
                        result="{grade}。你在噪声里听完了企业负责人的真话——会议室里那种话，八成是排练过的。" },
                    new EventOption{ label="跟紧材料组，把纪要做扎实",
                        effects=new Effects{ energy=-8, admin=1,
                            task=new TaskRecord{ title="调研纪要", note="随队材料员", signature="参与" } },
                        result="{grade}。一天三个点、两万字速记。老科员说：材料员的腿，是机关的腿。" },
                },
            });

            // 5月：竞赛季
            Flow.Register(new GameEvent
            {
                id = "mo_may", type = "society", title = "五月 · 劳动竞赛",
                when = new When { md = "05-08", fromYear = 2027 },
                paras = new List<string>
                {
                    "五一之后，市里发文组织机关业务技能竞赛：公文写作、数据分析、政策宣讲三个赛道。科室要报人。",
                    "周衡之的目光在科里转了一圈。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="报名——奖项是小事，露脸是大事",
                        check=new Check{ main="professional", bonus=0.1f },
                        effects=new Effects{ energy=-12, professional=1, reputation=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } },
                            task=new TaskRecord{ title="机关技能竞赛", note="科里出征", signature="主笔" } },
                        result="{grade}。决赛那天你在台上把政策讲出了评书味，拿了个三等奖——奖不大，名传出去了。" },
                    new EventOption{ label="把机会让给更需要的同事",
                        effects=new Effects{ morale=1, rel=new List<RelDelta>{ new RelDelta{ id="zhao", familiar=2 } } },
                        result="赵姐替你去比的，回来给你带了盒点心：“替你露的脸，回头得还。”人情账又记了一笔。" },
                },
            });

            // 6月：半年冲刺
            Flow.Register(new GameEvent
            {
                id = "mo_jun", type = "work", title = "六月 · 双过半",
                when = new When { md = "06-10", fromYear = 2027 },
                paras = new List<string>
                {
                    "“时间过半、任务过半”——六月的红线挂在每一个科室的进度表上。落后的口子在全科会上被点名。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="主动认领一个滞后的指标",
                        check=new Check{ main="exec", bonus=0.05f },
                        effects=new Effects{ energy=-12, stress=2, exec=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=1, evalv=1 } },
                            task=new TaskRecord{ title="滞后指标攻坚", note="双过半冲刺", signature="主笔" } },
                        result="{grade}。月底进度表翻红的时候，周衡之在全科会上说了句“年轻人顶上来了”。这个月值。" },
                    new EventOption{ label="把自己的摊子守好",
                        effects=new Effects{ energy=-7, admin=1 },
                        result="你的表是绿的。别人家的红旗跟你关系不大——科室是个整体，但责任是分格的。{grade}。" },
                },
            });

            // 7月：防汛值班
            Flow.Register(new GameEvent
            {
                id = "mo_jul", type = "society", title = "七月 · 汛期",
                when = new When { md = "07-12", fromYear = 2027 },
                paras = new List<string>
                {
                    "秦岭北麓的雨下了三天，渭河的水位涨了。市里启动防汛值班，机关干部排班上堤——发改局也要出人。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="主动报一个夜班",
                        effects=new Effects{ energy=-10, morale=2, political=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="ma", evalv=1, memo="汛期夜班，主动报名" } },
                            logKind="工作", logText="汛期上堤值班一夜" },
                        result="凌晨三点的堤坝上，手电的光柱扫过水面。马建国巡查路过，看了你一眼——没说话。第二天你在值班表上看到自己被批了调休。" },
                    new EventOption{ label="守好机关的物资调度台账",
                        effects=new Effects{ energy=-7, admin=1,
                            task=new TaskRecord{ title="防汛物资台账", note="汛期保障", signature="主笔" } },
                        result="帐篷、沙袋、抽水泵的数字在你手里进出。一线在外头，你在账上——账清，前线才稳。{grade}。" },
                },
            });

            // 8月：休整季
            Flow.Register(new GameEvent
            {
                id = "mo_aug", type = "society", title = "八月 · 年假",
                when = new When { md = "08-06", fromYear = 2027 },
                paras = new List<string>
                {
                    "八月，机关的淡季。年假条一张张递上去，办公室里的人肉眼可见地少了一圈。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="休满五天年假，出一趟远门",
                        effects=new Effects{ energy=18, stress=-10, morale=5, moneyDelta=-2500,
                            logKind="生活", logText="年假出远门，回来像换了个人" },
                        result="你去了南方的一个小城，睡到自然醒，看了三场落日。回来的高铁上你忽然想通了一件搁置很久的事——假期的作用就是这个。" },
                    new EventOption{ label="把假攒着，错峰再休",
                        effects=new Effects{ energy=6, reputation=1 },
                        result="你留下顶了别人的活。科里的情分就是这样攒出来的——但你的疲劳账户也在悄悄透支。" },
                },
            });

            // 9月：人事季前奏（9/20 有 sys_personnel）
            Flow.Register(new GameEvent
            {
                id = "mo_sep", type = "person", title = "九月 · 人事季的风",
                when = new When { md = "09-05", fromYear = 2027 },
                paras = new List<string>
                {
                    "九月的风里有人事季的味道。档案室的灯提前亮了，任雪梅的脚步声比平时急，走廊里的寒暄都带着试探。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="照常干活，不打听",
                        effects=new Effects{ morale=1, political=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="ren", trust=1, memo="人事季不乱打听，稳得住" } } },
                        result="任雪梅路过你工位时多看了你一眼——不打听的人，在她那里是加分项。" },
                    new EventOption{ label="向何斌打听风向",
                        effects=new Effects{ political=2, stress=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="he", familiar=2 } } },
                        result="何斌的消息又快又全，真假掺半。你听了一耳朵，删掉了一半，记下了一半。" },
                },
            });

            // 10月：预算季
            Flow.Register(new GameEvent
            {
                id = "mo_oct", type = "work", title = "十月 · 预算季",
                when = new When { md = "10-12", fromYear = 2027 },
                paras = new List<string>
                {
                    "下一年度的预算编制启动。各处室的申报表像雪片一样飞进综合科——每一个数字后面都是一场谈判。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="逐笔核减明显虚高的申报",
                        check=new Check{ main="admin", bonus=0.05f },
                        effects=new Effects{ energy=-10, admin=1, stress=1,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } },
                            task=new TaskRecord{ title="预算申报初审", note="十月预算季", signature="主笔" } },
                        result="{grade}。你核减了三百多万——也有两个处长在电话里不太客气。预算季的得罪，是工作的一部分。" },
                    new EventOption{ label="照单汇总，把矛盾交给领导",
                        effects=new Effects{ energy=-5, exec=1 },
                        result="你把争议项标黄上报。流程没错——只是周衡之看到那几个黄标时，眉头跳了一下。{grade}。" },
                },
            });

            // 11月：审计季
            Flow.Register(new GameEvent
            {
                id = "mo_nov", type = "oversight", title = "十一月 · 审计进点",
                when = new When { md = "11-10", fromYear = 2027 },
                paras = new List<string>
                {
                    "审计组进点。调阅清单开出来，综合科的档案柜首当其冲——每一份签字、每一张纪要，都可能被重新翻出来看。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="提前自查，把不规范的痕迹主动补正",
                        check=new Check{ main="admin", bonus=0.05f },
                        effects=new Effects{ energy=-10, stress=2, admin=1,
                            task=new TaskRecord{ title="档案自查补正", note="审计季", signature="主笔" } },
                        result="{grade}。你把三处格式瑕疵、一处签批缺项补齐了。审计翻阅时一页页顺畅——自查的夜没白熬。" },
                    new EventOption{ label="兵来将挡，不预演",
                        effects=new Effects{ energy=-4, stress=3 },
                        result="审计问到一处口径时你翻了五分钟才找到依据。不算错，但不算体面。{grade}。" },
                },
            });

            // 12月：年终冲刺（12/10 有 sys_yearend_rush）
            Flow.Register(new GameEvent
            {
                id = "mo_dec", type = "work", title = "十二月 · 收官",
                when = new When { md = "12-05", fromYear = 2027 },
                paras = new List<string>
                {
                    "年底收官。台账要齐、材料要结、承诺要兑现——十二月机关的灯，是一年里熄得最晚的。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把全年台账亲手理一遍，查漏补缺",
                        check=new Check{ main="admin", bonus=0.05f },
                        effects=new Effects{ energy=-12, admin=1, stress=2,
                            task=new TaskRecord{ title="年度台账归档", note="十二月收官", signature="主笔" } },
                        result="{grade}。台账码得整整齐齐，像给这一年磕了个头。赵姐翻完说了句：“这孩子的账，我放心。”" },
                    new EventOption{ label="抓大放小，保证重点件结办",
                        effects=new Effects{ energy=-8, exec=1 },
                        result="重点件全结，碎账留了尾巴——反正一月总结季还得再理一遍。{grade}。" },
                },
            });
        }
    }
}
