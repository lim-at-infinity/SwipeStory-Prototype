using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Town hub. Adventure, Shop and Roster are plain NavButtons.
// This script picks today's talk candidates and wires the talk buttons.
public class TownScreen : ScreenBase
{
    [SerializeField] private Button[] _talkButtons;

    private readonly List<HeroData> _candidates = new List<HeroData>();
    private int _candidatesDay = -1;

    private void Awake()
    {
        for (int i = 0; i < _talkButtons.Length; i++)
        {
            int index = i; // copy for the lambda
            _talkButtons[i].onClick.AddListener(() => TalkTo(index));
        }
    }

    private void OnEnable()
    {
        if (_candidatesDay != DayCycle.Instance.Day)
        {
            PickCandidates();
            _candidatesDay = DayCycle.Instance.Day;
        }

        RefreshButtons();
    }

    private void PickCandidates()
    {
        _candidates.Clear();

        List<HeroData> pool = new List<HeroData>();
        foreach (HeroData hero in RosterManager.Instance.Heroes)
        {
            if (!hero.IsPlayer)
            {
                pool.Add(hero);
            }
        }

        int count = Mathf.Min(GameManager.Instance.Config.TalkCandidatesPerDay, pool.Count, _talkButtons.Length);
        for (int i = 0; i < count; i++)
        {
            int pick = Random.Range(0, pool.Count);
            _candidates.Add(pool[pick]);
            pool.RemoveAt(pick);
        }
    }

    private void RefreshButtons()
    {
        for (int i = 0; i < _talkButtons.Length; i++)
        {
            bool hasHero = i < _candidates.Count;
            _talkButtons[i].gameObject.SetActive(hasHero);

            if (hasHero)
            {
                _talkButtons[i].GetComponentInChildren<TMP_Text>().text = "Talk to " + _candidates[i].Name;
            }
        }
    }

    private void TalkTo(int index)
    {
        ScreenManager.Instance.SelectedHero = _candidates[index];
        ScreenManager.Instance.Show(ScreenId.Dialogue);
    }
}