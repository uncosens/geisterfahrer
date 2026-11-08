using System;

namespace Geisterfahrer.Core
{
    // Continuous shake (camera and road vibration): only while the throttle is held at 180 km/h or more.
    public static class Shake
    {
        public const double ShakeScale = 0.52; // max amplitude vs the old maximum
        public const double MinKmh = 180, FullKmh = 320;

        // 0..1, grows with speed above 180 km/h. Zero when coasting, slow or game over.
        public static double Amount(double kmh, bool throttle, bool gameOver)
            => gameOver || !throttle || kmh < MinKmh ? 0 : Math.Min(1, (kmh - MinKmh) / (FullKmh - MinKmh));
    }
}
