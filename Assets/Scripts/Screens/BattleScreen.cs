using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Auto battle: fights each encounter of ScreenManager.Adventure in order, records the results, then opens Rewards.
// For now the fight log goes to the Console; the battle UI comes next
public class BattleScreen : ScreenBase
{
    [SerializeField] private float _stubSeconds = 1f;

    private readonly System.Random _rng = new System.Random();

    private void OnEnable()
    {
        RunAdventure();
        StartCoroutine(GoToRewards());
    }

    private IEnumerator GoToRewards()
    {
        yield return new WaitForSeconds(_stubSeconds);
        ScreenManager.Instance.Show(ScreenId.Rewards);
    }

    private void RunAdventure()
    {
        AdventureContext context = ScreenManager.Instance.Adventure;
        AdventureData adventure = context.Adventure;
        if (adventure == null)
        {
            Debug.LogError("[Battle] No adventure. Open Battle from Adventure Select.", this);
            return;
        }

        BattleSimulator simulator = new BattleSimulator(_rng);

        // One unit per hero for the whole adventure, so HP carries over between fights
        List<BattleUnit> heroes = new List<BattleUnit>();
        foreach (HeroData hero in context.Party)
        {
            heroes.Add(BattleUnit.FromHero(hero, RelationshipSystem.Instance.GetStatBonus(hero)));
        }

        while (context.HasMoreEncounters)
        {
            EncounterData encounter = adventure.Encounters[context.CurrentEncounterIndex];

            // Number duplicate enemies so the log reads "Slime 1", "Slime 2"
            List<BattleUnit> enemies = new List<BattleUnit>();
            for (int i = 0; i < encounter.Enemies.Count; i++)
            {
                EnemyData enemy = encounter.Enemies[i];
                string name = encounter.Enemies.Count > 1 ? enemy.Name + " " + (i + 1) : enemy.Name;
                enemies.Add(BattleUnit.FromEnemy(enemy, name));
            }

            List<BattleUnit> aliveBefore = heroes.FindAll(unit => unit.IsAlive);
            List<string> log = new List<string>();
            bool won = simulator.Fight(heroes, enemies, log);
            Debug.Log("[Battle] " + encounter.Name + "\n" + string.Join("\n", log));

            // NPC heroes who died in this fight. The player hero's death ends the run instead
            List<HeroData> fallen = new List<HeroData>();
            bool playerDied = false;
            foreach (BattleUnit unit in aliveBefore)
            {
                if (unit.IsAlive)
                {
                    continue;
                }

                if (unit.Hero.IsPlayer)
                {
                    playerDied = true;
                }
                else
                {
                    fallen.Add(unit.Hero);
                }
            }

            context.RecordBattle(new BattleResult(encounter, won, fallen));

            if (playerDied)
            {
                GameManager.Instance.EndRun(false);
                break;
            }

            if (!won)
            {
                break;
            }
        }

        // Write HP back so other screens can show it. Rewards restores survivors to full
        foreach (BattleUnit unit in heroes)
        {
            unit.Hero.CurrentHp = unit.CurrentHp;
        }

        // Rewards only for a cleared adventure (every encounter fought and won)
        BalanceConfig config = GameManager.Instance.Config;
        bool cleared = !context.HasMoreEncounters && context.Battles[context.Battles.Count - 1].Won;
        context.Finish(new AdventureResult(adventure, context.Party, context.Battles,
            cleared ? Mathf.RoundToInt(adventure.GoldReward * config.GoldRewardMultiplier) : 0,
            cleared ? Mathf.RoundToInt(adventure.XpReward * config.XpRewardMultiplier) : 0,
            cleared ? adventure.UndoTokenReward : 0,
            cleared ? adventure.ItemRewards : null));

        Debug.Log("[Battle] " + adventure.DisplayName + (cleared ? " cleared" : " failed")
            + ", gold " + context.LastResult.GoldEarned + ", fallen " + context.LastResult.Fallen.Count);
    }
}