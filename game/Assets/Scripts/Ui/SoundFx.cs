using System.Collections.Generic;
using UnityEngine;

namespace Starstate.Ui
{
    /// <summary>
    /// 程序生成音效（无音频资产）：印章闷响 / 纸张沙沙 / 轻点。
    /// 全部 PCM 合成，AudioSource 单例挂在 UiRoot 所在物体上；音量可在设置页开关。
    /// </summary>
    public static class SoundFx
    {
        private static AudioSource _src;
        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();
        public static bool Enabled = true;

        public static void Bind(GameObject host)
        {
            if (_src != null) return;
            _src = host.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.volume = 0.8f;
        }

        public static void Play(string name)
        {
            if (!Enabled || _src == null) return;
            var clip = Get(name);
            if (clip != null) _src.PlayOneShot(clip);
        }

        private static AudioClip Get(string name)
        {
            if (Cache.ContainsKey(name)) return Cache[name];
            AudioClip clip = null;
            switch (name)
            {
                case "stamp": clip = Stamp(); break;
                case "paper": clip = Paper(); break;
                case "tick": clip = Tick(); break;
            }
            if (clip != null) Cache[name] = clip;
            return clip;
        }

        /// <summary>印章砸落：低频冲击 + 短噪声。</summary>
        private static AudioClip Stamp()
        {
            const int sr = 22050;
            float dur = 0.16f;
            int n = (int)(sr * dur);
            var data = new float[n];
            var rng = new System.Random(7);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)sr;
                float thump = Mathf.Sin(2f * Mathf.PI * 52f * t) * Mathf.Exp(-t * 38f) * 0.9f;
                float noise = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 55f) * 0.35f;
                data[i] = Mathf.Clamp(thump + noise, -1f, 1f);
            }
            return Make("stamp", data, sr);
        }

        /// <summary>翻纸：带宽噪声 + 包络抖动。</summary>
        private static AudioClip Paper()
        {
            const int sr = 22050;
            float dur = 0.22f;
            int n = (int)(sr * dur);
            var data = new float[n];
            var rng = new System.Random(17);
            float last = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)sr;
                float raw = (float)rng.NextDouble() * 2f - 1f;
                last = last * 0.82f + raw * 0.18f;             // 简易低通
                float env = Mathf.Sin(Mathf.PI * (t / dur)) * (0.55f + 0.45f * Mathf.Sin(t * 61f));
                data[i] = Mathf.Clamp(last * env * 0.5f, -1f, 1f);
            }
            return Make("paper", data, sr);
        }

        /// <summary>轻点：极短的木质感 tick。</summary>
        private static AudioClip Tick()
        {
            const int sr = 22050;
            float dur = 0.05f;
            int n = (int)(sr * dur);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)sr;
                data[i] = Mathf.Sin(2f * Mathf.PI * 880f * t) * Mathf.Exp(-t * 140f) * 0.5f;
            }
            return Make("tick", data, sr);
        }

        private static AudioClip Make(string name, float[] data, int sr)
        {
            var clip = AudioClip.Create(name, data.Length, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
