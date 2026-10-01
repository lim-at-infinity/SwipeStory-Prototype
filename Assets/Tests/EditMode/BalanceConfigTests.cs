using System;
using NUnit.Framework;
using UnityEngine;

public class BalanceConfigTests
{
    private BalanceConfig _config;

    [SetUp]
    public void SetUp()
    {
        _config = ScriptableObject.CreateInstance<BalanceConfig>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(_config);
    }

    [TestCase(0, RelationshipTier.Stranger)]
    [TestCase(24, RelationshipTier.Stranger)]
    [TestCase(25, RelationshipTier.Friend)]
    [TestCase(49, RelationshipTier.Friend)]
    [TestCase(50, RelationshipTier.Close)]
    [TestCase(74, RelationshipTier.Close)]
    [TestCase(75, RelationshipTier.Devoted)]
    [TestCase(100, RelationshipTier.Devoted)]
    [TestCase(-10, RelationshipTier.Stranger)]
    [TestCase(150, RelationshipTier.Devoted)]
    public void GetTierForAffinity_ReturnsTierForBand(int affinity, RelationshipTier expected)
    {
        Assert.AreEqual(expected, _config.GetTierForAffinity(affinity));
    }

    [Test]
    public void GetXpToNextLevel_AtMaxLevel_IsIntMax()
    {
        Assert.AreEqual(int.MaxValue, _config.GetXpToNextLevel(_config.MaxLevel));
    }

    [Test]
    public void GetXpToNextLevel_BelowMaxLevel_IsPositive()
    {
        for (int level = 1; level < _config.MaxLevel; level++)
        {
            Assert.Greater(_config.GetXpToNextLevel(level), 0, "Level " + level);
        }
    }

    [TestCase(60, false)]
    [TestCase(61, true)]
    public void CanAskOut_NeedsAffinityAboveThreshold(int affinity, bool expected)
    {
        Assert.AreEqual(expected, _config.CanAskOut(affinity));
    }

    [TestCase(39, true)]
    [TestCase(40, false)]
    public void ShouldBreakUp_WhenAffinityBelowThreshold(int affinity, bool expected)
    {
        Assert.AreEqual(expected, _config.ShouldBreakUp(affinity));
    }

    [Test]
    public void GetClassStats_HasProfileForEveryClass()
    {
        foreach (HeroClass heroClass in Enum.GetValues(typeof(HeroClass)))
        {
            Assert.DoesNotThrow(() => _config.GetClassStats(heroClass), heroClass.ToString());
        }
    }
}
