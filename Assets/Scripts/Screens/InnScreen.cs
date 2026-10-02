using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Inn: night hub that DayCycle.EndDay opens (ScreenId.Recruit). Shows undo tokens, and Recruit opens the
// card swipe screen with tonight's recruit cards (one draw per night). Roster is a plain NavButton and
// Sleep is a DayCycleButton (StartNextDay), so neither needs code here
public class InnScreen : ScreenBase
{
    [SerializeField] private TMP_Text _undoTokensText;
    [SerializeField] private Button _recruitButton;
    [SerializeField] private TMP_Text _recruitLabel;

    private int _drawnOnDay = -1;

    private bool DrawnTonight => _drawnOnDay == DayCycle.Instance.Day;

    private void Awake()
    {
        _recruitButton.onClick.AddListener(StartRecruiting);
    }

    private void OnEnable()
    {
        GameManager.Instance.OnUndoTokensChanged += HandleUndoTokensChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnUndoTokensChanged -= HandleUndoTokensChanged;
        }
    }

    private void StartRecruiting()
    {
        if (DrawnTonight)
        {
            return;
        }

        _drawnOnDay = DayCycle.Instance.Day;
        RecruitDeck deck = new RecruitDeck(GameManager.Instance.Config.RecruitCardsPerNight);
        CardSwipeScreen.Open(deck, () => ScreenManager.Instance.Show(ScreenId.Recruit));
    }

    private void HandleUndoTokensChanged(int count)
    {
        Refresh();
    }

    private void Refresh()
    {
        _undoTokensText.text = "Undo: " + GameManager.Instance.UndoTokens;
        _recruitButton.interactable = !DrawnTonight;
        _recruitLabel.text = DrawnTonight ? "Recruited tonight" : "Recruit";
    }
}
