using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>周期性系统事件（春节/中秋/宪法日/开国纪念日/年终冲刺/人事窗口/年度考核/十年之约）＋职业链事件。</summary>
    public static class ContentCareer
    {
        public static void Register()
        {
            RegisterSystemRecurring();
            RegisterCareerChain();
        }

        // ---------------- 周期性系统事件 ----------------

        private static void RegisterSystemRecurring()
        {
            // 系统动态事件：人事窗口（每年9月）、年度考核（每年1月）、十年之约（2036-08-27）
            Flow.Register(new GameEvent { id = "sys_personnel", type = "person", title = "人事窗口", dynamic = true, when = new When { md = "09-20", fromYear = 2027 }, paras = new List<string>(), options = new List<EventOption> { new EventOption { label = "继续" } } });
            Flow.Register(new GameEvent { id = "sys_annual_eval", type = "politics", title = "年度考核", dynamic = true, when = new When { md = "01-15", fromYear = 2027 }, paras = new List<string>(), options = new List<EventOption> { new EventOption { label = "继续" } } });
            Flow.Register(new GameEvent { id = "sys_ending", type = "system", title = "十年之约", dynamic = true, when = new When { date = "2036-08-27" }, paras = new List<string>(), options = new List<EventOption> { new EventOption { label = "继续" } } });

            // 春节（每年除夕日，文案取自 YearInfo）
            var cnyEve = new Dictionary<int, string>
            {
                [2027] = "2027-02-05", [2028] = "2028-01-25", [2029] = "2029-02-12", [2030] = "2030-02-02",
                [2031] = "2031-01-22", [2032] = "2032-02-10", [2033] = "2033-01-30", [2034] = "2034-02-18",
                [2035] = "2035-02-07", [2036] = "2036-01-27",
            };
            foreach (var kv in cnyEve)
            {
                int y = kv.Key;
                Flow.Register(new GameEvent
                {
                    id = $"sys_cny_{y}", type = "society", title = $"{y - 1}—{y}年 · 春节",
                    when = new When { date = kv.Value },
                    paras = new List<string>
                    {
                        "春节。长安的写字楼第一次比城中村安静。",
                        ContentRegistry.Years[y].cny,
                    },
                    options = new List<EventOption>
                    {
                        new EventOption{ label="回家过年",
                            effects=new Effects{ morale=5, energy=8, logKind="人物", logText=$"{y}年春节回父母家" },
                            result="高铁四十分钟，就把“长安人”变回了“孩子”。返程时母亲往你包里塞的腊牛肉，够吃到元宵。" },
                        new EventOption{ label="值班，让家远的同事回去",
                            effects=new Effects{ energy=-6, reputation=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1, memo=$"{y}春节替同事值班" } }, logKind="工作", logText=$"{y}年春节值班" },
                            result="值班室、一杯茶、全城的路灯。你在值班日志上写下“一切正常”——这是机关人独有的年味。" },
                        new EventOption{ label= y>=2033 ? "带着家人在长安过年" : "留在宿舍，把年过得安静些",
                            effects=new Effects{ morale=3, energy=6 },
                            result= y>=2033 ? "孩子在阳台数烟花，你在厨房洗碗——年味变了形状，没变温度。" : "一个人的年夜饭，你点了份饺子外卖，给父母打了个长长的视频电话。" },
                    },
                });
            }

            // 中秋（2027—2036，占位日期）
            var midAutumn = new Dictionary<int, string>
            {
                [2027] = "2027-09-15", [2028] = "2028-09-22", [2029] = "2029-09-21", [2030] = "2030-09-11",
                [2031] = "2031-09-30", [2032] = "2032-09-19", [2033] = "2033-09-08", [2034] = "2034-09-26",
                [2035] = "2035-09-16",
            };
            foreach (var kv in midAutumn)
            {
                int y = kv.Key;
                Flow.Register(new GameEvent
                {
                    id = $"sys_midautumn_{y}", type = "society", title = $"{y}年 · 中秋节",
                    when = new When { date = kv.Value },
                    paras = new List<string>
                    {
                        "中秋节。“长安印月”的月饼盒换了新包装，月亮还是那一枚——照过城墙，也照过你家阳台。",
                    },
                    options = new List<EventOption>
                    {
                        new EventOption{ label="与家人朋友团聚",
                            effects=new Effects{ morale=4, energy=5 },
                            result="月圆之夜，人间小聚。你把工作的手机静了音——这是你对节日最大的诚意。" },
                        new EventOption{ label="单位值班备勤",
                            effects=new Effects{ energy=-6, reputation=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } }, logKind="工作", logText=$"{y}中秋值班" },
                            result="又是一年值班的中秋。保安老王给你留了块月饼——五仁的，你居然吃出了乡愁。" },
                    },
                });
            }

            // 宪法日（每年3-11）
            Flow.Register(new GameEvent
            {
                id = "sys_constitution", type = "politics", title = "宪法日",
                when = new When { md = "03-11", fromYear = 2027 },
                paras = new List<string>
                {
                    "宪法日。纪念1986年3月颁布的《中华帝国宪法》——“第二次建国”的成果，写进了十个条文。",
                    "全局组织宪法宣誓墙参观。你路过那面墙时，照例停了三秒——程序、边界、纠错，这三个词你一年比一年懂。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="在留言簿上写一句话",
                        effects=new Effects{ political=1, morale=1 },
                        result="你写的是：“权力可以集中，但必须能够被制度纠错。”——开国皇帝六信念的第一条，也是你入行那年抄在本子上的第一句话。" },
                    new EventOption{ label="低头赶材料，匆匆经过",
                        effects=new Effects{ energy=0 },
                        result="宪法日也要交材料。你从宣誓墙前快步走过——仪式在心里，不必在纸上。" },
                },
            });

            // 开国纪念日（每年10-08）
            Flow.Register(new GameEvent
            {
                id = "sys_nationalday", type = "politics", title = "开国纪念日",
                when = new When { md = "10-08", fromYear = 2026 },
                paras = new List<string>
                {
                    "开国纪念日。纪念1700年制度元年——太祖朱承乾在这一年立下“让制度不断纠错”的总章程。",
                    "皇宫广场举行阅兵，电视里军靴铿锵。你在长安的街头，能感觉到这座首都在这一天特有的庄重。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="观看阅兵直播",
                        effects=new Effects{ morale=3, political=1 },
                        result="受阅方队走过城墙的那一刻，你想起档案扉页那句国训。四百年的制度，今天是它的生日。" },
                    new EventOption{ label="假期加班赶件",
                        effects=new Effects{ energy=-10, reputation=1, exec=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } } },
                        result="全国放假，你的键盘没停。晚上领导发来四个字：“辛苦，可删。”——机关里最高级的表扬，是“可删”。" },
                },
            });

            // 年终冲刺（每年12月中）
            Flow.Register(new GameEvent
            {
                id = "sys_yearend_rush", type = "work", title = "年终冲刺",
                when = new When { md = "12-10", fromYear = 2026 },
                paras = new List<string>
                {
                    "年终冲刺季。总结、台账、考评表、来年计划——四线作战，全楼的打印机 进入一年中最响的两周。",
                    "周衡之在科务会上只说了一句：“年底的活，是给全年画句号的活。句号要圆。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把每份材料都过三遍",
                        check=new Check{ main="admin", bonus=0.1f },
                        effects=new Effects{ energy=-16, stress=4, admin=1, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=2 } },
                            task=new TaskRecord{ title="{年}年年终材料会战", note="三审三校", signature="主笔" } },
                        result="句号画得很圆。周衡之把你那份总结直接转发给副局长——一字未改。{grade}。" },
                    new EventOption{ label="抓大放小，先保重点",
                        check=new Check{ main="exec", bonus=0.05f },
                        effects=new Effects{ energy=-12, stress=3, exec=1,
                            task=new TaskRecord{ title="{年}年年终材料会战", note="重点优先", signature="主笔" } },
                        result="你把三份急件按顺序排开，一份份啃完。年底的办公室，效率就是慈悲。{grade}。" },
                },
            });

            // 监察线：低频灰度抉择（Q5-03：低频、可拒）
            Flow.Register(new GameEvent
            {
                id = "sys_grey_favor", type = "oversight", title = "熟人的请托",
                when = new When { md = "08-12", fromYear = 2029 },
                paras = new List<string>
                {
                    "一个相识多年的老同学约你喝茶，绕了半天，绕到他家企业的申报材料上：“你把把关，看能不能……快一点。”",
                    "他推过来一份材料，边角已经磨毛——看来不是第一次想开口。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="把政策条款讲清楚，材料退回去",
                        effects=new Effects{ political=2, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1 } } },
                        result="“流程一天都不能少，材料一份都不能代。”话说得客气，门关得严实。老同学讪讪地把材料收了回去——多年后他想起来，会感谢你这杯茶。" },
                    new EventOption{ label="收下材料，顺手标两处问题",
                        effects=new Effects{ political=1, integrity=new IntegrityRecord{ tag="请托痕迹", note="替熟人看申报材料，被巡视组翻出记录" } },
                        result="你觉得只是“看一眼”。后来在某个谈话室里，这杯茶会被重新泡一遍——每一个字都会有人问你为什么喝。" },
                },
            });

            // 监察链：数据美化（灰度事件2）
            Flow.Register(new GameEvent
            {
                id = "sys_grey_data", type = "oversight", title = "“把数改好看点”",
                when = new When { md = "06-18", fromYear = 2030 },
                paras = new List<string>
                {
                    "某区报送的重点项目投资进度明显虚高，对方在电话里软磨硬泡：“兄弟，年中考核要紧，你把数往好看里修一修，年底就补回来。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="如实标注，附情况说明上报",
                        effects=new Effects{ political=2, rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=2, memo="顶住了区里的说情" } }, logKind="监察", logText="拒绝数据美化，如实上报" },
                        result="你在报表上贴了张便签：“数据与实地核验存在偏差，建议按实核减。”周衡之看到后，把它复印了一份贴在科里——没署名，但大家都知道是谁。" },
                    new EventOption{ label="照区的口径报，年底再看",
                        effects=new Effects{ stress=4, integrity=new IntegrityRecord{ tag="数据虚高", note="明知偏差仍按区口径上报" } },
                        result="报表交上去了，美观，顺利。只是从那天起，你办公桌抽屉的最深处多了一张纸——不是材料，是心病。" },
                },
            });

            // 监察链：巡视组进驻（violationCount 触发）
            Flow.Register(new GameEvent
            {
                id = "sys_xunshi", type = "oversight", title = "州委巡视组进驻",
                when = new When { flag = "violation_2" },
                paras = new List<string>
                {
                    "州委巡视组进驻市发改局。公告贴在一楼大厅，红头白字——“欢迎如实反映问题”。",
                    "人事科连夜核对干部档案；综合科的材料被调走三箱。你工位上的台灯，这几天熄得比平时晚。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="主动向巡视组说明情况",
                        effects=new Effects{ political=2, stress=6,
                            rel=new List<RelDelta>{ new RelDelta{ id="zhou", evalv=1, memo="主动说明问题，有担当" } },
                            setFlags=new List<string>{ "proactive_confession" },
                            setMarks=new List<string>{ "confess_m" },
                            logKind="监察", logText="主动向巡视组说明程序问题" },
                        result="你把两件事的来龙去脉写成书面材料，签字、按印。谈话同志合上本子：“态度很好，组织会如实记录。”——走出谈话室，天很蓝，你的手心全是汗。" },
                    new EventOption{ label="按兵不动，等组织找",
                        effects=new Effects{ stress=10, setFlags=new List<string>{ "passive_wait" },
                            setMarks=new List<string>{ "wait_m" } },
                        result="你选择了最常见的策略：等。等来的第一份文件，是《情况说明通知书》。机关里最折磨人的纸，不是处分决定，是这一张。" },
                },
            });

            // 监察链：处理决定（violation_3）
            Flow.Register(new GameEvent
            {
                id = "sys_discipline", type = "oversight", title = "组织处理",
                when = new When { flag = "violation_3" },
                paras = new List<string>
                {
                    "组织的结论下来了。依据你的行为、记录、态度与整改情况，纪委按程序作出处理。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="接受处理，深刻整改",
                        when=new OptionWhen{ notFlag="violation_severe" },
                        effects=new Effects{ morale=-10, stress=15, reputation=-3,
                            setFlags=new List<string>{ "punished" }, logKind="监察", logText="因程序违规受到诫勉谈话＋书面检查处理" },
                        result="诫勉谈话、书面检查、取消当年评优资格。你在检查末尾写道：“程序的每一次让步，都是欠给制度的一笔债。”——这一次，债还清了，人还在。" },
                    new EventOption{ label="（情节严重）接受立案审查",
                        when=new OptionWhen{ flag="violation_severe" },
                        effects=new Effects{ underInvestigation=true },
                        result="" },
                },
            });

            // 辞职抉择（2033起，每年9月人事窗口后）
            Flow.Register(new GameEvent
            {
                id = "sys_resign", type = "person", title = "离开体制的可能",
                when = new When { md = "10-20", fromYear = 2033 },
                paras = new List<string>
                {
                    "大学同学陈树给你打来电话——他创办的产业智库正缺一个“懂政府的人”，开的薪水是现在的三倍。",
                    "“兄弟，你那支笔、那些年，不该只写材料。”电话这头，你看着桌上那盆养了七年的绿萝。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="婉拒——我的路在这里",
                        effects=new Effects{ morale=2, political=1 },
                        result="你婉拒得很干脆。挂了电话，你把没写完的材料摊开——有些人的成就感在市场上，你的在这摞纸里。" },
                    new EventOption{ label="认真考虑，与家人商量",
                        effects=new Effects{ stress=5 },
                        result="你把利弊列了一张纸，贴在冰箱上。这张纸会贴很久——直到某个你觉得“火候到了”的日子。" },
                    new EventOption{ label="递上辞职报告",
                        when=new OptionWhen{ minGradeYears=0 },
                        effects=new Effects{ resigned=true },
                        result="" },
                },
            });
        }

        // ---------------- 职业链：转正/路线/借调/挂职/考试/学院/任命 ----------------

        private static void RegisterCareerChain()
        {
            // 2027-09-01 转正定级＋路线选择（十年路线的第一次分流）
            Flow.Register(new GameEvent
            {
                id = "career_confirm", type = "person", title = "转正定级 · 路线",
                when = new When { date = "2027-09-01" },
                paras = new List<string>
                {
                    "试用期届满，考核合格，正式定级：吏三·科员。人事科的通知只有一行字，你却把那页纸看了三遍。",
                    "转正谈话时，周衡之问了个正式的问题：“综合科的活你都会了。往后十年，你想往哪条线上走？”",
                    "“局里四个口：综合文秘——笔杆子的道场；产业经济——围着企业和报表转；投资项目——管钱、管项目、管工地；区域协调——跟区县、都市圈打交道。选一条主攻，其余的，日子还长。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="综合文秘线：把笔杆子磨成全局的尺子",
                        effects=new Effects{ route="笔杆子", professional=2, rel=new List<RelDelta>{ new RelDelta{ id="lin", trust=3 } }, logKind="系统", logText="确立职业路线：综合文秘（笔杆子）" },
                        result="林晚听说你选了这条线，把她十年的材料笔记复印了一份给你：“笔杆子的路最慢，也最稳——写到后来，纸背面的东西都是你的。”" },
                    new EventOption{ label="产业经济线：去产业科，围着企业转",
                        effects=new Effects{ route="产业经济", comm=1, political=1, rel=new List<RelDelta>{ new RelDelta{ id="he", trust=3 } }, logKind="系统", logText="确立职业路线：产业经济" },
                        result="产业科的走廊永远飘着茶香——企业的人爱来。老科长拍拍你的肩：“这条线离市场最近，也离诱惑最近，腿要勤，手要净。”" },
                    new EventOption{ label="投资项目线：去投资科，管钱管项目",
                        effects=new Effects{ route="投资项目", exec=1, rel=new List<RelDelta>{ new RelDelta{ id="su", trust=3 } }, logKind="系统", logText="确立职业路线：投资项目" },
                        result="苏晴就在投资科。她给你看她的台账模板：“这条线，数字就是人品。”你在心里给这句话盖了个章。" },
                    new EventOption{ label="区域协调线：去地区科，跑都市圈",
                        effects=new Effects{ route="区域协调", comm=1, political=2, logKind="系统", logText="确立职业路线：区域协调" },
                        result="地区科的老科员递给你一张关中地图：“这条线，一半在文件里，一半在路上。先把这张图跑熟。”" },
                },
            });

            // 2029-09 借调抉择
            Flow.Register(new GameEvent
            {
                id = "career_secondment", type = "person", title = "借调的选择",
                when = new When { date = "2029-09-25" },
                paras = new List<string>
                {
                    "两份借调函几乎同时到局里：市政府办公室想要个“材料扎实的”；市“十五五”规划编制专班想要个“懂经济的”。周衡之把两份函都放在你桌上。",
                    "“借调是双刃剑，”他说，“平台大，见的人不一样；但离开了科室，考核、晋升，都得重新攒。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="借调市政府办公室",
                        effects=new Effects{ seconded="市政府办公室（借调）", reputation=2, comm=2, political=1, rel=new List<RelDelta>{ new RelDelta{ id="ma", familiar=3, memo="把好苗子借给了市府办" } }, logKind="系统", logText="借调市政府办公室" },
                        result="市府办的会议室里坐着的是全市的决策链条。你第一次在文件上看到自己的名字被传阅——白纸黑字，重若千钧。" },
                    new EventOption{ label="借调“十五五”规划编制专班",
                        effects=new Effects{ seconded="市规划编制专班（借调）", professional=3, reputation=2, rel=new List<RelDelta>{ new RelDelta{ id="lin", evalv=2, memo="专班需要笔杆子" } }, logKind="系统", logText="借调市规划编制专班" },
                        result="专班的日子像闭关：数据、访谈、测算、成稿。你第一次参与一座城市未来五年的书写——哪怕只是其中一小节。" },
                    new EventOption{ label="留在科室，深耕本线",
                        effects=new Effects{ exec=2, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=3, evalv=1, memo="稳，不飘" } } },
                        result="你把借调函退了回去。周衡之什么都没说，但那年你的名字出现在科室推荐评优的名单上——留有留的道理。" },
                },
            });

            // 2031-09 借调归位
            Flow.Register(new GameEvent
            {
                id = "career_return", type = "person", title = "借调期满",
                when = new When { date = "2031-09-25" },
                paras = new List<string>
                {
                    "两年借调期满，你回到市发改局。综合科的工位换了新的绿萝，赵姐的茶杯还在老地方。",
                    "周衡之翻着你在外的鉴定材料：“平台历练过了——回来，该给你压更重的担子了。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="回局履职",
                        effects=new Effects{ seconded="", reputation=1, exec=2, rel=new List<RelDelta>{ new RelDelta{ id="zhou", trust=2 } } },
                        result="归来仍是综合科——但你看报表的眼神，已经带了全局的焦距。" },
                },
            });

            // 2032-09 下沉挂职（补基层履历）
            Flow.Register(new GameEvent
            {
                id = "career_guazhi", type = "person", title = "下沉挂职",
                when = new When { date = "2032-09-22" },
                paras = new List<string>
                {
                    "局党组研究决定：选派年轻干部下沉基层挂职一年——区发改局副局长，补“基层公共事务履历”。名单上有你。",
                    "周衡之送你四个字：“下去，是上来。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="接受挂职：区发改局副局长",
                        effects=new Effects{ seconded="浐河区发改局（挂职副局长）", baseExpDelta=12, comm=2, political=1, reputation=1, logKind="系统", logText="下沉挂职区发改局副局长一年" },
                        result="区里的办公楼比市里旧，事比市里杂。征迁、保供、企业诉求——一年之后你会明白：基层履历不是晋升的筹码，是理解的刻度。" },
                    new EventOption{ label="暂不下沉（当条件不足时的保留选项）",
                        effects=new Effects{ morale=1 },
                        result="你把机会让给了更需要的同事。基层履历的事，来日方长。" },
                },
            });

            // 2033-09 州级转官考试（条件由 OptionWhen 把关）
            Flow.Register(new GameEvent
            {
                id = "career_exam", type = "politics", title = "州级转官考试",
                when = new When { date = "2033-09-26" },
                paras = new List<string>
                {
                    "州级转官考试。考场设在州府的标准化考点——考场外，和你同龄的考生们互相打量，眼神里都是“十年”两个字。",
                    "科目：行政能力、政策分析、公文写作。吏转官的那道门，门里是政治学院，门外是十年科员。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="沉着应考",
                        when=new OptionWhen{ grade="吏一", minOutstanding=2, minBaseExpMonths=24 },
                        check=new Check{ main="professional", bonus=0.2f },
                        effects=new Effects{ setFlags=new List<string>{ "exam_passed" }, stress=8, political=1, logKind="系统", logText="通过州级转官考试" },
                        result="放榜那天，你在名单第二行找到了自己。考试不是终点——省政治学院的一年，才是真正的淬火。{grade}。" },
                    new EventOption{ label="这次不考，再沉淀一年",
                        when=new OptionWhen{ grade="吏一" },
                        effects=new Effects{ morale=1 },
                        result="你把准考证的样式看了一眼，转身回了办公室。考试年年有，火候只有自己知道。" },
                },
            });

            // 2034-09 省政治学院入学（需考试通过）
            Flow.Register(new GameEvent
            {
                id = "career_academy", type = "politics", title = "省政治学院入学",
                when = new When { date = "2034-09-01", flag = "exam_passed" },
                paras = new List<string>
                {
                    "省政治学院，青年干部培训班。从“吏”到“官”的必经一站：一年，政策、财政、法治、危机模拟，还有那门著名的课——《帝国制度失灵与国家纠错》。",
                    "同学来自八州：有州的部委科员，有县长秘书，有国有企业来的挂职干部。第一堂课，老师只有一句话：“从今天起，学会为决策负责。”",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="入学，开始一年淬火",
                        effects=new Effects{ seconded="省政治学院（青年干部班）", professional=4, political=4, comm=2, reputation=2, stress=6, logKind="系统", logText="入读省政治学院青年干部班" },
                        result="课程表密得像铁板：案例、模拟、辩论、下沉调研。你把那门《制度失灵》的笔记记满了两本——现在你懂了，为什么1978年非改不可。" },
                },
            });

            // 2035-09 结业与十品任命（优秀破格线的主要出口）
            Flow.Register(new GameEvent
            {
                id = "career_pin10", type = "politics", title = "结业 · 十品任命",
                when = new When { date = "2035-09-01", flag = "exam_passed" },
                paras = new List<string>
                {
                    "结业考核通过，组织任命下达：任十品——副处级。按惯例，新晋十品的年轻干部优先安排基层实职历练。",
                    "任命书上的印章很正。从吏三到十品，你在档案里走了整整九年。",
                },
                options = new List<EventOption>
                {
                    new EventOption{ label="挂职副区长（基层实职）",
                        effects=new Effects{ gradeTo="十品·副处", seconded="浐河区（挂职副区长）", reputation=4, polCapital=3, comm=2, exec=2, logKind="系统", logText="任十品·副处，挂职副区长" },
                        result="副区长的名片印出来那天，你把第一张夹进了当年的工作笔记——不是为了纪念，是为了提醒：从今天起，你签字的每一个“同意”都长出了牙齿。" },
                    new EventOption{ label="留市局任副处级干部",
                        effects=new Effects{ gradeTo="十品·副处", reputation=3, polCapital=4, admin=2, logKind="系统", logText="任十品·副处，留局任职" },
                        result="副局长办公室的门离你更近了一步。组织上的评语是：“成熟，稳，可持续。”——在机关，这是三个很重的词。" },
                },
            });
        }
    }
}
