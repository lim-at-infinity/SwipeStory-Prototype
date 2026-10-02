using System.Collections.Generic;
using UnityEngine;

// Adventurer Guild: a grid of every living hero, and the selected hero's details beside it.
// Inspection lives here as a panel instead of its own screen. Rebuilds from RosterManager events
public class RosterScreen : ScreenBase
{
    [SerializeField] private Transform _grid;
    [SerializeField] private RosterSlot _slotPrefab;
    [SerializeField] private HeroDetailPanel _detailPanel;

    private readonly List<RosterSlot> _slots = new List<RosterSlot>();
    private HeroData _selected;

    private void OnEnable()
    {
        RosterManager.Instance.OnRosterChanged += Rebuild;
        RosterManager.Instance.OnHeroUpdated += HandleHeroUpdated;
        Rebuild();
    }

    private void OnDisable()
    {
        if (RosterManager.Instance != null)
        {
            RosterManager.Instance.OnRosterChanged -= Rebuild;
            RosterManager.Instance.OnHeroUpdated -= HandleHeroUpdated;
        }
    }

    // Throws away every slot and makes fresh ones. 
    private void Rebuild()
    {
        foreach (RosterSlot slot in _slots)
        {
            Destroy(slot.gameObject);
        }

        _slots.Clear();

        foreach (HeroData hero in RosterManager.Instance.Heroes)
        {
            RosterSlot slot = Instantiate(_slotPrefab, _grid);
            slot.Bind(hero, Select);
            _slots.Add(slot);
        }

        // Keep the selection if that hero is still here (they may have fallen or been dismissed); otherwise show the player hero
        if (_selected == null || RosterManager.Instance.GetById(_selected.Id) == null)
        {
            _selected = RosterManager.Instance.PlayerHero;
        }

        Select(_selected);
    }

    private void Select(HeroData hero)
    {
        _selected = hero;

        foreach (RosterSlot slot in _slots)
        {
            slot.SetSelected(slot.Hero == hero);
        }

        if (hero == null)
        {
            _detailPanel.Hide();
        }
        else
        {
            _detailPanel.Show(hero);
        }
    }

    // The detail panel refreshes itself, only the matching tile needs updating here
    private void HandleHeroUpdated(HeroData hero)
    {
        foreach (RosterSlot slot in _slots)
        {
            if (slot.Hero == hero)
            {
                slot.Refresh();
            }
        }
    }
}
