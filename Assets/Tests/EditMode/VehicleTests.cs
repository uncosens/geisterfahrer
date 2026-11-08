using System;
using System.Linq;
using Geisterfahrer.Core;
using NUnit.Framework;

namespace Geisterfahrer.Tests
{
    // Class table, variants, motorbikes and spawn rules (GDD 4.1, 2.5, 4.3, 5.3, 5.5).
    public class VehicleTests
    {
        const double Dt = 1.0 / 60;
        static readonly int Bike = Road.Index("Motorbike");

        static Run NewRun(double kmh)
        {
            var r = new Run(new Tuning(), 0, Track.Constant(0, 0)) { Spawning = false, Started = true };
            r.V = kmh / 3.6;
            r.X = Road.LaneX(1);
            return r;
        }

        // A vehicle in the player's lane just ahead, then a few steps with no throttle.
        static Run HeadOn(double playerKmh, int type, double vehicleKmh)
        {
            var r = NewRun(playerKmh);
            var t = Road.Types[type];
            r.Vehicles.Add(new Vehicle { Type = type, Lane = 1, Z = (Road.PlayerLength + t.Length) / 2 + 0.3, Speed = vehicleKmh / 3.6 });
            for (int i = 0; i < 10 && !r.Crashed; i++) r.Step(Dt, 0, false);
            return r;
        }

        // ---- the table ----

        [Test] public void SixteenClassesInTheGdd() => Assert.AreEqual(16, Road.Types.Length);

        [Test]
        public void PhaseWeightsSumTo100()
        {
            for (int p = 0; p < 4; p++) Assert.AreEqual(100, Road.Types.Sum(t => t.PhaseWeights[p]), 1e-9, $"phase {p + 1}");
        }

        [Test]
        public void LaneSharesSumTo100()
        {
            foreach (var t in Road.Types) Assert.AreEqual(100, t.LaneWeights.Sum(), 1e-9, t.Name);
        }

        [Test]
        public void HeavyAndTallSharesMatchTheGdd()
        {
            string[] heavy = { "RigidTruck", "Truck", "LongCombination", "Bus", "ArticulatedBus" };
            var heavyShare = Enumerable.Range(0, 4).Select(p => Road.Types.Where(t => heavy.Contains(t.Name)).Sum(t => t.PhaseWeights[p])).ToArray();
            CollectionAssert.AreEqual(new double[] { 0, 17, 29, 40 }, heavyShare);
            var tall = Enumerable.Range(0, 4).Select(p => Road.Types.Where(t => t.IsTall).Sum(t => t.PhaseWeights[p])).ToArray();
            CollectionAssert.AreEqual(new double[] { 16, 34, 47, 56 }, tall);
        }

        [Test]
        public void TallFlagIsHeightFrom2Point4()
        {
            string[] notTall = { "Compact", "Hatchback", "Sedan", "SUV", "Sports", "MPV", "Motorbike" };
            foreach (var t in Road.Types) Assert.AreEqual(!notTall.Contains(t.Name), t.IsTall, t.Name);
        }

        [Test]
        public void OnlyTheMotorbikeIsNonLethal()
        {
            foreach (var t in Road.Types) Assert.AreEqual(t.Name == "Motorbike", t.NonLethal, t.Name);
        }

        [Test]
        public void EveryClassHasVariantsWithSharesSummingToOne()
        {
            Assert.GreaterOrEqual(Road.Variants.Length, 30);
            for (int c = 0; c < Road.Types.Length; c++)
            {
                Assert.IsNotEmpty(Road.VariantsOf[c], Road.Types[c].Name);
                Assert.AreEqual(1, Road.VariantsOf[c].Sum(i => Road.Variants[i].Share), 1e-9, Road.Types[c].Name);
            }
            Assert.AreEqual(0.05, Road.Variants.First(v => v.Name == "Police car").Share, 1e-9);
            Assert.AreEqual(0.05, Road.Variants.First(v => v.Name == "Ambulance").Share, 1e-9);
        }

        // ---- GDD 4.1.4 / 2.5: head-on is game over for every class except the bike ----

        [Test]
        public void HeadOnWithTheSlowestVehicleIsGameOverForEveryClassButTheBike()
        {
            for (int c = 0; c < Road.Types.Length; c++)
            {
                var t = Road.Types[c];
                var r = HeadOn(0, c, t.MinKmh); // player stopped, slowest vehicle of the class
                if (t.NonLethal) Assert.IsFalse(r.Crashed, t.Name);
                else
                {
                    Assert.IsTrue(r.Crashed, t.Name);
                    Assert.GreaterOrEqual(r.LastImpulse, 19, t.Name); // GDD 4.1.4: every car and truck is above J = 19
                }
            }
        }

        [Test]
        public void SideHitLimitsMatchTheGdd()
        {
            // m_red = 1.5 m / (1.5 + m): a side hit ends the run above 6 / m_red (GDD 4.1.4).
            double Limit(string cls) { var m = Road.Types[Road.Index(cls)].Mass / 1000; return 6.0 / (1.5 * m / (1.5 + m)); }
            Assert.AreEqual(9.5, Limit("Compact"), 0.1);
            Assert.AreEqual(7.7, Limit("Sedan"), 0.1);
            Assert.AreEqual(6.9, Limit("SUV"), 0.1);
            Assert.AreEqual(4.2, Limit("Truck"), 0.1);
            Assert.AreEqual(4.4, Limit("RigidTruck"), 0.1);
        }

        // ---- motorbikes (GDD 2.5 worked examples) ----

        [Test]
        public void HeadOnBikeAt100KmhLeaves40Kmh()
        {
            var r = HeadOn(100, Bike, 120);
            Assert.IsFalse(r.Crashed);
            Assert.AreEqual(13.1, r.LastImpulse, 0.2);
            Assert.AreEqual(40, r.V * 3.6, 1.5); // loss clamp(105, 40 %, 60 %) = 60 km/h
            Assert.AreEqual(1, r.BikeHits);
            Assert.AreEqual(0, r.Vehicles.Count); // the bike is gone
        }

        [Test]
        public void HeadOnBikeAt300KmhLeaves120Kmh()
        {
            var r = HeadOn(300, Bike, 200);
            Assert.IsFalse(r.Crashed);
            Assert.AreEqual(29.8, r.LastImpulse, 0.4);
            Assert.AreEqual(120, r.V * 3.6, 3); // loss clamp(238, 120, 180) = 180 km/h
        }

        [Test]
        public void SideSwipeOfABikeCosts40Percent()
        {
            var r = NewRun(200);
            double hw = Road.PlayerWidth / 2;
            // The bike alongside, the player moving into it at 2 m/s.
            r.X = Road.LaneX(0) + 0.4 + hw + 0.005; r.Side = -2; r.Yaw = Math.Atan2(r.Side, r.V);
            r.Vehicles.Add(new Vehicle { Type = Bike, Lane = 0, Z = 0, Speed = 150 / 3.6 });
            for (int i = 0; i < 20 && r.BikeHits == 0; i++) r.Step(0.001, -2.0 / 9.8, false);
            Assert.IsFalse(r.Crashed);
            Assert.AreEqual(1, r.BikeHits);
            Assert.AreEqual(120, r.V * 3.6, 2.5); // 40 % of 200 km/h
            Assert.Greater(r.Side, 0); // pushed away from the bike
        }

        [Test]
        public void ASecondBikeWithin4SecondsIsGameOver()
        {
            var r = NewRun(150);
            double hl = (Road.PlayerLength + Road.Types[Bike].Length) / 2 + 0.3;
            r.Vehicles.Add(new Vehicle { Type = Bike, Lane = 1, Z = hl, Speed = 100 / 3.6 });
            for (int i = 0; i < 10 && r.BikeHits == 0; i++) r.Step(Dt, 0, true);
            Assert.AreEqual(1, r.BikeHits);
            Assert.IsFalse(r.Crashed);
            r.Vehicles.Add(new Vehicle { Type = Bike, Lane = 1, Z = hl, Speed = 100 / 3.6 });
            for (int i = 0; i < 10 && !r.Crashed; i++) r.Step(Dt, 0, true);
            Assert.IsTrue(r.Crashed); // plowing through a group of riders
        }

        [Test]
        public void ABikeAfter4SecondsIsSurvivedAgain()
        {
            var r = NewRun(150);
            double hl = (Road.PlayerLength + Road.Types[Bike].Length) / 2 + 0.3;
            r.Vehicles.Add(new Vehicle { Type = Bike, Lane = 1, Z = hl, Speed = 100 / 3.6 });
            for (int i = 0; i < 10 && r.BikeHits == 0; i++) r.Step(Dt, 0, true);
            for (int i = 0; i < 60 * 5; i++) r.Step(Dt, 0, true);
            r.Vehicles.Add(new Vehicle { Type = Bike, Lane = 1, Z = hl, Speed = 100 / 3.6 });
            double x = r.X;
            for (int i = 0; i < 10 && r.BikeHits < 2; i++) r.Step(Dt, 0, true);
            Assert.AreEqual(2, r.BikeHits);
            Assert.IsFalse(r.Crashed);
        }

        [Test]
        public void BikesKeep600MetresAtSpawn()
        {
            var r = new Run(new Tuning { CrashImpulse = double.MaxValue, DensityScale = 6 }, 5, Track.Constant(0, 0)) { Started = true };
            r.Time = 130; // phase 4: 4 % bikes
            int bikes = 0;
            for (int i = 0; i < 60 * 240; i++)
            {
                var before = r.Vehicles.Where(v => v.T.NonLethal).ToList();
                r.V = 200 / 3.6; r.X = -Road.RailX + 2;
                r.Step(Dt, 0, false);
                foreach (var b in r.Vehicles.Where(v => v.T.NonLethal && !before.Contains(v)))
                {
                    bikes++;
                    foreach (var o in r.Vehicles.Where(v => v.T.NonLethal && v != b && v.Z > -40))
                        Assert.GreaterOrEqual(Math.Abs(o.Z - b.Z), 600 - 40, "bike spacing at spawn"); // vehicles move 1 step before the check
                }
            }
            Assert.Greater(bikes, 3);
        }

        // ---- variants ----

        [Test]
        public void AVariantNeverFollowsItselfInTheSameClass()
        {
            var r = new Run(new Tuning { CrashImpulse = double.MaxValue, DensityScale = 6 }, 9, Track.Constant(0, 0)) { Started = true };
            r.Time = 130;
            var last = new int[Road.Types.Length].Select(_ => -1).ToArray();
            int spawned = 0;
            for (int i = 0; i < 60 * 240; i++)
            {
                var before = r.Vehicles.ToList();
                r.V = 150 / 3.6; r.X = -Road.RailX + 2;
                r.Step(Dt, 0, false);
                foreach (var v in r.Vehicles.Where(v => !before.Contains(v)))
                {
                    spawned++;
                    if (Road.VariantsOf[v.Type].Length > 1) Assert.AreNotEqual(last[v.Type], v.Variant, Road.Types[v.Type].Name);
                    last[v.Type] = v.Variant;
                }
            }
            Assert.Greater(spawned, 300);
        }

        [Test]
        public void PoliceAndAmbulancesUseTheirOwnSpeedAndTheFastLanes()
        {
            var r = new Run(new Tuning { CrashImpulse = double.MaxValue, DensityScale = 8 }, 3, Track.Constant(0, 0)) { Started = true };
            r.Time = 130;
            int special = 0;
            for (int i = 0; i < 60 * 600; i++)
            {
                var before = r.Vehicles.ToList();
                r.V = 120 / 3.6; r.X = -Road.RailX + 2;
                r.Step(Dt, 0, false);
                foreach (var v in r.Vehicles.Where(v => !before.Contains(v) && Road.Variants[v.Variant].Overrides))
                {
                    special++;
                    var vr = Road.Variants[v.Variant];
                    Assert.GreaterOrEqual(v.Lane, 2, vr.Name);                              // the two fast lanes only
                    Assert.That(v.Speed * 3.6, Is.InRange(vr.MinKmh - 1e-6, vr.MaxKmh + 1e-6), vr.Name);
                }
            }
            Assert.Greater(special, 3);
        }

        // ---- spawn rules ----

        [Test]
        public void SpawnDistanceIsPerVehicle()
        {
            var r = NewRun(342);
            Assert.AreEqual(566, r.SpawnDistanceFor(240 / 3.6), 1.5);  // GDD 5.3: sports car
            Assert.AreEqual(508, r.SpawnDistanceFor(180 / 3.6), 1.5);  // sedan
            Assert.AreEqual(400, NewRun(100).SpawnDistanceFor(80 / 3.6), 1e-9); // slow truck, never under 400 m
            Assert.AreEqual(400, NewRun(0).SpawnDistanceFor(240 / 3.6), 1e-9);
        }

        [Test]
        public void CatchUpRuleKeepsAFastVehicleBehindASlowOneFromReachingIt()
        {
            var r = NewRun(200);
            int truck = Road.Index("Truck"), sports = Road.Index("Sports");
            r.Vehicles.Add(new Vehicle { Type = truck, Lane = 0, Z = 300, Speed = 90 / 3.6, OMax = 0.5 });
            // GDD 4.3: gap >= (v_fast - v_slow) x t_meet_slow + 20 m = about 175 m here (the tall truck asks 255 m more).
            var tooClose = new Vehicle { Type = sports, Lane = 0, Z = 400, Speed = 240 / 3.6, OMax = 0.5 };
            var farEnough = new Vehicle { Type = sports, Lane = 0, Z = 600, Speed = 240 / 3.6, OMax = 0.5 };
            Assert.IsFalse(r.Fits(tooClose));
            Assert.IsTrue(r.Fits(farEnough));
        }

        [Test]
        public void FastLightVehiclesNeedTwoSecondsBehindATallVehicle()
        {
            var r = NewRun(200);
            int truck = Road.Index("Truck"), sports = Road.Index("Sports"), sedan = Road.Index("Sedan");
            double v = 90 / 3.6;
            r.Vehicles.Add(new Vehicle { Type = truck, Lane = 0, Z = 300, Speed = v, OMax = 0.5 });
            double closing = r.V + v; // 80.6 m/s: 2.0 s = 161 m, 1.5 s = 121 m, plus half the lengths
            double lens(int c) => (Road.Types[truck].Length + Road.Types[c].Length) / 2;
            // Same speed so the catch-up rule is out of the way.
            Vehicle Behind(int c, double gap) => new Vehicle { Type = c, Lane = 0, Z = 300 + gap, Speed = v, OMax = 0.5 };
            Assert.IsFalse(r.Fits(Behind(sedan, lens(sedan) + 1.5 * closing - 5)));
            Assert.IsTrue(r.Fits(Behind(sedan, lens(sedan) + 1.5 * closing + 5)));
            Assert.IsFalse(r.Fits(Behind(sports, lens(sports) + 2.0 * closing - 5)));  // 2.0 s for a sports car
            Assert.IsTrue(r.Fits(Behind(sports, lens(sports) + 2.0 * closing + 5)));
        }

        [Test]
        public void WanderIsCappedAt30Centimetres()
        {
            var r = new Run(new Tuning { CrashImpulse = double.MaxValue, DensityScale = 5 }, 11, Track.Constant(0, 0)) { Started = true };
            for (int i = 0; i < 60 * 120; i++)
            {
                r.V = 150 / 3.6; r.X = -Road.RailX + 2;
                r.Step(Dt, 0, false);
                foreach (var v in r.Vehicles) Assert.LessOrEqual(v.A, 0.30 + 1e-9);
            }
        }
    }
}
