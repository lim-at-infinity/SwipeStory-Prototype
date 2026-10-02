using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Read-only view of one hero. Doesn't know who opened it: Roster shows the selected hero,
// Recruit shows the card being inspected (not in the roster yet, so affinity reads 0)
public class HeroDetailPanel : MonoBehaviour
{
    [SerializeField] private HeroPortrait _portrait;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _classLevelText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _statsText;
    [SerializeField] private TMP_Text _equipmentText;
    [SerializeField] private TMP_Text _traitsText;
    [SerializeField] private TMP_Text _affinityText;

    public HeroData Hero { get; private set; }

    // Only listens while visible. OnHeroUpdated also covers affinity: RelationshipSystem notifies both heroes after a change
    private void OnEnable()
    {
        RosterManager.Instance.OnHeroUpdated += HandleHeroUpdated;
    }

    private void OnDisable()
    {
        if (RosterManager.Instance != null)
        {
            RosterManager.Instance.OnHeroUpdated -= HandleHeroUpdated;
        }
    }

    public void Show(HeroData hero)
    {
        Hero = hero;
        gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        Hero = null;
        gameObject.SetActive(false);
    }

    private void HandleHeroUpdated(HeroData hero)
    {
        if (hero == Hero)
        {
            Refresh();
        }
    }

    // Base stats only: equipment and tier bonuses are added by battle at fight time
    private void Refresh()
    {
        if (Hero == null)
        {
            return;
        }

        _portrait.Show(Hero);
        _nameText.text = Hero.Name;
        _classLevelText.text = "Lv " + Hero.Level + " " + Hero.Class;
        _hpText.text = "HP " + Hero.CurrentHp + " / " + Hero.MaxHp;
        _statsText.text = "ATK " + Hero.Attack + "   DEF " + Hero.Defense + "   SPD " + Hero.Speed;
        _equipmentText.text = "Weapon: " + ItemName(Hero.Weapon) + "\nHat: " + ItemName(Hero.Hat);
        _traitsText.text = "Traits: " + TraitNames();

        // No relationship with yourself
        _affinityText.gameObject.SetActive(!Hero.IsPlayer);
        if (!Hero.IsPlayer)
        {
            RelationshipSystem relationships = RelationshipSystem.Instance;
            _affinityText.text = "Affinity " + relationships.GetAffinity(Hero) + " / " + BalanceConfig.MaxAffinity
                + " (" + relationships.GetTier(Hero) + ")";

            RelationshipStatus status = relationships.GetStatus(Hero);
            if (status != RelationshipStatus.None)
            {
                _affinityText.text += ", " + status;
            }
        }
    }

    private static string ItemName(ItemData item)
    {
        return item != null ? item.DisplayName : "None";
    }

    private string TraitNames()
    {
        if (Hero.Traits.Count == 0)
        {
            return "None";
        }

        List<string> names = new List<string>();
        foreach (TraitData trait in Hero.Traits)
        {
            names.Add(trait.DisplayName);
        }

        return string.Join(", ", names);
    }

    // Test helper until RosterScreen exists: right-click the component header in Play mode. Delete once Roster works
    [ContextMenu("Show Player Hero")]
    private void ShowPlayerHero()
    {
        Show(RosterManager.Instance.PlayerHero);
    }
}
