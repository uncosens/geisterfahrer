using System;
using System.Linq;

namespace Geisterfahrer.Core
{
    // Road and vehicle data (GDD 3.1, 4.1). Road space: d (here x) = lateral offset from the road centre,
    // positive to the player's right; s = distance along the road. The view may bend the road; Core never does.
    // Lane index 0 = slowest lane (player's left, GDD "L4"), LaneCount - 1 = fastest (next to the median, GDD "L1").
    public static class Road
    {
        public const int LaneCount = 4;
        public const double LaneWidth = 3.75, EdgeStrip = 0.75;
        public const double RailX = LaneCount * LaneWidth / 2 + EdgeStrip; // 8.25 m: 16.5 m rail to rail (GDD 3.1)
        public static double LaneX(int lane) => (lane - (LaneCount - 1) / 2.0) * LaneWidth;

        public const double PlayerLength = 4.5, PlayerWidth = 1.85;
        public const double FastestTrafficKmh = 240; // sports car (GDD 5.3)

        // The 16 classes of GDD 4.1.1. Lane shares and phase weights follow 4.1.1 / 4.1.2; lane shares are listed
        // slowest lane first (GDD lists L1..L4). Phase weights sum to 100 in every phase.
        public static readonly VehicleType[] Types =
        {
            new VehicleType("Compact", 3.7, 1.7, 1.55, 1100, 110, 130, new[] { 33.0, 33, 34, 0 }, new[] { 14.0, 10, 6, 4 }),
            new VehicleType("Hatchback", 4.15, 1.76, 1.45, 1300, 115, 145, new[] { 30.0, 30, 30, 10 }, new[] { 22.0, 17, 12, 8 }),
            new VehicleType("Sedan", 4.8, 1.85, 1.5, 1600, 130, 180, new[] { 0.0, 25, 35, 40 }, new[] { 26.0, 19, 15, 12 }),
            new VehicleType("SUV", 4.9, 1.92, 1.8, 2100, 120, 160, new[] { 10.0, 35, 40, 15 }, new[] { 14.0, 12, 9, 7 }),
            new VehicleType("Sports", 4.5, 1.9, 1.3, 1500, 180, 240, new[] { 0.0, 0, 10, 90 }, new[] { 2.0, 2, 4, 6 }),
            new VehicleType("MPV", 4.6, 1.85, 1.7, 1800, 110, 150, new[] { 30.0, 40, 30, 0 }, new[] { 6.0, 5, 4, 3 }),
            new VehicleType("Van", 5.5, 2.0, 2.5, 2500, 110, 140, new[] { 45.0, 45, 10, 0 }, new[] { 10.0, 8, 7, 6 }),
            new VehicleType("LongVan", 5.9, 2.0, 2.6, 3000, 100, 130, new[] { 45.0, 45, 10, 0 }, new[] { 6.0, 5, 5, 5 }),
            new VehicleType("Motorbike", 2.2, 0.8, 1.4, 250, 120, 200, new[] { 25.0, 25, 25, 25 }, new[] { 0.0, 1, 3, 4 }, nonLethal: true),
            new VehicleType("Motorhome", 7.5, 2.3, 3.2, 3500, 90, 110, new[] { 70.0, 30, 0, 0 }, new[] { 0.0, 1, 2, 2 }),
            new VehicleType("CarWithCaravan", 11.0, 2.3, 2.6, 3000, 80, 100, new[] { 80.0, 20, 0, 0 }, new[] { 0.0, 3, 4, 3 }),
            new VehicleType("RigidTruck", 9.0, 2.5, 3.8, 14000, 80, 90, new[] { 85.0, 15, 0, 0 }, new[] { 0.0, 4, 8, 10 }),
            new VehicleType("Truck", 16.5, 2.55, 4.0, 35000, 80, 90, new[] { 80.0, 20, 0, 0 }, new[] { 0.0, 7, 12, 17 }),
            new VehicleType("LongCombination", 19.0, 2.55, 4.0, 36000, 80, 90, new[] { 80.0, 20, 0, 0 }, new[] { 0.0, 1, 3, 4 }),
            new VehicleType("Bus", 12.0, 2.55, 4.0, 18000, 90, 100, new[] { 80.0, 20, 0, 0 }, new[] { 0.0, 4, 5, 7 }),
            new VehicleType("ArticulatedBus", 18.0, 2.55, 3.4, 20000, 85, 100, new[] { 80.0, 20, 0, 0 }, new[] { 0.0, 1, 1, 2 }),
        };

        public static int Index(string className) => Array.FindIndex(Types, t => t.Name == className);

        // Variants (GDD 4.1.3): a model and paint on top of a class, same physics. Only the variants that have a model today.
        // Share = share inside the class (equal, police 5 % of Sedan, ambulance 5 % of LongVan); police and ambulance keep their
        // own speed range and use the two fast lanes only (GDD 4.1.3 rule 2).
        static readonly double[] FastLanes = { 0, 0, 50, 50 };

        public static readonly VehicleVariant[] Variants =
        {
            new VehicleVariant("City car", "Compact"),
            new VehicleVariant("Golf-class hatchback", "Hatchback"),
            new VehicleVariant("Sedan", "Sedan"), new VehicleVariant("Estate", "Sedan"), new VehicleVariant("Taxi", "Sedan"),
            new VehicleVariant("Police car", "Sedan", 0.05, 150, 200, FastLanes),
            new VehicleVariant("Mid SUV", "SUV"), new VehicleVariant("Large SUV", "SUV"),
            new VehicleVariant("Coupe", "Sports"), new VehicleVariant("Convertible", "Sports"),
            new VehicleVariant("Minivan", "MPV"),
            new VehicleVariant("Panel van", "Van"), new VehicleVariant("Delivery van", "Van"), new VehicleVariant("Camper van", "Van"),
            new VehicleVariant("Long panel van", "LongVan"),
            new VehicleVariant("Ambulance", "LongVan", 0.05, 130, 160, FastLanes),
            new VehicleVariant("Sport bike", "Motorbike"), new VehicleVariant("Touring bike", "Motorbike"),
            new VehicleVariant("Coachbuilt motorhome", "Motorhome"),
            new VehicleVariant("SUV with caravan", "CarWithCaravan"), new VehicleVariant("Estate with caravan", "CarWithCaravan"), new VehicleVariant("Minivan with caravan", "CarWithCaravan"),
            new VehicleVariant("Box truck", "RigidTruck"),
            new VehicleVariant("Box semi", "Truck"), new VehicleVariant("Tanker semi", "Truck"), new VehicleVariant("Flatbed semi", "Truck"),
            new VehicleVariant("Car transporter", "LongCombination"),
            new VehicleVariant("Coach", "Bus"), new VehicleVariant("Coach B", "Bus"),
            new VehicleVariant("Articulated line bus", "ArticulatedBus"),
        };

        // Variant indices per class, and each variant's normalised share inside its class.
        public static readonly int[][] VariantsOf = Types.Select((t, c) => Variants.Select((v, i) => (v, i)).Where(x => x.v.ClassName == t.Name).Select(x => x.i).ToArray()).ToArray();

        static Road()
        {
            for (int c = 0; c < Types.Length; c++)
            {
                var ids = VariantsOf[c];
                double fixedShare = ids.Where(i => Variants[i].ShareHint > 0).Sum(i => Variants[i].ShareHint);
                int open = ids.Count(i => Variants[i].ShareHint <= 0);
                foreach (var i in ids)
                {
                    Variants[i].Class = c;
                    Variants[i].Share = Variants[i].ShareHint > 0 ? Variants[i].ShareHint : (1 - fixedShare) / Math.Max(1, open);
                }
            }
        }
    }

    // A class row of GDD 4.1.1.
    public class VehicleType
    {
        public readonly string Name;
        public readonly double Length, Width, Height, Mass, MinKmh, MaxKmh;
        public readonly double[] LaneWeights, PhaseWeights;
        public readonly bool NonLethal; // a contact never ends the run (motorbike, GDD 2.5)

        public VehicleType(string name, double length, double width, double height, double mass,
            double minKmh, double maxKmh, double[] laneWeights, double[] phaseWeights, bool nonLethal = false)
        {
            Name = name; Length = length; Width = width; Height = height; Mass = mass;
            MinKmh = minKmh; MaxKmh = maxKmh; LaneWeights = laneWeights; PhaseWeights = phaseWeights; NonLethal = nonLethal;
        }

        public bool IsTall => Height >= 2.4; // can hide what drives behind it (GDD 5.5)
    }

    // A variant row of GDD 4.1.3. Min/MaxKmh and LaneWeights override the class ones when set (police, ambulance).
    public class VehicleVariant
    {
        public readonly string Name, ClassName;
        public readonly double ShareHint, MinKmh, MaxKmh;
        public readonly double[] LaneWeights;
        public int Class;
        public double Share;

        public VehicleVariant(string name, string className, double shareHint = 0, double minKmh = 0, double maxKmh = 0, double[] laneWeights = null)
        {
            Name = name; ClassName = className; ShareHint = shareHint; MinKmh = minKmh; MaxKmh = maxKmh; LaneWeights = laneWeights;
        }

        public bool Overrides => MaxKmh > 0;
    }
}
