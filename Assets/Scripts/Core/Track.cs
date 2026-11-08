using System;
using System.Collections.Generic;

namespace Geisterfahrer.Core
{
    public enum Section { Open, Tunnel, Viaduct, Underground, Trench, Interchange, Bridge }

    // Endless procedural road (GDD 6), generated ahead of the player as functions of road distance s:
    // curvature (1/R, + = curve to the player's right), grade (rise/run), structures and overpass points.
    // Also keeps a 2 m sampled centre line (x, y, z, heading) for the view. Core rules only use s and d.
    public class Track
    {
        public const double SampleStep = 2;
        const double Ahead = 2500;

        readonly List<double> hs = new List<double> { 0 }, hk = new List<double> { 0 };   // curvature knots
        readonly List<double> vs = new List<double> { 0 }, vg = new List<double> { 0 };   // grade knots
        public readonly List<(double s0, double s1, Section kind)> Structures = new List<(double, double, Section)>();
        public readonly List<double> Overpasses = new List<double>();
        public readonly List<double> X = new List<double> { 0 }, Y = new List<double> { 0 }, Z = new List<double> { 0 }, Heading = new List<double> { 0 };
        readonly Random rng;
        double nextOverpass, structuresDone;

        public readonly Zones Zones;

        public Track(int seed) { rng = new Random(seed); Zones = new Zones(seed); nextOverpass = 600 + rng.NextDouble() * 1000; }

        // Test helper: one curvature and grade forever.
        public static Track Constant(double curvature, double grade)
        {
            var t = new Track(0);
            t.hk[0] = curvature; t.vg[0] = grade;
            t.hs.Add(1e9); t.hk.Add(curvature); t.vs.Add(1e9); t.vg.Add(grade);
            return t;
        }

        public double Curvature(double s) => Lerp(hs, hk, s);
        // Terrain grade; the road itself dips below it in an underground section (Depth).
        public double GradeBase(double s) => Lerp(vs, vg, s);
        public double Grade(double s) => GradeBase(s) - (Depth(s + 0.5) - Depth(s - 0.5));

        // Underground section (cut-and-cover): the road sinks UgDepth below the terrain over UgRamp m, runs level, and rises again.
        // Smoothstep ramp: steepest slope 1.5 x UgDepth / UgRamp = 3.5 %.
        public const double UgDepth = 8, UgRamp = 340, UgRoof = 6.5; // m below terrain; roof starts where the depth reaches UgRoof
        // Trench (city, GDD 6.6): 6 m below street level between retaining walls, 230 m ramps (3.9 % at the steepest).
        public const double TrDepth = 6, TrRamp = 230;

        public double Depth(double s)
        {
            double d = 0;
            foreach (var st in Structures)
            {
                bool ug = st.kind == Section.Underground;
                if ((!ug && st.kind != Section.Trench) || s <= st.s0 || s >= st.s1) continue;
                double depth = ug ? UgDepth : TrDepth, ramp = ug ? UgRamp : TrRamp;
                double u = Math.Min(1, Math.Min(s - st.s0, st.s1 - s) / ramp);
                d = Math.Max(d, depth * u * u * (3 - 2 * u));
            }
            return d;
        }

        // True where the road is under a roof (bore or underground box): lit, echo, no sun.
        public bool Covered(double s)
        {
            var k = KindAt(s);
            return k == Section.Tunnel || (k == Section.Underground && Depth(s) >= UgRoof);
        }
        public double Generated => Math.Min(hs[hs.Count - 1], vs[vs.Count - 1]);

        public Section KindAt(double s)
        {
            foreach (var st in Structures) if (s >= st.s0 && s < st.s1) return st.kind;
            return Section.Open;
        }

        // Phase (0..3) is the one at generation time (GDD 6.5).
        public void Ensure(double s, int phase)
        {
            if (hs[hs.Count - 1] >= 1e8) return; // constant test track
            while (hs[hs.Count - 1] < s + Ahead) AddHorizontal(phase);
            while (vs[vs.Count - 1] < s + Ahead) AddVertical(phase);
            AddStructures(Generated, phase);
            while ((X.Count - 1) * SampleStep < Generated) Sample();
        }

        // ---- horizontal: straights and clothoid curves (6.2) ----

        static readonly double[,] StraightLen = { { 800, 2000 }, { 500, 1500 }, { 300, 1200 }, { 200, 1000 } };
        static readonly double[,] Radius = { { 2000, 5000 }, { 1200, 3000 }, { 720, 2000 }, { 720, 1500 } };
        static readonly double[] SCurve = { 0, 0, 0.2, 0.35 };
        static readonly double[] MaxGrade = { 0.01, 0.025, 0.04, 0.04 };
        static readonly double[] ViaductPerKm = { 0, 0.2, 0.2, 0.25 }, TunnelPerKm = { 0.35, 0.35, 0.35, 0.35 };

        double geomSinceTunnel;
        readonly HashSet<double> tunnelStraights = new HashSet<double>();

        double Rand(double a, double b) => a + rng.NextDouble() * (b - a);

        void AddHorizontal(int phase)
        {
            double s = hs[hs.Count - 1];
            bool first = s == 0;
            double straight = first ? Rand(1500, 2500) : Rand(StraightLen[phase, 0], StraightLen[phase, 1]); // long first straight: flat start (decision 27)
            // Tunnel host: after 3 km of road without one, the next straight is made at least 1400 m long and gets a tunnel.
            bool host = geomSinceTunnel > 3000;
            if (host && straight < 1400) straight = Rand(1400, 2000);
            if (host) { tunnelStraights.Add(s); geomSinceTunnel = 0; } else geomSinceTunnel += straight;
            Knot(hs, hk, s + straight, 0);
            if (phase >= 1) AddModule(phase);
            else
            {
                int side = rng.Next(2) * 2 - 1;
                do
                {
                    double r = phase == 3 && rng.NextDouble() < 0.2 ? Rand(600, 720) : Rand(Radius[phase, 0], Radius[phase, 1]);
                    double theta = Rand(10, 45) * Math.PI / 180, l = r / 4;            // clothoid A = R/2, L = R/4
                    double arc = Math.Max(0, r * theta - l);                           // the two clothoids turn L/R together
                    double s0 = hs[hs.Count - 1];
                    Knot(hs, hk, s0 + l, side / r);
                    Knot(hs, hk, s0 + l + arc, side / r);
                    Knot(hs, hk, s0 + 2 * l + arc, 0);
                    side = -side;
                } while (rng.NextDouble() < SCurve[phase]);

            }
            geomSinceTunnel += hs[hs.Count - 1] - (s + straight);
        }

        // ---- curve modules (GDD 6.6): after the straight, one of curve, sweeping bend, S-curve, sequence, tightening curve ----

        public readonly List<(double s0, double r, string module, int id)> Bends = new List<(double, double, string, int)>();
        int moduleId;
        static readonly string[] ModNames = { "curve", "sweep", "s", "sequence", "tight" };
        static readonly double[][] ModW = { null, new[] { 35.0, 25, 0, 0, 0 }, new[] { 25.0, 10, 20, 10, 10 }, new[] { 20.0, 8, 22, 20, 15 } };
        int lastModule = -1;

        // Clothoid - arc - clothoid, turning theta (rad); side +1 = right.
        void Bend(double r, double theta, int side, string module)
        {
            double l = r / 4, arc = Math.Max(0, r * theta - l), s0 = hs[hs.Count - 1];
            Bends.Add((s0, r, module, moduleId));
            Knot(hs, hk, s0 + l, side / r);
            Knot(hs, hk, s0 + l + arc, side / r);
            Knot(hs, hk, s0 + 2 * l + arc, 0);
        }

        double DrawRadius(int phase) => phase == 3 && rng.NextDouble() < 0.2 ? Rand(600, 720) : Rand(Radius[phase, 0], Radius[phase, 1]);

        void AddModule(int phase)
        {
            int m = PickWeighted(ModW[phase]);
            if (m == lastModule) m = PickWeighted(ModW[phase]); // variety: one reroll against the same module twice in a row
            lastModule = m; moduleId++;
            int side = rng.Next(2) * 2 - 1;
            double deg = Math.PI / 180;
            switch (m)
            {
                case 0: Bend(DrawRadius(phase), Rand(10, 45) * deg, side, "curve"); break;
                case 1:
                {
                    double r = Rand(3000, 5000);
                    Bend(r, Math.Min(Rand(25, 60) * deg, 3000 / r), side, "sweep");
                    break;
                }
                case 2: case 3:
                {
                    int n = m == 2 ? 2 : rng.Next(3, 6);
                    double min = phase == 2 ? 900 : 600, max = Radius[phase, 1];
                    var rs = new double[n];
                    rs[0] = Math.Max(min, Math.Min(max, DrawRadius(phase)));
                    for (int i = 1; i < n; i++) rs[i] = Math.Max(min, Math.Min(max, rs[i - 1] * Rand(0.7, 1.4)));
                    double lo = double.MaxValue; foreach (var x in rs) lo = Math.Min(lo, x);
                    rs[0] = Math.Max(rs[0], 1.2 * lo); // the tightest curve is never the entry
                    for (int i = 0; i < n; i++) { Bend(rs[i], Rand(10, 45) * deg, side, m == 2 ? "s" : "sequence"); side = -side; }
                    break;
                }
                default:
                {
                    double r1 = Math.Max(phase == 2 ? 900 : 700, DrawRadius(phase)), r2 = Rand(0.6, 0.9) * r1, s0 = hs[hs.Count - 1];
                    double l1 = r1 / 4, a1 = Rand(60, 200), lt = Rand(100, 200), a2 = Rand(80, 250), l2 = r2 / 4;
                    Bends.Add((s0, r1, "tight", moduleId));
                    Knot(hs, hk, s0 + l1, side / r1);
                    Knot(hs, hk, s0 + l1 + a1, side / r1);
                    Knot(hs, hk, s0 + l1 + a1 + lt, side / r2);   // the second clothoid: the radius falls from r1 to r2
                    Knot(hs, hk, s0 + l1 + a1 + lt + a2, side / r2);
                    Knot(hs, hk, s0 + l1 + a1 + lt + a2 + l2, 0);
                    break;
                }
            }
        }

        int PickWeighted(double[] w)
        {
            double sum = 0; foreach (var x in w) sum += x;
            double r = rng.NextDouble() * sum;
            for (int i = 0; i < w.Length; i++) { r -= w[i]; if (r < 0 && w[i] > 0) return i; }
            return 0;
        }

        // ---- vertical: constant grades joined by vertical curves (6.4) ----

        void AddVertical(int phase)
        {
            double s = vs[vs.Count - 1], g0 = vg[vg.Count - 1];
            if (s == 0) { Knot(vs, vg, Rand(800, 1500), 0); return; } // flat start
            double g = Rand(-MaxGrade[phase], MaxGrade[phase]);
            double len = Rand(400, 2000);
            // No crest inside a curve with R < 1500 m: move the crest past such curves (5.4).
            double rv = g < g0 ? 20000 : 9000, lt = rv * Math.Abs(g - g0);
            while (g < g0 && MaxCurvature(s, s + lt) > 1 / 1500.0) s += 50;
            // No curve under 1000 m on a downhill steeper than -2 % (6.3).
            if (g < -0.02 && MaxCurvature(s, s + lt + len) > 1 / 1000.0) { g = -0.02; lt = (g < g0 ? 20000 : 9000) * Math.Abs(g - g0); }
            Knot(vs, vg, s, g0);
            Knot(vs, vg, s + lt, g);
            Knot(vs, vg, s + lt + len, g);
        }

        double MaxCurvature(double a, double b)
        {
            double m = Math.Abs(Curvature(a));
            for (int i = 0; i < hs.Count; i++) if (hs[i] > a && hs[i] < b) m = Math.Max(m, Math.Abs(hk[i]));
            return Math.Max(m, Math.Abs(Curvature(b)));
        }

        // A tunnel may climb or sag but must not have a crest inside (5.4): the grade never falls along it.
        // (GDD says constant grade; with +-4 % hills that almost never fits, decision 37.)
        bool NoCrest(double a, double b)
        {
            double g = GradeBase(a);
            for (double x = a + 50; x <= b; x += 50)
            {
                double next = GradeBase(x);
                if (next < g - 0.001) return false;
                g = Math.Max(g, next);
            }
            return true;
        }

        // ---- structures (6.4, 6.5) ----

        double tunnelScan;

        void AddStructures(double upTo, int phase)
        {
            AddTunnels(upTo);
            AddSlots(upTo, phase);
            // Viaducts: one roll per km of new road.
            for (double km = Math.Max(structuresDone, 1000); km + 1000 <= upTo; km += 1000)
            {
                structuresDone = km + 1000;
                double len = Rand(300, 1500);
                if (!Busy(km - 300, km + len + 300) && rng.NextDouble() < ViaductPerKm[phase] && MaxCurvature(km, km + len) <= 1 / 720.0)
                    Structures.Add((km, km + len, Section.Viaduct));
            }
            while (nextOverpass < upTo)
            {
                if (KindAt(nextOverpass) == Section.Open) Overpasses.Add(nextOverpass);
                nextOverpass += Rand(1000, 2000);
            }
        }

        // ---- slot structures (GDD 6.6): interchange a, river bridge, trench ----

        double nextSlot = 800;
        static readonly double[] SlotGapMin = { 0, 1000, 900, 800 }, SlotGapMax = { 0, 2400, 2000, 1800 };
        static readonly double[][] SlotW = { null, new[] { 25.0, 15, 0, 30 }, new[] { 22.0, 14, 10, 20 }, new[] { 20.0, 13, 12, 18 } }; // interchange, bridge, trench, plain
        public const double TrenchMin = 700, TrenchMax = 1500;

        // Curvature stays under maxK over [a, b].
        bool Straightish(double a, double b, double maxK)
        {
            for (double x = a; x <= b; x += 20) if (Math.Abs(Curvature(x)) > maxK) return false;
            return true;
        }

        void AddSlots(double upTo, int phase)
        {
            if (phase < 1 || hs[hs.Count - 1] >= 1e8) return;
            double sampled = (X.Count - 1) * SampleStep;
            while (nextSlot + 2000 <= upTo)
            {
                double at0 = Math.Ceiling(Math.Max(nextSlot, sampled) / 4) * 4, end = 0;
                var kind = (Section)(-1);
                double[] w = (double[])SlotW[phase].Clone();
                int pick = PickWeighted(w);
                double len = pick == 0 ? Math.Floor(Rand(800, 1300) / 4) * 4 : pick == 1 ? Math.Floor(Rand(250, 800) / 4) * 4 : Math.Floor(Rand(TrenchMin, TrenchMax) / 4) * 4;
                double maxK = pick == 0 ? 1 / 1500.0 : pick == 1 ? 1 / 1000.0 : 0;
                if (pick < 3)
                    for (double at = at0; at + len <= at0 + 1500 && at + len + 100 <= upTo; at += 50)
                    {
                        if (!Straightish(at, at + len, maxK) || Busy(at - MinGap, at + len + MinGap)) continue;
                        if (pick == 2 && (Zones.At(at) != Zone.City || Zones.At(at + len) != Zone.City || !Level(at, at + len, TrRamp))) continue;
                        kind = pick == 0 ? Section.Interchange : pick == 1 ? Section.Bridge : Section.Trench;
                        Structures.Add((at, at + len, kind)); end = at + len;
                        if (kind == Section.Interchange) Overpasses.Add(Math.Floor((at + end) / 2 / 4) * 4); // the local road crossing inside the module
                        if (kind == Section.Trench) // street bridges every 120-250 m
                            for (double x = at + TrRamp + Rand(120, 250); x < end - TrRamp; x += Rand(120, 250)) Overpasses.Add(Math.Floor(x / 4) * 4);
                        break;
                    }
                nextSlot = (end > 0 ? end : at0) + Rand(SlotGapMin[phase], SlotGapMax[phase]);
            }
        }

        // A crest top lies within [a - 300, b + 150]: fairness rule F3 keeps tall scenery away from 150 m before to 300 m after it.
        public bool NearCrest(double a, double b)
        {
            for (double x = Math.Max(0, a - 300); x <= b + 150; x += 25)
                if (GradeBase(x) > 0.0005 && GradeBase(x + 25) <= 0.0005) return true;
            return false;
        }

        // Tunnels sit on straights, never in the first FirstTunnel metres, with a random chance per straight and MinGap between structures.
        // Two kinds, mixed at random: an earth mound over a bore (no crest inside), or an underground box (flat terrain on top,
        // needs terrain not steeper than 2 % against the ramp so the slope stays under 5.5 %). 200 m of straight road lead to each portal.
        public const double FirstTunnel = 1500, TunnelPerStraight = 0.5, UndergroundShare = 0.5, MinGap = 700;

        void AddTunnels(double upTo)
        {
            for (int i = 0; i + 1 < hs.Count; i++)
            {
                double a = hs[i], b = hs[i + 1];
                if (hk[i] != 0 || hk[i + 1] != 0 || b > upTo || a < tunnelScan) continue;
                tunnelScan = b;
                if (b - a < 500 || Structures.Exists(x => x.s0 < b + MinGap && x.s1 > a - MinGap)) continue;
                if (!tunnelStraights.Contains(a) && rng.NextDouble() >= TunnelPerStraight) continue;
                bool underground = rng.NextDouble() < UndergroundShare;
                if (underground && TryUnderground(a, b, upTo)) continue; // else a mound tunnel on this straight
                for (double at = Math.Ceiling(Math.Max(a + 200, FirstTunnel) / 4) * 4; at + 300 <= b; at += 52)
                {
                    double room = 0;
                    while (room < 1500 && at + room + 100 <= b && NoCrest(at, at + room + 100)) room += 100;
                    if (room < 300) continue;
                    Structures.Add((at, at + Math.Floor(Rand(300, room) / 4) * 4, Section.Tunnel));
                    break;
                }
            }
        }

        // The roofed middle of an underground section (depth >= UgRoof, about 240 m into each ramp) must lie on the straight [a, b];
        // the open ramps may run into the curves around it. Road already sampled is never changed.
        const double RoofIn = 240;

        bool TryUnderground(double a, double b, double upTo)
        {
            double len = Math.Floor(Rand(800, 1300) / 4) * 4, sampled = (X.Count - 1) * SampleStep;
            for (double at = Math.Ceiling(Math.Max(a - RoofIn, Math.Max(FirstTunnel, sampled)) / 4) * 4; at + len - RoofIn <= b && at + len <= upTo; at += 52)
                if (Level(at, at + len) && !Busy(at - MinGap, at + len + MinGap)) { Structures.Add((at, at + len, Section.Underground)); return true; }
            return false;
        }

        // The ramps add up to 3.5 % (down at the entry, up at the exit): keep the total under 5.5 %,
        // so the terrain must not fall faster than 2 % on the entry ramp or rise faster than 2 % on the exit ramp.
        bool Level(double a, double b, double ramp = UgRamp)
        {
            for (double x = a; x <= a + ramp; x += 50) if (GradeBase(x) < -0.02 || GradeBase(b - (x - a)) > 0.02) return false;
            return true;
        }

        bool Busy(double a, double b)
        {
            foreach (var st in Structures) if (st.s0 < b && st.s1 > a) return true;
            return false;
        }

        // ---- helpers ----

        static void Knot(List<double> s, List<double> v, double at, double value) { s.Add(at); v.Add(value); }

        static double Lerp(List<double> s, List<double> v, double x)
        {
            int i = s.BinarySearch(x);
            if (i >= 0) return v[i];
            i = ~i;
            if (i == 0) return v[0];
            if (i >= s.Count) return v[v.Count - 1];
            double k = (x - s[i - 1]) / (s[i] - s[i - 1]);
            return v[i - 1] + (v[i] - v[i - 1]) * k;
        }

        void Sample()
        {
            int n = X.Count - 1;
            double s = n * SampleStep, mid = s + SampleStep / 2;
            double h = Heading[n] + Curvature(mid) * SampleStep;
            double hm = (Heading[n] + h) / 2;
            X.Add(X[n] + Math.Sin(hm) * SampleStep);
            Z.Add(Z[n] + Math.Cos(hm) * SampleStep);
            Y.Add(Y[n] + Grade(mid) * SampleStep);
            Heading.Add(h);
        }
    }
}
