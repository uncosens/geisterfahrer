using System.Collections.Generic;
using Geisterfahrer.Core;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Roadside scenery (view only): buildings, noise walls, lamp posts and trees by zone (Core/Zones.cs).
    // Each 100 m chunk gets two extra meshes, built when the chunk comes within MassRange:
    //   Mass   (buildings, noise walls): drawn to MassRange, one mesh per chunk, up to 4 facade materials
    //   Detail (lamp posts, trees):      drawn to DetailRange only (LOD)
    public partial class TrackView
    {
        const float MassRange = 900, DetailRange = 350, WinW = 12.8f, WinH = 12f; // facade texture tile: 4 windows x 4 floors
        public static int SceneryPerFrame = 1; // chunks built per frame (the shots tool raises it)
        public static bool SceneryOn = true;   // off for before/after shots

        // The data table: one row per zone. Lengths in metres, setback from the rail line.
        struct Style
        {
            public float SetMin, SetMax, GapMin, GapMax, LenMin, LenMax, DepMin, DepMax, HMin, HMax;
            public M[] Facades;
            public int Trees;          // per 100 m and side
            public float NoiseWall;    // share of chunks with a noise wall
            public bool Lamps;         // every 50 m
        }

        static readonly Style[] Styles =
        {
            /* Country    */ new Style { SetMin = 30, SetMax = 90, GapMin = 120, GapMax = 400, LenMin = 10, LenMax = 16, DepMin = 8, DepMax = 12, HMin = 6, HMax = 9, Facades = new[] { M.Facade1, M.Facade0 }, Trees = 7, NoiseWall = 0, Lamps = false },
            /* Suburb     */ new Style { SetMin = 22, SetMax = 50, GapMin = 8, GapMax = 18, LenMin = 9, LenMax = 14, DepMin = 8, DepMax = 12, HMin = 6, HMax = 11, Facades = new[] { M.Facade0, M.Facade1 }, Trees = 4, NoiseWall = 0.5f, Lamps = true },
            /* City       */ new Style { SetMin = 18, SetMax = 40, GapMin = 4, GapMax = 12, LenMin = 14, LenMax = 30, DepMin = 14, DepMax = 24, HMin = 18, HMax = 70, Facades = new[] { M.Facade2, M.Facade0 }, Trees = 1, NoiseWall = 0.3f, Lamps = true },
            /* Industrial */ new Style { SetMin = 30, SetMax = 70, GapMin = 15, GapMax = 40, LenMin = 24, LenMax = 60, DepMin = 20, DepMax = 40, HMin = 8, HMax = 14, Facades = new[] { M.Facade3 }, Trees = 1, NoiseWall = 0, Lamps = true },
        };

        readonly Dictionary<GameObject, int> sceneryOf = new Dictionary<GameObject, int>(); // chunk object -> chunk index its scenery was built for
        public static int SceneryTriangles; // last build, for the budget check in the shots tool

        // Code-generated facade: 4 x 4 windows per tile, repeated along the wall.
        static Material FacadeMat(int style)
        {
            int[] wall = { 0xD8D4C8, 0x9A5B45, 0x4A5A6A, 0xA9ADB0 }, win = { 0x39424D, 0x2E3338, 0x86A8C0, 0x4A5058 };
            const int N = 128, C = N / 4;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear, anisoLevel = 4 };
            var rnd = new System.Random(style + 1);
            var px = new Color[N * N];
            for (int y = 0; y < N; y++) for (int x = 0; x < N; x++) px[y * N + x] = Placeholder.Hex(wall[style]);
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                {
                    if (style == 3 && row != 2) continue; // industrial: one band of small windows
                    var c = rnd.NextDouble() < 0.18 && style != 3 ? Placeholder.Hex(0xFFE6A8) : Placeholder.Hex(win[style]); // some lit
                    for (int y = row * C + 8; y < row * C + C - 6; y++)
                        for (int x = col * C + 6; x < col * C + C - 6; x++) px[y * N + x] = c;
                }
            if (style == 3) // corrugated look: faint vertical lines
                for (int y = 0; y < N; y++) for (int x = 0; x < N; x += 4) if (px[y * N + x] == Placeholder.Hex(wall[3])) px[y * N + x] *= 0.92f;
            tex.SetPixels(px); tex.Apply();
            var m = new Material(Placeholder.BaseMat) { mainTexture = tex };
            m.SetFloat("_Metallic", 0); m.SetFloat("_Smoothness", style == 2 ? 0.5f : 0.1f);
            return m;
        }

        GameObject Child(GameObject chunk, string name)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(chunk.transform, false);
            go.GetComponent<MeshFilter>().sharedMesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            go.GetComponent<MeshRenderer>().sharedMaterials = mats;
            go.SetActive(false);
            return go;
        }

        void ResetScenery(GameObject chunk)
        {
            sceneryOf.Remove(chunk);
            chunk.transform.Find("Mass").gameObject.SetActive(false);
            chunk.transform.Find("Detail").gameObject.SetActive(false);
        }

        // Called every frame after the chunks are placed: builds scenery for chunks that came in range, shows or hides by distance.
        void UpdateScenery(double s)
        {
            int budget = SceneryPerFrame;
            if (!SceneryOn) { foreach (var go in chunks.Values) { go.transform.Find("Mass").gameObject.SetActive(false); go.transform.Find("Detail").gameObject.SetActive(false); } return; }
            foreach (var kv in chunks)
            {
                float dist = Mathf.Abs((kv.Key + 0.5f) * ChunkLen - (float)s);
                var go = kv.Value;
                bool built = sceneryOf.TryGetValue(go, out int idx) && idx == kv.Key;
                if (!built && dist < MassRange && budget > 0 && (kv.Key + 1) * ChunkLen + 150 < track.Generated) { BuildScenery(kv.Key, go); budget--; built = true; }
                go.transform.Find("Mass").gameObject.SetActive(built && dist < MassRange);
                go.transform.Find("Detail").gameObject.SetActive(built && dist < DetailRange);
            }
        }

        // True when nothing special (tunnel, viaduct, underground, overpass) is near [a, b]: scenery stays away from structures.
        bool OpenRoad(double a, double b)
        {
            foreach (var st in track.Structures)
            {
                double m = st.kind == Section.Tunnel ? 330 : 120;
                if (st.s0 < b + m && st.s1 > a - m) return false;
            }
            foreach (double o in track.Overpasses) if (o > a - 60 && o < b + 60) return false;
            return true;
        }

        void BuildScenery(int index, GameObject chunk)
        {
            sceneryOf[chunk] = index;
            double s0 = index * ChunkLen, s1 = s0 + ChunkLen;
            var mass = new Builder(); var detail = new Builder();
            if (index >= 0 && OpenRoad(s0, s1))
            {
                var rnd = new System.Random(index * 104729 + 11);
                var st = Styles[(int)track.Zones.At((s0 + s1) / 2)];
                foreach (int side in new[] { -1, 1 })
                {
                    // Buildings: a row with random gaps; each fits inside this chunk, so neighbours never overlap.
                    if (st.GapMax > 0)
                    {
                        double cur = s0 + 2 + rnd.NextDouble() * st.GapMin;
                        while (cur < s1 - st.LenMin - 2)
                        {
                            float len = Mathf.Min(Rnd(rnd, st.LenMin, st.LenMax), (float)(s1 - 2 - cur)), dep = Rnd(rnd, st.DepMin, st.DepMax), h = Rnd(rnd, st.HMin, st.HMax);
                            float set = Rnd(rnd, st.SetMin, st.SetMax);
                            if (len >= st.LenMin * 0.7f) mass.Block(cur + len / 2, side * (Rail + set + dep / 2), len, dep, h, (int)st.Facades[rnd.Next(st.Facades.Length)], (float)rnd.NextDouble());
                            cur += len + Rnd(rnd, st.GapMin, st.GapMax);
                        }
                    }
                    if (st.NoiseWall > 0 && rnd.NextDouble() < st.NoiseWall)
                        for (double s = s0 + 4; s < s1; s += 8) mass.Box(s, side * (Rail + 3.6f), 2f, 8.05f, 0.3f, 4f, (int)M.Concrete);
                    // Detail: lamp posts every 50 m and trees.
                    if (st.Lamps)
                        for (double s = Mathf.Ceil((float)(s0 / 50)) * 50; s < s1; s += 50)
                        {
                            detail.Box(s, side * (Rail + 1.6f), 4.5f, 0.15f, 0.15f, 9f, (int)M.SignBack);
                            detail.Box(s, side * (Rail + 0.7f), 9.1f, 0.6f, 1.8f, 0.12f, (int)M.Lamp);
                        }
                    for (int i = 0; i < st.Trees; i++)
                    {
                        double s = s0 + rnd.NextDouble() * ChunkLen;
                        float d = side * (Rail + 6 + (float)rnd.NextDouble() * 40), k = 0.8f + (float)rnd.NextDouble() * 0.7f;
                        detail.Box(s, d, 1.1f * k, 0.4f * k, 0.4f * k, 2.2f * k, (int)M.Trunk);
                        detail.Box(s, d, 3.2f * k, 3f * k, 3f * k, 2.6f * k, (int)M.Tree);
                        detail.Box(s, d, 5.1f * k, 2f * k, 2f * k, 2f * k, (int)M.Tree);
                    }
                }
            }
            Fill(chunk.transform.Find("Mass").gameObject, mass);
            Fill(chunk.transform.Find("Detail").gameObject, detail);
        }

        static float Rnd(System.Random r, float a, float b) => a + (float)r.NextDouble() * (b - a);

        static void Fill(GameObject go, Builder b)
        {
            b.Apply(go.GetComponent<MeshFilter>().sharedMesh);
            SceneryTriangles += go.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3;
        }

        partial class Builder
        {
            // A building: facade on the four sides (UVs in window tiles, shifted by uOff so neighbours differ), flat concrete roof.
            public void Block(double s, float d, float length, float width, float height, int facade, float uOff)
            {
                Frame(s, out var p, out var r, out var f);
                var c = p + r * d + Vector3.up * (height / 2 - 0.3f);
                Vector3 x = r * width / 2, z = f * length / 2, u = Vector3.up * (height + 0.3f) / 2;
                var o = new[] { c - x - u - z, c + x - u - z, c + x + u - z, c - x + u - z, c - x - u + z, c + x - u + z, c + x + u + z, c - x + u + z };
                int[,] sides = { { 0, 3, 2, 1 }, { 5, 6, 7, 4 }, { 4, 7, 3, 0 }, { 1, 2, 6, 5 } };
                float v = height / WinH;
                for (int i = 0; i < 4; i++)
                {
                    float uu = (i < 2 ? width : length) / WinW, a = uOff;
                    Quad(o[sides[i, 0]], o[sides[i, 1]], o[sides[i, 2]], o[sides[i, 3]], facade, new Vector2(a, 0), new Vector2(a, v), new Vector2(a + uu, v), new Vector2(a + uu, 0));
                }
                Quad(o[3], o[7], o[6], o[2], (int)M.Concrete, Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
            }
        }
    }
}
