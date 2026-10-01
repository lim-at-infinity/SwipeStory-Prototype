using NUnit.Framework;

public class IntRangeTests
{
    [Test]
    public void Roll_StaysWithinBoundsAndReachesBothEnds()
    {
        IntRange range = new IntRange(3, 7);
        System.Random rng = new System.Random(1234);
        bool sawMin = false;
        bool sawMax = false;

        for (int i = 0; i < 1000; i++)
        {
            int value = range.Roll(rng);
            Assert.That(value, Is.InRange(3, 7));
            sawMin |= value == 3;
            sawMax |= value == 7;
        }

        Assert.IsTrue(sawMin, "Min was never rolled");
        Assert.IsTrue(sawMax, "Max was never rolled");
    }

    [Test]
    public void Roll_MinEqualsMax_ReturnsThatValue()
    {
        IntRange range = new IntRange(5, 5);
        Assert.AreEqual(5, range.Roll(new System.Random(1)));
    }

    [Test]
    public void Roll_SameSeed_GivesSameSequence()
    {
        IntRange range = new IntRange(0, 100);
        System.Random a = new System.Random(42);
        System.Random b = new System.Random(42);

        for (int i = 0; i < 20; i++)
        {
            Assert.AreEqual(range.Roll(a), range.Roll(b));
        }
    }

    [TestCase(1, 3)]
    [TestCase(5, 5)]
    [TestCase(9, 7)]
    public void Clamp_PullsValueIntoRange(int value, int expected)
    {
        Assert.AreEqual(expected, new IntRange(3, 7).Clamp(value));
    }
}
