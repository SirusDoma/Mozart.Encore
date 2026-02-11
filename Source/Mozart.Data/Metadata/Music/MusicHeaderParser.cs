using System.Text;

namespace Mozart.Metadata.Music;

public static class MusicHeaderParser
{
    public static bool Validate(string filename)
    {
        return Validate(File.Open(filename, FileMode.Open));
    }

    public static bool Validate(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);
        string encryptSign = Encoding.UTF8.GetString(reader.ReadBytes(3));

        stream.Seek(4, SeekOrigin.Begin);
        string fileSign = Encoding.UTF8.GetString(reader.ReadBytes(3));

        return encryptSign == "new" || fileSign == "ojn";
    }

    public static bool IsEncrypted(string filename)
    {
        return IsEncrypted(File.Open(filename, FileMode.Open));
    }

    public static bool IsEncrypted(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);
        string encryptSign = Encoding.UTF8.GetString(reader.ReadBytes(3));

        return encryptSign == "new";
    }

    public static MusicHeader Parse(string filename)
    {
        return Parse(File.Open(filename, FileMode.Open));
    }

    public static MusicHeader Parse(Stream stream)
    {
        byte[] inputData;
        bool encrypted = false;

        using var reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string encryptSign = Encoding.UTF8.GetString(reader.ReadBytes(3));
        if (encryptSign == "new")
        {
            inputData = Decrypt(stream);
            encrypted = true;
        }
        else
        {
            stream.Seek(0, SeekOrigin.Begin);
            inputData = reader.ReadBytes((int)stream.Length);
        }

        var header = Parse(inputData);
        header.Encrypted = encrypted;

        return header;
    }

    public static MusicHeader Parse(byte[] inputData)
    {
        using var mstream = new MemoryStream(inputData);
        using var reader = new BinaryReader(mstream);
        var header = new MusicHeader
        {
            Id = reader.ReadInt32(),
            Signature = reader.ReadBytes(4),
            EncodingVersion = reader.ReadSingle(),
            Genre = (Genre)reader.ReadInt32(),
            Bpm = reader.ReadSingle(),
            LevelEx = reader.ReadInt16(),
            LevelNx = reader.ReadInt16(),
            LevelHx = reader.ReadInt16(),
            Padding = reader.ReadInt16(),
            EventCountEx = reader.ReadInt32(),
            EventCountNx = reader.ReadInt32(),
            EventCountHx = reader.ReadInt32(),
            NoteCountEx = reader.ReadInt32(),
            NoteCountNx = reader.ReadInt32(),
            NoteCountHx = reader.ReadInt32(),
            MeasureCountEx = reader.ReadInt32(),
            MeasureCountNx = reader.ReadInt32(),
            MeasureCountHx = reader.ReadInt32(),
            BlockCountEx = reader.ReadInt32(),
            BlockCountNx = reader.ReadInt32(),
            BlockCountHx = reader.ReadInt32(),
            OldEncodingVersion = reader.ReadInt16(),
            OldSongId = reader.ReadInt16(),
            OldGenre = reader.ReadBytes(20),
            ThumbnailSize = reader.ReadInt32(),
            FileVersion = reader.ReadInt32(),
            Title = reader.ReadBytes(64),
            Artist = reader.ReadBytes(32),
            NoteDesigner = reader.ReadBytes(32),
            OJM = Encoding.UTF8.GetString(reader.ReadBytes(32)).Trim('\0'),
            CoverSize = reader.ReadInt32(),
            DurationEx = reader.ReadInt32(),
            DurationNx = reader.ReadInt32(),
            DurationHx = reader.ReadInt32(),
            BlockOffsetEx = reader.ReadInt32(),
            BlockOffsetNx = reader.ReadInt32(),
            BlockOffsetHx = reader.ReadInt32(),
            CoverOffset = reader.ReadInt32()
        };

        return header;
    }

    public static byte[] Decrypt(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.Unicode, true);
        stream.Seek(0, SeekOrigin.Begin);
        byte[] input = reader.ReadBytes((int)stream.Length);

        stream.Seek(3, SeekOrigin.Begin);
        byte blockSize = reader.ReadByte();
        byte mainKey = reader.ReadByte();
        byte midKey = reader.ReadByte();
        byte initialKey = reader.ReadByte();

        byte[] encryptKeys = Enumerable.Repeat(mainKey, blockSize).ToArray();
        encryptKeys[0] = initialKey;
        encryptKeys[(int)Math.Floor(blockSize / 2f)] = midKey;

        byte[] output = new byte[stream.Length - stream.Position];
        for (int i = 0; i < output.Length; i += blockSize)
        {
            for (int j = 0; j < blockSize; j++)
            {
                int offset = i + j;
                if (offset >= output.Length)
                {
                    return output;
                }

                output[offset] = (byte)(input[^(offset + 1)] ^ encryptKeys[j]);
            }
        }

        return output;
    }
}
