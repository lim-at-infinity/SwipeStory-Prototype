using System.Collections;
using UnityEngine;

public class BattleScreen : ScreenBase
{
    [SerializeField] private float _stubSeconds = 1f;

    private void OnEnable()
    {
        StartCoroutine(RunBattle());
    }

    private IEnumerator RunBattle()
    {
        yield return new WaitForSeconds(_stubSeconds);
        ScreenManager.Instance.Show(ScreenId.Rewards);
    }
}