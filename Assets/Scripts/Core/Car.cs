using System;

namespace Geisterfahrer.Core
{
    // Forward force model (GDD 2.2): a = (F_drive - F_drag - F_roll - F_grade - F_brake - F_steer) / m.
    public static class Car
    {
        const double G = 9.81;

        // steer: applied steering input -1..1 (tyre scrub grows with its square); grade: rise/run, + uphill.
        public static double Accel(Tuning t, double v, bool throttle, double steer = 0, double grade = 0)
        {
            double drive = throttle ? Math.Min(t.Power / Math.Max(v, 0.1), t.Mu * t.Mass * G) : 0;
            double drag = 0.5 * t.AirDensity * t.CdA * v * v;
            double roll = v > 0 ? t.Crr * t.Mass * G : 0;
            double brake = !throttle && v > 0 ? t.EngineBrake : 0;
            double scrub = t.ScrubFactor * t.Mass * t.Mu * G * steer * steer * Math.Min(1, v / t.ScrubFullSpeed);
            return (drive - drag - roll - brake - scrub - t.Mass * G * grade) / t.Mass;
        }

        // One step; speed never goes below 0 (no reverse, no rolling back).
        public static double Step(Tuning t, double v, bool throttle, double dt, double steer = 0, double grade = 0)
            => Math.Max(0, v + Accel(t, v, throttle, steer, grade) * dt);

        // Speed ceiling where the forces balance with the accelerator held (bisection).
        public static double TopSpeed(Tuning t, double steer = 0, double grade = 0)
        {
            double lo = 1, hi = 200;
            for (int i = 0; i < 60; i++)
            {
                double mid = (lo + hi) / 2;
                if (Accel(t, mid, true, steer, grade) > 0) lo = mid; else hi = mid;
            }
            return lo;
        }
    }
}
