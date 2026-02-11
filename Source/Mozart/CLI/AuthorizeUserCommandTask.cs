using System.CommandLine;
using System.Net;
using System.Text;
using Mozart.Data.Entities;
using Mozart.Services;

namespace Mozart.CLI;


public class AuthorizeUserCommandTask(IIdentityService identityService) : ICommandLineTask
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
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            string username = parseResult.GetRequiredValue(usernameArgument);
            string password = parseResult.GetRequiredValue(passwordArgument);

            int exitCode = await ExecuteAsync(username, password, cancellationToken);
            Environment.ExitCode = exitCode;
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        // This won't be called since we override the handler in ConfigureCommand
        throw new NotSupportedException("Use the overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, string password, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Generating auth token for: [{username}]..");

        try
        {
            string token = await identityService.Authenticate(new UsernamePasswordCredentialRequest
            {
                Username = username,
                Password = Encoding.UTF8.GetBytes(password),
                Address  = IPAddress.Any
            }, cancellationToken);

            Console.WriteLine();
            Console.WriteLine($"  Token: {token}");
            Console.WriteLine($"  Launch Token: {Convert.ToBase64String(Encoding.BigEndianUnicode.GetBytes(token))}");
            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
    }
}
