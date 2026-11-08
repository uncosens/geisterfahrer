using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Geisterfahrer.Editor
{
    // Turns a downloaded GLB into the prefab the game uses: hidden parts (interior, shadow quads) removed and, for models over the
    // triangle budget, every mesh thinned by vertex clustering. Output is in the git-ignored Assets/ThirdParty/Generated.
    public static class VehiclePrefabs
    {
        const string Src = "Assets/ThirdParty/Vehicles", Gen = "Assets/ThirdParty/Generated";
        static readonly Dictionary<string, GameObject> made = new Dictionary<string, GameObject>();

        public static void Reset()
        {
            made.Clear();
            if (Directory.Exists(Gen)) AssetDatabase.DeleteAsset(Gen);
        }

        public static GameObject Build(Game.Part p)
        {
            string name = p.File.Replace('/', '_') + "_" + p.Reduce + (p.Drop != null ? "_d" : "");
            if (made.TryGetValue(name, out var done)) return done;
            var src = AssetDatabase.LoadAssetAtPath<GameObject>($"{Src}/{p.File}.glb");
            if (!src) return null;
            Directory.CreateDirectory(Gen);
            var hidden = Game.VehicleRecipes.Hidden.Concat((p.Drop ?? "").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)).ToArray();
            var inst = Object.Instantiate(src);
            foreach (var r in inst.GetComponentsInChildren<Renderer>())
            {
                var ms = r.sharedMaterials;
                if (ms.Length > 0 && ms.All(m => m && hidden.Any(h => m.name.ToLowerInvariant().Contains(h))))
                {
                    var mf = r.GetComponent<MeshFilter>();
                    Object.DestroyImmediate(r);
                    if (mf) Object.DestroyImmediate(mf);
                }
            }
            int Tris() => inst.GetComponentsInChildren<MeshFilter>().Sum(f => f.sharedMesh ? f.sharedMesh.triangles.Length / 3 : 0);
            int before = Tris();
            if (p.Reduce > 0 && before > p.Reduce)
            {
                float ratio = (float)p.Reduce / before;
                int i = 0;
                foreach (var f in inst.GetComponentsInChildren<MeshFilter>())
                {
                    if (!f.sharedMesh || f.sharedMesh.triangles.Length / 3 < 300) continue;
                    var m = Reduce(f.sharedMesh, ratio);
                    AssetDatabase.CreateAsset(m, $"{Gen}/{name}_{i++}.asset");
                    f.sharedMesh = m;
                }
            }
            int after = Tris();
            var prefab = PrefabUtility.SaveAsPrefabAsset(inst, $"{Gen}/{name}.prefab");
            Object.DestroyImmediate(inst);
            Debug.Log($"VEH {name} tris {before} -> {after}");
            return made[name] = prefab;
        }

        // Vertex clustering: vertices in the same grid cell collapse to the first one; the cell grows until the triangle target is met.
        static Mesh Reduce(Mesh src, float ratio)
        {
            var vs = src.vertices; var ns = src.normals; var uvs = src.uv;
            var subs = Enumerable.Range(0, src.subMeshCount).Select(s => src.GetTriangles(s)).ToArray();
            int target = Mathf.Max(60, (int)(subs.Sum(s => s.Length / 3) * ratio));
            var min = src.bounds.min;
            float lo = 0, hi = src.bounds.size.magnitude;
            for (int it = 0; it < 16; it++)
            {
                float mid = (lo + hi) / 2;
                var rep = Reps(vs, min, mid);
                int n = 0;
                foreach (var s in subs) for (int t = 0; t < s.Length; t += 3) if (Live(rep, s, t)) n++;
                if (n > target) lo = mid; else hi = mid;
            }
            var rp = Reps(vs, min, hi);
            var map = new Dictionary<int, int>();
            var pos = new List<Vector3>(); var nor = new List<Vector3>(); var uv = new List<Vector2>();
            var m2 = new Mesh { name = src.name + "_r" };
            var tris = new List<int>[subs.Length];
            for (int si = 0; si < subs.Length; si++)
            {
                tris[si] = new List<int>();
                var s = subs[si];
                for (int t = 0; t < s.Length; t += 3)
                {
                    if (!Live(rp, s, t)) continue;
                    for (int k = 0; k < 3; k++)
                    {
                        int r = rp[s[t + k]];
                        if (!map.TryGetValue(r, out int ni))
                        {
                            map[r] = ni = pos.Count;
                            pos.Add(vs[r]); nor.Add(ns.Length > r ? ns[r] : Vector3.up); uv.Add(uvs.Length > r ? uvs[r] : Vector2.zero);
                        }
                        tris[si].Add(ni);
                    }
                }
            }
            if (pos.Count > 65000) m2.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m2.SetVertices(pos); m2.SetNormals(nor); m2.SetUVs(0, uv);
            m2.subMeshCount = subs.Length;
            for (int si = 0; si < subs.Length; si++) m2.SetTriangles(tris[si], si);
            m2.RecalculateBounds();
            return m2;
        }

        static bool Live(int[] rep, int[] s, int t)
        {
            int a = rep[s[t]], b = rep[s[t + 1]], c = rep[s[t + 2]];
            return a != b && b != c && a != c;
        }

        static int[] Reps(Vector3[] vs, Vector3 min, float cell)
        {
            var map = new Dictionary<long, int>();
            var rep = new int[vs.Length];
            for (int i = 0; i < vs.Length; i++)
            {
                var q = (vs[i] - min) / Mathf.Max(cell, 1e-5f);
                long key = (long)q.x + ((long)q.y << 21) + ((long)q.z << 42);
                if (!map.TryGetValue(key, out int r)) map[key] = r = i;
                rep[i] = r;
            }
            return rep;
        }
    }
}
