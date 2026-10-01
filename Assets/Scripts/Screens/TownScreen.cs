using UnityEngine;

// Town hub. Adventure, Shop and Roster are plain NavButtons.
// This script only decides whether the talk button is shown.
public class TownScreen : ScreenBase
{
    [SerializeField] private GameObject _talkButtons;

    private void OnEnable()
    {
        // Debug.Log("Roster: " + RosterManager.Instance.Heroes.Count + " heroes, player = " + RosterManager.Instance.PlayerHero.Name);
        _talkButtons.SetActive(HasHeroesToTalkTo());
    }

    // STUB: on Day 1 the roster is only the protagonist, so nobody to talk to.
    // Replace with "pick up to 2 random heroes from RosterManager" once it exists.
    private bool HasHeroesToTalkTo()
    {
        return DayCycle.Instance.Day > 1;
    }
}