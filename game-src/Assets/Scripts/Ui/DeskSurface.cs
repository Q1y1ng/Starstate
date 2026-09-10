using UnityEngine;
using UnityEngine.UI;

namespace Starstate.Ui
{
    /// <summary>
    /// 办公桌程序纹理与场景层（从 UiRoot 拆出的 DeskSurface 职责）。
    /// 木纹、台灯晕、纸纹、印章环、撕边——零外部美术资产。
    /// UiRoot.Build 通过 DeskSurface.BuildBackground 挂桌底。
    /// </summary>
    public static class DeskSurface
    {
        static Texture2D _paperTex, _woodTex;
        static Sprite _paperSprite, _woodSprite, _lampSprite, _ringSprite, _tornSprite;

        /// <summary>米纸纹：细颗粒 + 随机纤维，平铺用。</summary>
        public static Texture2D PaperTex()
        {
            if (_paperTex != null) return _paperTex;
            const int w = 128, h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            var rng = new System.Random(20260906);
            var px = new Color32[w * h];
            for (int i = 0; i < w * h; i++)
            {
                int v = 243 + rng.Next(-7, 8);
                px[i] = new Color32((byte)v, (byte)(v - 2), (byte)(v - 14), 255);
            }
            for (int k = 0; k < 110; k++)
            {
                int x0 = rng.Next(w), y0 = rng.Next(h), len = rng.Next(3, 10);
                int c = 232 + rng.Next(6);
                for (int j = 0; j < len; j++)
                {
                    int x = (x0 + j) % w;
                    px[y0 * w + x] = new Color32((byte)c, (byte)(c - 2), (byte)(c - 14), 255);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            _paperTex = tex;
            return tex;
        }

        public static Sprite PaperSprite()
        {
            if (_paperSprite == null)
                _paperSprite = Sprite.Create(PaperTex(), new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 64f);
            return _paperSprite;
        }

        /// <summary>胡桃木纹：横向纹理 + 节疤暗斑，平铺桌面。</summary>
        public static Texture2D WoodTex()
        {
            if (_woodTex != null) return _woodTex;
            const int w = 128, h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            var rng = new System.Random(20260919);
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float ring = Mathf.Sin(y * 0.22f) * 6f + Mathf.Sin(y * 0.07f + 1.3f) * 4f;
                for (int x = 0; x < w; x++)
                {
                    float grain = Mathf.Sin((x + y * 0.15f) * 0.55f) * 5f
                                + Mathf.Sin((x * 0.12f + y * 0.4f)) * 3f
                                + rng.Next(-3, 4);
                    int r = 78 + (int)(ring + grain);
                    int g = 52 + (int)(ring * 0.7f + grain * 0.6f);
                    int b = 34 + (int)(grain * 0.35f);
                    px[y * w + x] = new Color32(
                        (byte)Mathf.Clamp(r, 40, 120),
                        (byte)Mathf.Clamp(g, 28, 90),
                        (byte)Mathf.Clamp(b, 18, 70),
                        255);
                }
            }
            for (int k = 0; k < 3; k++)
            {
                int cx = rng.Next(16, w - 16), cy = rng.Next(16, h - 16), rad = rng.Next(6, 14);
                for (int y = cy - rad; y <= cy + rad; y++)
                    for (int x = cx - rad; x <= cx + rad; x++)
                    {
                        int xx = (x + w) % w, yy = (y + h) % h;
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        if (d < rad)
                        {
                            float t = 1f - d / rad;
                            var c = px[yy * w + xx];
                            px[yy * w + xx] = new Color32(
                                (byte)Mathf.Clamp(c.r - (int)(t * 22), 30, 130),
                                (byte)Mathf.Clamp(c.g - (int)(t * 14), 20, 100),
                                (byte)Mathf.Clamp(c.b - (int)(t * 8), 12, 80),
                                255);
                        }
                    }
            }
            tex.SetPixels32(px);
            tex.Apply();
            _woodTex = tex;
            return tex;
        }

        public static Sprite WoodSprite()
        {
            if (_woodSprite == null)
                _woodSprite = Sprite.Create(WoodTex(), new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 64f);
            return _woodSprite;
        }

        /// <summary>台灯暖晕：左上角径向渐变。</summary>
        public static Sprite LampGlowSprite()
        {
            if (_lampSprite != null) return _lampSprite;
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float lx = size * 0.28f, ly = size * 0.72f;
            float maxR = size * 0.95f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(lx, ly));
                    float a = Mathf.Clamp01(1f - d / maxR);
                    a = a * a * 0.22f;
                    tex.SetPixel(x, y, new Color(1f, 0.88f, 0.62f, a));
                }
            tex.Apply();
            _lampSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _lampSprite;
        }

        /// <summary>印章圆环（白填色，由 Image 染成朱砂）。</summary>
        public static Sprite RingSprite()
        {
            if (_ringSprite != null) return _ringSprite;
            const int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = (size - 1) / 2f;
            float r0 = size / 2f - 8.5f, r1 = size / 2f - 3f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                    float a = Mathf.Clamp01(Mathf.Min(d - r0, r1 - d) + 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            tex.Apply();
            _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _ringSprite;
        }

        /// <summary>报纸剪报撕边。</summary>
        public static Sprite TornSprite()
        {
            if (_tornSprite != null) return _tornSprite;
            const int w = 96, h = 20;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            var rng = new System.Random(41);
            for (int x = 0; x < w; x++)
            {
                int jag = 2 + rng.Next(7);
                for (int y = 0; y < h; y++)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, y < jag ? 0f : 1f));
            }
            tex.Apply();
            _tornSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            return _tornSprite;
        }

        /// <summary>在 Canvas 下挂木纹桌底＋台灯晕（sibling 0/1）。</summary>
        public static void BuildBackground(Transform canvas)
        {
            var bg = new GameObject("DeskBG", typeof(RectTransform));
            var brt = bg.GetComponent<RectTransform>();
            brt.SetParent(canvas, false);
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.sprite = WoodSprite();
            bgImg.type = Image.Type.Tiled;
            bg.transform.SetSiblingIndex(0);

            var lamp = new GameObject("LampGlow", typeof(RectTransform));
            var lrt = lamp.GetComponent<RectTransform>();
            lrt.SetParent(canvas, false);
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            var lampImg = lamp.AddComponent<Image>();
            lampImg.sprite = LampGlowSprite();
            lampImg.raycastTarget = false;
            lamp.transform.SetSiblingIndex(1);
        }
    }
}
