using System.CommandLine;
using System.Net;
using System.Text;
using Amadeus.Messages.Requests;
using Encore.CLI;
using Encore.Data.Entities;
using Encore.Data.Repositories;
using Encore.Services;

namespace Amadeus.CLI;


public class AuthorizeUserCommandTask(IAuthService authService, IUserRepository userRepository) : ICommandLineTask
{
    public static string Name => "user:authorize";
    public static string Description => "Authorize the user for a game session";

    public void ConfigureCommand(Command command)
    {
        // Add required arguments
        var usernameArgument = new Argument<string>("username") { Description = "The username of the user" };
        var passwordArgument = new Argument<string>("password") { Description = "The password of the user" };

        command.Arguments.Add(usernameArgument);
        command.Arguments.Add(passwordArgument);

        // Set the handler with all parameters
        command.SetAction(async (parseResult, _) =>
        {
            string username = parseResult.GetRequiredValue(usernameArgument);
            string password = parseResult.GetRequiredValue(passwordArgument);

            int exitCode = await ExecuteAsync(username, password);
            Environment.ExitCode = exitCode;
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        // This won't be called since we override the handler in ConfigureCommand
        throw new NotSupportedException("Use the overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, string password)
    {
        Console.WriteLine($"Generating auth token for: [{username}]..");

        try
        {
            string token = await authService.Authenticate(new UsernamePasswordCredentialRequest()
            {
                Username = username,
                Password = Encoding.UTF8.GetBytes(password),
                Address  = IPAddress.Any
            }, CancellationToken.None);

            var user = (await userRepository.FindByUsername(username, CancellationToken.None))!;
            int split = Math.Min(AuthRequest.GamaniaCredential.TokenSplitLength, token.Length);

            Console.WriteLine();
            Console.WriteLine($"  Token: {token}");
            Console.WriteLine($"  Launch Token (e-Games): {Convert.ToBase64String(Encoding.BigEndianUnicode.GetBytes(token))}");
            Console.WriteLine($"  Launch Arguments (GAMANIA): {token[..split]} {user.Id} {token[split..]}");
            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
    }
}
