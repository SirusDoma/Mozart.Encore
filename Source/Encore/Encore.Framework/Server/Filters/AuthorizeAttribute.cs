using System.Diagnostics.CodeAnalysis;
using Encore.Sessions;

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

        if (context.Session is not ITcpSession tcp)
        {
            throw new InvalidOperationException(
                "[Authorize] is not valid on a non-TCP session. Encode identity in the payload instead.");
        }

        if (!tcp.Authorized)
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
