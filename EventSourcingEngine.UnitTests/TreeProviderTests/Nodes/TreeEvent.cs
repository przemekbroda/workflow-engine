namespace EventSourcingEngine.UnitTests.TreeProviderTests.Nodes;

public abstract record TreeEvent
{
    public record Event1() : TreeEvent;
    public record Event2() : TreeEvent;
    public record Event3() : TreeEvent;
    public record Event4() : TreeEvent;
    public record Event5() : TreeEvent;
    public record Event6() : TreeEvent;
    public record Event7() : TreeEvent;
    public record Event8() : TreeEvent;
    public record Event9() : TreeEvent;
    public record Event10() : TreeEvent;
    public record Event11() : TreeEvent;
    public record Event12() : TreeEvent;
    public record Event13() : TreeEvent;
    public record Event14() : TreeEvent;
    public record Event15() : TreeEvent;
    public record Event16() : TreeEvent;
    public record WeirdEvent() : TreeEvent;
}