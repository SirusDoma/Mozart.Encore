using System.CommandLine;
using Encore.Data.Repositories;

namespace Encore.CLI;

public class DepositUserCommandTask(IUserRepository repository) : ICommandLineTask
{
    public static string Name => "user:deposit";
    public static string Description => "Add gems and points to a user";

    public void ConfigureCommand(Command command)
    {
        var usernameArgument = new Argument<string>("username") { Description = "The username" };
        var gemArgument = new Argument<int>("gem") { Description = "The gems to add" };
        var pointArgument = new Argument<int>("point")
        {
            DefaultValueFactory = _ => 0,
            Description = "The points to add (default: 0)"
        };

        command.Arguments.Add(usernameArgument);
        command.Arguments.Add(gemArgument);
        command.Arguments.Add(pointArgument);
        command.SetAction(async (parsedResult, cancellationToken) =>
        {
            string username = parsedResult.GetRequiredValue(usernameArgument);
            int gem = parsedResult.GetRequiredValue(gemArgument);
            int point = parsedResult.GetValue(pointArgument);
            Environment.ExitCode = await ExecuteAsync(username, gem, point, cancellationToken);
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, int gem, int point,
        CancellationToken cancellationToken)
    {
        if (gem < 0 || point < 0)
        {
            Console.WriteLine("Gem and point amounts cannot be negative.");
            return 1;
        }

        var user = await repository.FindByUsername(username, cancellationToken);
        if (user == null)
        {
            Console.WriteLine($"User '{username}' was not found.");
            return 1;
        }

        try
        {
            user.Gem = checked(user.Gem + gem);
            user.Point = checked(user.Point + point);
        }
        catch (OverflowException)
        {
            Console.WriteLine($"Unable to deposit currency: '{username}'s balance would overflow.");
            return 1;
        }

        await repository.Update(user, cancellationToken);
        await repository.Commit(cancellationToken);
        Console.WriteLine($"Deposited {gem} gem and {point} point to '{username}'.");
        return 0;
    }
}
