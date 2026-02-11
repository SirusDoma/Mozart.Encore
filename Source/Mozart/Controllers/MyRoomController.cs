using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Messages.Requests;
using Mozart.Messages.Responses;
using Mozart.Metadata.Items;
using Mozart.Data.Repositories;
using Mozart.Sessions;

namespace Mozart.Controllers;

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
        logger.LogInformation((int)RequestCommand.PurchaseItem,
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
}
