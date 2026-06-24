using Encore.Server;
using Memoryer.Controllers.Filters;
using Memoryer.Messages.Requests;
using Memoryer.Messages.Responses;
using Microsoft.Extensions.Logging;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;
using Mozart.Entities;
using Mozart.Sessions;

namespace Memoryer.Controllers;

[ChannelAuthorize]
public class ItemShopController(
    Session session,
    IUserRepository repository,
    ILogger<ItemShopController> logger
) : CommandController<Session>(session)
{
    private IChannel Channel => Session.Channel!;

    [CommandHandler(RequestCommand.EnterShop)]
    [CommandHandler(RequestCommand.ExitShop)]
    public void ChangeState()
    {
    }

    [CommandHandler(RequestCommand.StartPayment, ResponseCommand.StartPayment)]
    public void Checkout()
    {
        logger.LogInformation((int)RequestCommand.StartPayment,
            "Payment checkout");
    }

    [CommandHandler(RequestCommand.SyncItemPurchase)]
    public async Task<SyncItemPurchaseResponse> SyncItemPurchase(CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SyncItemPurchase,
            "Sync item purchase");

        // The actual transaction happen within the web page, we only need to sync the latest user info
        var user      = (await repository.Find(actor.UserId, cancellationToken))!;
        var inventory = user.Inventory;

        var extensionPeriod = TimeSpan.Zero;
        if (actor.FreePass.Type != FreePassType.None || user.FreePass.Type != FreePassType.None)
            extensionPeriod = (actor.FreePass.ExpiryDate - user.FreePass.ExpiryDate).Duration();

        actor.Sync(user);

        return new SyncItemPurchaseResponse
        {
            Gem                     = user.Gem,
            Point                   = user.Point,
            O2Cash                  = user.O2Cash,
            Inventory               = inventory.Take(inventory.Capacity).Select(item => (int)item.Id).ToList(),
            MusicCash               = user.MusicCash,
            ItemCash                = user.ItemCash,
            AttributiveItems        = user.Inventory
                .Where(i => i.Count > 0)
                .Select(i => new SyncItemPurchaseResponse.AttributiveItemInfo
                {
                    AttributiveItemId = i.Id,
                    ItemCount         = i.Count
                })
                .ToList(),
            FreePassExtensionPeriod =  extensionPeriod
        };
    }

    [CommandHandler]
    public async Task<SellItemResponse> SellItem(SellItemRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SellItem,
            "Sell item slot: {Slot} ({Item})", request.InventorySlotIndex, request.ItemId);

        var user      = (await repository.Find(actor.UserId, cancellationToken))!;
        var inventory = user.Inventory;

        int index = request.InventorySlotIndex;
        if (index < 0 || index >= inventory.Capacity)
            return new SellItemResponse { Invalid = true };

        var itemData = Channel.GetItemData();
        int itemId   = inventory[index].Id;

        if (!itemData.TryGetValue(itemId, out var item))
            return new SellItemResponse { Invalid = true };

        inventory[index] = Inventory.BagItem.Empty;
        user.Gem += item.Price.Gem;

        await repository.Update(user, cancellationToken);
        await repository.Commit(cancellationToken);

        actor.Sync(user);

        return new SellItemResponse
        {
            Invalid = false,
            Gem = user.Gem
        };
    }
}
