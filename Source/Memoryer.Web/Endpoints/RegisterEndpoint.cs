using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Mozart.Data.Contexts;
using Mozart.Data.Entities;
using Mozart.Metadata;
using Mozart.Metadata.Items;
using Mozart.Options;
using Mozart.Services;

namespace Memoryer.Web;

public static class RegisterEndpoint
{
    public sealed record RegisterRequest(
        string Username,
        string Password,
        Gender Gender = Gender.Male
    );

    public static async Task<IResult> Post(
        IAuthService authService,
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

            logger.LogInformation("Registering user '{User}'...", request.Username);

            bool usernameTaken = await context.Members
                .AnyAsync(m => EF.Functions.Like(m.Username, request.Username), cancellationToken);
            if (usernameTaken)
            {
                logger.LogWarning("Registration rejected: username '{User}' is already taken.", request.Username);
                return Results.Conflict(new {
                    success = false,
                    error = "username already taken",
                });
            }

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
            var member = new Member
            {
                Username = request.Username,
                Password = auth.Value.Mode == AuthMode.Default ? PasswordHasher.Hash(rawPassword) : rawPassword
            };

            await context.AddAsync(member, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            string token = await authService.Authenticate(new UsernamePasswordCredentialRequest()
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
