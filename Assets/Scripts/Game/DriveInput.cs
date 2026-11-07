using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Geisterfahrer.Game
{
    // One finger as the input code needs it (so the logic below can be tested without a touch screen).
    public struct TouchInfo
    {
        public int Id;
        public bool Began, Ended;
        public Vector2 Start, Position;
    }

    // Left half: steering with a floating origin. Right half: accelerator. Both at once (GDD 2.2).
    public class DriveInput
    {
        const float DeadZoneDp = 10, MaxDp = 75;
        int steerFinger = -1, gasFinger = -1;
        Vector2 origin;
        readonly HashSet<int> ignored = new HashSet<int>();
        readonly List<TouchInfo> frame = new List<TouchInfo>();

        public float Steer;
        public bool Gas;
        public float SteerX; // current steering finger x (screen px)
        public Vector2? SteerOrigin => steerFinger >= 0 ? origin : (Vector2?)null;

        public DriveInput() => EnhancedTouchSupport.Enable();

        // Fingers already down (the tap on Play, Retry or Resume) never steer or accelerate; they count again after lifting.
        public void IgnoreCurrent()
        {
            steerFinger = gasFinger = -1;
            foreach (var t in frame) ignored.Add(t.Id);
        }

        // blocked: true for screen points (bottom-left origin) that belong to the UI.
        public void Update(Func<Vector2, bool> blocked)
        {
            frame.Clear();
            foreach (var t in Touch.activeTouches)
                frame.Add(new TouchInfo
                {
                    Id = t.finger.index,
                    Began = t.phase == UnityEngine.InputSystem.TouchPhase.Began,
                    Ended = t.phase == UnityEngine.InputSystem.TouchPhase.Ended || t.phase == UnityEngine.InputSystem.TouchPhase.Canceled,
                    Start = t.startScreenPosition,
                    Position = t.screenPosition,
                });
            Apply(frame, blocked, Screen.width, (Screen.dpi > 0 ? Screen.dpi : 160) / 160);

            // Keyboard for the editor: A/D or arrows steer fully, W or Space accelerates.
            var k = Keyboard.current;
            if (k == null) return;
            float keySteer = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
            if (keySteer != 0) Steer = keySteer;
            Gas |= k.wKey.isPressed || k.spaceKey.isPressed;
        }

        // A finger gets its role from where it started: left half steers, right half is the accelerator.
        // It keeps the role until lifted, even across the middle. One finger per role; both work at once.
        public void Apply(IList<TouchInfo> touches, Func<Vector2, bool> blocked, float screenWidth, float dp)
        {
            bool steerSeen = false, gasSeen = false;
            Steer = 0;
            if (!ReferenceEquals(touches, frame)) { frame.Clear(); foreach (var t in touches) frame.Add(t); } // IgnoreCurrent reads the last frame
            foreach (var t in touches)
            {
                if (t.Ended) { ignored.Remove(t.Id); continue; }
                if (t.Began) ignored.Remove(t.Id); // a new touch on a reused finger slot
                if (ignored.Contains(t.Id)) continue;
                if (t.Id == steerFinger)
                {
                    steerSeen = true;
                    SteerX = t.Position.x;
                    float off = (t.Position.x - origin.x) / dp;
                    float mag = Mathf.Clamp01((Mathf.Abs(off) - DeadZoneDp) / (MaxDp - DeadZoneDp));
                    Steer = Mathf.Sign(off) * mag;
                }
                else if (t.Id == gasFinger) gasSeen = true;
                else
                {
                    if (blocked(t.Start)) continue;
                    if (t.Start.x < screenWidth / 2f && steerFinger < 0) { steerFinger = t.Id; origin = t.Start; SteerX = t.Start.x; steerSeen = true; }
                    else if (t.Start.x >= screenWidth / 2f && gasFinger < 0) { gasFinger = t.Id; gasSeen = true; }
                }
            }
            if (!steerSeen) steerFinger = -1;
            if (!gasSeen) gasFinger = -1;
            Gas = gasSeen;
        }
    }
}
