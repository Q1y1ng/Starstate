using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// Phase 5 · M1 剧情线路（主干分叉）：路线确立 → 四条路线链 → 常委会权力链 / 同批竞争链 / 企业邀约链。
    ///
    /// 作者须知（踩坑记录）：
    /// ① md 事件每年都会重新满足触发条件——除 `MarkFired(id@年)` 外没有别的去重，
    ///    因此**每一环都必须自带一次性门槛**：`requireMarks` 放上一环的完成标记，
    ///    `requireNotMarks` 放本环自己的完成标记（本文件统一用这个写法）。
    /// ② `When.route` 只对 `date` 触发生效，md 触发不看 route——路线分叉一律用 marks。
    /// ③ 标记命名：route_*（路线本身）／rte_&lt;线&gt;_*（环内进度）／pb_*（常委会）／rival_*（同批）／offer_*（邀约）。
    /// </summary>
    public static class ContentRoutesMayor
    {
        public static void Register()
        {
            RegisterRouteChoice();
            RegisterIndustryLine();
            RegisterPeopleLine();
            RegisterProjectLine();
            RegisterUplinkLine();
            RegisterPowerBoard();
            RegisterRivalLine();
            RegisterTalentOffer();
        }

        // =====================================================================
        // 0. 路线确立：四条职业路线的分叉点（st.route 此前全程无人赋值）
        // =====================================================================

        static void RegisterRouteChoice()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_route_open", type = "politics", title = "务虚会 · 这十年怎么走",
                when = new When
                {
                    md = "03-10", fromYear = 2027,
                    requireNotMarks = new[] { "route_set" },
                },
                paras = new List<string>
                {
                    "开春的务虚会，四张桌子，四份材料。周谨把每份的第一页折了角，摆在你面前：",
                    "左边是高新区的产业用地测算，右边是西城区的供暖投诉台账；再往右，是省里下达的能源保供指标，和云冈区刚贴出去的征地公告。",
                    "“老板，”周谨把茶续上，“一年了。总得让人知道，大同这几年想留下什么。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "产业转型：把老煤城的骨头换成新产业",
                        effects = new Effects
                        {
                            route = "industry", admin = 1, political = 1,
                            setMarks = new List<string> { "route_set", "route_industry" },
                            logKind = "系统", logText = "确立路线：产业转型",
                        },
                        result = "你在产业那页上划了一道：“先立规矩，再谈规模。”——大同的煤挖了一百年，换一副骨头，十年未必够。",
                    },
                    new EventOption
                    {
                        label = "民生兜底：先把暖气、学校和医院办好",
                        effects = new Effects
                        {
                            route = "people", comm = 1, morale = 3, reputation = 2,
                            setMarks = new List<string> { "route_set", "route_people" },
                            logKind = "系统", logText = "确立路线：民生兜底",
                        },
                        result = "你圈了供暖台账上那三千户：“先让屋里热起来。”——民生见效慢，但群众记得住冬天的温度。",
                    },
                    new EventOption
                    {
                        label = "项目攻坚：把开工的工地变成真金白银",
                        effects = new Effects
                        {
                            route = "project", exec = 1, stress = 2,
                            setMarks = new List<string> { "route_set", "route_project" },
                            rel = new List<RelDelta> { new RelDelta { id = "shao", familiar = 2, evalv = 1, memo = "路线：项目攻坚" } },
                            logKind = "系统", logText = "确立路线：项目攻坚",
                        },
                        result = "你指着那串没完工的项目：“今年只看开工率。”邵志远眼睛亮了一下——他喜欢听得懂的指标。",
                    },
                    new EventOption
                    {
                        label = "向上争取：先把省里的口袋撬开",
                        effects = new Effects
                        {
                            route = "uplink", political = 2, polCapital = 3,
                            setMarks = new List<string> { "route_set", "route_uplink" },
                            rel = new List<RelDelta> { new RelDelta { id = "cen", familiar = 1, trust = 1, memo = "路线：向上争取" } },
                            logKind = "系统", logText = "确立路线：向上争取",
                        },
                        result = "你把保供指标那份卷起来：“大同替省里扛任务，省里也不能只给任务。”——对上争取，是门手艺，也是门赌博。",
                    },
                },
            });

            // 路线确立的结果回响：一年后，班子里开始用路线看你
            Flow.Register(new GameEvent
            {
                id = "ch_route_echo", type = "politics", title = "班子里的说法",
                when = new When
                {
                    md = "03-25", fromYear = 2028,
                    requireMarks = new[] { "route_set" },
                    requireNotMarks = new[] { "route_echoed" },
                },
                paras = new List<string>
                {
                    "一年下来，市里给了你一个说法。",
                    "有人说你“路子清”，有人说你“押得重”，还有人在饭桌上把你的路线和邻市许飞的比了比——比完就笑，谁也不接话。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "记下这些说法，回头对一对文件里的进度",
                        effects = new Effects
                        {
                            admin = 1, political = 1,
                            setMarks = new List<string> { "route_echoed" },
                            logKind = "系统", logText = "路线一年：班子评价归档",
                        },
                        result = "你把这句“押得重”记在本子背面。评价不是政绩，但评价会决定政绩怎么被念出来。",
                    },
                },
            });
        }

        // =====================================================================
        // 1. 产业转型线：配套用地 → 本地配套率 → 承接产业转移示范区
        // =====================================================================

        static void RegisterIndustryLine()
        {
            Flow.Register(new GameEvent
            {
                id = "rte_ind_1", type = "business", title = "龙头企业的“配套用地”",
                when = new When
                {
                    md = "05-20", fromYear = 2027,
                    requireMarks = new[] { "route_industry" },
                    requireNotMarks = new[] { "rte_ind_1_done" },
                },
                paras = new List<string>
                {
                    "华智能源的副总第三次来谈：总部要在云中落区域基地，条件之一是把园区东侧那片地“定向”留给它的三家配套厂。",
                    "任慎把地籍图摊开：“市长，那片地是挂牌出让的。定向，得有文件依据。”",
                    "副总的钢笔在桌面上敲了两下，节奏像秒表。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "先做产业目录认定，按目录供地",
                        effects = new Effects
                        {
                            professional = 2, efficiency = 0, stress = 2,
                            setMarks = new List<string> { "rte_ind_1_done", "ind_land_clean" },
                            rel = new List<RelDelta> { new RelDelta { id = "rensheng", trust = 1, evalv = 1, memo = "产业用地按目录认定" } },
                            logKind = "卷宗", logText = "产业配套用地：按目录认定后供地",
                        },
                        result = "认定走了两个月。企业骂了两句“慢了”，但签了。慢出来的那两个月，把将来的解释权留给了文件，而不是留给了你。",
                    },
                    new EventOption
                    {
                        label = "先签框架、后补程序，抢时间",
                        effects = new Effects
                        {
                            efficiency = 4, stress = 3,
                            setMarks = new List<string> { "rte_ind_1_done", "ind_land_fast" },
                            integrity = new IntegrityRecord { tag = "产业用地", note = "配套用地先签框架后补程序" },
                            logKind = "卷宗", logText = "产业配套用地：先签后补",
                        },
                        result = "开工仪式很热闹，彩带剪了三段。任慎站在后排，手里捏着那份还没编号的会议纪要。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rte_ind_2", type = "business", title = "本地配套率",
                when = new When
                {
                    md = "11-08", fromYear = 2027,
                    requireMarks = new[] { "route_industry" },
                    requireNotMarks = new[] { "rte_ind_2_done" },
                },
                paras = new List<string>
                {
                    "基地投产半年，本地配套率11%。零件从沿海拉过来，一辆卡车迟一天，整条线停一天。",
                    "市工信局的汇报里写着一句实话：我们引进了一个“飞地工厂”，不是一条产业链。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "拿财政钱补本地中小配套厂的技术改造",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ind_2_done", "ind_chain_local" },
                            stress = 3, exec = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "fang", familiar = 2, memo = "配套技改补贴" } },
                            logKind = "卷宗", logText = "本地配套率：技改补贴",
                        },
                        result = "第一批十二家小厂改完，本地配套率爬到34%。数字不漂亮，但卡车少了三成——路上少跑的车，是账本上省下的钱。",
                    },
                    new EventOption
                    {
                        label = "让企业自己带配套来，政府只保服务",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ind_2_done", "ind_chain_market" },
                            comm = 1, efficiency = 2,
                            logKind = "卷宗", logText = "本地配套率：交给市场",
                        },
                        result = "企业果然带来了两家老关系户，注册地在大同，厂址在沿海。统计上是本地企业，物理上不是。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rte_ind_3", type = "politics", title = "承接产业转移示范区",
                when = new When
                {
                    md = "06-18", fromYear = 2028,
                    requireMarks = new[] { "route_industry" },
                    requireNotMarks = new[] { "rte_ind_3_done" },
                },
                paras = new List<string>
                {
                    "省发展和改革署下来一个名额：国家级承接产业转移示范区，全省两个，报一个落一个。",
                    "韩清把申报要件放在你桌上，第一页是硬指标：规上工业增加值增速、单位能耗、研发投入强度——大同差两项，差得不多，也不少。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "全力申建，把三年指标一次压进责任状",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ind_3_done", "ind_zone_won" },
                            stress = 6, political = 2, reputation = 3, polCapital = 3,
                            rel = new List<RelDelta> { new RelDelta { id = "han", trust = 1, evalv = 2, memo = "压责任状申建示范区" } },
                            logKind = "系统", logText = "申建承接产业转移示范区（压三年责任状）",
                        },
                        result = "牌子挂上那天，你把责任状锁进抽屉。牌子是给外面看的，责任状是给将来的自己看的。",
                    },
                    new EventOption
                    {
                        label = "今年不报，先把差的指标补上",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ind_3_done", "ind_zone_pass" },
                            admin = 2, professional = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "han", evalv = -1, memo = "错过示范区申报名额" } },
                            logKind = "系统", logText = "放弃申建示范区，先补指标",
                        },
                        result = "韩清收材料时没说什么，只说了一句：“名额明年还有。”——明年还有的名额，通常后年就没有了。",
                    },
                },
            });
        }

        // =====================================================================
        // 2. 民生兜底线：供暖投诉 → 老旧小区改造缺口 → 接诉即办
        // =====================================================================

        static void RegisterPeopleLine()
        {
            Flow.Register(new GameEvent
            {
                id = "rte_ppl_1", type = "society", title = "供暖季的最后一个投诉",
                when = new When
                {
                    md = "04-02", fromYear = 2027,
                    requireMarks = new[] { "route_people" },
                    requireNotMarks = new[] { "rte_ppl_1_done" },
                },
                paras = new List<string>
                {
                    "采暖季最后一周，西城区三个小区三千户的暖气片还是凉的。12345的热线量在最后三天翻了四倍。",
                    "热力公司报上来的原因是“管网末端水力失衡”——一句话把三千户的冬天解释完了。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "财政先垫钱抢修，责任秋后再算",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ppl_1_done", "ppl_heat_money" },
                            stress = 3, reputation = 3, efficiency = -2,
                            rel = new List<RelDelta> { new RelDelta { id = "fang", familiar = 2, memo = "供暖抢修财政垫资" } },
                            logKind = "卷宗", logText = "供暖：财政垫资抢修",
                        },
                        result = "抢修队进场第三天，西城区的老大爷拎着保温杯到市政府门口，非要把茶给你留下。你把茶喝了——比会上的矿泉水甜。",
                    },
                    new EventOption
                    {
                        label = "先派工作组查责任，修不修看责任怎么划",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ppl_1_done", "ppl_heat_probe" },
                            political = 2, stress = 2, reputation = -1,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", familiar = 1, memo = "供暖问题先查责任" } },
                            logKind = "监察", logText = "供暖：先查责任后修",
                        },
                        result = "查了三周，责任划得清清楚楚；暖气也热了，晚了二十天。清清楚楚的责任，暖不了十一月的那三个小区。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rte_ppl_2", type = "work", title = "老旧小区改造的资金缺口",
                when = new When
                {
                    md = "09-14", fromYear = 2027,
                    requireMarks = new[] { "route_people" },
                    requireNotMarks = new[] { "rte_ppl_2_done" },
                },
                paras = new List<string>
                {
                    "全市需要改造的老旧小区，按现有财政能力排，需要十九年。",
                    "市住建局给的方案很诚实：要么压别的支出，要么向上要钱，要么让居民自己掏一部分。三者都不好听。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "压办公经费与会议费，保改造",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ppl_2_done", "ppl_cut_office" },
                            admin = 1, reputation = 2, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "zhoujin", trust = 1, memo = "砍办公经费保老旧小区" } },
                            logKind = "卷宗", logText = "老旧小区：压办公经费",
                        },
                        result = "办公经费压了11%。办公厅把用了八年的复印机修了第三次。周谨没抱怨，只是在《节约型机关》简报上把大同列进了典型案例——标题是你自己批的。",
                    },
                    new EventOption
                    {
                        label = "向省里争取专项，等钱到了再动",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ppl_2_done", "ppl_wait_money" },
                            political = 1, efficiency = -2,
                            clockOps = new List<ClockOp>
                            {
                                new ClockOp { id = "clk_ppl_fund", label = "老旧小区专项", max = 6, kind = "opportunity", delta = 1, onFull = "ch_ppl_fund_back" },
                            },
                            logKind = "卷宗", logText = "老旧小区：争取省级专项",
                        },
                        result = "申报书报了四十六页。四十六页纸换来的，是省里一句“研究研究”——但“研究研究”总比“暂无计划”强一格。",
                    },
                    new EventOption
                    {
                        label = "引入社会资本，谁改造谁受益",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ppl_2_done", "ppl_social_cap" },
                            comm = 1, professional = 1, stress = 2,
                            integrity = new IntegrityRecord { tag = "民生项目", note = "老旧小区引入社会资本，配套经营性设施" },
                            logKind = "卷宗", logText = "老旧小区：引入社会资本",
                        },
                        result = "三家企业来谈，谈的都是加装电梯和停车位的收益。改造是改造了——先改的是能赚钱的那部分。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "ch_ppl_fund_back", type = "work", title = "专项资金下来了",
                when = new When
                {
                    md = "01-20", fromYear = 2028,
                    requireMarks = new[] { "route_people" },
                    requireNotMarks = new[] { "ch_ppl_fund_done" },
                },
                paras = new List<string>
                {
                    "省里的老旧小区专项下来了：三亿八千万，分三年，第一批到账一亿二。",
                    "周谨把到账单和另一份东西一起放在你桌上——某县同期的申报材料，拆成十二个子项，每个不超过五百万。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "集中投西城三个片区，一次改到位",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ch_ppl_fund_done", "ppl_fund_whole" },
                            exec = 2, reputation = 3,
                            logKind = "系统", logText = "老旧小区专项：集中整片改造",
                        },
                        result = "三个片区一起动，工地连成片，居民骂声也连成片。三个月后骂声停了——停的那天，管道里的水是热的。",
                    },
                    new EventOption
                    {
                        label = "按县区雨露均沾，各自报最急的",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "ch_ppl_fund_done", "ppl_fund_share" },
                            political = 2, admin = 1,
                            logKind = "系统", logText = "老旧小区专项：按县区切分",
                        },
                        result = "十个县区，十份感谢信。十年后回头看，这十份感谢信换来的是二十处都改了一半的小区。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rte_ppl_3", type = "society", title = "接诉即办",
                when = new When
                {
                    md = "03-22", fromYear = 2028,
                    requireMarks = new[] { "route_people" },
                    requireNotMarks = new[] { "rte_ppl_3_done" },
                },
                paras = new List<string>
                {
                    "市长信箱累计十万件。周谨统计了一个数：办结率96%，但“群众满意”只有61%。",
                    "“办结”和“解决”之间那条缝，是这座城市真正的民意。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "建“接诉即办”长效机制，考核满意率而非办结率",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ppl_3_done", "ppl_jiesu" },
                            admin = 2, reputation = 4, stress = 3,
                            logKind = "系统", logText = "接诉即办：考核满意率",
                        },
                        result = "制度印发那天，全市热线员的考核表都换了。第一个月，满意率掉到52%——数字掉下去的那一个月，才是真正开始办事的月份。",
                    },
                    new EventOption
                    {
                        label = "维持办结率考核，务实一些",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_ppl_3_done", "ppl_rate_keep" },
                            efficiency = 2, admin = 1,
                            logKind = "系统", logText = "12345维持办结率考核",
                        },
                        result = "报表很好看，全年的办结率是98.7%。你偶尔会想起那61%——它不在任何一份报表里。",
                    },
                },
            });
        }

        // =====================================================================
        // 3. 项目攻坚线：征地苗头 → 补偿争议 → 开工
        // =====================================================================

        static void RegisterProjectLine()
        {
            Flow.Register(new GameEvent
            {
                id = "rte_prj_1", type = "society", title = "征地公告贴出去的第三天",
                when = new When
                {
                    md = "05-06", fromYear = 2027,
                    requireMarks = new[] { "route_project" },
                    requireNotMarks = new[] { "rte_prj_1_done" },
                },
                paras = new List<string>
                {
                    "云冈区某村的征地公告贴出去第三天，村委会门口聚了四十几个人，大半是六十岁以上的。",
                    "区里报上来的是“情况可控”。周谨补了一句他没写进材料的话：“可控的意思是，今天还站在门口，没走路去区里。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "当天下午自己去村里，坐在村委会的板凳上谈",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_prj_1_done", "prj_village_mayor" },
                            stress = 5, reputation = 4, political = 1,
                            logKind = "卷宗", logText = "征地苗头：市长直接下村",
                        },
                        result = "从下午三点坐到晚上七点。走的时候，村里最凶的那位大爷把你送到路口，说了句：“你来过，我记账。”——在村里，来过，就是一笔账。",
                    },
                    new EventOption
                    {
                        label = "派工作组进驻，一周内出化解方案",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_prj_1_done", "prj_village_team" },
                            admin = 2, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "zhoujin", familiar = 1, memo = "征地派工作组" } },
                            logKind = "卷宗", logText = "征地苗头：派工作组",
                        },
                        result = "工作组第七天出方案，第十天群众散了。方案第一条是“补偿标准不降”——散，是因为这条。",
                    },
                    new EventOption
                    {
                        label = "暂缓公告，重新摸底后再推",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_prj_1_done", "prj_village_pause" },
                            efficiency = -4, professional = 1, reputation = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "shao", evalv = -2, memo = "征地项目暂缓" } },
                            logKind = "卷宗", logText = "征地苗头：暂缓公告重新摸底",
                        },
                        result = "公告撤下来的当天，邵志远在办公室摔了本子。三个月后重贴的公告，补偿方案厚了三页——那三页，是三个月换来的。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rte_prj_2", type = "society", title = "补偿标准的第二封信",
                when = new When
                {
                    md = "08-19", fromYear = 2027,
                    requireMarks = new[] { "route_project" },
                    requireNotMarks = new[] { "rte_prj_2_done" },
                },
                paras = new List<string>
                {
                    "补偿标准第二封信寄到了省里。信的落款有十九个签名，其中一个名字你在征地表上见过——村小的代课老师。",
                    "区里给的意见是“依法处置”。依法处置这四个字，通常意味着事情已经不在法治的层面上了。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "依法提起复议，同时把标准重新算一遍",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_prj_2_done", "prj_std_recalc" },
                            professional = 2, stress = 3, reputation = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "rensheng", evalv = -2, memo = "征地补偿标准被重新核算" } },
                            logKind = "卷宗", logText = "征地补偿：重新核算标准",
                        },
                        result = "重算的结果比原标准高了9%，比邻区低了3%。两头的数字都不完美——但从此这9%有了出处。",
                    },
                    new EventOption
                    {
                        label = "维持标准，按程序处置信访",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_prj_2_done", "prj_std_hold" },
                            efficiency = 3, political = 1, reputation = -2,
                            logKind = "卷宗", logText = "征地补偿：维持原标准",
                        },
                        result = "程序走完了，十九个签名散成了三批。项目按时开工——开工那天，村小那间教室的窗户上，贴了一张白纸。",
                    },
                    new EventOption
                    {
                        label = "以“特殊困难补助”名义个别解决",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_prj_2_done", "prj_std_gray" },
                            stress = 2, efficiency = 2,
                            integrity = new IntegrityRecord { tag = "征地补偿", note = "以特殊困难补助名义个别加价" },
                            logKind = "卷宗", logText = "征地补偿：个别特殊补助",
                        },
                        result = "签名的十九户里，有六户拿到了补助。剩下的十三户很快也知道了——个别解决最贵的地方，是它一定会被知道。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rte_prj_3", type = "work", title = "开工那天",
                when = new When
                {
                    md = "04-11", fromYear = 2028,
                    requireMarks = new[] { "route_project" },
                    requireNotMarks = new[] { "rte_prj_3_done" },
                },
                paras = new List<string>
                {
                    "项目开工。桩机第一声砸下去的时候，邵志远站在你旁边，说了句很像他风格的话：“去年这个时候，这地还是玉米。”",
                    "项目是开工了，工期表上写着2029年底投产——而账上还差4.2亿。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把缺口摊到台面上，请省里协调配套",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_prj_3_done", "prj_gap_open" },
                            political = 2, stress = 3,
                            logKind = "系统", logText = "项目资金缺口：上报省里协调",
                        },
                        result = "省里协调会开了两次，补上2.6亿。剩下的1.6亿，你写在了年度报告的第一页——第一页的字，是给下一任看的。",
                    },
                    new EventOption
                    {
                        label = "让平台公司先垫，投产后再结算",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_prj_3_done", "prj_gap_platform" },
                            efficiency = 3, stress = 4,
                            integrity = new IntegrityRecord { tag = "政府债务", note = "园区配套由平台公司垫资建设" },
                            logKind = "系统", logText = "项目缺口：平台垫资",
                        },
                        result = "桩机的声音没停，报表上也没有赤字。只是市属平台公司的资产负债率，从62%变成了71%——那个数字，明年才会有人来问。",
                    },
                },
            });
        }

        // =====================================================================
        // 4. 向上争取线：保供指标 → 进省汇报 → 专项资金落地
        // =====================================================================

        static void RegisterUplinkLine()
        {
            Flow.Register(new GameEvent
            {
                id = "rte_upl_1", type = "work", title = "保供指标",
                when = new When
                {
                    md = "04-16", fromYear = 2027,
                    requireMarks = new[] { "route_uplink" },
                    requireNotMarks = new[] { "rte_upl_1_done" },
                },
                paras = new List<string>
                {
                    "省发展和改革署的传真：请大同市在采暖季前落实电煤保供任务，较上年增产8%。",
                    "这行字下面，是另一份文件：全省热电联产的环保改造资金分配表——大同排在第九位，九个市里的第九位。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "接受指标，同时把环保资金一并要过来",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_upl_1_done", "upl_link_both" },
                            political = 2, polCapital = 2, stress = 3,
                            rel = new List<RelDelta> { new RelDelta { id = "cen", evalv = 2, memo = "接指标要资金的谈法" } },
                            logKind = "系统", logText = "保供指标：接任务换资金",
                        },
                        result = "你在传真上批了“坚决完成”，然后把那份资金分配表复印了一份，夹在汇报材料第一页。三周后，资金表里大同从第九升到第四。",
                    },
                    new EventOption
                    {
                        label = "先接受指标，资金的事回头再说",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_upl_1_done", "upl_link_task" },
                            exec = 2, stress = 4, political = -1,
                            logKind = "系统", logText = "保供指标：只接任务",
                        },
                        result = "指标接了，钱没要到。矿上的人连夜加班，煤价却没涨——差价最后落在市财政的账上。有些账，接任务的那天就记下了。",
                    },
                    new EventOption
                    {
                        label = "讨价还价：8%接不了，最多5%并要配套资金",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_upl_1_done", "upl_link_bargain" },
                            political = -2, polCapital = -1, professional = 1, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "cen", evalv = -2, memo = "保供指标讨价还价" } },
                            logKind = "系统", logText = "保供指标：还价到5%",
                        },
                        result = "传真来往三轮，最后定在5%。省里一位处长的电话里有一句话：“大同今年话多。”——话多的话，将来要少说几句才补得回来。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rte_upl_2", type = "work", title = "进省汇报",
                when = new When
                {
                    md = "10-12", fromYear = 2027,
                    requireMarks = new[] { "route_uplink" },
                    requireNotMarks = new[] { "rte_upl_2_done" },
                },
                paras = new List<string>
                {
                    "专项债申报答辩，每市二十分钟。你前面那个市讲了十八分钟成绩，剩两分钟讲诉求，被打断了。",
                    "轮到你。投影仪上是三页纸：缺什么、干什么、还差多少。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "只讲缺口和方案，成绩一页带过",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_upl_2_done", "upl_pitch_plan" },
                            professional = 2, political = 2, polCapital = 2,
                            logKind = "系统", logText = "进省答辩：只讲缺口与方案",
                        },
                        result = "讲完第十二分钟，主评人问：“这个测算表是自己做的？”你说“是”。他点了点头——那一下点头，比掌声有用。",
                    },
                    new EventOption
                    {
                        label = "先讲成绩后讲诉求，按惯例来",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_upl_2_done", "upl_pitch_soft" },
                            comm = 1, stress = 1,
                            logKind = "系统", logText = "进省答辩：先成绩后诉求",
                        },
                        result = "答辩很平顺，排在第二部分。第二部分的意思是：可以给，也可以下批再给。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "rte_upl_3", type = "politics", title = "资金落地的通知",
                when = new When
                {
                    md = "05-09", fromYear = 2028,
                    requireMarks = new[] { "route_uplink" },
                    requireNotMarks = new[] { "rte_upl_3_done" },
                },
                paras = new List<string>
                {
                    "省财政署下达专项债券额度：大同六亿四千万，是去年的两倍。",
                    "周谨把通知放在你桌上，压了一张便签：“额度有了，但要求三季度前开工——钱不是白给的。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "组建专班，把开工手续压到两个月内",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_upl_3_done", "upl_fund_fast" },
                            exec = 3, admin = 1, stress = 4,
                            reputation = 2,
                            logKind = "系统", logText = "专项债：两个月压完开工手续",
                        },
                        result = "两个月开工，市里第一次跑到了省里要求的前面。跑那么快有代价：有两项审批是补齐的——你知道是哪两项。",
                    },
                    new EventOption
                    {
                        label = "按正常程序走，宁可慢一点",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rte_upl_3_done", "upl_fund_slow" },
                            professional = 2, efficiency = -2,
                            logKind = "系统", logText = "专项债：按程序推进",
                        },
                        result = "三季度差十一天没开工，省里通报了一次。通报的第二天，你把手续清单贴在办公室门后——每一项后面都写了责任人。",
                    },
                },
            });
        }

        // =====================================================================
        // 5. 常委会权力链：提案被缓议 → 部长的暗示 → 纪委问询
        // =====================================================================

        static void RegisterPowerBoard()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_pb_1", type = "politics", title = "被缓议的三十亿",
                when = new When
                {
                    md = "06-05", fromYear = 2027,
                    requireNotMarks = new[] { "pb_1_done" },
                },
                paras = new List<string>
                {
                    "常委会第十二项议题：关于设立产业转型基金三十亿元的请示。你的材料准备了两个月。",
                    "岑伯衡听完，把材料合上：“金额不小，先放放。请财政再把风险敞口算一算。”",
                    "“先放放”三个字在议题单上只占一行，但它意味着：这份材料下个月还会再来一次，或者永远不会回来。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "会后去主席办公室，把风险敞口一条条讲清楚",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "pb_1_done", "pb_1_private" },
                            political = 2, stress = 3,
                            rel = new List<RelDelta> { new RelDelta { id = "cen", familiar = 2, trust = 2, memo = "缓议后主动上门讲清风险" } },
                            logKind = "政治", logText = "产业基金：会后单独沟通",
                        },
                        result = "讲了四十分钟，他没表态，只把材料留在了桌上。三周后基金过会，金额二十五亿——砍掉的五亿，是他替你把风险先砍掉的。",
                    },
                    new EventOption
                    {
                        label = "不谈，按程序下次会重提，让材料自己说话",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "pb_1_done", "pb_1_again" },
                            professional = 2, polCapital = -2, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "cen", evalv = -1, memo = "基金材料原样重提" } },
                            logKind = "政治", logText = "产业基金：原样重提",
                        },
                        result = "下次会你原样重提，材料一个字没改。岑伯衡看了你三秒，通过了。会后韩清说：“主席不记得数字，记得姿态。”",
                    },
                    new EventOption
                    {
                        label = "让邵志远先去吹风，自己不出面",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "pb_1_done", "pb_1_shao" },
                            comm = 1, polCapital = -1,
                            rel = new List<RelDelta>
                            {
                                new RelDelta { id = "shao", familiar = 2, trust = 1, memo = "替市长去主席处吹风" },
                                new RelDelta { id = "cen", evalv = -1, memo = "基金一事由常务副代说" },
                            },
                            logKind = "政治", logText = "产业基金：委托常务副吹风",
                        },
                        result = "邵志远把事办成了，也把话说成了他的版本。常委会上有人笑着说：“这基金，是邵市长的基金吧。”——笑着说的，最当真。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "ch_pb_2", type = "politics", title = "组织部长的暗示",
                when = new When
                {
                    md = "12-15", fromYear = 2027,
                    requireMarks = new[] { "pb_1_done" },
                    requireNotMarks = new[] { "pb_2_done" },
                },
                paras = new List<string>
                {
                    "韩清来送干部推荐材料，临走时多站了半分钟。",
                    "她说得很轻：“明年省里的班子要动几个位置。大同这几年……有人看着，也有人记着。”",
                    "“有人看着”和“有人记着”，是两拨人。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "请她把话说完，问清是哪些人在看",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "pb_2_done", "pb_2_ask" },
                            political = 2, polCapital = 1, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "han", familiar = 3, trust = 2, evalv = 1, memo = "就班子风向深谈一次" } },
                            logKind = "政治", logText = "组织部：深谈班子风向",
                        },
                        result = "她只说了三个字：“你懂的。”——懂了之后，你在本子上写了两个名字，又划掉了。划掉这两个字，用了很久。",
                    },
                    new EventOption
                    {
                        label = "把话题拉回材料本身，不接话",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "pb_2_done", "pb_2_evade" },
                            professional = 1, political = -1,
                            rel = new List<RelDelta> { new RelDelta { id = "han", evalv = -1, memo = "未接班子成员动向的话头" } },
                            logKind = "政治", logText = "组织部：不接话头",
                        },
                        result = "韩清笑了笑，把材料收进包：“那我先走了。”——档案在她手里，她也确实只需要档案。",
                    },
                },
            });

            // 分叉：干净（无程序违规记录）→ 纪委谈话；有灰度 → 另一种谈话
            Flow.Register(new GameEvent
            {
                id = "ch_pb_3_clean", type = "oversight", title = "纪委的一杯茶",
                when = new When
                {
                    md = "09-20", fromYear = 2028,
                    requireMarks = new[] { "pb_1_done" },
                    requireNotMarks = new[] { "pb_3_done", "has_violation" },
                },
                paras = new List<string>
                {
                    "沈砚亲自来，不是让人叫你过去。他把茶杯放在你桌上，自己坐下。",
                    "“今天不谈案子。”他说，“大同这几年的卷宗，我们翻过。有一类件，你退得多，签得快——我想听听为什么。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把两把尺的逻辑讲给他听：退是为了不背",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "pb_3_done", "pb_3_clean_talk" },
                            political = 2, professional = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 6, evalv = 4, memo = "向纪委讲清卷宗退件的逻辑" } },
                            logKind = "监察", logText = "纪委谈话：讲清退件逻辑",
                        },
                        result = "他听完说：“程序不是挡箭牌，但没程序连挡箭牌都没有。”走的时候他把茶杯带走了——那是他自己带来的杯子。",
                    },
                    new EventOption
                    {
                        label = "不解释，只说“按规矩办”",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "pb_3_done", "pb_3_clean_short" },
                            political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 2, evalv = 1, memo = "对纪委只说了按规矩办" } },
                            logKind = "监察", logText = "纪委谈话：只答按规矩办",
                        },
                        result = "他点了点头，没再问。纪委的人最不怕你解释，最怕你不解释——这次你两样都没给他。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "ch_pb_3_dirty", type = "oversight", title = "纪委的一杯茶（不回甘）",
                when = new When
                {
                    md = "09-20", fromYear = 2028,
                    requireMarks = new[] { "pb_1_done", "has_violation" },
                    requireNotMarks = new[] { "pb_3_done" },
                },
                paras = new List<string>
                {
                    "沈砚亲自来，没带茶杯，带的是一个牛皮纸袋。",
                    "“今天不谈案子。”他把纸袋推过来，里面是你签过的三份件的复印件，“我想听听，这几笔当时是怎么想的。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "逐件说明当时依据，承认两处程序不完备",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "pb_3_done", "pb_3_dirty_own" },
                            stress = 5, political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 3, evalv = -1, memo = "对纪委逐件说明并承认瑕疵" } },
                            logKind = "监察", logText = "纪委谈话：承认程序瑕疵",
                        },
                        result = "他收起纸袋：“记下你这句话了。”——记下，是把双刃的刀：可能救你，也可能十年后拿出来对时。",
                    },
                    new EventOption
                    {
                        label = "请办公厅先补齐材料，再正式答复",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "pb_3_done", "pb_3_dirty_supp" },
                            stress = 3, political = -1,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", evalv = -4, memo = "纪委问询后先补材料再答复" } },
                            integrity = new IntegrityRecord { tag = "监察配合", note = "纪委问询后补正材料再正式答复" },
                            logKind = "监察", logText = "纪委谈话：先补材料",
                        },
                        result = "补的材料很完整，答复也准时。沈砚看完，把两份日期并排看了一眼——一份的落款日期，比问询函早了三天。他没说话，把纸袋带走了。",
                    },
                },
            });
        }

        // =====================================================================
        // 6. 同批竞争链：许飞先晋六品 → 他要数据 → 窗口前夜
        // =====================================================================

        static void RegisterRivalLine()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_rival_1", type = "politics", title = "同批第一人",
                when = new When
                {
                    md = "09-25", fromYear = 2029,
                    requireNotMarks = new[] { "rival_1_done" },
                },
                paras = new List<string>
                {
                    "许飞进了六品，任省发展和改革署副署长。名单下来的那天，你们那一批人在群里发了很长一串鼓掌的表情。",
                    "他给你发了条私信，只有四个字：“先走一步。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "回一句“路上小心”，把事记在心里",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rival_1_done", "rival_grace" },
                            morale = 4, political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = 3, trust = 3, evalv = 3, memo = "先晋六品后彼此留了体面" } },
                            logKind = "人物", logText = "许飞先晋六品：回以祝福",
                        },
                        result = "你回了四个字：“路上小心。”发出去以后，你在办公室坐了一会儿，把明年的一季度指标表翻出来看了两遍。",
                    },
                    new EventOption
                    {
                        label = "打电话过去，问清是走的哪条线",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rival_1_done", "rival_probe" },
                            political = 2, polCapital = -1, stress = 3,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = 2, trust = -1, memo = "晋升后被打听门路" } },
                            logKind = "人物", logText = "许飞先晋六品：打听门路",
                        },
                        result = "他讲了一半，都是你也知道的那一半。挂了电话你才反应过来：他讲的那一半，是讲给电话里另一个可能的听众听的。",
                    },
                    new EventOption
                    {
                        label = "不回复。把手上的活干完再说",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rival_1_done", "rival_cold" },
                            morale = -3, exec = 2, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = -2, trust = -2 } },
                            logKind = "人物", logText = "许飞先晋六品：未回复",
                        },
                        result = "那句私信在对话框里躺了很久，最后被新的工作群顶了下去。你把加班当做回应——加班不会回话，但会留下痕迹。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "ch_rival_2", type = "work", title = "他要一份数",
                when = new When
                {
                    md = "03-12", fromYear = 2030,
                    requireMarks = new[] { "rival_1_done" },
                    requireNotMarks = new[] { "rival_2_done" },
                },
                paras = new List<string>
                {
                    "许飞的秘书来电话：署里要一份大同的产业用地储备与闲置土地台账，“口径要能对得上全省的盘子”。",
                    "周谨听完先说了句：“他要的不是台账，是大同的底。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "按正式渠道报送，附文件依据",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rival_2_done", "rival_2_formal" },
                            professional = 2, political = 1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", evalv = 1, memo = "台账按正式渠道报送" } },
                            logKind = "卷宗", logText = "省署要数：按正式渠道报送",
                        },
                        result = "三天后正式函件送达。许飞那边的处长在电话里说：“就喜欢你们这样。”——喜欢，是因为挑不出毛病。",
                    },
                    new EventOption
                    {
                        label = "先给他一份“好看的”版本，正式函件后补",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rival_2_done", "rival_2_soft" },
                            comm = 2, stress = 2,
                            integrity = new IntegrityRecord { tag = "对上口径", note = "省署索取台账先报估算版" },
                            logKind = "卷宗", logText = "省署要数：先报估算版",
                        },
                        result = "估算版比台账“规整”2个百分点。许飞收了，没说什么。这两点在盘子里会变成什么，一年后才知道。",
                    },
                    new EventOption
                    {
                        label = "压一压：等省里正式要件再说",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rival_2_done", "rival_2_hold" },
                            political = 1, polCapital = -1,
                            rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = -1, trust = -1, memo = "要数被压了一周" } },
                            logKind = "卷宗", logText = "省署要数：等正式要件",
                        },
                        result = "正式要件一周后才到，材料又多报了十天。压一压不长威风，只长一个记号：下一次，他也会想起来压一压。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "ch_rival_3", type = "politics", title = "窗口前夜",
                when = new When
                {
                    md = "09-05", fromYear = 2031,
                    requireMarks = new[] { "rival_1_done" },
                    requireNotMarks = new[] { "rival_3_done" },
                },
                paras = new List<string>
                {
                    "距离六品酝酿的开窗还有半个月。韩清把大同这五年的考核表调出来，摊在会议桌上。",
                    "“两把尺都够得着边。”她说，“但省里这次有几个位置，好几个人够得着边。”",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "只摆实绩，其余交给档案",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rival_3_done", "rival_3_merit" },
                            professional = 2, political = 2, reputation = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "han", trust = 2, evalv = 2, memo = "窗口前只摆实绩" } },
                            logKind = "政治", logText = "六品窗口：只摆实绩",
                        },
                        result = "你把五年的卷宗目录放在桌上，没多说。韩清看完说了一句：“这个目录本身，就是一份材料。”",
                    },
                    new EventOption
                    {
                        label = "请老同志递话，走一走老关系",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rival_3_done", "rival_3_network" },
                            political = 1, polCapital = -4, stress = 3,
                            rel = new List<RelDelta> { new RelDelta { id = "laokang", familiar = 3, trust = 2, memo = "为我往省里递话" } },
                            integrity = new IntegrityRecord { tag = "选拔任用", note = "六品酝酿前请老同志向省里递话" },
                            logKind = "政治", logText = "六品窗口：老同志递话",
                        },
                        result = "康慎行写了封信，字很好看。信递没递到、递到谁手里，他没说。你只在心里记了一笔：这笔人情，将来要用别的方式还。",
                    },
                    new EventOption
                    {
                        label = "这次不争，把大同的事做完",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "rival_3_done", "rival_3_withdraw" },
                            morale = 2, reputation = 1, political = -2,
                            logKind = "政治", logText = "六品窗口：主动退出本轮",
                        },
                        result = "你把名字从这一轮的名单上划掉，用的是一支新笔。划完，你把笔也扔了。第二天照常批件——批得比平时慢一些。",
                    },
                },
            });
        }

        // =====================================================================
        // 7. 企业邀约链：见一面 → 条件 → 转身离开（resigned 结局）
        // =====================================================================

        static void RegisterTalentOffer()
        {
            Flow.Register(new GameEvent
            {
                id = "ch_offer_1", type = "person", title = "老同学的电话",
                when = new When
                {
                    md = "04-18", fromYear = 2029,
                    requireNotMarks = new[] { "offer_1_done" },
                },
                paras = new List<string>
                {
                    "电话是老同学打来的，现在是一家新能源集团的联席总裁。寒暄三句，拐得非常直接：",
                    "“来我们这儿吧。政府事务首席顾问，或者子公司董事长——你挑。年薪你自己填。”",
                    "窗外的产业园灯火通明。有那么一秒钟，你算了一下自己每个月的工资条。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "见一面，听他讲完",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "offer_1_done", "offer_met" },
                            stress = 3, comm = 1,
                            logKind = "人物", logText = "接到企业邀约：见面听条件",
                        },
                        result = "饭局定在城郊，包间的窗对着你签过批的那片园区。他讲股权、讲期权、讲“体制内那点钱”。你听着，偶尔点头——点头是礼貌，不是答复。",
                    },
                    new EventOption
                    {
                        label = "当场回绝，说清楚“不挪”",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "offer_1_done", "offer_refused" },
                            morale = 3, reputation = 2,
                            logKind = "人物", logText = "回绝企业邀约",
                        },
                        result = "你说完那句“不挪”，电话那头沉默了两秒，笑了：“行，那我记着你这句话。”——记着，未必是好事，也未必是坏事。",
                    },
                },
            });

            Flow.Register(new GameEvent
            {
                id = "ch_offer_2", type = "person", title = "条件",
                when = new When
                {
                    md = "05-06", fromYear = 2029,
                    requireMarks = new[] { "offer_met" },
                    requireNotMarks = new[] { "offer_2_done" },
                },
                paras = new List<string>
                {
                    "他把合同推过来，条款只有一页半：年薪、股权、子女教育、以及一条“政府关系协调职责”。",
                    "最后那一条写得很客气，也很直白：要的是你这五年攒下的东西。",
                },
                options = new List<EventOption>
                {
                    new EventOption
                    {
                        label = "把“政府关系协调”划掉，其余可以谈",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "offer_2_done", "offer_renegotiate" },
                            professional = 2, comm = 1, stress = 2,
                            logKind = "人物", logText = "企业邀约：要求删除关系协调条款",
                        },
                        result = "他盯着那道划痕看了很久：“那你去我们这儿干什么？”这个问题你答不上来——答不上来，就是答案。",
                    },
                    new EventOption
                    {
                        label = "拒绝。但把这次接触如实向组织报告",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "offer_2_done", "offer_reported" },
                            political = 2, reputation = 2, stress = 2,
                            rel = new List<RelDelta> { new RelDelta { id = "shenyan", trust = 4, evalv = 3, memo = "主动报告企业接触情况" } },
                            logKind = "监察", logText = "企业邀约：主动向组织报告",
                        },
                        result = "报告交上去的第三天，沈砚回了两个字：“收到。”——在机关，“收到”这两个字有时候是收条，有时候是护身符。",
                    },
                    new EventOption
                    {
                        // 不可逆的收束性选择放最后：自动推进/连续点击不会误触地结束游戏
                        label = "签。这十年，换个活法（进入结局）",
                        effects = new Effects
                        {
                            setMarks = new List<string> { "offer_2_done", "offer_accepted" },
                            resigned = true,
                            logKind = "系统", logText = "接受企业邀约，辞去市长职务",
                        },
                        result = "签完字，他把酒杯推过来。你没有举杯——你在想下午那份还没批的件。第二天，你走进办公厅，说了一句准备了很久的话。",
                    },
                },
            });
        }
    }
}
