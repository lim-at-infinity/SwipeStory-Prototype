using System;
using UnityEngine;

// Placeholder look for each hero class: a shape only. Color belongs to each hero (HeroData.Color), not the class.
// Read by HeroPortrait and anything else that draws a hero.
// Swap in real art by replacing the sprites on the asset; no code changes
[CreateAssetMenu(menuName = "SwipeStory/Class Visuals", fileName = "ClassVisuals")]
public class ClassVisuals : ScriptableObject
{
    [SerializeField] private ClassVisual[] _visuals =
    {
        new ClassVisual(HeroClass.Warrior, null, null),
        new ClassVisual(HeroClass.Mage, null, null),
        new ClassVisual(HeroClass.Rogue, null, null),
        new ClassVisual(HeroClass.Healer, null, null)
    };

    // If no class visual exist for a heroClass it shows a plain white box instead
    public ClassVisual Get(HeroClass heroClass)
    {
        foreach (ClassVisual visual in _visuals)
        {
            if (visual.Class == heroClass)
            {
                return visual;
            }
        }

        Debug.LogWarning("[ClassVisuals] No visual for " + heroClass + ".", this);
        return new ClassVisual(heroClass, null, null);
    }

    private void OnValidate()
    {
        foreach (HeroClass heroClass in Enum.GetValues(typeof(HeroClass)))
        {
            bool found = false;
            foreach (ClassVisual visual in _visuals)
            {
                if (visual.Class != heroClass)
                {
                    continue;
                }

                found = true;
                if (visual.Sprite == null)
                {
                    Debug.LogWarning("[ClassVisuals] " + heroClass + " has no sprite.", this);
                }
            }

            if (!found)
            {
                Debug.LogWarning("[ClassVisuals] No visual for " + heroClass + ".", this);
            }
        }
    }
}

// The look of one class. Sprite is the small token (roster tiles, battle); Portrait is the large art
// (recruit cards, dialogue). Placeholder sprites are white so each hero's own color can tint them.
// Prototype: Portrait is left empty on purpose and PortraitOrSprite falls back to Sprite, so both look the same
[Serializable]
public struct ClassVisual
{
    [field: SerializeField] public HeroClass Class { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField, Tooltip("Optional. Empty uses Sprite (the prototype leaves it empty)")]
    public Sprite Portrait { get; private set; }
    public Sprite PortraitOrSprite => Portrait != null ? Portrait : Sprite;

    public ClassVisual(HeroClass heroClass, Sprite sprite, Sprite portrait)
    {
        Class = heroClass;
        Sprite = sprite;
        Portrait = portrait;
    }
}
