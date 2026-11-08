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
                Debug.Log($"CITY {name} at {s:F0} zone {track.Zones.At(s + 100)} scenery tris {Game.TrackView.SceneryTriangles}");
            }

            foreach (Zone z in System.Enum.GetValues(typeof(Zone)))
                for (double s = 1600; s < 13000; s += 25)
                    if (track.Zones.At(s) == z && track.Zones.At(s + 400) == z && track.Zones.At(s - 100) == z && track.KindAt(s + 100) == Section.Open && track.KindAt(s + 300) == Section.Open)
                    { Shot(z.ToString().ToLower(), s + 100); if (z == Zone.City) { Shot("city-before", s + 100, false); } break; }
        }
    }
}
