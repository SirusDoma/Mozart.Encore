using CrossTime.Messages.Codecs;
using Encore.Messaging;
using Mozart.Metadata;
using Mozart.Metadata.Items;

namespace CrossTime.Messages.Responses;

public class CharacterInfoResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetCharacterInfo;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

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

    [MessageField(order: 9)]
    public int Draw { get; set; }

    [MessageField<CharacterEquipmentInfoCodec>(order: 13)]
    public Dictionary<ItemType, int> Equipments { get; init; } = [];

    [CollectionMessageField(order: 14, minCount: 30, maxCount: 30)]
    public IList<int> Inventory { get; init; } = [];

    [MessageField(order: 19)]
    public int GemStar { get; set; }

    [MessageField(order: 20)]
    public int Ticket { get; set; }
}
