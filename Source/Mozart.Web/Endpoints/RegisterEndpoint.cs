using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Mozart.Data.Contexts;
using Mozart.Data.Entities;
using Mozart.Metadata;
using Mozart.Metadata.Items;
using Mozart.Options;
using Mozart.Services;

namespace Mozart.Web;

public static class RegisterEndpoint
{
    public sealed record RegisterRequest(
        string Username,
        string Password,
        Gender Gender = Gender.Male
    );

    public static async Task<IResult> Post(
        IIdentityService identityService,
        HttpContext http,
        UserDbContext context,
        IOptions<AuthOptions> auth,
        RegisterRequest request,
        ILogger<WebServer> logger,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Register"))
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                logger.LogWarning(
                    "Missing parameters for user registration: ({Username}, {Password})",
                    request.Username,
                    string.IsNullOrEmpty(request.Password)
                );
                return Results.BadRequest(new {
                    success = false,
                    error = "missing parameters",
                });
            }

            logger.LogInformation("Registering user '{User}'...", request.Username);

            var user = new User
            {
                Id              = 0,
                Username        = request.Username,
                Nickname        = request.Username,
                Gender          = request.Gender,
                IsAdministrator = false
            };
            await context.AddAsync(user, cancellationToken);

            user.Equipments[ItemType.Face] = (short)(request.Gender == Gender.Female ? 36 : 35);

            var rawPassword = Encoding.UTF8.GetBytes(request.Password);
            var credential = new Credential
            {
                UserId = user.Id,
                Username = request.Username,
                Password = auth.Value.Mode == AuthMode.Default ? PasswordHasher.Hash(rawPassword) : rawPassword
            };

            await context.AddAsync(credential, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            string token = await identityService.Authenticate(new UsernamePasswordCredentialRequest()
            {
                Address  = http.Connection.RemoteIpAddress ?? IPAddress.Any,
                Username = request.Username,
                Password = Encoding.UTF8.GetBytes(request.Password)
            }, cancellationToken);
            
            logger.LogInformation("User '{User}' registered successfully.", request.Username);
            return Results.Text(token);
        }
    }
}
