using System;

namespace Geisterfahrer.Core
{
    // All tuning numbers (GDD v0.3). SI units unless the name says otherwise.
    [Serializable]
    public class Tuning
    {
        // Forward force model (GDD 2.2)
        public double Mass = 1500;
        public double Power = 175e3;           // 175 kW at the wheels (decisions 73, 79: 300 km/h at 49 s); was 165, 155, 140
        public double Mu = 0.85;               // grip: scrub and curve drift
        public double DriveMu = 0.55;          // traction limit of the drive force (0-100 in 5.7 s); was 0.60
        public double CdA = 0.30;             // m^2, was 0.65: flat ceiling 328 km/h
        public double AirDensity = 1.225;
        public double Crr = 0.012;
        public double EngineBrake = 400;
        public double StartSpeedKmh = 0;
        public double ScrubFactor = 0.15;      // k_s: tyre scrub F = k_s m mu g s^2 min(1, v / ScrubFullSpeed)
        public double ScrubFullSpeed = 27.8;   // m/s (100 km/h)
        public double Superelevation = 0.04;   // q, curve drift c = max(0, v^2/R - g q)

        // Side motion
        public double MaxSideSpeed = 9.8;       // was 7 (+40 %, decision 33)
        public double SideSpeedPerSpeed = 0.35; // was 0.25 (+40 %)
        public double SteerTau = 0.12;
        public double RecenterTau = 0.30;       // was 0.15: slower return to straight

        // Collisions (GDD 2.5), impulse in t*m/s
        public double CrashImpulse = 6.0;
        public double SpeedLossKmhPerImpulse = 8;
        public double PushOffSideSpeed = 1.5;
        public double ScrapeKmhPerSecond = 15;
        public double MaxStepMove = 0.5;
        // Non-lethal vehicles (motorbikes, GDD 2.5): loss = clamp(8 km/h x J, 0.40 v, 0.60 v); a second contact within 4 s is game over.
        public double NonLethalMinLoss = 0.40, NonLethalMaxLoss = 0.60, NonLethalChainTime = 4;

        // Spawning (GDD 5.3, 4.2, 4.3)
        public double DensityScale = 1.4;      // +40 % traffic in every phase (decision 35)
        public double MinSpawnDistance = 400;
        public double SpawnWarningTime = 3.5;
        public double SameLaneGapTime = 1.5;
        public double LaneMargin = 0.10;       // body never closer than this to the lane line
        public double CorridorWidth = 2.85;    // player 1.85 + 2 x 0.5
        public double CorridorSpeed = 4;       // max corridor side speed, m/s
        public double CorridorRailGap = 0.2;
        public double MeetPad = 0.3;           // s added before and after a vehicle's meet window
        public double TunnelDensity = 0.7;
        public double FastBehindTallTime = 2.0; // sports cars, police, ambulances and bikes behind a tall vehicle (GDD 5.5)
        public double CatchUpMargin = 20;       // m, GDD 4.3 catch-up rule
        public double BikeSpacing = 600;        // m between motorbikes at spawn (GDD 2.5)
        public double WanderCap = 0.30;         // m, wander amplitude cap (GDD 4.2)
    }
}
