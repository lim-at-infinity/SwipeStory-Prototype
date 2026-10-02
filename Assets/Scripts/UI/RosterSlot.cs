using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One tile in the roster grid. Reports clicks to whoever bound it; knows nothing about the screen or the detail panel
[RequireComponent(typeof(Button))]
public class RosterSlot : MonoBehaviour
{
    [SerializeField] private HeroPortrait _portrait;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _classLevelText;
    [Tooltip("Shown while this hero is open in the detail panel.")]
    [SerializeField] private GameObject _selectedHighlight;

    private Action<HeroData> _onClick;

    public HeroData Hero { get; private set; }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(HandleClick);
    }

    // onClick is called with this slot's hero, so one method on the screen can handle every slot
    public void Bind(HeroData hero, Action<HeroData> onClick)
    {
        Hero = hero;
        _onClick = onClick;
        Refresh();
    }

    public void Refresh()
    {
        _portrait.Show(Hero);
        _nameText.text = Hero.IsPlayer ? Hero.Name + " (You)" : Hero.Name;
        _classLevelText.text = "Lv " + Hero.Level + " " + Hero.Class;
    }

    public void SetSelected(bool selected)
    {
        if (_selectedHighlight != null)
        {
            _selectedHighlight.SetActive(selected);
        }
    }

    private void HandleClick()
    {
        _onClick?.Invoke(Hero);
    }
}
