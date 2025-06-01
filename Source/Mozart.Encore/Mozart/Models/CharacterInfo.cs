using Encore.Messaging;

namespace Mozart;

public enum Gender : byte
{
    Female = 0,
    Male   = 1
}

public class CharacterInfo : SubMessage
{
    public required string Token { get; init; }

    [StringMessageField(order: 0)]
    public required string Nickname { get; init; }

    [MessageField(order: 1)]
    public Gender Gender { get; init; }

    [MessageField(order: 2)]
    public int Gem { get; set; }

    [MessageField(order: 3)]
    public int Point { get; set; }

    [MessageField(order: 4)]
    public int Level { get; set; }

    [MessageField(order: 5)]
    public int Win { get; set; }

    [MessageField(order: 6)]
    public int Lose { get; set; }

    [MessageField(order: 7)]
    public int Draw { get; set; }

    [MessageField(order: 8)]
    public int Experience { get; set; }

    [MessageField(order: 9)]
    public bool IsAdministrator { get; init; }

    [MessageField<CharacterEquipmentInfoCodec>(order: 10)]
    public Dictionary<ItemType, int> Equipments { get; init; } = [];

    [CollectionMessageField(order: 11, minCount: 30, maxCount: 30)]
    public IList<int> Inventory { get; init; } = [];

    [CollectionMessageField(order: 12, prefixSizeType: TypeCode.Int32)]
    public IList<int> AttributiveItemIds { get; init; } = [];

    public IReadOnlyList<int> MusicIds { get; set; } = [];
}