using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;
using Mozart.Data.Entities;
using Mozart.Metadata;

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
        await using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken))
        {
            var existingExtendedRankings = await context.UserRankingsExtended
                .ToDictionaryAsync(r => r.UserId, r => r.Ranking, cancellationToken);

            var topJammers = await context.Users
                .IgnoreAutoIncludes()
                .OrderByDescending(u => u.Level)
                .ThenByDescending(u => u.Experience)
                .Take(count)
                .ToListAsync(cancellationToken);

            for (int i = 0; i < topJammers.Count; i++)
            {
                var player = topJammers[i];
                int newRank = i + 1;
                int oldRank = existingExtendedRankings.GetValueOrDefault(player.Id, 0);

                int changeType;
                int changeDelta;
                if (oldRank == 0)
                {
                    changeType  = 1;
                    changeDelta = 0;
                }
                else if (newRank < oldRank)
                {
                    changeType  = 1;
                    changeDelta = oldRank - newRank;
                }
                else if (newRank == oldRank)
                {
                    changeType  = 2;
                    changeDelta = 0;
                }
                else
                {
                    changeType  = 0;
                    changeDelta = newRank - oldRank;
                }

                var extended = await context.UserRankingsExtended
                    .FirstOrDefaultAsync(r => r.UserId == player.Id, cancellationToken);

                if (extended == null)
                {
                    extended = new UserRankingExtended
                    {
                        UserId        = player.Id,
                        Username      = player.Username,
                        Nickname      = player.Nickname,
                        Sex           = player.Gender == Gender.Male || player.Gender == Gender.Any,
                        Level         = player.Level,
                        Battle        = player.Battle,
                        Win           = player.Win,
                        Draw          = player.Draw,
                        Lose          = player.Lose,
                        Experience    = player.Experience,
                        Ranking       = newRank,
                        ChangeType    = changeType,
                        ChangeRanking = changeDelta
                    };

                    await context.UserRankingsExtended.AddAsync(extended, cancellationToken);
                }
                else
                {
                    extended.Username      = player.Username;
                    extended.Nickname      = player.Nickname;
                    extended.Sex           = player.Gender == Gender.Male || player.Gender == Gender.Any;
                    extended.Level         = player.Level;
                    extended.Battle        = player.Battle;
                    extended.Win           = player.Win;
                    extended.Draw          = player.Draw;
                    extended.Lose          = player.Lose;
                    extended.Experience    = player.Experience;
                    extended.Ranking       = newRank;
                    extended.ChangeType    = changeType;
                    extended.ChangeRanking = changeDelta;
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            var rankedUserIds = topJammers.Select(u => u.Id).ToHashSet();
            await context.UserRankingsExtended
                .Where(r => !rankedUserIds.Contains(r.UserId))
                .ExecuteDeleteAsync(cancellationToken);


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
            }

            await context.SaveChangesAsync(cancellationToken);
            await context.UserRankings.Where(r => r.Ranking > topJammers.Count)
                .ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        Console.WriteLine("User ranking has been updated successfully");
        return 0;
    }
}
