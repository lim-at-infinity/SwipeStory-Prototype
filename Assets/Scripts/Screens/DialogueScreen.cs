using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Talk scene. Town puts the hero in ScreenManager.SelectedHero before opening this screen.
// Placeholder for the visual novel: one choice, then a short result (the affinity change), then Continue ends the day
public class DialogueScreen : ScreenBase
{
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private Button _kindButton;
    [SerializeField] private Button _rudeButton;
    [SerializeField] private Button _continueButton;

    private HeroData _hero;

    private void Awake()
    {
        _kindButton.onClick.AddListener(() => Choose(GameManager.Instance.Config.AffinityPerTalk));
        _rudeButton.onClick.AddListener(() => Choose(-GameManager.Instance.Config.AffinityPerTalk));
        _continueButton.onClick.AddListener(EndConversation);
    }

    private void OnEnable()
    {
        _hero = ScreenManager.Instance.SelectedHero;
        if (_hero == null)
        {
            _dialogueText.text = "No one to talk to.";
            ShowChoices(false);
            return;
        }

        RelationshipSystem relationships = RelationshipSystem.Instance;
        _dialogueText.text = _hero.Name + " wants to chat. How do you respond?\n"
            + "Affinity: " + relationships.GetAffinity(_hero) + " (" + relationships.GetTier(_hero) + ")";
        ShowChoices(true);
    }

    // Positive raises affinity, negative lowers it. Real choices will carry their own value
    private void Choose(int affinityChange)
    {
        RelationshipSystem relationships = RelationshipSystem.Instance;
        int before = relationships.GetAffinity(_hero);
        RelationshipTier tierBefore = relationships.GetTier(_hero);

        relationships.AddAffinity(_hero, affinityChange);

        int after = relationships.GetAffinity(_hero);
        RelationshipTier tierAfter = relationships.GetTier(_hero);
        int change = after - before;   // can be smaller than asked: affinity stays between 0 and 100

        string reaction = affinityChange > 0 ? _hero.Name + " smiles. That went well." : _hero.Name + " frowns and walks off.";
        string text = reaction + "\nAffinity " + before + " -> " + after + " (" + (change >= 0 ? "+" : "") + change + ")";
        text += tierAfter != tierBefore
            ? "\nYou are now " + tierAfter + "!"
            : "\nRelationship: " + tierAfter;

        _dialogueText.text = text;
        ShowChoices(false);
    }

    private void EndConversation()
    {
        ScreenManager.Instance.SelectedHero = null;
        DayCycle.Instance.EndDay();
    }

    // Choices while talking; only Continue once the result is showing
    private void ShowChoices(bool show)
    {
        _kindButton.gameObject.SetActive(show);
        _rudeButton.gameObject.SetActive(show);
        _continueButton.gameObject.SetActive(!show);
    }
}