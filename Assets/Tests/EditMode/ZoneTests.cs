using System.Collections.Generic;
using Geisterfahrer.Core;
using NUnit.Framework;

namespace Geisterfahrer.Tests
{
    public class ZoneTests
    {
        [Test] public void StartIsSuburbThenNotCountry()
        {
            var z = new Zones(3);
            Assert.AreEqual(Zone.Suburb, z.At(700));
        }

        [Test]
        public void SameSeedSameZones()
        {
            var a = new Zones(5); var b = new Zones(5);
            for (double s = 0; s < 20000; s += 137) Assert.AreEqual(a.At(s), b.At(s));
        }

        [Test]
        public void AllKindsAppearAndRunsAreLong()
        {
            var seen = new HashSet<Zone>();
            var z = new Zones(1);
            Zone last = z.At(0); double from = 0;
            for (double s = 0; s < 60000; s += 10)
            {
                var k = z.At(s); seen.Add(k);
                if (k != last) { Assert.GreaterOrEqual(s - from, 790, $"run before {s}"); last = k; from = s; }
            }
            Assert.AreEqual(4, seen.Count);
        }

        [Test]
        public void CountryNeverTouchesCity()
        {
            for (int seed = 1; seed <= 20; seed++)
            {
                var z = new Zones(seed);
                Zone last = z.At(0);
                for (double s = 0; s < 80000; s += 10)
                {
                    var k = z.At(s);
                    if (k != last) Assert.IsFalse((k == Zone.City && last == Zone.Country) || (k == Zone.Country && last == Zone.City), $"seed {seed} at {s}");
                    last = k;
                }
            }
        }
    }
}
