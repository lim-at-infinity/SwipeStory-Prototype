using TMPro;
using UnityEngine;

public class DayLabel : MonoBehaviour
{
    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        DayCycle.Instance.OnPhaseChanged += HandlePhaseChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (DayCycle.Instance != null)
        {
            DayCycle.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandlePhaseChanged(int day, bool isNight)
    {
        Refresh();
    }

    private void Refresh()
    {
        _text.text = DayCycle.Instance.PhaseLabel;
    }
}