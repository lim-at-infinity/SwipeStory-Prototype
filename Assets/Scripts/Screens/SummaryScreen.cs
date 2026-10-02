using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// End of the run: won (boss cleared) or lost (player hero died). Shows how it went; New Run starts over on day 1
public class SummaryScreen : ScreenBase
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _detailsText;
    [SerializeField] private Button _newRunButton;

    private void Awake()
    {
        _newRunButton.onClick.AddListener(NewRun);
    }

    private void OnEnable()
    {
        GameManager game = GameManager.Instance;
        _titleText.text = game.RunWon ? "Victory!" : "Game Over";

        List<string> lines = new List<string>();
        lines.Add(game.RunWon ? "You defeated the dragon." : "Your hero has fallen.");
        lines.Add("Days: " + DayCycle.Instance.Day);
        lines.Add("Adventures cleared: " + game.AdventuresCompleted + "/" + game.Config.Adventures.Count);

        IReadOnlyList<AdventureResult> history = ScreenManager.Instance.Adventure.History;
        int wins = 0;
        foreach (AdventureResult result in history)
        {
            if (result.Won)
            {
                wins++;
            }
        }

        lines.Add("Adventures attempted: " + history.Count + " (" + wins + " won)");
        lines.Add("Heroes in the guild: " + RosterManager.Instance.Heroes.Count);

        IReadOnlyList<HeroData> fallen = RosterManager.Instance.FallenHeroes;
        if (fallen.Count > 0)
        {
            List<string> names = new List<string>();
            foreach (HeroData hero in fallen)
            {
                names.Add(hero.Name);
            }

            lines.Add("Fallen: " + string.Join(", ", names));
        }

        _detailsText.text = string.Join("\n", lines);
    }

    // NewGame resets gold, roster, inventory, relationships and adventure progress.
    // DayCycle, ScreenManager and Town listen to OnNewGame and reset themselves
    private void NewRun()
    {
        GameManager.Instance.NewGame();
        ScreenManager.Instance.Show(ScreenId.Town);
    }
}