using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DayCycleButton : MonoBehaviour
{
    private enum DayAction
    {
        EndDay,
        StartNextDay
    }

    [SerializeField] private DayAction _action;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Run);
    }

    private void Run()
    {
        if (_action == DayAction.EndDay)
        {
            DayCycle.Instance.EndDay();
        }
        else
        {
            DayCycle.Instance.StartNextDay();
        }
    }
}