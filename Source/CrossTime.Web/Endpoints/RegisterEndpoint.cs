using System.Net;
using System.Text;
using Microsoft.Extensions.Options;

using Mozart.Data.Contexts;
using Mozart.Data.Entities;
using Mozart.Metadata;
using Mozart.Metadata.Items;
using Mozart.Options;
using Mozart.Services;

namespace CrossTime.Web;

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
        MainDbContext context,
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

            string token;
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            {
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
                await context.SaveChangesAsync(cancellationToken);

                var rawPassword = Encoding.UTF8.GetBytes(request.Password);
                var credential = new Credential
                {
                    Username = request.Username,
                    Password = auth.Value.Mode == AuthMode.Default ? PasswordHasher.Hash(rawPassword) : rawPassword
                };

                user.Equipments[ItemType.Face] = (short)(request.Gender == Gender.Female ? 36 : 35);
                await context.AddAsync(credential, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                token = await identityService.Authenticate(new UsernamePasswordCredentialRequest()
                {
                    Address  = http.Connection.RemoteIpAddress ?? IPAddress.Any,
                    Username = request.Username,
                    Password = Encoding.UTF8.GetBytes(request.Password)
                }, cancellationToken);

            }
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("User '{User}' registered successfully.", request.Username);
            return Results.Text(token);
        }
    }
}
