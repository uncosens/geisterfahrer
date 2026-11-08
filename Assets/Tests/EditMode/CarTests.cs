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

        // GDD 2.2 (decisions 53, 54, 73, 79): 175 kW, mu_drive 0.55, CdA 0.30.
        [Test] public void ZeroTo100() => Assert.AreEqual(5.5, TimeTo(0, 100), 0.3);
        [Test] public void ZeroTo200() => Assert.AreEqual(17.0, TimeTo(0, 200), 0.3);
        [Test] public void ZeroTo250() => Assert.AreEqual(28.1, TimeTo(0, 250), 0.3);
        [Test] public void ZeroTo300() => Assert.AreEqual(49.1, TimeTo(0, 300), 0.3);
        [Test] public void ZeroTo310() => Assert.AreEqual(56.5, TimeTo(0, 310), 0.6);
        [Test] public void ZeroTo320() => Assert.AreEqual(66.6, TimeTo(0, 320), 1.0);
        [Test] public void From100To200() => Assert.AreEqual(11.5, TimeTo(100, 200), 0.3);
        [Test] public void From100To250() => Assert.AreEqual(22.6, TimeTo(100, 250), 0.3);
        [Test] public void From100To300() => Assert.AreEqual(43.7, TimeTo(100, 300), 0.3);
        [Test] public void TopSpeedIs342() => Assert.AreEqual(342.5, Car.TopSpeed(T) * 3.6, 2);

        static double SpeedAt(double seconds)
        {
            double v = 0;
            for (double t = 0; t < seconds; t += Dt) v = Car.Step(T, v, true, Dt);
            return v * 3.6;
        }

        [Test] public void SpeedAfter10s() => Assert.AreEqual(150.2, SpeedAt(10), 2);
        [Test] public void SpeedAfter20s() => Assert.AreEqual(216.1, SpeedAt(20), 2);
        [Test] public void SpeedAfter30s() => Assert.AreEqual(256.3, SpeedAt(30), 2);
        [Test] public void SpeedAfter60s() => Assert.AreEqual(313.9, SpeedAt(60), 2);
        [Test] public void SpeedAfter120s() => Assert.AreEqual(339.2, SpeedAt(120), 2);

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
            Assert.AreEqual(top, v, 1);
        }

        [Test] public void CoastFrom325For2s() => Assert.AreEqual(315, Coast(325, 2), 2);
        [Test] public void CoastFrom325For10s() => Assert.AreEqual(280, Coast(325, 10), 2);
        [Test] public void CoastFrom325For20s() => Assert.AreEqual(243, Coast(325, 20), 2);
        [Test] public void CoastFrom325For60s() => Assert.AreEqual(139, Coast(325, 60), 2);

        [Test]
        public void CoastDecelIsLargerAtHighSpeed()
            => Assert.Less(Car.Accel(T, 300 / 3.6, false), Car.Accel(T, 100 / 3.6, false));

        [Test]
        public void CoastFrom100ReachesZeroAndStays()
        {
            double v = 100 / 3.6, t = 0;
            while (v > 0) { v = Car.Step(T, v, false, Dt); t += Dt; }
            Assert.AreEqual(67, t, 1.5);
            for (int i = 0; i < 1000; i++) v = Car.Step(T, v, false, Dt);
            Assert.AreEqual(0, v);
        }

        [Test]
        public void StartsFromZeroAtTractionLimit()
        {
            double a = Car.Accel(T, 0, true);
            Assert.IsFalse(double.IsNaN(a));
            Assert.AreEqual(T.DriveMu * 9.81, a, 0.01);
            Assert.AreEqual(0, Car.Accel(T, 0, false));
        }

        // Tyre scrub (GDD 2.2 table): speed change in 1 s, accelerator held.
        static double OneSecond(double kmh, double steer, bool throttle)
        {
            double v = kmh / 3.6;
            for (double t = 0; t < 1; t += Dt) v = Car.Step(T, v, throttle, Dt, steer);
            return v * 3.6 - kmh;
        }

        [Test] public void Scrub300HeldFull() => Assert.AreEqual(-2.9, OneSecond(300, 1, true), 0.5);
        [Test] public void Scrub300HeldStraight() => Assert.AreEqual(1.5, OneSecond(300, 0, true), 0.5);
        [Test] public void Scrub200HeldFull() => Assert.AreEqual(1.2, OneSecond(200, 1, true), 0.5);
        [Test] public void Scrub200HeldStraight() => Assert.AreEqual(5.6, OneSecond(200, 0, true), 0.5);
        [Test] public void Scrub100HeldFull() => Assert.AreEqual(9.2, OneSecond(100, 1, true), 0.5);
        [Test] public void Scrub100HeldStraight() => Assert.AreEqual(13.4, OneSecond(100, 0, true), 0.5);

        [Test] public void CeilingDownhill4() => Assert.AreEqual(381.5, Car.TopSpeed(T, 0, -0.04) * 3.6, 2);
        [Test] public void CeilingDownhill2() => Assert.AreEqual(362.0, Car.TopSpeed(T, 0, -0.02) * 3.6, 2);
        [Test] public void CeilingUphill2() => Assert.AreEqual(323.0, Car.TopSpeed(T, 0, 0.02) * 3.6, 2);
        [Test] public void CeilingUphill4() => Assert.AreEqual(303.8, Car.TopSpeed(T, 0, 0.04) * 3.6, 2);

        [Test]
        public void CoastDownhill4SettlesNear29()
        {
            double v = 100 / 3.6;
            for (double t = 0; t < 3000; t += 0.01) v = Car.Step(T, v, false, 0.01, 0, -0.04); // slow approach to balance
            Assert.AreEqual(29.2, v * 3.6, 2); // was 20 with CdA 0.65
        }

        [Test]
        public void NoRollingBackUphill()
        {
            Assert.AreEqual(0, Car.Step(T, 0, false, 0.1, 0, 0.04));
        }
    }
}
