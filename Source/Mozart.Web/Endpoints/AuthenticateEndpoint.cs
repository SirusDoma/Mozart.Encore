using System.Net;
using System.Text;

using Microsoft.Extensions.Options;

using Mozart.Data.Entities;
using Mozart.Options;
using Mozart.Services;

namespace Mozart.Web;

public static class AuthenticateEndpoint
{
    public sealed record AuthenticationRequest(
        string Username,
        string Password
    );

    public static async Task<IResult> Post(
        HttpContext context,
        IIdentityService identityService,
        IOptions<AuthOptions> auth,
        AuthenticationRequest request,
        ILogger<WebServer> logger,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Authenticate"))
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                logger.LogWarning(
                    "Missing parameters for user authenticate: ({Username}, {Password})",
                    request.Username,
                    string.IsNullOrEmpty(request.Password)
                );
                return Results.BadRequest(new {
                    success = false,
                    error = "missing parameters",
                });
            }

            try
            {
                string authToken = await identityService.Authenticate(new UsernamePasswordCredentialRequest()
                {
                    Address  = context.Connection.RemoteIpAddress ?? IPAddress.Any,
                    Username = request.Username,
                    Password = Encoding.UTF8.GetBytes(request.Password)
                }, cancellationToken);

                return Results.Text(authToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to authenticate user");
                return Results.UnprocessableEntity(new
                {
                    success = false,
                    error = "failed"
                });
            }
        }
    }
}
