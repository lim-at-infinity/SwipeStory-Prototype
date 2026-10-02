using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class DayCycle : MonoBehaviour
{
    public static DayCycle Instance { get; private set; }

    public event Action<int, bool> OnPhaseChanged; // (day, isNight)

    public int Day { get; private set; } = 1;
    public bool IsNight { get; private set; }

    public string PhaseLabel => "Day " + Day + (IsNight ? " · Night" : " · Daytime");

    private void Awake()
    {
        Instance = this;
    }

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

    // A new run starts on day 1, in the daytime
    private void HandleNewGame()
    {
        Day = 1;
        IsNight = false;
        OnPhaseChanged?.Invoke(Day, IsNight);
    }

    // Called after the day's main activity (Rewards or a conversation)
    public void EndDay()
    {
        if (IsNight)
        {
            Debug.LogWarning("[DayCycle] EndDay called at night, ignored.");
            return;
        }

        IsNight = true;
        OnPhaseChanged?.Invoke(Day, IsNight);
        ScreenManager.Instance.Show(ScreenId.Recruit);
    }

    // Called when the night's recruit cards are used up
    public void StartNextDay()
    {
        if (!IsNight)
        {
            Debug.LogWarning("[DayCycle] StartNextDay called during the day, ignored.");
            return;
        }

        Day++;
        IsNight = false;
        OnPhaseChanged?.Invoke(Day, IsNight);
        ScreenManager.Instance.Show(ScreenId.Town);
    }
}