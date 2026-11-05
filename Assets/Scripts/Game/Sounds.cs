using UnityEngine;

namespace Geisterfahrer.Game
{
    // Procedural placeholder sounds: engine hum (loop), noise (scrape loop, crash one-shot).
    public static class Sounds
    {
        const int Rate = 22050;

        public static AudioClip Engine()
        {
            // 0.5 s loop of a 100 Hz buzz with harmonics; pitch follows speed.
            var d = new float[Rate / 2];
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                d[i] = 0.3f * Mathf.Sin(2 * Mathf.PI * 100 * t) + 0.15f * Mathf.Sin(2 * Mathf.PI * 200 * t) + 0.08f * Mathf.Sin(2 * Mathf.PI * 300 * t);
            }
            return Make("engine", d);
        }

        public static AudioClip Noise(string name, float seconds, bool fade)
        {
            var d = new float[(int)(Rate * seconds)];
            var r = new System.Random(1);
            for (int i = 0; i < d.Length; i++)
                d[i] = (float)(r.NextDouble() * 2 - 1) * (fade ? 1 - i / (float)d.Length : 0.5f);
            return Make(name, d);
        }

        static AudioClip Make(string name, float[] d)
        {
            var c = AudioClip.Create(name, d.Length, 1, Rate, false);
            c.SetData(d, 0);
            return c;
        }
    }
}
