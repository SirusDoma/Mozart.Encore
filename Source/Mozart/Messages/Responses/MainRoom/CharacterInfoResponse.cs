using Encore.Messaging;
using Mozart.Messages.Codecs;
using Mozart.Metadata;
using Mozart.Metadata.Items;

namespace Mozart.Messages.Responses;

public class CharacterInfoResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetCharacterInfo;

    // Suspended?
    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool DisableInventory { get; init; }

    [StringMessageField(order: 1)]
    public required string Nickname { get; init; }

    [MessageField(order: 2)]
    public Gender Gender { get; init; }

    [MessageField(order: 3)]
    public int Gem { get; set; }

    [MessageField(order: 4)]
    public int Point { get; set; }

    [MessageField(order: 5)]
    public int Level { get; set; }

    [MessageField(order: 6)]
    public int Win { get; set; }

    [MessageField(order: 7)]
    public int Lose { get; set; }

    [MessageField(order: 8)]
    public int Draw { get; set; }

    [MessageField(order: 9)]
    public int Experience { get; set; }

    [MessageField(order: 10)]
    public bool IsAdministrator { get; init; }

    [MessageField<CharacterEquipmentInfoCodec>(order: 11)]
    public Dictionary<ItemType, int> Equipments { get; init; } = [];

    [CollectionMessageField(order: 12, minCount: 30, maxCount: 30)]
    public IList<int> Inventory { get; init; } = [];

    [CollectionMessageField(order: 13, prefixSizeType: TypeCode.Int32)]
    public IList<int> AttributiveItemIds { get; init; } = [];
}