using System;
using System.Collections;
using System.IO;
using Starstate.Core;
using UnityEngine;

namespace Starstate.Ui
{
    /// <summary>
    /// 组合根：菜单 → 主循环 → 自动存档；大模型增强层接线（NPC 交谈 / AI 日常小事件）。
    /// LLM 只增强文本：不可用/关闭时全部走原逻辑（内置台词池、原随机事件）。
    /// </summary>
    public class GameApp : MonoBehaviour
    {
        public UiRoot ui;

        private GameState st;
        private string tab = "状态";
        private string lastKind = "";
        private const int SaveVersion = 5;   // v5：Phase 5 七品市长＋卷宗引擎（旧档引导重开）
        private const string FontScaleKey = "starstate_font_scale";
        private const string LlmKey = "starstate_llm_cfg3";   // v3：默认 autoStart=false、ctx=16384（降启动占用）

        // —— LLM ——
        private LlmConfig llm;
        private bool llmReady;          // 服务可用（本地已就绪 / 远程默认按可用处理）
        private bool llmChecking;
        private bool microBusy;
        private bool ambienceBusy, remarkBusy;   // M2：氛围句/批示在途
        private int ambienceSession;

        // —— NPC 交谈状态机 ——
        private string talkNpc;
        private string talkStage;       // loading / choose / result
        private NpcTalkDto talkDto;
        private bool talkBusy;
        private int talkSession;        // 交谈会话号：关闭弹层后作废在途响应

        // —— AI 家信 / 微信 ——
        private bool letterBusy;
        private int letterSession;

        // —— 多存档槽：0=自动档，1–3=手动槽 ——
        private int activeSlot;
        private const int SlotCount = 3;

        private readonly System.Random uiRng = new System.Random();

        private static string AutoPath
        {
            get { return Path.Combine(Application.persistentDataPath, "starstate_save.json"); }
        }

        private static string SlotPath(int slot)
        {
            return slot <= 0 ? AutoPath
                : Path.Combine(Application.persistentDataPath, "starstate_save_s" + slot + ".json");
        }

        private static bool HasSlot(int slot)
        {
            try { return File.Exists(SlotPath(slot)); }
            catch { return false; }
        }

        public void Init(UiRoot root)
        {
            // 内容注册：必须早于任何 State.NewGame / Flow.Begin（旧版从未在运行时调用，
            // 导致 Play/出货时序章、卷宗、年度考核、结局全是空的——测试因为各自调了 RegisterAll 而看不到）
            ContentRegistry.RegisterAll();
            GameLog.Sink = Debug.Log;   // Core 纯 C#，日志出口在 Ui 层接线
            ui = root;
            ui.OnOptionChosen += Choose;
            ui.OnPlanAdjusted += PlanAdjusted;
            ui.OnTabSwitched += t => { tab = t; RenderSide(); };
            ui.OnRuleOpen += OpenRule;
            ui.OnNewGame += StartNew;
            ui.OnQuit += Quit;
            ui.OnFastForward += FastForward;
            ui.OnOpenSettings += () => ui.ShowSettings();
            ui.OnFontScaleChanged += SetFontScale;
            ui.OnResetSave += ResetSave;
            ui.OnNpcTalk += StartTalk;
            ui.OnTalkOption += TalkOption;
            ui.OnTalkClose += CloseTalk;
            ui.OnLlmApplied += OnLlmApplied;
            ui.OnLlmTest += TestLlm;
            ui.OnContinueSlot += ContinueSlot;
            ui.OnSnapshotSlot += SnapshotSlot;

            llm = LoadLlm();
            ui.SetLlmConfig(llm);
            if (llm.mode == "remote") llmReady = true;
            if (llm.enabled && llm.mode == "local" && llm.autoStart) KickServer("正在启动本地模型服务…");
            UpdateAiBadge();
            RefreshMenuSaves();
        }

        private void OnDestroy()
        {
            FlushSaveSync();   // 退出前兜底同步落盘（异步写可能在途）
            LlamaServer.Kill();
        }

        // ---------------- LLM 接入 ----------------

        private static LlmConfig LoadLlm()
        {
            try
            {
                string s = PlayerPrefs.GetString(LlmKey, "");
                if (!string.IsNullOrEmpty(s)) return JsonUtility.FromJson<LlmConfig>(s) ?? new LlmConfig();
            }
            catch { /* 配置损坏则用默认 */ }
            return new LlmConfig();
        }

        private void SaveLlm()
        {
            try
            {
                PlayerPrefs.SetString(LlmKey, JsonUtility.ToJson(llm));
                PlayerPrefs.Save();
            }
            catch { /* ignore */ }
        }

        private void OnLlmApplied()
        {
            SaveLlm();
            ui.SetLlmConfig(llm);
            UpdateAiBadge();
            if (llm.enabled && llm.mode == "local" && llm.autoStart && !LlamaServer.Running && !llmChecking)
                KickServer("正在启动本地模型服务…");
        }

        private void UpdateAiBadge()
        {
            if (llm == null || !llm.enabled) { ui.SetAiBadge(""); return; }
            if (llmChecking) ui.SetAiBadge("AI 唤醒中…");
            else if (llmReady) ui.SetAiBadge("AI 就绪");
            else ui.SetAiBadge("AI 未连接");
        }

        private void KickServer(string initial)
        {
            llmChecking = true;
            UpdateAiBadge();
            ui.ShowLlmStatus(initial);
            StartCoroutine(LlamaServer.EnsureRunning(llm, s => ui.ShowLlmStatus(s), ok =>
            {
                llmChecking = false;
                llmReady = ok;
                UpdateAiBadge();
                if (ok && st != null) RenderSide(); // 人物页签等不受影响，仅刷新状态
            }));
        }

        private void TestLlm()
        {
            ui.ShowLlmStatus("测试连接中…");
            if (llm.enabled && llm.mode == "local")
            {
                StartCoroutine(TestFlow());
                return;
            }
            StartCoroutine(TestChat());
        }

        private IEnumerator TestFlow()
        {
            // 始终走 EnsureRunning：已有外部实例/加载中只会等待，不会双开
            bool ok = false, settled = false;
            yield return LlamaServer.EnsureRunning(llm, s => ui.ShowLlmStatus(s), r => { ok = r; settled = true; });
            while (!settled) yield return null;
            llmReady = ok;
            UpdateAiBadge();
            if (!ok) { ui.ShowLlmStatus("✗ 本地服务未能就绪（见上方提示）。"); yield break; }
            yield return TestChat();
        }

        private IEnumerator TestChat()
        {
            yield return LlmClient.Chat(llm, new[]
            {
                new ChatMsg { role = "system", content = LlmPrompt.System() },
                new ChatMsg { role = "user", content = "连通性测试。请只输出：{\"ok\":1}" },
            }, 24,
            ok => ui.ShowLlmStatus("✓ 连接成功，模型已响应。AI 增强可用。"),
            err => ui.ShowLlmStatus("✗ " + err));
        }

        private IEnumerator ChatOnce(ChatMsg[] msgs, int maxTokens, Action<string> ok, Action<string> err, Func<bool> cancelled = null)
        {
            yield return LlmClient.Chat(llm, msgs, maxTokens, ok, err, cancelled);
        }

        // ---------------- NPC 交谈 ----------------

        private void StartTalk(string npcId)
        {
            if (st == null || talkBusy) return;
            talkBusy = true;
            talkNpc = npcId;
            talkStage = "loading";
            talkSession++;
            int session = talkSession;
            ui.ShowTalk(Npcs.Name(npcId));
            Npcs.Ensure(st);

            if (!llm.enabled)
            {
                FinishTalk(ContentNpcTalk.FallbackTalk(st, npcId), null, session);
                return;
            }
            if (llm.mode == "remote" || llmReady)
            {
                ui.ShowTalkStatus("正在请求 AI……");
                RunTalkGeneration(npcId, session);
                return;
            }
            // 本地未就绪：现场唤醒（而不是静默退回内置台词）
            ui.ShowTalkStatus("正在唤醒本地模型（首次约需一两分钟）……");
            StartCoroutine(WakeThenTalk(npcId, session));
        }

        private IEnumerator WakeThenTalk(string npcId, int session)
        {
            bool ok = false, settled = false;
            yield return LlamaServer.EnsureRunning(llm, s => { if (session == talkSession) ui.ShowTalkStatus(s); },
                r => { ok = r; settled = true; });
            while (!settled) yield return null;
            llmChecking = false;
            llmReady = ok;
            UpdateAiBadge();
            if (session != talkSession) yield break;   // 弹层已被关闭
            if (!ok)
            {
                FinishTalk(ContentNpcTalk.FallbackTalk(st, npcId), "AI 唤醒失败，本次使用内置台词（可在设置中测试连接）。", session);
                yield break;
            }
            RunTalkGeneration(npcId, session);
        }

        private void RunTalkGeneration(string npcId, int session)
        {
            Func<bool> cancelled = () => session != talkSession;   // 弹层关闭即中止在途请求
            StartCoroutine(LlmClient.ChatStream(llm, new[]
            {
                new ChatMsg { role = "system", content = LlmPrompt.System() },
                new ChatMsg { role = "user", content = LlmPrompt.NpcTalkUser(st, npcId) },
            }, 720,
            partial =>
            {
                if (session != talkSession) return;
                int n = partial != null ? partial.Length : 0;
                ui.ShowTalkStatus("正在生成…（已 " + n + " 字）");
            },
            content =>
            {
                if (session != talkSession) return;    // 弹层已关闭，丢弃在途响应
                var dto = LlmJson.ParseTalk(content);
                if (dto == null)
                    FinishTalk(ContentNpcTalk.FallbackTalk(st, npcId), "AI 返回内容无法解析，本次使用内置台词。", session);
                else
                    FinishTalk(dto, null, session);
            },
            e =>
            {
                if (session != talkSession) return;
                FinishTalk(ContentNpcTalk.FallbackTalk(st, npcId), "AI 未接入：" + e + "（使用内置台词）", session);
            }, cancelled));
        }

        private void FinishTalk(NpcTalkDto dto, string statusNote, int session)
        {
            if (session != talkSession || talkStage != "loading") return;
            talkDto = dto;
            talkStage = "choose";
            talkBusy = false;
            if (statusNote == null) ui.HideTalkStatus();
            else ui.ShowTalkStatus(statusNote);
            ui.AddTalkPara(dto.greeting);
            if (dto.lines != null)
                foreach (var l in dto.lines)
                    if (!string.IsNullOrEmpty(l)) ui.AddTalkPara(l);
            var labels = new string[dto.options.Length];
            for (int i = 0; i < dto.options.Length; i++) labels[i] = dto.options[i].label;
            ui.SetTalkOptions(labels);
        }

        private void TalkOption(int idx)
        {
            if (talkStage == "choose" && talkDto != null)
            {
                var opt = talkDto.options[Math.Max(0, Math.Min(talkDto.options.Length - 1, idx))];
                string summary = LlmGameplay.ApplyTalk(st, talkNpc, opt);
                Save();
                ui.AddTalkPara("你：" + opt.label);
                ui.AddTalkPara(Npcs.Name(talkNpc) + "：" + (string.IsNullOrEmpty(opt.reply) ? "……" : opt.reply));
                ui.AddTalkPara("（" + summary + "）");
                ui.SetTalkOptions(new[] { "再聊一句", "告 辞" });
                talkStage = "result";
                RenderSide(); // 关系数值即时刷新
                return;
            }
            if (talkStage == "result")
            {
                if (idx == 0) { StartTalk(talkNpc); return; }
                CloseTalk();
                return;
            }
            CloseTalk();
        }

        private void CloseTalk()
        {
            talkSession++;             // 作废所有在途响应
            ui.HideTalk();
            talkStage = null;
            talkBusy = false;
        }

        // ---------------- AI 日常小事件（点缀普通"day"，白名单效果） ----------------

        private void MaybeMicro(Scene scene)
        {
            if (llm == null || !llm.enabled || !llmReady || microBusy) return;
            if (st == null || st.phase != Phase.Day || st.hasPending) return;
            if (scene.kind != "day") return;                                  // 只点缀普通日常
            if (st.lastAiMicro == st.date) return;                            // 每日最多一次
            if (st.queue.Count > 0 || st.runtimeEvent == null
                || st.runtimeEvent.id != "_generic_day") return;              // 只替换伪事件
            st.lastAiMicro = st.date;                                         // 先占位，失败也不重试
            if (uiRng.NextDouble() > 0.45) { Save(); return; }
            microBusy = true;
            StartCoroutine(ChatOnce(new[]
            {
                new ChatMsg { role = "system", content = LlmPrompt.System() },
                new ChatMsg { role = "user", content = LlmPrompt.MicroUser(st) },
            }, 460,
            content =>
            {
                microBusy = false;
                var ev = LlmGameplay.BuildMicroEvent(st, LlmJson.ParseMicro(content));
                if (ev != null && st.phase == Phase.Day && !st.hasPending
                    && st.runtimeEvent != null && st.runtimeEvent.id == "_generic_day"
                    && st.queue.Count == 0)
                {
                    st.runtimeEvent = ev;
                    Save();
                    RenderAll();
                }
            },
            e => { microBusy = false; }));
        }

        // ---------------- 菜单与主循环 ----------------

        private static float GetFontScale()
        {
            return PlayerPrefs.GetFloat(FontScaleKey, 1f);
        }

        private void SetFontScale(float s)
        {
            PlayerPrefs.SetFloat(FontScaleKey, s);
            PlayerPrefs.Save();
            ui.Build(GameBootstrap.MakeFont(), GameBootstrap.MakeDocFont(), s);   // 重建界面以应用字号（事件订阅保留在 UiRoot 实例上）
            ui.SetLlmConfig(llm);
            UpdateAiBadge();
            ui.HideOverlays();
            if (st != null) RenderAll(); else RefreshMenuSaves();
        }

        private void ResetSave()
        {
            lock (SaveLock)
            {
                try { if (File.Exists(SlotPath(activeSlot))) File.Delete(SlotPath(activeSlot)); } catch { /* ignore */ }
                SavedTicket.Remove(SlotPath(activeSlot));
            }
            st = null;
            ui.SetSettingsHint("当前槽位存档已重置。点击“返回”回到主菜单。");
            RefreshMenuSaves();
        }

        private void RefreshMenuSaves()
        {
            var slots = new System.Collections.Generic.List<int>();
            var labels = new System.Collections.Generic.List<string>();
            for (int i = 0; i <= SlotCount; i++)
            {
                if (!HasSlot(i)) continue;
                slots.Add(i);
                labels.Add(SlotLabel(i));
            }
            ui.ShowMenu(slots.ToArray(), labels.ToArray());
        }

        private static string SlotLabel(int slot)
        {
            string head = slot == 0 ? "自动档" : "槽位 " + slot;
            try
            {
                var s = LoadFromPath(SlotPath(slot));
                if (s == null) return head;
                return head + " · " + s.date + " · " + (s.player != null ? s.player.name : "?") + " · " + s.grade;
            }
            catch { return head; }
        }

        private void StartNew()
        {
            activeSlot = PickFreeSlot();
            st = State.NewGame(ui.NameInput());
            st.saveVersion = SaveVersion;
            Flow.Begin(st);
            Save();
            ui.HideOverlays();
            RenderAll();
            ui.ShowTutorial();   // 新局自动播放新手引导（主菜单也可重看）
        }

        /// <summary>新局优先写入手动空槽（1–3），全满则覆盖自动档。</summary>
        private int PickFreeSlot()
        {
            for (int i = 1; i <= SlotCount; i++)
                if (!HasSlot(i)) return i;
            return 0;
        }

        private void ContinueSlot(int slot)
        {
            st = LoadFromPath(SlotPath(slot));
            if (st == null || st.saveVersion != SaveVersion)
            {
                ui.SetMenuHint("检测到旧版本存档（格式已升级到 v5），无法继续——已为你开始新的一局。");
                activeSlot = slot;
                st = State.NewGame(ui.NameInput());
                st.saveVersion = SaveVersion;
                Flow.Begin(st);
                Save();
                ui.HideOverlays();
                RenderAll();
                return;
            }
            activeSlot = slot;
            Npcs.Ensure(st);
            ui.HideOverlays();
            RenderAll();
        }

        /// <summary>把当前局快照到指定手动槽（1–3）。</summary>
        private void SnapshotSlot(int slot)
        {
            if (st == null || slot < 1 || slot > SlotCount) return;
            try
            {
                st.saveVersion = SaveVersion;
                string json = JsonUtility.ToJson(st);
                if (string.IsNullOrEmpty(json) || json.Length < 16) return;
                long ticket = System.Threading.Interlocked.Increment(ref saveTicket);
                lock (SaveLock)
                {
                    // 快照走与自动存档同一把锁＋同一序号体系，避免两边并发写同一文件
                    WriteOneAtomic(json, SlotPath(slot), ticket);
                    WriteOneAtomic(json, AutoPath, ticket);
                }
                ui.SetSettingsHint("已快照到槽位 " + slot + "。");
            }
            catch (System.Exception e)
            {
                ui.SetSettingsHint("快照失败：" + e.Message);
            }
        }

        private void Choose(int idx)
        {
            if (st == null) return;
            if (lastKind == "ending")
            {
                if (idx == 0) { StartNew(); return; }
                Quit();
                return;
            }
            bool wasWeekEnd = st.phase == Phase.WeekEnd;
            Flow.Choose(st, idx);
            Save();
            RenderAll();
            if (wasWeekEnd && llm != null && llm.enabled && llmReady && !microBusy)
                StartCoroutine(RequestWeeklyReview());
        }

        /// <summary>口径手册：摊开/收起（""=收起）。</summary>
        private void OpenRule(string ruleId)
        {
            if (st == null) return;
            if (string.IsNullOrEmpty(ruleId)) Rulebook.CloseDesk(st);
            else Rulebook.Open(st, ruleId);
            Save();
            RenderSide();
            // 卷宗正文里的案头提示行要跟上
            if (lastKind == "dossier")
            {
                var scene = Flow.CurrentScene(st);
                ui.RenderMain(scene);
            }
        }

        /// <summary>AI 科长周评：周五点评后追加一段引用本周真实事件的评语（失败静默）。</summary>
        private IEnumerator RequestWeeklyReview()
        {
            microBusy = true;
            yield return ChatOnce(new[]
            {
                new ChatMsg { role = "system", content = LlmPrompt.System() },
                new ChatMsg { role = "user", content = LlmPrompt.WeekReviewUser(st) },
            }, 280,
            content =>
            {
                microBusy = false;
                string text = StripPlain(content);
                if (!string.IsNullOrEmpty(text) && st != null && st.hasPending)
                    ui.AppendBodyPara("周谨合上记录本，补了一句：" + text);
            },
            e => { microBusy = false; });
        }

        /// <summary>剥离代码块围栏与首尾引号，压缩空白（纯文本回复用）。</summary>
        private static string StripPlain(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("```", "\n");
            var lines = s.Split('\n');
            var keep = new System.Collections.Generic.List<string>();
            foreach (var l in lines)
            {
                var t = l.Trim().Trim('"');
                if (!string.IsNullOrEmpty(t) && !t.StartsWith("{") && !t.StartsWith("}")) keep.Add(t);
            }
            var joined = string.Join(" ", keep.ToArray());
            return joined.Length > 90 ? joined.Substring(0, 90) : joined;
        }

        /// <summary>周计划编辑器：±槽位——只做局部数值刷新，不重建整树（无闪烁、无 GC 尖峰）。</summary>
        private void PlanAdjusted(int slot, int delta)
        {
            if (st == null) return;
            Flow.AdjustPlan(st, slot, delta);
            Save();
            ui.UpdatePlanEditor(new[] { st.plan.work, st.plan.study, st.plan.social, st.plan.family, st.plan.rest });
        }

        private void FastForward()
        {
            if (st == null || lastKind == "ending") return;
            // 结果页与结构阶段（周计划/周点评/周末/月结）：推进＝选默认项
            if (st.hasPending)
            {
                Flow.Choose(st, 0);
                Save();
                RenderAll();
                return;
            }
            switch (st.phase)
            {
                case Phase.WeekPlan:
                case Phase.WeekEnd:
                case Phase.Weekend:
                case Phase.MonthEnd:
                    Flow.Choose(st, 0);
                    Save();
                    RenderAll();
                    return;
            }
            if (st.phase != Phase.Day) return; // 序章事件含关键抉择，不代选
            // 屏上是日常/岗位任务：选默认项（与快进内部行为一致，一步一停）
            if (st.runtimeEvent == null || Flow.IsGeneratedDaily(st.runtimeEvent))
            {
                Flow.Choose(st, 0);
                Save();
                RenderAll();
                return;
            }
            // 其余情况（剧情/动态抉择事件）引擎护栏会拒绝——按钮此时应已置灰
            Flow.FastForward(st);
            Save();
            RenderAll();
        }

        private void RenderAll()
        {
            ui.RenderTop(st);
            var scene = Flow.CurrentScene(st);
            lastKind = scene.kind;
            ui.RenderMain(scene);
            RenderSide();
            MaybeMicro(scene);
            MaybeAmbience(scene);
            MaybeRemark();
            MaybeAiMail();
            UpdateFfButton();
        }

        // ---------------- M2：每日氛围句 + 批示涓色（失败静默，都有确定性回退） ----------------

        /// <summary>每日氛围一句：普通工作日首次渲染时请求一次（当日不重试），无 AI 时该行不显示。</summary>
        private void MaybeAmbience(Scene scene)
        {
            if (llm == null || !llm.enabled || !llmReady || ambienceBusy) return;
            if (st == null || st.phase != Phase.Day || st.hasPending) return;
            if (scene.kind != "day") return;
            if (st.lastAiAmbience == st.date) return;
            st.lastAiAmbience = st.date;   // 先占位：失败也不重试（防止每帧重试）
            Save();
            ambienceBusy = true;
            ambienceSession++;
            StartCoroutine(RequestAmbience(ambienceSession));
        }

        private IEnumerator RequestAmbience(int session)
        {
            string date = st != null ? st.date : "";
            yield return ChatOnce(new[]
            {
                new ChatMsg { role = "system", content = LlmPrompt.System() },
                new ChatMsg { role = "user", content = LlmPrompt.AmbienceUser(st) },
            }, 120,
            content =>
            {
                ambienceBusy = false;
                if (session != ambienceSession || st == null || st.date != date) return;   // 已过日/已重开，结果作废
                string t = LlmGameplay.SanitizeAmbience(content);
                if (string.IsNullOrEmpty(t)) return;
                st.ambience = t;
                Save();
                ui.SetAmbience(t);
            },
            e => { ambienceBusy = false; });
        }

        /// <summary>
        /// 批示涓色：签批结果页先把 Core 的确定性批语渲染出来（无 AI 也有），
        /// 有 AI 时再异步请模型涓成一句公文批语；回来后写存档并重绘（场景签名变了会重建）。
        /// </summary>
        private void MaybeRemark()
        {
            if (llm == null || !llm.enabled || !llmReady || remarkBusy) return;
            if (st == null || !st.hasPending) return;
            if (string.IsNullOrEmpty(st.remarkDossier) || st.remarkDossier != st.pendingDossierId) return;
            if (st.remarkAi) return;                                    // 已经涓过了
            if (st.remarkAiTried == st.remarkDossier) return;           // 这件已经试过、失败不再重试
            st.remarkAiTried = st.remarkDossier;
            Save();
            remarkBusy = true;
            StartCoroutine(RequestRemark());
        }

        private IEnumerator RequestRemark()
        {
            string id = st.remarkDossier;
            string fallback = st.remark;
            var dl = st.dossierLog;
            string title = st.pendingTitle;
            string label = "";
            for (int i = dl.Count - 1; i >= 0; i--)
                if (dl[i].dossierId == id) { label = dl[i].disposition; break; }
            string resultText = (st.pendingParas != null && st.pendingParas.Count > 0) ? st.pendingParas[0] : "";

            yield return ChatOnce(new[]
            {
                new ChatMsg { role = "system", content = LlmPrompt.System() },
                new ChatMsg { role = "user", content = LlmPrompt.RemarkUser(st, title, label, resultText) },
            }, 96,
            content =>
            {
                remarkBusy = false;
                if (st == null || st.remarkDossier != id) return;
                string t = LlmGameplay.SanitizeRemark(content);
                if (string.IsNullOrEmpty(t) || t == fallback) return;   // 涓色无变化则保留回退
                st.remark = t;
                st.remarkAi = true;
                Save();
                RenderAll();
            },
            e => { remarkBusy = false; });
        }

        /// <summary>AI 家信：每月一次（周末/月结附近），失败静默；同时偶发同事微信进日志。</summary>
        private void MaybeAiMail()
        {
            if (st == null || llm == null || !llm.enabled || !llmReady || letterBusy) return;
            if (st.phase != Phase.Weekend && st.phase != Phase.MonthEnd && st.phase != Phase.WeekEnd) return;
            if (st.date == null || st.date.Length < 7) return;
            string key = st.date.Substring(0, 7);
            if (st.lastLetterMonth == key) return;
            var d = GameClock.Parse(st.date);
            if (d.Day > 14) { st.lastLetterMonth = key; Save(); return; }   // 月下半段不再打扰
            st.lastLetterMonth = key;
            Save();
            letterBusy = true;
            letterSession++;
            int session = letterSession;
            StartCoroutine(RequestFamilyLetter(session));
        }

        private IEnumerator RequestFamilyLetter(int session)
        {
            yield return LlmClient.Chat(llm, new[]
            {
                new ChatMsg { role = "system", content = LlmPrompt.System() },
                new ChatMsg { role = "user", content = LlmPrompt.FamilyLetterUser(st) },
            }, 360,
            content =>
            {
                if (session != letterSession || st == null) { letterBusy = false; return; }
                letterBusy = false;
                string text = StripPlain(content);
                if (string.IsNullOrEmpty(text)) return;
                st.AddLog("家书", text);
                Save();
                // 有结果页时补一段；否则只进日志，不打断主叙事
                if (st.hasPending) ui.AppendBodyPara("—— 家里来信 ——" + text);
                RenderSide();
                MaybeWeChat();
            },
            e => { letterBusy = false; });
        }

        private void MaybeWeChat()
        {
            if (st == null || llm == null || !llm.enabled || !llmReady) return;
            if (uiRng.NextDouble() > 0.35) return;
            string npcId = PickCloseNpc();
            if (npcId == null) return;
            StartCoroutine(LlmClient.Chat(llm, new[]
            {
                new ChatMsg { role = "system", content = LlmPrompt.System() },
                new ChatMsg { role = "user", content = LlmPrompt.WeChatUser(st, npcId) },
            }, 120,
            content =>
            {
                if (st == null) return;
                string text = StripPlain(content);
                if (string.IsNullOrEmpty(text)) return;
                st.AddLog("微信", Npcs.Name(npcId) + "：" + text);
                Save();
                RenderSide();
            },
            e => { /* 静默 */ }));
        }

        private string PickCloseNpc()
        {
            string best = null;
            int bestF = 3;   // 太生疏的不私聊
            foreach (var id in Npcs.Order)
            {
                var r = Npcs.Get(st, id);
                if (r.familiar > bestF) { bestF = r.familiar; best = id; }
            }
            if (best == null) best = "zhoujin";   // 兜底：办公厅主任（旧代码回落到 Order[0]=常委会主席，不合常理）
            return best;
        }

        /// <summary>推进按钮可用性：结果页/结构阶段/日常可推进；剧情与动态抉择事件必须玩家亲自选。</summary>
        private void UpdateFfButton()
        {
            bool ok = st != null && lastKind != "ending";
            if (ok && !st.hasPending)
            {
                switch (st.phase)
                {
                    case Phase.WeekPlan:
                    case Phase.WeekEnd:
                    case Phase.Weekend:
                    case Phase.MonthEnd:
                        break;
                    case Phase.Day:
                        ok = string.IsNullOrEmpty(st.currentEvent) && !Flow.PendingChoiceEvent(st);
                        break;
                    default:
                        ok = false; // 序章抉择
                        break;
                }
            }
            ui.SetFfEnabled(ok);
        }

        private void RenderSide()
        {
            ui.RenderSide(tab, st);
        }

        private void Quit()
        {
            LlamaServer.Kill();
            var editorType = FindType("UnityEditor.EditorApplication");
            if (editorType != null)
            {
                var prop = editorType.GetProperty("isPlaying");
                if (prop != null && prop.CanWrite) prop.SetValue(null, false);
                return;
            }
            Application.Quit();
        }

        private static System.Type FindType(string fullName)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try { var t = asm.GetType(fullName); if (t != null) return t; }
                catch { /* ignore */ }
            }
            return null;
        }

        // ---------------- 存档（异步 + 原子替换；主线程序列化，后台线程落盘） ----------------

        private static readonly object SaveLock = new object();
        private static long saveTicket;                                   // 入队序号（单调）
        private static readonly System.Collections.Generic.Dictionary<string, long> SavedTicket =
            new System.Collections.Generic.Dictionary<string, long>();    // 每个文件已落盘的最大序号

        private void Save()
        {
            if (st == null) return;
            st.saveVersion = SaveVersion;
            string json = JsonUtility.ToJson(st);   // 紧凑格式：比 pretty 更快、文件更小
            if (string.IsNullOrEmpty(json) || json.Length < 16) return;
            int slot = activeSlot;
            long ticket = System.Threading.Interlocked.Increment(ref saveTicket);
            System.Threading.ThreadPool.QueueUserWorkItem(_ => WriteSaveAtomic(json, slot, ticket));
        }

        /// <summary>原子写盘。ticket 保证同一文件的写入单调：
        /// 旧实现只有互斥锁，ThreadPool 的调度顺序不保证 FIFO，早入队的快照可能后落盘，把新进度盖回旧状态。</summary>
        private static void WriteSaveAtomic(string json, int slot, long ticket)
        {
            if (string.IsNullOrEmpty(json) || json.Length < 16) return;
            lock (SaveLock)
            {
                try
                {
                    WriteOneAtomic(json, SlotPath(slot), ticket);
                    // 手动槽同步镜像一份到自动档，便于“继续”总能回到最新进度
                    if (slot > 0) WriteOneAtomic(json, AutoPath, ticket);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[STARSTATE] 存档失败：" + e.Message);
                }            }
        }

        private static void WriteOneAtomic(string json, string path, long ticket)
        {
            long done;
            if (SavedTicket.TryGetValue(path, out done) && ticket < done) return;   // 已有更新的快照落盘，丢弃旧票
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            if (new FileInfo(tmp).Length < 16) return;   // 落盘校验
            if (File.Exists(path)) File.Replace(tmp, path, null);
            else File.Move(tmp, path);
            SavedTicket[path] = ticket;
        }

        /// <summary>退出/销毁时同步兜底：确保最后一次状态落盘。</summary>
        private void FlushSaveSync()
        {
            if (st == null) return;
            try
            {
                st.saveVersion = SaveVersion;
                long ticket = System.Threading.Interlocked.Increment(ref saveTicket);
                WriteSaveAtomic(JsonUtility.ToJson(st), activeSlot, ticket);
            }
            catch { /* 兜底失败不阻断退出 */ }
        }

        private static GameState LoadFromPath(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                string raw = File.ReadAllText(path);
                // 上古存档没有 saveVersion 字段，JsonUtility 会用字段默认值顶替版本门槛——直接判不兼容
                if (!raw.Contains("\"saveVersion\"")) return null;
                return JsonUtility.FromJson<GameState>(raw);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[STARSTATE] 读档失败：" + e.Message);
                return null;
            }
        }
    }

    /// <summary>场景引导：Awake 中构建 UI 与应用。另用运行时自举，Boot 场景无需挂任何脚本（防场景脚本引用剥落）。</summary>
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoBootstrap()
        {
            if (UnityEngine.Object.FindObjectOfType<GameBootstrap>() != null) return;
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            var ui = gameObject.AddComponent<UiRoot>();
            var app = gameObject.AddComponent<GameApp>();
            float scale = PlayerPrefs.GetFloat("starstate_font_scale", 1f);
            ui.Build(MakeFont(), MakeDocFont(), scale);
            app.Init(ui);
        }

        private static Font _uiFont, _docFont;

        /// <summary>OS 中文字体（微软雅黑→黑体→宋体→内置字体兜底）。静态缓存：字号切换重建 UI 时不重复创建/泄漏 Font。</summary>
        /// <summary>OS 是否真的装了这个字体族。
        /// Font.CreateDynamicFontFromOSFont 对未知字体名通常也会返回非 null 回退字体，
        /// 不先查清单的话，候选链（雅黑→黑体→宋体）里后两者永远走不到。</summary>
        private static bool OsHasFont(string name)
        {
            try
            {
                var all = Font.GetOSInstalledFontNames();
                if (all == null || all.Length == 0) return true;   // 取不到清单时不阻断
                for (int i = 0; i < all.Length; i++)
                {
                    string n = all[i];
                    if (!string.IsNullOrEmpty(n) && n.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
                return false;
            }
            catch { return true; }
        }

        public static Font MakeFont()
        {
            if (_uiFont != null) return _uiFont;
            string[] candidates = { "Microsoft YaHei", "微软雅黑", "SimHei", "SimSun" };
            foreach (var name in candidates)
            {
                if (!OsHasFont(name)) continue;
                try
                {
                    var f = Font.CreateDynamicFontFromOSFont(name, 17);
                    if (f != null) { _uiFont = f; return f; }
                }
                catch { /* 继续尝试 */ }
            }
            _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _uiFont;
        }

        /// <summary>公文正文字体（仿宋→楷体，回退 UI 字体）——红头文件/剪报/印章的字体语汇。</summary>
        public static Font MakeDocFont()
        {
            if (_docFont != null) return _docFont;
            string[] candidates = { "FangSong", "仿宋", "FangSong_GB2312", "仿宋_GB2312", "KaiTi", "楷体" };
            foreach (var name in candidates)
            {
                if (!OsHasFont(name)) continue;
                try
                {
                    var f = Font.CreateDynamicFontFromOSFont(name, 17);
                    if (f != null) { _docFont = f; return f; }
                }
                catch { /* 继续尝试 */ }
            }
            _docFont = MakeFont();
            return _docFont;
        }
    }
}
