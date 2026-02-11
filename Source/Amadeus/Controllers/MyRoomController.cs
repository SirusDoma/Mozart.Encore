using Encore.Server;
using Microsoft.Extensions.Logging;
using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
using Mozart.Data.Entities;
using Mozart.Metadata.Items;
using Mozart.Data.Repositories;
using Mozart.Metadata;
using Mozart.Sessions;

namespace Amadeus.Controllers;

[Authorize]
public class MyRoomController(Session session, IUserRepository repository, ILogger<MyRoomController> logger)
    : CommandController<Session>(session)
{
    [CommandHandler(RequestCommand.EnterShop)]
    [CommandHandler(RequestCommand.ExitShop)]
    public void ChangeState()
    {
    }

    [CommandHandler]
    public async Task<EquipItemResponse> EquipItem(EquipItemRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SyncItemPurchase,
            "[{User}] Equip item slot: {Item}", actor.Nickname, request.InventorySlotIndex);

        var user       = (await repository.Find(actor.UserId, cancellationToken))!;
        int index      = request.InventorySlotIndex;
        var type       = request.ItemType;
        var inventory  = user.Inventory;
        var equipments = user.Equipments;

        if (index < 0 || index >= inventory.Capacity || !Enum.IsDefined(typeof(ItemType), type))
            return new EquipItemResponse { Invalid = true };

        short prevId = equipments[type];
        short newId  = inventory[index].Id;

        equipments[type] = newId;
        inventory[index] = new Inventory.BagItem { Id = prevId };

        await repository.Update(user, cancellationToken);
        await repository.Commit(cancellationToken);

        actor.Sync(user);

        return new EquipItemResponse
        {
            Invalid                 = false,
            ItemType                = type,
            NewEquippedItemId       = newId,
            InventorySlotIndex      = index,
            PreviousEquippedItemId  = prevId
        };
    }

    [CommandHandler]
    public async Task<ClaimGiftResponse> ClaimGift(ClaimGiftRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SyncItemPurchase,
            "[{User}] Claim gift: ({Type}) {Item}", actor.Nickname, request.GiftType, request.ResourceId);

        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        bool success = false;

        if (request.GiftType == GiftType.Item)
        {
            var items = Session.Channel!.GetItemData();
            success   = user.GiftBox.ClaimItem(request.GiftId, items[request.ResourceId]);
        }
        else
        {
            success = user.GiftBox.ClaimMusic(request.GiftId, request.ResourceId);
        }

        if (success)
        {
            await repository.Update(user, cancellationToken);
            await repository.Commit(cancellationToken);
        }

        actor.Sync(user);

        return new ClaimGiftResponse
        {
            Invalid = !success,
            Result  = success ? ClaimGiftResponse.ClaimGiftResult.Success :
                ClaimGiftResponse.ClaimGiftResult.UnknownError
        };
    }
}