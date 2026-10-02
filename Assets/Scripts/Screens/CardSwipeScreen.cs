using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Runs one SwipeDeck on a draggable card. Right (or the accept button) accepts, left passes, up inspects.
// Doesn't know whether it's recruiting heroes or handing out rewards: the deck decides that.
// Open it with CardSwipeScreen.Open(deck, onFinished); onFinished runs when the player presses Continue
public class CardSwipeScreen : ScreenBase
{
    // Screens are enabled by ScreenManager, not constructed, so Open leaves the deck here for OnEnable to pick up
    private static SwipeDeck _pendingDeck;
    private static Action _pendingOnFinished;

    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _counterText;
    [SerializeField] private TMP_Text _undoTokensText;
    [SerializeField] private TMP_Text _messageText;

    [SerializeField] private SwipeCard _card;
    [SerializeField] private SwipeCardView _cardView;

    [SerializeField] private Button _passButton;
    [Tooltip("Optional. Set to the deck's pass word (Reject, Leave)")]
    [SerializeField] private TMP_Text _passLabel;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private TMP_Text _acceptLabel;
    [SerializeField] private Button _inspectButton;
    [SerializeField] private Button _undoButton;
    [SerializeField] private Button _continueButton;

    [Tooltip("Covers the screen while inspecting; holds the detail panel and a close button")]
    [SerializeField] private GameObject _inspectOverlay;
    [SerializeField] private HeroDetailPanel _detailPanel;
    [SerializeField] private Button _closeInspectButton;

    private SwipeDeck _deck;
    private Action _onFinished;
    private bool _busy;

    public static void Open(SwipeDeck deck, Action onFinished)
    {
        _pendingDeck = deck;
        _pendingOnFinished = onFinished;
        ScreenManager.Instance.Show(ScreenId.CardSwipe);
    }

    private void Awake()
    {
        _passButton.onClick.AddListener(() => Resolve(SwipeDirection.Left));
        _acceptButton.onClick.AddListener(() => Resolve(SwipeDirection.Right));
        _inspectButton.onClick.AddListener(() => Resolve(SwipeDirection.Up));
        _undoButton.onClick.AddListener(Undo);
        _continueButton.onClick.AddListener(Finish);
        _closeInspectButton.onClick.AddListener(CloseInspect);
    }

    private void OnEnable()
    {
        _deck = _pendingDeck;
        _onFinished = _pendingOnFinished;
        _pendingDeck = null;
        _pendingOnFinished = null;
        _busy = false;

        _card.OnSwiped += Resolve;
        GameManager.Instance.OnUndoTokensChanged += HandleUndoTokensChanged;

        CloseInspect();
        _card.ResetCard();
        Refresh();
    }

    private void OnDisable()
    {
        _card.OnSwiped -= Resolve;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnUndoTokensChanged -= HandleUndoTokensChanged;
        }
    }

    // Drags and buttons both end up here
    private void Resolve(SwipeDirection direction)
    {
        if (_busy || _deck == null || _deck.IsFinished)
        {
            _card.SnapBack();
            return;
        }

        switch (direction)
        {
            case SwipeDirection.Right:
                if (_deck.TryAccept())
                {
                    FlyOff(direction);
                }
                else
                {
                    _card.SnapBack();
                    _messageText.text = _deck.BlockedReason;
                }
                break;

            case SwipeDirection.Left:
                _deck.Pass();
                FlyOff(direction);
                break;

            case SwipeDirection.Up:
                _card.SnapBack();
                if (_deck.CanInspect)
                {
                    OpenInspect();
                }
                break;

            default:
                _card.SnapBack();
                break;
        }
    }

    private void FlyOff(SwipeDirection direction)
    {
        _busy = true;
        _card.FlyOff(direction, () =>
        {
            _busy = false;
            _card.ResetCard();
            Refresh();
        });
    }

    private void Undo()
    {
        if (_busy || _deck == null || !_deck.TryUndo())
        {
            return;
        }

        _card.ResetCard();
        Refresh();
    }

    private void OpenInspect()
    {
        _inspectOverlay.SetActive(true);
        if (!_deck.TryInspect(_detailPanel))
        {
            _inspectOverlay.SetActive(false);
        }
    }

    private void CloseInspect()
    {
        _inspectOverlay.SetActive(false);
    }

    private void Finish()
    {
        Action onFinished = _onFinished;
        _deck = null;
        _onFinished = null;

        if (onFinished != null)
        {
            onFinished();
        }
        else
        {
            ScreenManager.Instance.Show(ScreenId.Town);
        }
    }

    private void HandleUndoTokensChanged(int count)
    {
        Refresh();
    }

    private void Refresh()
    {
        bool hasDeck = _deck != null;
        bool finished = !hasDeck || _deck.IsFinished;

        _titleText.text = hasDeck ? _deck.Title : "";
        _acceptLabel.text = hasDeck ? _deck.AcceptLabel : "";
        if (_passLabel != null)
        {
            _passLabel.text = hasDeck ? _deck.PassLabel : "";
        }

        _counterText.text = finished ? "" : "Card " + (_deck.Index + 1) + " / " + _deck.Count;
        _undoTokensText.text = "Undo: " + GameManager.Instance.UndoTokens;

        _card.gameObject.SetActive(!finished);
        if (!finished)
        {
            _deck.ShowCurrent(_cardView);
            _card.SetHints(_deck.PassLabel, _deck.AcceptLabel, _deck.CanInspect ? "Inspect" : null);
        }

        _passButton.interactable = !finished;
        _acceptButton.interactable = !finished && _deck.CanAccept;
        _inspectButton.interactable = !finished && _deck.CanInspect;
        _undoButton.interactable = hasDeck && _deck.CanUndo && GameManager.Instance.UndoTokens > 0;
        _continueButton.gameObject.SetActive(finished);

        if (finished)
        {
            _messageText.text = hasDeck ? _deck.FinishedMessage : "Nothing to swipe.";
        }
        else
        {
            _messageText.text = _deck.BlockedReason ?? "";
        }
    }
}
