namespace Encore.Server;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class CommandHandlerAttribute : Attribute
{
    public CommandHandlerAttribute()
    {
        RequestCommand  = null;
        ResponseCommand = null;
    }

    public CommandHandlerAttribute(object reqCommand)
    {
        if (!reqCommand.GetType().IsEnum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reqCommand), "The specified parameter must be an instance of Enum type"
            );
        }

        RequestCommand = (Enum)reqCommand;
        ResponseCommand = null;
    }

    public CommandHandlerAttribute(object reqCommand, object resCommand)
    {
        if (!reqCommand.GetType().IsEnum || !resCommand.GetType().IsEnum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reqCommand), "The specified parameter must be an instance of Enum type"
            );
        }

        RequestCommand  = (Enum)reqCommand;
        ResponseCommand = (Enum)resCommand;
    }

    public Enum? RequestCommand { get; }

    public Enum? ResponseCommand { get; }
}
