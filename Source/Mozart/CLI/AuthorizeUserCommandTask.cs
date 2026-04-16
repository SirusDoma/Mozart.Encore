using System.CommandLine;
using System.Net;
using System.Text;
using Encore.Server;
using Microsoft.Extensions.Options;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;
using Mozart.Metadata;
using Mozart.Options;
using Mozart.Services;

namespace Mozart.CLI;

public class AuthorizeUserCommandTask(
    IAuthService authService,
    IUserRepository userRepository,
    IOptions<ServerOptions> serverOptions,
    IOptions<TcpOptions> tcpOptions,
    IOptions<GatewayOptions> gatewayOptions
) : ICommandLineTask
{
    public static string Name => "user:authorize";
    public static string Description => "Authorize the user for a game session";

    public void ConfigureCommand(Command command)
    {
        var usernameArgument = new Argument<string>("username") { Description = "The username of the user" };
        var passwordArgument = new Argument<string>("password") { Description = "The password of the user" };

        command.Arguments.Add(usernameArgument);
        command.Arguments.Add(passwordArgument);

        command.SetAction(async (parseResult, _) =>
        {
            string username = parseResult.GetRequiredValue(usernameArgument);
            string password = parseResult.GetRequiredValue(passwordArgument);

            int exitCode = await ExecuteAsync(username, password, cancellationToken);
            Environment.ExitCode = exitCode;
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use the overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, string password, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Generating auth token for: [{username}]..");

        try
        {
            string token = await authService.Authenticate(new UsernamePasswordCredentialRequest
            {
                Username = username,
                Password = Encoding.UTF8.GetBytes(password),
                Address  = IPAddress.Any
            }, cancellationToken);

            var user = await userRepository.FindByUsername(username, CancellationToken.None);
            if (user == null)
            {
                Console.WriteLine("Error: User not found");
                return -1;
            }

            bool isChannel     = serverOptions.Value.Mode == DeploymentMode.Channel;
            string gatewayIp   = isChannel ? gatewayOptions.Value.Address : tcpOptions.Value.Address;
            int    gatewayPort = isChannel ? gatewayOptions.Value.Port    : tcpOptions.Value.Port;

            var authParams = new AuthParameters
            {
                UserIndexId    = user.Id.ToString(),
                Username       = token, // Originally use username
                Level          = user.Level.ToString(),
                Gender         = (user.Gender == Gender.Male ? 1 : 2).ToString(),
                Token          = user.Id.ToString(), // Intended
                Rank           = (user.FreePass.Type != FreePassType.None ? user.Ranking > 0 ? -user.Ranking : -999999 : user.Ranking).ToString(),
                GatewayAddress = gatewayIp,
                GatewayPort    = gatewayPort.ToString(),
            };

            Console.WriteLine();
            Console.WriteLine($"  Token:        {token}");
            Console.WriteLine($"  Launch token: {authParams.Encode()}");
            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
    }
}
