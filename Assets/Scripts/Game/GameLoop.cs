using System.Collections.Generic;
using Geisterfahrer.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Geisterfahrer.Game
{
    // Owns the run and syncs everything else to it: views, chase camera, HUD, feedback, sound.
    public class GameLoop : MonoBehaviour
    {
        public Tuning tuning = new Tuning();
        public Art art = new Art();

        enum State { Start, Playing, Paused, Dying, Over }
        State state = State.Start;
        Run run;
        DriveInput input;
        TrackView road;
        Hud hud;
        Feedback fx;
        Camera cam;
        Transform player;
        AudioSource engine, scrape, crash, hit, tyre, wind, whoosh, rumble;
        AudioLowPassFilter windFilter;
        AudioReverbFilter echo;
        ParticleSystem streaks;
        Light sun;
        float sunIntensity, ambient;
        ColorAdjustments grade;
        Vignette vignette;
        readonly Dictionary<Vehicle, GameObject> views = new Dictionary<Vehicle, GameObject>();
        readonly List<Vehicle> gone = new List<Vehicle>();
        readonly List<GameObject>[] pools = new List<GameObject>[Road.Types.Length];
        float camX, camVel, dyingTime;
        // Driving feel (look-spec 11): smoothed camera and body values.
        float fovH = 80, kick, camBack = 6.5f, camYaw, camRoll, lastV, pitch, pitchVel, bodyRoll, bodyRollVel, rumbleTime;
        int lastHits, seed;

        static readonly Color Sky = Placeholder.Hex(0xC9D6E2);

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Placeholder.BaseMat = art.baseLit;
            input = new DriveInput();

            cam = new GameObject("Camera").AddComponent<Camera>();
            cam.clearFlags = RenderSettings.skybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            cam.backgroundColor = Sky;
            cam.nearClipPlane = 0.3f;
            cam.gameObject.AddComponent<AudioListener>();
            cam.gameObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Sky;
            sun = FindAnyObjectByType<Light>();
            if (!sun) // the scene normally has the sun (look-spec 1)
            {
                sun = new GameObject("Sun").AddComponent<Light>();
                sun.type = LightType.Directional; sun.shadows = LightShadows.Hard;
                sun.transform.rotation = Quaternion.Euler(40, 30, 0);
            }
            sunIntensity = sun.intensity;
            ambient = RenderSettings.ambientIntensity;
            var vol = FindAnyObjectByType<Volume>();
            if (vol) { vol.profile.TryGet(out grade); vol.profile.TryGet(out vignette); } // runtime copy, the asset stays untouched
            streaks = Streaks(cam.transform, art.spark);
            echo = cam.gameObject.AddComponent<AudioReverbFilter>();
            echo.reverbPreset = AudioReverbPreset.StoneCorridor;
            echo.enabled = false;

            road = new GameObject("Road").AddComponent<TrackView>();
            float pl = (float)Road.PlayerLength;
            player = (art.player ? Placeholder.Fit(art.player, pl, "Player", Placeholder.PlayerPaint, Flipped.Contains("Player"))
                : Placeholder.Vehicle("Player", pl, (float)Road.PlayerWidth, 1.3f, Placeholder.PlayerPaint)).transform;
            for (int i = 0; i < pools.Length; i++) pools[i] = new List<GameObject>();

            hud = new Hud(art.bold, art.semi, art.black, art.boldShadow, art.semiShadow)
            {
                Play = () => { if (state == State.Over) NewRun(); Go(State.Playing); },
                Pause = () => Go(State.Paused),
                Resume = () => Go(State.Playing),
                Home = () => { NewRun(); Go(State.Start); },
                Retry = () => { NewRun(); Go(State.Playing); },
            };
            fx = new Feedback(art.spark);

            engine = Audio(Sounds.Engine(), true);
            scrape = Audio(Sounds.Noise("scrape", 1, false), true);
            crash = Audio(Sounds.Noise("crash", 0.8f, true), false);
            hit = Audio(Sounds.Noise("hit", 0.15f, true), false);
            tyre = Audio(Sounds.Noise("tyre", 1, false), true);
            wind = Audio(Sounds.Noise("wind", 1.3f, false), true);
            windFilter = gameObject.AddComponent<AudioLowPassFilter>(); // ponytail: filters every source on this object; give wind its own object if that matters
            whoosh = Audio(Sounds.Noise("whoosh", 0.5f, true), false);
            rumble = Audio(Sounds.Engine(), true);
            scrape.volume = tyre.volume = wind.volume = rumble.volume = 0;
            NewRun();
            Go(State.Start);
        }

        AudioSource Audio(AudioClip clip, bool loop)
        {
            var a = gameObject.AddComponent<AudioSource>();
            a.clip = clip; a.loop = loop;
            if (loop) a.Play();
            return a;
        }

        void NewRun()
        {
            foreach (var kv in views) Release(kv.Key, kv.Value);
            views.Clear();
            run = new Run(tuning, seed++);
            road.Init(art, run.Track);
            lastHits = 0;
            camX = (float)run.X;
            Time.timeScale = 1;
            if (grade) { grade.saturation.value = -6; grade.postExposure.value = 0; }
        }

        void Go(State s)
        {
            state = s;
            hud.Show(s == State.Start ? Hud.Screen.Start : s == State.Paused ? Hud.Screen.Pause : s == State.Over ? Hud.Screen.Over : Hud.Screen.Hud);
            hud.SetBest(PlayerPrefs.GetFloat("bestSpeed", 0), PlayerPrefs.GetFloat("bestDist", 0));
        }

        void Update()
        {
            var k = Keyboard.current;
            bool pauseKey = k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame);
            input.Update(p => state != State.Playing || RectTransformUtility.RectangleContainsScreenPoint(hud.PauseButton, p, null));

            if (state == State.Playing)
            {
                if (pauseKey) Go(State.Paused);
                run.Step(Mathf.Min(Time.deltaTime, 0.05f), input.Steer, input.Gas);
                Effects();
            }
            else if (state == State.Paused && pauseKey) Go(State.Playing);
            else if (state == State.Dying)
            {
                // Slow motion for 0.8 s real time, then frozen; game over at 1.0 s (look-spec 9.2).
                dyingTime += Time.unscaledDeltaTime;
                if (dyingTime > 0.8f) Time.timeScale = 0;
                if (grade) // grey-out over 0.8 s, so the red car loses its colour too (look-spec 9.2)
                {
                    float g = Mathf.Clamp01(dyingTime / 0.8f);
                    grade.saturation.value = Mathf.Lerp(-6, -100, g);
                    grade.postExposure.value = -0.3f * g;
                }
                if (dyingTime > 1.0f) EndRun();
            }
            else if ((state == State.Start || state == State.Over) && k != null && k.enterKey.wasPressedThisFrame) hud.Play();

            Sync();
            hud.SetTouch(state == State.Playing ? input.SteerOrigin : null, input.SteerX);
            hud.SetRun(run.V * 3.6, run.Distance);
            hud.Tick();
            if (Debug.isDebugBuild) Perf();
        }

        // Dev builds: one logcat line every 5 s (adb logcat -s Unity | findstr PERF).
        float perfTime, perfWorst;
        int perfFrames;

        void Perf()
        {
            perfFrames++;
            perfWorst = Mathf.Max(perfWorst, Time.unscaledDeltaTime);
            if ((perfTime += Time.unscaledDeltaTime) < 5) return;
            Debug.Log($"PERF fps {perfFrames / perfTime:F1} worst {perfWorst * 1000:F1} ms vehicles {run.Vehicles.Count} kmh {run.V * 3.6:F0} mem {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() >> 20} MB");
            perfTime = perfWorst = 0; perfFrames = 0;
        }

        void Effects()
        {
            var point = TrackView.Place((float)run.HitZ, (float)run.HitX, 0.5f);
            float side = (float)run.HitNx;
            if (run.Crashed)
            {
                state = State.Dying; dyingTime = 0; Time.timeScale = 0.25f;
                fx.Crash(point);
                hud.Flash(-1, 0.65f, 0.1f, 0.5f);
                crash.Play();
                return;
            }
            if (run.Hits > lastHits)
            {
                lastHits = run.Hits;
                float j = (float)run.LastImpulse;
                fx.Hit(point, side, j);
                bodyRollVel += 1.5f * j / 6 * 20 * (side < 0 ? 1 : -1); // roll kick
                if (side != 0) hud.Flash(side < 0 ? 0 : 1, Mathf.Min(0.33f, 0.15f + 0.03f * j), 0.06f, 0.2f);
                hit.volume = 0.5f + 0.08f * j;
                hit.pitch = Random.Range(0.9f, 1.1f);
                hit.Play();
            }
            else if (run.Scraping && run.V > 0) fx.Scrape(point, side, Time.deltaTime);
        }

        void EndRun()
        {
            float speed = (float)(run.MaxSpeed * 3.6), dist = (float)run.Distance;
            float bestSpeed = PlayerPrefs.GetFloat("bestSpeed", 0), bestDist = PlayerPrefs.GetFloat("bestDist", 0);
            bool nbs = speed > bestSpeed, nbd = dist > bestDist;
            if (nbs) PlayerPrefs.SetFloat("bestSpeed", bestSpeed = speed);
            if (nbd) PlayerPrefs.SetFloat("bestDist", bestDist = dist);
            PlayerPrefs.Save();
            hud.SetResult(0, speed.ToString("F1"), "km/h", bestSpeed.ToString("F1"), nbs);
            hud.SetResult(1, (dist / 1000).ToString("F2"), "km", (bestDist / 1000).ToString("F2"), nbd);
            Go(State.Over);
        }

        void Sync()
        {
            road.Scroll(run.Distance);
            // Road drawn to 1.5 km with light haze (GDD 5.3, decision 15); vehicles appear at the spawn distance.
            RenderSettings.fogStartDistance = 300;
            RenderSettings.fogEndDistance = 1600;
            cam.farClipPlane = 1650;

            float dt = Time.unscaledDeltaTime, sp01 = (float)(run.V / run.TopSpeed), kmh = (float)run.V * 3.6f;
            float accel = Time.deltaTime > 0 ? ((float)run.V - lastV) / Time.deltaTime : 0;
            lastV = (float)run.V;

            // Body (look-spec 11.2): pitch with acceleration, roll out of the turn; springs.
            float pitchTarget = accel > 0 ? -Mathf.Min(1, 0.12f * accel) : Mathf.Min(1, 0.08f * -accel);
            Spring(ref pitch, ref pitchVel, pitchTarget, 7, 0.7f, dt);
            Spring(ref bodyRoll, ref bodyRollVel, Mathf.Clamp(-0.35f * (float)run.Side, -2.2f, 2.2f), 9, 0.6f, dt);
            float lookYaw = Mathf.Clamp((float)(run.Yaw * Mathf.Rad2Deg * 2), -6, 6);
            player.SetPositionAndRotation(TrackView.Place(0, (float)run.X), TrackView.Facing(0) * Quaternion.Euler(pitch, lookYaw, bodyRoll));

            foreach (var v in run.Vehicles)
            {
                if (!views.TryGetValue(v, out var go)) views[v] = go = Take(v.Type);
                go.transform.SetPositionAndRotation(TrackView.Place((float)v.Z, (float)v.X), TrackView.Facing((float)v.Z) * Quaternion.Euler(0, 180, 0)); // oncoming
                // Passing whoosh when a vehicle passes within 6 m sideways (look-spec 11.4).
                if (state == State.Playing && v.Z < 0 && v.Z > -(v.Speed + run.V) * Time.deltaTime - 0.01 && System.Math.Abs(v.X - run.X) < 6)
                {
                    whoosh.volume = Mathf.Clamp((float)(v.Speed + run.V) * 3.6f / 400, 0.2f, 1) * 0.5f;
                    whoosh.pitch = Random.Range(0.95f, 1.05f) * (v.T.IsTall ? 0.7f : 1);
                    whoosh.Play();
                }
            }
            gone.Clear();
            foreach (var kv in views) if (!run.Vehicles.Contains(kv.Key)) gone.Add(kv.Key);
            foreach (var v in gone) { Release(v, views[v]); views.Remove(v); }

            // Chase camera (look-spec 5, 11.1): distance grows with speed, lags the car sideways, rolls with side speed.
            var section = run.Track.KindAt(run.Distance);
            bool onEdgeLine = System.Math.Abs(run.X) + Road.PlayerWidth / 2 > Road.LaneCount * Road.LaneWidth / 2 && run.V > 1;
            camBack = Mathf.Lerp(camBack, Mathf.Lerp(6.5f, 7.2f, sp01), dt / 0.8f);
            camX = Mathf.SmoothDamp(camX, (float)run.X, ref camVel, 0.3f, Mathf.Infinity, dt);
            camYaw = Mathf.Lerp(camYaw, (float)(run.Yaw * Mathf.Rad2Deg), dt * 8);
            camRoll = Mathf.Lerp(camRoll, Mathf.Clamp(-0.25f * (float)run.Side, -1.8f, 1.8f), dt / 0.2f);
            fx.ShakeOffset(out var sp, out var sr);
            // Speed shake from 60 km/h, scaled by surface; edge-line rumble at 30 Hz.
            float surf = section == Section.Tunnel ? 0.8f : section == Section.Viaduct ? 1.2f : 1;
            float q = kmh < 60 ? 0 : (kmh / 100) * (kmh / 100) * surf;
            float f = (onEdgeLine ? 30 : 12 + kmh / 10) * Time.unscaledTime;
            float amp = Mathf.Min(0.03f, 0.004f * q) + (onEdgeLine ? 0.006f : 0), rot = Mathf.Min(0.35f, 0.05f * q);
            sp += new Vector3(Mathf.PerlinNoise(f, 11) - 0.5f, Mathf.PerlinNoise(f, 12) - 0.5f, 0) * 2 * amp;
            sr += new Vector2(Mathf.PerlinNoise(f, 13) - 0.5f, Mathf.PerlinNoise(f, 14) - 0.5f) * 2 * rot;
            cam.transform.position = TrackView.Place(-camBack, camX, 2.7f) + sp;
            cam.transform.LookAt(TrackView.Place(14, camX, 0.8f));
            cam.transform.Rotate(sr.x, sr.y + camYaw * 0.5f, camRoll);

            // FOV as a horizontal angle (GDD 5.1: at least 75 deg): 80 + 12 s^1.5 plus a 1.5 deg throttle kick,
            // turned into Unity's vertical FOV for this screen.
            kick = Mathf.MoveTowards(kick, input.Gas && accel > 1 && state == State.Playing ? 1.5f : 0, dt * 1.5f / (kick < 1.5f ? 0.4f : 0.6f));
            fovH = Mathf.Lerp(fovH, 80 + 12 * Mathf.Pow(sp01, 1.5f) + kick, dt / 0.5f);
            float fov = Mathf.Max(2 * Mathf.Atan(Mathf.Tan(fovH / 2 * Mathf.Deg2Rad) / cam.aspect) * Mathf.Rad2Deg, 50 + 6 * Mathf.Pow(sp01, 1.5f));
            if (state == State.Dying || state == State.Over) // FOV punch: -5 in 0.08 s, back over 0.6 s
                fov -= 5 * (dyingTime < 0.08f ? dyingTime / 0.08f : Mathf.Clamp01(1 - (dyingTime - 0.08f) / 0.6f));
            cam.fieldOfView = fov;
            if (vignette) vignette.intensity.value = Mathf.Lerp(0.18f, 0.30f, (sp01 - 0.5f) / 0.5f);

            // Speed streaks from 160 km/h (look-spec 11.3).
            var em = streaks.emission;
            em.rateOverTime = kmh > 160 && state == State.Playing ? 60 : 0;
            var main = streaks.main;
            main.startColor = new Color(1, 1, 1, 0.18f * Mathf.Clamp01((kmh - 160) / 146));
            var vel = streaks.velocityOverLifetime;
            vel.z = -(float)run.V; // toward the camera at the car's speed
            streaks.GetComponent<ParticleSystemRenderer>().lengthScale = 0.06f * (float)run.V;

            // Tunnel: exposure adapts over 0.5 s; sun and sky light fade inside; echo on.
            bool inTunnel = section == Section.Tunnel;
            sun.intensity = Mathf.MoveTowards(sun.intensity, inTunnel ? sunIntensity * 0.15f : sunIntensity, dt * sunIntensity * 2);
            RenderSettings.ambientIntensity = Mathf.MoveTowards(RenderSettings.ambientIntensity, inTunnel ? ambient * 0.35f : ambient, dt * 2);
            echo.enabled = inTunnel;

            // Haptic rumble on the edge line: 10 ms every 60 ms.
            if (onEdgeLine && state == State.Playing && (rumbleTime -= dt) <= 0) { rumbleTime = 0.06f; Feedback.Vibrate(10, 120); }

            bool moving = state == State.Playing;
            // Engine with sound-only gears: rpm 2500-7500 inside each gear (look-spec 11.4).
            int gear = 1;
            while (gear < Shifts.Length - 1 && kmh > Shifts[gear]) gear++;
            float rpm = Mathf.Lerp(2500, 7500, Mathf.InverseLerp(gear > 1 ? Shifts[gear - 1] * 0.9f : 0, Shifts[gear], kmh));
            engine.volume = moving ? (input.Gas ? 0.9f : 0.6f) * 0.3f : 0;
            engine.pitch = rpm / 4000;
            float tyreMul = section == Section.Viaduct ? 1.2f : section == Section.Tunnel ? 1.3f : 1;
            tyre.volume = moving ? 0.6f * Mathf.Pow(Mathf.Clamp01((kmh - 30) / 250), 1.2f) * tyreMul * 0.4f : 0;
            tyre.pitch = (0.8f + 0.4f * sp01) * (section == Section.Viaduct ? 1.05f : 1);
            wind.volume = moving ? 0.8f * sp01 * sp01 * (inTunnel ? 0.5f : 1) * 0.4f : 0;
            wind.pitch = 0.9f + 0.3f * sp01;
            windFilter.cutoffFrequency = 800 + 4000 * sp01;
            rumble.volume = moving && onEdgeLine ? 0.7f * 0.4f : 0;
            rumble.pitch = 0.3f * (0.8f + 0.4f * sp01);

            scrape.volume = Mathf.MoveTowards(scrape.volume, moving && run.Scraping && run.V > 0 ? 0.6f : 0, Time.unscaledDeltaTime / 0.15f);
            scrape.pitch = 0.9f + 0.4f * (float)(run.V / run.TopSpeed);
        }

        // Downloaded models that were built facing backwards (checked on screen).
        static readonly HashSet<string> Flipped = new HashSet<string> { "Player" };

        static readonly float[] Shifts = { 0, 70, 110, 150, 195, 240, 285, 400 }; // sound-only gear tops, km/h

        // Damped spring toward target (w rad/s, damping ratio z).
        static void Spring(ref float x, ref float v, float target, float w, float z, float dt)
        {
            v += (w * w * (target - x) - 2 * z * w * v) * dt;
            x += v * dt;
        }

        // Thin streaks that rush past the camera at speed, in a ring 6-14 m off the view axis (look-spec 11.3).
        static ParticleSystem Streaks(Transform cam, Material mat)
        {
            var ps = new GameObject("Streaks").AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.transform.SetParent(cam, false);
            ps.transform.localPosition = new Vector3(0, 0, 40);
            var main = ps.main;
            main.maxParticles = 40; main.startLifetime = 0.5f; main.startSize = 0.05f; main.startSpeed = 0;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 14; shape.radiusThickness = 0.57f;
            var vel = ps.velocityOverLifetime;
            vel.enabled = true; vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = vel.y = vel.z = 0;
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0; r.sharedMaterial = mat;
            ps.Play();
            return ps;
        }

        GameObject Take(int type)
        {
            var pool = pools[type];
            if (pool.Count > 0)
            {
                var g = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
                g.SetActive(true);
                return g;
            }
            var t = Road.Types[type];
            var model = type < art.vehicles.Length ? art.vehicles[type] : null;
            var go = model ? Placeholder.Fit(model, (float)t.Length, t.Name, Placeholder.TrafficPaint(t.Name == "Van"), Flipped.Contains(t.Name))
                : Placeholder.Vehicle(t.Name, (float)t.Length, (float)t.Width, (float)t.Height,
                    t.Name == "Bus" ? Placeholder.Mat(Placeholder.Hex(0xE9E9E7), 0.1f, 0.75f) : Placeholder.TrafficPaint(t.Name == "Van"),
                    t.Name == "Truck" ? Placeholder.Trailer : null);
            return go;
        }

        void Release(Vehicle v, GameObject go)
        {
            go.SetActive(false);
            pools[v.Type].Add(go);
        }
    }
}
