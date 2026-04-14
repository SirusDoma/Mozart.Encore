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
        long remaining = stream.Length - stream.Position;
        if (remaining < sizeof(int))
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

            // P2: Unknown, observed to be always 0
            int p2 = reader.ReadInt32();

            // The following logic illustrate how the game treat these values:
            if (headers.TryGetValue(id, out var header))
            {
                header.IsNew = newType < 2 || overrideNew != 0;
                header.IsPremiumNew = isNewPremium != 0;
            }
        }

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

            // -- Unknown
            // Unused and always 0, presumably, a flag for Beginner / Practice Server
            int unused = reader.ReadInt32();
        }

        // -- Premium section
        // Defines pricing for songs. Removing a song from this section removes its premium status.

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id     = reader.ReadInt32();

            // -- o2Cash
            // Price in O2Cash currency.
            // A value of 0 results in a "Free" label.
            int o2Cash = reader.ReadInt32();

            // -- gem
            // Price in GEM currency.
            // Currently ignored in UI; O2Cash is always displayed instead.
            int gem = reader.ReadInt32();

            // -- Unused/Unknown
            // Unused, observed value is a constant value of 4379964 (0x42D53C)
            int unused = reader.ReadInt32();

            if (headers.TryGetValue(id, out var header))
            {
                header.IsPurchasable = o2Cash > 0 || gem > 0;
                header.PriceO2Cash   = o2Cash;
                header.PriceGem      = gem;
            }
        }

        // -- SuperEasy section
        // Overrides premium behavior specifically for the SuperEasy planet.

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- Unknown
            // Unused, observed value is always 0
            int unused = reader.ReadInt32();

            // -- availability
            // If non-zero, the song is free and playable in SuperEasy,
            // overriding any premium restrictions defined in the Premium section.
            int availability = reader.ReadInt32();
        }

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

        // -- Discount section
        // Defines discount percentages applied to premium songs.

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- Discount
            // Discount percentage (e.g., 50 = 50% off).
            int discount = reader.ReadInt32();
        }

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

            // Regardless of parameters, it is marked as new (even if all parameters are 0)
            if (headers.TryGetValue(id, out var header))
                header.IsNew = true;
        }

        // -- Mission section
        // Mainly used for mission mapping

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // Difficulty: which difficulty to play for this song mission
            var difficulty = (Difficulty)reader.ReadInt32();

            // P2: Unknown, no occurrence other than 0
            int p2 = reader.ReadInt32();

            // Level separated from the song level
            int level = reader.ReadInt32();

            if (headers.TryGetValue(id, out var header))
            {
                header.MissionDifficulty = difficulty;
                header.MissionLevel = level;
            }
        }

        // -- Extra metadata section
        // Mainly contain song release date

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // The exact payload is unknown, however it is confirmed they are:
            // - utf8 string timestamp (Format: yyyy-MM-dd)
            // - 12 bytes payload

            // P1: Unknown Date, presumably a music release date
            //     Appears to be always have 11 bytes in length
            //     But there's a chance that it is actually a null terminated string
            string p1 = Encoding.UTF8.GetString(reader.ReadBytes(11)).Trim('\0');

            // P2: Unknown, no occurrence other than 0
            byte p2 = reader.ReadByte();

            // P3: Unknown, no occurrence other than 0
            //     It might be possible that it is not int32
            int p3 = reader.ReadInt32();

            // P4: Unknown
            int p4 = reader.ReadInt32();

            // P5: Unknown
            //     It might be possible that it is not int32
            int p5 = reader.ReadInt32();

            if (headers.TryGetValue(id, out var header))
                header.ReleaseDate = DateOnly.ParseExact(p1, "yyyy-MM-dd");
        }

        return headers;
    }
}
