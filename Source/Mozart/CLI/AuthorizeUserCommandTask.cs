using System.CommandLine;
using System.Net;
using System.Text;
using Mozart.Persistence.Entities;
using Mozart.Services;

namespace Mozart.CLI;


public class AuthorizeUserCommandTask(IIdentityService identityService) : ICommandLineTask
{
    public static string Name => "user:authorize";
    public static string Description => "Authorize the user for a game session";

    public void ConfigureCommand(Command command)
    {
        // Add required arguments
        var usernameArgument = new Argument<string>("username", "The username of the user");
        var passwordArgument = new Argument<string>("password", "The password of the user");

        command.AddArgument(usernameArgument);
        command.AddArgument(passwordArgument);

        // Set the handler with all parameters
        command.SetHandler(async (username, password) =>
        {
            int exitCode = await ExecuteAsync(username, password);
            Environment.ExitCode = exitCode;
        }, usernameArgument, passwordArgument);
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // This won't be called since we override the handler in ConfigureCommand
        throw new NotSupportedException("Use the overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, string password)
    {
        Console.WriteLine($"Generating auth token for: [{username}]..");

        try
        {
            string token = await identityService.Authenticate(new UsernamePasswordCredentialRequest()
            {
                Username = username,
                Password = Encoding.UTF8.GetBytes(password),
                Address  = IPAddress.Any
            }, CancellationToken.None);

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