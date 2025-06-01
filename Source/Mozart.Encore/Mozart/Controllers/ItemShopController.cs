using Encore.Server;
using Encore.Sessions;

namespace Mozart;

[Authorize]
public class ItemShopController(Session session) : CommandController(session)
{
    [CommandHandler(RequestCommand.EnterShop)]
    [CommandHandler(RequestCommand.ExitShop)]
    public void ChangeState()
    {
    }

    [CommandHandler]
    public PurchaseItemResponse PurchaseItem(PurchaseItemRequest request)
    {
        var character = Session.GetAuthorizedToken<CharacterInfo>();
        var inventory = character.Inventory;

        int itemId = request.ItemId;
        if (inventory.All(id => id != 0) || inventory.Any(id => id == itemId))
            return new PurchaseItemResponse { Result = PurchaseItemResponse.PurchaseResult.InventoryFull };

        if (character.Gem < 1000)
            return new PurchaseItemResponse { Result = PurchaseItemResponse.PurchaseResult.InsufficientMoney };

        int index = inventory.IndexOf(0);
        inventory[index] = itemId;

        character.Gem -= 1000;

        return new PurchaseItemResponse
        {
            Result             = PurchaseItemResponse.PurchaseResult.Success,
            TotalUserGem       = character.Gem,
            TotalUserPoint     = character.Point,
            InventorySlotIndex = index,
            ItemId             = itemId
        };
    }

    [CommandHandler]
    public SellItemResponse SellItem(SellItemRequest request)
    {
        var character = Session.GetAuthorizedToken<CharacterInfo>();
        var inventory = character.Inventory;

        int index = request.InventorySlotIndex;
        if (index < 0 || index >= inventory.Count)
            return new SellItemResponse { Invalid = true };

        inventory[index] = 0;

        character.Gem   += 5_000_000;
        character.Point += 500_000;

        return new SellItemResponse
        {
            Invalid = false,
            TotalUserGem       = character.Gem,
            TotalUserPoint     = character.Point,
            InventorySlotIndex = index
        };
    }
}