using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }

    public event Action<ScreenId, ScreenId> OnScreenChanged; // (from, to)

    [SerializeField] private Transform _screenRoot;          // Canvas/Screens
    [SerializeField] private ScreenId _startScreen = ScreenId.Town;

    private readonly Dictionary<ScreenId, ScreenBase> _screens = new Dictionary<ScreenId, ScreenBase>();
    private readonly Stack<ScreenId> _history = new Stack<ScreenId>();

    public ScreenId Current { get; private set; } = ScreenId.None;

    public HeroData SelectedHero { get; set; }
    public AdventureContext Adventure { get; } = new AdventureContext();

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNewGame += HandleNewGame;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNewGame -= HandleNewGame;
        }
    }

    private void HandleNewGame()
    {
        SelectedHero = null;
        Adventure.Reset();
    }

    private void Awake()
    {
        Instance = this;

        foreach (ScreenBase screen in _screenRoot.GetComponentsInChildren<ScreenBase>(true))
        {
            screen.gameObject.SetActive(false);

            if (_screens.ContainsKey(screen.Id))
            {
                Debug.LogError("[ScreenManager] " + screen.name + " and " + _screens[screen.Id].name + " both use Id " + screen.Id);
                continue;
            }

            _screens.Add(screen.Id, screen);
        }
    }

    private void Start()
    {
        SwitchTo(_startScreen);
    }

    public void Show(ScreenId id)
    {
        if (id == Current)
        {
            return;
        }

        // Town and Recruit are fresh starts: Back should never leave them
        if (id == ScreenId.Town || id == ScreenId.Recruit)
        {
            _history.Clear();
        }
        else if (Current != ScreenId.None)
        {
            _history.Push(Current);
        }

        SwitchTo(id);
    }

    public void Back()
    {
        if (_history.Count > 0)
        {
            SwitchTo(_history.Pop());
        }
    }

    private void SwitchTo(ScreenId id)
    {
        if (!_screens.ContainsKey(id))
        {
            Debug.LogError("[ScreenManager] No screen for " + id);
            return;
        }

        ScreenId from = Current;
        if (Current != ScreenId.None)
        {
            _screens[Current].gameObject.SetActive(false);
        }

        Current = id;
        _screens[id].gameObject.SetActive(true);
        OnScreenChanged?.Invoke(from, id);
    }
}