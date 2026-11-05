namespace Geisterfahrer.Core
{
    // Road and vehicle data (GDD 3.1, 4.1). x = right, z = forward, road center at x = 0.
    // Lane index 0 = lane 3 (player's left, slow), 1 = lane 2, 2 = lane 1 (fast, next to the median).
    public static class Road
    {
        public const double LaneWidth = 3.75;
        public const double RailX = 6.25; // 3 lanes + 0.625 m edge strips, 12.5 m rail to rail
        public static double LaneX(int lane) => (lane - 1) * LaneWidth;

        public const double PlayerLength = 4.5, PlayerWidth = 1.85;

        public static readonly VehicleType[] Types =
        {
            new VehicleType("Compact", 3.7, 1.7, 1.5, 1100, 110, 130, new[] { 1.0, 1, 1 }, new[] { 40.0, 30, 20, 15 }),
            new VehicleType("Sedan", 4.8, 1.85, 1.45, 1600, 130, 180, new[] { 0.0, 1, 1 }, new[] { 40.0, 35, 30, 25 }),
            new VehicleType("Van", 5.5, 2.0, 2.5, 2500, 110, 140, new[] { 1.0, 1, 0 }, new[] { 20.0, 20, 20, 20 }),
            new VehicleType("Bus", 12, 2.55, 3.8, 18000, 90, 100, new[] { 0.8, 0.2, 0 }, new[] { 0.0, 8, 12, 15 }),
            new VehicleType("Truck", 16.5, 2.55, 4.0, 35000, 80, 90, new[] { 0.8, 0.2, 0 }, new[] { 0.0, 10, 18, 25 }),
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
