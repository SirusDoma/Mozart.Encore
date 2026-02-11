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

            // P2: Unknown, sometimes 1, but otherwise 0
            int p2 = reader.ReadInt32();

            // P3: Unknown, sometimes 0, but otherwise 1
            int p3 = reader.ReadInt32();

            // Regardless of parameters, it is marked as new (even if all parameters are 0)
            if (headers.TryGetValue(id, out var header))
                header.IsNew = true;
        }

        // -- Premium section
        // Mainly used to mark music as paid music and need to be acquired in music shop

        songCount = reader.ReadInt32();
        for (int i = 0; i < songCount; i++)
        {
            int id = reader.ReadInt32();

            // The exact payload is unknown, however it is confirmed they are 3 integers, presumably prices

            // P1: Unknown, observed to be either 10 or 20 most of the time, but there are other values as well
            //     Presumably to be price with primary premium currency (e.g, e-Point, MCash, etc)
            int p1 = reader.ReadInt32();

            // P2: Unknown, no occurrence other than 0
            //     Presumably to be price with secondary premium currency (e.g, O2Cash, MusicCash)
            int p2 = reader.ReadInt32();

            // P3: Unknown, no occurrence other than 0
            //     Presumably to be price with in-game currency (Gem)
            int p3 = reader.ReadInt32();

            // Regardless of parameters, it is marked as new (even if all parameters are 0)
            if (headers.TryGetValue(id, out var header))
                header.IsPurchasable = true;
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

            // P2: Unknown,no occurrence other than 0
            int p2 = reader.ReadInt32();

            // P3: Unknown, observed always to be `1243692` (`0x12FA2C`)
            //     It might be possible that it is not int32
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
