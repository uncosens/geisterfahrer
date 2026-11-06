using Geisterfahrer.Core;
using NUnit.Framework;

namespace Geisterfahrer.Tests
{
    // Speed curve from GDD 2.2.
    public class CarTests
    {
        static readonly Tuning T = new Tuning();
        const double Dt = 0.001;

        static double TimeTo(double fromKmh, double toKmh)
        {
            double v = fromKmh / 3.6, t = 0;
            while (v < toKmh / 3.6) { v = Car.Step(T, v, true, Dt); t += Dt; }
            return t;
        }

        static double Coast(double fromKmh, double seconds)
        {
            double v = fromKmh / 3.6;
            for (double t = 0; t < seconds; t += Dt) v = Car.Step(T, v, false, Dt);
            return v * 3.6;
        }

        [Test] public void ZeroTo100() => Assert.AreEqual(3.6, TimeTo(0, 100), 0.1);
        [Test] public void From100To200() => Assert.AreEqual(8.1, TimeTo(100, 200), 0.1);
        [Test] public void TopSpeedIs306() => Assert.AreEqual(306.2, Car.TopSpeed(T) * 3.6, 0.2);

        [Test]
        public void ThrottleRisesMonotonicallyAndNeverPassesTop()
        {
            double top = Car.TopSpeed(T), v = 0, last = -1;
            for (double t = 0; t < 300; t += 0.01)
            {
                v = Car.Step(T, v, true, 0.01);
                Assert.GreaterOrEqual(v, last);
                Assert.LessOrEqual(v, top + 1e-6);
                last = v;
            }
            Assert.AreEqual(top, v, 0.5);
        }

        [Test] public void CoastFrom303For2s() => Assert.AreEqual(288, Coast(303, 2), 1);

        [Test]
        public void CoastDecelIsLargerAtHighSpeed()
            => Assert.Less(Car.Accel(T, 300 / 3.6, false), Car.Accel(T, 100 / 3.6, false));

        [Test]
        public void CoastFrom100ReachesZeroAndStays()
        {
            double v = 100 / 3.6, t = 0;
            while (v > 0) { v = Car.Step(T, v, false, Dt); t += Dt; }
            Assert.AreEqual(62, t, 1.5);
            for (int i = 0; i < 1000; i++) v = Car.Step(T, v, false, Dt);
            Assert.AreEqual(0, v);
        }

        [Test]
        public void StartsFromZeroAtTractionLimit()
        {
            double a = Car.Accel(T, 0, true);
            Assert.IsFalse(double.IsNaN(a));
            Assert.AreEqual(0.85 * 9.81, a, 0.01);
            Assert.AreEqual(0, Car.Accel(T, 0, false));
        }

        // Tyre scrub (GDD 2.2 table): speed change in 1 s, accelerator held.
        static double OneSecond(double kmh, double steer, bool throttle)
        {
            double v = kmh / 3.6;
            for (double t = 0; t < 1; t += Dt) v = Car.Step(T, v, throttle, Dt, steer);
            return v * 3.6 - kmh;
        }

        [Test] public void Scrub200HeldFull() => Assert.AreEqual(3.2, OneSecond(200, 1, true), 0.3);
        [Test] public void Scrub200HeldLight() => Assert.AreEqual(7.1, OneSecond(200, 0.3, true), 0.3);
        [Test] public void Scrub300HeldFull() => Assert.AreEqual(-3.9, OneSecond(300, 1, true), 0.3);
        [Test] public void Scrub100UpFull() => Assert.AreEqual(-6.4, OneSecond(100, 1, false), 0.3);
        [Test] public void CeilingWhileSteeringFull() => Assert.AreEqual(243, Car.TopSpeed(T, 1) * 3.6, 2);

        [Test] public void CeilingDownhill4() => Assert.AreEqual(327, Car.TopSpeed(T, 0, -0.04) * 3.6, 1.5);
        [Test] public void CeilingUphill4() => Assert.AreEqual(286, Car.TopSpeed(T, 0, 0.04) * 3.6, 1.5);

        [Test]
        public void CoastDownhill4SettlesNear20()
        {
            double v = 100 / 3.6;
            for (double t = 0; t < 3000; t += 0.01) v = Car.Step(T, v, false, 0.01, 0, -0.04); // slow approach to balance
            Assert.AreEqual(20, v * 3.6, 2);
        }

        [Test]
        public void NoRollingBackUphill()
        {
            Assert.AreEqual(0, Car.Step(T, 0, false, 0.1, 0, 0.04));
        }
    }
}
