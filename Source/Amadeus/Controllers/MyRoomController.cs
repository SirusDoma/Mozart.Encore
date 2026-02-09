using Encore.Server;
using Microsoft.Extensions.Logging;
using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
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

        if (index < 0 || index >= inventory.Capacity)
            return new EquipItemResponse { Invalid = true };

        if (type is < 0 or > ItemType.Face)
            return new EquipItemResponse { Invalid = true };

        short prevId = equipments[type];
        short newId  = inventory[index];

        equipments[type] = newId;
        inventory[index] = prevId;

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
    public async Task<AcceptGiftResponse> AcceptGift(AcceptGiftRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SyncItemPurchase,
            "[{User}] Accept gift: ({Type}) {Item}", actor.Nickname, request.GiftType, request.ItemId);

        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        bool success = false;

        if (request.GiftType == GiftType.Item)
        {
            // TODO: Validate GiftId against gift db table

            var inventory = user.Inventory;
            for (int i = 0; i < inventory.Capacity; i++)
            {
                if (inventory[i] == 0)
                {
                    inventory[i] = (short)request.ItemId;
                    success = true;

                    break;
                }
            }

            await repository.Update(user, cancellationToken);
            await repository.Commit(cancellationToken);

            actor.Sync(user);
        }
        else
        {
            return new AcceptGiftResponse
            {
                Invalid = false,
                Result  = AcceptGiftResponse.AcceptGiftResult.NotDefined
            };
        }

        return new AcceptGiftResponse
        {
            Invalid = !success,
            Result  = success ? AcceptGiftResponse.AcceptGiftResult.Success :
                AcceptGiftResponse.AcceptGiftResult.UnknownError
        };
    }
}