using System.CommandLine;
using Encore.CLI;
using Encore.Data.Repositories;
using Encore.Metadata.Items;
using Encore.Options;
using Microsoft.Extensions.Options;
using Mozart.Metadata.Items;

namespace Mozart.CLI;

public class EquipUserCommandTask(
    IUserRepository repository,
    IOptions<MetadataOptions> metadataOptions
) : ICommandLineTask
{
    public static string Name => "user:equip";
    public static string Description => "Equip an item for a user";

    public void ConfigureCommand(Command command)
    {
        var usernameArgument = new Argument<string>("username") { Description = "The username" };
        var itemIdArgument = new Argument<int>("item-id") { Description = "The item ID to equip" };

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

        if (!TryGetItemType(item.ItemPart, out var itemType))
        {
            Console.WriteLine($"Item {itemId} cannot be equipped.");
            return 1;
        }

        short equippedItemId = user.Equipments[itemType];
        if (equippedItemId == itemId)
        {
            Console.WriteLine($"Item {itemId} is already equipped by '{username}'.");
            return 0;
        }

        if (equippedItemId != 0)
        {
            int emptySlot = user.Inventory.FindSlot(0);
            if (emptySlot < 0)
            {
                Console.WriteLine($"Unable to equip item {itemId}: '{username}' has no empty bag slot.");
                return 1;
            }

            user.Inventory[emptySlot] = equippedItemId;
        }

        user.Equipments[itemType] = (short)itemId;
        await repository.Update(user, cancellationToken);
        await repository.Commit(cancellationToken);
        Console.WriteLine($"Item {itemId} has been equipped by '{username}'.");
        return 0;
    }

    private IReadOnlyDictionary<int, ItemData>? LoadItemData()
    {
        string path = metadataOptions.Value.ItemData;
        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("ItemData metadata file is not found", path);

            return ItemDataParser.Parse(File.ReadAllBytes(path));
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Console.WriteLine($"Unable to load ItemData: {exception.Message}");
            return null;
        }
    }

    private static bool TryGetItemType(ItemPart itemPart, out ItemType itemType)
    {
        itemType = itemPart switch
        {
            ItemPart.Instrument  => ItemType.Instrument,
            ItemPart.Hair        => ItemType.Hair,
            ItemPart.Accessories => ItemType.Accessories,
            ItemPart.Glove       => ItemType.Gloves,
            ItemPart.Necklace    => ItemType.Necklace,
            ItemPart.Jacket      => ItemType.Top,
            ItemPart.Pants       => ItemType.Pants,
            ItemPart.Glasses     => ItemType.Glasses,
            ItemPart.Earrings    => ItemType.Earring,
            ItemPart.Armlet      => ItemType.ClothesAccessories,
            ItemPart.Shoes       => ItemType.Shoes,
            ItemPart.Face        => ItemType.Face,
            _                    => default
        };
        return itemPart <= ItemPart.Face;
    }
}
