using Encore.Server;
using Microsoft.Extensions.Logging;

namespace Mozart.Controllers.Filters;

public class DefaultExceptionLogger(ILogger<DefaultExceptionLogger> logger) : CommandExceptionLogger
{
    public override void Log(CommandExceptionLoggerContext context)
    {
        if (context.Exception is ArgumentException or ArgumentOutOfRangeException)
        {
            logger.LogWarning(context.Exception, "Invalid request parameters");
            return;
        }

        if (context.Descriptor is not null)
        {
            var type = context.Descriptor.RequestCommand.GetType();
            ushort command = Convert.ToUInt16(context.Descriptor.RequestCommand);

            logger.LogError(context.Exception,
                "An error occurred when executing '{Type}::{Command}' (0x{Code:X4}) command",
                type.Name, Enum.GetName(type, context.Descriptor.RequestCommand), command);
        }
        else
            logger.LogError(context.Exception, "An error occurred when processing request frame");

        // TODO: Check for known error
        // context.PropagateException = false;
    }
}