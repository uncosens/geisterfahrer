using System;
using System.Collections.Generic;
using System.Linq;

namespace Geisterfahrer.Core
{
    public class Vehicle
    {
        public int Type, Variant, Lane;    // Type = class index in Road.Types, Variant = index in Road.Variants
        public bool Dead;                  // removed by a non-lethal hit
        public double Z, Speed;            // Z = road distance ahead of the player; Speed = own speed in m/s, toward the player
        public double OBase, A, Period, Phase, OMax, Offset; // side offset inside the lane with slow wander (GDD 4.2)
        public bool Touching;
        public VehicleType T => Road.Types[Type];
        public double X => Road.LaneX(Lane) + Offset;
        public double OffsetAt(double t) => Period > 0 ? Math.Max(-OMax, Math.Min(OMax, OBase + A * Math.Sin(2 * Math.PI * t / Period + Phase))) : OBase;
    }

    // One run in road space: the player at road distance Distance and side offset X; vehicles at Z ahead.
    public class Run
    {
        const double G = 9.81;
        public readonly Tuning T;
        public readonly Track Track;
        public readonly double TopSpeed;
        public readonly List<Vehicle> Vehicles = new List<Vehicle>();
        readonly Random rng;

        public double X, V, Side, Yaw;
        public double Time, Distance, MaxSpeed;
        public bool Started, Crashed;
        public bool Spawning = true;
        public double SHold;               // steering needed to hold the line in the current curve (GDD 6.3)
        public double LastImpulse;         // last survived or fatal hit, for feedback
        public int Hits;
        public int BikeHits;               // non-lethal contacts so far (the view shows the debris)
        public double BikeX, BikeZ;        // where the last one happened
        double lastNonLethalTime = -1e9;
        public double HitX, HitZ, HitNx;   // last contact point (road space) and its side, for effects
        public bool Scraping;              // touched a rail or vehicle during the last Step
        readonly bool[] railTouching = new bool[2];
        double nextSpawnTime;
        readonly List<double> corridorT = new List<double>(), corridorX = new List<double>();
        double corridorTarget;

        public Run(Tuning t, int seed = 0, Track track = null)
        {
            T = t;
            TopSpeed = Car.TopSpeed(t);
            rng = new Random(seed);
            Track = track ?? new Track(seed);
            Track.Ensure(0, 0);
            V = MaxSpeed = t.StartSpeedKmh / 3.6;
            X = corridorTarget = Road.LaneX(2);
        }

        // Per vehicle (GDD 5.3): max(400 m, closing speed x 3.5 s) with its own speed.
        public double SpawnDistanceFor(double vehicleSpeed) => Math.Max(T.MinSpawnDistance, (V + vehicleSpeed) * T.SpawnWarningTime);

        // Worst case (the fastest vehicle), for the haze and for spawn timing.
        public double SpawnDistance => SpawnDistanceFor(Road.FastestTrafficKmh / 3.6);

        public void Step(double dt, double steer, bool throttle)
        {
            if (Crashed) return;
            // The run clock, spawner and road phase start on the first accelerator press (GDD 2.4).
            if (!Started && !(Started = throttle)) return;
            Scraping = false;
            double maxMove = (V + 60) * dt + Math.Abs(Side) * dt; // 60 m/s > fastest traffic
            int n = Math.Max(1, (int)Math.Ceiling(maxMove / T.MaxStepMove));
            double h = dt / n;
            for (int i = 0; i < n && !Crashed; i++) SubStep(h, steer, throttle);
            Track.Ensure(Distance, Phase(Time));
            Spawn();
            Vehicles.RemoveAll(v => v.Z < -40);
        }

        void SubStep(double h, double steer, bool throttle)
        {
            double kappa = Track.Curvature(Distance);
            V = Car.Step(T, V, throttle, h, steer, Track.Grade(Distance));
            double sideMax = Math.Min(T.MaxSideSpeed, T.SideSpeedPerSpeed * V);
            SHold = Math.Max(0, V * V * Math.Abs(kappa) - G * T.Superelevation) / (T.Mu * G);
            double target = sideMax * steer - Math.Sign(kappa) * sideMax * SHold; // drift pushes outward
            target = Math.Max(-T.MaxSideSpeed, Math.Min(T.MaxSideSpeed, target));
            double tau = steer != 0 ? T.SteerTau : T.RecenterTau;
            Side += (target - Side) * (1 - Math.Exp(-h / tau));
            X += Side * h;
            Yaw = Math.Atan2(Side, V);
            foreach (var v in Vehicles) { v.Z -= (V + v.Speed) * h; v.Offset = v.OffsetAt(Time); }
            Distance += V * h;
            Time += h;
            MaxSpeed = Math.Max(MaxSpeed, V);
            Collide(h);
        }

        // After a rail hit, the push-off bounce and the car straightening (corner moves ~0.3 m back)
        // still count as touching, so pressing along the rail scrapes instead of hitting again and again.
        const double RailTouchGap = 0.6;

        void Collide(double h)
        {
            double hl = Road.PlayerLength / 2, hw = Road.PlayerWidth / 2;
            for (int s = 0; s < 2; s++)
            {
                int side = s == 0 ? -1 : 1;
                double depth = Contacts.RailDepth(X, Yaw, hl, hw, side);
                if (depth > 0) Hit(ref railTouching[s], side, 0, depth, side * Side, double.PositiveInfinity, h);
                else if (railTouching[s] &= depth > -RailTouchGap) { V = Math.Max(0, V - T.ScrapeKmhPerSecond / 3.6 * h); Scraping = true; }
            }
            foreach (var v in Vehicles)
            {
                if (Contacts.Box(X, Yaw, hl, hw, v.X, v.Z, v.T.Length / 2, v.T.Width / 2, out var nx, out var nz, out var depth))
                {
                    double vn = nx * Side + nz * (V + v.Speed);
                    Hit(ref v.Touching, nx, nz, depth, vn, v.T.Mass, h, v);
                }
                else v.Touching = false;
                if (Crashed) return;
            }
            Vehicles.RemoveAll(x => x.Dead);
        }

        // First touch: impulse decides crash or survive. Still touching: scrape.
        void Hit(ref bool touching, double nx, double nz, double depth, double vn, double otherMass, double h, Vehicle other = null)
        {
            HitX = X + nx * Road.PlayerWidth / 2; HitZ = nz * Road.PlayerLength / 2; HitNx = nx;
            Scraping = true;
            if (!touching && vn > 0)
            {
                double j = Contacts.Impulse(T.Mass, otherMass, vn);
                LastImpulse = j;
                if (other != null && other.T.NonLethal) { NonLethalContact(other, nx, j); return; }
                if (j >= T.CrashImpulse) { Crashed = true; return; }
                Hits++;
                V = Math.Max(0, V - T.SpeedLossKmhPerImpulse * j / 3.6);
                // Same 0.25 x speed side limit as steering, so a slow car does not spin round.
                if (nx != 0) Side = -Math.Sign(nx) * Math.Min(T.PushOffSideSpeed, T.SideSpeedPerSpeed * V);
            }
            else V = Math.Max(0, V - T.ScrapeKmhPerSecond / 3.6 * h);
            touching = true;
            X -= Math.Sign(nx) * depth; // push out sideways; the player cannot move in z
        }

        // A motorbike never ends the run, but costs 40-60 % of the speed (GDD 2.5). A second contact within 4 s is game over.
        void NonLethalContact(Vehicle bike, double nx, double j)
        {
            if (Time - lastNonLethalTime < T.NonLethalChainTime) { Crashed = true; return; }
            lastNonLethalTime = Time;
            double loss = Math.Max(T.NonLethalMinLoss * V, Math.Min(T.NonLethalMaxLoss * V, T.SpeedLossKmhPerImpulse * j / 3.6));
            V = Math.Max(0, V - loss);
            double dir = Math.Abs(X - bike.X) < 0.05 ? -Math.Sign(X) : Math.Sign(X - bike.X); // away from the bike, toward the middle if level
            if (dir == 0) dir = 1;
            Side = dir * Math.Min(T.PushOffSideSpeed, T.SideSpeedPerSpeed * V);
            Hits++; BikeHits++;
            BikeX = bike.X; BikeZ = bike.Z;
            bike.Dead = true;
        }

        // ---- traffic (GDD 2.4, 4) ----

        // Vehicles per km over all 4 lanes, rising smoothly through the phases.
        public static double Density(double t) =>
            t < 20 ? 8 + 5 * t / 20 : t < 60 ? 13 + 7 * (t - 20) / 40 : t < 120 ? 20 : Math.Min(32, 20 + (t - 120) * 12 / 180);

        public static int Phase(double t) => t < 20 ? 0 : t < 60 ? 1 : t < 120 ? 2 : 3;

        void Spawn()
        {
            if (!Spawning || Time < nextSpawnTime) return;
            // Vehicles met per second = density * closing speed; spawn on average at that rate.
            double z = SpawnDistance;
            double density = Density(Time) * T.DensityScale * (Track.Covered(Distance + z) ? T.TunnelDensity : 1);
            double rate = density / 1000 * (V + 30);
            nextSpawnTime = Time + (0.5 + rng.NextDouble()) / rate;
            TrySpawn();
        }

        readonly int[] lastVariant = Enumerable.Repeat(-1, Road.Types.Length).ToArray();

        public bool TrySpawn()
        {
            int type = Pick(i => Road.Types[i].PhaseWeights[Phase(Time)], Road.Types.Length);
            var t = Road.Types[type];
            int variant = PickVariant(type);
            var vr = Road.Variants[variant];
            double minK = vr.Overrides ? vr.MinKmh : t.MinKmh, maxK = vr.Overrides ? vr.MaxKmh : t.MaxKmh;
            double speed = (minK + rng.NextDouble() * (maxK - minK)) / 3.6;
            double oMax = (Road.LaneWidth - t.Width) / 2 - T.LaneMargin;
            var v = new Vehicle
            {
                Type = type, Variant = variant, Z = SpawnDistanceFor(speed), Speed = speed, OMax = oMax,
                OBase = Math.Max(-oMax, Math.Min(oMax, Normal() * 0.5 * oMax)),
                A = rng.NextDouble() * Math.Min(0.3 * oMax, T.WanderCap), Period = 8 + rng.NextDouble() * 7, Phase = rng.NextDouble() * 2 * Math.PI,
            };
            if (t.NonLethal && Vehicles.Exists(o => o.T.NonLethal && Math.Abs(o.Z - v.Z) < T.BikeSpacing)) return false; // bikes keep 600 m apart
            var lanes = vr.LaneWeights ?? t.LaneWeights;
            var tried = new bool[Road.LaneCount];
            for (int k = 0; k < Road.LaneCount; k++)
            {
                int lane = Pick(i => tried[i] ? 0 : lanes[i], Road.LaneCount);
                if (lane < 0) break;
                tried[lane] = true;
                v.Lane = lane;
                if (Fits(v)) { v.Offset = v.OffsetAt(Time); Vehicles.Add(v); lastVariant[type] = variant; return true; }
            }
            return false;
        }

        // Variant by its share inside the class, never the one that was just used (GDD 4.1.3 rule 3).
        int PickVariant(int type)
        {
            var ids = Road.VariantsOf[type];
            int pick = Pick(i => ids.Length > 1 && ids[i] == lastVariant[type] ? 0 : Road.Variants[ids[i]].Share, ids.Length);
            return ids[Math.Max(0, pick)];
        }

        static bool IsFast(Vehicle x) => x.T.Name == "Sports" || x.T.NonLethal || Road.Variants[x.Variant].Overrides;

        // Same lane (GDD 4.3, 5.5): a gap by length, 1.5 s (2.0 s for fast light vehicles) behind a tall vehicle, and the catch-up rule
        // (a faster vehicle behind a slower one must not reach it before the slower one passes the player).
        // Every lane: the body with its whole wander envelope stays clear of the safe corridor during its meet window.
        // ponytail: meet times use the current player speed; the GDD's re-plan after a 20 % speed change,
        // truck columns and the crest/curve corridor limits are not done.
        public bool Fits(Vehicle n)
        {
            foreach (var v in Vehicles)
            {
                if (v.Lane != n.Lane) continue;
                // The one nearer to the player is "ahead" (it meets the player first).
                var ahead = v.Z <= n.Z ? v : n; var behind = ahead == v ? n : v;
                double dz = Math.Abs(n.Z - v.Z);
                double gap = (n.T.Length + v.T.Length) / 2 + (ahead.T.IsTall ? (IsFast(behind) ? T.FastBehindTallTime : T.SameLaneGapTime) * (V + behind.Speed) : 10);
                if (dz < gap) return false;
                if (behind.Speed > ahead.Speed)
                {
                    double tMeet = Math.Max(ahead.Z, 1) / Math.Max(V + ahead.Speed, 1);
                    if (dz < (behind.Speed - ahead.Speed) * tMeet + T.CatchUpMargin) return false;
                }
            }
            double closing = Math.Max(V + n.Speed, 1), reach = (n.T.Length + Road.PlayerLength) / 2;
            double t0 = Time + (n.Z - reach) / closing - T.MeetPad, t1 = Time + (n.Z + reach) / closing + T.MeetPad;
            double lane = Road.LaneX(n.Lane), half = n.T.Width / 2 + T.CorridorWidth / 2;
            double lo = lane + Math.Max(-n.OMax, n.OBase - n.A) - half, hi = lane + Math.Min(n.OMax, n.OBase + n.A) + half;
            for (double t = t0; t <= t1 + 1e-9; t += 0.1)
            {
                double c = CorridorAt(t);
                if (c > lo && c < hi) return false;
            }
            return true;
        }

        // Planned corridor centre at absolute run time t: 1 s knots, moving at most CorridorSpeed toward a target
        // that changes now and then, staying clear of the rails (GDD 4.3). It starts at the player.
        public double CorridorAt(double t)
        {
            if (corridorT.Count == 0) { corridorT.Add(Time); corridorX.Add(X); }
            double edge = Road.RailX - T.CorridorWidth / 2 - T.CorridorRailGap;
            while (corridorT[corridorT.Count - 1] < t)
            {
                double x = corridorX[corridorX.Count - 1];
                if (rng.NextDouble() < 0.25) corridorTarget = (rng.NextDouble() * 2 - 1) * edge;
                double step = Math.Max(-T.CorridorSpeed, Math.Min(T.CorridorSpeed, corridorTarget - x));
                corridorT.Add(corridorT[corridorT.Count - 1] + 1);
                corridorX.Add(Math.Max(-edge, Math.Min(edge, x + step)));
            }
            int i = corridorT.BinarySearch(t);
            if (i >= 0) return corridorX[i];
            i = ~i;
            if (i == 0) return corridorX[0];
            double k = (t - corridorT[i - 1]) / (corridorT[i] - corridorT[i - 1]);
            return corridorX[i - 1] + (corridorX[i] - corridorX[i - 1]) * k;
        }

        double Normal() => Math.Sqrt(-2 * Math.Log(1 - rng.NextDouble())) * Math.Cos(2 * Math.PI * rng.NextDouble());

        int Pick(Func<int, double> weight, int count)
        {
            double sum = 0;
            for (int i = 0; i < count; i++) sum += weight(i);
            if (sum <= 0) return -1;
            double r = rng.NextDouble() * sum;
            for (int i = 0; i < count; i++) if ((r -= weight(i)) < 0) return i;
            return count - 1;
        }
    }
}
