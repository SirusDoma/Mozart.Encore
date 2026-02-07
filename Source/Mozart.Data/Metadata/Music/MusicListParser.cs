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
           // TODO: Implement parsing extra metadata
        }

        return headers;
    }
}