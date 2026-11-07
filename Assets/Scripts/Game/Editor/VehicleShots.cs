using System.IO;
using Geisterfahrer.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Geisterfahrer.Editor
{
    // Renders cars on the road to docs/dev/shots/veh-*.png (with and without VehicleFx).
    // Batch: Unity.exe -batchmode -quit -projectPath . -executeMethod Geisterfahrer.Editor.VehicleShots.Render
    public static class VehicleShots
    {
        public static void Render()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
            var art = Object.FindAnyObjectByType<Game.GameLoop>().art;
            Game.Placeholder.BaseMat = art.baseLit;
            var track = new Track(1);
            track.Ensure(3000, 3);
            var view = new GameObject("Road").AddComponent<Game.TrackView>();
            view.Init(art, track);
            Game.VehicleFx.Init(art.spark);
            var cam = new GameObject("Cam").AddComponent<Camera>();
            cam.clearFlags = RenderSettings.skybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            cam.fieldOfView = 50; cam.farClipPlane = 1650;
            RenderSettings.fogStartDistance = 300; RenderSettings.fogEndDistance = 1600;
            var rt = new RenderTexture(1600, 720, 24);
            cam.targetTexture = rt;
            Directory.CreateDirectory("docs/dev/shots");
            view.Scroll(100); Game.TrackView.S = 100;

            int lastPaint = -1;
            var cars = new System.Collections.Generic.List<GameObject>();
            void Car(int type, float z, float x, bool oncoming)
            {
                var t = type < 0 ? null : Road.Types[type];
                var go = type < 0 ? Game.Placeholder.Fit(art.player, (float)Road.PlayerLength, "Player", false, false, Game.GameLoop.PlayerPaintMaterial)
                    : Game.Placeholder.Fit(art.vehicles[type].list[0], (float)t.Length, t.Name, false, t.Name != "Bus" && t.Name != "Truck");
                go.GetComponent<Game.Paintable>().Paint(type < 0 ? Game.Placeholder.PlayerRed : Game.Placeholder.RandomPaint(ref lastPaint));
                go.transform.SetPositionAndRotation(Game.TrackView.Place(z, x), Game.TrackView.Facing(z) * Quaternion.Euler(0, oncoming ? 180 : 0, 0));
                cars.Add(go);
            }
            Car(-1, 0, 0, false); Car(1, 14, -3.5f, true); Car(0, 26, 3.5f, true); Car(4, 45, 0, true); Car(2, 60, -3.5f, true);
            Shoot(cam, rt, "veh-before"); Shoot(cam, rt, "veh-before");
            foreach (var c in cars) Game.VehicleFx.Attach(c, c.name == "Player");
            Shoot(cam, rt, "veh-after"); Shoot(cam, rt, "veh-after");
            // close-up with wheels turned
            foreach (var c in cars) { var fx = c.GetComponent<Game.VehicleFx>(); fx.Drive(8, 1, c.name == "Player" ? 20 : 0); }
            cam.transform.position = Game.TrackView.Place(-3.5f, 2.5f, 1.2f); cam.transform.LookAt(Game.TrackView.Place(0, 0, 0.5f));
            Shoot(cam, rt, "veh-wheels", false);
            foreach (var c in cars) c.GetComponent<Game.VehicleFx>().Drive(30, 1, 0);
            Shoot(cam, rt, "veh-blur", false);
            cars[0].GetComponent<Game.VehicleFx>().Scratch(1, 0.4f);
            Shoot(cam, rt, "veh-scratch", false);
        }

        static void Shoot(Camera cam, RenderTexture rt, string name, bool chase = true)
        {
            if (chase)
            {
                cam.transform.position = Game.TrackView.Place(-6.5f, 0, 2.7f);
                cam.transform.LookAt(Game.TrackView.Place(14, 0, 0.8f));
            }
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            File.WriteAllBytes($"docs/dev/shots/{name}.png", tex.EncodeToPNG());
        }
    }
}
