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

public class RegisterUserCommandTask(UserDbContext context, IOptions<AuthOptions> authOptions) : ICommandLineTask
{
    public static string Name => "user:register";
    public static string Description => "Register the user to the configured database";

    public void ConfigureCommand(Command command)
    {
        // Add required arguments
        var usernameArgument = new Argument<string>("username", "The username for the new user (will be used for nickname too)");
        var passwordArgument = new Argument<string>("password", "The password for the new user");

        command.AddArgument(usernameArgument);
        command.AddArgument(passwordArgument);

        // Add optional options
        var genderOption = new Option<Gender>("--gender", () => Gender.Male, "The gender of character (default: Male)");
        var adminOption  = new Option<bool>("--admin", () => false, "Specify whether the user has admin right");

        command.AddOption(genderOption);

        // Set the handler with all parameters
        command.SetHandler(async (username, password, gender, admin) =>
        {
            int exitCode = await ExecuteAsync(username, password, gender, admin);
            Environment.ExitCode = exitCode;
        }, usernameArgument, passwordArgument, genderOption, adminOption);
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // This won't be called since we override the handler in ConfigureCommand
        throw new NotSupportedException("Use overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, string password, Gender gender, bool admin)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var user = new User
        {
            Id              = 0,
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
        await context.AddAsync(user);
        await context.SaveChangesAsync();

        user.Equipments[ItemType.Face] = (short)(gender == Gender.Female ? 36 : 35);

        var rawPassword = Encoding.UTF8.GetBytes(password);
        var credential = new Credential
        {
            Id       = 0,
            UserId   = user.Id,
            Username = username,
            Password = authOptions.Value.Mode == AuthMode.Default ? PasswordHasher.Hash(rawPassword) : rawPassword
        };

        await context.AddAsync(credential);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
        Console.WriteLine("User has been registered successfully");

        return 0;
    }
} 