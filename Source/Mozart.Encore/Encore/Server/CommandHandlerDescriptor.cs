using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using Encore.Messaging;

namespace Encore.Server;

public sealed class CommandHandlerDescriptor
{
    private readonly MethodInfo? _method;
    private readonly IReadOnlyList<ICommandFilter> _filters = [];

    public CommandHandlerDescriptor(
        Enum requestCommand,
        Type? requestType = null,
        Enum? responseCommand = null,
        Type? responseType = null
    )
    {
        Name            = string.Empty;
        RequestCommand  = requestCommand;
        RequestType     = requestType;
        ResponseCommand = responseCommand;
        ResponseType    = responseType;
    }

    [RequiresUnreferencedCode("Scan for method signature to determine request and response type")]
    public CommandHandlerDescriptor(MethodInfo method, CommandHandlerAttribute? attribute = null)
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        ArgumentOutOfRangeException.ThrowIfEqual(method.IsStatic, true, nameof(method));
        ArgumentOutOfRangeException.ThrowIfEqual(method.ContainsGenericParameters, true, nameof(method));

        if (!typeof(CommandController).IsAssignableFrom(method.ReflectedType) ||
            !method.GetCustomAttributes<CommandHandlerAttribute>().Any() ||
            method.GetParameters().Any(p => p.IsOut || p.ParameterType.IsByRef))
        {
            throw new ArgumentOutOfRangeException(nameof(method));
        }

        Name            = $"{method.ReflectedType.Name}:{method.Name}";
        RequestType     = GetRequestType(method);
        RequestCommand  = GetRequestCommand(method, RequestType, attribute);
        ResponseType    = GetResponseType(method);
        ResponseCommand = GetResponseCommand(method, ResponseType, attribute);
        IsAsync         = IsMethodAsync(method);
        IsCancelable    = IsMethodCancellable(method);

        if (!IsAsync && IsCancelable)
            throw new ArgumentOutOfRangeException(nameof(method));

        var filters = method.ReflectedType
            .GetCustomAttributes<CommandFilterAttribute>()
            .OrderBy(attribute => attribute.Order)
            .ToList();

        filters = method
            .GetCustomAttributes<CommandFilterAttribute>()
            .OrderBy(attribute => attribute.Order)
            .Concat(filters)
            .ToList();

        _method  = method;
        _filters = filters;
    }

    public string? Name { get; }

    public Enum RequestCommand { get; }

    public Enum? ResponseCommand { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [field: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? RequestType { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [field: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? ResponseType { get; }

    public bool IsAsync { get; }

    public bool IsCancelable { get; }

    private static Enum GetRequestCommand(MethodInfo method, Type? requestType, CommandHandlerAttribute? attribute)
    {
        attribute ??= method.GetCustomAttribute<CommandHandlerAttribute>();
        if (attribute is { RequestCommand: not null })
            return attribute.RequestCommand;

        return GetCommandFrom(requestType);
    }

    private static Enum? GetResponseCommand(MethodInfo method, Type? responseType, CommandHandlerAttribute? attribute)
    {
        attribute ??= method.GetCustomAttribute<CommandHandlerAttribute>();
        if (attribute is { ResponseCommand: not null })
            return attribute.ResponseCommand;

        if (responseType != null)
            return GetCommandFrom(responseType);

        return null;
    }

    private static Enum GetCommandFrom(Type? type)
    {
        if (!typeof(IMessage).IsAssignableFrom(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        var property = type.GetProperty("Command",
            BindingFlags.Public | BindingFlags.Static);

        if (property == null || !property.CanRead)
            throw new ArgumentOutOfRangeException(nameof(type));

        return property.GetValue(null) as Enum ??
               throw new ArgumentOutOfRangeException(nameof(type));
    }

    [RequiresUnreferencedCode(
        "Scan the signature of the given method to validate parameters and determine request type")]
    private static Type? GetRequestType(MethodInfo method)
    {
        var parameters = method.GetParameters();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(parameters.Length, 2, nameof(method));

        if (parameters.Length >= 1 && typeof(IMessage).IsAssignableFrom(parameters.First().ParameterType))
            return parameters.First().ParameterType;

        if (parameters.Length == 1 && !typeof(CancellationToken).IsAssignableFrom(parameters.First().ParameterType))
            throw new ArgumentOutOfRangeException(nameof(method));

        if (parameters.Length == 2 && !typeof(CancellationToken).IsAssignableFrom(parameters.Last().ParameterType))
            throw new ArgumentOutOfRangeException(nameof(method));

        return null;
    }

    [RequiresUnreferencedCode(
        "Inspect the return type of the given method to validate and determine response type")]
    private static Type? GetResponseType(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(void) || returnType == typeof(Task))
            return null;

        if (returnType.GenericTypeArguments.Length == 1 && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            returnType = returnType.GenericTypeArguments.First();

        if (!typeof(IMessage).IsAssignableFrom(returnType))
            throw new ArgumentOutOfRangeException(nameof(method));

        return returnType;
    }

    [RequiresUnreferencedCode("Inspect the return type of the given method ")]
    private static bool IsMethodAsync(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(Task) || returnType == typeof(void))
            return true;

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            return true;

        if (!typeof(IMessage).IsAssignableFrom(returnType))
            throw new ArgumentOutOfRangeException(nameof(method));

        return false;
    }

    [RequiresUnreferencedCode("Inspect the return type of the given method")]
    private static bool IsMethodCancellable(MethodInfo method)
    {
        var parameters = method.GetParameters();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(parameters.Length, 2, nameof(method));

        if (parameters.Length == 2)
        {
            if (!typeof(CancellationToken).IsAssignableFrom(parameters.Last().ParameterType))
                throw new ArgumentOutOfRangeException(nameof(method));

            return true;
        }

        if (parameters.Length == 1 && typeof(CancellationToken).IsAssignableFrom(parameters.Last().ParameterType))
            return true;

        return false;
    }

    public object[] GetCustomAttributes(bool inherit = true)
    {
        return _method?.GetCustomAttributes(inherit) ?? [];
    }

    public T? GetCustomAttribute<T>(bool inherit = true)
        where T : Attribute
    {
        return _method?.GetCustomAttribute<T>(inherit) ?? null;
    }

    public bool HasCustomAttribute<T>(bool inherit = true)
        where T : Attribute
    {
        return _method?.GetCustomAttribute<T>(inherit) != null;
    }

    public IEnumerable<ICommandFilter> GetCommandFilters()
    {
        return _filters;
    }

    public T? GetCommandFilter<T>()
        where T : class, ICommandFilter
    {
        return (T?)_filters.FirstOrDefault(f => f.GetType() == typeof(T));
    }

    public bool HasCommandFilter<T>()
        where T : class, ICommandFilter
    {
        return GetCommandFilter<T>() != null;
    }
}