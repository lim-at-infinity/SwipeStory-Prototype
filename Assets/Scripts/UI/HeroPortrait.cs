using UnityEngine;
using UnityEngine.UI;

// Draws a hero's class shape on a UI Image, tinted with the hero's own color. Save it as a prefab with ClassVisuals assigned,
// so every screen that drops one in gets the right look without wiring anything
[RequireComponent(typeof(Image))]
public class HeroPortrait : MonoBehaviour
{
    [SerializeField] private ClassVisuals _visuals;
    [SerializeField] private bool _usePortrait;

    private Image _image;

    // Null hides the image
    public void Show(HeroData hero)
    {
        // Fetched here, not in Awake: Show can be called on a portrait that hasn't been active yet
        if (_image == null)
        {
            _image = GetComponent<Image>();
        }

        if (hero == null)
        {
            _image.enabled = false;
            return;
        }

        if (_visuals == null)
        {
            Debug.LogWarning("[HeroPortrait] No ClassVisuals assigned.", this);
            return;
        }

        ClassVisual visual = _visuals.Get(hero.Class);
        _image.sprite = _usePortrait ? visual.PortraitOrSprite : visual.Sprite;
        _image.color = hero.Color;
        _image.enabled = true;
    }
}
