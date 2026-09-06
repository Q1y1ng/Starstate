using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Starstate.Ui
{
    /// <summary>
    /// 轻量代码补间（无 Animator 资产）：由 UiRoot / ButtonFx 用协程驱动。
    /// 所有协程在目标销毁后自动中止（Unity 假 null 检查），重复调用以最新目标接管。
    /// </summary>
    public static class Tween
    {
        // ---------------- 缓动 ----------------

        public static float OutCubic(float t) { return 1f - Mathf.Pow(1f - t, 3f); }
        public static float OutQuad(float t) { return 1f - (1f - t) * (1f - t); }
        public static float OutBack(float t)
        {
            const float c = 1.70158f;
            float x = t - 1f;
            return 1f + x * x * ((c + 1f) * x + c);
        }

        // ---------------- 协程体 ----------------

        public static IEnumerator AlphaCo(CanvasGroup cg, float to, float dur, Func<float, float> ease)
        {
            if (cg == null) yield break;   // Delayed 恢复时目标可能已被重建销毁
            float from = cg.alpha;
            float time = 0f;
            while (time < dur)
            {
                if (cg == null) yield break;
                time += Time.unscaledDeltaTime;
                cg.alpha = Mathf.LerpUnclamped(from, to, ease(Mathf.Clamp01(time / dur)));
                yield return null;
            }
            if (cg != null) cg.alpha = to;
        }

        public static IEnumerator ScaleCo(Transform tr, float to, float dur, Func<float, float> ease)
        {
            if (tr == null) yield break;
            Vector3 from = tr.localScale;
            Vector3 target = Vector3.one * to;
            float time = 0f;
            while (time < dur)
            {
                if (tr == null) yield break;
                time += Time.unscaledDeltaTime;
                tr.localScale = Vector3.LerpUnclamped(from, target, ease(Mathf.Clamp01(time / dur)));
                yield return null;
            }
            if (tr != null) tr.localScale = target;
        }

        public static IEnumerator FillCo(Image img, float to, float dur, Func<float, float> ease)
        {
            if (img == null) yield break;
            float from = img.fillAmount;
            float time = 0f;
            while (time < dur)
            {
                if (img == null) yield break;
                time += Time.unscaledDeltaTime;
                img.fillAmount = Mathf.LerpUnclamped(from, to, ease(Mathf.Clamp01(time / dur)));
                yield return null;
            }
            if (img != null) img.fillAmount = to;
        }

        public static IEnumerator MoveCo(RectTransform rt, Vector2 to, float dur, Func<float, float> ease)
        {
            if (rt == null) yield break;
            Vector2 from = rt.anchoredPosition;
            float time = 0f;
            while (time < dur)
            {
                if (rt == null) yield break;
                time += Time.unscaledDeltaTime;
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, ease(Mathf.Clamp01(time / dur)));
                yield return null;
            }
            if (rt != null) rt.anchoredPosition = to;
        }

        public static IEnumerator ColorCo(Graphic g, Color to, float dur, Func<float, float> ease)
        {
            if (g == null) yield break;
            Color from = g.color;
            float time = 0f;
            while (time < dur)
            {
                if (g == null) yield break;
                time += Time.unscaledDeltaTime;
                g.color = Color.LerpUnclamped(from, to, ease(Mathf.Clamp01(time / dur)));
                yield return null;
            }
            if (g != null) g.color = to;
        }

        public static IEnumerator Delayed(float delay, IEnumerator then)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            while (then.MoveNext()) yield return then.Current;
        }
    }
}
