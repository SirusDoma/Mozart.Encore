using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;
using Mozart.Data.Entities;

namespace Amadeus.CLI;

public class UpsertUserRankingCommandTask(MainDbContext context) : ICommandLineTask
{
    public static string Name => "ranking:upsert";
    public static string Description => "Generates or updates user rankings. This command is intended to be executed periodically using a scheduled cron job.";

    public void ConfigureCommand(Command command)
    {
        var countOption = new Option<int>("--count")
        {
            DefaultValueFactory = _ => 1000,
            Description = "The maximum number of rankings to compute"
        };

        command.Options.Add(countOption);

        command.SetAction(async (parsedResult, cancellationToken) =>
        {
            int count = parsedResult.GetValue(countOption);
            int exitCode = await ExecuteAsync(count, cancellationToken);
            Environment.ExitCode = exitCode;
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        return ExecuteAsync(1000, cancellationToken);
    }

    public async Task<int> ExecuteAsync(int count, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        {
            var topJammers = await context.Users
                .OrderByDescending(u => u.Level)
                .ThenByDescending(u => u.Experience)
                .Take(count)
                .ToListAsync(cancellationToken);

            for (int i = 0; i < topJammers.Count; i++)
            {
                var player = topJammers[i];
                var record = await context.UserRankings.FirstOrDefaultAsync(r => r.Ranking == i + 1, cancellationToken);

                if (record == null)
                {
                    record = new UserRanking
                    {
                        UserId  = player.Id,
                        Ranking = i + 1
                    };

                    await context.UserRankings.AddAsync(record, cancellationToken);
                }
                else
                {
                    record.UserId = player.Id;
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            await context.UserRankings.Where(r => r.Ranking > topJammers.Count)
                .ExecuteDeleteAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        Console.WriteLine("User ranking has been updated successfully");
        return 0;
    }
}