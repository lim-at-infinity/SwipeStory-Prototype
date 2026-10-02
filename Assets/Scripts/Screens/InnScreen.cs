using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Inn (ScreenId.Recruit), reached from Town by day or night. Shows undo tokens, and Recruit opens the
// card swipe screen with the day's recruit cards (one draw per night). Roster is a plain NavButton and
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
        DayCycle dayCycle = DayCycle.Instance;
        if (!dayCycle.IsNight || dayCycle.RecruitedTonight)
        {
            return;
        }

        dayCycle.MarkRecruited();
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
        DayCycle dayCycle = DayCycle.Instance;
        _recruitButton.interactable = dayCycle.IsNight && !dayCycle.RecruitedTonight;
        _recruitLabel.text = !dayCycle.IsNight ? "Opens at night" : dayCycle.RecruitedTonight ? "Recruited tonight" : "Recruit";

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
