namespace Mozart.Metadata.Music;

public class AlbumListParser
{
    public static IReadOnlyDictionary<int, AlbumHeader> Parse(String filename)
    {
        return Parse(File.Open(filename, FileMode.Open));
    }

    public static IReadOnlyDictionary<int, AlbumHeader> Parse(Stream stream)
    {
        var headers = new Dictionary<int, AlbumHeader>();
        using var reader = new BinaryReader(stream);

        // Retrieve the number of songs in OJNList
        stream.Seek(0, SeekOrigin.Begin);
        int albumCount = reader.ReadInt32();

        // Retrieve all OJN Headers
        for (int i = 0; i < albumCount; i++)
        {
            var header = new AlbumHeader
            {
                ServerId = reader.ReadInt32(),
                AlbumId  = reader.ReadInt32(),
                Name     = reader.ReadBytes(64),
                MasterId = reader.ReadInt16(),
                Level    = reader.ReadInt16()
            };

            reader.ReadInt32(); // padding
            for (int j = 0; j < 10; j++)
            {
                int musicId = reader.ReadInt32();
                var diff    = (Difficulty)reader.ReadInt32();

                if (musicId == 0)
                    continue;

                header.Entries.Add(new AlbumHeader.MusicEntry
                {
                    Id = musicId,
                    Difficulty = diff
                });
            }

            headers.Add(header.AlbumId, header);
        }

        return headers;
    }
}
