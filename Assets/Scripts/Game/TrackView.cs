using System.Collections.Generic;
using Geisterfahrer.Core;
using TMPro;
using UnityEngine;

namespace Geisterfahrer.Game
{
    // Draws the procedural road (GDD 6) along the Track's centre line in 100 m chunks, about 1.5 km ahead.
    // Core stays in road space (s, d); Place() is the only road-to-world mapping.
    public class TrackView : MonoBehaviour
    {
        const float ChunkLen = 100, Ahead = 1500, Behind = 100, Rail = (float)Road.RailX, Lanes = (float)(Road.LaneCount * Road.LaneWidth / 2);
        // Tunnel bore: vertical walls to Spring on the rail lines, then a half-ellipse vault of rise VaultH (GDD 6.4); earth Cover over it.
        const float Spring = 1.0f, VaultH = 6.5f, Cover = 1.2f;
        enum M { Asphalt, Paint, Steel, Grass, White, Black, Concrete, SignBack, Lamp, Field0, Field1, Field2, Field3, Count }

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
        }

        GameObject Build(int index)
        {
            var go = spare.Count > 0 ? spare.Pop() : NewChunk();
            go.SetActive(true);
            foreach (var t in go.GetComponentsInChildren<TMP_Text>(true)) t.transform.parent.gameObject.SetActive(false);
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
                {
                    b.Arch(s, e, Rail, VaultH, Spring, (int)M.Concrete, true, 0);                 // the bore, seen from inside
                    b.Arch(s, e, Rail + Cover, VaultH + Cover, Spring, (int)M.Grass, false, 3f);  // earth mound over it, seen from outside
                }
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
                // Ground follows the road (and the valley under a viaduct) with both strip ends at their own height, so no steps.
                float inner = kind == Section.Tunnel ? Rail + Cover : Rail;
                float g0 = -Depth(s, kind) - 0.05f, g1 = -Depth(e, kind) - 0.05f;
                b.Strip(s, e, -40, -inner, g0, (int)M.Grass, 3f, false, g1);
                b.Strip(s, e, inner, 40, g0, (int)M.Grass, 3f, false, g1);
                b.Strip(s, e, -400, -40, g0 - 0.05f, fieldL, 0, false, g1 - 0.05f);
                b.Strip(s, e, 40, 400, g0 - 0.05f, fieldR, 0, false, g1 - 0.05f);
            }

            for (double s = Mathf.Ceil((float)(s0 / 2)) * 2; s < s1; s += 2)
            {
                var kind = track.KindAt(s);
                if (kind != Section.Tunnel) // guardrail posts every 2 m
                    foreach (int side in new[] { -1, 1 }) b.Box(s, side * (Rail + 0.12f), 0.375f, 0.05f, 0.1f, 0.75f, (int)M.Steel);
                if (kind == Section.Tunnel && s % (TunnelZone(s) ? 4 : 12) < 0.01) b.Box(s, 0, Spring + VaultH - 0.1f, 0.6f, 1.2f, 0.08f, (int)M.Lamp);
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

            // Tunnel portals: a concrete ring around each opening, entry and exit.
            foreach (var st in track.Structures)
                if (st.kind == Section.Tunnel)
                    foreach (double ps in new[] { st.s0, st.s1 })
                        if (ps >= s0 && ps < s1)
                            b.Ring(ps, Rail, VaultH, Spring, Cover, (int)M.Concrete); // portal ring around the opening

            // Overpasses: deck 12 m wide, 4.7 m clearance, piers outside the rails and in the median.
            foreach (double os in track.Overpasses)
                if (os >= s0 && os < s1)
                {
                    b.Box(os, 0, 5.3f, 12, 60, 1.2f, (int)M.Concrete);
                    foreach (float d in new[] { -Rail - 2, Rail + 2, 0f })
                        if (d != 0) b.Box(os, d, 2.35f, 1.2f, 1.2f, 4.7f, (int)M.Concrete);
                }

            b.Apply(go.GetComponent<MeshFilter>().sharedMesh);

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

        GameObject NewChunk()
        {
            var go = new GameObject("Chunk", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterials = mats;
            return go;
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
        class Builder
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
            static List<Vector2> Section2D(float a, float rise, float spring)
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
