using System.Text;

namespace Mozart.Metadata.Music;

public static class MusicListParser
{
    public static IReadOnlyDictionary<int, MusicHeader> Parse(String filename)
    {
        return Parse(File.Open(filename, FileMode.Open));
    }

    public static IReadOnlyDictionary<int, MusicHeader> Parse(Stream stream)
    {
        var headers = new Dictionary<int, MusicHeader>();
        byte[] inputData = new byte[0];

        using var reader = new BinaryReader(stream);

        // Retrieve the number of songs in OJNList
        stream.Seek(0, SeekOrigin.Begin);
        int songCount = reader.ReadInt32();

        // Retrieve all OJN Headers
        for (int i = 0; i < songCount; i++)
        {
            var header = MusicHeaderParser.Parse(reader.ReadBytes(300));
            headers.Add(header.Id, header);
        }

        // Check if OJNList is a new version of OJNList
        if (stream.Position == stream.Length)
            return headers;

        // Parse the extra payload of the new version of OJNList
        // TODO: Find out these parameters

        // -- Marker section
        // Mainly used to mark music as "New"
        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // The exact payload is unknown, however it is confirmed they are 3 integers, presumably act as flags

            // P1: Unknown, observed to be always 0
            int p1 = reader.ReadInt32();

            // P2: Unknown, sometimes 1, but otherwise 0
            int p2 = reader.ReadInt32();

            // P3: Unknown, sometimes 0, but otherwise 1
            int p3 = reader.ReadInt32();

            // Regardless of parameters, it is marked as new (even if all parameters are 0)
            if (headers.TryGetValue(id, out var header))
            {
                header.IsNew = newType < 2 || overrideNew != 0;
                header.IsPremiumNew = isNewPremium != 0;
            }
        }

        if (stream.Position == stream.Length)
            return headers;

        // -- Planet section
        // Controls song availability across different planet difficulty tiers.

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- defaultAvailability
            // Indicates availability in standard planets (i.e., not SuperEasy, Easy, or Beginner/Practice).
            int defaultAvailability = reader.ReadInt32();

            // -- superEasyAvailability
            // Indicates availability in SuperEasy planet.
            // SuperEasy planet is specifically made for 10 years old children or younger.
            int superEasyAvailability = reader.ReadInt32();

            // -- easyAvailability
            // Indicates availability in Easy planet.
            // Easy planet is specifically made for beginner adult.
            int easyAvailability = reader.ReadInt32();

            // -- fallbackAvailability
            // Override availability when the server is not SuperEasy, Easy or 3K Planet.
            int fallbackAvailability = reader.ReadInt32();
        }

        if (stream.Position == stream.Length)
            return headers;

        // -- Premium section
        // Mainly used to mark music as paid music and need to be acquired in music shop

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // The exact payload is unknown, however it is confirmed they are 3 integers, presumably prices

            // Price in Ep
            int point = reader.ReadInt32();

            // P2: Unknown, no occurrence other than 0
            //     Presumably to be price with secondary premium currency (e.g, O2Cash, MusicCash)
            int p2 = reader.ReadInt32();

            // P3: Unknown, no occurrence other than 0
            //     Presumably to be price with in-game currency (Gem)
            int p3 = reader.ReadInt32();

            // Regardless of parameters, it is marked as new (even if all parameters are 0)
            if (headers.TryGetValue(id, out var header))
            {
                header.IsPurchasable = true;
                header.PricePoint =  point;
            }
        }

        if (stream.Position == stream.Length)
            return headers;

        // -- VIP Exclusion section
        // Overrides premium behavior.

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- availability
            //  If non-zero, the song is free and playable in SuperEasy, music will not be part of VIP package.
            int availability = reader.ReadInt32();

            // -- Unused
            // Unused, observed value is always 0
            int unused = reader.ReadInt32();
        }

            // P2: Unknown,no occurrence other than 0
            int p2 = reader.ReadInt32();

        // -- Music Label section
        // Assigns a colored label (1–5) to each song

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- labelId
            // Labels are used across the game UI; both in the music selection screen and the music shop, displayed as filter tabs.
            //
            //   1. Gold   (골드) / 골드라벨: Best curated O2Jam songs
            //   2. Black  (블랙) / 블랙라벨: Songs for true O2Jam enthusiasts
            //   3. Blue   (블루) / 블루라벨: Best collection of popular songs (대중가요)
            //   4. Red    (레드) / 레드라벨: O2Jam self-produced k-pop songs (가요풍 자작곡)
            //   5. Silver (실버) / 실버라벨: Various other genres

            int labelId = reader.ReadInt32();
        }

        if (stream.Position == stream.Length)
            return headers;

        // -- Discount section
        // Defines discount percentages applied to premium songs.

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- O2Cash Discount
            // Discount percentage (e.g., 50 = 50% off).
            int cashDiscount = reader.ReadInt32();

            // -- Gem Discount
            // Discount percentage (e.g., 50 = 50% off).
            int gemDiscount = reader.ReadInt32();
        }

        if (stream.Position == stream.Length)
            return headers;

        // -- Unknown section
        // Stored partially inside the client, but has no reference, never being used
        // The actual OJNList.dat has song count set to 0.

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- Unknown
            int p1 = reader.ReadInt16();

            // -- Unknown
            int p2 = reader.ReadInt16();
        }

        if (stream.Position == stream.Length)
            return headers;

        // -- Key mode
        // Defines key mode of the music.
        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- Key Mode
            // Defines the key mode of the song, only one key mode can be active at a time per music entry.
            // 0x03 = 3K, 0x07 = 7K
            byte mode = reader.ReadByte();

            // -- Unused
            // Unused, observed value is always 244
            byte p2 = reader.ReadByte();

            // -- Unused
            // Unused, observed value is always 67
            short p3 = reader.ReadInt16();
        }

        if (stream.Position == stream.Length)
            return headers;

        // -- Release date section
        // Defines release date of a music

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- releaseDate
            // Formatted in "yyyy-MM-dd" and parsed into year/month/day int16 fields by splitting the string with the "-" token.
            // The int16 fields then used to sort the songs by this order: year -> month -> day -> music id
            string releaseDate = Encoding.UTF8.GetString(reader.ReadBytes(10)).Trim('\0');

            // -- Unknown
            // Unused, observed value is always 0
            int p2 = reader.ReadInt16();

            // -- Unknown
            // Unused, observed value is always 1243708 (0x12FABC)
            int p3 = reader.ReadInt32();

            // P4: Unknown, observed always to be `4540192` (`0x454720`)
            //     It might be possible that it is not int32
            int p4 = reader.ReadInt32();

            if (headers.TryGetValue(id, out var header))
                header.ReleaseDate = DateOnly.ParseExact(p1, "yyyy-MM-dd");
        }

        return headers;
    }
}
