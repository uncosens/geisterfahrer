using System;

namespace Geisterfahrer.Core
{
    // Runtime quality ladder for the scenery (urban-spec section 5): when the average frame time stays above SlowMs for a
    // 2 s window the level goes up one step (less scenery); after several fast windows it goes back down, slowly.
    // Pure logic: the view feeds it frame times and reads Level.
    public class QualityLadder
    {
        public const int Max = 10;
        public const double SlowMs = 18, FastMs = 14, Window = 2, Warmup = 3, UpCooldown = 3, DownCooldown = 10;
        public const int FastWindowsToStepDown = 3; // 6 s of fast frames

        public int Level { get; private set; }
        public double LastAverageMs { get; private set; }
        double now, windowTime, lastChange = -1e9;
        int frames, fastWindows;

        // dt in seconds of one frame. Returns true when the level changed.
        public bool Update(double dt)
        {
            now += dt;
            if (now < Warmup) return false; // loading hitches at the start do not count
            windowTime += dt; frames++;
            if (windowTime < Window) return false;
            double avg = windowTime / frames * 1000;
            LastAverageMs = avg;
            windowTime = 0; frames = 0;
            if (avg > SlowMs)
            {
                fastWindows = 0;
                if (Level < Max && now - lastChange >= UpCooldown) { Level++; lastChange = now; return true; }
            }
            else if (avg < FastMs)
            {
                if (++fastWindows >= FastWindowsToStepDown && Level > 0 && now - lastChange >= DownCooldown) { Level--; lastChange = now; fastWindows = 0; return true; }
            }
            else fastWindows = 0;
            return false;
        }
    }

    // Fairness numbers for scenery (GDD 6.8) in one place, so tests can check them.
    public static class NoBuild
    {
        // F2: clearance on the inner side of a curve of radius r (m): max(15, 31250 / r - 1.6).
        public static double InnerClearance(double r) => Math.Max(15, 31250 / r - 1.6);
    }
}
