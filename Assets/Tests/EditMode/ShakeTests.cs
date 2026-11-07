using Geisterfahrer.Core;
using NUnit.Framework;

namespace Geisterfahrer.Tests
{
    public class ShakeTests
    {
        [Test] public void ZeroBelow200() => Assert.AreEqual(0, Shake.Amount(199, true, false));
        [Test] public void ZeroWhenCoasting() => Assert.AreEqual(0, Shake.Amount(300, false, false));
        [Test] public void ZeroInGameOver() => Assert.AreEqual(0, Shake.Amount(300, true, true));
        [Test] public void GrowsWithSpeed() => Assert.Less(Shake.Amount(250, true, false), Shake.Amount(350, true, false));
        [Test] public void CappedAtOne() => Assert.AreEqual(1, Shake.Amount(500, true, false));
    }
}
