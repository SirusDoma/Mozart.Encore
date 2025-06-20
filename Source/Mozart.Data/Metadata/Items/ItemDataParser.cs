using System.Text;

namespace Mozart.Metadata.Items;

public enum ItemDataFormat
{
    Old,
    New
}

public static class ItemDataParser
{
    public static IReadOnlyDictionary<int, ItemData> Parse(byte[] data, ItemDataFormat format = ItemDataFormat.Old)
    {
        var items = new Dictionary<int, ItemData>();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var defaultEncoding    = Encoding.UTF8;
        var identifierEncoding = Encoding.GetEncoding("EUC-KR");

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            var item = new ItemData
            {
                Id       = reader.ReadInt32(),
                ItemKind = (ItemKind) reader.ReadByte(),
                Origin   = (Planet) reader.ReadByte()
            };

            short bitflag         = reader.ReadInt16();
            item.Gender           = (Gender)((bitflag >> 7) & 15);
            item.IsNew            = (bitflag >> 11) == 1;
            item.Quantity         = reader.ReadByte(); // reader.ReadInt16();
            item.GameModifier     = (GameModifier) reader.ReadByte();
            item.GameModifierType = (GameModifierType) reader.ReadByte();
            item.Price.Currency   = (Currency)reader.ReadByte();
            item.Price.Gem        = reader.ReadInt32();
            item.Price.Point      = reader.ReadInt32();

            byte part = reader.ReadByte();
            if (part == 255)
            {
                item.ItemPart = item.ItemKind switch
                {
                    ItemKind.Body            => ItemPart.Body,
                    ItemKind.LeftArm         => ItemPart.LeftArm,
                    ItemKind.RightArm        => ItemPart.RightArm,
                    ItemKind.LeftHand        => ItemPart.LeftHand,
                    ItemKind.RightHand       => ItemPart.RightHand,
                    ItemKind.AttributiveItem => ItemPart.AttributiveItem,
                    _ => ItemPart.Body
                };
            }
            else
                item.ItemPart = (ItemPart)part;


            // O2KR Item Data
            // Special animated item, (similar to O2MO Costume)
            if (format == ItemDataFormat.New)
            {
                bool special = false;
                var specialGender = Gender.Any;

                if (reader.ReadInt32() == 10)
                {
                    special       = true;
                    specialGender = Gender.Male;
                }

                if (reader.ReadInt32() == 10)
                {
                    special       = true;
                    specialGender = specialGender == Gender.Male ? Gender.Any : Gender.Female;
                }

                if (special)
                    item.Special = new ItemSpecialAttribute {Gender = specialGender};
            }
            else
                item.Special = null;

            item.Name        = identifierEncoding.GetString(reader.ReadBytes(reader.ReadInt32()));
            item.Description = identifierEncoding.GetString(reader.ReadBytes(reader.ReadInt32()));

            foreach (ItemRenderPart renderPart in Enum.GetValues(typeof(ItemRenderPart)))
            {
                if (renderPart == ItemRenderPart.SmallPreview || renderPart == ItemRenderPart.LargePreview)
                {
                    bool valid = reader.ReadBoolean();
                    if (!valid)
                        continue;

                    var frame = new ItemRenderFrame
                    {
                        ItemRenderPart = renderPart,
                        Reference      = defaultEncoding.GetString(reader.ReadBytes(reader.ReadInt32())).Trim('\0')
                    };
                    item.RenderFrames.Add(frame);

                    continue;
                }

                foreach (Instrument instrument in Enum.GetValues(typeof(Instrument)))
                {
                    foreach (var gender in new[] {Gender.Male, Gender.Female})
                    {
                        bool valid  = reader.ReadBoolean();
                        if (!valid)
                            continue;

                        var frame = new ItemRenderFrame
                        {
                            ItemRenderPart = renderPart,
                            Instrument     = instrument,
                            Gender         = gender,
                            Reference      = defaultEncoding.GetString(reader.ReadBytes(reader.ReadInt32())).Trim('\0')
                        };

                        item.RenderFrames.Add(frame);
                    }
                }
            }

            items.Add(item.Id, item);
        }

        return items;
    }
}