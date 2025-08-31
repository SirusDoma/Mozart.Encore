using System.Text;

namespace Mozart.Metadata.Music;

public static class MusicListParser
{
    public static IReadOnlyList<MusicHeader> Parse(String filename)
    {
        return Parse(File.Open(filename, FileMode.Open));
    }

    public static IReadOnlyList<MusicHeader> Parse(Stream stream)
    {
        var headers = new List<MusicHeader>();
        byte[] inputData = new byte[0];

        using var reader = new BinaryReader(stream);

        // Retrieve the number of songs in OJNList
        stream.Seek(0, SeekOrigin.Begin);
        int songCount = reader.ReadInt32();

        // Detect version of OJNList
        bool newVersion = stream.Length > 4 + (songCount * 300) ? true : false;

        // Retrieve all OJN Headers
        for (int i = 0; i < songCount; i++)
            headers.Add(MusicHeaderParser.Parse(reader.ReadBytes(300)));

        // Parse the extra payload of new OJNList
        if (newVersion)
        {
            // Section #1 - Optional
            songCount = reader.ReadInt32();
            for (int i = 0; i < songCount; i++)
            {
                int id = reader.ReadInt32();
                int val1 = reader.ReadInt32();
                int val2 = reader.ReadInt32();
                int val3 = reader.ReadInt32();
            }

            // Section #2 - Mandatory
            songCount = reader.ReadInt32();
            for (int i = 0; i < songCount; i++)
            {
                int id = reader.ReadInt32();
                int state = reader.ReadInt32(); // 01
                int val1 = reader.ReadInt32();
                int val2 = reader.ReadInt32();
                int val3 = reader.ReadInt32();
            }

            // Section #3 - Optional
            songCount = reader.ReadInt32();
            for (int i = 0; i < songCount; i++)
            {
                int id = reader.ReadInt32();
                int val1 = reader.ReadInt32(); // 1000
                int val2 = reader.ReadInt32(); // 0
                int val3 = reader.ReadInt32(); // 2010252407
            }

            // Section #4 - Optional
            songCount = reader.ReadInt32();
            for (int i = 0; i < songCount; i++)
            {
                int id = reader.ReadInt32();
                int val1 = reader.ReadInt32(); // 0 if val2 1, otherwise 1
                int val2 = reader.ReadInt32(); // 1 if val1 0, otherwise 0
            }

            // Section #5 - Mandatory
            songCount = reader.ReadInt32();
            for (int i = 0; i < songCount; i++)
            {
                int id = reader.ReadInt32();
                int val1 = reader.ReadInt32(); // Values either 0, 3, 4, 5
            }

            // Section #6 - Optional
            songCount = reader.ReadInt32();
            for (int i = 0; i < songCount; i++)
            {
                int id = reader.ReadInt32();
                int val1 = reader.ReadInt32(); // 50
                int val2 = reader.ReadInt32(); // 0
            }

            // Section #7 - Optional (???? - not clue yet)
            songCount = reader.ReadInt32();

            // Section #8 - Mandatory (KeyMode)
            songCount = reader.ReadInt32();
            for (int i = 0; i < songCount; i++)
            {
                int id = reader.ReadInt32();
                if (id > 0 && headers.Any(h => h.Id == id))
                {
                    var header = headers[id];
                    reader.ReadByte(); //header.KeyMode = reader.ReadByte(); // either 3K, 5K or 7K
                    reader.ReadInt16(); // 17396
                    reader.ReadByte(); // padding?
                }
                else
                {
                    reader.ReadInt32(); // skip
                }
            }
        }

        return headers;
    }
}