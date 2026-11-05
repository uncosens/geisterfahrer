using System;

namespace Geisterfahrer.Core
{
    // 2D box contacts on the road plane (x, z) and the impulse rule (GDD 2.5).
    public static class Contacts
    {
        // Player box (rotated by yaw) vs an axis-aligned vehicle box. SAT on all 4 axes for the overlap test.
        // Normal (nx, nz) points from player to vehicle.
        // ponytail: the normal is picked from the vehicle's faces only (world x or z). The player's yaw is small
        // (< 6 deg), and its own face axes would mix the huge closing speed into v_n on a side touch.
        public static bool Box(double px, double yaw, double hl, double hw,
            double ox, double oz, double ohl, double ohw,
            out double nx, out double nz, out double depth)
        {
            nx = nz = depth = 0;
            double c = Math.Cos(yaw), s = Math.Sin(yaw);
            double dx = ox - px, dz = oz; // player is at z = 0

            // Player's own axes: right (c, -s), forward (s, c).
            if (Gap(dx * c - dz * s, hw, ohw * Math.Abs(c) + ohl * Math.Abs(s))) return false;
            if (Gap(dx * s + dz * c, hl, ohw * Math.Abs(s) + ohl * Math.Abs(c))) return false;

            // World axes: x and z.
            double ox1 = Overlap(dx, hw * Math.Abs(c) + hl * Math.Abs(s), ohw);
            double oz1 = Overlap(dz, hw * Math.Abs(s) + hl * Math.Abs(c), ohl);
            if (ox1 <= 0 || oz1 <= 0) return false;

            if (ox1 < oz1) { nx = Math.Sign(dx); depth = ox1; }
            else { nz = Math.Sign(dz); depth = oz1; }
            if (nx == 0 && nz == 0) nz = 1;
            return true;
        }

        static bool Gap(double d, double a, double b) => Math.Abs(d) >= a + b;
        static double Overlap(double d, double a, double b) => a + b - Math.Abs(d);

        // How far the player's box sticks past a rail. side +1 = median rail (right), -1 = outer rail (left).
        public static double RailDepth(double px, double yaw, double hl, double hw, int side)
        {
            double ex = hw * Math.Cos(yaw) + hl * Math.Abs(Math.Sin(yaw));
            return side * px + ex - Road.RailX;
        }

        // J = m_red * v_n in t*m/s. otherMass = infinity for a rail.
        public static double Impulse(double playerMass, double otherMass, double vn)
        {
            double mRed = double.IsInfinity(otherMass) ? playerMass : playerMass * otherMass / (playerMass + otherMass);
            return mRed / 1000 * vn;
        }
    }
}
