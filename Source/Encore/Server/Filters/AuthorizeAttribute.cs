using System.Diagnostics.CodeAnalysis;

namespace Encore.Server;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : CommandFilterAttribute
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private readonly Type? _exceptionType;

    public AuthorizeAttribute()
    {
    }

    protected AuthorizeAttribute(Type? exceptionType)
    {
        _exceptionType = exceptionType;
    }

    public override void OnActionExecuting(CommandExecutingContext context)
    {
        if (context.Descriptor.HasCustomAttribute<AllowAnonymousAttribute>())
            return;

        if (!context.Session.Authorized)
        {
            if (_exceptionType == null)
                throw new InvalidOperationException("Unauthorized access");

            throw (Exception)Activator.CreateInstance(_exceptionType)!;
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute<TException> : AuthorizeAttribute
    where TException : Exception
{
    public AuthorizeAttribute() :
        base(typeof(TException))
    {
    }
}
