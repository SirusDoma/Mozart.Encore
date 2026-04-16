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

        // -- New Marker section
        // Defines which songs are marked as "New" and whether they require a special subscription.

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // -- isNewPremium
            // Indicates whether this "New" song is restricted to a special subscription tier.
            // Subscription type 0x04 unlock access to all songs in this section.
            // Subscription types 0x01 and 0x40 also unlock these songs, along with all other content.
            int isNewPremium = reader.ReadInt32();

            // -- MAYBE: newType
            // Determines "New" status using a threshold:
            //   - Value < 2  => treated as "New"
            //   - Value >= 2 => treated as not "New" (unless overridden)
            // Likely an enum or state flag rather than a strict boolean.
            // No other usages observed beyond this comparison.
            int newType = reader.ReadInt32();

            // -- overrideNew
            // Acts as an override flag for "New" status.
            // If non-zero, the song is always treated as "New", regardless of newType.
            // Behavior strongly suggests a boolean (non-zero = true).
            int overrideNew = reader.ReadInt32();

            // The following logic illustrate how the game treat these values:
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

        if (stream.Position == stream.Length)
            return headers;

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

            // -- Unknown
            // Unused, observed value is always 4655611 (0x470DFB)
            int p4 = reader.ReadInt32();

            // -- Unknown
            // Unused, observed value is always 0
            int p5 = reader.ReadInt32();

            if (headers.TryGetValue(id, out var header))
                header.ReleaseDate = DateOnly.ParseExact(releaseDate, "yyyy-MM-dd");
        }

        return headers;
    }
}
