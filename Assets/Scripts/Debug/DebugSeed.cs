using System;
using System.Collections.Generic;
using UnityEngine;

// Editor-only test data, added on Play after GameManager.NewGame (-200) and before ScreenManager shows Town (-100).
// Skip-to-screen toggle comes after screen-manager is merged (ScreenId isn't on Core-Data yet)
[DefaultExecutionOrder(-150)]
public class DebugSeed : MonoBehaviour
{
    [Serializable]
    private struct SeedHero
    {
        public HeroClass Class;
        public int Affinity;

        public SeedHero(HeroClass heroClass, int affinity)
        {
            Class = heroClass;
            Affinity = affinity;
        }
    }

    [SerializeField] private bool _seedOnPlay = true;

    [Tooltip("Default: one hero per relationship tier. The Rogue (65) can be asked out")]
    [SerializeField] private SeedHero[] _heroes =
    {
        new SeedHero(HeroClass.Warrior, 0),
        new SeedHero(HeroClass.Mage, 30),
        new SeedHero(HeroClass.Rogue, 65),
        new SeedHero(HeroClass.Healer, 90)
    };

    [Tooltip("Drag in item assets, e.g. one of each ItemType")]
    [SerializeField] private List<ItemData> _items = new List<ItemData>();

#if UNITY_EDITOR
    private void Start()
    {
        if (!_seedOnPlay)
        {
            return;
        }

        BalanceConfig config = GameManager.Instance.Config;
        System.Random rng = new System.Random();

        foreach (SeedHero seed in _heroes)
        {
            HeroData hero = HeroFactory.Create(config, rng, "Test " + seed.Class, seed.Class, 1);

            if (!RosterManager.Instance.TryAddHero(hero))
            {
                Debug.LogWarning("[DebugSeed] Roster full, skipped " + hero.Name + ".");
                continue;
            }

            // Affinity with the player hero; starts at 0, so adding sets it
            RelationshipSystem.Instance.AddAffinity(hero, seed.Affinity);
        }

        foreach (ItemData item in _items)
        {
            Inventory.Instance.Add(item);
        }
    }
#endif
}
