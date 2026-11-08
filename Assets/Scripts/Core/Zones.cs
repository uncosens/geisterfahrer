using System;
using System.Collections.Generic;

namespace Geisterfahrer.Core
{
    public enum Zone { Country, Suburb, City, Industrial }

    // Scenery zones along the road, a function of road distance s: runs of 600-1600 m, chained by chance.
    // View only for now; Core can use it later (for example fairness rules near interchanges).
    public class Zones
    {
        public const double CountryStart = 1500; // calm start
        readonly Random rng;
        readonly List<double> ends = new List<double> { CountryStart };
        readonly List<Zone> kinds = new List<Zone> { Zone.Country };

        // Next zone by chance, from the current one.
        static readonly double[][] Next =
        {
            new[] { 0.2, 0.6, 0, 0.2 },   // from Country: Country, Suburb, City, Industrial
            new[] { 0.3, 0, 0.5, 0.2 },   // from Suburb
            new[] { 0, 0.7, 0, 0.3 },     // from City
            new[] { 0.5, 0.5, 0, 0 },     // from Industrial
        };

        public Zones(int seed) { rng = new Random(seed * 31 + 7); }

        public Zone At(double s)
        {
            while (ends[ends.Count - 1] <= s) Extend();
            return kinds[ends.FindIndex(e => s < e)];
        }

        void Extend()
        {
            var p = Next[(int)kinds[kinds.Count - 1]];
            double r = rng.NextDouble(), acc = 0;
            int z = 0;
            for (; z < 3; z++) { acc += p[z]; if (r < acc) break; }
            if (p[z] == 0) z = Array.FindIndex(p, x => x > 0); // rounding guard
            kinds.Add((Zone)z);
            ends.Add(ends[ends.Count - 1] + 600 + rng.NextDouble() * 1000);
        }
    }
}
