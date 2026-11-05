using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Geisterfahrer.Game
{
    // Left half: steering with a floating origin. Right half: accelerator. Both at once (GDD 2.2).
    public class DriveInput
    {
        const float DeadZoneDp = 10, MaxDp = 75;
        int steerFinger = -1, gasFinger = -1;
        Vector2 origin;

        public float Steer;
        public bool Gas;
        public Vector2? SteerOrigin => steerFinger >= 0 ? origin : (Vector2?)null;

        public DriveInput() => EnhancedTouchSupport.Enable();

        // blocked: screen rect (top-left origin, like OnGUI) where touches belong to the UI.
        public void Update(Rect blocked)
        {
            bool steerSeen = false, gasSeen = false;
            Steer = 0;
            float dp = (Screen.dpi > 0 ? Screen.dpi : 160) / 160;
            foreach (var t in Touch.activeTouches)
            {
                int id = t.finger.index;
                bool ending = t.phase == UnityEngine.InputSystem.TouchPhase.Ended || t.phase == UnityEngine.InputSystem.TouchPhase.Canceled;
                if (ending) continue;
                if (id == steerFinger)
                {
                    steerSeen = true;
                    float off = (t.screenPosition.x - origin.x) / dp;
                    float mag = Mathf.Clamp01((Mathf.Abs(off) - DeadZoneDp) / (MaxDp - DeadZoneDp));
                    Steer = Mathf.Sign(off) * mag;
                }
                else if (id == gasFinger) gasSeen = true;
                else
                {
                    var s = t.startScreenPosition;
                    if (blocked.Contains(new Vector2(s.x, Screen.height - s.y))) continue;
                    if (s.x < Screen.width / 2f && steerFinger < 0) { steerFinger = id; origin = s; steerSeen = true; }
                    else if (s.x >= Screen.width / 2f && gasFinger < 0) { gasFinger = id; gasSeen = true; }
                }
            }
            if (!steerSeen) steerFinger = -1;
            if (!gasSeen) gasFinger = -1;
            Gas = gasSeen;

            // Keyboard for the editor: A/D or arrows steer fully, W or Space accelerates.
            var k = Keyboard.current;
            if (k == null) return;
            float keySteer = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
            if (keySteer != 0) Steer = keySteer;
            Gas |= k.wKey.isPressed || k.spaceKey.isPressed;
        }
    }
}
