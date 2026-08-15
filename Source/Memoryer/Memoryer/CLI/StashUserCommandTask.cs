using System.CommandLine;
using Encore.CLI;
using Encore.Data.Repositories;
using Encore.Metadata.Items;
using Encore.Options;
using Microsoft.Extensions.Options;
using Mozart.Data.Entities;

namespace Memoryer.CLI;

public class StashUserCommandTask(
    IUserRepository repository,
    IOptions<MetadataOptions> metadataOptions
) : ICommandLineTask
{
    public static string Name => "user:stash";
    public static string Description => "Add an item that match with the specified item id to a user's bag";

    public void ConfigureCommand(Command command)
    {
        var usernameArgument = new Argument<string>("username") { Description = "The username" };
        var itemIdArgument = new Argument<int>("item-id") { Description = "The item ID to add" };

        command.Arguments.Add(usernameArgument);
        command.Arguments.Add(itemIdArgument);
        command.SetAction(async (parsedResult, cancellationToken) =>
        {
            string username = parsedResult.GetRequiredValue(usernameArgument);
            int itemId = parsedResult.GetRequiredValue(itemIdArgument);
            Environment.ExitCode = await ExecuteAsync(username, itemId, cancellationToken);
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string username, int itemId, CancellationToken cancellationToken)
    {
        var itemData = LoadItemData();
        if (itemData == null)
            return 1;

        if (itemId <= 0 || itemId > short.MaxValue || !itemData.TryGetValue(itemId, out var item))
        {
            Console.WriteLine($"Item {itemId} was not found in ItemData.");
            return 1;
        }

        var user = await repository.FindByUsername(username, cancellationToken);
        if (user == null)
        {
            Console.WriteLine($"User '{username}' was not found.");
            return 1;
        }

        int slot = user.Inventory.FindSlot(itemId);
        if (slot < 0)
            slot = user.Inventory.FindSlot(0);
        if (slot < 0)
        {
            Console.WriteLine($"Unable to stash item {itemId}: '{username}' has no empty bag slot.");
            return 1;
        }

        user.Inventory[slot] = new Inventory.BagItem
        {
            Id = (short)itemId,
            Count = item.Quantity > 1 ? item.Quantity : 0
        };
        await repository.Update(user, cancellationToken);
        await repository.Commit(cancellationToken);
        Console.WriteLine($"Item {itemId} has been added to '{username}'s bag.");
        return 0;
    }

    private IReadOnlyDictionary<int, ItemData>? LoadItemData()
    {
        string path = metadataOptions.Value.ItemData;
        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("ItemData metadata file is not found", path);

            return ItemDataParser.Parse(File.ReadAllBytes(path), ItemDataFormat.Classic);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Console.WriteLine($"Unable to load ItemData: {exception.Message}");
            return null;
        }
    }
}


