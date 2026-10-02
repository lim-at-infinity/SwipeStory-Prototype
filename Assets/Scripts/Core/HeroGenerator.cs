using System;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class HeroGenerator : MonoBehaviour
{
    public static HeroGenerator Instance { get; private set; }

    private static readonly HeroClass[] AllClasses = (HeroClass[])Enum.GetValues(typeof(HeroClass));

    [Tooltip("0 = different heroes every run. Any other value repeats the same sequence")]
    [SerializeField] private int _seed;
    [SerializeField] private string[] _names = { "Brian", "Matt", "Alan", "Daisy", "Dunia", "Moe", "Miguel", "Carlos", "Chen", "Arushai" };

    private System.Random _rng;

    private void Awake()
    {
        Instance = this;
        _rng = _seed == 0 ? new System.Random() : new System.Random(_seed);
    }

    // One random recruit at the player hero's level
    public HeroData Generate()
    {
        HeroData player = RosterManager.Instance.PlayerHero;
        int level = player != null ? player.Level : 1;
        HeroClass heroClass = AllClasses[_rng.Next(AllClasses.Length)];
        string heroName = _names.Length > 0 ? _names[_rng.Next(_names.Length)] : heroClass.ToString();

        return HeroFactory.Create(GameManager.Instance.Config, _rng, heroName, heroClass, level);
    }

    public HeroData CreatePlayer(HeroClass heroClass, string heroName)
    {
        return HeroFactory.Create(GameManager.Instance.Config, _rng, heroName, heroClass, 1, true);
    }
}
