using NUnit.Framework;

public class RelationshipGraphTests
{
    [Test]
    public void MissingPair_MeansNoAffinityAndNoStatus()
    {
        RelationshipGraph graph = new RelationshipGraph();

        Assert.IsNull(graph.Get("a", "b"));
        Assert.AreEqual(0, graph.GetAffinity("a", "b"));
        Assert.AreEqual(RelationshipStatus.None, graph.GetStatus("a", "b"));
    }

    [Test]
    public void GetOrCreate_EitherOrder_ReturnsSameRecord()
    {
        RelationshipGraph graph = new RelationshipGraph();

        RelationshipData first = graph.GetOrCreate("a", "b");
        RelationshipData second = graph.GetOrCreate("b", "a");

        Assert.AreSame(first, second);
        Assert.AreEqual(1, graph.All.Count);
    }

    [Test]
    public void GetOrCreate_SameHeroTwice_Throws()
    {
        RelationshipGraph graph = new RelationshipGraph();
        Assert.Throws<System.ArgumentException>(() => graph.GetOrCreate("a", "a"));
    }

    [Test]
    public void GetOtherId_ReturnsTheOtherSide()
    {
        RelationshipData relationship = new RelationshipGraph().GetOrCreate("a", "b");

        Assert.AreEqual("b", relationship.GetOtherId("a"));
        Assert.AreEqual("a", relationship.GetOtherId("b"));
    }

    [Test]
    public void CountWithStatus_OnlyCountsThatHero()
    {
        RelationshipGraph graph = new RelationshipGraph();
        graph.GetOrCreate("a", "b").Status = RelationshipStatus.Dating;
        graph.GetOrCreate("a", "c").Status = RelationshipStatus.Dating;
        graph.GetOrCreate("b", "c").Affinity = 40;

        Assert.AreEqual(2, graph.CountWithStatus("a", RelationshipStatus.Dating));
        Assert.AreEqual(1, graph.CountWithStatus("c", RelationshipStatus.Dating));
    }

    [Test]
    public void HandleDeath_DatingBecomesWidowed_ExKept_PlainRemoved()
    {
        RelationshipGraph graph = new RelationshipGraph();
        graph.GetOrCreate("dead", "partner").Status = RelationshipStatus.Dating;
        graph.GetOrCreate("dead", "ex").Status = RelationshipStatus.Ex;
        graph.GetOrCreate("dead", "friend").Affinity = 30;
        graph.GetOrCreate("partner", "friend").Affinity = 10;

        graph.HandleDeath("dead");

        Assert.AreEqual(RelationshipStatus.Widowed, graph.GetStatus("dead", "partner"));
        Assert.AreEqual(RelationshipStatus.Ex, graph.GetStatus("dead", "ex"));
        Assert.IsNull(graph.Get("dead", "friend"));
        Assert.IsNotNull(graph.Get("partner", "friend"), "Records without the dead hero are untouched");
    }
}