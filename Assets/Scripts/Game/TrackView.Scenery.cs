using System.Collections.Generic;
using Geisterfahrer.Core;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Roadside scenery and the new structure views (GDD 6.6-6.8). View only.
    // Each 100 m chunk gets two extra meshes, built when the chunk comes within MassRange:
    //   Mass   (buildings, noise walls): drawn to MassRange, one mesh per chunk, up to 4 facade materials
    //   Detail (lamp posts, trees):      drawn to DetailRange only (LOD)
    // Sides: -1 = the player's left (outer rail), +1 = the median side.
    public partial class TrackView
    {
        const float MassRange = 900, DetailRange = 350, WinW = 12.8f, WinH = 12f; // facade texture tile: 4 windows x 4 floors
        public static int SceneryPerFrame = 1; // chunks built per frame (the shots tool raises it)
        public static bool SceneryOn = true;   // off for before/after shots

        // The data table: one row per zone and side (GDD 6.7). Lengths in metres, setback from the rail line.
        struct Side
        {
            public float SetMin, SetMax, LenMin, LenMax, DepMin, DepMax, GapMin, GapMax;
            public int Max; // buildings per 100 m chunk on this side
        }

        struct Style
        {
            public Side Left, Right;
            public float HMin, HMax;       // building height
            public M[] Facades;
            public int Trees;              // per 100 m and side
            public float TreeNear, TreeFar; // tree distance from the rail
            public float NoiseWall, NoiseH; // share of chunks on the outer side, wall height
            public float LampSpacing;      // median lamp posts, 0 = none
        }

        static readonly Style[] Styles =
        {
            /* Country    */ new Style { Left = new Side { SetMin = 120, SetMax = 400, LenMin = 15, LenMax = 30, DepMin = 10, DepMax = 20, GapMin = 120, GapMax = 400, Max = 1 },
                                          Right = new Side { SetMin = 60, SetMax = 300, LenMin = 15, LenMax = 30, DepMin = 10, DepMax = 20, GapMin = 150, GapMax = 400, Max = 1 },
                                          HMin = 6, HMax = 10, Facades = new[] { M.Facade1, M.Facade0 }, Trees = 8, TreeNear = 10, TreeFar = 25, NoiseWall = 0, NoiseH = 0, LampSpacing = 0 },
            /* Suburb     */ new Style { Left = new Side { SetMin = 25, SetMax = 60, LenMin = 10, LenMax = 20, DepMin = 10, DepMax = 15, GapMin = 8, GapMax = 30, Max = 8 },
                                          Right = new Side { SetMin = 55, SetMax = 130, LenMin = 10, LenMax = 20, DepMin = 10, DepMax = 15, GapMin = 8, GapMax = 30, Max = 5 },
                                          HMin = 7, HMax = 16, Facades = new[] { M.Facade0, M.Facade1 }, Trees = 5, TreeNear = 8, TreeFar = 40, NoiseWall = 0.4f, NoiseH = 3, LampSpacing = 80 },
            /* City       */ new Style { Left = new Side { SetMin = 15, SetMax = 35, LenMin = 25, LenMax = 60, DepMin = 20, DepMax = 45, GapMin = 6, GapMax = 20, Max = 5 },
                                          Right = new Side { SetMin = 40, SetMax = 100, LenMin = 25, LenMax = 60, DepMin = 20, DepMax = 45, GapMin = 6, GapMax = 20, Max = 3 },
                                          HMin = 19, HMax = 80, Facades = new[] { M.Facade2, M.Facade0 }, Trees = 1, TreeNear = 8, TreeFar = 30, NoiseWall = 0.6f, NoiseH = 4, LampSpacing = 40 },
            /* Industrial */ new Style { Left = new Side { SetMin = 30, SetMax = 80, LenMin = 40, LenMax = 120, DepMin = 30, DepMax = 60, GapMin = 20, GapMax = 80, Max = 2 },
                                          Right = new Side { SetMin = 65, SetMax = 150, LenMin = 40, LenMax = 120, DepMin = 30, DepMax = 60, GapMin = 20, GapMax = 80, Max = 1 },
                                          HMin = 8, HMax = 15, Facades = new[] { M.Facade3 }, Trees = 1, TreeNear = 8, TreeFar = 30, NoiseWall = 0.2f, NoiseH = 3, LampSpacing = 60 },
        };

        readonly Dictionary<GameObject, int> sceneryOf = new Dictionary<GameObject, int>(); // chunk object -> chunk index its scenery was built for
        public static int SceneryTriangles; // last build, for the budget check in the shots tool

        // Code-generated facade: 4 x 4 windows per tile, repeated along the wall. Mid to dark, low saturation (F8).
        static Material FacadeMat(int style)
        {
            int[] wall = { 0x9D9A90, 0x8A5646, 0x3B4A58, 0x8D9296 }, win = { 0x2F3740, 0x252A2F, 0x6F8CA3, 0x3D4349 };
            const int N = 128, C = N / 4;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear, anisoLevel = 4 };
            var rnd = new System.Random(style + 1);
            var px = new Color[N * N];
            for (int y = 0; y < N; y++) for (int x = 0; x < N; x++) px[y * N + x] = Placeholder.Hex(wall[style]);
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                {
                    if (style == 3 && row != 2) continue; // industrial: one band of small windows
                    var c = rnd.NextDouble() < 0.18 && style != 3 ? Placeholder.Hex(0xE8CF9A) : Placeholder.Hex(win[style]); // some lit
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
            if (!SceneryOn) { foreach (var c in chunks.Values) { c.transform.Find("Mass").gameObject.SetActive(false); c.transform.Find("Detail").gameObject.SetActive(false); } return; }
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

        // True when nothing special (tunnel, viaduct, underground, interchange, overpass) is near [a, b]: scenery stays away (F4, F5).
        bool OpenRoad(double a, double b)
        {
            foreach (var st in track.Structures)
            {
                double m = st.kind == Section.Tunnel ? 330 : st.kind == Section.Interchange ? 160 : 120;
                if (st.s0 < b + m && st.s1 > a - m) return false;
            }
            foreach (double o in track.Overpasses) if (o > a - 60 && o < b + 60) return false;
            return true;
        }

        // F2: on the inner side of a curve nothing tall within c(R) of the rail. Returns the clearance per side (0 = none).
        void CurveClearance(double s0, double s1, out float left, out float right)
        {
            left = right = 0;
            for (double x = s0 - 60; x <= s1 + 60; x += 20)
            {
                double k = track.Curvature(x);
                if (System.Math.Abs(k) < 1e-5) continue;
                float c = Mathf.Max(15, (float)(31250 * System.Math.Abs(k) - 1.6));
                if (k > 0) right = Mathf.Max(right, c); else left = Mathf.Max(left, c); // a right curve has its inner side on the right
            }
        }

        static float Rnd(System.Random r, float a, float b) => a + (float)r.NextDouble() * (b - a);

        void BuildScenery(int index, GameObject chunk)
        {
            sceneryOf[chunk] = index;
            double s0 = index * ChunkLen, s1 = s0 + ChunkLen, mid = (s0 + s1) / 2;
            var mass = new Builder(); var detail = new Builder();
            if (index >= 0 && OpenRoad(s0, s1))
            {
                var rnd = new System.Random(index * 104729 + 11);
                var st = Styles[(int)track.Zones.At(mid)];
                bool border = track.Zones.NearBorder(mid, 250); // density ramps down towards a zone border
                CurveClearance(s0, s1, out float clearL, out float clearR);
                bool crest = track.NearCrest(s0, s1);          // F3: nothing taller than 3 m within 30 m of the rail
                foreach (int side in new[] { -1, 1 })
                {
                    var sd = side < 0 ? st.Left : st.Right;
                    float clear = Mathf.Max(side < 0 ? clearL : clearR, crest ? 30 : 0);
                    // Buildings: a row with random gaps; each fits inside this chunk, so neighbours never overlap.
                    int placed = 0, streetIn = rnd.Next(3, 6);
                    double cur = s0 + 2 + rnd.NextDouble() * sd.GapMin;
                    while (cur < s1 - sd.LenMin * 0.7 - 2 && placed < sd.Max)
                    {
                        float len = Mathf.Min(Rnd(rnd, sd.LenMin, sd.LenMax), (float)(s1 - 2 - cur)), dep = Rnd(rnd, sd.DepMin, sd.DepMax);
                        float h = Height(rnd, st);
                        float set = Mathf.Max(Rnd(rnd, sd.SetMin, sd.SetMax), clear);
                        if (len >= sd.LenMin * 0.7f && !(border && rnd.NextDouble() < 0.5))
                        {
                            mass.Block(cur + len / 2, side * (Rail + set + dep / 2), len, dep, h, (int)st.Facades[rnd.Next(st.Facades.Length)], (float)rnd.NextDouble());
                            placed++;
                        }
                        float gap = Rnd(rnd, sd.GapMin, sd.GapMax);
                        if (--streetIn <= 0) { gap = Rnd(rnd, 20, 35); streetIn = rnd.Next(3, 6); } // a street gap every 3-5 buildings
                        cur += len + gap;
                    }
                    // Noise wall: outer side only, 3-6 m behind the rail (F1: not on the inner side of a curve, F2).
                    if (side < 0 && st.NoiseWall > 0 && clearL == 0 && !crest && rnd.NextDouble() < st.NoiseWall)
                        for (double s = s0 + 4; s < s1; s += 8)
                        {
                            mass.Box(s, -(Rail + 4f), st.NoiseH / 2, 8.05f, 0.3f, st.NoiseH, (int)M.Concrete);
                            detail.Box(s - 4, -(Rail + 3.9f), st.NoiseH / 2, 0.4f, 0.5f, st.NoiseH, (int)M.SignBack); // post every 8 m
                        }
                    // Median lamp posts, double arm, 9 m (F1: 0.15 m wide pole), right side only.
                    if (side > 0 && st.LampSpacing > 0)
                        for (double s = Mathf.Ceil((float)(s0 / st.LampSpacing)) * st.LampSpacing; s < s1; s += st.LampSpacing)
                        {
                            detail.Box(s, Rail + 1.4f, 4.5f, 0.15f, 0.15f, 9f, (int)M.SignBack);
                            detail.Box(s, Rail + 0.2f, 9.1f, 0.6f, 1.6f, 0.12f, (int)M.Lamp);
                            detail.Box(s, Rail + 2.6f, 9.1f, 0.6f, 1.6f, 0.12f, (int)M.Lamp);
                        }
                    for (int i = 0; i < st.Trees; i++)
                    {
                        double s = s0 + rnd.NextDouble() * ChunkLen;
                        float d = side * (Rail + Mathf.Max(Rnd(rnd, st.TreeNear, st.TreeFar), clear)), k = 0.8f + (float)rnd.NextDouble() * 0.7f;
                        detail.Box(s, d, 1.1f * k, 0.4f * k, 0.4f * k, 2.2f * k, (int)M.Trunk);
                        detail.Box(s, d, 3.2f * k, 3f * k, 3f * k, 2.6f * k, (int)M.Tree);
                        detail.Box(s, d, 5.1f * k, 2f * k, 2f * k, 2f * k, (int)M.Tree);
                    }
                }
                // Industrial landmarks: a silo or tank every 200 m, a chimney at most every 600 m.
                if (track.Zones.At(mid) == Zone.Industrial)
                {
                    if (index % 2 == 0) mass.Block(s0 + 50, -(Rail + 45 + (float)rnd.NextDouble() * 30), 12, 12, Rnd(rnd, 20, 45), (int)M.Facade3, 0);
                    if (index % 6 == 3) mass.Block(s0 + 30, -(Rail + 70), 4, 4, Rnd(rnd, 60, 100), (int)M.Facade3, 0);
                }
            }
            Fill(chunk.transform.Find("Mass").gameObject, mass);
            Fill(chunk.transform.Find("Detail").gameObject, detail);
        }

        // City: 40 % 19-32 m, 40 % 32-52 m, 20 % 52-80 m (6-25 floors); other zones uniform in their range.
        static float Height(System.Random r, Style st)
        {
            if (st.HMax < 60) return Rnd(r, st.HMin, st.HMax);
            double q = r.NextDouble();
            return q < 0.4 ? Rnd(r, 19, 32) : q < 0.8 ? Rnd(r, 32, 52) : Rnd(r, 52, 80);
        }

        static void Fill(GameObject go, Builder b)
        {
            b.Apply(go.GetComponent<MeshFilter>().sharedMesh);
            SceneryTriangles += go.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3;
        }

        // ---- trench (GDD 6.6, F7): retaining walls 2.5 m behind the rail, panels every 4 m ----

        void TrenchSegment(Builder b, double s, double e, int fieldL, int fieldR)
        {
            float g0 = (float)track.Depth(s), g1 = (float)track.Depth(e), wx = Rail + 2.5f;
            foreach (int side in new[] { -1, 1 })
            {
                b.Side(s, e, side * wx, g0, g1, (int)M.Concrete);
                if (g0 > 0.5f) b.Box(s, side * (wx - 0.08f), g0 / 2, 0.25f, 0.12f, g0, (int)M.SignBack); // panel joint
            }
            b.Strip(s, e, Rail, wx, -0.02f, (int)M.Concrete, 2f);
            b.Strip(s, e, -wx, -Rail, -0.02f, (int)M.Concrete, 2f);
            b.Strip(s, e, -40, -wx, g0 - 0.05f, (int)M.Grass, 3f, false, g1 - 0.05f);
            b.Strip(s, e, wx, 40, g0 - 0.05f, (int)M.Grass, 3f, false, g1 - 0.05f);
            b.Strip(s, e, -400, -40, g0 - 0.1f, fieldL, 0, false, g1 - 0.1f);
            b.Strip(s, e, 40, 400, g0 - 0.1f, fieldR, 0, false, g1 - 0.1f);
        }

        // ---- interchange a (GDD 6.6): visible slip roads on the player's left, never joining the road ----

        const float RampW = 6.5f, RampTaper = 120, RampLen = 820;

        // Lateral gap between the rail and the ramp's inner edge at distance t from the gore tip: taper beside the road,
        // a grass wedge, 200 m parallel, then it swings away to 60 m within 500 m.
        static float RampGap(float t)
        {
            if (t < 120) return 1.2f;
            if (t < 200) return Mathf.Lerp(1.2f, 6f, Smooth((t - 120) / 80));
            if (t < 320) return 6f;
            return Mathf.Lerp(6f, 60f, Smooth((t - 320) / 500));
        }

        void Ramps(Builder b, double s, double e)
        {
            foreach (var st in track.Structures)
            {
                if (st.kind != Section.Interchange || s < st.s0 || s >= st.s1) continue;
                // Exit ramp from the start, entry ramp towards the end (mirrored); the entry one sits 14 m further out where they overlap.
                for (int r = 0; r < 2; r++)
                {
                    float ta = r == 0 ? (float)(s - st.s0) : (float)(st.s1 - s), tb = r == 0 ? (float)(e - st.s0) : (float)(st.s1 - e);
                    if (ta > RampLen && tb > RampLen) continue;
                    float extra = r == 1 ? 14 : 0;
                    float wa = RampW * Smooth(ta / RampTaper), wb = RampW * Smooth(tb / RampTaper);
                    float ca = Rail + RampGap(Mathf.Min(ta, RampLen)) + extra + wa / 2, cb = Rail + RampGap(Mathf.Min(tb, RampLen)) + extra + wb / 2;
                    b.Ribbon(s, e, -ca, -cb, wa, wb, 0.01f, (int)M.Asphalt, 2.08f);
                    b.Ribbon(s, e, -(ca - wa / 2 + 0.2f), -(cb - wb / 2 + 0.2f), 0.15f, 0.15f, 0.025f, (int)M.Paint, 0);
                    b.Ribbon(s, e, -(ca + wa / 2 - 0.2f), -(cb + wb / 2 - 0.2f), 0.15f, 0.15f, 0.025f, (int)M.Paint, 0);
                    if (ta < RampLen && tb >= RampLen) // the end: a small roundabout for the local street
                        b.Pad((s + e) / 2, -(Rail + 60 + extra + RampW / 2), 14, 0.01f, (int)M.Asphalt);
                }
            }
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

            // Flat ribbon from s0 to s1 whose centre and width change linearly (ramps). World UVs when tile > 0.
            public void Ribbon(double s0, double s1, float c0, float c1, float w0, float w1, float y, int sub, float tile)
            {
                if (w0 <= 0.01f && w1 <= 0.01f) return;
                float t = tile > 0 ? tile : 1;
                float l0 = Mathf.Min(c0 - w0 / 2, c0 + w0 / 2), r0 = Mathf.Max(c0 - w0 / 2, c0 + w0 / 2), l1 = Mathf.Min(c1 - w1 / 2, c1 + w1 / 2), r1 = Mathf.Max(c1 - w1 / 2, c1 + w1 / 2);
                Quad(At(s0, l0, y), At(s1, l1, y), At(s1, r1, y), At(s0, r0, y), sub,
                    new Vector2(l0 / t, (float)(s0 / t)), new Vector2(l1 / t, (float)(s1 / t)), new Vector2(r1 / t, (float)(s1 / t)), new Vector2(r0 / t, (float)(s0 / t)));
            }

            // Flat round pad (roundabout) at (s, d).
            public void Pad(double s, float d, float radius, float y, int sub)
            {
                Frame(s, out var p, out var r, out var f);
                var c = p + r * d + Vector3.up * y;
                const int N = 20;
                for (int i = 0; i < N; i++)
                {
                    float a0 = 2 * Mathf.PI * i / N, a1 = 2 * Mathf.PI * (i + 1) / N;
                    var p0 = c + (r * Mathf.Cos(a0) + f * Mathf.Sin(a0)) * radius;
                    var p1 = c + (r * Mathf.Cos(a1) + f * Mathf.Sin(a1)) * radius;
                    Quad(c, p1, p0, p0, sub, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                }
            }
        }
    }
}
