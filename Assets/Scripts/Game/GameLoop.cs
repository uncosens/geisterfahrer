using System.Collections.Generic;
using Geisterfahrer.Core;
using UnityEngine;
using UnityEngine.InputSystem;
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
        RoadView road;
        Hud hud;
        Feedback fx;
        Camera cam;
        Transform player;
        AudioSource engine, scrape, crash, hit;
        readonly Dictionary<Vehicle, GameObject> views = new Dictionary<Vehicle, GameObject>();
        readonly List<Vehicle> gone = new List<Vehicle>();
        readonly List<GameObject>[] pools = new List<GameObject>[Road.Types.Length];
        float camX, camVel, dyingTime;
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
            if (!FindAnyObjectByType<Light>()) // the scene normally has the sun (look-spec 1)
            {
                var sun = new GameObject("Sun").AddComponent<Light>();
                sun.type = LightType.Directional; sun.shadows = LightShadows.Hard;
                sun.transform.rotation = Quaternion.Euler(40, 30, 0);
            }

            road = new GameObject("Road").AddComponent<RoadView>();
            road.Init(art);
            float pl = (float)Road.PlayerLength;
            player = (art.player ? Placeholder.Fit(art.player, pl, "Player")
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
            scrape.volume = 0;
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
            lastHits = 0;
            camX = (float)run.X;
            Time.timeScale = 1;
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
                if (dyingTime > 1.0f) EndRun();
            }
            else if ((state == State.Start || state == State.Over) && k != null && k.enterKey.wasPressedThisFrame) hud.Play();

            Sync();
            hud.SetTouch(state == State.Playing ? input.SteerOrigin : null, input.SteerX);
            hud.SetRun(run.V * 3.6, run.Distance, run.TopSpeed * 3.6);
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
            var point = RoadView.Place((float)run.HitZ, (float)run.HitX, 0.5f);
            float side = (float)run.HitNx;
            if (run.Crashed)
            {
                state = State.Dying; dyingTime = 0; Time.timeScale = 0.25f;
                fx.Crash(point);
                hud.Flash(-1, 0.55f, 0.15f, 0.6f);
                crash.Play();
                return;
            }
            if (run.Hits > lastHits)
            {
                lastHits = run.Hits;
                float j = (float)run.LastImpulse;
                fx.Hit(point, side, j);
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
            float spawn = (float)run.SpawnDistance;
            RenderSettings.fogStartDistance = 0.45f * spawn;
            RenderSettings.fogEndDistance = spawn;
            cam.farClipPlane = spawn + 30;

            float lookYaw = Mathf.Clamp((float)(run.Yaw * Mathf.Rad2Deg * 2), -6, 6);
            float roll = Mathf.Clamp((float)(-run.Side / 7 * 3), -3, 3);
            player.SetPositionAndRotation(RoadView.Place(0, (float)run.X), Quaternion.Euler(0, lookYaw, roll));

            foreach (var v in run.Vehicles)
            {
                if (!views.TryGetValue(v, out var go)) views[v] = go = Take(v.Type);
                go.transform.position = RoadView.Place((float)v.Z, (float)v.X);
            }
            gone.Clear();
            foreach (var kv in views) if (!run.Vehicles.Contains(kv.Key)) gone.Add(kv.Key);
            foreach (var v in gone) { Release(v, views[v]); views.Remove(v); }

            // Chase camera (look-spec 5): 6.5 m behind, 2.7 m up, looks 14 m ahead; FOV 58 at 100 km/h to 66 at the ceiling.
            camX = Mathf.SmoothDamp(camX, (float)run.X, ref camVel, 0.15f, Mathf.Infinity, Time.unscaledDeltaTime);
            fx.ShakeOffset(out var sp, out var sr);
            cam.transform.position = RoadView.Place(-6.5f, camX, 2.7f) + sp;
            cam.transform.LookAt(RoadView.Place(14, camX, 0.8f));
            cam.transform.Rotate(sr.x, sr.y, 0);
            float fov = Mathf.LerpUnclamped(58, 66, (float)((run.V - 100 / 3.6) / (run.TopSpeed - 100 / 3.6)));
            if (state == State.Dying || state == State.Over) // FOV punch: -5 in 0.08 s, back over 0.6 s
                fov -= 5 * (dyingTime < 0.08f ? dyingTime / 0.08f : Mathf.Clamp01(1 - (dyingTime - 0.08f) / 0.6f));
            cam.fieldOfView = fov;

            bool moving = state == State.Playing;
            engine.volume = moving ? 0.25f : 0;
            engine.pitch = 0.6f + (float)(run.V / run.TopSpeed) * 2.4f;
            scrape.volume = Mathf.MoveTowards(scrape.volume, moving && run.Scraping && run.V > 0 ? 0.6f : 0, Time.unscaledDeltaTime / 0.15f);
            scrape.pitch = 0.9f + 0.4f * (float)(run.V / run.TopSpeed);
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
            var go = model ? Placeholder.Fit(model, (float)t.Length, t.Name)
                : Placeholder.Vehicle(t.Name, (float)t.Length, (float)t.Width, (float)t.Height,
                    t.Name == "Bus" ? Placeholder.Mat(Placeholder.Hex(0xE9E9E7), 0.1f, 0.75f) : Placeholder.TrafficPaint(t.Name == "Van"),
                    t.Name == "Truck" ? Placeholder.Trailer : null);
            go.transform.rotation = Quaternion.Euler(0, 180, 0); // oncoming: front faces the player
            return go;
        }

        void Release(Vehicle v, GameObject go)
        {
            go.SetActive(false);
            pools[v.Type].Add(go);
        }
    }
}
