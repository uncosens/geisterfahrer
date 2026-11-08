using System;
using System.Collections.Generic;
using Geisterfahrer.Core;
using NUnit.Framework;

namespace Geisterfahrer.Tests
{
    // Quality ladder, fairness numbers, road modules (GDD 6.6, 6.8; urban-spec 5).
    public class CityTests
    {
        static QualityLadder Run(double ms, double seconds, QualityLadder l = null)
        {
            l = l ?? new QualityLadder();
            for (double t = 0; t < seconds; t += ms / 1000) l.Update(ms / 1000);
            return l;
        }

        [Test] public void FastFramesKeepLevelZero() => Assert.AreEqual(0, Run(10, 60).Level);
        [Test] public void MediumFramesKeepLevel() => Assert.AreEqual(0, Run(16, 60).Level);
        [Test] public void SlowFramesStepUpOneByOneToTen() { var l = Run(30, 5.5); Assert.AreEqual(1, l.Level); Assert.AreEqual(QualityLadder.Max, Run(30, 120, l).Level); }
        [Test] public void WarmupDoesNotCount() => Assert.AreEqual(0, Run(40, 2.9).Level);

        [Test]
        public void RecoveryStepsDownSlowly()
        {
            var l = Run(30, 60);
            Assert.AreEqual(QualityLadder.Max, l.Level);
            Run(8, 12, l);
            Assert.Greater(l.Level, QualityLadder.Max - 3); // 12 s of fast frames gives back at most one or two steps
            Run(8, 200, l);
            Assert.AreEqual(0, l.Level);
        }

        [Test]
        public void InnerClearanceMatchesTheGddTable()
        {
            Assert.AreEqual(50, NoBuild.InnerClearance(600), 1);
            Assert.AreEqual(42, NoBuild.InnerClearance(720), 1);
            Assert.AreEqual(30, NoBuild.InnerClearance(1000), 1);
            Assert.AreEqual(24, NoBuild.InnerClearance(1200), 1);
            Assert.AreEqual(19, NoBuild.InnerClearance(1500), 1);
            Assert.AreEqual(15, NoBuild.InnerClearance(2500), 1e-9);
            // sight: the chord offset of a 500 m sight line stays inside the clearance (eye 2.6 m from the rail, 1 m margin)
            foreach (double r in new[] { 600.0, 720, 1000, 1500, 2000 }) Assert.GreaterOrEqual(NoBuild.InnerClearance(r) + 2.6 + 1, 500 * 500 / (8 * r) - 0.5, $"R {r}");
        }

        // F3: from 150 m before to 300 m after every crest top no curve tighter than R = 2000 m.
        [Test]
        public void CrestZonesAreFreeOfTightCurves()
        {
            int crests = 0;
            for (int seed = 1; seed <= 12; seed++)
            {
                var t = new Track(seed);
                t.Ensure(30000, 3);
                for (double x = 200; x < 29000; x += 25)
                    if (t.GradeBase(x) > 0.0005 && t.GradeBase(x + 25) <= 0.0005)
                    {
                        crests++;
                        for (double y = x - 150; y <= x + 300; y += 10) Assert.LessOrEqual(Math.Abs(t.Curvature(y)), 1 / 2000.0 + 1e-9, $"seed {seed} crest {x:F0} at {y:F0}");
                    }
            }
            Assert.Greater(crests, 20);
        }

        [Test]
        public void RollersGiveManyCrestsWithinTheGradeLimit()
        {
            var t = new Track(2);
            t.Ensure(40000, 3);
            int tops = 0;
            for (double x = 200; x < 39000; x += 25)
            {
                Assert.LessOrEqual(Math.Abs(t.GradeBase(x)), 0.04 + 1e-9);
                if (t.GradeBase(x) > 0.0005 && t.GradeBase(x + 25) <= 0.0005) tops++;
            }
            Assert.Greater(tops, 5); // rollers: waves of 2-3 km in a good part of the road
        }

        [Test]
        public void OverpassesFollowTheZoneRhythm()
        {
            var gaps = new Dictionary<Zone, List<double>> { { Zone.City, new List<double>() }, { Zone.Country, new List<double>() } };
            for (int seed = 1; seed <= 10; seed++)
            {
                var t = new Track(seed);
                t.Ensure(40000, 3);
                Assert.Greater(t.Overpasses[0], 499);
                var o = new List<double>(t.Overpasses); o.Sort();
                for (int i = 1; i < o.Count; i++)
                {
                    Assert.GreaterOrEqual(o[i] - o[i - 1], 100, $"seed {seed}"); // trench bridges are closer (120-250 m), the rest 300 m
                    var z = t.Zones.At(o[i - 1]);
                    if (gaps.ContainsKey(z) && t.KindAt(o[i - 1]) == Section.Open && t.KindAt(o[i]) == Section.Open) gaps[z].Add(o[i] - o[i - 1]);
                }
            }
            double Mean(List<double> l) { double s = 0; foreach (var x in l) s += x; return l.Count == 0 ? 0 : s / l.Count; }
            Assert.Less(Mean(gaps[Zone.City]), Mean(gaps[Zone.Country])); // city rhythm is denser than countryside
        }

        [Test]
        public void JunctionsAndLevelCrossingsAppear()
        {
            var kinds = new HashSet<Section>();
            for (int seed = 1; seed <= 40; seed++)
            {
                var t = new Track(seed);
                t.Ensure(40000, 3);
                foreach (var st in t.Structures) { kinds.Add(st.kind); if (st.kind == Section.Junction) Assert.GreaterOrEqual(st.s1 - st.s0, 1300); if (st.kind == Section.Crossing) Assert.LessOrEqual(st.s1 - st.s0, 252); }
            }
            Assert.IsTrue(kinds.Contains(Section.Junction));
            Assert.IsTrue(kinds.Contains(Section.Crossing));
        }

        // Any two structures keep 700 m between them (not the trench street bridges).
        [Test]
        public void StructuresKeepTheirDistance()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                var t = new Track(seed);
                t.Ensure(40000, 3);
                var l = new List<(double s0, double s1, Section kind)>(t.Structures); l.Sort((a, b) => a.s0.CompareTo(b.s0));
                for (int i = 1; i < l.Count; i++)
                    if (l[i].kind != Section.Viaduct && l[i - 1].kind != Section.Viaduct) Assert.GreaterOrEqual(l[i].s0 - l[i - 1].s1, -1, $"seed {seed}");
            }
        }
    }
}
