using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Auto battle: fights each encounter of ScreenManager.Adventure in order and plays every fight back line by line,
// then records the results and opens Rewards. Skip finishes the playback instantly
public class BattleScreen : ScreenBase
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _logText;
    [SerializeField] private Transform _heroList;
    [SerializeField] private Transform _enemyList;
    [SerializeField] private BattleUnitRow _rowPrefab;
    [SerializeField] private Button _skipButton;
    [SerializeField] private float _secondsPerLine = 0.4f;
    [SerializeField] private float _pauseBetweenFights = 1f;
    [SerializeField] private int _visibleLogLines = 8;

    private readonly System.Random _rng = new System.Random();
    private readonly Dictionary<BattleUnit, BattleUnitRow> _rows = new Dictionary<BattleUnit, BattleUnitRow>();
    private readonly Queue<string> _logLines = new Queue<string>();
    private bool _skip;

    private void Awake()
    {
        _skipButton.onClick.AddListener(() => _skip = true);
    }

    private void OnEnable()
    {
        _skip = false;
        _logLines.Clear();
        _logText.text = "";
        StartCoroutine(RunAdventure());
    }

    private IEnumerator RunAdventure()
    {
        AdventureContext context = ScreenManager.Instance.Adventure;
        AdventureData adventure = context.Adventure;
        if (adventure == null)
        {
            Debug.LogError("[Battle] No adventure. Open Battle from Adventure Select.", this);
            yield break;
        }

        BattleSimulator simulator = new BattleSimulator(_rng);

        // One unit per hero for the whole adventure, so HP carries over between fights
        List<BattleUnit> heroes = new List<BattleUnit>();
        foreach (HeroData hero in context.Party)
        {
            heroes.Add(BattleUnit.FromHero(hero, RelationshipSystem.Instance.GetStatBonus(hero)));
        }

        _rows.Clear();
        ClearRows(_heroList);
        foreach (BattleUnit hero in heroes)
        {
            AddRow(hero, _heroList);
        }

        while (context.HasMoreEncounters)
        {
            int fightIndex = context.CurrentEncounterIndex;
            EncounterData encounter = adventure.Encounters[fightIndex];
            _titleText.text = adventure.DisplayName + "  ·  Fight " + (fightIndex + 1) + "/" + adventure.Encounters.Count
                + ": " + encounter.Name;

            // Number duplicate enemies so the log reads "Slime 1", "Slime 2"
            List<BattleUnit> enemies = new List<BattleUnit>();
            for (int i = 0; i < encounter.Enemies.Count; i++)
            {
                EnemyData enemy = encounter.Enemies[i];
                string name = encounter.Enemies.Count > 1 ? enemy.Name + " " + (i + 1) : enemy.Name;
                enemies.Add(BattleUnit.FromEnemy(enemy, name));
            }

            ClearRows(_enemyList);
            foreach (BattleUnit enemy in enemies)
            {
                AddRow(enemy, _enemyList);
            }

            // Simulate the whole fight instantly, then play it back
            List<BattleUnit> aliveBefore = heroes.FindAll(unit => unit.IsAlive);
            List<BattleEvent> events = new List<BattleEvent>();
            bool won = simulator.Fight(heroes, enemies, events);

            foreach (BattleEvent battleEvent in events)
            {
                AddLogLine(battleEvent.Text);
                if (battleEvent.Target != null)
                {
                    _rows[battleEvent.Target].SetHp(battleEvent.HpAfter);
                }

                if (!_skip)
                {
                    yield return new WaitForSeconds(_secondsPerLine);
                }
            }

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

            if (!_skip)
            {
                yield return new WaitForSeconds(_pauseBetweenFights);
            }

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

        ScreenManager.Instance.Show(ScreenId.Rewards);
    }

    private void AddRow(BattleUnit unit, Transform list)
    {
        BattleUnitRow row = Instantiate(_rowPrefab, list);
        row.Bind(unit);
        _rows[unit] = row;
    }

    private static void ClearRows(Transform list)
    {
        foreach (Transform child in list)
        {
            child.gameObject.SetActive(false);   // leaves the layout now; Destroy only happens at the end of the frame
            Destroy(child.gameObject);
        }
    }

    // Keeps only the last few lines so the log never overflows its box
    private void AddLogLine(string line)
    {
        _logLines.Enqueue(line);
        while (_logLines.Count > _visibleLogLines)
        {
            _logLines.Dequeue();
        }

        _logText.text = string.Join("\n", _logLines);
    }
}