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
    }

    [Test]
    public void Finish_StoresResultAndAddsToHistory()
    {
        AdventureContext context = new AdventureContext();
        context.Begin(_adventure, _party);
        BattleResult result = new BattleResult(true, _adventure, _party, null, 10, 5, 0, null);

        context.Finish(result);

        Assert.AreSame(result, context.LastResult);
        Assert.AreEqual(1, context.History.Count);
    }

    [Test]
    public void Begin_AfterFinish_KeepsHistoryButClearsLastResult()
    {
        AdventureContext context = new AdventureContext();
        context.Begin(_adventure, _party);
        context.Finish(new BattleResult(true, _adventure, _party, null, 0, 0, 0, null));

        context.Begin(_adventure, _party);

        Assert.IsNull(context.LastResult);
        Assert.AreEqual(1, context.History.Count);
    }

    [Test]
    public void Reset_ClearsEverything()
    {
        AdventureContext context = new AdventureContext();
        context.Begin(_adventure, _party);
        context.Finish(new BattleResult(false, _adventure, _party, null, 0, 0, 0, null));

        context.Reset();

        Assert.IsNull(context.Adventure);
        Assert.IsEmpty(context.Party);
        Assert.IsNull(context.LastResult);
        Assert.IsEmpty(context.History);
    }
}
