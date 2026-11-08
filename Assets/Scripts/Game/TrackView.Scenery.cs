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
            public int Trees;              // per 100 m and side
            public float TreeNear, TreeFar; // tree distance from the rail
            public float NoiseWall, NoiseH; // share of chunks on the outer side, wall height
            public int NoiseColor;         // panel colour: glass-look, timber-look or concrete
            public float LampSpacing;      // median lamp posts, 0 = none
        }

        static readonly Style[] Styles =
        {
            /* Country    */ new Style { Left = new Side { SetMin = 120, SetMax = 400, GapMin = 120, GapMax = 400, Max = 1 }, Right = new Side { SetMin = 60, SetMax = 300, GapMin = 150, GapMax = 400, Max = 1 },
                                          Trees = 8, TreeNear = 10, TreeFar = 25, NoiseWall = 0, NoiseH = 0, NoiseColor = 0, LampSpacing = 0 },
            /* Suburb     */ new Style { Left = new Side { SetMin = 25, SetMax = 60, GapMin = 8, GapMax = 30, Max = 8 }, Right = new Side { SetMin = 55, SetMax = 130, GapMin = 8, GapMax = 30, Max = 5 },
                                          Trees = 5, TreeNear = 8, TreeFar = 40, NoiseWall = 0.4f, NoiseH = 3, NoiseColor = 0x7C8068, LampSpacing = 80 },
            /* City       */ new Style { Left = new Side { SetMin = 15, SetMax = 35, GapMin = 6, GapMax = 20, Max = 5 }, Right = new Side { SetMin = 40, SetMax = 100, GapMin = 6, GapMax = 20, Max = 3 },
                                          Trees = 1, TreeNear = 8, TreeFar = 30, NoiseWall = 0.6f, NoiseH = 4, NoiseColor = 0xA9B8C2, LampSpacing = 40 },
            /* Industrial */ new Style { Left = new Side { SetMin = 30, SetMax = 80, GapMin = 20, GapMax = 80, Max = 2 }, Right = new Side { SetMin = 65, SetMax = 150, GapMin = 20, GapMax = 80, Max = 1 },
                                          Trees = 1, TreeNear = 8, TreeFar = 30, NoiseWall = 0.2f, NoiseH = 3, NoiseColor = 0x9C9FA3, LampSpacing = 60 },
        };

        // Building kit (urban-spec 1.1): footprint and height ranges (metres), facade slices, roof.
        enum Roof { Flat, Gable, LowPitch }
        struct Kind { public float LenMin, LenMax, DepMin, DepMax, HMin, HMax; public int[] Slices; public Roof Roof; }

        static readonly Kind Block_ = new Kind { LenMin = 25, LenMax = 60, DepMin = 12, DepMax = 16, HMin = 16, HMax = 29, Slices = new[] { 0, 1, 2, 4 }, Roof = Roof.Flat };
        static readonly Kind SuburbBlock = new Kind { LenMin = 25, LenMax = 45, DepMin = 12, DepMax = 16, HMin = 13, HMax = 16, Slices = new[] { 0, 1, 4 }, Roof = Roof.Flat };
        static readonly Kind Tower = new Kind { LenMin = 22, LenMax = 30, DepMin = 22, DepMax = 30, HMin = 38, HMax = 70, Slices = new[] { 2, 3 }, Roof = Roof.Flat };
        static readonly Kind Office = new Kind { LenMin = 30, LenMax = 50, DepMin = 25, DepMax = 40, HMin = 32, HMax = 80, Slices = new[] { 3 }, Roof = Roof.Flat };
        static readonly Kind Mixed = new Kind { LenMin = 25, LenMax = 45, DepMin = 15, DepMax = 25, HMin = 16, HMax = 29, Slices = new[] { 0, 2, 4 }, Roof = Roof.Flat };
        static readonly Kind House = new Kind { LenMin = 10, LenMax = 20, DepMin = 8, DepMax = 12, HMin = 6.4f, HMax = 6.4f, Slices = new[] { 6 }, Roof = Roof.Gable };
        static readonly Kind Shed = new Kind { LenMin = 40, LenMax = 120, DepMin = 30, DepMax = 60, HMin = 8, HMax = 15, Slices = new[] { 5 }, Roof = Roof.LowPitch };
        static readonly Kind Farm = new Kind { LenMin = 15, LenMax = 30, DepMin = 10, DepMax = 20, HMin = 6, HMax = 10, Slices = new[] { 6 }, Roof = Roof.Gable };

        static Kind PickKind(Zone z, System.Random r)
        {
            double q = r.NextDouble();
            switch (z)
            {
                case Zone.City: return q < 0.35 ? Block_ : q < 0.55 ? Mixed : q < 0.7 ? Tower : Office;
                case Zone.Suburb: return q < 0.6 ? House : SuburbBlock;
                case Zone.Industrial: return Shed;
                default: return Farm;
            }
        }

        readonly Dictionary<GameObject, int> sceneryOf = new Dictionary<GameObject, int>(); // chunk object -> chunk index its scenery was built for
        public static int SceneryTriangles; // last build, for the budget check in the shots tool

        // Solid scenery colour: slice 7 is plain white, the vertex colour is the colour (sRGB hex in, linear stored).
        static Color32 Solid(int hex)
        {
            var c = Placeholder.Hex(hex).linear;
            return new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), 7 * 32);
        }

        // Instance tint of a textured wall: value 0.88-1.04, saturation untouched.
        static Color32 Tinted(int slice, System.Random r)
        {
            byte v = (byte)Mathf.Min(255, 255 * Rnd(r, 0.88f, 1.04f));
            return new Color32(v, v, v, (byte)(slice * 32));
        }

        // One Texture2DArray, 8 slices of 256 px: facade tiles of 2 bays x 2 floors (7.2 x 6.4 m, 36 px per metre); alpha = smoothness.
        // Every colour channel stays at or below #BE (urban-spec rule 2).
        public const float TileW = 7.2f, TileH = 6.4f;

        static Texture2DArray FacadeArray()
        {
            const int N = 256;
            var arr = new Texture2DArray(N, N, 8, TextureFormat.RGBA32, true, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear, anisoLevel = 2 };
            var rnd = new System.Random(42);
            int[] wall = { 0xB8B2A3, 0xAEB5A0, 0x8E9093, 0x4A5058, 0xA19470, 0x7F909A, 0xB5AE9A, 0xFFFFFF };
            int[] glass = { 0x3E4A57, 0x3E4A57, 0x434C57, 0x7898B6, 0x4A525C, 0x4B5560, 0x3E4A57, 0 };
            for (int sl = 0; sl < 8; sl++)
            {
                var px = new Color32[N * N];
                void Fill(int x0, int y0, int x1, int y1, int hex, int smooth)
                {
                    var c = Placeholder.Hex(hex);
                    var k = new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), (byte)smooth);
                    for (int y = Mathf.Max(0, y0); y < Mathf.Min(N, y1); y++) for (int x = Mathf.Max(0, x0); x < Mathf.Min(N, x1); x++) px[y * N + x] = k;
                }
                Fill(0, 0, N, N, wall[sl], 26);
                if (sl <= 2 || sl == 4 || sl == 6)
                    for (int cy = 0; cy < 2; cy++)
                        for (int cx = 0; cx < 2; cx++) // each cell: one bay (3.6 m = 128 px) x one floor (3.2 m = 128 px)
                        {
                            int ow = sl == 6 ? 50 : sl == 4 ? 57 : 60, oh = sl == 6 ? 50 : 53, ox = cx * 128 + (128 - ow) / 2, oy = cy * 128 + (sl == 6 ? 36 : 32);
                            Fill(ox - 2, oy - 2, ox + ow + 2, oy + oh + 2, 0x8D9096, 26);   // frame
                            Fill(ox, oy, ox + ow, oy + oh, glass[sl], 153);                  // glass
                            Fill(ox, oy + oh - 2, ox + ow, oy + oh - 1, 0x58626E, 153);      // lighter top edge
                            if (sl == 0 || sl == 1) Fill(cx * 128, cy * 128, cx * 128 + 128, cy * 128 + 2, 0xA39E91, 26); // joint band every floor
                        }
                if (sl == 2) for (int i = 0; i < 2; i++) { Fill(i * 128, 0, i * 128 + 1, N, 0x74777A, 26); Fill(0, i * 128, N, i * 128 + 1, 0x74777A, 26); } // panel joints
                if (sl == 3) // curtain wall: glass cells at a 1.8 m mullion pitch
                    for (int cy = 0; cy < 2; cy++)
                        for (int cx = 0; cx < 4; cx++)
                        {
                            int gx = cx * 64 + 1, gy = cy * 128 + 20;
                            Fill(gx, gy, gx + 61, gy + 68, glass[3], 204);
                            Fill(gx, gy + 54, gx + 61, gy + 68, 0x8DAAC4, 204);          // top 20 % lighter: sky reflection
                        }
                if (sl == 4) for (int y = 0; y < N; y += 4) Fill(0, y, N, y + 1, 0x8C8060, 26); // mortar lines
                if (sl == 5)
                {
                    for (int x = 0; x < N; x += 21) Fill(x, 0, x + 1, N, 0x6C7B84, 26);   // vertical ribs
                    Fill(0, 190, N, 222, 0x4B5560, 26); Fill(0, 0, N, 28, 0x566F5F, 26);  // ribbon and dark base band
                }
                if (sl < 7)
                    for (int i = 0; i < px.Length; i++) // grain: +-3 % tone
                    {
                        float g = 1 + ((float)rnd.NextDouble() - 0.5f) * 0.06f;
                        var c = px[i]; px[i] = new Color32((byte)Mathf.Min(0xBE, c.r * g), (byte)Mathf.Min(0xBE, c.g * g), (byte)Mathf.Min(0xBE, c.b * g), c.a);
                    }
                arr.SetPixels32(px, sl);
            }
            arr.Apply(true, false);
            return arr;
        }

        static Material FacadeMaterial(Material src)
        {
            if (!src || !src.HasProperty("_Slices")) return Placeholder.Mat(Placeholder.Hex(0x8A8D90), 0, 0.1f); // no shader asset yet: flat grey
            var m = new Material(src);
            m.SetTexture("_Slices", FacadeArray());
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
            chunk.transform.Find("Sky").gameObject.SetActive(false);
        }

        // Called every frame after the chunks are placed: builds scenery for chunks that came in range, shows or hides by distance.
        void UpdateScenery(double s)
        {
            int budget = SceneryPerFrame;
            if (!SceneryOn) { foreach (var c in chunks.Values) foreach (var n in new[] { "Mass", "Detail", "Sky" }) c.transform.Find(n).gameObject.SetActive(false); return; }
            foreach (var kv in chunks)
            {
                float dist = Mathf.Abs((kv.Key + 0.5f) * ChunkLen - (float)s);
                var go = kv.Value;
                bool built = sceneryOf.TryGetValue(go, out int idx) && idx == kv.Key;
                if (!built && dist < SkyRange && budget > 0 && (kv.Key + 1) * ChunkLen + 150 < track.Generated) { BuildScenery(kv.Key, go); budget--; built = true; }
                go.transform.Find("Mass").gameObject.SetActive(built && dist < MassRange);
                go.transform.Find("Sky").gameObject.SetActive(built && dist >= MassRange - 100);
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

        const int Sub = (int)M.Facade0; // every scenery quad goes into this one submesh: one material, one draw call per mesh
        const float SkyRange = 1500;

        void BuildScenery(int index, GameObject chunk)
        {
            sceneryOf[chunk] = index;
            double s0 = index * ChunkLen, s1 = s0 + ChunkLen, mid = (s0 + s1) / 2;
            var mass = new Builder(); var detail = new Builder(); var sky = new Builder();
            if (index >= 0 && OpenRoad(s0, s1))
            {
                var rnd = new System.Random(index * 104729 + 11);
                var zone = track.Zones.At(mid);
                var st = Styles[(int)zone];
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
                    var first = PickKind(zone, rnd);
                    while (cur < s1 - 12 && placed < sd.Max)
                    {
                        var k = placed == 0 ? first : PickKind(zone, rnd);
                        float len = Mathf.Min(Rnd(rnd, k.LenMin, k.LenMax), (float)(s1 - 2 - cur)), dep = Rnd(rnd, k.DepMin, k.DepMax);
                        float h = Rnd(rnd, k.HMin, k.HMax), set = Mathf.Max(Rnd(rnd, sd.SetMin, sd.SetMax), clear);
                        if (len >= k.LenMin * 0.7f && !(border && rnd.NextDouble() < 0.5))
                        {
                            int slice = k.Slices[rnd.Next(k.Slices.Length)];
                            mass.Block(cur + len / 2, side * (Rail + set + dep / 2), len, dep, h, Tinted(slice, rnd), k.Roof, Solid(k.Roof == Roof.Flat ? 0x6E7174 : k.Roof == Roof.Gable ? 0x4A4D52 : 0x9AA3A6), (float)rnd.NextDouble());
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
                            mass.Tint = Solid(st.NoiseColor);
                            mass.Box(s, -(Rail + 4f), st.NoiseH / 2, 8.05f, 0.3f, st.NoiseH, Sub);
                            mass.Tint = Solid(0x5E6469);
                            mass.Box(s - 4, -(Rail + 3.9f), st.NoiseH / 2, 0.25f, 0.5f, st.NoiseH, Sub); // post
                        }
                    // Median lamp posts, double arm, 9 m (F1: 0.15 m wide pole), right side only.
                    if (side > 0 && st.LampSpacing > 0)
                        for (double s = Mathf.Ceil((float)(s0 / st.LampSpacing)) * st.LampSpacing; s < s1; s += st.LampSpacing)
                        {
                            detail.Tint = Solid(0x9AA0A5);
                            detail.Box(s, Rail + 1.4f, 4.5f, 0.15f, 0.15f, 9f, Sub);
                            detail.Tint = Solid(0xD8DADC);
                            detail.Box(s, Rail + 0.2f, 9.1f, 0.6f, 1.6f, 0.12f, Sub);
                            detail.Box(s, Rail + 2.6f, 9.1f, 0.6f, 1.6f, 0.12f, Sub);
                        }
                    for (int i = 0; i < st.Trees; i++)
                    {
                        double s = s0 + rnd.NextDouble() * ChunkLen;
                        float d = side * (Rail + Mathf.Max(Rnd(rnd, st.TreeNear, st.TreeFar), clear)), k = 0.8f + (float)rnd.NextDouble() * 0.7f;
                        detail.Tint = Solid(0x4E3B2B);
                        detail.Box(s, d, 1.1f * k, 0.4f * k, 0.4f * k, 2.2f * k, Sub);
                        detail.Tint = Solid(0x3E5233);
                        detail.Box(s, d, 3.2f * k, 3f * k, 3f * k, 2.6f * k, Sub);
                        detail.Box(s, d, 5.1f * k, 2f * k, 2f * k, 2f * k, Sub);
                    }
                }
                // Industrial landmarks: a silo or tank every 200 m, a chimney at most every 600 m (beyond 300 m only: the mass mesh).
                if (zone == Zone.Industrial)
                {
                    mass.Tint = Solid(0xA9ACA8);
                    if (index % 2 == 0) mass.Cyl(s0 + 50, -(Rail + 45 + (float)rnd.NextDouble() * 30), Rnd(rnd, 3, 6), Rnd(rnd, 20, 45));
                    if (index % 6 == 3) mass.Cyl(s0 + 30, -(Rail + 70), 2, Rnd(rnd, 60, 100));
                }
            }
            // Skyline strip (900-1500 m): a few merged flat-coloured boxes behind the building line, at most 5 per side = 100 triangles.
            if (index >= 0 && track.Zones.At(mid) != Zone.Country && OpenRoad(s0, s1))
            {
                var rs = new System.Random(index * 7727 + 3);
                var zn = track.Zones.At(mid);
                foreach (int side in new[] { -1, 1 })
                    for (int i = 0; i < 5; i++)
                    {
                        float h = zn == Zone.City ? Rnd(rs, 25, 112) : zn == Zone.Industrial ? Rnd(rs, 8, 15) : Rnd(rs, 8, 16);
                        float tone = Rnd(rs, 0x7E, 0x8E);
                        sky.Tint = new Color32((byte)tone, (byte)(tone + 12), (byte)(tone + 22), 7 * 32);
                        double sPos = s0 + 10 + i * 18 + rs.NextDouble() * 6;
                        sky.Block(sPos, side * (Rail + Rnd(rs, 70, 220)), Rnd(rs, 14, 30), Rnd(rs, 20, 40), h, sky.Tint, Roof.Flat, sky.Tint, 0);
                    }
            }
            Fill(chunk.transform.Find("Mass").gameObject, mass);
            Fill(chunk.transform.Find("Detail").gameObject, detail);
            Fill(chunk.transform.Find("Sky").gameObject, sky);
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

        // The ramp climbs at 5 % from 150 m on, up to the flyover level +6.3 m (urban-spec 4).
        static float RampY(float t) => 0.01f + Mathf.Clamp(0.05f * (t - 150), 0, 6.3f);

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
                    float ya = RampY(Mathf.Min(ta, RampLen)), yb = RampY(Mathf.Min(tb, RampLen)), ym = (ya + yb) / 2;
                    b.Ribbon(s, e, -ca, -cb, wa, wb, ya, yb, (int)M.Asphalt, 2.08f);
                    b.Ribbon(s, e, -(ca - wa / 2 + 0.2f), -(cb - wb / 2 + 0.2f), 0.15f, 0.15f, ya + 0.015f, yb + 0.015f, (int)M.Paint, 0);
                    b.Ribbon(s, e, -(ca + wa / 2 - 0.2f), -(cb + wb / 2 - 0.2f), 0.15f, 0.15f, ya + 0.015f, yb + 0.015f, (int)M.Paint, 0);
                    if (ym > 0.5f && wa > 3) // elevated: deck slab, barriers, and a pier every 28 m
                    {
                        double sm = (s + e) / 2;
                        b.Box(sm, -(ca + cb) / 2, ym - 0.75f, 4.05f, (wa + wb) / 2, 1.5f, (int)M.Concrete);
                        foreach (float sg in new[] { -1f, 1f }) b.Box(sm, -((ca + cb) / 2 + sg * (wa + wb) / 4), ym + 0.35f, 4.05f, 0.3f, 0.7f, (int)M.Concrete);
                        if (ym > 1.5f && (int)(ta / 28) != (int)(tb / 28)) b.Box(sm, -(ca + cb) / 2, ym / 2 - 0.75f, 1.8f, 1.8f, ym * 1f, (int)M.Concrete);
                    }
                    if (ta < RampLen && tb >= RampLen) // the end: a small roundabout for the local street
                        b.Pad((s + e) / 2, -(Rail + 60 + extra + RampW / 2), 14, yb, (int)M.Asphalt);
                }
            }
        }

        partial class Builder
        {
            // A building: facade on the four sides (UVs in metres / tile, slice in the vertex colour), a roof of the given kind.
            public void Block(double s, float d, float length, float width, float height, Color32 wallTint, Roof roof, Color32 roofTint, float uOff)
            {
                Frame(s, out var p, out var r, out var f);
                var c = p + r * d + Vector3.up * (height / 2 - 0.3f);
                Vector3 x = r * width / 2, z = f * length / 2, u = Vector3.up * (height + 0.3f) / 2;
                var o = new[] { c - x - u - z, c + x - u - z, c + x + u - z, c - x + u - z, c - x - u + z, c + x - u + z, c + x + u + z, c - x + u + z };
                int[,] sides = { { 0, 3, 2, 1 }, { 5, 6, 7, 4 }, { 4, 7, 3, 0 }, { 1, 2, 6, 5 } };
                float v = (height + 0.3f) / TileH;
                Tint = wallTint;
                for (int i = 0; i < 4; i++)
                {
                    float uu = (i < 2 ? width : length) / TileW, a = uOff;
                    Quad(o[sides[i, 0]], o[sides[i, 1]], o[sides[i, 2]], o[sides[i, 3]], Sub, new Vector2(a, 0), new Vector2(a, v), new Vector2(a + uu, v), new Vector2(a + uu, 0));
                }
                Tint = roofTint;
                if (roof == Roof.Flat) { Quad(o[3], o[7], o[6], o[2], Sub, Vector2.zero, Vector2.up, Vector2.one, Vector2.right); return; }
                // gable or low pitch: ridge along the road
                float rise = roof == Roof.Gable ? width * 0.3f : width * 0.05f;
                Vector3 ridge = Vector3.up * rise, t0 = o[3], t1 = o[2], t2 = o[6], t3 = o[7], r0 = (o[3] + o[2]) / 2 + ridge, r1 = (o[7] + o[6]) / 2 + ridge;
                Quad(t0, t3, r1, r0, Sub, Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
                Quad(r0, r1, t2, t1, Sub, Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
                Tint = wallTint; // gable ends are wall
                Quad(t0, r0, t1, t1, Sub, new Vector2(0, 0), new Vector2(0.5f * width / TileW, rise / TileH), new Vector2(width / TileW, 0), new Vector2(width / TileW, 0));
                Quad(t2, r1, t3, t3, Sub, new Vector2(0, 0), new Vector2(0.5f * width / TileW, rise / TileH), new Vector2(width / TileW, 0), new Vector2(width / TileW, 0));
            }

            // Silo, tank or chimney: an 8-sided prism with a flat cap, in the current solid Tint.
            public void Cyl(double s, float d, float radius, float height)
            {
                Frame(s, out var p, out var r, out var f);
                var c = p + r * d;
                const int N = 8;
                for (int i = 0; i < N; i++)
                {
                    float a0 = 2 * Mathf.PI * i / N, a1 = 2 * Mathf.PI * (i + 1) / N;
                    Vector3 q0 = (r * Mathf.Cos(a0) + f * Mathf.Sin(a0)) * radius, q1 = (r * Mathf.Cos(a1) + f * Mathf.Sin(a1)) * radius;
                    Quad(c + q0 - Vector3.up * 0.3f, c + q0 + Vector3.up * height, c + q1 + Vector3.up * height, c + q1 - Vector3.up * 0.3f, Sub, Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
                    Quad(c + Vector3.up * height, c + q1 + Vector3.up * height, c + q0 + Vector3.up * height, c + q0 + Vector3.up * height, Sub, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                }
            }

            // Flat ribbon from s0 to s1 whose centre and width change linearly (ramps). World UVs when tile > 0.
            public void Ribbon(double s0, double s1, float c0, float c1, float w0, float w1, float y, float y1, int sub, float tile)
            {
                if (w0 <= 0.01f && w1 <= 0.01f) return;
                float t = tile > 0 ? tile : 1;
                float l0 = Mathf.Min(c0 - w0 / 2, c0 + w0 / 2), r0 = Mathf.Max(c0 - w0 / 2, c0 + w0 / 2), l1 = Mathf.Min(c1 - w1 / 2, c1 + w1 / 2), r1 = Mathf.Max(c1 - w1 / 2, c1 + w1 / 2);
                Quad(At(s0, l0, y), At(s1, l1, y1), At(s1, r1, y1), At(s0, r0, y), sub,
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
