using System.Text;

namespace Encore.Metadata.Items;

public enum ItemDataFormat
{
    Beta,     // O2Jam v2.93: length-only render frames, no Back part, frame block gated by an int32 flag
    Original, // O2Jam v3.10: item quantity is a single byte
    Nx,       // O2Jam NX and later: item quantity widened to 2 bytes
    Classic   // O2Jam Classic (O2KR): NX layout plus the special animated item block
}

public static class ItemDataParser
{
    public static IReadOnlyDictionary<int, ItemData> Parse(byte[] data)
    {
        return Parse(data, DetectFormat(data));
    }

    private static ItemDataFormat DetectFormat(byte[] data)
    {
        return Array.FindIndex(data, 12, b => b != 0) switch
        {
            4 + 33 => ItemDataFormat.Beta,
            4 + 71 => ItemDataFormat.Original,
            4 + 72 => ItemDataFormat.Nx,
            4 + 80 => ItemDataFormat.Classic,
            _      => throw new InvalidDataException("Unrecognized item data format")
        };
    }

    public static IReadOnlyDictionary<int, ItemData> Parse(byte[] data, ItemDataFormat format)
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
            item.Quantity         = format is ItemDataFormat.Beta or ItemDataFormat.Original ? reader.ReadByte() : reader.ReadInt16();
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
            if (format == ItemDataFormat.Classic)
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

            if (format == ItemDataFormat.Beta && reader.ReadInt32() == 0)
            {
                items.Add(item.Id, item);
                continue;
            }

            foreach (ItemRenderPart renderPart in Enum.GetValues(typeof(ItemRenderPart)))
            {
                if (format == ItemDataFormat.Beta && renderPart == ItemRenderPart.Back)
                    continue;

                if (renderPart is ItemRenderPart.SmallPreview or ItemRenderPart.LargePreview)
                {
                    if (!TryReadReference(reader, format, defaultEncoding, out string reference))
                        continue;

                    var frame = new ItemRenderFrame
                    {
                        ItemRenderPart = renderPart,
                        Reference      = reference
                    };
                    item.RenderFrames.Add(frame);

                    continue;
                }

                foreach (Instrument instrument in Enum.GetValues(typeof(Instrument)))
                {
                    foreach (var gender in new[] {Gender.Male, Gender.Female})
                    {
                        if (!TryReadReference(reader, format, defaultEncoding, out string reference))
                            continue;

                        var frame = new ItemRenderFrame
                        {
                            ItemRenderPart = renderPart,
                            Instrument     = instrument,
                            Gender         = gender,
                            Reference      = reference
                        };

                        item.RenderFrames.Add(frame);
                    }
                }
            }

            items.Add(item.Id, item);
        }

        return items;
    }

    private static bool TryReadReference(BinaryReader reader, ItemDataFormat format, Encoding encoding, out string reference)
    {
        reference = string.Empty;
        if (format != ItemDataFormat.Beta && !reader.ReadBoolean())
            return false;

        byte[] bytes = reader.ReadBytes(reader.ReadInt32());
        if (format == ItemDataFormat.Beta && bytes.Length == 0)
            return false;

        reference = encoding.GetString(bytes).Trim('\0');
        return true;
    }
}
