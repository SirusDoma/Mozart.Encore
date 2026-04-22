using System.CommandLine;
using Memoryer;

namespace Memoryer.CLI;

public class EncDecCommandTask : ICommandLineTask
{
    public static string Name => "encdec";
    public static string Description => "EncDec";

    public void ConfigureCommand(Command command)
    {
        var paramArgument = new Argument<string>("param") { Description = "The param to encrypt / decrypt" };

        command.Arguments.Add(paramArgument);

        // Add optional options
        var encryptOption = new Option<bool>("--encrypt")
        {
            DefaultValueFactory = _ => true,
            Description = "Specify whether it is decrypt operation"
        };
        var decryptOption  = new Option<bool>("--decrypt")
        {
            DefaultValueFactory = _  => false,
            Description = "Specify whether it is decrypt operation"
        };

        command.Options.Add(encryptOption);
        command.Options.Add(decryptOption);

        // Set the handler with all parameters
        command.SetAction(async (parsedResult, cancellationToken) =>
        {
            string username = parsedResult.GetRequiredValue(paramArgument);
            bool isEncrypt  = parsedResult.GetRequiredValue(encryptOption);
            bool isDecrypt  = parsedResult.GetRequiredValue(decryptOption);

            int exitCode = await ExecuteAsync(username, isEncrypt && !isDecrypt, cancellationToken);
            Environment.ExitCode = exitCode;
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use the overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string param, bool encrypt, CancellationToken cancellationToken)
    {
        try
        {
            if (encrypt)
                Console.WriteLine(AuthParameterRsaCipher.Encrypt(param));
            else
                Console.WriteLine(AuthParameterRsaCipher.Decrypt(param));

            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
    }
}
