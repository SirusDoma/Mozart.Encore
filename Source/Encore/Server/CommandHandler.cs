using System.Diagnostics.CodeAnalysis;
using Encore.Sessions;

namespace Encore.Server;

public sealed partial class CommandDispatcher
{
    private class CommandHandler
    {
        private readonly Func<Session, dynamic?, CancellationToken, Task<dynamic?>> _handler;
        private readonly CommandHandlerDescriptor _descriptor;

        public CommandHandler(Enum command, Func<Session, object?, CancellationToken, Task<object?>> handler)
        {
            _handler    = handler;
            _descriptor = new CommandHandlerDescriptor(command, null, null, null);
        }

        public CommandHandler(Enum requestCommand, Type? requestType,
            Func<Session, object?, CancellationToken, Task<object?>> handler)
        {
            _handler    = handler;
            _descriptor = new CommandHandlerDescriptor(requestCommand, requestType, null, null);
        }

        public CommandHandler(Enum requestCommand, Type? requestType,
            Enum? responseCommand, Type? responseType,
            Func<Session, object?, CancellationToken, Task<object?>> handler)
        {
            _handler    = handler;
            _descriptor = new CommandHandlerDescriptor(requestCommand, requestType, responseCommand, responseType);
        }

        public CommandHandler(CommandHandlerDescriptor descriptor, Func<Session, object?, CancellationToken, Task<object?>> handler)
        {
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            _handler = handler;
        }

        public CommandHandlerDescriptor Descriptor => _descriptor;

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public Type? RequestType => _descriptor.RequestType;

        public string? Name => _descriptor.Name;

        public IEnumerable<ICommandFilter> Filters => _descriptor.GetCommandFilters();

        public object[] Attributes => _descriptor.GetCustomAttributes();

        public Task<object?> Execute(Session session, object? request, CancellationToken cancellationToken)
        {
            return _handler(session, request, cancellationToken);
        }
    }
}
