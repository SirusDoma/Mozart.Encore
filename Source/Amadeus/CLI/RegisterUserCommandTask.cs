using System.CommandLine;
using System.Text;
using Microsoft.Extensions.Options;
using Mozart.Metadata;
using Mozart.Metadata.Items;
using Mozart.Options;
using Mozart.Data.Contexts;
using Mozart.Data.Entities;
using Mozart.Services;

namespace Amadeus.CLI;

public class RegisterUserCommandTask(MainDbContext context, IOptions<AuthOptions> authOptions) : ICommandLineTask
{
    public static string Name => "user:register";
    public static string Description => "Register the user to the configured database";

    public void ConfigureCommand(Command command)
    {
        // Add required arguments
        var usernameArgument = new Argument<string>("username") { Description = "The username for the new user (will be used for nickname too)" };
        var passwordArgument = new Argument<string>("password") { Description = "The password for the new user" };

        command.Arguments.Add(usernameArgument);
        command.Arguments.Add(passwordArgument);

        // Add optional options
        var genderOption = new Option<Gender>("--gender")
        {
            DefaultValueFactory = _ => Gender.Male,
            Description = "The gender of character (default: Male)"
        };
        var adminOption  = new Option<bool>("--admin")
        {
            DefaultValueFactory = _  => false,
            Description = "Specify whether the user has admin right"
        };

        command.Options.Add(genderOption);
        command.Options.Add(adminOption);

        // Set the handler with all parameters
        command.SetAction(async (parsedResult, cancellationToken) =>
        {
            string username = parsedResult.GetRequiredValue(usernameArgument);
            string password = parsedResult.GetRequiredValue(passwordArgument);
            var gender      = parsedResult.GetRequiredValue(genderOption);
            bool admin      = parsedResult.GetRequiredValue(adminOption);

            int exitCode = await ExecuteAsync(username, password, gender, admin, cancellationToken);
            Environment.ExitCode = exitCode;
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        // This won't be called since we override the handler in ConfigureCommand
        throw new NotSupportedException("Use overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, string password, Gender gender, bool admin, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var user = new User
        {
            Username        = username,
            Nickname        = username,
            Gender          = gender,
            Level           = 1,
            Battle          = 0,
            Win             = 0,
            Lose            = 0,
            Draw            = 0,
            Experience      = 0,
            IsAdministrator = admin,
            Gem             = 0,
            Point           = 0
        };
        await context.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        user.Equipments[ItemType.Face] = (short)(gender == Gender.Female ? 36 : 35);

        var rawPassword = Encoding.UTF8.GetBytes(password);
        var credential = new Credential
        {
            Username = username,
            Password = authOptions.Value.Mode == AuthMode.Default ? PasswordHasher.Hash(rawPassword) : rawPassword
        };

        await context.AddAsync(credential, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        Console.WriteLine("User has been registered successfully");

        return 0;
    }
}
