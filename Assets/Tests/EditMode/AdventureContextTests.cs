using NUnit.Framework;
using UnityEngine;

public class AdventureContextTests
{
    private AdventureData _adventure;
    private HeroData[] _party;

    [SetUp]
    public void SetUp()
    {
        _adventure = ScriptableObject.CreateInstance<AdventureData>();
        _party = new[] { new HeroData("A", HeroClass.Warrior), new HeroData("B", HeroClass.Mage) };
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_adventure);
    }

    [Test]
    public void Begin_SetsAdventureAndParty()
    {
        AdventureContext context = new AdventureContext();
        context.Begin(_adventure, _party);

        Assert.AreSame(_adventure, context.Adventure);
        CollectionAssert.AreEqual(_party, context.Party);
        Assert.IsNull(context.LastResult);
        Assert.AreEqual(0, context.CurrentEncounterIndex);
    }

    [Test]
    public void RecordBattle_AdvancesEncounterIndex()
    {
        AdventureContext context = new AdventureContext();
        context.Begin(_adventure, _party);

        context.RecordBattle(new BattleResult(null, true, null));

        Assert.AreEqual(1, context.CurrentEncounterIndex);
        Assert.AreEqual(1, context.Battles.Count);
    }

    [Test]
    public void RecordBattle_BeforeBegin_Throws()
    {
        AdventureContext context = new AdventureContext();
        Assert.Throws<System.InvalidOperationException>(() => context.RecordBattle(new BattleResult(null, true, null)));
    }

    [Test]
    public void Finish_StoresResultAndAddsToHistory()
    {
        AdventureContext context = new AdventureContext();
        context.Begin(_adventure, _party);
        AdventureResult result = new AdventureResult(_adventure, _party, context.Battles, 10, 5, 0, null);

        context.Finish(result);

        Assert.AreSame(result, context.LastResult);
        Assert.AreEqual(1, context.History.Count);
    }

    [Test]
    public void Begin_AfterFinish_KeepsHistoryButClearsBattlesAndLastResult()
    {
        AdventureContext context = new AdventureContext();
        context.Begin(_adventure, _party);
        context.RecordBattle(new BattleResult(null, true, null));
        context.Finish(new AdventureResult(_adventure, _party, context.Battles, 0, 0, 0, null));

        context.Begin(_adventure, _party);

        Assert.IsNull(context.LastResult);
        Assert.IsEmpty(context.Battles);
        Assert.AreEqual(1, context.History.Count);
    }

    [Test]
    public void AdventureResult_WithALostBattle_IsNotWon()
    {
        BattleResult[] battles = { new BattleResult(null, true, null), new BattleResult(null, false, null) };
        AdventureResult result = new AdventureResult(_adventure, _party, battles, 0, 0, 0, null);

        Assert.IsFalse(result.Won);
    }

    [Test]
    public void AdventureResult_Fallen_CombinesEveryBattle()
    {
        BattleResult[] battles =
        {
            new BattleResult(null, true, new[] { _party[0] }),
            new BattleResult(null, true, new[] { _party[1] })
        };
        AdventureResult result = new AdventureResult(_adventure, _party, battles, 0, 0, 0, null);

        CollectionAssert.AreEquivalent(_party, result.Fallen);
    }

    [Test]
    public void Reset_ClearsEverything()
    {
        AdventureContext context = new AdventureContext();
        context.Begin(_adventure, _party);
        context.RecordBattle(new BattleResult(null, false, null));
        context.Finish(new AdventureResult(_adventure, _party, context.Battles, 0, 0, 0, null));

        context.Reset();

        Assert.IsNull(context.Adventure);
        Assert.IsEmpty(context.Party);
        Assert.IsEmpty(context.Battles);
        Assert.IsNull(context.LastResult);
        Assert.IsEmpty(context.History);
    }
}