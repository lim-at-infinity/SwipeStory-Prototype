using System;
using NUnit.Framework;
using UnityEngine;

public class HeroFactoryTests
{
    private BalanceConfig _config;
    private System.Random _rng;

    [SetUp]
    public void SetUp()
    {
        _config = ScriptableObject.CreateInstance<BalanceConfig>();
        _rng = new System.Random(42);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(_config);
    }

    [Test]
    public void Create_Level1_StatsWithinClassRanges()
    {
        foreach (HeroClass heroClass in Enum.GetValues(typeof(HeroClass)))
        {
            ClassStatProfile profile = _config.GetClassStats(heroClass);
            HeroData hero = HeroFactory.Create(_config, _rng, "Test", heroClass, 1);

            Assert.That(hero.MaxHp, Is.InRange(profile.MaxHp.Min, profile.MaxHp.Max), heroClass + " MaxHp");
            Assert.That(hero.Attack, Is.InRange(profile.Attack.Min, profile.Attack.Max), heroClass + " Attack");
            Assert.That(hero.Defense, Is.InRange(profile.Defense.Min, profile.Defense.Max), heroClass + " Defense");
            Assert.That(hero.Speed, Is.InRange(profile.Speed.Min, profile.Speed.Max), heroClass + " Speed");
            Assert.AreEqual(hero.MaxHp, hero.CurrentHp, heroClass + " starts at full HP");
        }
    }

    [Test]
    public void Create_HigherLevel_AddsGrowthPerLevel()
    {
        ClassStatProfile profile = _config.GetClassStats(HeroClass.Warrior);
        HeroData hero = HeroFactory.Create(_config, _rng, "Test", HeroClass.Warrior, 5);
        int gained = 4;

        Assert.AreEqual(5, hero.Level);
        Assert.That(hero.Attack, Is.InRange(profile.Attack.Min + profile.AttackPerLevel * gained, profile.Attack.Max + profile.AttackPerLevel * gained));
        Assert.That(hero.MaxHp, Is.InRange(profile.MaxHp.Min + profile.MaxHpPerLevel * gained, profile.MaxHp.Max + profile.MaxHpPerLevel * gained));
    }

    [Test]
    public void Create_LevelOutOfRange_IsClamped()
    {
        Assert.AreEqual(_config.MaxLevel, HeroFactory.Create(_config, _rng, "Test", HeroClass.Mage, 99).Level);
        Assert.AreEqual(1, HeroFactory.Create(_config, _rng, "Test", HeroClass.Mage, 0).Level);
    }

    [Test]
    public void Create_NewHero_StartsWithNoRelationship()
    {
        HeroData hero = HeroFactory.Create(_config, _rng, "Test", HeroClass.Healer, 1);

        Assert.AreEqual(0, hero.Affinity);
        Assert.AreEqual(RelationshipStatus.None, hero.Status);
        Assert.IsNull(hero.Weapon);
        Assert.IsNull(hero.Hat);
        Assert.IsEmpty(hero.Traits);
    }

    [Test]
    public void Create_GivesEachHeroAUniqueId()
    {
        HeroData a = HeroFactory.Create(_config, _rng, "A", HeroClass.Rogue, 1);
        HeroData b = HeroFactory.Create(_config, _rng, "B", HeroClass.Rogue, 1);

        Assert.IsFalse(string.IsNullOrEmpty(a.Id));
        Assert.AreNotEqual(a.Id, b.Id);
    }

    [Test]
    public void Create_Player_SetsIsPlayer()
    {
        Assert.IsTrue(HeroFactory.Create(_config, _rng, "Me", HeroClass.Warrior, 1, true).IsPlayer);
        Assert.IsFalse(HeroFactory.Create(_config, _rng, "Npc", HeroClass.Warrior, 1).IsPlayer);
    }
}
