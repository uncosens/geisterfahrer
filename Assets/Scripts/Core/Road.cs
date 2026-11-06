namespace Geisterfahrer.Core
{
    // Road and vehicle data (GDD 3.1, 4.1). Road space: d (here x) = lateral offset from the road centre,
    // positive to the player's right; s = distance along the road. The view may bend the road; Core never does.
    // Lane index 0 = slowest lane (player's left), LaneCount - 1 = fastest (next to the median).
    public static class Road
    {
        public const int LaneCount = 4;
        public const double LaneWidth = 3.75, EdgeStrip = 0.75;
        public const double RailX = LaneCount * LaneWidth / 2 + EdgeStrip; // 8.25 m: 16.5 m rail to rail (GDD 3.1)
        public static double LaneX(int lane) => (lane - (LaneCount - 1) / 2.0) * LaneWidth;

        public const double PlayerLength = 4.5, PlayerWidth = 1.85;

        public static readonly VehicleType[] Types =
        {
            new VehicleType("Compact", 3.7, 1.7, 1.5, 1100, 110, 130, new[] { 1.0, 1, 1, 0 }, new[] { 40.0, 30, 20, 15 }),
            new VehicleType("Sedan", 4.8, 1.85, 1.45, 1600, 130, 180, new[] { 0.0, 0.25, 0.35, 0.4 }, new[] { 40.0, 35, 30, 25 }),
            new VehicleType("Van", 5.5, 2.0, 2.5, 2500, 110, 140, new[] { 0.45, 0.45, 0.1, 0 }, new[] { 20.0, 20, 20, 20 }),
            new VehicleType("Bus", 12, 2.55, 3.8, 18000, 90, 100, new[] { 0.8, 0.2, 0, 0 }, new[] { 0.0, 8, 12, 15 }),
            new VehicleType("Truck", 16.5, 2.55, 4.0, 35000, 80, 90, new[] { 0.8, 0.2, 0, 0 }, new[] { 0.0, 10, 18, 25 }),
        };

        public const double FastestTrafficKmh = 180;
    }

    public class VehicleType
    {
        public readonly string Name;
        public readonly double Length, Width, Height, Mass, MinKmh, MaxKmh;
        public readonly double[] LaneWeights, PhaseWeights;

        public VehicleType(string name, double length, double width, double height, double mass,
            double minKmh, double maxKmh, double[] laneWeights, double[] phaseWeights)
        {
            Name = name; Length = length; Width = width; Height = height; Mass = mass;
            MinKmh = minKmh; MaxKmh = maxKmh; LaneWeights = laneWeights; PhaseWeights = phaseWeights;
        }

        public bool IsTall => Height > 2;
    }
}
