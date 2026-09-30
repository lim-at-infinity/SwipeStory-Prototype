using System;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int> OnGoldChanged;        // (newGold)
    public event Action<int> OnUndoTokensChanged;  // (newCount)
    public event Action<int> OnAdventuresChanged;  // (completed)
    public event Action OnNewGame;

    [SerializeField] private BalanceConfig _config;

    public BalanceConfig Config => _config;
    public int Gold { get; private set; }
    public int UndoTokens { get; private set; }
    public int AdventuresCompleted { get; private set; }
    public bool IsRunOver => AdventuresCompleted >= _config.AdventuresPerRun;

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

    public void CompleteAdventure()
    {
        AdventuresCompleted++;
        OnAdventuresChanged?.Invoke(AdventuresCompleted);
    }
}
