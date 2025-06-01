namespace Encore.Messaging;

public interface IMessage
{
    static abstract Enum Command { get; }
}

public abstract class SubMessage : IMessage
{
    public static Enum Command => throw new NotSupportedException("Cannot get Command of a SubMessage instance");
}
