using System;

namespace Geisterfahrer.Core
{
    // Forward force model (GDD 2.2): a = (F_drive - F_drag - F_roll - F_brake) / m.
    public static class Car
    {
        const double G = 9.81;

        public static double Accel(Tuning t, double v, bool throttle)
        {
            double drive = throttle ? Math.Min(t.Power / Math.Max(v, 0.1), t.Mu * t.Mass * G) : 0;
            double drag = 0.5 * t.AirDensity * t.CdA * v * v;
            double roll = v > 0 ? t.Crr * t.Mass * G : 0;
            double brake = !throttle && v > 0 ? t.EngineBrake : 0;
            return (drive - drag - roll - brake) / t.Mass;
        }

        // One step; speed never goes below 0 (no reverse).
        public static double Step(Tuning t, double v, bool throttle, double dt)
            => Math.Max(0, v + Accel(t, v, throttle) * dt);

        // Speed ceiling where drive force = drag + rolling (bisection).
        public static double TopSpeed(Tuning t)
        {
            double lo = 1, hi = 200;
            for (int i = 0; i < 60; i++)
            {
                double mid = (lo + hi) / 2;
                if (Accel(t, mid, true) > 0) lo = mid; else hi = mid;
            }
            return lo;
        }
    }
}
