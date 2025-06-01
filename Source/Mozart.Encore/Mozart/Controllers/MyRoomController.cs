using Encore.Server;
using Encore.Sessions;

namespace Mozart;

[Authorize]
public class MyRoomController(Session session) : CommandController(session)
{
    [CommandHandler(RequestCommand.EnterShop)]
    [CommandHandler(RequestCommand.ExitShop)]
    public void ChangeState()
    {
    }

    [CommandHandler]
    public EquipItemResponse EquipItem(EquipItemRequest request)
    {
        var character = Session.GetAuthorizedToken<CharacterInfo>();

        int index      = request.InventorySlotIndex;
        var type       = request.ItemType;
        var inventory  = character.Inventory;
        var equipments = character.Equipments;

        if (index < 0 || index >= inventory.Count)
            return new EquipItemResponse { Invalid = true };

        if (type is < 0 or > ItemType.Face)
            return new EquipItemResponse { Invalid = true };

        int prevId = equipments[type];
        int newId  = inventory[index];

        equipments[type] = newId;
        inventory[index] = prevId;

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