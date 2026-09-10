using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Starstate.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Starstate.Ui
{
    /// <summary>
    /// UGUI 界面（第五版）：
    /// ① 动画——卡片入场、段落与选项交错淡入、页签过渡、弹窗弹出、进度条填充、悬停/按压微反馈（Tween/ButtonFx）；
    /// ② 新闻栏——新增“新闻”页签，国际/国内/本地三线，按游戏日期推送（Core.News）；
    /// ③ 排版——状态页改用真实进度条与信息行、侧栏加宽、正文行距、统一圆角卡片；
    /// ④ 修复按钮 ColorTint 过渡色 alpha 错误（悬停时按钮近乎透明）。
    /// EventSystem 仍挂在 Canvas 下随重建销毁（修复“多个 EventSystem”报错）。
    /// </summary>
    public class UiRoot : MonoBehaviour
    {
        public Font font;
        public float fontScale = 1f;

        private GameObject canvasGo;
        private Text topText;
        private CanvasGroup topCg;
        private Text aiBadge;
        private Button ffButton;
        private string lastTopText;

        private RectTransform cardRt;
        private Text titleText;
        private RectTransform bodyContent;
        private RectTransform bodyViewport;
        private RectTransform optionsBox;

        private RectTransform sideContent;
        private CanvasGroup sideCg;
        private Coroutine sideFadeCo;
        private ScrollRect sideScroll;
        private readonly List<Button> tabButtons = new List<Button>();

        private GameObject menuPanel;
        private RectTransform titleRect, subRect;
        private GameObject settingsPanel;
        private CanvasGroup settingsCg;
        private RectTransform settingsCard;
        private InputField nameInput;
        private Text menuHint;
        private Text settingsHint;

        // —— 档案美学（Phase 4）——
        private Font docFont;                       // 公文正文字体（仿宋/楷体，回退 UI 字体）
        private GameObject titleAccentBar;          // 标题带左缘红条（文件态隐藏）
        private Text docOrg, docNoText;             // 红头：局名 + 文号
        private GameObject docRule;                 // 红头双线
        private RectTransform mainCardRect;
        private Text dayNumText;                    // 台历大字日期
        private Button settingsSoundBtn;            // 设置页音效开关
        private Text[] planValTexts;                // 周计划编辑器：数值缓存（局部刷新用）
        private Image[] planFillImages;
        private Text planTotalText;
        private string lastSceneSig;                // 场景签名：同场景重复渲染直接跳过

        // AI 接入（设置页控件）
        private LlmConfig llmCfg;
        private Button llmEnableBtn, llmLocalBtn, llmRemoteBtn, llmAutoBtn;
        private InputField llmEndpoint, llmKey, llmModel, llmServer, llmModelPath, llmLora, llmCtx;
        private Text llmStatus;

        // NPC 交谈弹层
        private GameObject talkPanel;
        private CanvasGroup talkCg;
        private RectTransform talkCard;
        private Text talkTitle, talkStatus;
        private RectTransform talkBody, talkOptions;
        private ScrollRect talkScroll;

        // 新手引导
        private GameObject tutorPanel;
        private Text tutorTitle, tutorBody;
        private Button tutorNext;
        private int tutorPage;

        private readonly List<Button> optionButtons = new List<Button>();
        private GameState lastSt;
        private string lastSideTab;
        private string newsFilter;                       // null=全部
        private readonly Dictionary<string, float> barPrev = new Dictionary<string, float>();

        private const string Ink = "#2B2620";
        private const string InkSoft = "#5B5348";
        private const string Accent = "#9E2B25";
        private const string PaperDark = "#F1EADB";
        private const string PaperWarm = "#FAF5E8";
        private const string TabIdle = "#6B6052";

        private static readonly string[] Tabs = { "状态", "职业", "档案", "人物", "口径", "日志", "新闻" };
        private static readonly string[] NewsCats = { "国际", "国内", "本地" };

        public event Action<int> OnOptionChosen;
        public event Action<int, int> OnPlanAdjusted;   // 周计划编辑器：槽位序号 ±增量
        public event Action<string> OnTabSwitched;
        public event Action<string> OnRuleOpen;   // 口径手册：摊开/收起（""=收起）
        public event Action OnNewGame;
        public event Action<int> OnContinueSlot;   // 多存档槽：槽位 0=自动 / 1–3
        public event Action<int> OnSnapshotSlot;   // 设置页：快照到手动槽
        public event Action OnQuit;
        public event Action OnFastForward;
        public event Action OnOpenSettings;
        public event Action<float> OnFontScaleChanged;
        public event Action OnResetSave;
        public event Action<string> OnNpcTalk;
        public event Action<int> OnTalkOption;
        public event Action OnTalkClose;
        public event Action OnLlmApplied;
        public event Action OnLlmTest;

        private int S(int baseSize) { return Mathf.Max(12, Mathf.RoundToInt(baseSize * fontScale)); }

        // ---------------- 构建 ----------------

        public void Build(Font f, Font docF, float scale)
        {
            font = f;
            docFont = docF != null ? docF : f;
            fontScale = scale;
            optionButtons.Clear();
            tabButtons.Clear();
            barPrev.Clear();
            lastSceneSig = null;      // 整树重建后必须允许重新渲染
            SoundFx.Bind(gameObject);

            // 清掉旧的 EventSystem（修复“场景中存在多个 EventSystem”）
            foreach (var es in UnityEngine.Object.FindObjectsOfType<EventSystem>())
                UnityEngine.Object.Destroy(es.gameObject);
            if (canvasGo != null) Destroy(canvasGo);

            canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            // EventSystem 作为 Canvas 子物体：Canvas 重建时自动随销毁
            var esGo = new GameObject("EventSystem", typeof(EventSystem));
            esGo.transform.SetParent(canvasGo.transform, false);
            var inputType = FindType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            if (inputType != null) esGo.AddComponent(inputType);
            else esGo.AddComponent<StandaloneInputModule>();

            // 全局底：DeskSurface 木纹桌面＋台灯晕
            DeskSurface.BuildBackground(canvasGo.transform);

            BuildTopBar(canvasGo.transform);
            BuildMainCard(canvasGo.transform);
            BuildSidePanel(canvasGo.transform);
            BuildMenu(canvasGo.transform);
            BuildSettings(canvasGo.transform);
            BuildTalk(canvasGo.transform);
            BuildTutorial(canvasGo.transform);
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try { var t = asm.GetType(fullName); if (t != null) return t; }
                catch { /* ignore */ }
            }
            return null;
        }

        // ---------------- 圆角贴图 ----------------

        private static Sprite _cardSprite;
        private static Sprite _buttonSprite;
        private static Sprite _barSprite;

        /// <summary>程序生成圆角矩形贴图（含细边），九宫格切片用。</summary>
        private static Sprite RoundedSprite(int size, int radius, int border, Color fill, Color line)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float r = radius, b = border;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 到圆角矩形的带符号距离（近似）
                    float cx = Mathf.Max(r - x, x - (size - 1 - r), 0);
                    float cy = Mathf.Max(r - y, y - (size - 1 - r), 0);
                    float dist = Mathf.Sqrt(cx * cx + cy * cy) - r;
                    Color c = Color.clear;
                    if (dist <= 0)
                    {
                        c = fill;
                        if (dist > -b) c = line; // 边框
                    }
                    // 边缘 1px 抗锯齿
                    if (dist > -1f && dist < 0f) c.a *= (1f + dist);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            float half = size / 2f;
            var borderV = new Vector4(radius + 1, radius + 1, radius + 1, radius + 1);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(half / size, half / size),
                100f, 0, SpriteMeshType.FullRect, borderV);
        }

        private static Sprite CardSprite()
        {
            if (_cardSprite == null)
                _cardSprite = RoundedSprite(96, 18, 2, FromHex("#FFFFFF"), FromHex("#D8CFBA"));
            return _cardSprite;
        }

        // ---------------- 档案美学程序纹理（委托 DeskSurface） ----------------

        private static Sprite PaperSprite() => DeskSurface.PaperSprite();
        private static Sprite WoodSprite() => DeskSurface.WoodSprite();
        private static Sprite LampGlowSprite() => DeskSurface.LampGlowSprite();
        private static Sprite RingSprite() => DeskSurface.RingSprite();
        private static Sprite TornSprite() => DeskSurface.TornSprite();

        private static Sprite ButtonSprite()
        {
            if (_buttonSprite == null)
                _buttonSprite = RoundedSprite(64, 12, 2, FromHex("#9E2B25"), FromHex("#7C1F1A"));
            return _buttonSprite;
        }

        private static Sprite BarSprite()
        {
            if (_barSprite == null)
                _barSprite = RoundedSprite(32, 8, 0, Color.white, Color.clear);
            return _barSprite;
        }

        // ---------------- 顶栏 ----------------

        private void BuildTopBar(Transform parent)
        {
            var bg = NewGo("TopBar", parent);
            Stretch(bg.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -66), Vector2.zero);
            var img = bg.AddComponent<Image>();
            img.sprite = CardSprite();
            img.type = Image.Type.Sliced;
            img.color = FromHex(PaperDark);

            // 底缘朱线：公文页眉的暗示
            var line = NewGo("TopLine", bg.transform);
            Stretch(line.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(14, 0), new Vector2(-14, 2));
            line.AddComponent<Image>().color = WithAlpha(FromHex(Accent), 0.55f);

            // 台历：今日大字 + 竖分隔（档案美学的桌面台历）
            dayNumText = NewText(bg.transform, "DayNum", 26, Accent, TextAnchor.MiddleCenter);
            dayNumText.fontStyle = FontStyle.Bold;
            var drt = dayNumText.rectTransform;
            drt.anchorMin = drt.anchorMax = new Vector2(0, 0.5f);
            drt.pivot = new Vector2(0.5f, 0.5f);
            drt.anchoredPosition = new Vector2(46, 2);
            drt.sizeDelta = new Vector2(58, 44);
            dayNumText.text = "";
            var dsep = NewGo("DaySep", bg.transform);
            Stretch(dsep.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(82, 10), new Vector2(84, -10));
            dsep.AddComponent<Image>().color = WithAlpha(FromHex(Accent), 0.35f);

            topText = NewText(bg.transform, "TopText", 15, Ink, TextAnchor.MiddleLeft);
            Stretch(topText.rectTransform, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(96, 6), new Vector2(-226, -6));
            topCg = topText.gameObject.AddComponent<CanvasGroup>();

            // AI 状态徽标（就绪/唤醒中/未连接/关闭）
            aiBadge = NewText(bg.transform, "AiBadge", 11, InkSoft, TextAnchor.MiddleRight);
            var brt = aiBadge.rectTransform;
            brt.anchorMin = brt.anchorMax = new Vector2(1, 0.5f);
            brt.pivot = new Vector2(1, 0.5f);
            brt.anchoredPosition = new Vector2(-226, 0);
            brt.sizeDelta = new Vector2(120, 20);
            aiBadge.text = "";

            var ff = MakeButton(bg.transform, "▸▸ 推进", 15, true);
            ffButton = ff;
            var rt = ff.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(-114, 0);
            rt.sizeDelta = new Vector2(100, 38);
            ff.onClick.AddListener(() => { if (OnFastForward != null) OnFastForward(); });

            var gear = MakeButton(bg.transform, "设 置", 15, false);
            var rt2 = gear.GetComponent<RectTransform>();
            rt2.anchorMin = rt2.anchorMax = new Vector2(1, 0.5f);
            rt2.pivot = new Vector2(1, 0.5f);
            rt2.anchoredPosition = new Vector2(-10, 0);
            rt2.sizeDelta = new Vector2(96, 38);
            gear.onClick.AddListener(() => { if (OnOpenSettings != null) OnOpenSettings(); });
        }

        // ---------------- 主卡 ----------------

        private void BuildMainCard(Transform parent)
        {
            var panel = NewGo("MainCard", parent);
            Stretch(panel.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(12, 16), new Vector2(-354, -74));
            var pimg = panel.AddComponent<Image>();
            pimg.sprite = CardSprite();
            pimg.type = Image.Type.Sliced;
            pimg.color = Color.white;
            mainCardRect = panel.GetComponent<RectTransform>();
            // 桌面阴影：公文浮在木纹上
            var psh = panel.AddComponent<Shadow>();
            psh.effectColor = new Color(0f, 0f, 0f, 0.35f);
            psh.effectDistance = new Vector2(3, -4);

            // 标题带：左侧红色公文侧标 + 浅底（事件态时整带切换为红头文头）
            var titleBand = NewGo("TitleBand", panel.transform);
            Stretch(titleBand.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -58), Vector2.zero);
            titleBand.AddComponent<Image>().color = FromHex("#F7F1E2");

            var bar = NewGo("AccentBar", titleBand.transform);
            Stretch(bar.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(10, 8), new Vector2(14, -8));
            bar.AddComponent<Image>().color = FromHex(Accent);
            titleAccentBar = bar;

            titleText = NewText(titleBand.transform, "Title", 21, Accent, TextAnchor.MiddleLeft);
            Stretch(titleText.rectTransform, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 4), new Vector2(-16, -4));
            var sh = titleText.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.12f);
            sh.effectDistance = new Vector2(1, 1);

            // 红头文头：局名（朱红居中）+ 文号（灰）
            docOrg = NewText(titleBand.transform, "DocOrg", 16, Accent, TextAnchor.MiddleCenter);
            docOrg.text = "大同市人民政府";
            docOrg.fontStyle = FontStyle.Bold;
            Stretch(docOrg.rectTransform, new Vector2(0, 0.42f), new Vector2(1, 1), new Vector2(0, 2), new Vector2(0, -2));
            docOrg.gameObject.SetActive(false);

            docNoText = NewText(titleBand.transform, "DocNo", 11, InkSoft, TextAnchor.MiddleCenter);
            Stretch(docNoText.rectTransform, new Vector2(0, 0), new Vector2(1, 0.42f), new Vector2(0, 0), Vector2.zero);
            docNoText.gameObject.SetActive(false);

            // 红头双线（文头下的粗+细朱线）
            docRule = NewGo("DocRule", panel.transform);
            Stretch(docRule.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -58), Vector2.zero);
            var rule1 = NewGo("R1", docRule.transform);
            Stretch(rule1.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(20, -4), new Vector2(-20, -1));
            rule1.AddComponent<Image>().color = FromHex(Accent);
            var rule2 = NewGo("R2", docRule.transform);
            Stretch(rule2.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(20, -6), new Vector2(-20, -5));
            rule2.AddComponent<Image>().color = WithAlpha(FromHex(Accent), 0.7f);
            docRule.SetActive(false);

            // 正文滚动（段落分排）
            var scrollGo = new GameObject("BodyScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            var srt = scrollGo.GetComponent<RectTransform>();
            srt.SetParent(panel.transform, false);
            Stretch(srt, new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, 214), new Vector2(-20, -68));
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24;

            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.GetComponent<RectTransform>().SetParent(scrollGo.transform, false);
            Stretch(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scroll.viewport = viewport.GetComponent<RectTransform>();
            bodyViewport = viewport.GetComponent<RectTransform>();

            var content = new GameObject("Content", typeof(RectTransform));
            var crt = content.GetComponent<RectTransform>();
            crt.SetParent(viewport.transform, false);
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            crt.sizeDelta = new Vector2(0, 100);
            bodyContent = crt;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 11;
            vlg.padding = new RectOffset(8, 8, 10, 14);
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = crt;

            // 选项区
            optionsBox = NewGo("Options", panel.transform).GetComponent<RectTransform>();
            Stretch(optionsBox, new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 16), new Vector2(-20, 206));
            var layout = optionsBox.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        // ---------------- 侧栏（含新闻页签） ----------------

        private void BuildSidePanel(Transform parent)
        {
            var panel = NewGo("SidePanel", parent);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 0);
            rt.anchoredPosition = new Vector2(-12, 0);
            rt.sizeDelta = new Vector2(330, -90);
            var pimg = panel.AddComponent<Image>();
            pimg.sprite = CardSprite();
            pimg.type = Image.Type.Sliced;
            pimg.color = FromHex(PaperWarm);
            var ssh = panel.AddComponent<Shadow>();
            ssh.effectColor = new Color(0f, 0f, 0f, 0.28f);
            ssh.effectDistance = new Vector2(2, -3);

            var tabRow = NewGo("TabRow", panel.transform);
            Stretch(tabRow.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -44), Vector2.zero);
            var hlayout = tabRow.AddComponent<HorizontalLayoutGroup>();
            hlayout.spacing = 4;
            hlayout.padding = new RectOffset(8, 8, 8, 0);
            hlayout.childForceExpandHeight = false;
            hlayout.childForceExpandWidth = false;
            hlayout.childControlWidth = true;    // 必须由布局组落实 preferred 尺寸，否则矩形停留在 sizeDelta(0,0)
            hlayout.childControlHeight = true;

            for (int i = 0; i < Tabs.Length; i++)
            {
                string tab = Tabs[i];
                int captured = i;
                var b = MakeButton(tabRow.transform, tab, 13, false);
                var le = b.GetComponent<LayoutElement>();
                le.preferredWidth = 40;
                le.preferredHeight = 28;
                b.onClick.AddListener(() => { if (OnTabSwitched != null) OnTabSwitched(tab); });
                tabButtons.Add(b);
            }

            // 侧栏内容滚动化
            var scrollGo = new GameObject("SideScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            var srt = scrollGo.GetComponent<RectTransform>();
            srt.SetParent(panel.transform, false);
            Stretch(srt, new Vector2(0, 0), new Vector2(1, 1), new Vector2(8, 14), new Vector2(-8, -56));
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 22;
            sideScroll = scroll;

            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.GetComponent<RectTransform>().SetParent(scrollGo.transform, false);
            Stretch(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var content = new GameObject("SideContent", typeof(RectTransform));
            var crt = content.GetComponent<RectTransform>();
            crt.SetParent(viewport.transform, false);
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            crt.sizeDelta = new Vector2(0, 100);
            sideContent = crt;
            var cv = content.AddComponent<VerticalLayoutGroup>();
            cv.padding = new RectOffset(12, 12, 8, 12);
            cv.spacing = 7;
            cv.childForceExpandHeight = false;
            cv.childForceExpandWidth = true;
            cv.childControlHeight = true;
            cv.childControlWidth = true;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = crt;
            sideCg = content.AddComponent<CanvasGroup>();
        }

        // ---------------- 侧栏页签渲染 ----------------

        public void RenderSide(string tab, GameState st)
        {
            lastSt = st;
            bool tabChanged = tab != lastSideTab;
            lastSideTab = tab;
            for (int i = 0; i < tabButtons.Count; i++)
                SetBtnColor(tabButtons[i], Tabs[i] == tab ? FromHex(Accent) : FromHex(TabIdle));

            ClearChildren(sideContent);
            switch (tab)
            {
                case "状态": BuildStatusWidgets(st); break;
                case "新闻": BuildNewsWidgets(st, tabChanged); break;
                case "职业": AddTextBlock(sideContent, TextBuilders.CareerPanel(st), 14, Ink); break;
                case "档案": AddTextBlock(sideContent, TextBuilders.Records(st), 14, Ink); break;
                case "人物": BuildNpcWidgets(st); break;
                case "口径": BuildRulebookWidgets(st); break;
                case "日志": AddTextBlock(sideContent, TextBuilders.LogPanel(st), 14, Ink); break;
            }

            if (tabChanged)
            {
                // 仅页签切换时淡入；日常数值刷新不闪
                if (sideFadeCo != null) StopCoroutine(sideFadeCo);
                sideCg.alpha = 0f;
                sideFadeCo = StartCoroutine(Tween.AlphaCo(sideCg, 1f, 0.18f, Tween.OutCubic));
            }
            else
            {
                sideCg.alpha = 1f;
            }
            if (tabChanged) StartCoroutine(ResetSideScrollCo());
        }

        private IEnumerator ResetSideScrollCo()
        {
            yield return null;
            if (sideScroll == null) yield break;
            Canvas.ForceUpdateCanvases();
            sideScroll.verticalNormalizedPosition = 1f;
        }

        // ---------------- 口径手册（RulebookPanel） ----------------

        private string ruleQuery = "";

        private void BuildRulebookWidgets(GameState st)
        {
            var head = AddTextBlock(sideContent, "—— 口径手册 ——", 12, InkSoft);
            head.fontStyle = FontStyle.Bold;

            // 检索（浅色纸面输入）
            var searchRow = NewGo("RuleSearch", sideContent);
            var sle = searchRow.AddComponent<LayoutElement>();
            sle.preferredHeight = 34;
            var simg = searchRow.AddComponent<Image>();
            simg.sprite = CardSprite();
            simg.type = Image.Type.Sliced;
            simg.color = FromHex("#FFFDF6");
            var sfield = searchRow.AddComponent<InputField>();
            sfield.characterLimit = 24;
            var stxtGo = NewGo("Text", searchRow.transform);
            Stretch(stxtGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var stxt = stxtGo.AddComponent<Text>();
            stxt.font = font; stxt.fontSize = S(13); stxt.color = FromHex(Ink);
            stxt.alignment = TextAnchor.MiddleLeft;
            stxt.text = ruleQuery ?? "";
            sfield.textComponent = stxt;
            var sphGo = NewGo("Placeholder", searchRow.transform);
            Stretch(sphGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var sph = sphGo.AddComponent<Text>();
            sph.font = font; sph.fontSize = S(12); sph.color = FromHex("#8F8368");
            sph.alignment = TextAnchor.MiddleLeft;
            sph.text = "检索：数字 / 算法 / 土地…";
            sfield.placeholder = sph;
            sfield.text = ruleQuery ?? "";
            sfield.onEndEdit.AddListener(v =>
            {
                ruleQuery = v ?? "";
                if (lastSt != null) RenderSide("口径", lastSt);
            });

            string openTitle = string.IsNullOrEmpty(st.openRule) ? "（未摊开）" : Rulebook.Title(st.openRule);
            AddTextBlock(sideContent, "摊开：" + openTitle, 13, string.IsNullOrEmpty(st.openRule) ? InkSoft : Accent);
            int deskN = st.deskRules != null ? st.deskRules.Count : 0;
            AddTextBlock(sideContent, $"案头 {deskN}/{Rulebook.DeskSlots} · 档案 {st.knownRules.Count} 条（新件会顶掉最旧）", 11, InkSoft);

            // 案头槽位
            if (deskN > 0)
            {
                var deskHead = AddTextBlock(sideContent, "—— 案头 ——", 11, InkSoft);
                deskHead.fontStyle = FontStyle.Bold;
                for (int i = 0; i < st.deskRules.Count; i++)
                {
                    string rid = st.deskRules[i];
                    var def = Rulebook.Get(rid);
                    if (def == null) continue;
                    bool isOpen = st.openRule == rid;
                    var card = BeginSideCard(isOpen ? "▣ " + def.title : def.title);
                    if (isOpen)
                    {
                        var himg = card.GetComponent<Image>();
                        if (himg != null) himg.color = FromHex("#FFF4E4");
                        CardInfoRow(card.transform, "类别", def.category + "　" + def.source);
                        foreach (var line in def.body.Split('\n'))
                        {
                            if (string.IsNullOrEmpty(line.Trim())) continue;
                            var t = AddTextBlock(card.transform, line.Trim(), 12, Ink);
                            t.lineSpacing = 1.2f;
                        }
                        var close = MakeButton(card.transform, "收 起", 12, false);
                        close.GetComponent<LayoutElement>().preferredHeight = 28;
                        close.onClick.AddListener(() => { if (OnRuleOpen != null) OnRuleOpen(""); });
                    }
                    else
                    {
                        string ridCap = rid;
                        var open = MakeButton(card.transform, "摊 开", 12, true);
                        open.GetComponent<LayoutElement>().preferredHeight = 28;
                        open.onClick.AddListener(() => { if (OnRuleOpen != null) OnRuleOpen(ridCap); });
                    }
                }
            }

            // 档案（可检索；含未上案头的）
            var archHead = AddTextBlock(sideContent, "—— 档案 ——", 11, InkSoft);
            archHead.fontStyle = FontStyle.Bold;
            var matches = Rulebook.Search(st, ruleQuery);
            int shown = 0;
            foreach (var rid in matches)
            {
                if (Rulebook.OnDesk(st, rid)) continue; // 案头已列
                var def = Rulebook.Get(rid);
                if (def == null) continue;
                shown++;
                var card = BeginSideCard(def.title);
                CardInfoRow(card.transform, "类别", def.category);
                CardInfoRow(card.transform, "出处", def.source.Length > 28 ? def.source.Substring(0, 28) + "…" : def.source);
                string ridCap2 = rid;
                var pull = MakeButton(card.transform, "上 案 头", 12, true);
                pull.GetComponent<LayoutElement>().preferredHeight = 28;
                pull.onClick.AddListener(() => { if (OnRuleOpen != null) OnRuleOpen(ridCap2); });
            }
            if (shown == 0 && st.knownRules.Count > 0)
                AddTextBlock(sideContent, string.IsNullOrEmpty(ruleQuery) ? "（档案里暂无未上案头的条目）" : "（无匹配：试试「数字」「算法」「土地」）", 12, InkSoft);
            if (st.knownRules.Count == 0)
                AddTextBlock(sideContent, "（办公厅尚未送达口径。查出问题或剧情推进会补授。）", 13, InkSoft);
        }

        // ---------------- 状态页：信息行 + 真进度条 ----------------

        private void SideHeader(string s)
        {
            var t = AddTextBlock(sideContent, "—— " + s + " ——", 12, InkSoft);
            t.fontStyle = FontStyle.Bold;
        }

        /// <summary>侧栏卡片容器：公文信笺底 + 内边距，让状态/时钟成为可扫读的块。</summary>
        private GameObject BeginSideCard(string title)
        {
            var card = NewGo("Card_" + title, sideContent);
            var img = card.AddComponent<Image>();
            img.sprite = CardSprite();
            img.type = Image.Type.Sliced;
            img.color = FromHex("#FFFDF6");
            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 8, 10);
            vlg.spacing = 3;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;

            var head = NewText(card.transform, "CardHead", 12, Accent, TextAnchor.MiddleLeft);
            head.text = title;
            head.fontStyle = FontStyle.Bold;
            head.rectTransform.SetParent(card.transform, false);
            var hle = head.gameObject.AddComponent<LayoutElement>();
            hle.preferredHeight = 18;
            return card;
        }

        private void CardInfoRow(Transform parent, string label, string value)
        {
            var row = NewGo("Row_" + label, parent);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var lt = NewText(row.transform, "L", 12, InkSoft, TextAnchor.MiddleLeft);
            lt.text = label;
            lt.rectTransform.SetParent(row.transform, false);
            var lle = lt.gameObject.AddComponent<LayoutElement>();
            lle.preferredWidth = 70;
            lle.minWidth = 70;

            var vt = NewText(row.transform, "V", 13, Ink, TextAnchor.MiddleLeft);
            vt.text = value;
            vt.rectTransform.SetParent(row.transform, false);
            var vle = vt.gameObject.AddComponent<LayoutElement>();
            vle.flexibleWidth = 1;
        }

        private void CardBarRow(Transform parent, string label, int value, string colorHex)
        {
            // 复用 BarRow 的视觉：临时把 sideContent 换成 parent 不现实，这里复制精简版
            var row = NewGo("Bar_" + label, parent);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 22;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var lt = NewText(row.transform, "L", 12, InkSoft, TextAnchor.MiddleLeft);
            lt.text = label;
            var lle = lt.gameObject.AddComponent<LayoutElement>();
            lle.preferredWidth = 70;
            lle.minWidth = 70;

            var wrap = NewGo("BarWrap", row.transform);
            var wle = wrap.AddComponent<LayoutElement>();
            wle.flexibleWidth = 1;
            wle.preferredHeight = 12;
            var bg = wrap.AddComponent<Image>();
            bg.sprite = BarSprite();
            bg.type = Image.Type.Simple;
            bg.color = FromHex("#E3DAC4");

            var fillGo = NewGo("Fill", wrap.transform);
            Stretch(fillGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one,
                new Vector2(1.5f, 1.5f), new Vector2(-1.5f, -1.5f));
            var fill = fillGo.AddComponent<Image>();
            fill.sprite = BarSprite();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.color = FromHex(colorHex);

            float target = Mathf.Clamp01(value / 100f);
            fill.fillAmount = target;

            var vt = NewText(row.transform, "V", 12, InkSoft, TextAnchor.MiddleRight);
            vt.text = value.ToString();
            var vle = vt.gameObject.AddComponent<LayoutElement>();
            vle.preferredWidth = 34;
        }

        private void InfoRow(string label, string value)
        {
            var row = NewGo("Row_" + label, sideContent);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var lt = NewText(row.transform, "L", 12, InkSoft, TextAnchor.MiddleLeft);
            lt.text = label;
            lt.rectTransform.SetParent(row.transform, false);
            var lle = lt.gameObject.AddComponent<LayoutElement>();
            lle.preferredWidth = 70;
            lle.minWidth = 70;

            var vt = NewText(row.transform, "V", 13, Ink, TextAnchor.MiddleLeft);
            vt.text = value;
            vt.rectTransform.SetParent(row.transform, false);
            var vle = vt.gameObject.AddComponent<LayoutElement>();
            vle.flexibleWidth = 1;
        }

        private void BarRow(string label, int value, string colorHex)
        {
            var row = NewGo("Bar_" + label, sideContent);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 22;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var lt = NewText(row.transform, "L", 12, InkSoft, TextAnchor.MiddleLeft);
            lt.text = label;
            var lle = lt.gameObject.AddComponent<LayoutElement>();
            lle.preferredWidth = 70;
            lle.minWidth = 70;

            var wrap = NewGo("BarWrap", row.transform);
            var wle = wrap.AddComponent<LayoutElement>();
            wle.flexibleWidth = 1;
            wle.preferredHeight = 12;
            var wrt = wrap.GetComponent<RectTransform>();
            var bg = wrap.AddComponent<Image>();
            bg.sprite = BarSprite();
            bg.type = Image.Type.Simple;
            bg.color = FromHex("#E3DAC4");

            var fillGo = NewGo("Fill", wrap.transform);
            Stretch(fillGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one,
                new Vector2(1.5f, 1.5f), new Vector2(-1.5f, -1.5f));
            var fill = fillGo.AddComponent<Image>();
            fill.sprite = BarSprite();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.color = FromHex(colorHex);

            var vt = NewText(row.transform, "V", 12, InkSoft, TextAnchor.MiddleRight);
            vt.text = value.ToString();
            var vle = vt.gameObject.AddComponent<LayoutElement>();
            vle.preferredWidth = 34;

            // 值未变时不重复动画；变了则从旧值滑到新值
            float target = Mathf.Clamp01(value / 100f);
            float prev;
            bool hasPrev = barPrev.TryGetValue(label, out prev);
            barPrev[label] = target;
            if (hasPrev && Mathf.Abs(prev - target) < 0.001f)
            {
                fill.fillAmount = target;
            }
            else
            {
                fill.fillAmount = hasPrev ? prev : 0f;
                StartCoroutine(Tween.FillCo(fill, target, 0.45f, Tween.OutCubic));
            }
        }

        private void BuildStatusWidgets(GameState st)
        {
            var p = st.player;
            int age = GameClock.Parse(st.date).Year - p.birthYear;
            int pending = st.pendingDossierIds.Count + (st.activeDossier != null && !st.activeDossier.resolved ? 1 : 0);

            // 任职（市长案头，去掉科员字段）
            var c1 = BeginSideCard("任职");
            CardInfoRow(c1.transform, "姓名", p.name + " · " + age + "岁");
            CardInfoRow(c1.transform, "职务", p.post);
            CardInfoRow(c1.transform, "品级", p.rank);
            CardInfoRow(c1.transform, "单位", p.unit);
            CardInfoRow(c1.transform, "本届起任", st.gradeSince);
            if (!string.IsNullOrEmpty(st.route)) CardInfoRow(c1.transform, "主攻", st.route);
            CardInfoRow(c1.transform, "案头待办", pending + " 件卷宗");
            string openRb = string.IsNullOrEmpty(st.openRule) ? "未摊开" : Rulebook.Title(st.openRule);
            CardInfoRow(c1.transform, "案头口径", openRb.Length > 18 ? openRb.Substring(0, 18) + "…" : openRb);

            // 两把尺：年度考核与巡视底稿的主尺
            var c2 = BeginSideCard("两把尺");
            CardBarRow(c2.transform, "合规", st.compliance, st.compliance >= 70 ? "#5E8C6A" : st.compliance >= 50 ? "#C29B3C" : "#B0563F");
            CardBarRow(c2.transform, "效率", st.efficiency, st.efficiency >= 60 ? "#4E6E8E" : st.efficiency >= 45 ? "#C29B3C" : "#B0563F");
            CardInfoRow(c2.transform, "本年程序问题", st.yearIntegrity + " 次");
            CardInfoRow(c2.transform, "累计违规", st.violationCount + " 次");

            // 常委会权力地形（PowerBoard 雏形）
            var c3 = BeginSideCard("常委会 · 权力地形");
            string[] seats = { "cen", "han", "shenyan", "shao", "zhoujin" };
            string[] seatRoles = { "主席", "组织", "纪委", "常务副", "办公厅" };
            for (int i = 0; i < seats.Length; i++)
            {
                var r = Npcs.Get(st, seats[i]);
                string nm = Npcs.Name(seats[i]);
                string col = r.trust >= 25 ? "#5E8C6A" : r.trust <= -20 ? "#B0563F" : "#6B6052";
                CardInfoRow(c3.transform, seatRoles[i], nm + "　信任 " + (r.trust >= 0 ? "+" : "") + r.trust);
                // 将值文本染成冷热色（最后一行的 V 字段）
                int lastIdx = c3.transform.childCount - 1;
                var row = c3.transform.GetChild(lastIdx);
                foreach (var t in row.GetComponentsInChildren<Text>())
                {
                    if (t.gameObject.name == "V") t.color = FromHex(col);
                }
            }

            // 身心
            var c4 = BeginSideCard("身心");
            CardBarRow(c4.transform, "精力", p.energy, "#5E8C6A");
            CardBarRow(c4.transform, "压力", p.stress, "#B0563F");
            CardBarRow(c4.transform, "士气", p.morale, "#C29B3C");

            // 资源与家庭
            var c5 = BeginSideCard("资源");
            CardInfoRow(c5.transform, "社会声望", p.reputation.ToString());
            CardInfoRow(c5.transform, "政治资本", p.polCapital.ToString());
            CardInfoRow(c5.transform, "积蓄", p.savings + " 元");
            CardInfoRow(c5.transform, "月结余", (p.monthlyIn - p.monthlyOut) + " 元");
            string fam = st.hasChild ? "已婚有孩" : st.married ? "已婚" : (string.IsNullOrEmpty(st.partner) ? "—" : st.partner);
            CardInfoRow(c5.transform, "家庭", fam + "　" + st.housing);

            // 同批竞争（许飞线仍在跑）
            var c6 = BeginSideCard("同批");
            string rname = Npcs.Name(st.rival.id);
            CardInfoRow(c6.transform, rname + "势头", st.rival.progress + " / 100");
            if (st.rival.stage > 0) CardInfoRow(c6.transform, "里程碑", "阶段 " + st.rival.stage);

            // 常驻时钟仪表
            if (st.clocks != null && st.clocks.Count > 0)
            {
                var c7 = BeginSideCard("时钟");
                foreach (var c in st.clocks)
                {
                    if (c == null || c.max <= 0) continue;
                    int pct = Mathf.Clamp(Mathf.RoundToInt(c.value * 100f / c.max), 0, 100);
                    string color = c.kind == "threat" ? "#B0563F" : "#5E8C6A";
                    string label = string.IsNullOrEmpty(c.label) ? c.id : c.label;
                    CardBarRow(c7.transform, label, pct, color);
                }
            }
        }

        // ---------------- 新闻页 ----------------

        private void BuildNewsWidgets(GameState st, bool animate)
        {
            // 筛选行
            var filterRow = NewGo("NewsFilter", sideContent);
            var fle = filterRow.AddComponent<LayoutElement>();
            fle.preferredHeight = 28;
            var hlg = filterRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;
            hlayoutChildControl(hlg);

            AddFilterChip(filterRow.transform, "全部", null);
            foreach (var cat in NewsCats) AddFilterChip(filterRow.transform, cat, cat);

            var items = News.Visible(st.date, newsFilter, 30);
            int total = News.Visible(st.date, newsFilter, int.MaxValue).Count;
            var head = AddTextBlock(sideContent, "新闻汇 · 最近 " + items.Count + " 条（随日期推送）", 11, InkSoft);
            head.fontStyle = FontStyle.Bold;
            if (items.Count == 0)
            {
                AddTextBlock(sideContent, "（暂无新闻）", 13, InkSoft);
                return;
            }
            int i = 0;
            foreach (var n in items)
            {
                var card = NewsCard(n);
                if (animate)
                {
                    var cg = card.AddComponent<CanvasGroup>();
                    StartCoroutine(Tween.Delayed(0.03f + i * 0.025f, Tween.AlphaCo(cg, 1f, 0.16f, Tween.OutCubic)));
                }
                i++;
            }
            AddTextBlock(sideContent,
                total > items.Count ? "—— 更早的新闻已归档（共 " + total + " 条）——" : "—— 已经是最早的消息 ——",
                11, InkSoft);
        }

        private static void hlayoutChildControl(HorizontalLayoutGroup h)
        {
            h.childControlWidth = true;
            h.childControlHeight = true;
        }

        private void AddFilterChip(Transform parent, string label, string cat)
        {
            var b = MakeButton(parent, label, 12, false);
            var le = b.GetComponent<LayoutElement>();
            le.preferredWidth = 52;
            le.preferredHeight = 26;
            bool selected = (newsFilter ?? null) == (cat ?? null);
            if (selected) SetBtnColor(b, FromHex(Accent));
            b.onClick.AddListener(() =>
            {
                newsFilter = cat;
                if (lastSt != null) RenderSide("新闻", lastSt);
            });
        }

        private GameObject NewsCard(NewsItem n)
        {
            var card = NewGo("News_" + n.date, sideContent);
            var img = card.AddComponent<Image>();
            img.sprite = CardSprite();
            img.type = Image.Type.Sliced;
            img.color = FromHex("#FFFDF6");
            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 9, 10);
            vlg.spacing = 4;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;

            // 左侧分类色脊（公文侧标风格）
            var spine = NewGo("Spine", card.transform);
            Stretch(spine.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(3, 6), new Vector2(8, -6));
            spine.AddComponent<Image>().color = NewsCatColor(n.cat);

            // 头行：报头（剪报 masthead）· 日期 · 类别徽标
            var mast = NewText(card.transform, "Mast", 12, NewsCatColor(n.cat), TextAnchor.MiddleCenter);
            mast.text = "长 安 日 报";
            mast.fontStyle = FontStyle.Bold;
            mast.rectTransform.SetParent(card.transform, false);
            var mle = mast.gameObject.AddComponent<LayoutElement>();
            mle.preferredHeight = 15;

            var head = NewGo("Head", card.transform);
            var hh = head.AddComponent<LayoutElement>();
            hh.preferredHeight = 18;
            var h = head.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 6;
            h.childForceExpandHeight = false;
            h.childForceExpandWidth = false;
            hlayoutChildControl(h);

            var d = GameClock.Parse(n.date);
            var dateT = NewText(head.transform, "Date", 11, InkSoft, TextAnchor.MiddleLeft);
            dateT.text = d.Year + "." + d.Month + "." + d.Day;
            var spacer = NewGo("Sp", head.transform);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1;

            var tag = NewGo("Tag", head.transform);
            var tle = tag.AddComponent<LayoutElement>();
            tle.minWidth = 36;
            tle.preferredHeight = 17;
            var timg = tag.AddComponent<Image>();
            timg.sprite = BarSprite();
            timg.type = Image.Type.Simple;
            timg.color = WithAlpha(NewsCatColor(n.cat), 0.16f);
            var tt = NewText(tag.transform, "TagText", 10, NewsCatColor(n.cat), TextAnchor.MiddleCenter);
            tt.text = n.cat;
            Stretch(tt.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            tt.raycastTarget = false;

            var title = NewText(card.transform, "Title", 13, Ink, TextAnchor.UpperLeft);
            title.text = n.title;
            title.fontStyle = FontStyle.Bold;
            title.rectTransform.SetParent(card.transform, false);

            var body = NewText(card.transform, "Body", 12, InkSoft, TextAnchor.UpperLeft);
            body.text = n.body;
            body.lineSpacing = 1.18f;
            body.rectTransform.SetParent(card.transform, false);
            if (docFont != null) body.font = docFont;   // 报纸正文用公文字型（报宋质感）

            // 撕边：底部不规则缺口（剪报感）
            var torn = NewGo("Torn", card.transform);
            var tle2 = torn.AddComponent<LayoutElement>();
            tle2.ignoreLayout = true;
            tle2.preferredHeight = 12;
            var timg2 = torn.AddComponent<Image>();
            timg2.sprite = TornSprite();
            timg2.type = Image.Type.Tiled;
            timg2.color = FromHex("#FFFDF6");
            timg2.raycastTarget = false;
            var trt = torn.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(1, 0);
            trt.pivot = new Vector2(0.5f, 0f);
            trt.anchoredPosition = new Vector2(0, 0);
            trt.sizeDelta = new Vector2(0, 12);
            return card;
        }

        private static Color NewsCatColor(string cat)
        {
            switch (cat)
            {
                case "国际": return FromHex("#3F6E8E");
                case "国内": return FromHex("#8A6A2F");
                default: return FromHex(Accent);
            }
        }

        // ---------------- 主菜单 ----------------

        private void BuildMenu(Transform parent)
        {
            menuPanel = NewGo("Menu", parent);
            Stretch(menuPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            menuPanel.transform.SetAsLastSibling();
            menuPanel.AddComponent<Image>().color = FromHex("#221E19");

            // 菜单顶部装饰：一条红金细带
            var band = NewGo("MenuBand", menuPanel.transform);
            Stretch(band.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -6), new Vector2(0, -2));
            band.AddComponent<Image>().color = FromHex(Accent);

            var title = NewText(menuPanel.transform, "MenuTitle", 46, "#E8DCC0", TextAnchor.MiddleCenter);
            Top(title.rectTransform, 0, -108, 700, 66);
            title.text = "STARSTATE";
            titleRect = title.rectTransform;
            var tsh = title.gameObject.AddComponent<Shadow>();
            tsh.effectColor = new Color(0f, 0f, 0f, 0.5f);
            tsh.effectDistance = new Vector2(2, 2);

            var sub = NewText(menuPanel.transform, "MenuSub", 17, "#B7AA8C", TextAnchor.MiddleCenter);
            Top(sub.rectTransform, 0, -182, 700, 28);
            sub.text = "七品·大同市市长 · 卷宗签批 · 2026—2036";
            subRect = sub.rectTransform;

            nameInput = MakeInput(menuPanel.transform, new Vector2(0, -252));

            menuHint = NewText(menuPanel.transform, "MenuHint", 13, "#8F8368", TextAnchor.MiddleCenter);
            Top(menuHint.rectTransform, 0, -304, 640, 24);

            var quote = NewText(menuPanel.transform, "MenuQuote", 14, "#8F8368", TextAnchor.MiddleCenter);
            Bottom(quote.rectTransform, 0, 30, 820, 26);
            quote.text = "让每一代人，都有改变自己命运的机会。";

            var ver = NewText(menuPanel.transform, "MenuVersion", 11, "#6E6350", TextAnchor.MiddleRight);
            var vrt = ver.rectTransform;
            vrt.anchorMin = new Vector2(1, 0);
            vrt.anchorMax = new Vector2(1, 0);
            vrt.pivot = new Vector2(1, 0);
            vrt.anchoredPosition = new Vector2(-14, 10);
            vrt.sizeDelta = new Vector2(300, 20);
            ver.text = "STARSTATE v0.2.6 Preview";
        }

        public void ShowMenu(bool hasSave)
        {
            // 兼容旧签名：有自动档则只展示自动档
            if (hasSave) ShowMenu(new[] { 0 }, new[] { "自动档" });
            else ShowMenu(new int[0], new string[0]);
        }

        /// <summary>多存档槽主菜单：每个可用槽一条“继续”按钮。</summary>
        public void ShowMenu(int[] slots, string[] labels)
        {
            menuPanel.SetActive(true);
            settingsPanel.SetActive(false);
            ClearChildren(optionsBox);
            float y = -330;
            var btns = new List<Button>();
            int n = slots != null ? slots.Length : 0;
            if (n > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    int captured = slots[i];
                    string label = (labels != null && i < labels.Length && !string.IsNullOrEmpty(labels[i]))
                        ? labels[i] : (captured == 0 ? "自动档" : "槽位 " + captured);
                    if (label.Length > 22) label = label.Substring(0, 22) + "…";
                    btns.Add(MakeMenuButton(label, y, true, () => { if (OnContinueSlot != null) OnContinueSlot(captured); }));
                    y -= 40;
                }
                y -= 8;
            }
            btns.Add(MakeMenuButton("新 游 戏", y, true, () => { if (OnNewGame != null) OnNewGame(); })); y -= 54;
            btns.Add(MakeMenuButton("玩 法 说 明", y, false, ShowTutorial)); y -= 54;
            btns.Add(MakeMenuButton("设 置", y, false, () => { if (OnOpenSettings != null) OnOpenSettings(); })); y -= 54;
            btns.Add(MakeMenuButton("退 出", y, false, () => { if (OnQuit != null) OnQuit(); }));

            // 入场：标题→副题→输入框→按钮→提示，依次滑入
            float delay = 0f;
            StartCoroutine(MenuIn(titleRect, delay)); delay += 0.07f;
            StartCoroutine(MenuIn(subRect, delay)); delay += 0.07f;
            StartCoroutine(MenuIn(nameInput.GetComponent<RectTransform>(), delay)); delay += 0.08f;
            foreach (var b in btns)
            {
                StartCoroutine(MenuIn(b.GetComponent<RectTransform>(), delay));
                delay += 0.04f;
            }
            StartCoroutine(MenuIn(menuHint.rectTransform, delay));

            menuHint.text = n > 0
                ? "选择存档槽继续，或改名后点击“新游戏”另起一局（优先写入空槽）。"
                : "输入主角姓名（或保留默认），点击“新游戏”开始。";
        }

        public void HideOverlays()
        {
            menuPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        private Button MakeMenuButton(string label, float y, bool primary, Action onClick)
        {
            var b = MakeButton(menuPanel.transform, label, 18, primary);
            Top(b.GetComponent<RectTransform>(), 0, y, 320, 44);
            b.onClick.AddListener(() => onClick());
            return b;
        }

        public string NameInput()
        {
            return nameInput != null && !string.IsNullOrEmpty(nameInput.text) ? nameInput.text.Trim() : "沈知行";
        }

        // ---------------- 设置页（含 AI 接入） ----------------

        private void BuildSettings(Transform parent)
        {
            settingsPanel = NewGo("Settings", parent);
            Stretch(settingsPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var dim = settingsPanel.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.72f);
            // 点击遮罩关闭
            var dimBtn = settingsPanel.AddComponent<Button>();
            dimBtn.transition = Selectable.Transition.None;
            dimBtn.onClick.AddListener(CloseSettings);
            settingsCg = settingsPanel.AddComponent<CanvasGroup>();

            // 卡片：按屏高收缩，保证“返回”始终可见
            float cardW = 560f;
            float cardH = Mathf.Min(900f, Mathf.Max(560f, Screen.height - 48f));
            var card = NewGo("SettingsCard", settingsPanel.transform);
            Center(card.GetComponent<RectTransform>(), 0, 0, cardW, cardH);
            var cimg = card.AddComponent<Image>();
            cimg.sprite = CardSprite();
            cimg.type = Image.Type.Sliced;
            cimg.color = FromHex(PaperWarm);
            settingsCard = card.GetComponent<RectTransform>();
            // 卡片自身吞掉点击，避免点内容也关面板
            var cardBtn = card.AddComponent<Button>();
            cardBtn.transition = Selectable.Transition.None;

            // —— 固定头：标题 + 关闭（永远可退出）——
            var header = NewGo("SettingsHeader", card.transform);
            Stretch(header.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(18, -52), new Vector2(-18, -8));
            var hh = header.AddComponent<HorizontalLayoutGroup>();
            hh.childForceExpandWidth = true;
            hh.childForceExpandHeight = false;
            hh.childControlWidth = true;
            hh.childControlHeight = true;
            hh.childAlignment = TextAnchor.MiddleLeft;
            var title = NewText(header.transform, "SettingsTitle", 20, Accent, TextAnchor.MiddleLeft);
            title.text = "设 置";
            title.fontStyle = FontStyle.Bold;
            var titleLe = title.gameObject.AddComponent<LayoutElement>();
            titleLe.flexibleWidth = 1;
            var xBtn = MakeButton(header.transform, "关 闭", 13, true);
            var xLe = xBtn.GetComponent<LayoutElement>();
            xLe.preferredWidth = 88;
            xLe.preferredHeight = 34;
            xBtn.onClick.AddListener(CloseSettings);

            // —— 滚动正文 ——
            var scrollGo = new GameObject("SettingsScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            var srt = scrollGo.GetComponent<RectTransform>();
            srt.SetParent(card.transform, false);
            Stretch(srt, Vector2.zero, Vector2.one, new Vector2(14, 56), new Vector2(-14, -56));
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 26;
            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.GetComponent<RectTransform>().SetParent(scrollGo.transform, false);
            Stretch(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scroll.viewport = viewport.GetComponent<RectTransform>();
            var contentGo = new GameObject("Content", typeof(RectTransform));
            var crt = contentGo.GetComponent<RectTransform>();
            crt.SetParent(viewport.transform, false);
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            crt.sizeDelta = new Vector2(0, 100);
            scroll.content = crt;
            var cv = contentGo.AddComponent<VerticalLayoutGroup>();
            cv.padding = new RectOffset(4, 4, 4, 10);
            cv.spacing = 10;
            cv.childForceExpandHeight = false;
            cv.childForceExpandWidth = true;
            cv.childControlHeight = true;
            cv.childControlWidth = true;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // —— 分区：显示 ——
            var cDisplay = SettingsSection(crt, "显示");
            var sizeRow = NewGo("SizeRow", cDisplay);
            var srg = sizeRow.AddComponent<HorizontalLayoutGroup>();
            srg.spacing = 6;
            srg.childForceExpandHeight = false;
            srg.childForceExpandWidth = false;
            srg.childControlWidth = true;
            srg.childControlHeight = true;
            foreach (var kv in new[] { new[] { "字号：小", "0.85" }, new[] { "字号：标准", "1" }, new[] { "字号：大", "1.2" }, new[] { "音效：开", "sound" } })
            {
                string label = kv[0];
                var b = MakeButton(sizeRow.transform, label, 13, false);
                var le = b.GetComponent<LayoutElement>();
                le.preferredWidth = 118;
                le.preferredHeight = 34;
                if (kv[1] == "sound")
                {
                    settingsSoundBtn = b;
                    b.onClick.AddListener(() =>
                    {
                        SoundFx.Enabled = !SoundFx.Enabled;
                        var lt = b.GetComponentInChildren<Text>();
                        if (lt != null) lt.text = "音效：" + (SoundFx.Enabled ? "开" : "关");
                    });
                }
                else
                {
                    float scale = float.Parse(kv[1], System.Globalization.CultureInfo.InvariantCulture);
                    b.onClick.AddListener(() => { if (OnFontScaleChanged != null) OnFontScaleChanged(scale); });
                }
            }

            // —— 分区：存档 ——
            var cSave = SettingsSection(crt, "存档");
            var snapRow = NewGo("SnapRow", cSave);
            var snapHlg = snapRow.AddComponent<HorizontalLayoutGroup>();
            snapHlg.spacing = 8;
            snapHlg.childForceExpandHeight = false;
            snapHlg.childForceExpandWidth = false;
            snapHlg.childControlWidth = true;
            snapHlg.childControlHeight = true;
            for (int si = 1; si <= 3; si++)
            {
                int captured = si;
                var sb = MakeButton(snapRow.transform, "快照到槽" + si, 12, false);
                var sle = sb.GetComponent<LayoutElement>();
                sle.preferredWidth = 160;
                sle.preferredHeight = 32;
                sb.onClick.AddListener(() => { if (OnSnapshotSlot != null) OnSnapshotSlot(captured); });
            }
            var resetB = MakeButton(cSave, "重置当前槽存档（清空进度）", 13, false);
            var rle = resetB.gameObject.AddComponent<LayoutElement>();
            rle.preferredHeight = 34;
            resetB.onClick.AddListener(() => { if (OnResetSave != null) OnResetSave(); });

            // —— 分区：AI 接入 ——
            var cAi = SettingsSection(crt, "AI 接入（大模型增强，关闭不影响游玩）");
            llmEnableBtn = MakeButton(cAi, "AI 增强：已开启（点此关闭）", 13, true);
            llmEnableBtn.gameObject.AddComponent<LayoutElement>().preferredHeight = 36;
            llmEnableBtn.onClick.AddListener(ToggleLlm);

            var modeRow = NewGo("ModeRow", cAi);
            var mrg = modeRow.AddComponent<HorizontalLayoutGroup>();
            mrg.spacing = 10;
            mrg.childForceExpandHeight = false;
            mrg.childForceExpandWidth = false;
            mrg.childControlWidth = true;
            mrg.childControlHeight = true;
            llmLocalBtn = MakeButton(modeRow.transform, "本地 llama-server", 12, false);
            llmLocalBtn.GetComponent<LayoutElement>().preferredWidth = 240;
            llmLocalBtn.GetComponent<LayoutElement>().preferredHeight = 32;
            llmLocalBtn.onClick.AddListener(() => SetLlmMode("local"));
            llmRemoteBtn = MakeButton(modeRow.transform, "外部 OpenAI 兼容", 12, false);
            llmRemoteBtn.GetComponent<LayoutElement>().preferredWidth = 240;
            llmRemoteBtn.GetComponent<LayoutElement>().preferredHeight = 32;
            llmRemoteBtn.onClick.AddListener(() => SetLlmMode("remote"));

            var autoRow = NewGo("AutoRow", cAi);
            var arg = autoRow.AddComponent<HorizontalLayoutGroup>();
            arg.spacing = 10;
            arg.childForceExpandHeight = false;
            arg.childForceExpandWidth = false;
            arg.childControlWidth = true;
            arg.childControlHeight = true;
            llmAutoBtn = MakeButton(autoRow.transform, "启动时自动加载：关", 12, false);
            llmAutoBtn.GetComponent<LayoutElement>().preferredWidth = 240;
            llmAutoBtn.GetComponent<LayoutElement>().preferredHeight = 32;
            llmAutoBtn.onClick.AddListener(ToggleLlmAuto);
            llmCtx = MakeLlmRowInput(autoRow.transform, "上下文 tokens（默认 16384）",
                v =>
                {
                    if (llmCfg == null) return;
                    int n;
                    if (!int.TryParse(v, out n)) n = 16384;
                    llmCfg.ctx = Math.Max(1024, Math.Min(n, 65536));
                    if (llmCtx != null) llmCtx.text = llmCfg.ctx.ToString();
                    LlmChanged();
                }, 240);
            llmCtx.characterLimit = 6;

            var testB = MakeButton(cAi, "测试连接（首次加载约 1–2 分钟）", 13, true);
            testB.gameObject.AddComponent<LayoutElement>().preferredHeight = 36;
            testB.onClick.AddListener(() => { if (OnLlmTest != null) OnLlmTest(); });

            llmStatus = NewText(cAi, "AiStatus", 12, InkSoft, TextAnchor.UpperLeft);
            var stLe = llmStatus.gameObject.AddComponent<LayoutElement>();
            stLe.preferredHeight = 48;
            llmStatus.text = "未测试。本地模式会自动检测已有服务，不会重复启动。";

            // —— 分区：AI 高级（路径，折叠）——
            var cAdv = SettingsSection(crt, "AI 高级（路径与远程端点）");
            llmEndpoint = MakeSettingsInput(cAdv, "服务地址（OpenAI 兼容 …/v1/chat/completions）",
                v => { if (llmCfg != null) llmCfg.endpoint = v; LlmChanged(); });
            llmKey = MakeSettingsInput(cAdv, "API 密钥（本地服务可留空）",
                v => { if (llmCfg != null) llmCfg.apiKey = v; LlmChanged(); });
            llmModel = MakeSettingsInput(cAdv, "模型名（本地服务可留空）",
                v => { if (llmCfg != null) llmCfg.model = v; LlmChanged(); });
            llmServer = MakeSettingsInput(cAdv, "llama-server.exe 路径",
                v => { if (llmCfg != null) llmCfg.serverExe = v; LlmChanged(); });
            llmModelPath = MakeSettingsInput(cAdv, "模型路径（gguf 文件或所在目录）",
                v => { if (llmCfg != null) llmCfg.modelPath = v; LlmChanged(); });
            llmLora = MakeSettingsInput(cAdv, "LoRA 适配器路径（可选，留空=不挂载）",
                v => { if (llmCfg != null) llmCfg.loraPath = v; LlmChanged(); });

            // —— 底栏：返回（固定）——
            var bottom = NewGo("BottomBar", card.transform);
            Stretch(bottom.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(18, 10), new Vector2(-18, 48));
            var brg = bottom.AddComponent<HorizontalLayoutGroup>();
            brg.spacing = 10;
            brg.childForceExpandHeight = false;
            brg.childForceExpandWidth = true;
            brg.childControlWidth = true;
            brg.childControlHeight = true;
            settingsHint = NewText(bottom.transform, "SettingsHint", 11, "#8F8368", TextAnchor.MiddleLeft);
            var hle = settingsHint.gameObject.AddComponent<LayoutElement>();
            hle.flexibleWidth = 1;
            settingsHint.text = "AI 只产文本，数值走白名单。已有本地服务时只等待、不双开。";
            var closeB = MakeButton(bottom.transform, "返 回", 14, true);
            var cble = closeB.GetComponent<LayoutElement>();
            cble.preferredWidth = 120;
            cble.preferredHeight = 36;
            closeB.onClick.AddListener(CloseSettings);
        }

        private void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsPanel != null && settingsPanel.activeSelf) { CloseSettings(); return; }
                if (talkPanel != null && talkPanel.activeSelf)
                {
                    if (OnTalkClose != null) OnTalkClose();
                    return;
                }
            }
        }

        private Transform SettingsSection(Transform parent, string title)
        {
            var sec = NewGo("Sec_" + title, parent);
            var img = sec.AddComponent<Image>();
            img.sprite = CardSprite();
            img.type = Image.Type.Sliced;
            img.color = FromHex("#FFFDF6");
            var vlg = sec.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 10, 12);
            vlg.spacing = 8;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            var head = NewText(sec.transform, "SecHead", 13, Accent, TextAnchor.MiddleLeft);
            head.text = title;
            head.fontStyle = FontStyle.Bold;
            head.rectTransform.SetParent(sec.transform, false);
            head.gameObject.AddComponent<LayoutElement>().preferredHeight = 20;
            return sec.transform;
        }

        private InputField MakeSettingsInput(Transform parent, string placeholder, Action<string> onCommit)
        {
            var go = NewGo("In_" + placeholder.GetHashCode(), parent);
            go.AddComponent<LayoutElement>().preferredHeight = 34;
            var img = go.AddComponent<Image>();
            img.sprite = CardSprite();
            img.type = Image.Type.Sliced;
            img.color = FromHex("#FFFFFF");
            var field = go.AddComponent<InputField>();
            field.characterLimit = 140;

            var txtGo = NewGo("Text", go.transform);
            Stretch(txtGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var txt = txtGo.AddComponent<Text>();
            txt.font = font; txt.fontSize = S(12); txt.color = FromHex(Ink);
            txt.alignment = TextAnchor.MiddleLeft;
            txt.supportRichText = false;
            field.textComponent = txt;

            var phGo = NewGo("Placeholder", go.transform);
            Stretch(phGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var ph = phGo.AddComponent<Text>();
            ph.font = font; ph.fontSize = S(12); ph.color = FromHex("#8F8368");
            ph.alignment = TextAnchor.MiddleLeft;
            ph.text = placeholder;
            field.placeholder = ph;
            field.onEndEdit.AddListener(s => { if (onCommit != null) onCommit(s == null ? "" : s.Trim()); });
            return field;
        }

        private InputField MakeLlmRowInput(Transform parent, string placeholder, Action<string> onCommit, float w)
        {
            var go = NewGo("InCtx", parent);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = w;
            le.preferredHeight = 32;
            var img = go.AddComponent<Image>();
            img.sprite = CardSprite();
            img.type = Image.Type.Sliced;
            img.color = FromHex("#FFFFFF");
            var field = go.AddComponent<InputField>();
            field.characterLimit = 8;
            var txtGo = NewGo("Text", go.transform);
            Stretch(txtGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var txt = txtGo.AddComponent<Text>();
            txt.font = font; txt.fontSize = S(12); txt.color = FromHex(Ink);
            txt.alignment = TextAnchor.MiddleLeft;
            txt.supportRichText = false;
            field.textComponent = txt;
            var phGo = NewGo("Placeholder", go.transform);
            Stretch(phGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var ph = phGo.AddComponent<Text>();
            ph.font = font; ph.fontSize = S(11); ph.color = FromHex("#8F8368");
            ph.alignment = TextAnchor.MiddleLeft;
            ph.text = placeholder;
            field.placeholder = ph;
            field.onEndEdit.AddListener(s => { if (onCommit != null) onCommit(s == null ? "" : s.Trim()); });
            return field;
        }

        private Button MakeSettingsButton(Transform card, string label, float y, float w, Action onClick)
        {
            var b = MakeButton(card, label, 13, true);
            Top(b.GetComponent<RectTransform>(), 0, y, w, 36);
            b.onClick.AddListener(() => onClick());
            return b;
        }

        // —— AI 接入控件行为 ——

        public void SetLlmConfig(LlmConfig cfg)
        {
            llmCfg = cfg;
            if (cfg == null || llmEndpoint == null) return;
            llmEndpoint.text = cfg.endpoint ?? "";
            llmKey.text = cfg.apiKey ?? "";
            llmModel.text = cfg.model ?? "";
            llmServer.text = cfg.serverExe ?? "";
            llmModelPath.text = cfg.modelPath ?? "";
            llmLora.text = cfg.loraPath ?? "";
            if (llmCtx != null) llmCtx.text = cfg.ctx.ToString();
            RefreshLlmWidgets();
        }

        private void RefreshLlmWidgets()
        {
            if (llmCfg == null || llmEnableBtn == null) return;
            BtnLabel(llmEnableBtn, llmCfg.enabled ? "AI 增强：已开启（点此关闭）" : "AI 增强：已关闭（点此开启）");
            SetBtnColor(llmLocalBtn, llmCfg.mode == "local" ? FromHex(Accent) : FromHex(TabIdle));
            SetBtnColor(llmRemoteBtn, llmCfg.mode == "remote" ? FromHex(Accent) : FromHex(TabIdle));
            if (llmAutoBtn != null)
            {
                BtnLabel(llmAutoBtn, llmCfg.autoStart ? "启动时自动加载：开" : "启动时自动加载：关");
                SetBtnColor(llmAutoBtn, llmCfg.autoStart ? FromHex(Accent) : FromHex(TabIdle));
            }
        }

        private void ToggleLlm()
        {
            if (llmCfg == null) return;
            llmCfg.enabled = !llmCfg.enabled;
            RefreshLlmWidgets();
            if (OnLlmApplied != null) OnLlmApplied();
        }

        private void ToggleLlmAuto()
        {
            if (llmCfg == null) return;
            llmCfg.autoStart = !llmCfg.autoStart;
            RefreshLlmWidgets();
            if (OnLlmApplied != null) OnLlmApplied();
        }

        private void SetLlmMode(string m)
        {
            if (llmCfg == null) return;
            llmCfg.mode = m;
            RefreshLlmWidgets();
            if (OnLlmApplied != null) OnLlmApplied();
        }

        private void LlmChanged() { if (OnLlmApplied != null) OnLlmApplied(); }

        public void ShowLlmStatus(string s) { if (llmStatus != null) llmStatus.text = s; }

        private static void BtnLabel(Button b, string s)
        {
            var t = b.GetComponentInChildren<Text>();
            if (t != null) t.text = s;
        }

        public void ShowSettings()
        {
            settingsPanel.SetActive(true);
            settingsCg.alpha = 0f;
            StartCoroutine(Tween.AlphaCo(settingsCg, 1f, 0.16f, Tween.OutCubic));
            settingsCard.localScale = Vector3.one * 0.92f;
            StartCoroutine(Tween.ScaleCo(settingsCard, 1f, 0.28f, Tween.OutBack));
        }

        public void SetSettingsHint(string s) { if (settingsHint != null) settingsHint.text = s; }

        // ---------------- 顶部与主卡渲染 ----------------

        public void RenderTop(GameState st)
        {
            string s = TextBuilders.TopBar(st);
            lastTopText = s;
            topText.text = s;   // 日期变化直接更新，避免每次交互顶栏闪烁
            if (dayNumText != null && st != null)
                dayNumText.text = GameClock.Parse(st.date).Day.ToString();
        }

        /// <summary>场景签名：kind+标题+段数+选项数+前两段+首选项（+文号/计划值）。同签名=同场景，跳过重建。</summary>
        private static string SceneSig(Scene s)
        {
            var sb = new System.Text.StringBuilder(64);
            sb.Append(s.kind).Append('¦').Append(s.title).Append('¦').Append(s.paras.Count)
              .Append('¦').Append(s.options.Count);
            for (int i = 0; i < s.paras.Count && i < 2; i++) sb.Append('¦').Append(s.paras[i]);
            if (s.options.Count > 0) sb.Append('¦').Append(s.options[0]);
            if (!string.IsNullOrEmpty(s.docNo)) sb.Append("¦D").Append(s.docNo);
            return sb.ToString();
        }

        public void RenderMain(Scene scene)
        {
            // 同场景重复渲染（如锁定选项点击、外部触发的 RenderAll）直接跳过：去闪烁、去 GC 尖峰
            string sig = SceneSig(scene);
            if (sig == lastSceneSig) return;
            lastSceneSig = sig;
            planValTexts = null; planFillImages = null; planTotalText = null;

            // 文件态（红头文头）与便签态（标题带）切换
            bool isDoc = !string.IsNullOrEmpty(scene.docNo);
            titleText.gameObject.SetActive(!isDoc);
            if (titleAccentBar != null) titleAccentBar.SetActive(!isDoc);
            if (docOrg != null) docOrg.gameObject.SetActive(isDoc);
            if (docNoText != null) { docNoText.gameObject.SetActive(isDoc); if (isDoc) docNoText.text = scene.docNo; }
            if (docRule != null) docRule.SetActive(isDoc);

            // 段落分排：逐段交错淡入；文件态首行为文件标题，正文用公文字体＋首行缩进
            ClearChildren(bodyContent);
            int pi = 0;
            if (isDoc)
            {
                var dt = NewDocText(bodyContent, "DocTitle", 19, Ink, TextAnchor.MiddleCenter);
                dt.fontStyle = FontStyle.Bold;
                dt.text = scene.title;
                dt.lineSpacing = 1.16f;
                dt.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                var dcg = dt.gameObject.AddComponent<CanvasGroup>();
                StartCoroutine(Tween.Delayed(0.04f, Tween.AlphaCo(dcg, 1f, 0.18f, Tween.OutCubic)));
                pi = 1;
            }
            foreach (var p in scene.paras)
            {
                var t = isDoc ? NewDocText(bodyContent, "Para", 17, Ink, TextAnchor.UpperLeft)
                              : NewText(bodyContent, "Para", 17, Ink, TextAnchor.UpperLeft);
                t.text = isDoc ? "　　" + p : p;
                t.lineSpacing = 1.16f;
                t.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                var cg = t.gameObject.AddComponent<CanvasGroup>();
                StartCoroutine(Tween.Delayed(0.04f + pi * 0.035f, Tween.AlphaCo(cg, 1f, 0.18f, Tween.OutCubic)));
                pi++;
            }
            if (scene.kind == "week_plan" && scene.planValues != null && scene.planValues.Length == 5)
                BuildPlanEditor(bodyContent.transform, scene.planValues);
            Canvas.ForceUpdateCanvases();
            CenterBodyIfShort();
            Canvas.ForceUpdateCanvases();

            // 选项：交错淡入 + 轻弹；锁定选项置灰禁点并给解锁条件
            ClearChildren(optionsBox);
            optionButtons.Clear();
            for (int i = 0; i < scene.options.Count; i++)
            {
                int captured = i;
                bool locked = scene.optionLocks != null && i < scene.optionLocks.Count
                              && !string.IsNullOrEmpty(scene.optionLocks[i]);
                var b = MakeOptionButton(optionsBox, scene.options[i], 16);
                b.GetComponent<LayoutElement>().preferredHeight = 46;
                if (locked)
                {
                    b.interactable = false;
                    SetBtnColor(b, FromHex("#BFB4A2"));
                    var lt = b.GetComponentInChildren<Text>();
                    if (lt != null)
                        lt.text = scene.options[i].Replace(" 🔒", "") + "　—" + scene.optionLocks[i];
                }
                else
                {
                    b.onClick.AddListener(() => { if (OnOptionChosen != null) OnOptionChosen(captured); });
                }
                optionButtons.Add(b);
                var cg = b.gameObject.AddComponent<CanvasGroup>();
                var tr = b.transform;
                StartCoroutine(Tween.Delayed(0.10f + i * 0.05f, OptionInCo(cg, tr)));
            }

            // 印章：结果核阅 / 月结核毕 / 结局归档 / 卷宗签批
            string stampText = null;
            if (scene.kind == "result") stampText = "已阅";
            else if (scene.kind == "month_end") stampText = "核毕";
            else if (scene.kind == "ending") stampText = "归档";
            else if (scene.kind == "dossier") stampText = "签批";
            if (stampText != null && mainCardRect != null) BuildStamp(stampText);
        }

        // ---------------- 周计划编辑器（Phase 4 / P5 改语义） ----------------

        private static readonly string[] PlanSlotNames = { "签批专注", "调研摸底", "会商协调", "关系走动", "家庭休整" };
        private static readonly string[] PlanSlotHints =
        {
            "当日可多办一件·核对更稳", "问题发现率↑", "协调与会签更顺", "常委与局长关系", "精力回血·压力↓"
        };

        private void BuildPlanEditor(Transform parent, int[] plan)
        {
            planValTexts = new Text[5];
            planFillImages = new Image[5];
            int total = plan[0] + plan[1] + plan[2] + plan[3] + plan[4];
            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                var row = NewGo("PlanRow" + i, parent);
                var rle = row.AddComponent<LayoutElement>();
                rle.preferredHeight = 46;

                var label = NewText(row.transform, "Name", 15, InkSoft, TextAnchor.MiddleLeft);
                label.text = PlanSlotNames[i] + " · " + PlanSlotHints[i];
                Stretch(label.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(2, 2), new Vector2(150, -2));

                var minus = MakeButton(row.transform, "－", 15, false);
                Stretch(minus.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(158, -16), new Vector2(202, 16));
                minus.onClick.AddListener(() => { if (OnPlanAdjusted != null) OnPlanAdjusted(idx, -10); });

                var val = NewText(row.transform, "Val", 17, Accent, TextAnchor.MiddleCenter);
                val.text = plan[i].ToString();
                val.fontStyle = FontStyle.Bold;
                Stretch(val.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(208, 2), new Vector2(258, -2));
                planValTexts[i] = val;

                var track = NewGo("Track", row.transform);
                var timg = track.AddComponent<Image>();
                timg.sprite = BarSprite();
                timg.type = Image.Type.Sliced;
                timg.color = FromHex("#E3DAC4");
                Stretch(track.GetComponent<RectTransform>(), new Vector2(0, 0.32f), new Vector2(1, 0.68f), new Vector2(268, 0), new Vector2(-80, 0));

                var fillGo = NewGo("Fill", track.transform);
                var fimg = fillGo.AddComponent<Image>();
                fimg.sprite = BarSprite();
                fimg.type = Image.Type.Filled;
                fimg.fillMethod = Image.FillMethod.Horizontal;
                fimg.fillAmount = Mathf.Clamp01(plan[i] / 100f);
                fimg.color = FromHex("#4E6E8E");
                Stretch(fillGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                planFillImages[i] = fimg;

                var plus = MakeButton(row.transform, "＋", 15, false);
                Stretch(plus.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-74, -16), new Vector2(-28, 16));
                plus.onClick.AddListener(() => { if (OnPlanAdjusted != null) OnPlanAdjusted(idx, 10); });
            }

            var totalRow = NewGo("PlanTotal", parent);
            totalRow.AddComponent<LayoutElement>().preferredHeight = 34;
            var tt = NewText(totalRow.transform, "Total", 14, total >= 100 ? InkSoft : Accent, TextAnchor.MiddleLeft);
            tt.text = total >= 100
                ? "已分配 " + total + " / 100 —— 一周的力气，就这么多。"
                : "已分配 " + total + " / 100 —— 剩下的精力会白白流走。";
            Stretch(tt.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(2, 2), new Vector2(-2, -2));
            planTotalText = tt;
        }

        /// <summary>周计划编辑器局部刷新：只改数值文本/进度条/合计行，不重建整树。</summary>
        public void UpdatePlanEditor(int[] plan)
        {
            if (plan == null || planValTexts == null || planFillImages == null) return;
            int total = 0;
            for (int i = 0; i < 5; i++) total += plan[i];
            for (int i = 0; i < 5; i++)
            {
                if (planValTexts[i] != null) planValTexts[i].text = plan[i].ToString();
                if (planFillImages[i] != null) planFillImages[i].fillAmount = Mathf.Clamp01(plan[i] / 100f);
            }
            if (planTotalText != null)
            {
                planTotalText.text = total >= 100
                    ? "已分配 " + total + " / 100 —— 一周的力气，就这么多。"
                    : "已分配 " + total + " / 100 —— 剩下的精力会白白流走。";
                planTotalText.color = FromHex(total >= 100 ? InkSoft : Accent);
            }
        }

        /// <summary>改主菜单提示行（存档不兼容等原因说明）。</summary>
        public void SetMenuHint(string s)
        {
            if (menuHint != null) menuHint.text = s;
        }

        // ---------------- 公文字体与印章（Phase 4） ----------------

        /// <summary>公文字体文本（仿宋/楷体；缺字体时回退 UI 字体）。</summary>
        private Text NewDocText(Transform parent, string name, int baseSize, string hex, TextAnchor anchor)
        {
            var t = NewText(parent, name, baseSize, hex, anchor);
            if (docFont != null) t.font = docFont;
            return t;
        }

        /// <summary>朱砂印章：圆环 + 印文，砸落动效（缩放 OutBack 回弹 + 淡入 + 闷响）。</summary>
        private void BuildStamp(string label)
        {
            var stamp = NewGo("Stamp_" + label, mainCardRect);
            var srt = stamp.GetComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = new Vector2(1, 1);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(-92, -42);
            srt.sizeDelta = new Vector2(104, 104);
            var ring = stamp.AddComponent<Image>();
            ring.sprite = RingSprite();
            ring.color = FromHex(Accent);
            ring.raycastTarget = false;
            var txt = NewDocText(stamp.transform, "StampText", 28, Accent, TextAnchor.MiddleCenter);
            txt.text = label;
            txt.fontStyle = FontStyle.Bold;
            txt.raycastTarget = false;
            Stretch(txt.rectTransform, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f), Vector2.zero, Vector2.zero);
            stamp.transform.localEulerAngles = new Vector3(0, 0, -12);
            var cg = stamp.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            stamp.transform.localScale = Vector3.one * 1.8f;
            StartCoroutine(Tween.Delayed(0.10f, StampInCo(cg, stamp.transform)));
        }

        private IEnumerator StampInCo(CanvasGroup cg, Transform tr)
        {
            if (cg == null || tr == null) yield break;
            yield return Tween.AlphaCo(cg, 1f, 0.13f, Tween.OutQuad);
            if (tr == null) yield break;
            yield return Tween.ScaleCo(tr, 1f, 0.24f, Tween.OutBack);
            SoundFx.Play("stamp");
        }

        /// <summary>向正文追加一段（AI 周评等异步文本），带淡入。</summary>
        public void AppendBodyPara(string p)
        {
            if (bodyContent == null || string.IsNullOrEmpty(p)) return;
            var t = NewText(bodyContent, "ParaAi", 16, Ink, TextAnchor.UpperLeft);
            t.text = p;
            t.lineSpacing = 1.16f;
            t.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            StartCoroutine(Tween.AlphaCo(cg, 1f, 0.3f, Tween.OutCubic));
        }

        /// <summary>正文短于视口时垂直居中，避免短场景下方大片空白；超长时回到顶对齐以支持滚动。</summary>
        private void CenterBodyIfShort()        {
            if (bodyViewport == null || bodyContent == null) return;
            float vh = bodyViewport.rect.height;
            float ch = bodyContent.rect.height;
            if (ch < vh - 80f)
            {
                bodyContent.anchorMin = new Vector2(0f, 0.5f);
                bodyContent.anchorMax = new Vector2(1f, 0.5f);
                bodyContent.pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                bodyContent.anchorMin = new Vector2(0f, 1f);
                bodyContent.anchorMax = new Vector2(1f, 1f);
                bodyContent.pivot = new Vector2(0.5f, 1f);
            }
            bodyContent.offsetMin = Vector2.zero;
            bodyContent.offsetMax = Vector2.zero;
            bodyContent.anchoredPosition = Vector2.zero;
        }

        private IEnumerator OptionInCo(CanvasGroup cg, Transform tr)
        {
            if (cg == null || tr == null) yield break;   // 延迟期间选项已被重建销毁
            tr.localScale = Vector3.one * 0.96f;
            yield return Tween.AlphaCo(cg, 1f, 0.16f, Tween.OutCubic);
            if (tr != null) yield return Tween.ScaleCo(tr, 1f, 0.22f, Tween.OutCubic);
        }

        private IEnumerator MenuIn(RectTransform rt, float delay)
        {
            if (rt == null) yield break;
            var cg = rt.GetComponent<CanvasGroup>();
            if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();
            Vector2 final = rt.anchoredPosition;
            rt.anchoredPosition = final + new Vector2(0, 18f);
            cg.alpha = 0f;
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            if (rt == null || cg == null) yield break;
            StartCoroutine(Tween.AlphaCo(cg, 1f, 0.22f, Tween.OutCubic));
            yield return Tween.MoveCo(rt, final, 0.30f, Tween.OutCubic);
        }

        /// <summary>选项按钮：白底、红侧标、墨色文字（公文风）。</summary>
        private Button MakeOptionButton(Transform parent, string label, int baseSize)
        {
            var go = NewGo("Opt_" + label, parent);
            var img = go.AddComponent<Image>();
            img.sprite = CardSprite();
            img.type = Image.Type.Sliced;
            img.color = FromHex("#FDFBF4");
            var b = go.AddComponent<Button>();
            var colors = b.colors;
            colors.highlightedColor = new Color(0.955f, 0.94f, 0.90f, 1f);
            colors.pressedColor = new Color(0.88f, 0.86f, 0.80f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.1f;
            b.colors = colors;
            go.AddComponent<LayoutElement>();
            go.AddComponent<ButtonFx>();

            var bar = NewGo("Accent", go.transform);
            Stretch(bar.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(3, 6), new Vector2(7, -6));
            bar.AddComponent<Image>().color = FromHex(Accent);

            var t = NewText(go.transform, "Label", baseSize, Ink, TextAnchor.MiddleLeft);
            t.text = label;
            t.raycastTarget = false;
            Stretch(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(18, 2), new Vector2(-12, -2));
            b.onClick.AddListener(() => SoundFx.Play("paper"));  // 翻纸音
            return b;
        }

        // ---------------- NPC 交谈弹层 ----------------

        private void BuildTalk(Transform parent)
        {
            talkPanel = NewGo("Talk", parent);
            Stretch(talkPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            talkPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);
            talkCg = talkPanel.AddComponent<CanvasGroup>();
            talkPanel.SetActive(false);

            var card = NewGo("TalkCard", talkPanel.transform);
            Center(card.GetComponent<RectTransform>(), 0, 0, 600, 540);
            var cimg = card.AddComponent<Image>();
            cimg.sprite = CardSprite();
            cimg.type = Image.Type.Sliced;
            cimg.color = FromHex(PaperWarm);
            talkCard = card.GetComponent<RectTransform>();

            // 标题带
            var band = NewGo("TitleBand", card.transform);
            Stretch(band.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -56), Vector2.zero);
            band.AddComponent<Image>().color = FromHex("#F7F1E2");
            var bar = NewGo("AccentBar", band.transform);
            Stretch(bar.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(10, 8), new Vector2(14, -8));
            bar.AddComponent<Image>().color = FromHex(Accent);
            talkTitle = NewText(band.transform, "TalkTitle", 18, Accent, TextAnchor.MiddleLeft);
            Stretch(talkTitle.rectTransform, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 4), new Vector2(-56, -4));

            // 常驻关闭按钮（任何阶段都能退出交谈）
            var closeB = MakeButton(band.transform, "✕", 14, false);
            var crt2 = closeB.GetComponent<RectTransform>();
            crt2.anchorMin = crt2.anchorMax = new Vector2(1, 0.5f);
            crt2.pivot = new Vector2(1, 0.5f);
            crt2.anchoredPosition = new Vector2(-10, 0);
            crt2.sizeDelta = new Vector2(34, 30);
            closeB.onClick.AddListener(() => { if (OnTalkClose != null) OnTalkClose(); });

            talkStatus = NewText(card.transform, "TalkStatus", 12, InkSoft, TextAnchor.MiddleLeft);
            Top(talkStatus.rectTransform, 0, -72, 550, 22);

            // 对话正文滚动
            var scrollGo = new GameObject("TalkScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            var srt = scrollGo.GetComponent<RectTransform>();
            srt.SetParent(card.transform, false);
            Stretch(srt, new Vector2(0, 0), new Vector2(1, 1), new Vector2(18, 104), new Vector2(-18, -98));
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.scrollSensitivity = 20;
            talkScroll = scroll;
            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.GetComponent<RectTransform>().SetParent(scrollGo.transform, false);
            Stretch(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scroll.viewport = viewport.GetComponent<RectTransform>();
            var content = new GameObject("TalkContent", typeof(RectTransform));
            var crt = content.GetComponent<RectTransform>();
            crt.SetParent(viewport.transform, false);
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            crt.sizeDelta = new Vector2(0, 100);
            talkBody = crt;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 9;
            vlg.padding = new RectOffset(6, 6, 8, 8);
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = crt;

            // 回应选项区
            talkOptions = NewGo("TalkOptions", card.transform).GetComponent<RectTransform>();
            Stretch(talkOptions, new Vector2(0, 0), new Vector2(1, 0), new Vector2(18, 14), new Vector2(-18, 100));
            var olg = talkOptions.gameObject.AddComponent<VerticalLayoutGroup>();
            olg.spacing = 8;
            olg.childForceExpandHeight = false;
            olg.childForceExpandWidth = true;
        }

        public void ShowTalk(string npcName)
        {
            talkPanel.SetActive(true);
            talkCg.alpha = 0f;
            StartCoroutine(Tween.AlphaCo(talkCg, 1f, 0.15f, Tween.OutCubic));
            talkCard.localScale = Vector3.one * 0.94f;
            StartCoroutine(Tween.ScaleCo(talkCard, 1f, 0.26f, Tween.OutBack));
            talkTitle.text = "交谈 · " + npcName;
            ClearChildren(talkBody);
            ClearChildren(talkOptions);
            ShowTalkStatus("正在组织语言……");
        }

        public void HideTalk() { talkPanel.SetActive(false); }

        public void ShowTalkStatus(string s)
        {
            if (talkStatus != null) { talkStatus.gameObject.SetActive(true); talkStatus.text = s; }
        }

        public void HideTalkStatus() { if (talkStatus != null) talkStatus.gameObject.SetActive(false); }

        public void AddTalkPara(string p)
        {
            var t = NewText(talkBody, "TalkPara", 14, Ink, TextAnchor.UpperLeft);
            t.text = p;
            t.lineSpacing = 1.15f;
            t.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            StartCoroutine(TalkScrollBottomCo());
        }

        private IEnumerator TalkScrollBottomCo()
        {
            yield return null;
            if (talkScroll == null) yield break;
            Canvas.ForceUpdateCanvases();
            talkScroll.verticalNormalizedPosition = 0f;
        }

        public void SetTalkOptions(string[] labels)
        {
            ClearChildren(talkOptions);
            for (int i = 0; i < labels.Length; i++)
            {
                int captured = i;
                var b = MakeOptionButton(talkOptions, labels[i], 14);
                b.GetComponent<LayoutElement>().preferredHeight = 40;
                b.onClick.AddListener(() => { if (OnTalkOption != null) OnTalkOption(captured); });
            }
        }

        // ---------------- 人物页签：NPC 卡片 + 交谈入口 ----------------

        private void BuildNpcWidgets(GameState st)
        {
            foreach (var id in Npcs.Order)
            {
                var def = Npcs.Defs[id];
                var r = Npcs.Get(st, id);
                var card = NewGo("Npc_" + id, sideContent);
                var img = card.AddComponent<Image>();
                img.sprite = CardSprite();
                img.type = Image.Type.Sliced;
                img.color = FromHex("#FFFDF6");
                var vlg = card.AddComponent<VerticalLayoutGroup>();
                vlg.padding = new RectOffset(10, 10, 8, 9);
                vlg.spacing = 3;
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childControlHeight = true;
                vlg.childControlWidth = true;

                // 头行：印章头像（姓氏白文印）+ 姓名 + 交谈按钮
                var head = NewGo("Head", card.transform);
                var hh = head.AddComponent<LayoutElement>();
                hh.preferredHeight = 40;
                var h = head.AddComponent<HorizontalLayoutGroup>();
                h.spacing = 8;
                h.childForceExpandHeight = false;
                h.childForceExpandWidth = false;
                h.childControlWidth = true;
                h.childControlHeight = true;

                var seal = NewGo("Seal", head.transform);
                var sle = seal.AddComponent<LayoutElement>();
                sle.minWidth = 40;
                sle.preferredWidth = 40;
                sle.preferredHeight = 40;
                var simg = seal.AddComponent<Image>();
                simg.sprite = BarSprite();
                simg.type = Image.Type.Sliced;
                simg.color = FromHex(Accent);
                var sealTxt = NewDocText(seal.transform, "SealChar", 20, "#F5EFE2", TextAnchor.MiddleCenter);
                sealTxt.text = def.name.Substring(0, 1);
                sealTxt.fontStyle = FontStyle.Bold;
                Stretch(sealTxt.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                sealTxt.raycastTarget = false;

                var nm = NewText(head.transform, "Name", 13, Ink, TextAnchor.MiddleLeft);
                nm.text = def.name;
                nm.fontStyle = FontStyle.Bold;
                var sp = NewGo("Sp", head.transform);
                sp.AddComponent<LayoutElement>().flexibleWidth = 1;
                var b = MakeButton(head.transform, "交谈", 11, true);
                var ble = b.GetComponent<LayoutElement>();
                ble.preferredWidth = 48;
                ble.preferredHeight = 22;
                string captured = id;
                b.onClick.AddListener(() => { if (OnNpcTalk != null) OnNpcTalk(captured); });

                var info = NewText(card.transform, "Info", 11, InkSoft, TextAnchor.UpperLeft);
                info.text = def.title + "（" + def.grade + "）";
                var rel = NewText(card.transform, "Rel", 11, InkSoft, TextAnchor.UpperLeft);
                rel.text = "熟悉 " + r.familiar + " · 信任 " + r.trust.ToString("+0;-0;0") + " · 评价 " + r.evalv.ToString("+0;-0;0");

                // 熟悉度关系条（履历卡）
                var track = NewGo("RelTrack", card.transform);
                track.AddComponent<LayoutElement>().preferredHeight = 6;
                var timg = track.AddComponent<Image>();
                timg.sprite = BarSprite();
                timg.type = Image.Type.Sliced;
                timg.color = FromHex("#E3DAC4");
                var fillGo = NewGo("RelFill", track.transform);
                var fimg = fillGo.AddComponent<Image>();
                fimg.sprite = BarSprite();
                fimg.type = Image.Type.Filled;
                fimg.fillMethod = Image.FillMethod.Horizontal;
                fimg.fillAmount = Mathf.Clamp01(r.familiar / 100f);
                fimg.color = FromHex("#4E6E8E");
                Stretch(fillGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

                if (r.memories.Count > 0)
                {
                    var mm = r.memories[r.memories.Count - 1];
                    string mt = mm.text;
                    if (mt.Length > 44) mt = mt.Substring(0, 44) + "…";
                    var mem = NewText(card.transform, "Mem", 11, InkSoft, TextAnchor.UpperLeft);
                    mem.text = "最近：" + mt;
                }
            }
        }

        // ---------------- 新手引导 ----------------

        private static readonly string[][] TutorPages =
        {
            new[] { "欢迎来到长安",
                "2026 年 9 月。你 23 岁，国立中央翰林院大学毕业，通过帝国公职资格考试，入职长安市发展和改革局综合科——一名吏三·科员，试用期一年。",
                "接下来是十年（2026—2036）：晋升、转官、成家、下沉、离开……没有标准答案，只有你的选择。开局先回答一问：“你为什么来？”——志向会悄悄改变你遇到的和没遇到的一切。" },
            new[] { "你的每一周，由你安排",
                "每周一进入「周计划」：把 100 点精力分给岗位、学习、人际、家庭、休整五件事（＋／－ 调整）。投入岗位任务评级更稳，学习长专业，人际长关系，家庭养士气，休整回精力降压力。",
                "周一周二周三周四周五周末，事件照常发生；周五例会按你的分配结算成长。偷懒的话，「推进」会沿用上周的计划自动跑——但疲惫到极限时，组织会强制你休整。",
                "留意左侧锁定（🔒）的选项：它们显示解锁条件——更熟的关系、某个契机。够不着的东西，就该看得见。" },
            new[] { "选择是有回声的",
                "今天说的话，几周、几个月、甚至几年后会回来找你：序章选的出身、基期谈话的措辞、面对一份台账时的摇头或点头——都记在你的「印记」里。",
                "中标了「已阅」印章的结果页，代表这件事已归档；每月月结核对任务、收支与本月大事记；结局时，十年回响会一段段念给你听。",
                "侧面六页签：状态／职业／档案／人物（履历卡）／日志／新闻（剪报）。人物页点「交谈」可闲聊，AI 增强开启时对话更鲜活，关闭也完全可玩。" },
            new[] { "推进与 AI",
                "右上角「推进」：结果页与结构阶段自动选默认项；工作日快进到下一件需要你亲自处理的事——剧情事件、时钟到点、回响抵达都会把它叫停。剧情抉择必须你亲自选。",
                "AI 增强默认挂载本机大模型：交谈、日常小事、科长周评由 AI 生成，首次唤醒约一两分钟。AI 只写文本，数值全走白名单；设置里可测试、换外部 API 或关闭。",
                "顶栏左端是台历（今日大字）与精力／压力／士气。精力见底撑不住任务，压力过高会出错。——现在，去开始你的十年吧。" },
        };

        private void BuildTutorial(Transform parent)
        {
            tutorPanel = NewGo("Tutorial", parent);
            Stretch(tutorPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            tutorPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.66f);
            tutorPanel.transform.SetAsLastSibling();
            tutorPanel.SetActive(false);

            var card = NewGo("TutorCard", tutorPanel.transform);
            Center(card.GetComponent<RectTransform>(), 0, 0, 660, 500);
            var cimg = card.AddComponent<Image>();
            cimg.sprite = CardSprite();
            cimg.type = Image.Type.Sliced;
            cimg.color = FromHex(PaperWarm);

            var band = NewGo("TitleBand", card.transform);
            Stretch(band.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -56), Vector2.zero);
            band.AddComponent<Image>().color = FromHex("#F7F1E2");
            var bar = NewGo("AccentBar", band.transform);
            Stretch(bar.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(10, 8), new Vector2(14, -8));
            bar.AddComponent<Image>().color = FromHex(Accent);
            tutorTitle = NewText(band.transform, "TutorTitle", 18, Accent, TextAnchor.MiddleLeft);
            Stretch(tutorTitle.rectTransform, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(24, 4), new Vector2(-20, -4));

            var scrollGo = new GameObject("TutorScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            var srt = scrollGo.GetComponent<RectTransform>();
            srt.SetParent(card.transform, false);
            Stretch(srt, new Vector2(0, 0), new Vector2(1, 1), new Vector2(24, 76), new Vector2(-24, -70));
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.scrollSensitivity = 20;
            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.GetComponent<RectTransform>().SetParent(scrollGo.transform, false);
            Stretch(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scroll.viewport = viewport.GetComponent<RectTransform>();
            var content = new GameObject("TutorContent", typeof(RectTransform));
            var crt = content.GetComponent<RectTransform>();
            crt.SetParent(viewport.transform, false);
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            crt.sizeDelta = new Vector2(0, 100);
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(4, 4, 10, 10);
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = crt;
            tutorBody = NewText(content.transform, "TutorBody", 14, Ink, TextAnchor.UpperLeft);
            tutorBody.lineSpacing = 1.2f;
            tutorBody.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var skipB = MakeButton(card.transform, "跳过引导", 14, false);
            var srt2 = skipB.GetComponent<RectTransform>();
            srt2.anchorMin = srt2.anchorMax = new Vector2(0, 0);
            srt2.pivot = new Vector2(0, 0);
            srt2.anchoredPosition = new Vector2(24, 16);
            srt2.sizeDelta = new Vector2(130, 40);
            skipB.onClick.AddListener(() => tutorPanel.SetActive(false));

            tutorNext = MakeButton(card.transform, "下一步", 14, true);
            var nrt = tutorNext.GetComponent<RectTransform>();
            nrt.anchorMin = nrt.anchorMax = new Vector2(1, 0);
            nrt.pivot = new Vector2(1, 0);
            nrt.anchoredPosition = new Vector2(-24, 16);
            nrt.sizeDelta = new Vector2(150, 40);
            tutorNext.onClick.AddListener(() =>
            {
                tutorPage++;
                if (tutorPage >= TutorPages.Length) { tutorPanel.SetActive(false); return; }
                RenderTutorial();
            });
        }

        public void ShowTutorial()
        {
            tutorPage = 0;
            RenderTutorial();
            tutorPanel.SetActive(true);
            var cg = tutorPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = tutorPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            StartCoroutine(Tween.AlphaCo(cg, 1f, 0.2f, Tween.OutCubic));
        }

        private void RenderTutorial()
        {
            var page = TutorPages[Mathf.Clamp(tutorPage, 0, TutorPages.Length - 1)];
            tutorTitle.text = "新手引导 · " + (tutorPage + 1) + "／" + TutorPages.Length + " · " + page[0];
            var sb = new StringBuilder();
            for (int i = 1; i < page.Length; i++)
            {
                sb.Append("◆ ");
                sb.Append(page[i]);
                if (i < page.Length - 1) sb.Append("\n\n");
            }
            tutorBody.text = sb.ToString();
            BtnLabel(tutorNext, tutorPage >= TutorPages.Length - 1 ? "开始游戏" : "下一步");
        }

        public void SetAiBadge(string s) { if (aiBadge != null) aiBadge.text = s; }

        /// <summary>推进按钮可用性：待抉择事件在屏时置灰，提示需要玩家亲自选择。</summary>
        public void SetFfEnabled(bool enabled)
        {
            if (ffButton == null) return;
            if (ffButton.interactable == enabled) return;
            ffButton.interactable = enabled;
            SetBtnColor(ffButton, enabled ? Color.white : FromHex("#A99D89"));
        }

        // ---------------- 控件工厂 ----------------

        private static GameObject NewGo(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = offMin; rt.offsetMax = offMax;
        }

        private static void Top(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void Center(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void Bottom(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        /// <summary>侧栏文本块：自动按内容高度伸展。</summary>
        private Text AddTextBlock(Transform parent, string content, int baseSize, string hex)
        {
            var t = NewText(parent, "Block", baseSize, FromHex(hex), TextAnchor.UpperLeft);
            t.text = content;
            t.lineSpacing = 1.14f;
            t.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return t;
        }

        private Text NewText(Transform parent, string name, int baseSize, string hex, TextAnchor anchor)
        {
            return NewText(parent, name, baseSize, FromHex(hex), anchor);
        }

        private Text NewText(Transform parent, string name, int baseSize, Color color, TextAnchor anchor)
        {
            var go = NewGo(name, parent);
            var t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = S(baseSize);
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = false;
            return t;
        }

        /// <summary>普通按钮：圆角红底白字（primary=true）/ 灰底白字（secondary）。</summary>
        private Button MakeButton(Transform parent, string label, int fontSize, bool primary)
        {
            var go = NewGo("Btn_" + label, parent);
            var img = go.AddComponent<Image>();
            img.sprite = ButtonSprite();
            img.type = Image.Type.Sliced;
            img.color = primary ? Color.white : FromHex(TabIdle);
            var b = go.AddComponent<Button>();
            var colors = b.colors;
            colors.highlightedColor = new Color(0.93f, 0.93f, 0.93f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.1f;
            b.colors = colors;
            var t = NewText(go.transform, "Label", fontSize, "#F5EFE2", TextAnchor.MiddleCenter);
            t.text = label;
            t.raycastTarget = false;
            Stretch(t.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            if (go.GetComponent<LayoutElement>() == null) go.AddComponent<LayoutElement>();
            go.AddComponent<ButtonFx>();
            b.onClick.AddListener(() => SoundFx.Play("tick"));   // 全局轻点音
            return b;
        }

        /// <summary>代码里改按钮底色时同步 ButtonFx 基色，避免悬停回跳。</summary>
        private static void SetBtnColor(Button b, Color c)
        {
            if (b == null || b.image == null) return;
            b.image.color = c;
            var fx = b.GetComponent<ButtonFx>();
            if (fx != null) fx.SetBase(c);
        }

        private InputField MakeInput(Transform parent, Vector2 pos)
        {
            var go = NewGo("NameInput", parent);
            Top(go.GetComponent<RectTransform>(), pos.x, pos.y, 320, 42);
            var img = go.AddComponent<Image>();
            img.sprite = CardSprite();
            img.type = Image.Type.Sliced;
            img.color = FromHex("#3A342C");
            var field = go.AddComponent<InputField>();
            field.characterLimit = 12;

            var txtGo = NewGo("Text", go.transform);
            Stretch(txtGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(12, 0), new Vector2(-12, 0));
            var txt = txtGo.AddComponent<Text>();
            txt.font = font; txt.fontSize = S(17); txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.text = "沈知行";
            field.textComponent = txt;
            field.text = "沈知行";

            var phGo = NewGo("Placeholder", go.transform);
            Stretch(phGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(12, 0), new Vector2(-12, 0));
            var ph = phGo.AddComponent<Text>();
            ph.font = font; ph.fontSize = S(14); ph.color = FromHex("#8F8368");
            ph.alignment = TextAnchor.MiddleLeft;
            ph.text = "输入主角姓名…";
            field.placeholder = ph;
            return field;
        }

        private static void ClearChildren(RectTransform rt)
        {
            for (int i = rt.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(rt.GetChild(i).gameObject);
        }

        private static void Destroy(GameObject go)
        {
            UnityEngine.Object.Destroy(go);
        }

        private static Color FromHex(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color32(r, g, b, 255);
        }

        private static Color WithAlpha(Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }
    }
}
