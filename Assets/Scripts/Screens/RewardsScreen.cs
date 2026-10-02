using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shows how the adventure went and applies its result: rewards, survivors healed and closer to the player,
// the fallen removed from the roster, and the next adventure unlocked.
// Continue ends the day, or opens Summary when the run is over (boss cleared or player hero dead)
public class RewardsScreen : ScreenBase
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _detailsText;
    [SerializeField] private Button _continueButton;

    private AdventureResult _applied;   // so the same result is never applied twice

    private void Awake()
    {
        _continueButton.onClick.AddListener(Continue);
    }

    private void OnEnable()
    {
        AdventureResult result = ScreenManager.Instance.Adventure.LastResult;
        if (result == null)
        {
            _titleText.text = "Rewards";
            _detailsText.text = "No adventure result.";
            return;
        }

        if (result != _applied)
        {
            _detailsText.text = Apply(result);
            _applied = result;
        }

        _titleText.text = result.Won ? "Victory!" : "Defeat";
    }

    // Applies the result and returns the lines to show
    private string Apply(AdventureResult result)
    {
        GameManager game = GameManager.Instance;
        List<string> lines = new List<string>();

        // Rewards are all zero for a lost adventure
        game.AddGold(result.GoldEarned);
        lines.Add("Gold +" + result.GoldEarned);

        for (int i = 0; i < result.UndoTokensEarned; i++)
        {
            game.AddUndoToken();
        }

        if (result.UndoTokensEarned > 0)
        {
            lines.Add("Undo tokens +" + result.UndoTokensEarned);
        }

        foreach (ItemData item in result.ItemsEarned)
        {
            Inventory.Instance.Add(item);
            lines.Add("Found " + item.DisplayName);
        }

        // Survivors come home: full HP, and a bit closer to the player (the player hero is ignored for affinity)
        List<HeroData> fallen = new List<HeroData>(result.Fallen);
        int affinity = game.Config.AffinityPerBattle;
        bool anyNpcSurvived = false;
        foreach (HeroData hero in result.Party)
        {
            if (fallen.Contains(hero))
            {
                continue;
            }

            hero.RestoreFullHp();
            RelationshipSystem.Instance.AddAffinity(hero, affinity);
            anyNpcSurvived |= !hero.IsPlayer;
        }

        if (anyNpcSurvived)
        {
            lines.Add("Bond with your party +" + affinity);
        }

        // The fallen leave the roster for good; their gear is lost and their partners become Widowed
        foreach (HeroData hero in fallen)
        {
            RosterManager.Instance.MarkFallen(hero);
            lines.Add(hero.Name + " fell in battle");
        }

        // XP and leveling: not built yet

        if (result.Won)
        {
            int before = game.AdventuresCompleted;
            game.CompleteAdventure(result.Adventure);

            IReadOnlyList<AdventureData> adventures = game.Config.Adventures;
            if (game.AdventuresCompleted > before && game.AdventuresCompleted < adventures.Count)
            {
                lines.Add(adventures[game.AdventuresCompleted].DisplayName + " unlocked!");
            }
        }

        if (game.IsRunOver)
        {
            lines.Add(game.RunWon ? "The dragon is defeated!" : "Your hero has fallen. The run is over.");
        }

        Debug.Log("[Rewards] " + result.Adventure.DisplayName + ": gold now " + game.Gold + ", undo tokens " + game.UndoTokens);
        return string.Join("\n", lines);
    }

    private void Continue()
    {
        if (GameManager.Instance.IsRunOver)
        {
            ScreenManager.Instance.Show(ScreenId.Summary);
        }
        else
        {
            DayCycle.Instance.EndDay();
        }
    }
}