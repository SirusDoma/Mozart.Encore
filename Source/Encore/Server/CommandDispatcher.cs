using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Encore.Messaging;
using Encore.Sessions;

namespace Encore.Server;

public sealed partial class CommandDispatcher : ICommandDispatcher
{
    private readonly Dictionary<Enum, List<CommandHandler>> _handlers = new();
    private readonly IMessageCodec _codec;
    private readonly IList<ICommandFilter> _filters = [];
    private readonly List<ICommandExceptionLogger>  _exceptionLogger = [];

    private ICommandExceptionHandler? _exceptionHandler = null;

    public CommandDispatcher()
    {
        _codec = new DefaultMessageCodec();
    }

    public CommandDispatcher(IMessageCodec codec)
    {
        _codec  = codec;
    }

    ICommandDispatcher ICommandDispatcher.Map<TSession, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TRequest>(Func<TSession, TRequest, Task> handler)
    {
        MapRoute((Session session, TRequest request, CancellationToken _) => handler((TSession)session, request));
        return this;
    }

    ICommandDispatcher ICommandDispatcher.Map<TSession, TCommand>(TCommand command, Func<TSession, Task> handler)
    {
        MapRoute(command, (session, _) => handler((TSession)session));
        return this;
    }

    ICommandDispatcher ICommandDispatcher.Map<TSession, TCommand>(TCommand command,
        Func<TSession, CancellationToken, Task> handler)
    {
        MapRoute(command, (session, token) => handler((TSession)session, token));
        return this;
    }

    ICommandDispatcher ICommandDispatcher.Map<TSession,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResponse
    >(Func<TSession, TRequest, Task<TResponse>> handler)
    {
        MapRoute((Session session, TRequest request, CancellationToken _) => handler((TSession)session, request));
        return this;
    }

    ICommandDispatcher ICommandDispatcher.Map<TSession,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResponse
    >(Func<TSession, TRequest, CancellationToken, Task<TResponse>> handler)
    {
        MapRoute((Session session, TRequest request, CancellationToken token) =>
            handler((TSession)session, request, token));

        return this;
    }

    [RequiresUnreferencedCode("Controller registration require reflection to scan available handlers")]
    ICommandDispatcher ICommandDispatcher.Map<TSession,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TController>(Func<TSession, TController> factory, params IEnumerable<ICommandFilter> filters)
    {
        MapRoute(factory, filters);
        return this;
    }

    ICommandDispatcher ICommandDispatcher.AddFilter<TCommandFilter>()
    {
        _filters.Add(new TCommandFilter());
        return this;
    }

    ICommandDispatcher ICommandDispatcher.AddFilter(ICommandFilter filter)
    {
        _filters.Add(filter);
        return this;
    }

    ICommandDispatcher ICommandDispatcher.AddExceptionFilter<TExceptionHandler>()
    {
        if (_exceptionHandler != null)
            throw new InvalidOperationException("An exception filter has already been added");

        _exceptionHandler = new TExceptionHandler();
        return this;
    }

    ICommandDispatcher ICommandDispatcher.AddExceptionFilter(ICommandExceptionHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (_exceptionHandler != null)
            throw new InvalidOperationException("An exception filter has already been added");

        _exceptionHandler = handler;
        return this;
    }

    ICommandDispatcher ICommandDispatcher.AddExceptionLogger<TExceptionLogger>()
    {
        _exceptionLogger.Add(new TExceptionLogger());
        return this;
    }

    ICommandDispatcher ICommandDispatcher.AddExceptionLogger(ICommandExceptionLogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _exceptionLogger.Add(logger);

        return this;
    }

    private void ValidateRequestType<TCommand>(TCommand command, Type requestType)
        where TCommand : Enum
    {
        if (!_handlers.TryGetValue(command, out var existingHandlers))
            return;

        foreach (var existingHandler in existingHandlers)
        {
            var existingType = existingHandler.RequestType;

            if (existingType == null)
                continue;

            if (!existingType.IsAssignableFrom(requestType) && !requestType.IsAssignableFrom(existingType))
            {
                throw new InvalidOperationException(
                    $"Request type mismatch for command '{command}'. " +
                    $"Existing handlers expect '{existingType.Name}', " +
                    $"but new handler expects '{requestType.Name}'. " +
                    "All handlers for the same command must have uniform assignable request types."
                );
            }
        }
    }

    private void MapRoute<TCommand>(TCommand command, Func<Session, CancellationToken, Task> handler)
        where TCommand  : Enum
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(handler);

        var commandHandler = new CommandHandler(command, async (session, _, cancellationToken) =>
        {
            await handler(session, cancellationToken).ConfigureAwait(false);
            return null;
        });

        if (!_handlers.TryGetValue(command, out var handlers))
        {
            handlers = new List<CommandHandler>();
            _handlers[command] = handlers;
        }
        handlers.Add(commandHandler);
    }

    private void MapRoute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]  TRequest>(
        Func<Session, TRequest, CancellationToken, Task> handler
    )
        where TRequest  : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(TRequest.Command);
        ArgumentNullException.ThrowIfNull(handler);

        ValidateRequestType(TRequest.Command, typeof(TRequest));

        var commandHandler = new CommandHandler(TRequest.Command, typeof(TRequest),
            async (session, request, cancellationToken) =>
            {
                await handler(session, (TRequest)request!, cancellationToken).ConfigureAwait(false);
                return null;
            }
        );

        if (!_handlers.TryGetValue(TRequest.Command, out var handlers))
        {
            handlers = new List<CommandHandler>();
            _handlers[TRequest.Command] = handlers;
        }
        handlers.Add(commandHandler);

        _codec.Register<TRequest>();
    }

    private void MapRoute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResponse>(
        Func<Session, TRequest, CancellationToken, Task<TResponse>> handler
    )
        where TRequest  : class, IMessage
        where TResponse : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(TRequest.Command);
        ArgumentNullException.ThrowIfNull(handler);

        ValidateRequestType(TRequest.Command, typeof(TRequest));

        var commandHandler = new CommandHandler(TRequest.Command, typeof(TRequest), TResponse.Command, typeof(TResponse),
            async (session, request, cancellationToken) =>
                await handler(session, (TRequest)request!, cancellationToken).ConfigureAwait(false));

        if (!_handlers.TryGetValue(TRequest.Command, out var handlers))
        {
            handlers = new List<CommandHandler>();
            _handlers[TRequest.Command] = handlers;
        }
        handlers.Add(commandHandler);

        _codec.Register<TRequest>();
        _codec.Register<TResponse>();
    }

    [RequiresUnreferencedCode("Controller registration require reflection to scan available handlers")]
    private void MapRoute<TSession, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TController>(
        Func<TSession, TController> factory, params IEnumerable<ICommandFilter> filters
    )
        where TSession    : Session
        where TController : CommandController
    {
        var type = typeof(TController);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in methods)
        {
            HashSet<Enum?> commandSet = [];
            foreach (var attribute in method.GetCustomAttributes<CommandHandlerAttribute>())
            {
                if (!commandSet.Add(attribute.RequestCommand))
                {
                    if (attribute.RequestCommand != null)
                    {
                        throw new InvalidOperationException(
                            $"CommandHandler for '{attribute.RequestCommand}' is already defined");
                    }

                    throw new InvalidOperationException(
                        $"Default CommandHandler is already defined");
                }

                // ReSharper disable once PossibleMultipleEnumeration
                var descriptor = new CommandHandlerDescriptor(method, attribute, filters);

                if (descriptor.RequestType != null)
                {
                    _codec.Register(descriptor.RequestType);
                    _codec.Register(descriptor.RequestCommand, descriptor.RequestType);
                }
                else
                    _codec.Register(descriptor.RequestCommand);

                if (descriptor.ResponseType != null)
                {
                    _codec.Register(descriptor.ResponseType);
                    if (descriptor.ResponseCommand != null)
                        _codec.Register(descriptor.ResponseCommand, descriptor.ResponseType);
                }
                else if (descriptor.ResponseCommand != null)
                    _codec.Register(descriptor.ResponseCommand);

                var commandHandler = new CommandHandler(descriptor, async (session, request, cancellationToken) =>
                {
                    try
                    {
                        var controller = factory((TSession)session);

                        dynamic response;
                        if (descriptor is { RequestType: not null, IsCancelable: true })
                            response = method.Invoke(controller, [request, cancellationToken])!;
                        else if (descriptor is { RequestType: not null, IsCancelable: false })
                            response = method.Invoke(controller, [request])!;
                        else if (descriptor is { RequestType: null, IsCancelable: true })
                            response = method.Invoke(controller, [cancellationToken])!;
                        else
                            response = method.Invoke(controller, [])!;

                        if (response is Task { Exception: not null } task)
                            ExceptionDispatchInfo.Capture(task.Exception.InnerException ?? task.Exception).Throw();

                        if (descriptor is { IsAsync: true, ResponseType: not null })
                        {
                            dynamic result = await response;
                            if (result is IMessage message && !message.GetType().IsAssignableTo(descriptor.ResponseType))
                            {
                                throw new InvalidOperationException("Response type mismatch " +
                                                                    $"(Expected: '{descriptor.ResponseType}' /" +
                                                                    $"Got: '{message.GetType()}')");
                            }

                            return result;
                        }
                        else if (descriptor.ResponseType != null && response is IMessage message)
                        {
                            if (!message.GetType().IsAssignableTo(descriptor.ResponseType))
                            {
                                throw new InvalidOperationException("Response type mismatch " +
                                                                    $"(Expected: '{descriptor.ResponseType}' /" +
                                                                    $"Got: '{message.GetType()}')");
                            }

                            return message;
                        }

                        return null;
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException != null)
                        {
                            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                            return null;
                        }

                        throw;
                    }
                });

                if (!_handlers.TryGetValue(descriptor.RequestCommand, out var handlers))
                {
                    handlers = [];
                    _handlers[descriptor.RequestCommand] = handlers;
                }
                handlers.Add(commandHandler);
            }
        }
    }

    private async Task<CommandResponse> Execute(CommandHandler handler, Session session, Enum command,
        IMessage? request, CancellationToken cancellationToken)
    {
        var context = new CommandExecutionContext(session, command, request, handler.Descriptor);

        var response = CommandResponse.Empty;
        var frames = new List<ResponseFrame>();

        bool cancel = false;
        foreach (var filter in _filters.Concat(handler.Filters))
        {
            try
            {
                var executingContext = new CommandExecutingContext(context);
                await filter.OnActionExecutingAsync(executingContext, cancellationToken).ConfigureAwait(false);

                if (executingContext.Cancel)
                {
                    if (executingContext.Result != null)
                    {
                        if (_codec.GetRegisteredType(executingContext.Result.GetType()) == null)
                            _codec.Register(executingContext.Result.GetType());

                        frames.Add(new ResponseFrame(request, _codec.Encode(executingContext.Result)));
                    }

                    cancel = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                return await FilterException(
                    new CommandExceptionContext(ex, session, handler.Descriptor, request),
                    cancellationToken
                ).ConfigureAwait(false);
            }
        }

        if (cancel)
            return new CommandResponse(frames);

        Exception? exception = null;
        
        try
        {
            object? message = await handler.Execute(session, request, cancellationToken).ConfigureAwait(false);
            response = message is IMessage msg ? CommandResponse.Single(request, _codec.Encode(msg)) : response;
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var executedContext = new CommandExecutedContext(context, exception);
        foreach (var filter in handler.Filters)
        {
            try
            {
                await filter.OnActionExecutedAsync(executedContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var mainException = ex;
                if (exception != null)
                    mainException = new AggregateException(ex, exception);

                return await FilterException(
                    new CommandExceptionContext(mainException, session, handler.Descriptor, request),
                    cancellationToken
                ).ConfigureAwait(false);
            }
        }

        if (executedContext is { Exception: not null, ExceptionHandled: false })
        {
            return await FilterException(
                new CommandExceptionContext(executedContext.Exception, session, handler.Descriptor, request),
                cancellationToken
            ).ConfigureAwait(false);
        }

        if (executedContext.Result != null)
        {
            if (_codec.GetRegisteredType(executedContext.Result.GetType()) == null)
                _codec.Register(executedContext.Result.GetType());

            return CommandResponse.Single(request, _codec.Encode(executedContext.Result));
        }

        return response;
    }

    private async Task<CommandResponse> FilterException(CommandExceptionContext context, CancellationToken cancellationToken)
    {
        bool propagate = true;
        foreach (var logger in _exceptionLogger)
        {
            var ctx = new CommandExceptionLoggerContext(context);
            await logger.LogAsync(ctx, cancellationToken).ConfigureAwait(false);

            propagate = propagate && ctx.PropagateException;
        }

        if (!propagate)
            return CommandResponse.Empty;

        if (_exceptionHandler != null)
        {
            var ctx = new CommandExceptionHandlerContext(context);
            await _exceptionHandler.HandleAsync(ctx, cancellationToken).ConfigureAwait(false);
            if (ctx.Result != null)
            {
                if (_codec.GetRegisteredType(ctx.Result.GetType()) == null)
                    _codec.Register(ctx.Result.GetType());

                return CommandResponse.Single(context.Request, _codec.Encode(ctx.Result));
            }

            if (!ctx.Handled)
                ExceptionDispatchInfo.Capture(context.Exception).Throw();
        }

        return CommandResponse.Empty;
    }

    public async Task Dispatch(Session session, byte[] payload, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        IMessage? request;
        Enum? command = null;
        List<CommandHandler>? handlers = null;

        try
        {
            request = _codec.Decode(payload);
        }
        catch (Exception ex)
        {
            var context = new CommandExceptionContext(ex, session, null);
            var response = await FilterException(context, cancellationToken)
                .ConfigureAwait(false);

            await WriteFrame(session, response, cancellationToken);
            return;
        }

        try
        {
            command = _codec.DecodeCommand(payload);
            if ((request != null && !_handlers.TryGetValue(command, out handlers)) || !_handlers.TryGetValue(command, out handlers))
                throw new NotSupportedException($"Command '0x{command:X4}' is not recognized");
        }
        catch (Exception ex)
        {
            var context = new CommandExceptionContext(ex, session, null, request);
            var response = await FilterException(context, cancellationToken)
                .ConfigureAwait(false);

            await WriteFrame(session, response, cancellationToken);
            return;
        }

        var tasks = new ConcurrentBag<Task>();
        foreach (var handler in handlers)
        {
            try
            {
                var response = await Execute(handler, session, command, request, cancellationToken);
                if (!response.IsEmpty)
                    tasks.Add(WriteFrame(session, response, cancellationToken));
            }
            catch (Exception ex)
            {
                var context = new CommandExceptionContext(ex, session, handler.Descriptor, request);
                var response = await FilterException(context, cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsEmpty)
                    tasks.Add(WriteFrame(session, response, cancellationToken));
            }
        }

        await Task.WhenAll(tasks);
    }

    private static async Task WriteFrame(Session session, CommandResponse response, CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            response.Frames.Select(frame => session.WriteFrame(frame.Payload, cancellationToken))
        );
    }
}