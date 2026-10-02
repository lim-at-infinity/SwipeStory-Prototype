using TMPro;
using UnityEngine;

// One line in the Battle screen's hero or enemy list: portrait (heroes only) and "Name  hp/max"
public class BattleUnitRow : MonoBehaviour
{
    [SerializeField] private HeroPortrait _portrait;
    [SerializeField] private TMP_Text _label;

    private string _name;
    private int _maxHp;

    public void Bind(BattleUnit unit)
    {
        _name = unit.Name;
        _maxHp = unit.MaxHp;
        _portrait.Show(unit.Hero);   // enemies have no HeroData, so the portrait hides
        SetHp(unit.CurrentHp);
    }

    public void SetHp(int hp)
    {
        _label.text = _name + "  " + hp + "/" + _maxHp;
        _label.alpha = hp > 0 ? 1f : 0.35f;   // fallen units fade out
    }
}