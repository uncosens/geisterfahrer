using System.IO;
using Geisterfahrer.Core;
using Geisterfahrer.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Geisterfahrer.Editor
{
    // Renders the generated road at a curve, crest, tunnel, viaduct and overpass, to check the view without driving there.
    // Batch: Unity.exe -batchmode -quit -projectPath . -executeMethod Geisterfahrer.Editor.TrackShots.Render
    public static class TrackShots
    {
        public static void Render()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
            var art = Object.FindAnyObjectByType<GameLoop>().art;
            Placeholder.BaseMat = art.baseLit;
            Track track = null;
            for (int seed = 1; seed < 40; seed++) // the first seed with an underground section in 30 km
            {
                track = new Track(seed);
                track.Ensure(30000, 3); // phase 4 rules: all structures on
                if (track.Structures.Exists(x => x.kind == Section.Underground)) break;
            }
            foreach (var st in track.Structures) Debug.Log($"SHOT structure {st.kind} {st.s0:F0}-{st.s1:F0}");
            var view = new GameObject("Road").AddComponent<TrackView>();
            view.Init(art, track);
            var cam = new GameObject("Cam").AddComponent<Camera>();
            cam.clearFlags = RenderSettings.skybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            cam.fieldOfView = 55; cam.farClipPlane = 1650;
            RenderSettings.fogStartDistance = 300; RenderSettings.fogEndDistance = 1600;
            var rt = new RenderTexture(1600, 720, 24);
            cam.targetTexture = rt;
            Directory.CreateDirectory("docs/dev/shots");

            void Shot(string name, double s, float up = 2.7f)
            {
                Debug.Log($"SHOT {name}: kind {track.KindAt(s)} y {track.Y[(int)(s / 2)]:F1} grade {track.Grade(s):F3} curv {track.Curvature(s):F5}");
                view.Scroll(s);
                TrackView.S = s;
                cam.transform.position = TrackView.Place(-6.5f, 0, up);
                cam.transform.LookAt(TrackView.Place(14, 0, 0.8f));
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                File.WriteAllBytes($"docs/dev/shots/track-{name}.png", tex.EncodeToPNG());
                Debug.Log($"SHOT {name} at {s:F0} m");
            }

            double Find(System.Func<double, bool> test, double from = 1000)
            {
                for (double s = from; s < 29000; s += 10) if (test(s)) return s;
                return -1;
            }

            double curve = Find(s => System.Math.Abs(track.Curvature(s)) > 1 / 1000.0);
            if (curve > 0) { Shot("curve", curve - 250); Shot("curve", curve - 250); } // first render in batch mode comes out wrong; render twice
            double crest = Find(s => track.Grade(s) > 0.015 && track.Grade(s + 600) < -0.01);
            if (crest > 0) Shot("hill", crest);
            bool m = false, u = false;
            foreach (var st in track.Structures)
            {
                if (st.kind == Section.Tunnel && !m)
                {
                    m = true;
                    Shot("tunnel-400m", st.s0 - 400); Shot("tunnel-400m", st.s0 - 400);
                    Shot("tunnel-sign", st.s0 - 190); Shot("tunnel-cutting", st.s0 - 80); Shot("tunnel-portal", st.s0 - 25);
                    Shot("tunnel-inside", st.s0 + 150); Shot("tunnel-exit", st.s1 - 60); Shot("tunnel-after", st.s1 + 60);
                }
                if (st.kind == Section.Underground && !u)
                {
                    u = true;
                    double roof = st.s0; while (track.Depth(roof) < Track.UgRoof) roof += 4;
                    Shot("ug-400m", st.s0 - 400); Shot("ug-400m", st.s0 - 400);
                    Shot("ug-sign", st.s0 - 130); Shot("ug-ramp", st.s0 + 150); Shot("ug-mouth", roof - 40);
                    Shot("ug-inside", roof + 100); Shot("ug-exit", st.s1 - 60);
                }
            }
            foreach (var st in track.Structures)
                if (st.kind == Section.Viaduct) { Shot("viaduct", st.s0 + 100); break; }
            if (track.Overpasses.Count > 0) Shot("overpass", track.Overpasses[0] - 120);
        }
    }
}
