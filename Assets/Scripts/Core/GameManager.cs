using System;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int> OnGoldChanged;        // (newGold)
    public event Action<int> OnUndoTokensChanged;  // (newCount)
    public event Action<int> OnAdventuresChanged;  // (completed)
    public event Action<bool> OnRunEnded;          // (won)
    public event Action OnNewGame;

    [SerializeField] private BalanceConfig _config;

    public BalanceConfig Config => _config;
    public int Gold { get; private set; }
    public int UndoTokens { get; private set; }

    // Unlock progress: how many adventures in Config.Adventures have been cleared, in order.
    // Replays don't count, so this is 0 to Config.Adventures.Count
    public int AdventuresCompleted { get; private set; }

    public bool IsRunOver { get; private set; }
    public bool RunWon { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (_config == null)
        {
            Debug.LogError("[GameManager] No BalanceConfig assigned.", this);
        }
    }

    // Start, not Awake: RosterManager, Inventory and HeroGenerator share this execution order,
    // so their Instances may not exist yet during Awake. Still runs before ScreenManager shows Town (-100)
    private void Start()
    {
        NewGame();
    }

    public void NewGame()
    {
        Gold = _config.StartingGold;
        UndoTokens = _config.StartingUndoTokens;
        AdventuresCompleted = 0;
        IsRunOver = false;
        RunWon = false;

        RosterManager.Instance.Clear();
        Inventory.Instance.Clear();

        HeroData player = HeroGenerator.Instance.CreatePlayer(_config.DefaultPlayerClass, _config.DefaultPlayerName);
        RosterManager.Instance.TryAddHero(player);

        OnGoldChanged?.Invoke(Gold);
        OnUndoTokensChanged?.Invoke(UndoTokens);
        OnAdventuresChanged?.Invoke(AdventuresCompleted);
        OnNewGame?.Invoke();
    }

    public bool TrySpendGold(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("[GameManager] TrySpendGold called with a negative amount, ignored.");
            return false;
        }

        if (Gold < amount)
        {
            return false;
        }

        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("[GameManager] AddGold called with a negative amount, ignored. Use TrySpendGold.");
            return;
        }

        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool TryUseUndoToken()
    {
        if (UndoTokens <= 0)
        {
            return false;
        }

        UndoTokens--;
        OnUndoTokensChanged?.Invoke(UndoTokens);
        return true;
    }

    public void AddUndoToken()
    {
        UndoTokens++;
        OnUndoTokensChanged?.Invoke(UndoTokens);
    }

    // The first adventure is always unlocked; each one after it unlocks when the one before is cleared
    public bool IsUnlocked(AdventureData adventure)
    {
        int index = _config.GetAdventureIndex(adventure);
        return index >= 0 && index <= AdventuresCompleted;
    }

    public bool IsCleared(AdventureData adventure)
    {
        int index = _config.GetAdventureIndex(adventure);
        return index >= 0 && index < AdventuresCompleted;
    }

    // Call after a won adventure. Only a first clear of the newest unlocked adventure advances progress;
    // replays change nothing. Clearing the last adventure (the boss) wins the run
    public void CompleteAdventure(AdventureData adventure)
    {
        if (_config.GetAdventureIndex(adventure) != AdventuresCompleted)
        {
            return;
        }

        AdventuresCompleted++;
        OnAdventuresChanged?.Invoke(AdventuresCompleted);

        if (AdventuresCompleted >= _config.Adventures.Count)
        {
            EndRun(true);
        }
    }

    // won: the boss was cleared (called by CompleteAdventure). Lost: the player hero died (called by battle)
    public void EndRun(bool won)
    {
        if (IsRunOver)
        {
            return;
        }

        IsRunOver = true;
        RunWon = won;
        OnRunEnded?.Invoke(won);
    }
}
