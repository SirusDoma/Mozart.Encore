using Encore.Messaging;
using Amadeus.Messages.Codecs;
using Mozart.Metadata;
using Mozart.Metadata.Items;

namespace Amadeus.Messages.Responses;

public class CharacterInfoResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetCharacterInfo;

    public class GiftItemInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int Unknown { get; init; } = 0; // Flag?

        [MessageField(order: 1)]
        public int ItemId { get; init; }

        [StringMessageField(order: 2)]
        public string Sender { get; init; } = string.Empty;
    }

    public class GiftMusicInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int Unknown { get; init; } = 0; // Flag?

        [MessageField(order: 1)]
        public int MusicId { get; init; }

        [StringMessageField(order: 2)]
        public string Sender { get; init; } = string.Empty;
    }

    public class AttributiveItemInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int AttributiveItemId { get; init; }

        [MessageField(order: 1)]
        public int ItemCount { get; init; }
    }

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
    public int Unknown1 { get; set; }

    [MessageField(order: 6)]
    public int Level { get; set; }

    [MessageField(order: 7)]
    public int Win { get; set; }

    [MessageField(order: 8)]
    public int Lose { get; set; }

    [MessageField(order: 9)]
    public int Draw { get; set; }

    [MessageField(order: 10)]
    public int Experience { get; set; }

    [MessageField(order: 11)]
    public int Unknown2 { get; set; }

    [MessageField(order: 12)]
    public bool IsAdministrator { get; init; }

    [MessageField<CharacterEquipmentInfoCodec>(order: 13)]
    public Dictionary<ItemType, int> Equipments { get; init; } = [];

    [CollectionMessageField(order: 14, minCount: 30, maxCount: 30)]
    public IList<int> Inventory { get; init; } = [];

    [CollectionMessageField(order: 15, prefixSizeType: TypeCode.Int32)]
    public IList<ushort> MissingMusicIds { get; init; } = [];

    [MessageField(order: 16)]
    public int Unknown3 { get; set; } // Cash point?

    [CollectionMessageField(order: 17, prefixSizeType: TypeCode.Int16)]
    public IList<GiftItemInfo> ItemGiftBox { get; init; } = [];

    [CollectionMessageField(order: 18, prefixSizeType: TypeCode.Int16)]
    public IList<GiftMusicInfo> MusicGiftBox { get; init; } = [];

    [MessageField(order: 19)]
    public int Unknown4 { get; set; } // Number of penalties?

    [MessageField(order: 20)]
    public int Unknown5 { get; set; } // Current penalty level?

    [CollectionMessageField(order: 21, prefixSizeType: TypeCode.Int32)]
    public IList<AttributiveItemInfo> AttributiveItems { get; init; } = [];
}