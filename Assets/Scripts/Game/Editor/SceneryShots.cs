using System.IO;
using Geisterfahrer.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Geisterfahrer.Editor
{
    // Renders the roadside scenery per zone to docs/dev/shots/city-*.png, plus a before/after pair at speed.
    // Batch: Unity.exe -batchmode -quit -projectPath . -executeMethod Geisterfahrer.Editor.SceneryShots.Render
    public static class SceneryShots
    {
        public static void Render()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
            var art = Object.FindAnyObjectByType<Game.GameLoop>().art;
            Game.Placeholder.BaseMat = art.baseLit;
            var track = new Track(3);
            track.Ensure(14000, 3);
            var view = new GameObject("Road").AddComponent<Game.TrackView>();
            view.Init(art, track);
            Game.TrackView.SceneryPerFrame = 200;
            var cam = new GameObject("Cam").AddComponent<Camera>();
            cam.clearFlags = RenderSettings.skybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            cam.fieldOfView = 60; cam.farClipPlane = 1650;
            RenderSettings.fogStartDistance = 300; RenderSettings.fogEndDistance = 1600;
            var rt = new RenderTexture(1600, 720, 24); cam.targetTexture = rt;
            Directory.CreateDirectory("docs/dev/shots");

            void Shot(string name, double s, bool scenery = true, float up = 2.7f)
            {
                Game.TrackView.SceneryOn = scenery; Game.TrackView.SceneryTriangles = 0;
                view.Scroll(s); view.Scroll(s);
                Game.TrackView.S = s;
                cam.transform.position = Game.TrackView.Place(-6.5f, 0, up);
                cam.transform.LookAt(Game.TrackView.Place(14, 0, 0.8f));
                cam.Render(); cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                File.WriteAllBytes($"docs/dev/shots/city-{name}.png", tex.EncodeToPNG());
                int tris = 0, calls = 0; // the scenery in view: meshes named Mass, Detail, Sky (one material each = one draw call each)
                foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
                    if ((mf.name == "Mass" || mf.name == "Detail" || mf.name == "Sky" || mf.name == "Trees" || mf.name == "Props") && mf.gameObject.activeInHierarchy) { tris += mf.sharedMesh.triangles.Length / 3; if (mf.sharedMesh.triangles.Length > 0) calls++; }
                Debug.Log($"CITY {name} at {s:F0} zone {track.Zones.At(s + 100)} scenery tris {tris} calls {calls}");
            }

            foreach (Zone z in System.Enum.GetValues(typeof(Zone)))
                for (double s = 1600; s < 13000; s += 25)
                    if (track.Zones.At(s) == z && track.Zones.At(s + 400) == z && track.Zones.At(s - 100) == z && track.KindAt(s + 100) == Section.Open && track.KindAt(s + 300) == Section.Open)
                    { Shot(z.ToString().ToLower(), s + 100); if (z == Zone.City) { Shot("city-before", s + 100, false); } break; }

            // Quality ladder: the same city view at ladder steps 0, 5 and 10 (triangles and draw calls in the log).
            foreach (int lv in new[] { 0, 5, 10 }) { Game.TrackView.QualityLevel = lv; Shot("ladder" + lv, 6625 + 100); }
            Game.TrackView.QualityLevel = 0;
            // A zone border with the skyline rising and a gantry: the first border into the city.
            for (double s = 1600; s < 13000; s += 25)
                if (track.Zones.At(s - 30) != Zone.City && track.Zones.At(s + 30) == Zone.City && track.KindAt(s) == Section.Open) { Shot("gateway", s - 80); Shot("gateway-in", s + 250); break; }
            // Structures: the first seed that has each kind.
            foreach (var (kind, label, offs) in new[] { (Section.Interchange, "interchange", new[] { -60.0, 100, 400 }), (Section.Trench, "trench", new[] { -150.0, 300, 450 }), (Section.Bridge, "bridge", new[] { -100.0, 60, 150 }), (Section.Junction, "junction", new[] { -60.0, 600, 900 }), (Section.Crossing, "crossing", new[] { -80.0, 20, 60 }) })
            {
                Track t2 = null; (double s0, double s1, Section kind) found = default;
                for (int seed = 1; seed < 80 && t2 == null; seed++)
                {
                    var tt = new Track(seed); tt.Ensure(40000, 3);
                    foreach (var st in tt.Structures) if (st.kind == kind) { t2 = tt; found = st; break; }
                }
                if (t2 == null) { Debug.Log("CITY no " + label); continue; }
                track = t2; view.Init(art, track);
                int i = 0;
                foreach (double o in offs) Shot(label + (++i), found.s0 + o);
            }
        }
    }
}
