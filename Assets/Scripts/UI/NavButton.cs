using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NavButton : MonoBehaviour
{
    [SerializeField] private ScreenId _target;
    [SerializeField] private bool _goBack;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Navigate);
    }

    private void Navigate()
    {
        if (_goBack)
        {
            ScreenManager.Instance.Back();
        }
        else
        {
            ScreenManager.Instance.Show(_target);
        }
    }
}