using System;
using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 岗位任务生成器：按路线/科室与职级，从模板池生成日常任务事件。
    /// 模板=参数化内容（标题/正文/检定/选项），支撑十年长线的日常密度。
    /// </summary>
    public static class TaskGenerator
    {
        public class Template
        {
            public string title, note, para, main;
            public float bonus;
            public string[] routes;   // 空=通用
        }

        private static readonly List<Template> Pool = new List<Template>
        {
            // ---------- 通用 ----------
            new Template{ title="核对月度数据台账", note="数据核对", main="admin",
                para="各科室报送的台账又堆了一摞。你把每一列数字与上月对齐——差额超过一个百分点的，都用红笔圈出来待查。" },
            new Template{ title="起草一份会议通知", note="会务", main="exec",
                para="下周的协调会要发通知。参会单位名单、议程、材料目录——通知写得好，会就开得顺。" },
            new Template{ title="整理领导调研讲话提纲", note="讲话提纲", main="professional",
                para="领导后天去基层调研。提纲要短、要准、要接地气——你把去年的调研记录翻出来找感觉。" },
            new Template{ title="答复一件人大代表建议", note="建议答复", main="comm",
                para="转来一件代表建议，涉及跨部门职能。先沟通、再答复——答复函的态度，就是机关的态度。" },
            new Template{ title="接待兄弟地市考察团", note="接待", main="comm",
                para="外地发改系统来考察营商环境改革。材料、路线、口径——接待也是工作，细节里全是分寸。" },
            new Template{ title="归档上半年项目资料", note="档案整理", main="exec",
                para="档案室的铁皮柜一年比一年满。你按年份和项目把资料码齐——档案是机关的记忆。" },
            new Template{ title="校核年度目标责任分解表", note="目标分解", main="admin",
                para="把全市年度目标拆到部门、拆到季度。数字背后是责任，落笔要慎重。" },
            new Template{ title="处理一件群众来电转办件", note="群众来信", main="comm",
                para="热线转来一件咨询，反映老旧小区加装电梯的审批流程。放下电话前，你把政策依据一条条讲清楚。" },
            new Template{ title="汇编每周经济运行简报", note="周报", main="admin",
                para="把各口数据浓缩成一页纸。简报短，功夫深——哪个数字该加粗，是眼力。" },
            new Template{ title="参加政务平台事项流程梳理", note="流程梳理", main="exec",
                para="把一项审批流程从“跑三次”压到“跑一次”。流程图改到第五版，窗口的同志说办事群众笑了。" },

            // ---------- 笔杆子（综合文秘） ----------
            new Template{ title="起草局长在全市会议上的交流发言", note="发言材料", main="professional", routes=new[]{"笔杆子"},
                para="十分钟的发言，改了五遍。第三遍删掉了所有排比句——领导喜欢“有干货的话”。" },
            new Template{ title="综合科年终总结材料", note="科室总结", main="professional", routes=new[]{"笔杆子"},
                para="把全科一年的活写成三千字。写别人的成绩要具体，写自己的要含蓄——这是综合科的微妙手艺。" },
            new Template{ title="审核基层报送的请示函", note="公文审核", main="admin", routes=new[]{"笔杆子"},
                para="区里报来的请示，格式有三处不规范、依据有一处引用错误。你逐条批注，退回时附了范文。" },
            new Template{ title="整理市领导批示件督办台账", note="批示督办", main="exec", routes=new[]{"笔杆子"},
                para="领导批示件，件件要回音。台账上每一条后面都跟着一个时限——督办的艺术是提前三天提醒。" },

            // ---------- 产业经济 ----------
            new Template{ title="汇总高新区企业运行监测数据", note="产业监测", main="admin", routes=new[]{"产业经济"},
                para="AI企业的订单、用工、研发投入——三张表交叉着看。有一家的用工数据异常漂亮，先记下来。" },
            new Template{ title="起草重点产业链培育方案初稿", note="产业方案", main="professional", routes=new[]{"产业经济"},
                para="强链补链延链——方案写得热闹不难，难的是落到具体项目和具体企业。" },
            new Template{ title="对接一家拟落地的智能装备企业", note="企业服务", main="comm", routes=new[]{"产业经济"},
                para="企业问的是政策，关心的是电价、用地和上下游配套。你把能答应的、需请示的、办不到的，分三栏讲清楚。" },
            new Template{ title="测算一张技改补贴兑现清单", note="政策兑现", main="admin", routes=new[]{"产业经济"},
                para="补贴政策兑现要“快”，更要“准”。每一笔钱背后都是申报材料和审计痕迹。" },
            new Template{ title="参加服务业企业座谈会并整理纪要", note="座谈纪要", main="comm", routes=new[]{"产业经济"},
                para="企业吐苦水的多，提建议的少。你从七千字录音里拎出四条真问题——报上去的每一条都要能落地。" },

            // ---------- 投资项目 ----------
            new Template{ title="审核重点项目月度投资完成情况", note="投资调度", main="admin", routes=new[]{"投资项目"},
                para="三十六个重点项目，两个明显滞后。滞后原因栏里写着“征地拆迁”——最硬的骨头。" },
            new Template{ title="复核一份项目可行性研究报告", note="可研复核", main="professional", routes=new[]{"投资项目"},
                para="可研报告一百八十页。你重点看了投资估算和资金来源——那里最容易“乐观”。" },
            new Template{ title="梳理政府投资计划调整建议", note="计划调整", main="exec", routes=new[]{"投资项目"},
                para="钱跟着项目走，项目跟着规划走。调整建议要有理有据，才好向人大和审计交代。" },
            new Template{ title="现场踏勘一个市政基础设施项目", note="现场踏勘", main="exec", routes=new[]{"投资项目"},
                para="图纸上的线，落在地上是坑。你踩着泥看完管廊基坑，回来把设计单位约谈了一次。" },
            new Template{ title="汇总民间投资活力分析", note="投资分析", main="professional", routes=new[]{"投资项目"},
                para="民资敢不敢投，看预期。你把土地出让、订单指数、贷款利率拉成一张表——预期就藏在这些曲线里。" },

            // ---------- 区域协调 ----------
            new Template{ title="编制关中都市圈交通衔接问题清单", note="区域交通", main="admin", routes=new[]{"区域协调"},
                para="断头路、错位桥、时刻表对不上的城际班线——都市圈一体化，先从“对表”开始。" },
            new Template{ title="起草生态补偿机制试点评估", note="生态补偿", main="professional", routes=new[]{"区域协调"},
                para="上游护水、下游买单。评估写到第三稿，才把“谁受益、谁付费”算明白。" },
            new Template{ title="协调一桩跨区产业转移落地事项", note="跨区协调", main="comm", routes=new[]{"区域协调"},
                para="两个区抢一个项目，又都不想接它的配套。你把分成方案做成一张表，双方各让一步。" },
            new Template{ title="更新城镇化监测指标数据", note="城镇监测", main="admin", routes=new[]{"区域协调"},
                para="人口往哪里流，公共服务就要跟到哪里。数据更新到第三季度，几个区县的差距在缩小。" },

            // ---------- 通用扩容（Phase 4） ----------
            new Template{ title="起草节能降耗专项检查的通知", note="节能检查", main="exec",
                para="文件要管用：查什么、怎么查、查出问题怎么办。你把“配合”“支持”这类虚词删了一半。" },
            new Template{ title="汇总全市招商引资项目落地率", note="招商调度", main="admin",
                para="签约一百个，落地六十个——落地率这三个字，比签约仪式上的掌声诚实得多。" },
            new Template{ title="答复一件政协提案", note="提案答复", main="comm",
                para="提案人关心的是老旧厂区改造。答复函写了“正在研究”——你知道这四个字背后的进度条在哪里。" },
            new Template{ title="整理碳达峰重点任务进展", note="双碳台账", main="admin",
                para="能耗强度、绿电占比、技改进度——三列数字里藏着这座城市与“双碳”约定的诚实程度。" },
            new Template{ title="审核政府购买服务合同", note="合同审核", main="admin",
                para="合同里的每一个“应当”都要有下文。你把付款节点和验收标准逐条对齐——钱出去之前，先把话写死。" },
            new Template{ title="筹备营商环境座谈会的企业名单", note="会务筹备", main="exec",
                para="请谁不请谁，是门学问：既要代表性，又要敢说话。你把名单改到第四版，删了三家“永远说好话”的。" },
            new Template{ title="校核一份统计公报的相关章节", note="公报校核", main="professional",
                para="“同比增长”和“同比提高”差在哪？你逐字校了三小时——公报印出去，差一个字都是事故。" },
            new Template{ title="跟进一件市级批示件的落实", note="批示落实", main="exec",
                para="批示件的生命线是“回音”。你把牵头单位、时限、反馈方式列成一页——督办表的尽头是信任。" },
            new Template{ title="起草营商环境评价整改方案", note="整改方案", main="professional",
                para="评价结果全市第七，方案要回答两个问题：差在哪，怎么追。你把“整改”写成了可以验收的动词。" },
            new Template{ title="对接“信易贷”平台推广事项", note="信用服务", main="comm",
                para="中小企业贷款难，难在信息不对称。你把平台操作指南讲给区里的联络员——政策落地的最后一公里，靠人腿。" },

            // ---------- 产业经济扩容 ----------
            new Template{ title="起草 AI 产业专项政策的解读口径", note="政策解读", main="professional", routes=new[]{"产业经济"},
                para="企业问得最多的是“备案找谁、补贴多少、数据放哪”。你把三十页政策提炼成一页问答——解读比文件难写。" },
            new Template{ title="核查新能源电池项目的能耗承诺", note="能耗审查", main="admin", routes=new[]{"产业经济"},
                para="承诺书上的能耗指标漂亮得可疑。你调出同行业三家的实绩作参照——乐观，要用数据降温。" },
            new Template{ title="整理专精特新企业培育库", note="企业培育", main="admin", routes=new[]{"产业经济"},
                para="入库标准卡在“专精特新”四个字上。你逐家过筛子：谁是真专，谁是包装出来的专。" },
            new Template{ title="协调一场产业链供需对接会", note="供需对接", main="comm", routes=new[]{"产业经济"},
                para="上游缺订单，下游缺配套——你把两家的需求做成一张对照表。对接会的本质是翻译。" },

            // ---------- 投资项目扩容 ----------
            new Template{ title="复核政府投资项目概算调整", note="概算调整", main="admin", routes=new[]{"投资项目"},
                para="概算调增百分之十八，理由是“材料涨价”。你把涨价指数和合同条款并排一放——有些涨幅，合同里本来就该有缓冲。" },
            new Template{ title="梳理专项债项目储备清单", note="专项债", main="professional", routes=new[]{"投资项目"},
                para="专项债要“资金跟着项目走”。你把储备项目的收益平衡测算重新过了一遍——借来的钱，是要还的。" },
            new Template{ title="跟进而北片区管廊项目的移交事项", note="竣工移交", main="exec", routes=new[]{"投资项目"},
                para="建了三年的管廊要移交运营了。图纸、钥匙、债权债务——移交清单比施工图还厚。" },
            new Template{ title="起草重大项目集中开工活动的方案", note="开工活动", main="exec", routes=new[]{"投资项目"},
                para="仪式半小时，筹备两周。你把主席台、复工序、话筒数量列成清单——重大活动的成败，都在细节里埋着。" },

            // ---------- 区域协调扩容 ----------
            new Template{ title="汇总关中平原城市群合作事项进展", note="城市群协作", main="admin", routes=new[]{"区域协调"},
                para="与四个兄弟城市的合作协议签了一年，二十七项事项推进到哪一步了？你把“已办”“在办”“未启动”排成三列。" },
            new Template{ title="起草飞地园区利益分享机制建议", note="飞地机制", main="professional", routes=new[]{"区域协调"},
                para="项目落在甲地，税收想归乙地——飞地经济的账要算得两边都服气。你把分成公式写了三个方案。" },
            new Template{ title="核对跨市通勤客流调查数据", note="通勤调查", main="admin", routes=new[]{"区域协调"},
                para="早晚高峰的跨市班车挤不挤，数据说了算。你把三天的闸机数据拉出来——交通规划的浪漫，全在客流曲线里。" },
            new Template{ title="参与生态补偿协议的年度评估", note="生态补偿", main="comm", routes=new[]{"区域协调"},
                para="上游护水，下游付钱，一年一算账。评估会上你负责念数据——数据念得平，两边的火气就小一半。" },
        };

        private static readonly Random Rng = new Random();

        public static GameEvent Generate(GameState st, DateTime d)
        {
            // 路线偏好：六成概率从路线相关模板里抽，其余走进通用池。
            // （旧实现是硬过滤：route 一旦与模板标籾不匹配，可选模板会解减一半；
            //   而且路线键已改为 Phase 5 的 industry/people/project/uplink，需要映射。）
            string legacy = LegacyRoute(st.route);
            var all = new List<Template>();
            var preferred = new List<Template>();
            foreach (var t in Pool)
            {
                all.Add(t);
                if (legacy != null && t.routes != null && Array.IndexOf(t.routes, legacy) >= 0) preferred.Add(t);
            }
            var candidates = (preferred.Count > 0 && Rng.NextDouble() < 0.6) ? preferred : all;
            var t2 = candidates[Rng.Next(candidates.Count)];

            // 决策骨架抽签：例行 / 限时交办 / 协作 / 有程序风险 / 当众露脸
            double roll = Rng.NextDouble();
            string kind = roll < 0.55 ? "routine"
                        : roll < 0.70 ? "deadline"
                        : roll < 0.82 ? "coop"
                        : roll < 0.92 ? "risk"
                        : "stage";
            return BuildByKind(st, t2, kind);
        }

        /// <summary>Phase 5 路线键 → 本文件旧科员线模板的标签（null = 不做偏好，走通用池）。
        /// 民生兜底暂无对应池子，故意返回 null（宁可不偏好，也不要拿错池子替换日常）。</summary>
        static string LegacyRoute(string route)
        {
            switch (route)
            {
                case "industry": return "产业经济";
                case "project": return "投资项目";
                case "uplink": return "区域协调";
            }
            return null;
        }

        /// <summary>同一任务主题 × 五种决策骨架：选项结构、代价与回报各不相同。</summary>
        private static GameEvent BuildByKind(GameState st, Template t2, string kind)
        {
            string note = t2.note;
            var paras = new List<string> { t2.para };
            var opts = new List<EventOption>();
            var sig = SignatureOf(st);
            var gb = GradeBonus(st);

            if (kind == "deadline")
            {
                paras.Add("这件活带着时限——周五上午分管副局长要听汇报，材料必须提前成稿。");
                opts.Add(new EventOption
                {
                    label = "连着几天加班，提前两天交上去",
                    check = new Check { main = t2.main, bonus = 0.15f + gb },
                    effects = new Effects { energy = -15, stress = 3, task = new TaskRecord { title = t2.title, note = note + "（提前完成）", signature = sig } },
                    result = "第三天凌晨你按了保存。材料提前两天摆在科长桌上，他翻完只说了一个字：“稳。”{grade}。"
                });
                opts.Add(new EventOption
                {
                    label = "按部就班，卡着节点交",
                    check = new Check { main = t2.main, bonus = gb },
                    effects = new Effects { energy = -8, task = new TaskRecord { title = t2.title, note = note, signature = sig } },
                    result = "周四下班前交了稿。时间掐得刚好——机关里，“刚好”也是一种能力。{grade}。"
                });
                opts.Add(new EventOption
                {
                    label = "向科长请示，宽限两天",
                    effects = new Effects { energy = -4, stress = -1, rel = new List<RelDelta> { new RelDelta { id = "zhou", evalv = -1 } },
                        task = new TaskRecord { title = t2.title, note = note + "（申请延期）", signature = sig, grade = "C" } },
                    result = "周衡之盯着你看了两秒，批了。期限是松了——但“请示延期”这四个字，也留在他的小本子上了。"
                });
            }
            else if (kind == "coop")
            {
                paras.Add("这活一个人干不完——数据要两个处室对，材料要两个口子合，就看你把人聚得起来聚不起来。");
                opts.Add(new EventOption
                {
                    label = "自己啃下来，一家一家去磨",
                    check = new Check { main = t2.main, bonus = 0.1f + gb },
                    effects = new Effects { energy = -12, professional = 1, task = new TaskRecord { title = t2.title, note = note + "（独立完成）", signature = sig } },
                    result = "你跑了三个科室、打了一下午电话，把所有口径亲手对齐。累是真累，长进也是真长进。{grade}。"
                });
                opts.Add(new EventOption
                {
                    label = "拉上同批新人分头干",
                    check = new Check { main = t2.main, bonus = 0.05f + gb },
                    effects = new Effects { energy = -7, comm = 1, rel = new List<RelDelta> { new RelDelta { id = "xu", familiar = 2 }, new RelDelta { id = "su", familiar = 2 }, new RelDelta { id = "he", familiar = 1 } },
                        task = new TaskRecord { title = t2.title, note = note + "（协作完成）", signature = sig } },
                    result = "你把活拆成三块，微信群里两天对完。有人私下说你会“用人”——这算不算夸，你还没想明白。{grade}。"
                });
                opts.Add(new EventOption
                {
                    label = "请赵姐把把关再合稿",
                    check = new Check { main = t2.main, bonus = 0.08f + gb },
                    effects = new Effects { energy = -6, admin = 1, rel = new List<RelDelta> { new RelDelta { id = "zhao", familiar = 2, trust = 1 } },
                        task = new TaskRecord { title = t2.title, note = note + "（老同志把关）", signature = sig } },
                    result = "赵姐用红笔圈了四处，嘴上说着“现在的年轻人”。稿子顺了，人情也记下了。{grade}。"
                });
            }
            else if (kind == "risk")
            {
                paras.Add("这事的口径有点微妙：按规范走最稳，但有更快的路——快路，从来不是白来的。");
                opts.Add(new EventOption
                {
                    label = "按规范流程来，多花两天",
                    check = new Check { main = t2.main, bonus = 0.05f + gb },
                    effects = new Effects { energy = -10, admin = 1, task = new TaskRecord { title = t2.title, note = note + "（全程留痕）", signature = sig } },
                    result = "你把每一步都留了痕：会议纪要、签批单、核对记录。慢是慢了点，可这摞纸经得起任何人翻。{grade}。"
                });
                opts.Add(new EventOption
                {
                    label = "用变通口径，先出结果再说",
                    effects = new Effects { energy = -6, exec = 1,
                        integrity = new IntegrityRecord { tag = "口径变通", note = "未按规范流程留痕，先行办结" },
                        task = new TaskRecord { title = t2.title, note = note + "（口径变通）", signature = sig, grade = "A" } },
                    result = "结果第二天就出了，上面很满意。只有你知道，档案柜里少了两页纸——这类账，迟早要还。"
                });
                opts.Add(new EventOption
                {
                    label = "上报请示，把决定权交给科长",
                    effects = new Effects { energy = -7, rel = new List<RelDelta> { new RelDelta { id = "zhou", trust = 1, evalv = 1 } },
                        task = new TaskRecord { title = t2.title, note = note + "（请示后办理）", signature = sig, grade = "B" } },
                    result = "周衡之听完，只说了句“按第二种口径办，纪要写清楚”。锅有人背，路有人指——这就是科长的用处。"
                });
            }
            else if (kind == "stage")
            {
                paras.Add("干好了，分管副局长看得见；干砸了，也是当着副局长砸。");
                opts.Add(new EventOption
                {
                    label = "精心准备，把这次露脸接住",
                    check = new Check { main = t2.main, bonus = 0.12f + gb },
                    effects = new Effects { energy = -12, reputation = 1, task = new TaskRecord { title = t2.title, note = note + "（当众汇报）", signature = sig } },
                    result = "汇报十五分钟，副局长抬头看了你三次。散会后马建国路过你工位，点了点头——什么都没说，什么都说了。{grade}。"
                });
                opts.Add(new EventOption
                {
                    label = "低调把活干好，风头让给科长",
                    check = new Check { main = t2.main, bonus = gb },
                    effects = new Effects { energy = -7, task = new TaskRecord { title = t2.title, note = note, signature = sig } },
                    result = "你把汇报稿写完，把上台的机会留给了周衡之。他看懂了——有些投资，回报慢但稳。{grade}。"
                });
            }
            else // routine：例行骨架（认真 / 交差）
            {
                opts.Add(new EventOption
                {
                    label = "沉下心把它做扎实",
                    check = new Check { main = t2.main, bonus = 0.05f + gb },
                    effects = new Effects { energy = -9, stress = st.player.stress > 65 ? 2 : 1, task = new TaskRecord { title = t2.title, note = note, signature = sig } },
                    result = "做完抬头，窗外天色已沉。这件活办得{grade}——档案上又多了一行。"
                });
                opts.Add(new EventOption
                {
                    label = "按部就班交差",
                    check = new Check { main = t2.main, bonus = gb - 0.1f },
                    effects = new Effects { energy = -5, task = new TaskRecord { title = t2.title, note = note, signature = sig } },
                    result = "活交了，不算出彩，也挑不出错。{grade}。"
                });
            }

            ApplyKindAttr(opts, t2.main, kind);
            return new GameEvent { id = "_gen_task", type = "work", title = t2.title, paras = paras, options = opts };
        }

        /// <summary>给带检定且无属性增量的选项补一点属性成长（按检定主属性）。</summary>
        private static void ApplyKindAttr(List<EventOption> opts, string main, string kind)
        {
            if (kind == "risk" || kind == "deadline") return;   // 这两类靠结果说话，不重复给成长
            foreach (var o in opts)
                if (o.check != null) ApplyAttr(o.effects, main, 1);
        }

        private static float GradeBonus(GameState st)
        {
            if (st.grade.StartsWith("吏二")) return 0.05f;
            if (st.grade.StartsWith("吏一")) return 0.1f;
            if (st.grade.StartsWith("十品")) return 0.15f;
            return 0f;
        }

        private static string SignatureOf(GameState st)
        {
            if (st.grade.StartsWith("吏一") || st.grade.StartsWith("十品")) return "主笔（复核他人）";
            if (st.grade.StartsWith("吏二")) return "主笔";
            return "参与";
        }

        /// <summary>按属性名写入单个属性增量。</summary>
        private static void ApplyAttr(Effects e, string main, int v)
        {
            switch (main)
            {
                case "professional": e.professional += v; break;
                case "admin": e.admin += v; break;
                case "exec": e.exec += v; break;
                case "comm": e.comm += v; break;
                case "political": e.political += v; break;
            }
        }
    }
}
