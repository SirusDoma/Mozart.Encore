using Identity.Messages.Requests;
using Identity.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Data.Contexts;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;
using Mozart.Metadata;
using Mozart.Metadata.Items;
using Mozart.Sessions;

namespace Identity.Controllers;

[Authorize]
public class MyRoomController(
    Session session,
    IUserRepository repository,
    ILogger<MyRoomController> logger
) : CommandController<Session>(session)
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
        logger.LogInformation((int)RequestCommand.EquipItem,
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
        logger.LogInformation((int)RequestCommand.ClaimGift,
            "Claim gift: ({Type}) {Resource}", request.GiftType, request.ResourceId);

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

    [CommandHandler]
    public async Task<ChangeNameResponse> ChangeName(ChangeNameRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.ChangeName, "Change name: {Name}", request.Name);

        try
        {
            var user = (await repository.Find(actor.UserId, cancellationToken))!;
            var bagItem = user.Inventory[request.InventorySlotIndex];

            if (bagItem.Id != request.ItemId)
                throw new ArgumentOutOfRangeException(nameof(request));

            if (!Session.Channel!.GetItemData().TryGetValue(request.ItemId, out var item))
                throw new ArgumentOutOfRangeException(nameof(request));

            if (item.ItemKind != ItemKind.NameChanger)
                throw new ArgumentOutOfRangeException(nameof(request));

            if (bagItem.Count > 1)
            {
                user.Inventory[request.InventorySlotIndex] = new Inventory.BagItem
                {
                    Id = bagItem.Id,
                    Count = bagItem.Count - 1
                };
            }
            else
                user.Inventory[request.InventorySlotIndex] = Inventory.BagItem.Empty;

            user.Nickname = request.Name;

            await repository.Update(user, cancellationToken);
            await repository.Commit(cancellationToken);

            actor.Sync(user);

            return new ChangeNameResponse
            {
                Invalid = false,
                Name = request.Name
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            return new ChangeNameResponse
            {
                Invalid = true,
                Name    = actor.Nickname
            };
        }
    }


    [CommandHandler]
    public async Task<CheckNameResponse> CheckName(CheckNameRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.CheckName, "Check name: {Name}", request.Name);

        return new CheckNameResponse
        {
            Exists = await repository.FindByNickname(request.Name, cancellationToken) != null,
            Name   = request.Name
        };
    }

    [CommandHandler(RequestCommand.PenaltyReset)]
    public async Task<PenaltyResetResponse> ResetPenalty(CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.PenaltyReset, "Penalty reset");

        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        for (int i = 0; i < user.Inventory.Capacity; i++)
        {
            var bagItem = user.Inventory[i];
            if (bagItem.Id == 0)
                continue;

            if (!Session.Channel!.GetItemData().TryGetValue(bagItem.Id, out var item))
                continue;

            if (item.ItemKind != ItemKind.PenaltyReset)
                continue;

            if (bagItem.Count > 1)
            {
                user.Inventory[i] = new Inventory.BagItem
                {
                    Id = bagItem.Id,
                    Count = bagItem.Count - 1
                };
            }
            else
                user.Inventory[i] = Inventory.BagItem.Empty;

            user.PenaltyCount = 0;
            user.PenaltyLevel = 0;

            await repository.Update(user, cancellationToken);
            await repository.Commit(cancellationToken);

            actor.Sync(user);

            return new PenaltyResetResponse
            {
                Invalid = false
            };
        }

        return new PenaltyResetResponse
        {
            Invalid = true
        };
    }

    [CommandHandler]
    public async Task<BagExpansionResponse> UseBagExpansion(BagExpansionRequest request,
        CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.BagExpansion, "Use bag expansion: [{Slot}:{Item}]",
            request.BagSlotIndex, request.ItemId);

        try
        {
            var user      = (await repository.Find(actor.UserId, cancellationToken))!;
            var inventory = user.Inventory;
            var bagItem   = inventory[request.BagSlotIndex];

            if (bagItem.Id != request.ItemId)
                throw new ArgumentOutOfRangeException(nameof(request));

            if (!Session.Channel!.GetItemData().TryGetValue(request.ItemId, out var item))
                throw new ArgumentOutOfRangeException(nameof(request));

            if (item.ItemKind != ItemKind.BagExpansion)
                throw new ArgumentOutOfRangeException(nameof(request));

            inventory.Expand();

            if (bagItem.Count > 1)
            {
                inventory[request.BagSlotIndex] = new Inventory.BagItem
                {
                    Id    = bagItem.Id,
                    Count = bagItem.Count - 1
                };
            }
            else
                inventory[request.BagSlotIndex] = Inventory.BagItem.Empty;

            await repository.Update(user, cancellationToken);
            await repository.Commit(cancellationToken);

            actor.Sync(user);

            return new BagExpansionResponse
            {
                Invalid       = false,
                ExpansionSize = Inventory.SlotsPerExpansion
            };
        }
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or InvalidOperationException)
        {
            return new BagExpansionResponse
            {
                Invalid       = true,
                ExpansionSize = 0
            };
        }
    }

    [CommandHandler(RequestCommand.RefershBag)]
    public async Task<BagRefreshResponse> RefreshBag(CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.RefershBag, "Refresh bag");

        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        actor.Sync(user);

        return new BagRefreshResponse
        {
            Invalid          = false,
            Inventory        = actor.Inventory.Select(i => (int)i.Id).ToList(),
            AttributiveItems = actor.Inventory.Where(i => i.Count > 0)
                .Select(i => new BagRefreshResponse.AttributiveItemInfo
                {
                    AttributiveItemId = i.Id,
                    ItemCount         = i.Count
                }).ToList()
        };
    }
}
