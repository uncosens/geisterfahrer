using System;
using Geisterfahrer.Core;
using System.Collections.Generic;
using NUnit.Framework;

namespace Geisterfahrer.Tests
{
    // Collision worked examples (GDD 2.5), side motion, spawning.
    public class RunTests
    {
        const double Dt = 1.0 / 60;
        const double VAT10 = 150.2, DAT10 = 238.3; // 10 s from standstill with the 175 kW model (decision 58)
        static readonly int Compact = Road.Index("Compact"), Sedan = Road.Index("Sedan"), Truck = Road.Index("Truck");

        static Run NewRun(double kmh, double curvature = 0, double grade = 0)
        {
            var r = new Run(new Tuning(), 0, Track.Constant(curvature, grade)) { Spawning = false, Started = true };
            r.V = kmh / 3.6;
            return r;
        }

        // Head-on: vehicle in the player's lane, just ahead.
        static Run HeadOn(double playerKmh, int type, double vehicleKmh)
        {
            var r = NewRun(playerKmh);
            var t = Road.Types[type];
            r.X = Road.LaneX(1);
            r.Vehicles.Add(new Vehicle { Type = type, Lane = 1, Z = (Road.PlayerLength + t.Length) / 2 + 0.3, Speed = vehicleKmh / 3.6 });
            for (int i = 0; i < 10 && !r.Crashed; i++) r.Step(Dt, 0, false);
            return r;
        }

        // Side hit: vehicle in lane 3 next to the player, player moving left at sideSpeed.
        static Run SideHit(int type, double sideSpeed, out double speedBefore)
        {
            var r = NewRun(150);
            var t = Road.Types[type];
            r.Side = -sideSpeed;
            r.Yaw = Math.Atan2(r.Side, r.V);
            double ex = Road.PlayerWidth / 2 * Math.Cos(r.Yaw) + Road.PlayerLength / 2 * Math.Abs(Math.Sin(r.Yaw));
            r.X = Road.LaneX(0) + t.Width / 2 + ex + 0.01;
            r.Vehicles.Add(new Vehicle { Type = type, Lane = 0, Z = 0, Speed = 85 / 3.6 });
            speedBefore = r.V;
            StepUntilHit(r, Steer(r, -sideSpeed));
            return r;
        }

        // Steering input that holds a given side speed at the car's current speed.
        static double Steer(Run r, double side) => side / Math.Min(r.T.MaxSideSpeed, r.T.SideSpeedPerSpeed * r.V);

        static Run RailHit(double playerKmh, double sideSpeed, out double speedBefore)
        {
            var r = NewRun(playerKmh);
            r.Side = -sideSpeed;
            r.Yaw = Math.Atan2(r.Side, r.V);
            r.X = -Road.RailX + Contacts.RailDepth(-Road.RailX, r.Yaw, Road.PlayerLength / 2, Road.PlayerWidth / 2, -1) + 0.01;
            speedBefore = r.V;
            StepUntilHit(r, Steer(r, -sideSpeed));
            return r;
        }

        static void StepUntilHit(Run r, double steer)
        {
            // Small steps, so the push-off side speed can be read right after the hit.
            for (int i = 0; i < 2000 && r.LastImpulse == 0; i++) r.Step(0.001, steer, true);
        }

        static void AssertSurvive(Run r, double j, double lossKmh, double before)
        {
            Assert.IsFalse(r.Crashed);
            Assert.AreEqual(j, r.LastImpulse, 0.1);
            Assert.AreEqual(lossKmh, (before - r.V) * 3.6, 1.0);
            Assert.AreEqual(1.5, r.Side, 0.2); // pushed off to the right, away from the contact
        }

        [Test] public void HeadOnSedanClosing190() { var r = HeadOn(60, Sedan, 130); Assert.IsTrue(r.Crashed); Assert.AreEqual(40.9, r.LastImpulse, 0.1); }
        [Test] public void HeadOnSedanClosing60() { var r = HeadOn(0, Sedan, 60); Assert.IsTrue(r.Crashed); Assert.AreEqual(12.9, r.LastImpulse, 0.1); }
        [Test] public void StoppedVsTruck80() { var r = HeadOn(0, Truck, 80); Assert.IsTrue(r.Crashed); Assert.AreEqual(32.0, r.LastImpulse, 0.1); }
        [Test] public void StoppedVsCompact110() { var r = HeadOn(0, Compact, 110); Assert.IsTrue(r.Crashed); Assert.AreEqual(19.4, r.LastImpulse, 0.1); }

        [Test] public void TruckSideSwipe1() { var r = SideHit(Truck, 1, out var b); AssertSurvive(r, 1.4, 12, b); }
        [Test] public void TruckSideSwipe2() { var r = SideHit(Truck, 2, out var b); AssertSurvive(r, 2.9, 23, b); }
        [Test] public void TruckSideHit7() { var r = SideHit(Truck, 7, out _); Assert.IsTrue(r.Crashed); Assert.AreEqual(10.1, r.LastImpulse, 0.1); }
        [Test] public void SedanSideHit7() { var r = SideHit(Sedan, 7, out var b); AssertSurvive(r, 5.4, 43, b); }

        [Test] public void RailFlat1() { var r = RailHit(200, 1, out var b); AssertSurvive(r, 1.5, 12, b); }
        [Test] public void RailMedium3() { var r = RailHit(180, 3, out var b); AssertSurvive(r, 4.5, 36, b); }
        [Test] public void RailSteep7() { var r = RailHit(100, 7, out _); Assert.IsTrue(r.Crashed); Assert.AreEqual(10.5, r.LastImpulse, 0.1); }

        [Test]
        public void ScrapeSlows15PerSecondWhileTouching()
        {
            var r = RailHit(200, 1, out _);
            double before = r.V;
            for (int i = 0; i < 60; i++) r.Step(Dt, -1.0 / 7, false); // keep pressing into the rail for 1 s
            Assert.IsFalse(r.Crashed);
            Assert.AreEqual(1, r.Hits);
            double coast = 4.3; // km/h lost per second from coasting at 200 (GDD 2.2)
            Assert.AreEqual(15 + coast, (before - r.V) * 3.6, 3);
        }

        [Test]
        public void SlowRailScrapeDoesNotStopTheCarAtOnce()
        {
            var r = RailHit(60, 3, out _); // J = 4.5: survive, -36 km/h
            for (int i = 0; i < 60; i++) r.Step(Dt, -0.4, false); // keep pressing into the rail for 1 s
            Assert.IsFalse(r.Crashed);
            Assert.AreEqual(1, r.Hits);
            Assert.Less(Math.Abs(r.Yaw), Math.Atan(0.36));
            Assert.Greater(r.V * 3.6, 4); // 60 - 36 - 15 - coasting ≈ 7
        }

        [Test]
        public void NoTunnelingAtTopClosingSpeed()
        {
            var r = NewRun(306);
            r.X = Road.LaneX(1);
            r.Vehicles.Add(new Vehicle { Type = Compact, Lane = 1, Z = 60, Speed = 180 / 3.6 });
            for (int i = 0; i < 60 && !r.Crashed; i++) r.Step(Dt, 0, true);
            Assert.IsTrue(r.Crashed);
        }

        [Test]
        public void StoppedCarDoesNotMoveSideways()
        {
            var r = NewRun(0);
            double x0 = r.X;
            for (int i = 0; i < 60; i++) r.Step(Dt, 1, false);
            Assert.AreEqual(x0, r.X, 1e-9);
            Assert.AreEqual(0, r.Yaw, 1e-9);
        }

        [Test]
        public void FullSteerAt100Gives7()
        {
            var r = NewRun(100);
            for (int i = 0; i < 60; i++) r.Step(Dt, 1, true);
            Assert.AreEqual(9.72, r.Side, 0.1); // min(9.8, 0.35 x 27.8 m/s)
            Assert.AreEqual(Math.Atan2(r.Side, r.V), r.Yaw, 1e-9);
        }

        [Test]
        public void SpawnDistanceGrowsWithSpeed()
        {
            var r = NewRun(150);
            Assert.AreEqual(400, r.SpawnDistance, 1);
            r.V = r.TopSpeed;
            Assert.AreEqual((r.TopSpeed + 240 / 3.6) * 3.5, r.SpawnDistance, 1); // fastest vehicle (240 km/h) x 3.5 s, once it passes 400 m
            r.V = 300 / 3.6;
            Assert.AreEqual((300 / 3.6 + 240 / 3.6) * 3.5, r.SpawnDistance, 1); // GDD 5.3: 525 m
        }

        [Test]
        public void RecordsTopSpeedAndDistance()
        {
            var r = NewRun(100);
            for (int i = 0; i < 600; i++) r.Step(Dt, 0, true);
            Assert.Greater(r.MaxSpeed * 3.6, 150);
            Assert.Greater(r.Distance, 300);
        }

        // Safe corridor (GDD 4.3): at constant speed, whatever passes the player leaves a gap of at least 2.85 m.
        [Test]
        public void SafeCorridorAlwaysOpen()
        {
            var t = new Tuning { CrashImpulse = double.MaxValue };
            var r = new Run(t, 1, Track.Constant(0, 0)) { Started = true };
            int spawned = 0;
            for (int i = 0; i < 60 * 300; i++)
            {
                int before = r.Vehicles.Count;
                r.V = 200 / 3.6;
                r.X = -Road.RailX + 2; // stay out of the way: only the gap is checked
                r.Step(Dt, 0, false);
                spawned += Math.Max(0, r.Vehicles.Count - before);
                var bodies = new List<(double a, double b)>();
                foreach (var v in r.Vehicles)
                    if (Math.Abs(v.Z) < (v.T.Length + Road.PlayerLength) / 2) bodies.Add((v.X - v.T.Width / 2, v.X + v.T.Width / 2));
                bodies.Sort();
                double edge = -Road.RailX, widest = 0;
                foreach (var b in bodies) { widest = Math.Max(widest, b.a - edge); edge = Math.Max(edge, b.b); }
                widest = Math.Max(widest, Road.RailX - edge);
                Assert.GreaterOrEqual(widest, t.CorridorWidth - 1e-6, $"t={r.Time:F1}");
            }
            Assert.Greater(spawned, 200);
        }

        [Test]
        public void WanderStaysInLaneAndSlow()
        {
            var r = new Run(new Tuning(), 3, Track.Constant(0, 0)) { Started = true };
            for (int i = 0; i < 60 * 60; i++)
            {
                r.V = 150 / 3.6;
                r.X = -Road.RailX + 2;
                var before = new Dictionary<Vehicle, double>();
                foreach (var v in r.Vehicles) before[v] = v.Offset;
                r.Step(Dt, 0, false);
                foreach (var v in r.Vehicles)
                {
                    Assert.LessOrEqual(Math.Abs(v.Offset), (Road.LaneWidth - v.T.Width) / 2 - 0.1 + 1e-9);
                    if (before.TryGetValue(v, out var o)) Assert.Less(Math.Abs(v.Offset - o) / Dt, 0.25);
                }
            }
        }

        [Test]
        public void RunStartsOnFirstThrottle()
        {
            var r = new Run(new Tuning(), 0);
            for (int i = 0; i < 120; i++) r.Step(Dt, 0, false);
            Assert.IsFalse(r.Started);
            Assert.AreEqual(0, r.Time);
            Assert.AreEqual(0, r.Vehicles.Count);
            r.Spawning = false; // only the speed curve from here
            for (int i = 0; i < 60 * 10; i++) r.Step(Dt, 0, true);
            Assert.IsTrue(r.Started);
            Assert.AreEqual(VAT10, r.V * 3.6, 2);       // GDD 2.2: 10 s from standstill
            Assert.AreEqual(DAT10, r.Distance, 6);
        }

        // Curve drift (GDD 6.3, 2.5): full steering against a curve to the right, drifting into the outer (left) rail.
        static Run CurveRail(double radius, out double before)
        {
            var r = NewRun(306, 1 / radius);
            r.X = -Road.RailX + 4; // far enough for the side speed to settle before the rail
            before = r.V;
            for (int i = 0; i < 10000 && r.LastImpulse == 0; i++) { r.V = 306 / 3.6; r.Step(0.001, 1, true); } // GDD example speed
            return r;
        }

        [Test]
        public void CurveDriftScrapesAtR720()
        {
            var r = CurveRail(720, out _);
            Assert.IsFalse(r.Crashed);
            Assert.AreEqual(2.3, r.LastImpulse, 0.25);
        }

        [Test]
        public void CurveDriftCrashesAtR470()
        {
            var r = CurveRail(470, out _);
            Assert.IsTrue(r.Crashed);
            Assert.AreEqual(11.7, r.LastImpulse, 0.5);
        }

        [Test]
        public void CurveHoldsAtR1000()
        {
            var r = NewRun(306, 1 / 1000.0);
            for (int i = 0; i < 120; i++) { r.V = 306 / 3.6; r.Step(Dt, 1, true); }
            Assert.AreEqual(0.82, r.SHold, 0.03);
            Assert.Greater(r.Side, 0); // full steering beats the drift: still some inward reach
        }

        [Test]
        public void DriftPushesOutwardWithoutSteering()
        {
            var r = NewRun(200, 1 / 1000.0);
            double x0 = r.X;
            for (int i = 0; i < 30; i++) r.Step(Dt, 0, true);
            Assert.Less(r.X, x0); // curve to the right pushes the car left
        }

        // Tunnels are regular but random: none in the first 1.5 km, both kinds appear, and the spacing is not fixed.
        [Test]
        public void TunnelsStartAfterFirstKmAndMixKinds()
        {
            int mound = 0, under = 0;
            var gaps = new HashSet<int>();
            for (int seed = 1; seed <= 20; seed++)
            {
                var t = new Track(seed);
                t.Ensure(14000, 3);
                double last = -1;
                foreach (var st in t.Structures)
                {
                    if (st.kind != Section.Tunnel && st.kind != Section.Underground) continue;
                    Assert.GreaterOrEqual(st.s0, Track.FirstTunnel, $"seed {seed}");
                    if (st.kind == Section.Tunnel) mound++; else under++;
                    if (last >= 0) gaps.Add((int)((st.s0 - last) / 500));
                    last = st.s1;
                }
            }
            Assert.Greater(mound, 5, $"mound {mound} under {under}");
            Assert.Greater(under, 15, $"mound {mound} under {under}");
            Assert.Greater(gaps.Count, 3);
        }

        [Test]
        public void TunnelsAreStraightAndLong()
        {
            var t = new Track(5);
            t.Ensure(20000, 3);
            foreach (var st in t.Structures)
                if (st.kind == Section.Tunnel || st.kind == Section.Underground)
                {
                    Assert.GreaterOrEqual(st.s1 - st.s0, st.kind == Section.Tunnel ? 300 : 800);
                    // The mound bore is straight with 200 m before it; the underground roof (240 m into each ramp) is straight, the open ramps may curve.
                    double from = st.kind == Section.Tunnel ? st.s0 - 200 : st.s0 + 240, to = st.kind == Section.Tunnel ? st.s1 : st.s1 - 240;
                    for (double s = from; s < to; s += 10) Assert.AreEqual(0, t.Curvature(s), 1e-12);
                }
        }

        // Underground: the road sinks 8 m below the terrain and comes back to the same height; ramps stay under 5 %.
        [Test]
        public void UndergroundRampsAreGentleAndReturnToTerrain()
        {
            int n = 0;
            for (int seed = 1; seed <= 20; seed++)
            {
                var t = new Track(seed);
                t.Ensure(14000, 3);
                foreach (var st in t.Structures)
                {
                    if (st.kind != Section.Underground) continue;
                    n++;
                    Assert.AreEqual(0, t.Depth(st.s0), 1e-9);
                    Assert.AreEqual(Track.UgDepth, t.Depth((st.s0 + st.s1) / 2), 1e-9);
                    Assert.AreEqual(0, t.Depth(st.s1), 1e-9);
                    for (double s = st.s0; s < st.s1; s += 5) Assert.Less(Math.Abs(t.Grade(s)), 0.056, $"seed {seed} s {s}");
                    // Road height equals terrain height minus depth.
                    int i = (int)((st.s0 + st.s1) / 2 / Track.SampleStep);
                    double terrain = t.Y[(int)(st.s0 / Track.SampleStep)];
                    for (double s = st.s0; s < (st.s0 + st.s1) / 2; s += 2) terrain += t.GradeBase(s) * 2;
                    Assert.AreEqual(terrain - Track.UgDepth, t.Y[i], 0.2, $"seed {seed}");
                }
            }
            Assert.Greater(n, 15, $"n {n}");
        }

        // Curve modules (GDD 6.6): every module appears, radii stay >= 600 m, neighbours in S-curves and sequences differ by at most 1.4,
        // the entry of an S-curve or sequence is never the tightest, tightening curves fall by at least 0.6.
        [Test]
        public void CurveModulesFollowTheRules()
        {
            var seen = new HashSet<string>();
            for (int seed = 1; seed <= 20; seed++)
            {
                var t = new Track(seed);
                t.Ensure(30000, 3);
                for (int i = 0; i < t.Bends.Count; i++)
                {
                    var b = t.Bends[i];
                    seen.Add(b.module);
                    Assert.GreaterOrEqual(b.r, 600 - 1e-9, $"seed {seed} {b.module}");
                    if ((b.module == "s" || b.module == "sequence") && i + 1 < t.Bends.Count && t.Bends[i + 1].id == b.id)
                        Assert.That(t.Bends[i + 1].r / b.r, Is.InRange(0.69, 1.42), $"seed {seed} ratio");
                }
            }
            foreach (var m in new[] { "curve", "sweep", "s", "sequence", "tight" }) Assert.IsTrue(seen.Contains(m), m);
        }

        // Slot structures appear: interchange, bridge and (in city zones) trench; a trench is straight with Depth 6 m and ramps under 4 %.
        [Test]
        public void SlotStructuresAndTrenchRules()
        {
            var kinds = new HashSet<Section>();
            for (int seed = 1; seed <= 30; seed++)
            {
                var t = new Track(seed);
                t.Ensure(40000, 3);
                foreach (var st in t.Structures)
                {
                    kinds.Add(st.kind);
                    if (st.kind != Section.Trench) continue;
                    Assert.AreEqual(Track.TrDepth, t.Depth((st.s0 + st.s1) / 2), 1e-9);
                    Assert.AreEqual(Zone.City, t.Zones.At(st.s0 + 1));
                    for (double s = st.s0; s < st.s1; s += 10) { Assert.AreEqual(0, t.Curvature(s), 1e-12); Assert.Less(Math.Abs(t.Grade(s)), 0.06); }
                }
            }
            Assert.IsTrue(kinds.Contains(Section.Interchange), string.Join(",", kinds));
            Assert.IsTrue(kinds.Contains(Section.Bridge), string.Join(",", kinds));
            Assert.IsTrue(kinds.Contains(Section.Trench), string.Join(",", kinds));
        }

        // User changes: +40 % steering, return to straight twice as slow, +40 % traffic.
        [Test]
        public void SteeringIsFortyPercentStronger()
        {
            var t = new Tuning();
            Assert.AreEqual(7 * 1.4, t.MaxSideSpeed, 0.01);
            Assert.AreEqual(0.25 * 1.4, t.SideSpeedPerSpeed, 0.001);
            Assert.AreEqual(0.30, t.RecenterTau, 1e-9);
        }

        [Test]
        public void SideSpeedReturnsSlowlyWhenTheFingerLifts()
        {
            var r = NewRun(150);
            for (int i = 0; i < 15; i++) r.Step(Dt, 1, true); // 0.25 s of steering, short enough to stay clear of the rail
            double full = r.Side;
            for (int i = 0; i < 18; i++) r.Step(Dt, 0, true); // 0.3 s = one new time constant: about 37 % left
            Assert.IsFalse(r.Scraping);
            Assert.AreEqual(full * Math.Exp(-1), r.Side, full * 0.06);
        }

        [Test]
        public void DeadZoneInputIsZeroSteering()
        {
            var r = NewRun(100);
            for (int i = 0; i < 60; i++) r.Step(Dt, 0, true);
            Assert.AreEqual(0, r.Side, 1e-9);
        }

        [Test]
        public void TrafficDensityIsFortyPercentHigher()
        {
            Assert.AreEqual(1.4, new Tuning().DensityScale, 1e-9);
            // Realised traffic: vehicles per km over 2 min of steady driving at 150 km/h, phase 2-3.
            var r = new Run(new Tuning { CrashImpulse = double.MaxValue }, 4, Track.Constant(0, 0)) { Started = true };
            int spawned = 0;
            for (int i = 0; i < 60 * 120; i++)
            {
                int before = r.Vehicles.Count;
                r.V = 150 / 3.6; r.X = -Road.RailX + 2;
                r.Step(Dt, 0, false);
                spawned += Math.Max(0, r.Vehicles.Count - before);
            }
            double perKm = spawned / (r.Distance / 1000);
            Assert.Greater(perKm, 14); // nominal 11 to 28 per km over those phases, times 1.4 minus rejected spawns
        }
    }
}
