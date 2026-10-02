using TMPro;
using UnityEngine;
using UnityEngine.UI;

// What's drawn on a swipe card. One card prefab holds a hero layout and an item layout;
// the deck calls ShowHero or ShowItem, which turns on the matching layout and fills it in
public class SwipeCardView : MonoBehaviour
{
    [Header("Hero")]
    [SerializeField] private GameObject _heroGroup;
    [SerializeField] private HeroPortrait _heroPortrait;
    [SerializeField] private TMP_Text _heroNameText;
    [SerializeField] private TMP_Text _heroClassLevelText;
    [SerializeField] private TMP_Text _heroStatsText;

    [Header("Item")]
    [SerializeField] private GameObject _itemGroup;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemTypeText;
    [SerializeField] private TMP_Text _itemDescriptionText;

    public void ShowHero(HeroData hero)
    {
        _heroGroup.SetActive(true);
        _itemGroup.SetActive(false);

        _heroPortrait.Show(hero);
        _heroNameText.text = hero.Name;
        _heroClassLevelText.text = "Lv " + hero.Level + " " + hero.Class;
        _heroStatsText.text = "HP " + hero.MaxHp + "\nATK " + hero.Attack + "   DEF " + hero.Defense + "   SPD " + hero.Speed;
    }

    public void ShowItem(ItemData item)
    {
        _heroGroup.SetActive(false);
        _itemGroup.SetActive(true);

        _itemIcon.sprite = item.Icon;
        _itemIcon.enabled = item.Icon != null;
        _itemIcon.preserveAspect = true;

        _itemNameText.text = item.DisplayName;
        _itemTypeText.text = item.Type + ", " + item.Stars + (item.Stars == 1 ? " star" : " stars");
        _itemDescriptionText.text = item.Description;
    }
}
