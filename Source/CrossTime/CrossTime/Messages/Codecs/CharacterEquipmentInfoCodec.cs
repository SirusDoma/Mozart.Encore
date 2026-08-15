using Encore.Messaging;
using Encore.Metadata.Items;
using Mozart.Data.Entities;

namespace CrossTime.Messages.Codecs;

public class CharacterEquipmentInfoCodec : MessageFieldCodec
{
    public CharacterEquipmentInfoCodec(IMessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value, Type sourceType)
    {
        if (value is not Dictionary<ItemType, int> equipmentTable)
            throw new ArgumentOutOfRangeException(nameof(value));

        foreach (var type in EquipmentItems.SupportedTypes)
        {
            if (equipmentTable.TryGetValue(type, out int id))
                writer.Write(id);
            else
                writer.Write((int)0);
        }
    }

    public override object Decode(BinaryReader reader, Type targetType)
    {
        var equipmentTable = new Dictionary<ItemType, int>();
        foreach (var type in EquipmentItems.SupportedTypes)
            equipmentTable[type] = reader.ReadInt32();

        return equipmentTable;
    }
}
