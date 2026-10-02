using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Inn (ScreenId.Recruit), reached from Town by day or night. Shows undo tokens, and Recruit opens the
// card swipe screen with the day's recruit cards (one draw per day). Roster is a plain NavButton and
// Sleep is a DayCycleButton (StartNextDay), shown only at night
public class InnScreen : ScreenBase
{
    [SerializeField] private TMP_Text _undoTokensText;
    [SerializeField] private Button _recruitButton;
    [SerializeField] private TMP_Text _recruitLabel;
    [Tooltip("Hidden during the day: sleeping only works at night (DayCycle.StartNextDay)")]
    [SerializeField] private GameObject _sleepButton;
    [Tooltip("The header's Back button. Goes to Town")]
    [SerializeField] private Button _backButton;

    private int _drawnOnDay = -1;

    private bool DrawnToday => _drawnOnDay == DayCycle.Instance.Day;

    private void Awake()
    {
        _recruitButton.onClick.AddListener(StartRecruiting);

        // Showing the Inn clears the Back history, so Back has nowhere to return to: send it to Town directly
        if (_backButton != null)
        {
            _backButton.onClick.AddListener(() => ScreenManager.Instance.Show(ScreenId.Town));
        }
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
        if (DrawnToday)
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
        _undoTokensText.text = "Undo's Left: " + GameManager.Instance.UndoTokens;
        bool canRecruit = DayCycle.Instance.IsNight && !DrawnToday;
        _recruitButton.interactable = canRecruit;
        _recruitLabel.text = !DayCycle.Instance.IsNight ? "Opens at night" : DrawnToday ? "Recruited tonight" : "Recruit";
        
        if (_sleepButton != null)
        {
            _sleepButton.SetActive(DayCycle.Instance.IsNight);
        }

        if (_backButton != null)
        {
            _backButton.gameObject.SetActive(true);
        }
    }
}
