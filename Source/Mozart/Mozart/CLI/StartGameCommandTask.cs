using System.CommandLine;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using Encore.CLI;
using Encore.Data.Entities;
using Encore.Server;
using Encore.Services;
using Microsoft.Extensions.Options;
using Mozart.Options;

namespace Mozart.CLI;

public class StartGameCommandTask(
    IAuthService authService,
    IOptions<ServerOptions> serverOptions,
    IOptions<TcpOptions> tcpOptions
) : ICommandLineTask
{
    private const int GatewayMirrorCount = 5;

    public static string Name => "game:start";
    public static string Description => "Authorize a user and launch O2Jam";

    public void ConfigureCommand(Command command)
    {
        var usernameArgument = new Argument<string>("username") { Description = "The username of the user" };
        var passwordArgument = new Argument<string>("password") { Description = "The password of the user" };
        var directoryArgument = new Argument<string>("dir")
        {
            Description = "The supported O2Jam installation directory",
            DefaultValueFactory = _ => Environment.CurrentDirectory
        };

        command.Arguments.Add(usernameArgument);
        command.Arguments.Add(passwordArgument);
        command.Arguments.Add(directoryArgument);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            string username = parseResult.GetRequiredValue(usernameArgument);
            string password = parseResult.GetRequiredValue(passwordArgument);
            string directory = parseResult.GetRequiredValue(directoryArgument);
            Environment.ExitCode = await ExecuteAsync(username, password, directory, cancellationToken);
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use the overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, string password, string directory,
        CancellationToken cancellationToken)
    {
        try
        {
            if (serverOptions.Value.Mode is not DeploymentMode.Full and not DeploymentMode.Gateway)
            {
                Console.WriteLine("game:start is only available in Full or Gateway mode.");
                return 1;
            }

            string gameDirectory = CommandLinePath.GetFullPath(directory);
            string? executablePath = FindGameExecutable(gameDirectory);
            if (executablePath == null)
            {
                Console.WriteLine($"OTwo.exe was not found in: {gameDirectory}");
                return 1;
            }

            string token = await authService.Authenticate(new UsernamePasswordCredentialRequest
            {
                Username = username,
                Password = Encoding.UTF8.GetBytes(password),
                Address  = IPAddress.Any
            }, cancellationToken);

            string address = tcpOptions.Value.Address;
            string port = tcpOptions.Value.Port.ToString();
            string launchToken = Convert.ToBase64String(Encoding.BigEndianUnicode.GetBytes(token));
            var arguments = new List<string>
            {
                launchToken, "127.0.0.1:21", "O2Jam", GatewayMirrorCount.ToString()
            };
            for (int i = 0; i < GatewayMirrorCount; i++)
            {
                arguments.Add(address);
                arguments.Add(port);
            }

            Launch(executablePath, gameDirectory, arguments.ToArray());

            Console.WriteLine($"Started O2Jam for: {username}");
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or
                                   InvalidOperationException or Win32Exception)
        {
            Console.WriteLine($"Unable to start O2Jam: {ex.Message}");
            return 1;
        }
    }

    private static void Launch(string executablePath, string workingDirectory, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false
        };
        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using Process? process = Process.Start(startInfo);
        if (process == null)
            throw new InvalidOperationException("The game process could not be created.");
    }

    private static string? FindGameExecutable(string directory)
    {
        if (!Directory.Exists(directory))
            return null;

        return Directory.EnumerateFiles(directory)
            .FirstOrDefault(path => Path.GetFileName(path).Equals("OTwo.exe", StringComparison.OrdinalIgnoreCase));
    }
}
