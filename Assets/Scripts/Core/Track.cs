using System;
using System.Collections.Generic;

namespace Geisterfahrer.Core
{
    public enum Section { Open, Tunnel, Viaduct }

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

        public Track(int seed) { rng = new Random(seed); nextOverpass = 600 + rng.NextDouble() * 1000; }

        // Test helper: one curvature and grade forever.
        public static Track Constant(double curvature, double grade)
        {
            var t = new Track(0);
            t.hk[0] = curvature; t.vg[0] = grade;
            t.hs.Add(1e9); t.hk.Add(curvature); t.vs.Add(1e9); t.vg.Add(grade);
            return t;
        }

        public double Curvature(double s) => Lerp(hs, hk, s);
        public double Grade(double s) => Lerp(vs, vg, s);
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
            double straight = first ? Rand(1500, 2500) : Rand(StraightLen[phase, 0], StraightLen[phase, 1]); // long first straight: flat start and the first tunnel (decision 27)
            // Tunnel host: after 3 km of road without one, the next straight is made at least 1000 m long and gets a tunnel.
            bool host = first || geomSinceTunnel > 3000;
            if (host && straight < 1000) straight = Rand(1000, 1500);
            if (host) { tunnelStraights.Add(s); geomSinceTunnel = 0; } else geomSinceTunnel += straight;
            Knot(hs, hk, s + straight, 0);
            int side = rng.Next(2) * 2 - 1;
            do
            {
                double r = phase == 3 && rng.NextDouble() < 0.2 ? Rand(470, 720) : Rand(Radius[phase, 0], Radius[phase, 1]);
                double theta = Rand(10, 45) * Math.PI / 180, l = r / 4;            // clothoid A = R/2, L = R/4
                double arc = Math.Max(0, r * theta - l);                           // the two clothoids turn L/R together
                double s0 = hs[hs.Count - 1];
                Knot(hs, hk, s0 + l, side / r);
                Knot(hs, hk, s0 + l + arc, side / r);
                Knot(hs, hk, s0 + 2 * l + arc, 0);
                side = -side;
            } while (rng.NextDouble() < SCurve[phase]);
            geomSinceTunnel += hs[hs.Count - 1] - (s + straight);
        }

        // ---- vertical: constant grades joined by vertical curves (6.4) ----

        void AddVertical(int phase)
        {
            double s = vs[vs.Count - 1], g0 = vg[vg.Count - 1];
            if (s == 0) { Knot(vs, vg, Rand(800, 1500), 0); return; } // flat start
            double g = Rand(-MaxGrade[phase], MaxGrade[phase]);
            double len = Rand(400, 2000);
            // No crest inside a curve with R < 1500 m: move the crest past such curves (5.4).
            double rv = g < g0 ? 15000 : 9000, lt = rv * Math.Abs(g - g0);
            while (g < g0 && MaxCurvature(s, s + lt) > 1 / 1500.0) s += 50;
            // No curve under 1000 m on a downhill steeper than -2 % (6.3).
            if (g < -0.02 && MaxCurvature(s, s + lt + len) > 1 / 1000.0) { g = -0.02; lt = (g < g0 ? 15000 : 9000) * Math.Abs(g - g0); }
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
            double g = Grade(a);
            for (double x = a + 50; x <= b; x += 50)
            {
                double next = Grade(x);
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

        // Tunnels sit on straights: every straight of at least 500 m (after the first 500 m of the run) gets one with chance TunnelPerStraight,
        // and the first straight that qualifies always does, so the first tunnel comes within the first minute.
        // The portal has 200 m of straight road before it and the grade must not fall inside it (no crest).
        // The old per-km roll in the phase at generation time never fired in the first minute: the road is built 2.5 km ahead.
        const double TunnelPerStraight = 0.9;

        void AddTunnels(double upTo)
        {
            for (int i = 0; i + 1 < hs.Count; i++)
            {
                double a = hs[i], b = hs[i + 1];
                if (hk[i] != 0 || hk[i + 1] != 0 || b > upTo || a < tunnelScan) continue;
                tunnelScan = b;
                if (b - a < 500 || Structures.Exists(x => x.s0 < b && x.s1 > a - 300)) continue;
                bool must = tunnelStraights.Contains(a) || !Structures.Exists(x => x.kind == Section.Tunnel);
                if (!must && rng.NextDouble() >= TunnelPerStraight) continue;
                for (double at = Math.Max(a + 200, 500); at + 300 <= b; at += 50)
                {
                    double room = 0;
                    while (room < 1500 && at + room + 100 <= b && NoCrest(at, at + room + 100)) room += 100;
                    if (room < 300) continue;
                    Structures.Add((at, at + Rand(300, room), Section.Tunnel));
                    break;
                }
            }
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
