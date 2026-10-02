using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Lists the adventures from BalanceConfig in unlock order. Locked ones can't be picked.
// The party is picked automatically for now: the player hero plus the NPC heroes with the highest affinity.
public class AdventureSelectScreen : ScreenBase
{
    [SerializeField] private Button[] _adventureButtons;
    [SerializeField] private TMP_Text _partyText;

    private readonly List<HeroData> _party = new List<HeroData>();

    private void Awake()
    {
        for (int i = 0; i < _adventureButtons.Length; i++)
        {
            int index = i; // copy for the lambda
            _adventureButtons[i].onClick.AddListener(() => StartAdventure(index));
        }
    }

    private void OnEnable()
    {
        PickParty();
        RefreshButtons();
    }

    private void PickParty()
    {
        _party.Clear();
        _party.Add(RosterManager.Instance.PlayerHero);

        List<HeroData> others = new List<HeroData>();
        foreach (HeroData hero in RosterManager.Instance.Heroes)
        {
            if (!hero.IsPlayer)
            {
                others.Add(hero);
            }
        }

        // Highest affinity first
        RelationshipSystem relationships = RelationshipSystem.Instance;
        others.Sort((a, b) => relationships.GetAffinity(b).CompareTo(relationships.GetAffinity(a)));

        int slots = GameManager.Instance.Config.PartySize - 1;
        for (int i = 0; i < others.Count && i < slots; i++)
        {
            _party.Add(others[i]);
        }

        List<string> names = new List<string>();
        foreach (HeroData hero in _party)
        {
            names.Add(hero.Name);
        }

        _partyText.text = "Party: " + string.Join(", ", names);
    }

    private void RefreshButtons()
    {
        IReadOnlyList<AdventureData> adventures = GameManager.Instance.Config.Adventures;

        for (int i = 0; i < _adventureButtons.Length; i++)
        {
            bool exists = i < adventures.Count;
            _adventureButtons[i].gameObject.SetActive(exists);
            if (!exists)
            {
                continue;
            }

            AdventureData adventure = adventures[i];
            bool unlocked = GameManager.Instance.IsUnlocked(adventure);

            string label = adventure.DisplayName;
            if (GameManager.Instance.IsCleared(adventure))
            {
                label += " (Cleared)";
            }
            else if (!unlocked)
            {
                label += " (Locked)";
            }

            _adventureButtons[i].interactable = unlocked;
            _adventureButtons[i].GetComponentInChildren<TMP_Text>().text = label;
        }
    }

    private void StartAdventure(int index)
    {
        AdventureData adventure = GameManager.Instance.Config.Adventures[index];
        ScreenManager.Instance.Adventure.Begin(adventure, _party);
        Debug.Log("[AdventureSelect] " + adventure.DisplayName + ", " + _partyText.text);
        ScreenManager.Instance.Show(ScreenId.Battle);
    }
}