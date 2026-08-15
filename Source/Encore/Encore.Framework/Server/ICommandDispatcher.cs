using System.Diagnostics.CodeAnalysis;
using Encore.Messaging;
using Encore.Sessions;

namespace Encore.Server;

public interface ICommandDispatcher
{
    ICommandDispatcher Map<TSession, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TRequest>(Func<TSession, TRequest, Task> handler)
        where TSession : ISession
        where TRequest : class, IMessage;

    ICommandDispatcher Map<TSession, TCommand>(TCommand command, Func<TSession, Task> handler)
        where TSession : ISession
        where TCommand  : Enum;

    ICommandDispatcher Map<TSession, TCommand>(TCommand command, Func<TSession, CancellationToken, Task> handler)
        where TSession : ISession
        where TCommand  : Enum;

    ICommandDispatcher Map<TSession, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResponse>(Func<TSession, TRequest,
        Task<TResponse>> handler)
        where TSession : ISession
        where TRequest  : class, IMessage
        where TResponse : class, IMessage;

    ICommandDispatcher Map<TSession, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResponse
    >(Func<TSession, TRequest, CancellationToken, Task<TResponse>> handler)
        where TSession : ISession
        where TRequest  : class, IMessage
        where TResponse : class, IMessage;

    [RequiresUnreferencedCode("Controller registration require reflection to scan available handlers")]
    ICommandDispatcher Map<TSession, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TController>(Func<TSession, TController> factory, params IEnumerable<ICommandFilter> filterFactories)
        where TSession : ISession
        where TController : CommandController;

    ICommandDispatcher AddFilter<TCommandFilter>()
        where TCommandFilter : class, ICommandFilter, new();

    ICommandDispatcher AddFilter(ICommandFilter filter);

    ICommandDispatcher AddExceptionFilter<TExceptionHandler>()
        where TExceptionHandler : class, ICommandExceptionHandler, new();

    ICommandDispatcher AddExceptionFilter(ICommandExceptionHandler handler);

    ICommandDispatcher AddExceptionLogger<TExceptionLogger>()
        where TExceptionLogger : class, ICommandExceptionLogger, new();
    ICommandDispatcher AddExceptionLogger(ICommandExceptionLogger logger);

    Task Dispatch(ISession session, byte[] payload, CancellationToken cancellationToken);

    Task Dispatch<TMessage>(ISession session, TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage;

    Task Dispatch(ISession session, Enum command, CancellationToken cancellationToken);
}
