using System;
using System.Collections.Generic;
using UnityEngine;

// Every tuning number in the game. One asset in Assets/Data/Config, reached through GameManager.Config
[CreateAssetMenu(menuName = "SwipeStory/Balance Config", fileName = "BalanceConfig")]
public class BalanceConfig : ScriptableObject
{
    public const int MaxAffinity = 100;

    [Header("Run")]
    [SerializeField, Min(0)] private int _startingGold = 100;
    [SerializeField, Min(0)] private int _startingUndoTokens = 1;

    [Header("Adventures")]
    [Tooltip("In unlock order. The last one is the boss; clearing it wins the run")]
    [SerializeField] private List<AdventureData> _adventures = new List<AdventureData>();

    [Header("Player hero")]
    [SerializeField] private string _defaultPlayerName = "Hero";
    [Tooltip("Used until the class pick screen exists")]
    [SerializeField] private HeroClass _defaultPlayerClass = HeroClass.Warrior;

    [Header("Roster and party")]
    [Tooltip("Every hero in the Adventurer Guild, including the player hero. 0 = unlimited")]
    [SerializeField, Min(0)] private int _rosterCapacity = 50;
    [Tooltip("Most heroes the player can send on one adventure, including the player hero")]
    [SerializeField, Min(1)] private int _partySize = 6;

    [Header("Day cycle")]
    [SerializeField, Min(1)] private int _recruitCardsPerNight = 5;
    [SerializeField, Min(0)] private int _talkCandidatesPerDay = 2;

    [Header("Hero stats and levels")]
    [Tooltip("One entry per HeroClass: level 1 ranges plus growth per level")]
    [SerializeField] private ClassStatProfile[] _classStats =
    {
        new ClassStatProfile(HeroClass.Warrior, new IntRange(30, 36), new IntRange(6, 8), new IntRange(5, 7), new IntRange(3, 5), 5, 2, 2, 1),
        new ClassStatProfile(HeroClass.Mage, new IntRange(18, 22), new IntRange(9, 11), new IntRange(2, 3), new IntRange(4, 6), 3, 3, 1, 1),
        new ClassStatProfile(HeroClass.Rogue, new IntRange(22, 26), new IntRange(7, 9), new IntRange(3, 4), new IntRange(7, 9), 4, 2, 1, 2),
        new ClassStatProfile(HeroClass.Healer, new IntRange(20, 24), new IntRange(3, 5), new IntRange(4, 5), new IntRange(4, 6), 4, 1, 2, 1)
    };
    [SerializeField, Min(1)] private int _maxLevel = 10;
    [Tooltip("Entry 0 is XP from level 1 to 2. Needs MaxLevel - 1 entries")]
    [SerializeField] private int[] _xpToNextLevel = { 10, 15, 22, 30, 40, 52, 66, 82, 100 };

    [Header("Hero colors")]
    [Tooltip("Every hero gets a random hue. These keep the colors from coming out washed out or too dark")]
    [SerializeField, Range(0f, 1f)] private float _heroColorSaturation = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _heroColorValue = 0.9f;

    [Header("Relationships")]
    [Tooltip("Lowest affinity for Stranger, Friend, Close, Devoted")]
    [SerializeField] private int[] _tierThresholds = { 0, 25, 50, 75 };
    [Tooltip("Flat battle stat bonus for Stranger, Friend, Close, Devoted")]
    [SerializeField] private int[] _tierStatBonus = { 0, 1, 2, 4 };
    [SerializeField, Min(0)] private int _affinityPerTalk = 5;
    [SerializeField, Min(0)] private int _affinityPerBattle = 3;
    [Tooltip("Asking out needs affinity above this")]
    [SerializeField, Range(0, MaxAffinity)] private int _askOutAffinityThreshold = 60;
    [Tooltip("Dating becomes Ex when affinity drops below this")]
    [SerializeField, Range(0, MaxAffinity)] private int _breakupAffinityThreshold = 40;
    [Tooltip("Partners allowed without a trait that overrides it")]
    [SerializeField, Min(1)] private int _defaultDatingCap = 1;

    [Header("Traits")]
    [SerializeField, Min(1)] private int _traitCardsPerLevelUp = 6;
    [SerializeField] private List<TraitData> _traitPool = new List<TraitData>();

    [Header("Rewards")]
    [SerializeField, Min(0f)] private float _goldRewardMultiplier = 1f;
    [SerializeField, Min(0f)] private float _xpRewardMultiplier = 1f;

    [Header("Shop")]
    [SerializeField] private List<ItemData> _shopItems = new List<ItemData>();

    public int StartingGold => _startingGold;
    public int StartingUndoTokens => _startingUndoTokens;
    public IReadOnlyList<AdventureData> Adventures => _adventures;
    public string DefaultPlayerName => _defaultPlayerName;
    public HeroClass DefaultPlayerClass => _defaultPlayerClass;
    public int RosterCapacity => _rosterCapacity;
    public int PartySize => _partySize;
    public int RecruitCardsPerNight => _recruitCardsPerNight;
    public int TalkCandidatesPerDay => _talkCandidatesPerDay;
    public int MaxLevel => _maxLevel;
    public float HeroColorSaturation => _heroColorSaturation;
    public float HeroColorValue => _heroColorValue;
    public int AffinityPerTalk => _affinityPerTalk;
    public int AffinityPerBattle => _affinityPerBattle;
    public int AskOutAffinityThreshold => _askOutAffinityThreshold;
    public int BreakupAffinityThreshold => _breakupAffinityThreshold;
    public int DefaultDatingCap => _defaultDatingCap;
    public int TraitCardsPerLevelUp => _traitCardsPerLevelUp;
    public IReadOnlyList<TraitData> TraitPool => _traitPool;
    public float GoldRewardMultiplier => _goldRewardMultiplier;
    public float XpRewardMultiplier => _xpRewardMultiplier;
    public IReadOnlyList<ItemData> ShopItems => _shopItems;

    // Position in the unlock order (0 = first adventure), or -1 if it isn't in the list
    public int GetAdventureIndex(AdventureData adventure)
    {
        return adventure == null ? -1 : _adventures.IndexOf(adventure);
    }

    public ClassStatProfile GetClassStats(HeroClass heroClass)
    {
        foreach (ClassStatProfile profile in _classStats)
        {
            if (profile.Class == heroClass)
            {
                return profile;
            }
        }

        throw new KeyNotFoundException("[BalanceConfig] No stat profile for " + heroClass);
    }

    // int.MaxValue at max level, so XP can never level past it
    public int GetXpToNextLevel(int level)
    {
        if (level >= _maxLevel)
        {
            return int.MaxValue;
        }

        int index = Mathf.Clamp(level - 1, 0, _xpToNextLevel.Length - 1);
        return _xpToNextLevel[index];
    }

    public RelationshipTier GetTierForAffinity(int affinity)
    {
        affinity = Mathf.Clamp(affinity, 0, MaxAffinity);

        for (int i = _tierThresholds.Length - 1; i > 0; i--)
        {
            if (affinity >= _tierThresholds[i])
            {
                return (RelationshipTier)i;
            }
        }

        return RelationshipTier.Stranger;
    }

    public int GetTierStatBonus(RelationshipTier tier)
    {
        int index = (int)tier;
        return index < _tierStatBonus.Length ? _tierStatBonus[index] : 0;
    }

    public bool CanAskOut(int affinity)
    {
        return affinity > _askOutAffinityThreshold;
    }

    public bool ShouldBreakUp(int affinity)
    {
        return affinity < _breakupAffinityThreshold;
    }

    private void OnValidate()
    {
        int tierCount = Enum.GetValues(typeof(RelationshipTier)).Length;
        if (_tierThresholds.Length != tierCount)
        {
            Debug.LogWarning("[BalanceConfig] Tier thresholds need " + tierCount + " entries.", this);
        }
        else
        {
            for (int i = 1; i < _tierThresholds.Length; i++)
            {
                if (_tierThresholds[i] <= _tierThresholds[i - 1])
                {
                    Debug.LogWarning("[BalanceConfig] Tier thresholds must go up.", this);
                    break;
                }
            }
        }

        if (_tierStatBonus.Length != tierCount)
        {
            Debug.LogWarning("[BalanceConfig] Tier stat bonus needs " + tierCount + " entries.", this);
        }

        if (_xpToNextLevel.Length != _maxLevel - 1)
        {
            Debug.LogWarning("[BalanceConfig] XP to next level needs " + (_maxLevel - 1) + " entries (MaxLevel - 1).", this);
        }

        if (_rosterCapacity > 0 && _partySize > _rosterCapacity)
        {
            Debug.LogWarning("[BalanceConfig] Party size is bigger than roster capacity.", this);
        }

        if (_breakupAffinityThreshold > _askOutAffinityThreshold)
        {
            Debug.LogWarning("[BalanceConfig] Breakup threshold is above the ask-out threshold.", this);
        }

        if (_adventures.Count == 0)
        {
            Debug.LogWarning("[BalanceConfig] No adventures listed.", this);
        }

        for (int i = 0; i < _adventures.Count; i++)
        {
            if (_adventures[i] == null)
            {
                Debug.LogWarning("[BalanceConfig] Adventure slot " + (i + 1) + " is empty.", this);
            }
            else if (_adventures.IndexOf(_adventures[i]) != i)
            {
                Debug.LogWarning("[BalanceConfig] " + _adventures[i].name + " is listed more than once.", this);
            }
        }

        foreach (HeroClass heroClass in Enum.GetValues(typeof(HeroClass)))
        {
            bool found = false;
            foreach (ClassStatProfile profile in _classStats)
            {
                if (profile.Class != heroClass)
                {
                    continue;
                }

                found = true;
                if (!profile.MaxHp.IsValid || !profile.Attack.IsValid || !profile.Defense.IsValid || !profile.Speed.IsValid)
                {
                    Debug.LogWarning("[BalanceConfig] " + heroClass + " has a stat range with Min above Max.", this);
                }
            }

            if (!found)
            {
                Debug.LogWarning("[BalanceConfig] No stat profile for " + heroClass + ".", this);
            }
        }
    }
}

// Level 1 stat ranges for one class, plus flat growth per level gained
[Serializable]
public struct ClassStatProfile
{
    [field: SerializeField] public HeroClass Class { get; private set; }
    [field: SerializeField] public IntRange MaxHp { get; private set; }
    [field: SerializeField] public IntRange Attack { get; private set; }
    [field: SerializeField] public IntRange Defense { get; private set; }
    [field: SerializeField] public IntRange Speed { get; private set; }
    [field: SerializeField] public int MaxHpPerLevel { get; private set; }
    [field: SerializeField] public int AttackPerLevel { get; private set; }
    [field: SerializeField] public int DefensePerLevel { get; private set; }
    [field: SerializeField] public int SpeedPerLevel { get; private set; }

    public ClassStatProfile(HeroClass heroClass, IntRange maxHp, IntRange attack, IntRange defense, IntRange speed,
        int maxHpPerLevel, int attackPerLevel, int defensePerLevel, int speedPerLevel)
    {
        Class = heroClass;
        MaxHp = maxHp;
        Attack = attack;
        Defense = defense;
        Speed = speed;
        MaxHpPerLevel = maxHpPerLevel;
        AttackPerLevel = attackPerLevel;
        DefensePerLevel = defensePerLevel;
        SpeedPerLevel = speedPerLevel;
    }
}
