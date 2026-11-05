using System;
using Geisterfahrer.Core;
using NUnit.Framework;

namespace Geisterfahrer.Tests
{
    // Collision worked examples (GDD 2.5), side motion, spawning.
    public class RunTests
    {
        const double Dt = 1.0 / 60;
        const int Compact = 0, Sedan = 1, Truck = 4;

        static Run NewRun(double kmh)
        {
            var r = new Run(new Tuning()) { Spawning = false };
            r.V = kmh / 3.6;
            return r;
        }

        // Head-on: vehicle in the player's lane, just ahead.
        static Run HeadOn(double playerKmh, int type, double vehicleKmh)
        {
            var r = NewRun(playerKmh);
            var t = Road.Types[type];
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
            StepUntilHit(r, -sideSpeed / 7);
            return r;
        }

        static Run RailHit(double playerKmh, double sideSpeed, out double speedBefore)
        {
            var r = NewRun(playerKmh);
            r.Side = -sideSpeed;
            r.Yaw = Math.Atan2(r.Side, r.V);
            r.X = -Road.RailX + Contacts.RailDepth(-Road.RailX, r.Yaw, Road.PlayerLength / 2, Road.PlayerWidth / 2, -1) + 0.01;
            speedBefore = r.V;
            StepUntilHit(r, -sideSpeed / 7);
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
        public void NoTunnelingAtTopClosingSpeed()
        {
            var r = NewRun(306);
            r.Vehicles.Add(new Vehicle { Type = Compact, Lane = 1, Z = 60, Speed = 180 / 3.6 });
            for (int i = 0; i < 60 && !r.Crashed; i++) r.Step(Dt, 0, true);
            Assert.IsTrue(r.Crashed);
        }

        [Test]
        public void StoppedCarDoesNotMoveSideways()
        {
            var r = NewRun(0);
            for (int i = 0; i < 60; i++) r.Step(Dt, 1, false);
            Assert.AreEqual(0, r.X, 1e-9);
            Assert.AreEqual(0, r.Yaw, 1e-9);
        }

        [Test]
        public void FullSteerAt100Gives7()
        {
            var r = NewRun(100);
            for (int i = 0; i < 60; i++) r.Step(Dt, 1, true);
            Assert.AreEqual(7, r.Side, 0.05);
            Assert.AreEqual(Math.Atan2(r.Side, r.V), r.Yaw, 1e-9);
        }

        [Test]
        public void SpawnDistanceGrowsWithSpeed()
        {
            var r = NewRun(230);
            Assert.AreEqual(400, r.SpawnDistance, 1);
            r.V = r.TopSpeed;
            Assert.AreEqual(470, r.SpawnDistance, 5);
        }

        [Test]
        public void RecordsTopSpeedAndDistance()
        {
            var r = NewRun(100);
            for (int i = 0; i < 600; i++) r.Step(Dt, 0, true);
            Assert.Greater(r.MaxSpeed * 3.6, 150);
            Assert.Greater(r.Distance, 300);
        }

        // Fair-lane rule: ahead of the player, never all 3 lanes blocked at the same z.
        [Test]
        public void NeverAWallOfThreeLanes()
        {
            var t = new Tuning { CrashImpulse = double.MaxValue };
            var r = new Run(t, 1);
            int spawned = 0;
            for (int i = 0; i < 60 * 300; i++)
            {
                int before = r.Vehicles.Count;
                r.Step(Dt, 0, true);
                spawned += Math.Max(0, r.Vehicles.Count - before);
                for (double z = 10; z < r.SpawnDistance; z += 1)
                {
                    int blocked = 0;
                    for (int lane = 0; lane < 3; lane++)
                        foreach (var v in r.Vehicles)
                            if (v.Lane == lane && Math.Abs(v.Z - z) < v.T.Length / 2) { blocked++; break; }
                    Assert.Less(blocked, 3, $"wall at z={z} t={r.Time:F1}");
                }
            }
            Assert.Greater(spawned, 200);
        }
    }
}
