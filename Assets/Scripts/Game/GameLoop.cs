using System.Collections.Generic;
using Geisterfahrer.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Geisterfahrer.Game
{
    // Owns the run, syncs views, chase camera, HUD and panels. The scene only holds this component.
    public class GameLoop : MonoBehaviour
    {
        public Tuning tuning = new Tuning();
        [Tooltip("Optional models by vehicle type index (Compact, Sedan, Van, Bus, Truck). Empty = placeholder.")]
        public GameObject[] vehiclePrefabs = new GameObject[0];
        public GameObject playerPrefab;
        public Material baseMaterial;

        enum State { Start, Playing, Paused, Dying, Over }
        State state = State.Start;
        Run run;
        DriveInput input;
        RoadView road;
        Camera cam;
        Transform player;
        AudioSource engine, scrape, crash;
        readonly Dictionary<Vehicle, GameObject> views = new Dictionary<Vehicle, GameObject>();
        readonly List<Vehicle> gone = new List<Vehicle>();
        readonly List<GameObject>[] pools = new List<GameObject>[Road.Types.Length];
        float camX, shake, dyingTime;
        int lastHits;
        bool newBestSpeed, newBestDist;
        int seed;

        static readonly Color Sky = new Color(0.72f, 0.8f, 0.88f);

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            input = new DriveInput();
            Placeholder.BaseMat = baseMaterial;

            cam = new GameObject("Camera").AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Sky;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 600;
            cam.gameObject.AddComponent<AudioListener>();

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Hard;
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Sky;

            road = new GameObject("Road").AddComponent<RoadView>();
            player = playerPrefab ? Instantiate(playerPrefab).transform
                : Placeholder.Vehicle("Player", (float)Road.PlayerLength, (float)Road.PlayerWidth, 1.3f, new Color(0.24f, 0.48f, 1f)).transform;
            for (int i = 0; i < pools.Length; i++) pools[i] = new List<GameObject>();

            engine = Audio(Sounds.Engine(), true);
            scrape = Audio(Sounds.Noise("scrape", 1, false), true);
            crash = Audio(Sounds.Noise("crash", 0.8f, true), false);
            scrape.volume = 0;
            NewRun();
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
            camX = 0;
            Sync(0);
        }

        void Update()
        {
            var k = Keyboard.current;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            // Outside a run the whole screen is UI, so the finger that taps Play/Retry does not steer or accelerate.
            input.Update(state == State.Playing ? PauseRect : new Rect(0, 0, Screen.width, Screen.height));
            if (state == State.Playing)
            {
                if (k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame)) state = State.Paused;
                run.Step(dt, input.Steer, input.Gas);
                if (run.Hits > lastHits) { shake = Mathf.Max(shake, (float)run.LastImpulse * 0.06f); lastHits = run.Hits; }
                if (run.Crashed) { state = State.Dying; dyingTime = 0.7f; shake = 0.6f; crash.Play(); }
            }
            else if (state == State.Paused && k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame)) state = State.Playing;
            else if (state == State.Dying && (dyingTime -= dt) <= 0) EndRun();
            else if ((state == State.Start || state == State.Over) && k != null && k.enterKey.wasPressedThisFrame) Play();
            Sync(dt);
        }

        void Play()
        {
            if (state == State.Over) NewRun();
            state = State.Playing;
        }

        void EndRun()
        {
            state = State.Over;
            float speed = (float)(run.MaxSpeed * 3.6), dist = (float)run.Distance;
            newBestSpeed = speed > PlayerPrefs.GetFloat("bestSpeed", 0);
            newBestDist = dist > PlayerPrefs.GetFloat("bestDist", 0);
            if (newBestSpeed) PlayerPrefs.SetFloat("bestSpeed", speed);
            if (newBestDist) PlayerPrefs.SetFloat("bestDist", dist);
            PlayerPrefs.Save();
        }

        void Sync(float dt)
        {
            road.Scroll(run.Distance);
            RenderSettings.fogStartDistance = 120;
            RenderSettings.fogEndDistance = (float)run.SpawnDistance + 20;

            float lookYaw = Mathf.Clamp((float)(run.Yaw * Mathf.Rad2Deg * 2), -6, 6);
            float roll = Mathf.Clamp((float)(-run.Side / 7 * 3), -3, 3);
            player.SetPositionAndRotation(new Vector3((float)run.X, 0, 0), Quaternion.Euler(0, lookYaw, roll));

            foreach (var v in run.Vehicles)
            {
                if (!views.TryGetValue(v, out var go)) views[v] = go = Take(v.Type);
                go.transform.position = new Vector3((float)v.X, 0, (float)v.Z);
            }
            gone.Clear();
            foreach (var kv in views) if (!run.Vehicles.Contains(kv.Key)) gone.Add(kv.Key);
            foreach (var v in gone) { Release(v, views[v]); views.Remove(v); }

            // Chase camera: behind and above, follows x with a small delay, FOV widens with speed.
            camX += ((float)run.X - camX) * (1 - Mathf.Exp(-dt / 0.15f));
            shake = Mathf.Max(0, shake - dt * 1.5f);
            var jitter = Random.insideUnitSphere * shake * 0.3f;
            cam.transform.position = new Vector3(camX, 2.7f, -6.5f) + jitter;
            cam.transform.LookAt(new Vector3(camX, 1.0f, 12) + jitter);
            cam.fieldOfView = Mathf.Lerp(58, 72, (float)(run.V / run.TopSpeed));

            bool moving = state == State.Playing;
            engine.volume = moving ? 0.25f : 0;
            engine.pitch = 0.6f + (float)(run.V / run.TopSpeed) * 2.4f;
            bool touching = moving && run.V > 0 && Mathf.Abs((float)run.X) + 1.2f > (float)Road.RailX;
            scrape.volume = Mathf.MoveTowards(scrape.volume, touching ? 0.3f : 0, dt * 3);
        }

        GameObject Take(int type)
        {
            var pool = pools[type];
            GameObject go;
            if (pool.Count > 0) { go = pool[pool.Count - 1]; pool.RemoveAt(pool.Count - 1); go.SetActive(true); return go; }
            var t = Road.Types[type];
            if (type < vehiclePrefabs.Length && vehiclePrefabs[type]) go = Instantiate(vehiclePrefabs[type]);
            else go = Placeholder.Vehicle(t.Name, (float)t.Length, (float)t.Width, (float)t.Height, Colors[type]);
            go.transform.rotation = Quaternion.Euler(0, 180, 0); // oncoming: front faces the player
            return go;
        }

        void Release(Vehicle v, GameObject go)
        {
            go.SetActive(false);
            pools[v.Type].Add(go);
        }

        static readonly Color[] Colors =
        {
            new Color(0.91f, 0.89f, 0.85f), new Color(0.2f, 0.22f, 0.26f), new Color(0.95f, 0.95f, 0.95f),
            new Color(0.95f, 0.7f, 0.24f), new Color(0.6f, 0.15f, 0.15f),
        };

        // ---- HUD and panels (IMGUI placeholder until the designer's UI) ----

        float U => Screen.height / 20f;
        Rect PauseRect => state == State.Playing || state == State.Paused ? new Rect(Screen.width - U * 2.6f, U * 0.6f, U * 2, U * 2) : new Rect();

        GUIStyle Style(float size, TextAnchor a = TextAnchor.MiddleLeft)
        {
            var s = new GUIStyle(GUI.skin.label) { fontSize = (int)(U * size), alignment = a, fontStyle = FontStyle.Bold };
            s.normal.textColor = Color.white;
            return s;
        }

        void Shadow(Rect r, string text, GUIStyle s)
        {
            var c = s.normal.textColor;
            s.normal.textColor = new Color(0, 0, 0, 0.6f);
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, s);
            s.normal.textColor = c;
            GUI.Label(r, text, s);
        }

        void OnGUI()
        {
            float w = Screen.width, h = Screen.height, u = U;
            var button = new GUIStyle(GUI.skin.button) { fontSize = (int)(u * 1.1f), fontStyle = FontStyle.Bold };

            if (state == State.Playing || state == State.Paused || state == State.Dying)
            {
                Shadow(new Rect(u, u * 0.5f, w / 2, u * 2.4f), $"{run.V * 3.6:F0} km/h", Style(2));
                Shadow(new Rect(u, u * 2.8f, w / 2, u * 1.4f), $"{run.Distance / 1000:F2} km", Style(1.1f));
                if (state != State.Dying && GUI.Button(PauseRect, state == State.Paused ? "▶" : "II", button))
                    state = state == State.Paused ? State.Playing : State.Paused;
                if (input.SteerOrigin is Vector2 o && state == State.Playing)
                    GUI.Box(new Rect(o.x - u, h - o.y - u, u * 2, u * 2), GUIContent.none);
            }

            if (state == State.Start || state == State.Over)
            {
                GUI.color = new Color(0, 0, 0, 0.55f);
                GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
                GUI.color = Color.white;
                var center = Style(1.2f, TextAnchor.MiddleCenter);
                float bestSpeed = PlayerPrefs.GetFloat("bestSpeed", 0), bestDist = PlayerPrefs.GetFloat("bestDist", 0);
                if (state == State.Start)
                {
                    GUI.Label(new Rect(0, h * 0.18f, w, u * 3), "GEISTERFAHRER", Style(2.6f, TextAnchor.MiddleCenter));
                    GUI.Label(new Rect(0, h * 0.4f, w, u * 1.5f), $"{bestSpeed:F1} km/h    {bestDist / 1000:F2} km", center);
                }
                else
                {
                    GUI.Label(new Rect(0, h * 0.14f, w, u * 2.5f), $"{run.MaxSpeed * 3.6:F1} km/h    {run.Distance / 1000:F2} km", Style(2, TextAnchor.MiddleCenter));
                    string bs = newBestSpeed ? "  New best" : "", bd = newBestDist ? "  New best" : "";
                    GUI.Label(new Rect(0, h * 0.34f, w, u * 1.4f), $"{bestSpeed:F1} km/h{bs}", center);
                    GUI.Label(new Rect(0, h * 0.42f, w, u * 1.4f), $"{bestDist / 1000:F2} km{bd}", center);
                }
                if (GUI.Button(new Rect(w / 2 - u * 4, h * 0.62f, u * 8, u * 3), state == State.Start ? "Play" : "Retry", button)) Play();
            }
        }
    }
}
