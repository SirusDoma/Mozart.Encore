using Microsoft.Extensions.Logging;

using Encore.Server;

using Amadeus.Controllers.Filters;
using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;

using Mozart.Entities;
using Mozart.Data.Repositories;
using Mozart.Services;
using Mozart.Sessions;

namespace Amadeus.Controllers;

[ChannelAuthorize]
public class ItemShopController(Session session, IMetadataResolver resolver, IUserRepository repository,
    ILogger<ItemShopController> logger) : CommandController<Session>(session)
{
    private IChannel Channel => Session.Channel!;

    [CommandHandler(RequestCommand.EnterShop)]
    [CommandHandler(RequestCommand.ExitShop)]
    public void ChangeState()
    {
    }

    [CommandHandler]
    public async Task<PurchaseItemResponse> PurchaseItem(PurchaseItemRequest request,
        CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.PurchaseItem,
            "[{User}] Purchase item id: {Item}", actor.Nickname, request.ItemId);

        var user      = (await repository.Find(actor.UserId, cancellationToken))!;
        var inventory = user.Inventory;

        var itemData = resolver.GetItemData(Channel);
        int itemId   = request.ItemId;
        int index    = inventory.FindSlot(0);

        if (!itemData.TryGetValue(itemId, out var item))
            throw new ArgumentOutOfRangeException(nameof(request));

        if (inventory.Any(id => itemId == id))
        {
            return new PurchaseItemResponse
            {
                Result             = PurchaseItemResponse.PurchaseResult.InventoryFull,
                TotalUserGem       = actor.Gem,
                TotalUserPoint     = actor.Point,
                InventorySlotIndex = 0,
                ItemId             = inventory[0]
            };
        }

        if (index < 0 || inventory.Count >= inventory.Capacity)
            return new PurchaseItemResponse { Result = PurchaseItemResponse.PurchaseResult.InventoryFull };

        if (user.Gem < item.Price.Gem)
            return new PurchaseItemResponse { Result = PurchaseItemResponse.PurchaseResult.InsufficientMoney };

        inventory[index] = (short)itemId;
        user.Gem -= item.Price.Gem;

        await repository.Update(user, cancellationToken);
        await repository.Commit(cancellationToken);

        actor.Sync(user);

        return new PurchaseItemResponse
        {
            Result             = PurchaseItemResponse.PurchaseResult.Success,
            TotalUserGem       = actor.Gem,
            TotalUserPoint     = actor.Point,
            InventorySlotIndex = index,
            ItemId             = itemId
        };
    }

    [CommandHandler]
    public async Task<SellItemResponse> SellItem(SellItemRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SellItem,
            "[{User}] Sell item slot: {Item}", actor.Nickname, request.InventorySlotIndex);

        var user      = (await repository.Find(actor.UserId, cancellationToken))!;
        var inventory = user.Inventory;

        int index = request.InventorySlotIndex;
        if (index < 0 || index >= inventory.Capacity)
            return new SellItemResponse { Invalid = true };

        var itemData = resolver.GetItemData(Channel);
        int itemId   = inventory[index];

        if (!itemData.TryGetValue(itemId, out var item))
            return new SellItemResponse { Invalid = true };

        inventory[index] = 0;
        user.Gem += item.Price.Gem;

        await repository.Update(user, cancellationToken);
        await repository.Commit(cancellationToken);

        actor.Sync(user);

        return new SellItemResponse
        {
            Invalid = false,
            Unknown = index
        };
    }
}