using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Not saved anywhere, but explicit values like every other enum
public enum SwipeDirection
{
    None = 0,
    Left = 1,
    Right = 2,
    Up = 3
}

// Drag-to-swipe behavior for one card. Knows nothing about what's on the card: it reports which way it was
// swiped, and the screen decides what happens (fly off, or snap back if the swipe isn't allowed).
// The card needs a raycast-target Image on this object to receive drags.
// Feel values (distance, tilt, timing) are UI tuning, so they live here rather than in BalanceConfig
public class SwipeCard : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Tooltip("How far the card must be dragged, in canvas units, to count as a swipe")]
    [SerializeField, Min(1f)] private float _swipeDistance = 200f;
    [Tooltip("Tilt in degrees once the card is dragged the full swipe distance sideways")]
    [SerializeField] private float _maxTilt = 15f;
    [Tooltip("How far the card travels when it flies off")]
    [SerializeField] private float _flyOffDistance = 1500f;
    [SerializeField, Min(0.01f)] private float _animationSeconds = 0.2f;

    [Header("Drag hints (optional)")]
    [Tooltip("Fades in as the card is dragged left, e.g. Reject")]
    [SerializeField] private TMP_Text _leftHint;
    [Tooltip("Fades in as the card is dragged right, e.g. Pick")]
    [SerializeField] private TMP_Text _rightHint;
    [Tooltip("Fades in as the card is dragged up, e.g. Inspect")]
    [SerializeField] private TMP_Text _upHint;

    public event Action<SwipeDirection> OnSwiped;

    private RectTransform _rect;
    private Canvas _canvas;
    private Vector2 _restPosition;
    private bool _animating;

    // Set up on first use, not in Awake: the screen may call ResetCard before this card's Awake has run
    private void EnsureInitialized()
    {
        if (_rect != null)
        {
            return;
        }

        _rect = (RectTransform)transform;
        _canvas = GetComponentInParent<Canvas>();
        _restPosition = _rect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        EnsureInitialized();
        if (_animating)
        {
            return;
        }

        // Screen pixels to canvas units, so the card follows the pointer at any resolution
        float scale = _canvas != null ? _canvas.scaleFactor : 1f;
        _rect.anchoredPosition += eventData.delta / scale;
        ApplyTilt();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EnsureInitialized();
        if (_animating)
        {
            return;
        }

        SwipeDirection direction = GetDirection(_rect.anchoredPosition - _restPosition);
        if (direction == SwipeDirection.None)
        {
            SnapBack();
            return;
        }

        OnSwiped?.Invoke(direction);
    }

    // Instantly back to the middle, upright. Used before showing the next card
    public void ResetCard()
    {
        EnsureInitialized();
        StopAllCoroutines();
        _animating = false;
        _rect.anchoredPosition = _restPosition;
        _rect.localRotation = Quaternion.identity;
        UpdateHints();
    }

    // The words that fade in while dragging. Null hides that hint (e.g. no Inspect for item cards)
    public void SetHints(string left, string right, string up)
    {
        SetHintText(_leftHint, left);
        SetHintText(_rightHint, right);
        SetHintText(_upHint, up);
    }

    // Slides back to the middle, e.g. after a swipe that isn't allowed or an inspect
    public void SnapBack()
    {
        EnsureInitialized();
        StopAllCoroutines();
        StartCoroutine(MoveTo(_restPosition, null));
    }

    // Flies off screen in that direction, then calls onDone
    public void FlyOff(SwipeDirection direction, Action onDone)
    {
        EnsureInitialized();
        StopAllCoroutines();
        StartCoroutine(MoveTo(_restPosition + DirectionVector(direction) * _flyOffDistance, onDone));
    }

    private IEnumerator MoveTo(Vector2 target, Action onDone)
    {
        _animating = true;
        Vector2 start = _rect.anchoredPosition;

        for (float t = 0f; t < _animationSeconds; t += Time.unscaledDeltaTime)
        {
            _rect.anchoredPosition = Vector2.Lerp(start, target, t / _animationSeconds);
            ApplyTilt();
            yield return null;
        }

        _rect.anchoredPosition = target;
        ApplyTilt();
        _animating = false;
        onDone?.Invoke();
    }

    private void ApplyTilt()
    {
        float sideways = Mathf.Clamp((_rect.anchoredPosition.x - _restPosition.x) / _swipeDistance, -1f, 1f);
        _rect.localRotation = Quaternion.Euler(0f, 0f, -sideways * _maxTilt);
        UpdateHints();
    }

    // Each hint is invisible at rest and fully shown at the swipe distance, so the player sees what letting go will do
    private void UpdateHints()
    {
        Vector2 offset = _rect.anchoredPosition - _restPosition;
        bool mostlyUp = offset.y > Mathf.Abs(offset.x);

        SetHintAlpha(_leftHint, mostlyUp ? 0f : -offset.x / _swipeDistance);
        SetHintAlpha(_rightHint, mostlyUp ? 0f : offset.x / _swipeDistance);
        SetHintAlpha(_upHint, mostlyUp ? offset.y / _swipeDistance : 0f);
    }

    private static void SetHintText(TMP_Text hint, string text)
    {
        if (hint == null)
        {
            return;
        }

        hint.gameObject.SetActive(!string.IsNullOrEmpty(text));
        hint.text = text;
    }

    private static void SetHintAlpha(TMP_Text hint, float alpha)
    {
        if (hint != null)
        {
            hint.alpha = Mathf.Clamp01(alpha);
        }
    }

    // Up wins only when the drag is mostly vertical, so a diagonal drag still counts as left or right
    private SwipeDirection GetDirection(Vector2 offset)
    {
        if (offset.y >= _swipeDistance && offset.y > Mathf.Abs(offset.x))
        {
            return SwipeDirection.Up;
        }

        if (offset.x >= _swipeDistance)
        {
            return SwipeDirection.Right;
        }

        if (offset.x <= -_swipeDistance)
        {
            return SwipeDirection.Left;
        }

        return SwipeDirection.None;
    }

    private static Vector2 DirectionVector(SwipeDirection direction)
    {
        switch (direction)
        {
            case SwipeDirection.Left:
                return Vector2.left;
            case SwipeDirection.Right:
                return Vector2.right;
            case SwipeDirection.Up:
                return Vector2.up;
            default:
                return Vector2.zero;
        }
    }
}
