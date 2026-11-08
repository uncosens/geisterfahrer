using System;
using System.Collections.Generic;

namespace Geisterfahrer.Core
{
    public enum Zone { Country, Suburb, City, Industrial }

    // Scenery zones along the road (GDD 6.7), a function of road distance s. Start: suburb for 800-1200 m, then by chance.
    // Order rule: countryside never touches the city directly. View only for now; the tests and the fairness rules read it.
    public class Zones
    {
        readonly Random rng;
        readonly List<double> ends = new List<double>();
        readonly List<Zone> kinds = new List<Zone> { Zone.Suburb };

        // Zone lengths in metres (GDD 6.7), indexed by Zone.
        static readonly double[][] Len = { new[] { 2000.0, 5000 }, new[] { 1000.0, 2500 }, new[] { 1200.0, 3000 }, new[] { 800.0, 2000 } };
        // Next zone by chance (Country, Suburb, City, Industrial) from the current one.
        static readonly double[][] Next =
        {
            new[] { 0, 0.85, 0, 0.15 },      // from Country
            new[] { 0.3, 0, 0.5, 0.2 },      // from Suburb
            new[] { 0, 0.6, 0, 0.4 },        // from City
            new[] { 0.2, 0.5, 0.3, 0 },      // from Industrial
        };

        public Zones(int seed) { rng = new Random(seed * 31 + 7); ends.Add(800 + rng.NextDouble() * 400); }

        public Zone At(double s)
        {
            while (ends[ends.Count - 1] <= s) Extend();
            return kinds[ends.FindIndex(e => s < e)];
        }

        // True within `margin` metres of a zone border: the scenery thins out there.
        public bool NearBorder(double s, double margin) => At(s - margin) != At(s + margin);

        void Extend()
        {
            var p = Next[(int)kinds[kinds.Count - 1]];
            double r = rng.NextDouble(), acc = 0;
            int z = 0;
            for (; z < 3; z++) { acc += p[z]; if (r < acc && p[z] > 0) break; }
            if (z == 3 && p[3] == 0) z = Array.FindIndex(p, x => x > 0);
            kinds.Add((Zone)z);
            var l = Len[z];
            ends.Add(ends[ends.Count - 1] + l[0] + rng.NextDouble() * (l[1] - l[0]));
        }
    }
}
