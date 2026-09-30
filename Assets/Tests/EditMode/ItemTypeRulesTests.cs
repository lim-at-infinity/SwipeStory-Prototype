using System;
using NUnit.Framework;

public class ItemTypeRulesTests
{
    [TestCase(ItemType.Hat, EquipSlot.Hat)]
    [TestCase(ItemType.Sword, EquipSlot.Weapon)]
    [TestCase(ItemType.Dagger, EquipSlot.Weapon)]
    [TestCase(ItemType.Staff, EquipSlot.Weapon)]
    [TestCase(ItemType.Cross, EquipSlot.Weapon)]
    [TestCase(ItemType.Potion, EquipSlot.None)]
    [TestCase(ItemType.Gift, EquipSlot.None)]
    public void GetSlot_MapsTypeToSlot(ItemType type, EquipSlot expected)
    {
        Assert.AreEqual(expected, ItemTypeRules.GetSlot(type));
    }

    [TestCase(ItemType.Sword, HeroClass.Warrior, true)]
    [TestCase(ItemType.Sword, HeroClass.Mage, false)]
    [TestCase(ItemType.Dagger, HeroClass.Rogue, true)]
    [TestCase(ItemType.Dagger, HeroClass.Healer, false)]
    [TestCase(ItemType.Staff, HeroClass.Mage, true)]
    [TestCase(ItemType.Staff, HeroClass.Warrior, false)]
    [TestCase(ItemType.Cross, HeroClass.Healer, true)]
    [TestCase(ItemType.Cross, HeroClass.Rogue, false)]
    [TestCase(ItemType.Hat, HeroClass.Mage, true)]
    [TestCase(ItemType.Potion, HeroClass.Warrior, false)]
    [TestCase(ItemType.Gift, HeroClass.Healer, false)]
    public void CanEquip_FollowsClassLocks(ItemType type, HeroClass heroClass, bool expected)
    {
        Assert.AreEqual(expected, ItemTypeRules.CanEquip(type, heroClass));
    }

    [Test]
    public void EveryWeaponType_IsLockedToAClass()
    {
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            if (ItemTypeRules.GetSlot(type) == EquipSlot.Weapon)
            {
                Assert.IsTrue(ItemTypeRules.TryGetRequiredClass(type, out _), type.ToString());
            }
        }
    }
}
