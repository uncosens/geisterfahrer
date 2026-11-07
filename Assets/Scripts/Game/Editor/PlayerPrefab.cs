using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Geisterfahrer.Editor
{
    // The 991 model has no wheel nodes: tyres, rims and brakes are merged by material. This builds a prefab from the GLB
    // with four "Wheel_n" nodes (the triangles around each hub), so wheel spin and steering find them by name.
    // Batch: Unity.exe -batchmode -quit -projectPath . -executeMethod Geisterfahrer.Editor.PlayerPrefab.Dump
    public static class PlayerPrefab
    {
        const string Glb = "Assets/ThirdParty/Vehicles/Player.glb", Dir = "Assets/ThirdParty/Generated/Player", Prefab = "Assets/ThirdParty/Generated/Player.prefab";
        // Materials of wheel parts in this model, and the hub layout in model units (world axes after the root matrix: x across, y up, z along).
        static readonly string[] WheelMats = { "T1Mtl", "Ri1Mtl", "R1Mtl", "R5Mtl", "Sw1Mtl" }; // calipers (Ca1Mtl) stay on the body
        const float HubY = 2.40f, HubZFront = 4.77f, HubZRear = -4.89f, HubXLeft = -3.65f, HubXRight = 2.41f, Reach = 1.35f, Side = 1.0f;

        public static GameObject Build()
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(Glb);
            if (!src || src.GetComponentsInChildren<Renderer>().All(r => r.sharedMaterials.All(m => !m || m.name != "T1Mtl"))) return null; // not the 991 file
            if (Directory.Exists(Dir)) AssetDatabase.DeleteAsset(Dir);
            if (!AssetDatabase.IsValidFolder("Assets/ThirdParty/Generated")) AssetDatabase.CreateFolder("Assets/ThirdParty", "Generated");
            AssetDatabase.CreateFolder("Assets/ThirdParty/Generated", "Player");
            var inst = Object.Instantiate(src);
            inst.name = "Player";
            var hubs = new[] { new Vector3(HubXLeft, HubY, HubZFront), new Vector3(HubXRight, HubY, HubZFront), new Vector3(HubXLeft, HubY, HubZRear), new Vector3(HubXRight, HubY, HubZRear) };
            var wheels = new Transform[4];
            for (int k = 0; k < 4; k++)
            {
                wheels[k] = new GameObject("Wheel_" + (k < 2 ? "F" : "R") + (k % 2 == 0 ? "L" : "R")).transform;
                wheels[k].SetParent(inst.transform, false);
                wheels[k].position = hubs[k];
            }
            int n = 0;
            foreach (var r in inst.GetComponentsInChildren<MeshRenderer>().ToArray())
            {
                var mf = r.GetComponent<MeshFilter>();
                if (!mf || !mf.sharedMesh || r.sharedMaterials.Length != 1 || !WheelMats.Contains(r.sharedMaterial.name)) continue;
                var m = mf.sharedMesh;
                var v = m.vertices; var nr = m.normals; var uv = m.uv; var t = m.triangles;
                var owner = new int[t.Length / 3];
                for (int i = 0; i < owner.Length; i++) owner[i] = Hub((r.transform.TransformPoint(v[t[3 * i]]) + r.transform.TransformPoint(v[t[3 * i + 1]]) + r.transform.TransformPoint(v[t[3 * i + 2]])) / 3, hubs);
                for (int k = -1; k < 4; k++)
                {
                    var tris = Enumerable.Range(0, owner.Length).Where(i => owner[i] == k).ToList();
                    if (tris.Count == 0) continue;
                    var frame = k < 0 ? r.transform : wheels[k];
                    var map = new Dictionary<int, int>(); var nv = new List<Vector3>(); var nn = new List<Vector3>(); var nu = new List<Vector2>(); var nt = new List<int>();
                    foreach (int i in tris)
                        for (int c = 0; c < 3; c++)
                        {
                            int o = t[3 * i + c];
                            if (!map.TryGetValue(o, out int j))
                            {
                                map[o] = j = nv.Count;
                                nv.Add(frame.InverseTransformPoint(r.transform.TransformPoint(v[o])));
                                if (nr.Length > 0) nn.Add(frame.InverseTransformDirection(r.transform.TransformDirection(nr[o])));
                                if (uv.Length > 0) nu.Add(uv[o]);
                            }
                            nt.Add(j);
                        }
                    var mesh = new Mesh { name = $"{r.name}_{k}", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                    mesh.SetVertices(nv); if (nn.Count > 0) mesh.SetNormals(nn); if (nu.Count > 0) mesh.SetUVs(0, nu); mesh.SetTriangles(nt, 0);
                    mesh.RecalculateBounds();
                    AssetDatabase.CreateAsset(mesh, $"{Dir}/m{n++}.asset");
                    if (k < 0) { mf.sharedMesh = mesh; continue; }
                    var go = new GameObject(r.name + "_part", typeof(MeshFilter), typeof(MeshRenderer));
                    go.transform.SetParent(wheels[k], false);
                    go.GetComponent<MeshFilter>().sharedMesh = mesh;
                    go.GetComponent<MeshRenderer>().sharedMaterial = r.sharedMaterial;
                }
                if (owner.All(o => o >= 0)) Object.DestroyImmediate(r.gameObject); // nothing left in the original
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(inst, Prefab);
            Object.DestroyImmediate(inst);
            return prefab;
        }

        // Index of the wheel whose hub is within reach (round the wheel axis, along x), or -1.
        static int Hub(Vector3 p, Vector3[] hubs)
        {
            for (int k = 0; k < hubs.Length; k++)
                if (Mathf.Abs(p.x - hubs[k].x) <= Side && new Vector2(p.y - hubs[k].y, p.z - hubs[k].z).magnitude <= Reach) return k;
            return -1;
        }

        // Sets the player model in the saved scene without rebuilding everything else (Setup does it as part of its run).
        public static void Apply()
        {
            var prefab = Build();
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
            var loop = Object.FindAnyObjectByType<Game.GameLoop>();
            loop.art.player = prefab ? prefab : loop.art.player;
            EditorUtility.SetDirty(loop);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log("PLAYER applied " + loop.art.player);
        }

        public static void Dump()
        {
            var prefab = Build();
            Debug.Log("PLAYER prefab " + prefab);
            if (!prefab) return;
            var inst = Object.Instantiate(prefab);
            foreach (var r in inst.GetComponentsInChildren<Renderer>())
            {
                var m = r.GetComponent<MeshFilter>().sharedMesh;
                Debug.Log($"PLAYER {r.transform.parent.name}/{r.name} mat {r.sharedMaterial.name} verts {m.vertexCount} bounds {r.bounds.center:F2} size {r.bounds.size:F2}");
            }
        }
    }
}
