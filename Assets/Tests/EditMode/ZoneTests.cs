using System.Collections.Generic;
using Geisterfahrer.Core;
using NUnit.Framework;

namespace Geisterfahrer.Tests
{
    public class ZoneTests
    {
        [Test] public void StartIsCountry() => Assert.AreEqual(Zone.Country, new Zones(3).At(1000));

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
                if (k != last) { Assert.GreaterOrEqual(s - from, 590, $"run before {s}"); last = k; from = s; }
            }
            Assert.AreEqual(4, seen.Count);
        }
    }
}
