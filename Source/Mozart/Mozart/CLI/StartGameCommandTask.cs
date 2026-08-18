using System.CommandLine;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using Encore.CLI;
using Encore.Data.Entities;
using Encore.Data.Repositories;
using Encore.Server;
using Encore.Services;
using Microsoft.Extensions.Options;
using Mozart.Options;

namespace Mozart.CLI;

public class StartGameCommandTask(
    IAuthService authService,
    IUserRepository userRepository,
    IOptions<ServerOptions> serverOptions,
    IOptions<TcpOptions> tcpOptions
) : ICommandLineTask
{
    private static readonly Version EGamesVersion  = new(3, 10);
    private static readonly Version GamaniaVersion = new(2, 93);

    private const int GamaniaMaxArgLength = 32;

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
        var versionOption = new Option<string?>("--client-version")
        {
            Description = "The client version to launch (3.10 or 2.93). Detected from VersionInfo.dat when omitted"
        };

        command.Arguments.Add(usernameArgument);
        command.Arguments.Add(passwordArgument);
        command.Arguments.Add(directoryArgument);
        command.Options.Add(versionOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            string username = parseResult.GetRequiredValue(usernameArgument);
            string password = parseResult.GetRequiredValue(passwordArgument);
            string directory = parseResult.GetRequiredValue(directoryArgument);
            string? version = parseResult.GetValue(versionOption);
            Environment.ExitCode = await ExecuteAsync(username, password, directory, version, cancellationToken);
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use the overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, string password, string directory, string? clientVersion,
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

            Version version;
            if (clientVersion == null)
            {
                version = DetectClientVersion(gameDirectory);
            }
            else if (!Version.TryParse(clientVersion, out version!) || (version != EGamesVersion && version != GamaniaVersion))
            {
                Console.WriteLine($"Unsupported client version: {clientVersion}");
                return 1;
            }

            string token = await authService.Authenticate(new UsernamePasswordCredentialRequest
            {
                Username = username,
                Password = Encoding.UTF8.GetBytes(password),
                Address  = IPAddress.Any
            }, cancellationToken);

            var user = (await userRepository.FindByUsername(username, cancellationToken))!;

            string address = tcpOptions.Value.Address;
            string port = tcpOptions.Value.Port.ToString();
            int gatewayCount = 5;
            var arguments = new List<string>();
            if (version == GamaniaVersion)
            {
                gatewayCount  = 3;
                string first  = user.Username;
                string second = token;

                if (token.Length >= GamaniaMaxArgLength)
                {
                    first  = token[..Math.Min(GamaniaMaxArgLength - 1, token.Length)];
                    second = token[first.Length..];
                }

                arguments.AddRange([first, second, "0", "127.0.0.1", "o2jam"]);
            }
            else
            {
                string launchToken = Convert.ToBase64String(Encoding.BigEndianUnicode.GetBytes(token));
                arguments.AddRange([launchToken, "127.0.0.1:21", "O2Jam"]);
            }

            arguments.Add(gatewayCount.ToString());
            for (int i = 0; i < gatewayCount; i++)
            {
                arguments.Add(address);
                arguments.Add(port);
            }

            Launch(executablePath, gameDirectory, arguments.ToArray());

            Console.WriteLine($"Started O2Jam v{version} for: {username}");
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or
                                   InvalidOperationException or Win32Exception)
        {
            Console.WriteLine($"Unable to start O2Jam: {ex.Message}");
            return 1;
        }
    }

    private static Version DetectClientVersion(string gameDirectory)
    {
        string path = Path.Combine(gameDirectory, "VersionInfo.dat");
        if (!File.Exists(path))
            return EGamesVersion;

        // [Version]
        // 1.06 1.22 2.93
        string[] tokens = File.ReadAllText(path).Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length >= 4 && Version.TryParse(tokens[3], out var version) && version == GamaniaVersion
            ? GamaniaVersion : EGamesVersion;
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
