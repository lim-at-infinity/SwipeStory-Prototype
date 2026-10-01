using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Talk scene. Town puts the hero in ScreenManager.SelectedHero before opening this screen.
// Placeholder for the visual novel: one choice, then the conversation ends and so does the day.
public class DialogueScreen : ScreenBase
{
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private Button _kindButton;
    [SerializeField] private Button _rudeButton;

    private HeroData _hero;

    private void Awake()
    {
        _kindButton.onClick.AddListener(() => Choose(GameManager.Instance.Config.AffinityPerTalk));
        _rudeButton.onClick.AddListener(() => Choose(-GameManager.Instance.Config.AffinityPerTalk));
    }

    private void OnEnable()
    {
        _hero = ScreenManager.Instance.SelectedHero;
        if (_hero == null)
        {
            _dialogueText.text = "No one to talk to.";
            return;
        }

        RelationshipSystem relationships = RelationshipSystem.Instance;
        _dialogueText.text = _hero.Name + " wants to chat. How do you respond?\n"
            + "Affinity: " + relationships.GetAffinity(_hero) + " (" + relationships.GetTier(_hero) + ")";
    }

    // Positive raises affinity, negative lowers it. Real choices will carry their own value
    private void Choose(int affinityChange)
    {
        if (_hero != null)
        {
            RelationshipSystem relationships = RelationshipSystem.Instance;
            int before = relationships.GetAffinity(_hero);
            relationships.AddAffinity(_hero, affinityChange);
            Debug.Log("[Dialogue] " + _hero.Name + " affinity " + before + " -> " + relationships.GetAffinity(_hero));
        }

        ScreenManager.Instance.SelectedHero = null;
        DayCycle.Instance.EndDay();
    }
}