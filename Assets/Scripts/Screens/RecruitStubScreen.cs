using TMPro;
using UnityEngine;
using UnityEngine.UI;

// STUB for the night Recruit screen for testing.
public class RecruitStubScreen : ScreenBase
{
    [SerializeField] private int _cardsPerNight = 5;
    [SerializeField] private TMP_Text _cardLabel;
    [SerializeField] private Button _passButton;
    [SerializeField] private Button _recruitButton;

    private int _cardIndex;

    private void Awake()
    {
        _passButton.onClick.AddListener(NextCard);
        _recruitButton.onClick.AddListener(NextCard);
    }

    private void OnEnable()
    {
        _cardIndex = 0;
        Refresh();
    }

    private void NextCard()
    {
        _cardIndex++;

        if (_cardIndex >= _cardsPerNight)
        {
            DayCycle.Instance.StartNextDay();
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        _cardLabel.text = "Hero card " + (_cardIndex + 1) + " / " + _cardsPerNight;
    }
}