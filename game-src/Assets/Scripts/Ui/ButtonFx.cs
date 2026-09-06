using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Starstate.Ui
{
    /// <summary>按钮悬停/按压微反馈：轻微缩放＋颜色过渡（挂在带 Graphic 的按钮物体上）。</summary>
    public class ButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Graphic g;
        private Color baseColor;
        private bool hover;
        private Coroutine colorCo, scaleCo;

        private void OnEnable()
        {
            g = GetComponent<Graphic>();
            if (g != null) baseColor = g.color;
        }

        /// <summary>代码里改按钮底色时同步基色（否则悬停会弹回旧色）。</summary>
        public void SetBase(Color c)
        {
            baseColor = c;
            if (g != null) g.color = c;
        }

        public void OnPointerEnter(PointerEventData e)
        {
            hover = true;
            Scale(1.02f);
            Tint(Darken(baseColor, 0.90f));
        }

        public void OnPointerExit(PointerEventData e)
        {
            hover = false;
            Scale(1f);
            Tint(baseColor);
        }

        public void OnPointerDown(PointerEventData e) { Scale(0.97f); }
        public void OnPointerUp(PointerEventData e) { Scale(hover ? 1.02f : 1f); }

        private static Color Darken(Color c, float k)
        {
            return new Color(c.r * k, c.g * k, c.b * k, c.a);
        }

        private void Scale(float to)
        {
            if (scaleCo != null) StopCoroutine(scaleCo);
            scaleCo = StartCoroutine(Tween.ScaleCo(transform, to, 0.09f, Tween.OutQuad));
        }

        private void Tint(Color to)
        {
            if (g == null) return;
            if (colorCo != null) StopCoroutine(colorCo);
            colorCo = StartCoroutine(Tween.ColorCo(g, to, 0.12f, Tween.OutQuad));
        }
    }
}
