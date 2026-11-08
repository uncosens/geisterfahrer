using System.Collections.Generic;
using Geisterfahrer.Core;
using TMPro;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Draws the procedural road (GDD 6) along the Track's centre line in 100 m chunks, about 1.5 km ahead.
    // Core stays in road space (s, d); Place() is the only road-to-world mapping.
    public partial class TrackView : MonoBehaviour
    {
        const float ChunkLen = 100, Ahead = 1500, Behind = 100, Rail = (float)Road.RailX, Lanes = (float)(Road.LaneCount * Road.LaneWidth / 2);
        // Tunnel bore: vertical walls to Spring on the rail lines, then a half-ellipse vault of rise VaultH (GDD 6.4); earth Cover over it.
        const float Spring = 1.0f, VaultH = 6.5f, Cover = 1.2f;
        // Mound tunnel surroundings (decision 47): the hill over the bore is HillH high and eases in over HillRamp m; the road runs in a cutting
        // with concrete wing walls (WingH, over Wing m) and 1 : SlopeRun earth slopes; the hill is wide and soft (flat to LatFlat, gone at LatEnd).
        const float HillH = Spring + VaultH + Cover, HillRamp = 300, WallX = Rail + Cover, WingH = 4.5f, Wing = 40, SlopeRun = 2.5f, LatFlat = 20, LatEnd = 130;
        // Underground box (decision 46): inside half-width, clear height, roof slab thickness (top at Track.UgRoof).
        const float BoxX = Rail + 0.5f, BoxH = 6.0f;
        static readonly float[] Bands = { WallX, 14, 20, 28, 40, 55, 75, 100, 130, 400 }; // |d| edges of the hill grid
        enum M { Asphalt, Paint, Steel, Grass, White, Black, Concrete, SignBack, Lamp, Field0, Field1, Field2, Field3, Rock, Tree, Trunk, Roof, Blue, Red, Facade0, Facade1, Facade2, Facade3, Count }

        Track track;
        Material[] mats;
        readonly Dictionary<int, GameObject> chunks = new Dictionary<int, GameObject>();
        readonly Stack<GameObject> spare = new Stack<GameObject>();
        readonly List<TMP_Text[]> kmPosts = new List<TMP_Text[]>();
        TMP_FontAsset signFont;
        public static double S; // player road distance, set each frame
        static TrackView self;

        public void Init(Art art, Track t)
        {
            self = this;
            track = t;
            signFont = art.sign;
            mats = new Material[(int)M.Count];
            mats[(int)M.Asphalt] = Placeholder.CalmAsphalt(2 * (Rail + 0.1f), 2.08f); // matches the road Strip below
            mats[(int)M.Paint] = Placeholder.Mat(Placeholder.Hex(0xE9E9E4), 0, 0.35f);
            mats[(int)M.Steel] = Placeholder.Mat(Placeholder.Hex(0x959BA1), 0.2f, 0.25f); // matte steel: bright metal mirrored the sky (blue-violet rails)
            mats[(int)M.Grass] = art.grass ? art.grass : Placeholder.Mat(Placeholder.Hex(0x6F8A3E), 0, 0.1f);
            mats[(int)M.White] = Placeholder.Mat(Placeholder.Hex(0xF2F2F0), 0, 0.3f);
            mats[(int)M.Black] = Placeholder.Mat(Placeholder.Hex(0x1A1A1A), 0, 0.3f);
            mats[(int)M.Concrete] = Placeholder.Mat(Placeholder.Hex(0x7A7C80), 0, 0.05f); // mid-grey matte concrete (overpass, tunnel, viaduct)
            mats[(int)M.SignBack] = Placeholder.Mat(Placeholder.Hex(0x8B9097), 0.3f, 0.4f);
            mats[(int)M.Lamp] = Placeholder.Mat(Placeholder.Hex(0xFFF6E0), 0, 0.9f);
            mats[(int)M.Rock] = Placeholder.Mat(Placeholder.Hex(0x6E675C), 0, 0.05f);   // earth / rock face at portals
            mats[(int)M.Tree] = Placeholder.Mat(Placeholder.Hex(0x3F5E2B), 0, 0.05f);
            mats[(int)M.Trunk] = Placeholder.Mat(Placeholder.Hex(0x4E3B2B), 0, 0.05f);
            mats[(int)M.Roof] = Placeholder.Mat(Placeholder.Hex(0x8A4B3A), 0, 0.1f);
            mats[(int)M.Blue] = Placeholder.Mat(Placeholder.Hex(0x1C5AA8), 0, 0.3f);
            mats[(int)M.Red] = Placeholder.Mat(Placeholder.Hex(0xC4262E), 0, 0.3f);
            for (int i = 0; i < 4; i++) mats[(int)M.Facade0 + i] = FacadeMat(i);
            sceneryOf.Clear();
            int[] fields = { 0x6F8A3E, 0xC8B46A, 0x8A7556, 0x7E9A4A };
            for (int i = 0; i < 4; i++) mats[(int)M.Field0 + i] = Placeholder.Mat(Dim(Placeholder.Hex(fields[i]), 0.85f), 0, 0.05f);
            foreach (var c in chunks.Values) Destroy(c);
            chunks.Clear();
        }

        static Color Dim(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, 1);

        // ---- road space to world ----

        static void Frame(double s, out Vector3 p, out Vector3 right, out Vector3 fwd)
        {
            var t = self.track;
            double f = s / Track.SampleStep;
            int i = Mathf.Clamp((int)f, 0, t.X.Count - 2);
            float k = (float)(f - i);
            p = Vector3.Lerp(new Vector3((float)t.X[i], (float)t.Y[i], (float)t.Z[i]), new Vector3((float)t.X[i + 1], (float)t.Y[i + 1], (float)t.Z[i + 1]), k);
            float h = Mathf.Lerp((float)t.Heading[i], (float)t.Heading[i + 1], k);
            fwd = new Vector3(Mathf.Sin(h), 0, Mathf.Cos(h));
            right = new Vector3(fwd.z, 0, -fwd.x);
            if (s < 0) p += fwd * (float)s; // behind the start (camera at run start): straight line back
        }

        // zAhead: road distance ahead of the player; d: side offset; y: height above the road.
        public static Vector3 Place(float zAhead, float d, float y = 0) => At(S + zAhead, d, y);

        public static Vector3 At(double s, float d, float y = 0)
        {
            Frame(s, out var p, out var r, out _);
            return p + r * d + Vector3.up * y;
        }

        // Road direction at zAhead, with the road's grade as pitch.
        public static Quaternion Facing(float zAhead)
        {
            Frame(S + zAhead, out _, out _, out var f);
            float pitch = -Mathf.Atan((float)self.track.Grade(S + zAhead)) * Mathf.Rad2Deg;
            return Quaternion.LookRotation(f) * Quaternion.Euler(pitch, 0, 0);
        }

        // ---- chunks ----

        public void Scroll(double s)
        {
            S = s;
            int first = Mathf.FloorToInt((float)((s - Behind) / ChunkLen)), last = Mathf.FloorToInt((float)((s + Ahead) / ChunkLen));
            var drop = new List<int>();
            foreach (var k in chunks.Keys) if (k < first || k > last) drop.Add(k);
            foreach (var k in drop) { chunks[k].SetActive(false); spare.Push(chunks[k]); chunks.Remove(k); }
            for (int k = Mathf.Max(-1, first); k <= last; k++) // chunk -1: road behind the start, seen at standstill
                if (!chunks.ContainsKey(k) && (k + 1) * ChunkLen < track.Generated - 10) chunks[k] = Build(k);
            UpdateScenery(s);
        }

        GameObject Build(int index)
        {
            var go = spare.Count > 0 ? spare.Pop() : NewChunk();
            go.SetActive(true);
            ResetScenery(go);
            foreach (var t in go.GetComponentsInChildren<TMP_Text>(true)) Destroy(t.transform.parent.gameObject); // posts and signs of the chunk's previous use
            var b = new Builder();
            double s0 = index * ChunkLen, s1 = s0 + ChunkLen;
            var rnd = new System.Random(index * 7919);
            int fieldL = (int)M.Field0 + rnd.Next(4), fieldR = (int)M.Field0 + rnd.Next(4);

            for (double s = s0; s < s1 - 0.01; s += 4)
            {
                double e = s + 4, mid = s + 2;
                var kind = track.KindAt(mid);
                // Road surface with world-anchored UVs (1 tile per 2.08 m), edge lines.
                b.Strip(s, e, -Rail - 0.1f, Rail + 0.1f, 0, (int)M.Asphalt, 2.08f);
                b.Strip(s, e, -Lanes - 0.3f, -Lanes, 0.012f, (int)M.Paint, 0);
                b.Strip(s, e, Lanes, Lanes + 0.3f, 0.012f, (int)M.Paint, 0);

                if (kind == Section.Tunnel)
                    b.Arch(s, e, Rail, VaultH, Spring, (int)M.Concrete, true, 0); // the bore, seen from inside
                else
                {
                    foreach (int side in new[] { -1, 1 })
                        b.Wall(s, e, side * (Rail + 0.04f), 0.395f, 0.705f, (int)M.Steel);
                    if (kind == Section.Viaduct)
                    {
                        foreach (int side in new[] { -1, 1 }) b.Box(mid, side * (Rail + 0.35f), 0.45f, 4, 0.3f, 0.9f, (int)M.Concrete);
                        b.Box(mid, 0, -1.05f, 4, Rail * 2 + 1, 1.8f, (int)M.Concrete); // deck slab, top 15 cm under the road (no z-fight)
                    }
                }
                if (kind == Section.Underground) UndergroundSegment(b, s, e, fieldL, fieldR);
                else if (Hill(s) > 0.001f || Hill(e) > 0.001f) HillGround(b, s, e, kind == Section.Tunnel, fieldL, fieldR);
                else
                {
                    // Ground follows the road (and the valley under a viaduct) with both strip ends at their own height, so no steps.
                    float g0 = -Depth(s, kind) - 0.05f, g1 = -Depth(e, kind) - 0.05f;
                    b.Strip(s, e, -40, -Rail, g0, (int)M.Grass, 3f, false, g1);
                    b.Strip(s, e, Rail, 40, g0, (int)M.Grass, 3f, false, g1);
                    b.Strip(s, e, -400, -40, g0 - 0.05f, fieldL, 0, false, g1 - 0.05f);
                    b.Strip(s, e, 40, 400, g0 - 0.05f, fieldR, 0, false, g1 - 0.05f);
                }
            }

            for (double s = Mathf.Ceil((float)(s0 / 2)) * 2; s < s1; s += 2)
            {
                var kind = track.KindAt(s);
                if (kind != Section.Tunnel) // guardrail posts every 2 m
                    foreach (int side in new[] { -1, 1 }) b.Box(s, side * (Rail + 0.12f), 0.375f, 0.05f, 0.1f, 0.75f, (int)M.Steel);
                if (kind == Section.Tunnel && s % (TunnelZone(s) ? 4 : 12) < 0.01) b.Box(s, 0, Spring + VaultH - 0.1f, 0.6f, 1.2f, 0.08f, (int)M.Lamp);
                if (kind == Section.Underground && track.Covered(s) && s % 8 < 0.01)
                    foreach (int side in new[] { -1, 1 }) b.Box(s, side * 3.5f, BoxH - 0.1f, 0.6f, 1.2f, 0.08f, (int)M.Lamp);
                if (kind == Section.Viaduct && s % 40 < 0.01)
                    foreach (int side in new[] { -1, 1 }) b.Box(s, side * 5, -ValleyDepth(s) / 2, 2.5f, 2.5f, ValleyDepth(s), (int)M.Concrete);
                // Chevron boards every 20 m on the outer side of curves under 1000 m, seen from behind (grey back, white frame).
                double kappa = track.Curvature(s);
                if (kind == Section.Open && System.Math.Abs(kappa) > 1 / 1000.0 && s % 20 < 0.01)
                {
                    float d = -Mathf.Sign((float)kappa) * (Rail + 0.6f);
                    b.Box(s, d, 1.4f, 0.04f, 0.56f, 0.81f, (int)M.White);
                    b.Box(s - 0.03, d, 1.4f, 0.04f, 0.5f, 0.75f, (int)M.SignBack);
                    b.Box(s, d, 0.5f, 0.05f, 0.06f, 1f, (int)M.SignBack);
                }
            }

            // Lane dashes by road distance: 6 m line, 12 m gap.
            for (double s = Mathf.Ceil((float)(s0 / 18)) * 18; s < s1; s += 18)
                for (int i = 1; i < Road.LaneCount; i++)
                {
                    float d = (float)(Road.LaneX(i) - Road.LaneWidth / 2);
                    b.Strip(s, s + 6, d - 0.075f, d + 0.075f, 0.012f, (int)M.Paint, 0);
                }

            // Delineators every 50 m (white post, black band), outside the rails on open road.
            for (double s = Mathf.Ceil((float)(s0 / 50)) * 50; s < s1; s += 50)
                if (track.KindAt(s) == Section.Open)
                    foreach (int side in new[] { -1, 1 })
                    {
                        b.Box(s, side * (Rail + 0.5f), 0.5f, 0.1f, 0.12f, 1f, (int)M.White);
                        b.Box(s, side * (Rail + 0.5f), 0.725f, 0.105f, 0.125f, 0.25f, (int)M.Black);
                    }

            // Tunnel portals and their surroundings.
            signs.Clear();
            foreach (var st in track.Structures)
            {
                if (st.s1 < s0 - 400 || st.s0 > s1 + 400) continue;
                if (st.kind == Section.Tunnel)
                    foreach (double ps in new[] { st.s0, st.s1 })
                        if (ps >= s0 && ps < s1) { b.Ring(ps, Rail, VaultH, Spring, Cover, (int)M.Concrete); PortalFace(b, ps, ps == st.s0); }
                if (st.kind == Section.Underground)
                {
                    foreach (double ps in RoofEnds(st))
                        if (ps >= s0 && ps < s1) b.Box(ps, 0, BoxH + 0.1f, 1f, BoxX * 2 + 0.8f, 1f, (int)M.Concrete); // lintel over the box mouth
                }
                if (st.kind == Section.Tunnel || st.kind == Section.Underground)
                    Furniture(b, st, s0, s1, rnd);
            }
            Scenery(b, s0, s1, index);

            // Overpasses: deck 12 m wide, 4.7 m clearance, piers outside the rails and in the median.
            foreach (double os in track.Overpasses)
                if (os >= s0 && os < s1)
                {
                    b.Box(os, 0, 5.3f, 12, 60, 1.2f, (int)M.Concrete);
                    foreach (float d in new[] { -Rail - 2, Rail + 2, 0f })
                        if (d != 0) b.Box(os, d, 2.35f, 1.2f, 1.2f, 4.7f, (int)M.Concrete);
                }

            b.Apply(go.GetComponent<MeshFilter>().sharedMesh);
            foreach (var sg in signs) SignText(go.transform, sg.s, sg.d, sg.y, sg.text);

            // Km posts every 500 m on the outer side, numbers on both faces (read from behind too).
            for (double s = Mathf.Ceil((float)(s0 / 500)) * 500; s < s1; s += 500)
                if (s > 0 && track.KindAt(s) == Section.Open) KmPost(go.transform, s);
            return go;
        }

        float Depth(double s, Section kind) => kind == Section.Viaduct ? Mathf.Max(3, ValleyDepth(s)) : 0; // valley at least 3 m below the deck

        float ValleyDepth(double s)
        {
            foreach (var st in track.Structures)
                if (st.kind == Section.Viaduct && s >= st.s0 && s < st.s1)
                {
                    float h = 20 + (float)(st.s0 % 60);
                    return h * Mathf.Sin(Mathf.PI * (float)((s - st.s0) / (st.s1 - st.s0)));
                }
            return 0;
        }

        bool TunnelZone(double s)
        {
            foreach (var st in track.Structures)
                if (st.kind == Section.Tunnel && s >= st.s0 && s < st.s1) return s - st.s0 < 100 || st.s1 - s < 100;
            return false;
        }

        // ---- mound tunnel surroundings ----

        static float Smooth(float x) { x = Mathf.Clamp01(x); return x * x * (3 - 2 * x); }
        static float Lat(float ad) => 1 - Smooth((ad - LatFlat) / (LatEnd - LatFlat));

        // Height of the mound crown over the road at s: eases in over HillRamp m before the portal, flat over the bore, eases out after.
        float Hill(double s)
        {
            float h = 0;
            foreach (var st in track.Structures)
            {
                if (st.kind != Section.Tunnel || s <= st.s0 - HillRamp || s >= st.s1 + HillRamp) continue;
                float u = s < st.s0 ? (float)(s - (st.s0 - HillRamp)) / HillRamp : s > st.s1 ? (float)(st.s1 + HillRamp - s) / HillRamp : 1;
                h = Mathf.Max(h, HillH * Smooth(u));
            }
            return h;
        }

        // Wing wall height in the cutting: WingH at the portal, easing to 0 over Wing m.
        float Wh(double s)
        {
            float h = 0;
            foreach (var st in track.Structures)
            {
                if (st.kind != Section.Tunnel) continue;
                double dist = s <= st.s0 ? st.s0 - s : s >= st.s1 ? s - st.s1 : double.MaxValue;
                if (dist < Wing) h = Mathf.Max(h, WingH * (1 - Smooth((float)(dist / Wing))));
            }
            return h;
        }

        // Ground height over the road at |d|: the mound, cut by the cutting outside the bore.
        float Gh(double s, float ad, bool inside)
        {
            float h = Hill(s) * Lat(ad);
            return inside || h <= 0 ? h : Mathf.Min(h, Wh(s) + (ad - WallX) / SlopeRun);
        }

        void HillGround(Builder b, double s, double e, bool inside, int fieldL, int fieldR)
        {
            if (inside) b.Strip(s, e, -WallX, WallX, Hill(s) - 0.05f, (int)M.Grass, 3f, false, Hill(e) - 0.05f); // earth over the bore
            else
            {
                b.Strip(s, e, -WallX, -Rail, -0.05f, (int)M.Grass, 3f);
                b.Strip(s, e, Rail, WallX, -0.05f, (int)M.Grass, 3f);
                float w0 = Wh(s), w1 = Wh(e);
                if (w0 > 0.02f || w1 > 0.02f)
                    foreach (int side in new[] { -1, 1 }) b.Side(s, e, side * WallX, w0, w1, (int)M.Concrete); // wing walls
            }
            foreach (int side in new[] { -1, 1 })
                for (int k = 0; k + 1 < Bands.Length; k++)
                {
                    float d0 = Bands[k], d1 = Bands[k + 1];
                    int sub = d1 <= 40 ? (int)M.Grass : side < 0 ? fieldL : fieldR;
                    float lift = d1 <= 40 ? 0.05f : 0.1f;
                    b.Patch(s, e, side, d0, d1, Gh(s, d0, inside) - lift, Gh(e, d0, inside) - lift, Gh(s, d1, inside) - lift, Gh(e, d1, inside) - lift, sub, d1 <= 40 ? 3f : 0);
                }
        }

        // Earth and rock face of the hill at a portal plane, around the concrete ring.
        void PortalFace(Builder b, double P, bool entry)
        {
            var outer = Builder.Section2D(Rail + Cover, VaultH + Cover, Spring);
            for (int j = 0; j + 1 < outer.Count; j++)
                if (Mathf.Abs(outer[j].x - outer[j + 1].x) > 1e-4f)
                    b.Face(P, outer[j].x, outer[j].y, outer[j].x, HillH, outer[j + 1].x, HillH, outer[j + 1].x, outer[j + 1].y, (int)M.Rock);
            foreach (int side in new[] { -1, 1 })
                for (int k = 0; k + 1 < Bands.Length; k++)
                {
                    float d0 = Bands[k], d1 = Bands[k + 1];
                    float b0 = Gh(P, d0, false), b1 = Gh(P, d1, false), t0 = Gh(P, d0, true), t1 = Gh(P, d1, true);
                    if (t0 - b0 < 0.01f && t1 - b1 < 0.01f) continue;
                    b.Face(P, side * d0, b0, side * d0, t0, side * d1, t1, side * d1, b1, (int)M.Rock);
                }
        }

        // ---- underground box ----

        void UndergroundSegment(Builder b, double s, double e, int fieldL, int fieldR)
        {
            float g0 = (float)track.Depth(s), g1 = (float)track.Depth(e);
            bool roof = track.Depth(s + 2) >= Track.UgRoof;
            foreach (int side in new[] { -1, 1 })
                b.Side(s, e, side * BoxX, roof ? BoxH : g0, roof ? BoxH : g1, (int)M.Concrete); // retaining or box wall
            b.Strip(s, e, Rail, BoxX, -0.02f, (int)M.Concrete, 2f);
            b.Strip(s, e, -BoxX, -Rail, -0.02f, (int)M.Concrete, 2f);
            if (roof)
            {
                b.Strip(s, e, -BoxX, BoxX, BoxH, (int)M.Concrete, 0, true);                              // ceiling
                b.Strip(s, e, -40, 40, g0 - 0.05f, (int)M.Grass, 3f, false, g1 - 0.05f);                 // terrain over the roof
            }
            else
            {
                b.Strip(s, e, -40, -BoxX, g0 - 0.05f, (int)M.Grass, 3f, false, g1 - 0.05f);
                b.Strip(s, e, BoxX, 40, g0 - 0.05f, (int)M.Grass, 3f, false, g1 - 0.05f);
            }
            b.Strip(s, e, -400, -40, g0 - 0.1f, fieldL, 0, false, g1 - 0.1f);
            b.Strip(s, e, 40, 400, g0 - 0.1f, fieldR, 0, false, g1 - 0.1f);
        }

        // Where the roof starts and ends (multiples of 4 m).
        IEnumerable<double> RoofEnds((double s0, double s1, Section kind) st)
        {
            double a = st.s0; while (a < st.s1 && track.Depth(a + 2) < Track.UgRoof) a += 4;
            double z = st.s1; while (z > st.s0 && track.Depth(z - 2) < Track.UgRoof) z -= 4;
            yield return a; yield return z;
        }

        // ---- furniture and scenery ----

        readonly List<(double s, float d, float y, string text)> signs = new List<(double, float, float, string)>();

        // Lamp rows, tunnel and speed signs, a control building: for one structure, the part in chunk [c0, c1).
        void Furniture(Builder b, (double s0, double s1, Section kind) st, double c0, double c1, System.Random rnd)
        {
            if (st.kind == Section.Tunnel)
                foreach (var (anchor, dir) in new[] { (st.s0, -1), (st.s1, 1) })
                    for (int k = 0; k < 6; k++)
                    {
                        double s = anchor + dir * (20 + 25 * k);
                        if (s < c0 || s >= c1) continue;
                        foreach (int side in new[] { -1, 1 })
                        {
                            b.Box(s, side * (Rail + 0.7f), 3.25f, 0.12f, 0.12f, 6.5f, (int)M.SignBack);
                            b.Box(s, side * (Rail + 0.2f), 6.55f, 0.5f, 1.2f, 0.1f, (int)M.Lamp);
                        }
                    }
            double ss = Mathf.Floor((float)(st.s0 - (st.kind == Section.Tunnel ? 170 : 110)) / 4) * 4;
            if (ss >= c0 && ss < c1)
            {
                float d = Rail + 0.9f;
                b.Box(ss, d, 2.6f, 0.12f, 0.12f, 5.2f, (int)M.SignBack);
                b.Box(ss - 0.05, d, 2.2f, 0.08f, 1.7f, 1.7f, (int)M.Blue);      // tunnel sign: white arch with a dark mouth
                b.Box(ss - 0.07, d, 2.2f, 0.04f, 1.1f, 1.1f, (int)M.White);
                b.Box(ss - 0.09, d, 2.05f, 0.04f, 0.6f, 0.7f, (int)M.Black);
                b.Disc(ss - 0.06, d, 4.1f, 0.85f, (int)M.Red);                   // speed limit: red ring, white face, number added as text
                b.Disc(ss - 0.07, d, 4.1f, 0.68f, (int)M.White);
                signs.Add((ss - 0.1, d, 4.1f, "80"));
            }
            if (st.kind == Section.Tunnel && ((int)(st.s0 / 4) & 1) == 0) // control building beside the entry portal on one side
            {
                double s = st.s0 - 40;
                if (s >= c0 && s < c1)
                {
                    float d = WallX + 30, y = Gh(s, d, false);
                    b.Box(s, d, y + 1.5f, 8, 6, 5.5f, (int)M.White);
                    b.Box(s, d, y + 4.4f, 8.6f, 6.6f, 0.4f, (int)M.Concrete);
                }
            }
        }

        // Trees on the mounds and on the ground over underground sections; a few houses over the underground box.
        void Scenery(Builder b, double c0, double c1, int index)
        {
            var rnd = new System.Random(index * 131 + 17);
            for (int i = 0; i < 12; i++)
            {
                double s = c0 + rnd.NextDouble() * (c1 - c0);
                float d = (rnd.Next(2) * 2 - 1) * (22 + (float)rnd.NextDouble() * 85), k = 0.8f + (float)rnd.NextDouble() * 0.6f;
                bool house = i < 2;
                float y = -1;
                var kind = track.KindAt(s);
                if (kind == Section.Underground) y = (float)track.Depth(s);
                else if (Hill(s) > 0.5f) y = Gh(s, Mathf.Abs(d), kind == Section.Tunnel);
                if (y < 0 || (kind == Section.Underground && Mathf.Abs(d) < BoxX + 4)) continue;
                if (house)
                {
                    if (kind != Section.Underground || Mathf.Abs(d) < 16) continue;
                    b.Box(s, d, y + 2.4f, 9, 7, 5, (int)M.White);
                    b.Box(s, d, y + 5.2f, 9.6f, 7.6f, 0.5f, (int)M.Roof);
                    continue;
                }
                b.Box(s, d, y + 1.1f * k, 0.4f * k, 0.4f * k, 2.2f * k, (int)M.Trunk);
                b.Box(s, d, y + 3.2f * k, 3f * k, 3f * k, 2.6f * k, (int)M.Tree);
                b.Box(s, d, y + 5.1f * k, 2f * k, 2f * k, 2f * k, (int)M.Tree);
            }
        }

        GameObject NewChunk()
        {
            var go = new GameObject("Chunk", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterials = mats;
            Child(go, "Mass"); Child(go, "Detail");
            return go;
        }

        // Number on the speed sign, readable from behind like the km posts.
        void SignText(Transform chunk, double s, float d, float y, string text)
        {
            var holder = new GameObject("SignHolder").transform;
            holder.SetParent(chunk, false);
            holder.position = At(s, d, y);
            Frame(s, out _, out _, out var f);
            holder.rotation = Quaternion.LookRotation(f);
            var t = new GameObject("Sign").AddComponent<TextMeshPro>();
            t.transform.SetParent(holder, false);
            t.rectTransform.sizeDelta = new Vector2(1.2f, 0.8f);
            if (signFont) t.font = signFont;
            t.fontSize = 6; t.color = Placeholder.Hex(0x111111);
            t.alignment = TextAlignmentOptions.Center;
            t.text = text;
        }

        void KmPost(Transform chunk, double s)
        {
            var post = new GameObject("KmPost").transform;
            post.SetParent(chunk, false);
            post.position = At(s, -(Rail + 1));
            Frame(s, out _, out _, out var f);
            post.rotation = Quaternion.LookRotation(f);
            Placeholder.Box(post, new Vector3(0, 0.6f, 0), new Vector3(0.08f, 1.2f, 0.08f), Placeholder.Hex(0x8B9097));
            Placeholder.Box(post, new Vector3(0, 1.35f, 0), new Vector3(0.53f, 0.38f, 0.05f), Placeholder.Hex(0x111111));
            Placeholder.Box(post, new Vector3(0, 1.35f, 0), new Vector3(0.5f, 0.35f, 0.06f), Placeholder.Hex(0xF4F4F2));
            foreach (int face in new[] { -1, 1 })
            {
                var t = new GameObject("Km").AddComponent<TextMeshPro>();
                t.transform.SetParent(post, false);
                t.transform.localPosition = new Vector3(0, 1.35f, face * 0.035f);
                t.transform.localRotation = Quaternion.Euler(0, face > 0 ? 180 : 0, 0);
                t.rectTransform.sizeDelta = new Vector2(0.5f, 0.35f);
                if (signFont) t.font = signFont;
                t.fontSize = 2.2f; t.color = Placeholder.Hex(0x111111);
                t.alignment = TextAlignmentOptions.Center;
                t.text = (s / 1000).ToString("F1");
            }
        }

        // Collects quads and boxes in road space into one mesh with a submesh per material.
        partial class Builder
        {
            readonly List<Vector3> v = new List<Vector3>();
            readonly List<Vector2> uv = new List<Vector2>();
            readonly List<Vector3> nrm = new List<Vector3>();
            readonly List<int>[] tris = new List<int>[(int)M.Count];

            public Builder() { for (int i = 0; i < tris.Length; i++) tris[i] = new List<int>(); }

            // normals: per-corner (smooth surfaces); null = the flat face normal.
            void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int sub, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud, bool twoSided = false, Vector3[] normals = null)
            {
                int n = v.Count;
                v.Add(a); v.Add(b); v.Add(c); v.Add(d);
                uv.Add(ua); uv.Add(ub); uv.Add(uc); uv.Add(ud);
                if (normals != null) nrm.AddRange(normals);
                else { var f = Vector3.Cross(b - a, c - a).normalized; nrm.Add(f); nrm.Add(f); nrm.Add(f); nrm.Add(f); }
                tris[sub].AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
                if (twoSided) Quad(a, d, c, b, sub, ua, ud, uc, ub); // the back face gets its own vertices, so its normals point the other way
            }

            // Cross-section of a bore from the left wall foot, up the wall, over the half-ellipse vault, down the right wall.
            public static List<Vector2> Section2D(float a, float rise, float spring)
            {
                var r = new List<Vector2> { new Vector2(-a, 0), new Vector2(-a, spring) };
                const int N = 24;
                for (int i = 1; i < N; i++)
                {
                    float th = Mathf.PI * (1 - i / (float)N);
                    r.Add(new Vector2(a * Mathf.Cos(th), spring + rise * Mathf.Sin(th)));
                }
                r.Add(new Vector2(a, spring)); r.Add(new Vector2(a, 0));
                return r;
            }

            // Tube along the road between s0 and s1; inward = surface faces the middle of the bore. tile > 0: UVs in metres / tile.
            public void Arch(double s0, double s1, float a, float rise, float spring, int sub, bool inward, float tile)
            {
                var r = Section2D(a, rise, spring);
                float t = tile > 0 ? tile : 1, v0 = 0;
                // Smooth normals: outward direction of the section at each ring point (flipped for the inside of the bore).
                Frame(s0, out _, out var r0, out _);
                Frame(s1, out _, out var r1, out _);
                Vector3 Nrm(Vector3 right, int j)
                {
                    var tan = (r[Mathf.Min(j + 1, r.Count - 1)] - r[Mathf.Max(j - 1, 0)]).normalized;
                    var o = new Vector2(-tan.y, tan.x) * (inward ? -1 : 1);
                    return (right * o.x + Vector3.up * o.y).normalized;
                }
                for (int j = 0; j + 1 < r.Count; j++)
                {
                    float v1 = v0 + Vector2.Distance(r[j], r[j + 1]);
                    Vector3 A = At(s0, r[j].x, r[j].y), B = At(s1, r[j].x, r[j].y), C = At(s1, r[j + 1].x, r[j + 1].y), D = At(s0, r[j + 1].x, r[j + 1].y);
                    Vector3 nA = Nrm(r0, j), nB = Nrm(r1, j), nC = Nrm(r1, j + 1), nD = Nrm(r0, j + 1);
                    Vector2 ua = new Vector2((float)s0 / t, v0 / t), ub = new Vector2((float)s1 / t, v0 / t), uc = new Vector2((float)s1 / t, v1 / t), ud = new Vector2((float)s0 / t, v1 / t);
                    if (inward) Quad(A, D, C, B, sub, ua, ud, uc, ub, false, new[] { nA, nD, nC, nB });
                    else Quad(A, B, C, D, sub, ua, ub, uc, ud, false, new[] { nA, nB, nC, nD });
                    v0 = v1;
                }
            }

            // Flat ring face at s between the bore and the mound (portal), visible from both sides.
            public void Ring(double s, float a, float rise, float spring, float cover, int sub)
            {
                var inner = Section2D(a, rise, spring);
                var outer = Section2D(a + cover, rise + cover, spring);
                for (int j = 0; j + 1 < inner.Count; j++)
                    Quad(At(s, inner[j].x, inner[j].y), At(s, outer[j].x, outer[j].y), At(s, outer[j + 1].x, outer[j + 1].y), At(s, inner[j + 1].x, inner[j + 1].y),
                        sub, Vector2.zero, Vector2.up, Vector2.one, Vector2.right, true);
            }

            // Vertical two-sided wall at side offset d, height hs at s0 and he at s1, from y = 0.
            public void Side(double s0, double s1, float d, float hs, float he, int sub)
                => Quad(At(s0, d, 0), At(s0, d, hs), At(s1, d, he), At(s1, d, 0), sub, Vector2.zero, Vector2.up, Vector2.one, Vector2.right, true);

            // Vertical two-sided quad in the plane across the road at s; corners are (d, y) pairs.
            public void Face(double s, float da, float ya, float db, float yb, float dc, float yc, float dd, float yd, int sub)
                => Quad(At(s, da, ya), At(s, db, yb), At(s, dc, yc), At(s, dd, yd), sub, Vector2.zero, Vector2.up, Vector2.one, Vector2.right, true);

            // Ground patch between |d| d0 < d1 on one side, with the height at each corner (s then e, inner then outer). World UVs when tile > 0.
            public void Patch(double s0, double s1, int side, float d0, float d1, float h00, float h10, float h01, float h11, int sub, float tile)
            {
                float t = tile > 0 ? tile : 1;
                if (side > 0)
                    Quad(At(s0, d0, h00), At(s1, d0, h10), At(s1, d1, h11), At(s0, d1, h01), sub,
                        new Vector2(d0 / t, (float)(s0 / t)), new Vector2(d0 / t, (float)(s1 / t)), new Vector2(d1 / t, (float)(s1 / t)), new Vector2(d1 / t, (float)(s0 / t)));
                else
                    Quad(At(s0, -d1, h01), At(s1, -d1, h11), At(s1, -d0, h10), At(s0, -d0, h00), sub,
                        new Vector2(-d1 / t, (float)(s0 / t)), new Vector2(-d1 / t, (float)(s1 / t)), new Vector2(-d0 / t, (float)(s1 / t)), new Vector2(-d0 / t, (float)(s0 / t)));
            }

            // Upright disc facing along the road, seen from both sides.
            public void Disc(double s, float d, float y, float radius, int sub)
            {
                Frame(s, out var p, out var r, out _);
                var c = p + r * d + Vector3.up * y;
                const int N = 20;
                for (int i = 0; i < N; i++)
                {
                    float a0 = 2 * Mathf.PI * i / N, a1 = 2 * Mathf.PI * (i + 1) / N;
                    var p0 = c + (r * Mathf.Cos(a0) + Vector3.up * Mathf.Sin(a0)) * radius;
                    var p1 = c + (r * Mathf.Cos(a1) + Vector3.up * Mathf.Sin(a1)) * radius;
                    Quad(c, p0, p1, p1, sub, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, true);
                }
            }

            // Flat strip from s0 to s1 between side offsets d0 < d1 at height y. tile > 0: world UVs in metres / tile.
            public void Strip(double s0, double s1, float d0, float d1, float y, int sub, float tile, bool below = false, float y1 = float.NaN)
            {
                if (float.IsNaN(y1)) y1 = y;
                Vector3 a = At(s0, d0, y), b = At(s1, d0, y1), c = At(s1, d1, y1), d = At(s0, d1, y);
                float t = tile > 0 ? tile : 1;
                Vector2 ua = new Vector2(d0 / t, (float)(s0 / t)), ub = new Vector2(d0 / t, (float)(s1 / t)), uc = new Vector2(d1 / t, (float)(s1 / t)), ud = new Vector2(d1 / t, (float)(s0 / t));
                if (below) Quad(a, d, c, b, sub, ua, ud, uc, ub); else Quad(a, b, c, d, sub, ua, ub, uc, ud);
            }

            // Vertical two-sided wall along the road at side offset d from height y0 to y1.
            public void Wall(double s0, double s1, float d, float y0, float y1, int sub)
                => Quad(At(s0, d, y0), At(s0, d, y1), At(s1, d, y1), At(s1, d, y0), sub, Vector2.zero, Vector2.up, Vector2.one, Vector2.right, true);

            // Box centred at (s, d, y) in road space: length along the road, width across, height up.
            public void Box(double s, float d, float y, float length, float width, float height, int sub)
            {
                Frame(s, out var p, out var r, out var f);
                f = (f + Vector3.up * (float)self.track.Grade(s)).normalized; // follow the slope
                var c = p + r * d + Vector3.up * y;
                Vector3 x = r * width / 2, z = f * length / 2, u = Vector3.up * height / 2;
                var o = new[] { c - x - u - z, c + x - u - z, c + x + u - z, c - x + u - z, c - x - u + z, c + x - u + z, c + x + u + z, c - x + u + z };
                int[,] faces = { { 0, 3, 2, 1 }, { 5, 6, 7, 4 }, { 4, 7, 3, 0 }, { 1, 2, 6, 5 }, { 3, 7, 6, 2 }, { 4, 0, 1, 5 } };
                for (int i = 0; i < 6; i++)
                    Quad(o[faces[i, 0]], o[faces[i, 1]], o[faces[i, 2]], o[faces[i, 3]], sub, Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
            }

            public void Apply(Mesh m)
            {
                m.Clear();
                m.SetVertices(v);
                m.SetUVs(0, uv);
                m.subMeshCount = tris.Length;
                for (int i = 0; i < tris.Length; i++) m.SetTriangles(tris[i], i);
                m.SetNormals(nrm);
                m.RecalculateBounds();
            }
        }
    }
}
