using System.Collections.Generic;
using Geisterfahrer.Game;
using NUnit.Framework;
using UnityEngine;

namespace Geisterfahrer.Tests
{
    // Two-thumb control (GDD 2.2): left half steers from where the finger landed, right half is the accelerator, both at once.
    public class InputTests
    {
        const float W = 2560, H = 1600, Dp = 2.125f; // Galaxy Tab S7, 340 dpi
        static readonly System.Func<Vector2, bool> Free = _ => false;

        static TouchInfo Finger(int id, Vector2 start, Vector2 pos, bool began = false, bool ended = false)
            => new TouchInfo { Id = id, Start = start, Position = pos, Began = began, Ended = ended };

        [Test]
        public void SteerAndGasWorkAtTheSameTime()
        {
            var input = new DriveInput();
            var gasAt = new Vector2(2000, 800);
            var steerAt = new Vector2(600, 800);
            var list = new List<TouchInfo> { Finger(0, gasAt, gasAt, true), Finger(1, steerAt, steerAt, true) };
            input.Apply(list, Free, W, Dp);
            Assert.IsTrue(input.Gas);
            Assert.AreEqual(0, input.Steer, 1e-6); // resting thumb: inside the dead zone

            list = new List<TouchInfo> { Finger(0, gasAt, gasAt), Finger(1, steerAt, steerAt + new Vector2(60 * Dp, 0)) };
            input.Apply(list, Free, W, Dp);
            Assert.IsTrue(input.Gas);
            Assert.AreEqual((60 - 10) / 65f, input.Steer, 1e-4); // between the 10 dp dead zone and 75 dp full steering

            list = new List<TouchInfo> { Finger(0, gasAt, gasAt), Finger(1, steerAt, steerAt + new Vector2(-100 * Dp, 0)) };
            input.Apply(list, Free, W, Dp);
            Assert.AreEqual(-1, input.Steer, 1e-6);
            Assert.AreEqual(steerAt.x - 100 * Dp, input.SteerX, 1e-3);
        }

        [Test]
        public void LiftingEitherFingerKeepsTheOther()
        {
            var input = new DriveInput();
            var gasAt = new Vector2(2000, 800); var steerAt = new Vector2(600, 800);
            var moved = steerAt + new Vector2(80 * Dp, 0);
            input.Apply(new List<TouchInfo> { Finger(0, gasAt, gasAt, true), Finger(1, steerAt, steerAt, true) }, Free, W, Dp);
            input.Apply(new List<TouchInfo> { Finger(0, gasAt, gasAt, ended: true), Finger(1, steerAt, moved) }, Free, W, Dp);
            Assert.IsFalse(input.Gas);
            Assert.AreEqual(1, input.Steer, 1e-6);
            input.Apply(new List<TouchInfo> { Finger(1, steerAt, moved, ended: true) }, Free, W, Dp);
            Assert.AreEqual(0, input.Steer); // finger up = straight
            Assert.IsNull(input.SteerOrigin);
        }

        [Test]
        public void AFingerKeepsItsRoleWhenItCrossesTheMiddle()
        {
            var input = new DriveInput();
            var start = new Vector2(1200, 800); // left half
            input.Apply(new List<TouchInfo> { Finger(0, start, start, true) }, Free, W, Dp);
            input.Apply(new List<TouchInfo> { Finger(0, start, new Vector2(1900, 800)) }, Free, W, Dp);
            Assert.IsFalse(input.Gas);
            Assert.AreEqual(1, input.Steer, 1e-6);
        }

        [Test]
        public void ANewSteeringTouchSetsANewOrigin()
        {
            var input = new DriveInput();
            var a = new Vector2(400, 500);
            input.Apply(new List<TouchInfo> { Finger(0, a, a + new Vector2(80 * Dp, 0), true) }, Free, W, Dp);
            input.Apply(new List<TouchInfo> { Finger(0, a, a, ended: true) }, Free, W, Dp);
            var b = new Vector2(900, 1100);
            input.Apply(new List<TouchInfo> { Finger(0, b, b, true) }, Free, W, Dp);
            Assert.AreEqual(b, input.SteerOrigin.Value);
            Assert.AreEqual(0, input.Steer, 1e-6);
        }

        [Test]
        public void TheTapThatPressedPlayDoesNotAccelerate()
        {
            var input = new DriveInput();
            var play = new Vector2(1280, 800);
            // The thumb is down on the start screen (everything blocked), then the screen switches to the run while it is still down.
            input.Apply(new List<TouchInfo> { Finger(0, play, play, true) }, _ => true, W, Dp);
            input.IgnoreCurrent();
            input.Apply(new List<TouchInfo> { Finger(0, play, play) }, Free, W, Dp);
            Assert.IsFalse(input.Gas);
            input.Apply(new List<TouchInfo> { Finger(0, play, play, ended: true) }, Free, W, Dp);
            input.Apply(new List<TouchInfo> { Finger(0, play, play, true) }, Free, W, Dp); // pressing again counts
            Assert.IsTrue(input.Gas);
        }

        [Test]
        public void TouchesOnTheUiAreIgnored()
        {
            var input = new DriveInput();
            var pause = new Vector2(100, 1500);
            input.Apply(new List<TouchInfo> { Finger(0, pause, pause, true) }, p => p.y > 1400, W, Dp);
            Assert.IsNull(input.SteerOrigin);
            Assert.IsFalse(input.Gas);
        }
    }
}
