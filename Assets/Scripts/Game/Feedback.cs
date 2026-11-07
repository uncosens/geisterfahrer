using UnityEngine;

namespace Geisterfahrer.Game
{
    // Hit and crash feedback (look-spec 9): sparks, camera shake, haptics.
    // ponytail: no debris, smoke, decals or dents yet; add when the real models are in.
    public class Feedback
    {
        readonly ParticleSystem sparks;
        float shakeT, shakeDur, shakePos, shakeRot, shakeFreq;

        public Feedback(Material sparkMat)
        {
            sparks = new GameObject("Sparks").AddComponent<ParticleSystem>();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = sparks.main;
            main.loop = false; main.playOnAwake = false; main.maxParticles = 128;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6, 14);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
            main.gravityModifier = 1;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = sparks.emission;
            emission.rateOverTime = 0;
            var shape = sparks.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25;
            shape.radius = 0.05f;
            var col = sparks.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(new Color(1, 0.96f, 0.82f), 0), new GradientColorKey(new Color(1, 0.7f, 0.28f), 0.4f), new GradientColorKey(new Color(1, 0.48f, 0.1f), 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.4f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var r = sparks.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.04f; r.lengthScale = 4;
            r.sharedMaterial = sparkMat;
            sparks.Play();
        }

        // normalX: +1 contact on the player's right, -1 left; 0 = front.
        void Aim(Vector3 point, float normalX)
        {
            sparks.transform.position = point;
            // Sparks fly away from the contact and backward (the world rushes toward -z).
            var dir = new Vector3(-normalX, 0.3f, normalX == 0 ? 1 : -0.6f);
            sparks.transform.rotation = Quaternion.LookRotation(dir);
        }

        public void Hit(Vector3 point, float normalX, float j)
        {
            Aim(point, normalX);
            sparks.Emit(Mathf.Min(36, 12 + (int)(4 * j)));
            Shake(0.02f + 0.012f * j, 0.2f + 0.15f * j, 25, 0.18f);
            Vibrate(20 + (long)(5 * j), Mathf.Min(255, 80 + (int)(20 * j)));
        }

        public void Scrape(Vector3 point, float normalX, float dt)
        {
            Aim(point, normalX);
            sparks.Emit(Mathf.Max(1, Mathf.RoundToInt(60 * dt)));
        }

        public void Crash(Vector3 point)
        {
            Aim(point, 0);
            var shape = sparks.shape;
            shape.angle = 70;
            sparks.Emit(80);
            shape.angle = 25;
            Shake(0.25f, 2.5f, 18, 0.5f);
            Vibrate(180, 255);
        }

        void Shake(float pos, float rot, float freq, float dur)
        {
            if (pos < shakePos * (1 - shakeT / Mathf.Max(shakeDur, 1e-3f))) return; // keep a stronger shake running
            shakePos = pos; shakeRot = rot; shakeFreq = freq; shakeDur = dur; shakeT = 0;
        }

        // Real-time Perlin shake, ease-out over its duration. Returns position offset and pitch/yaw in degrees.
        public void ShakeOffset(out Vector3 pos, out Vector2 rot)
        {
            shakeT += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(1 - shakeT / shakeDur);
            k *= k;
            float t = Time.unscaledTime * shakeFreq;
            float N(float seed) => Mathf.PerlinNoise(t, seed) * 2 - 1;
            pos = new Vector3(N(1), N(2), 0) * shakePos * k;
            rot = new Vector2(N(3), N(4)) * shakeRot * k;
        }

        static AndroidJavaObject vibrator;

        public static void Vibrate(long ms, int amplitude)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (vibrator == null)
                    using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                        vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                using (var effect = new AndroidJavaClass("android.os.VibrationEffect"))
                    vibrator.Call("vibrate", effect.CallStatic<AndroidJavaObject>("createOneShot", ms, amplitude));
            }
            catch (System.Exception) { } // no vibrator: ignore
#endif
        }

        // Never called. Referencing Handheld.Vibrate makes Unity add the VIBRATE permission to the manifest.
        static void KeepVibratePermission() => Handheld.Vibrate();
    }
}
