using System;
using System.Collections.Generic;

namespace Geisterfahrer.Core
{
    public class Vehicle
    {
        public int Type, Lane;
        public double Z, Speed; // Speed = own speed in m/s, toward the player
        public bool Touching;
        public VehicleType T => Road.Types[Type];
        public double X => Road.LaneX(Lane);
    }

    // One run: player car, traffic, contacts, records. The player stays at z = 0; the world moves toward it.
    public class Run
    {
        public readonly Tuning T;
        public readonly double TopSpeed;
        public readonly List<Vehicle> Vehicles = new List<Vehicle>();
        readonly Random rng;

        public double X, V, Side, Yaw;
        public double Time, Distance, MaxSpeed;
        public bool Crashed;
        public bool Spawning = true;
        public double LastImpulse; // last survived or fatal hit, for feedback
        public int Hits;
        bool[] railTouching = new bool[2];
        double nextSpawnTime;

        public Run(Tuning t, int seed = 0)
        {
            T = t;
            TopSpeed = Car.TopSpeed(t);
            rng = new Random(seed);
            V = MaxSpeed = t.StartSpeedKmh / 3.6;
        }

        public double SpawnDistance => Math.Max(T.MinSpawnDistance, (V + Road.FastestTrafficKmh / 3.6) * T.SpawnWarningTime);

        public void Step(double dt, double steer, bool throttle)
        {
            if (Crashed) return;
            double maxMove = (V + 60) * dt + Math.Abs(Side) * dt; // 60 m/s > fastest traffic
            int n = Math.Max(1, (int)Math.Ceiling(maxMove / T.MaxStepMove));
            double h = dt / n;
            for (int i = 0; i < n && !Crashed; i++) SubStep(h, steer, throttle);
            Spawn(dt);
            Vehicles.RemoveAll(v => v.Z < -40);
        }

        void SubStep(double h, double steer, bool throttle)
        {
            V = Car.Step(T, V, throttle, h);
            double target = steer * Math.Min(T.MaxSideSpeed, T.SideSpeedPerSpeed * V);
            double tau = steer != 0 ? T.SteerTau : T.RecenterTau;
            Side += (target - Side) * (1 - Math.Exp(-h / tau));
            X += Side * h;
            Yaw = Math.Atan2(Side, V);
            foreach (var v in Vehicles) v.Z -= (V + v.Speed) * h;
            Distance += V * h;
            Time += h;
            MaxSpeed = Math.Max(MaxSpeed, V);
            Collide(h);
        }

        // After a rail hit, the small push-off bounce still counts as touching, so sliding along the rail scrapes.
        const double RailTouchGap = 0.3;

        void Collide(double h)
        {
            double hl = Road.PlayerLength / 2, hw = Road.PlayerWidth / 2;
            for (int s = 0; s < 2; s++)
            {
                int side = s == 0 ? -1 : 1;
                double depth = Contacts.RailDepth(X, Yaw, hl, hw, side);
                if (depth > 0) Hit(ref railTouching[s], side, 0, depth, side * Side, double.PositiveInfinity, h);
                else if (railTouching[s] &= depth > -RailTouchGap) V = Math.Max(0, V - T.ScrapeKmhPerSecond / 3.6 * h);
            }
            foreach (var v in Vehicles)
            {
                if (Contacts.Box(X, Yaw, hl, hw, v.X, v.Z, v.T.Length / 2, v.T.Width / 2, out var nx, out var nz, out var depth))
                {
                    double vn = nx * Side + nz * (V + v.Speed);
                    Hit(ref v.Touching, nx, nz, depth, vn, v.T.Mass, h);
                }
                else v.Touching = false;
                if (Crashed) return;
            }
        }

        // First touch: impulse decides crash or survive. Still touching: scrape.
        void Hit(ref bool touching, double nx, double nz, double depth, double vn, double otherMass, double h)
        {
            if (!touching && vn > 0)
            {
                double j = Contacts.Impulse(T.Mass, otherMass, vn);
                LastImpulse = j;
                if (j >= T.CrashImpulse) { Crashed = true; return; }
                Hits++;
                V = Math.Max(0, V - T.SpeedLossKmhPerImpulse * j / 3.6);
                if (nx != 0) Side = -Math.Sign(nx) * T.PushOffSideSpeed;
            }
            else V = Math.Max(0, V - T.ScrapeKmhPerSecond / 3.6 * h);
            touching = true;
            X -= Math.Sign(nx) * depth; // push out sideways; the player cannot move in z
        }

        // ponytail: density curve and gaps are first guesses from GDD 2.4/4.2; truck columns and
        // the "tall vehicle hides the free lane" rule (GDD 5.3) are not done yet.
        public static double Density(double t) =>
            t < 20 ? 6 + 4 * t / 20 : t < 60 ? 10 + 5 * (t - 20) / 40 : t < 120 ? 15 : Math.Min(25, 15 + (t - 120) / 18);

        public static int Phase(double t) => t < 20 ? 0 : t < 60 ? 1 : t < 120 ? 2 : 3;

        void Spawn(double dt)
        {
            if (!Spawning || Time < nextSpawnTime) return;
            // Vehicles met per second = density * closing speed; spawn on average at that rate.
            double closing = V + 30;
            double rate = Density(Time) / 1000 * closing;
            nextSpawnTime = Time + (0.5 + rng.NextDouble()) / rate;
            TrySpawn();
        }

        public bool TrySpawn()
        {
            int type = Pick(i => Road.Types[i].PhaseWeights[Phase(Time)], Road.Types.Length);
            var t = Road.Types[type];
            double speed = (t.MinKmh + rng.NextDouble() * (t.MaxKmh - t.MinKmh)) / 3.6;
            double z = SpawnDistance;
            var tried = new bool[3];
            for (int k = 0; k < 3; k++)
            {
                int lane = Pick(i => tried[i] ? 0 : t.LaneWeights[i], 3);
                if (lane < 0) break;
                tried[lane] = true;
                var v = new Vehicle { Type = type, Lane = lane, Z = z, Speed = speed };
                if (Fits(v)) { Vehicles.Add(v); return true; }
            }
            return false;
        }

        // Same lane: keep a gap and never let a faster vehicle catch a slower one ahead.
        // Other lanes: never let all 3 lanes be blocked at the same time before the vehicle reaches the player.
        bool Fits(Vehicle n)
        {
            double horizon = n.Z / Math.Max(V + n.Speed, 1);
            bool[] blocked = new bool[3];
            foreach (var v in Vehicles)
            {
                if (v.Lane == n.Lane)
                {
                    double gap = (n.T.Length + v.T.Length) / 2 + (v.T.IsTall ? T.SameLaneGapTime * (V + n.Speed) : 10);
                    if (Math.Abs(n.Z - v.Z) < gap) return false;
                    if (v.Z < n.Z) n.Speed = Math.Min(n.Speed, v.Speed);
                }
                else if (Meets(n, v, horizon)) blocked[v.Lane] = true;
            }
            blocked[n.Lane] = true;
            return !(blocked[0] && blocked[1] && blocked[2]);
        }

        // Do the two vehicles' blocked z ranges overlap at some time in [0, horizon]?
        bool Meets(Vehicle a, Vehicle b, double horizon)
        {
            double reach = (a.T.Length + b.T.Length) / 2 + T.CrossLaneGap;
            double d0 = a.Z - b.Z, d1 = d0 + (b.Speed - a.Speed) * horizon;
            return Math.Sign(d0) != Math.Sign(d1) || Math.Min(Math.Abs(d0), Math.Abs(d1)) < reach;
        }

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
