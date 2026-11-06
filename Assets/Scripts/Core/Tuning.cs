using System;

namespace Geisterfahrer.Core
{
    // All tuning numbers (GDD v0.3). SI units unless the name says otherwise.
    [Serializable]
    public class Tuning
    {
        // Forward force model (GDD 2.2)
        public double Mass = 1500;
        public double Power = 260e3;
        public double Mu = 0.85;
        public double CdA = 0.65;
        public double AirDensity = 1.225;
        public double Crr = 0.012;
        public double EngineBrake = 400;
        public double StartSpeedKmh = 0;
        public double ScrubFactor = 0.15;      // k_s: tyre scrub F = k_s m mu g s^2 min(1, v / ScrubFullSpeed)
        public double ScrubFullSpeed = 27.8;   // m/s (100 km/h)
        public double Superelevation = 0.04;   // q, curve drift c = max(0, v^2/R - g q)

        // Side motion
        public double MaxSideSpeed = 7;
        public double SideSpeedPerSpeed = 0.25;
        public double SteerTau = 0.12;
        public double RecenterTau = 0.15;

        // Collisions (GDD 2.5), impulse in t*m/s
        public double CrashImpulse = 6.0;
        public double SpeedLossKmhPerImpulse = 8;
        public double PushOffSideSpeed = 1.5;
        public double ScrapeKmhPerSecond = 15;
        public double MaxStepMove = 0.5;

        // Spawning (GDD 5.3, 4.2, 4.3)
        public double MinSpawnDistance = 400;
        public double SpawnWarningTime = 3.5;
        public double SameLaneGapTime = 1.5;
        public double LaneMargin = 0.10;       // body never closer than this to the lane line
        public double CorridorWidth = 2.85;    // player 1.85 + 2 x 0.5
        public double CorridorSpeed = 4;       // max corridor side speed, m/s
        public double CorridorRailGap = 0.2;
        public double MeetPad = 0.3;           // s added before and after a vehicle's meet window
        public double TunnelDensity = 0.7;
    }
}
