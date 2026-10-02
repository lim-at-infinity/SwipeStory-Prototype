using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Town hub. Adventure, Shop and Inn are plain NavButtons.
// This script picks today's talk candidates, wires the talk buttons, and switches between day and night:
// at night only the Inn is open (Adventure, Shop and talking are daytime activities)
public class TownScreen : ScreenBase
{
    [SerializeField] private Button[] _talkButtons;
    [Tooltip("Hidden at night, e.g. the Adventure and Shop buttons. Talk buttons are hidden at night as well")]
    [SerializeField] private GameObject[] _dayOnly;

    private readonly List<HeroData> _candidates = new List<HeroData>();
    private int _candidatesDay = -1;

    private void Awake()
    {
        for (int i = 0; i < _talkButtons.Length; i++)
        {
            int index = i; // copy for the lambda
            _talkButtons[i].onClick.AddListener(() => TalkTo(index));
        }
        
        GameManager.Instance.OnNewGame += HandleNewGame;
    }

    private void OnEnable()
    {
        if (_candidatesDay != DayCycle.Instance.Day)
        {
            PickCandidates();
            _candidatesDay = DayCycle.Instance.Day;
        }

        DayCycle.Instance.OnPhaseChanged += HandlePhaseChanged;
        RefreshButtons();
    }

    private void OnDisable()
    {
        if (DayCycle.Instance != null)
        {
            DayCycle.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    // Covers phase changes while Town is already showing (ScreenManager.Show(Town) won't re-run OnEnable then)
    private void HandlePhaseChanged(int day, bool isNight)
    {
        if (_candidatesDay != day)
        {
            PickCandidates();
            _candidatesDay = day;
        }

        RefreshButtons();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNewGame -= HandleNewGame;
        }
    }

    // A new run has a new roster, so today's candidates must be picked again
    private void HandleNewGame()
    {
        _candidatesDay = -1;
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
        bool isNight = DayCycle.Instance.IsNight;

        foreach (GameObject dayOnly in _dayOnly)
        {
            if (dayOnly != null)
            {
                dayOnly.SetActive(!isNight);
            }
        }

        for (int i = 0; i < _talkButtons.Length; i++)
        {
            bool hasHero = !isNight && i < _candidates.Count;
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