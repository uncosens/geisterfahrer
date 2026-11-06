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

        // Spawning (GDD 5.2, 4.2)
        public double MinSpawnDistance = 400;
        public double SpawnWarningTime = 3.5;
        public double CrossLaneGap = 10;
        public double SameLaneGapTime = 1.5;
    }
}
