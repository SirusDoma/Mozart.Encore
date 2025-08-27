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
        UserDbContext context,
        IOptions<AuthOptions> auth,
        RegisterRequest body,
        ILogger<WebServer> logger)
    {
        using (logger.BeginScope("Register"))
        {
            if (string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
            {
                logger.LogWarning(
                    "Missing parameters for user registration: ({Username}, {Password})",
                    body.Username,
                    string.IsNullOrEmpty(body.Password)
                );
                return Results.BadRequest(new {
                    success = false,
                    error = "missing parameters",
                });
            }

            logger.LogInformation("Registering user '{User}'...", body.Username);

            var user = User.NewUser(body.Username, body.Gender);
            await context.AddAsync(user);

            user.Equipments[ItemType.Face] = (short)(body.Gender == Gender.Female ? 36 : 35);

            var rawPassword = System.Text.Encoding.UTF8.GetBytes(body.Password);
            var credential = new Credential
            {
                UserId = user.Id,
                Username = body.Username,
                Password = auth.Value.Mode == AuthMode.Default ? PasswordHasher.Hash(rawPassword) : rawPassword
            };

            await context.AddAsync(credential);
            await context.SaveChangesAsync();
            
            logger.LogInformation("User '{User}' registered successfully.", body.Username);

            return Results.Ok(new { success = true });
        }
    }
}
