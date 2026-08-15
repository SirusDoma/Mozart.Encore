using System.CommandLine;
using System.Text;
using Encore.CLI;
using Encore.Options;
using Microsoft.Extensions.Options;

namespace Memoryer.CLI;

public class ImportMetadataCommandTask(IOptions<MetadataOptions> metadataOptions) : ICommandLineTask
{
    public static string Name => "metadata:import";
    public static string Description => "Import metadata from an O2Jam installation";

    public void ConfigureCommand(Command command)
    {
        var directoryArgument = new Argument<string>("dir")
        {
            Description = "The supported O2Jam installation directory",
            DefaultValueFactory = _ => Environment.CurrentDirectory
        };
        command.Arguments.Add(directoryArgument);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            string directory = parseResult.GetRequiredValue(directoryArgument);
            Environment.ExitCode = await ExecuteAsync(directory, cancellationToken);
        });
    }

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use the overload of ExecuteAsync instead");
    }

    private async Task<int> ExecuteAsync(string directory, CancellationToken cancellationToken)
    {
        try
        {
            string gameDirectory = CommandLinePath.GetFullPath(directory);
            if (!Directory.Exists(gameDirectory))
            {
                Console.WriteLine($"O2Jam directory not found: {gameDirectory}");
                return 1;
            }

            string? imageDirectory = FindDirectory(gameDirectory, "Image");
            if (imageDirectory == null)
            {
                Console.WriteLine($"Image directory not found in: {gameDirectory}");
                return 1;
            }

            int imported = 0;

            MetadataFile? itemData = ReadItemData(imageDirectory);
            if (itemData is { } item)
            {
                await WriteAsync(metadataOptions.Value.ItemData, "Itemdata.dat", item, cancellationToken);
                imported++;
            }
            else
                Console.WriteLine("ItemData.dat is not found.");

            MetadataFile? musicList = ReadStandaloneFile(imageDirectory, "OJNList.dat");
            if (musicList is { } music)
            {
                await WriteAsync(metadataOptions.Value.MusicList, "OJNList.dat", music, cancellationToken);
                imported++;
            }
            else
                Console.WriteLine("OJNList.dat is not found.");

            MetadataFile? albumList = ReadArchiveFile(imageDirectory, "AlbumList.ojs", "Interface.opi", "Interface1.opi");
            if (albumList is { } album)
            {
                await WriteAsync(metadataOptions.Value.AlbumList, "AlbumList.ojs", album, cancellationToken);
                imported++;
            }
            else
                Console.WriteLine("AlbumList.ojs is not found.");

            return imported > 0 ? 0 : 1;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"Metadata import failed: {ex.Message}");
            return 1;
        }
    }

    private static async Task WriteAsync(string configuredPath, string defaultPath, MetadataFile source,
        CancellationToken cancellationToken)
    {
        string outputPath = Path.GetFullPath(string.IsNullOrWhiteSpace(configuredPath) ? defaultPath : configuredPath);
        string? outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        await File.WriteAllBytesAsync(outputPath, source.Data, cancellationToken);
        Console.WriteLine($"Imported {source.Source} -> {outputPath}");
    }

    private static MetadataFile? ReadItemData(string imageDirectory)
    {
        string? archivePath = FindArchive(imageDirectory, "Avatar.opa");
        if (archivePath != null)
        {
            ArchiveEntry? entry = ReadArchiveEntries(archivePath)
                .Select(entry => (Entry: entry, Priority: GetItemDataPriority(entry.Name)))
                .Where(candidate => candidate.Priority >= 0)
                .OrderBy(candidate => candidate.Priority)
                .ThenBy(candidate => candidate.Entry.Name, StringComparer.OrdinalIgnoreCase)
                .Select(candidate => (ArchiveEntry?)candidate.Entry)
                .FirstOrDefault();

            if (entry is { } itemEntry)
                return ReadArchiveEntry(archivePath, itemEntry);
        }

        return null;
    }

    private static int GetItemDataPriority(string path)
    {
        string name = GetEntryFileName(path);
        if (name.Equals("Itemdata.dat", StringComparison.OrdinalIgnoreCase))
            return 0;

        string stem = Path.GetFileNameWithoutExtension(name);
        if (Path.GetExtension(name).Equals(".dat", StringComparison.OrdinalIgnoreCase) &&
            stem.StartsWith("ItemData_", StringComparison.OrdinalIgnoreCase))
            return 1;

        return -1;
    }

    private static MetadataFile? ReadStandaloneFile(string directory, string name)
    {
        string? path = Directory.EnumerateFiles(directory)
            .FirstOrDefault(path => Path.GetFileName(path).Equals(name, StringComparison.OrdinalIgnoreCase));
        return path == null ? null : new MetadataFile(Path.GetFileName(path), File.ReadAllBytes(path));
    }

    private static MetadataFile? ReadArchiveFile(string directory, string entryName, params string[] archiveNames)
    {
        string? archivePath = FindArchive(directory, archiveNames);
        if (archivePath == null)
            return null;

        ArchiveEntry? entry = ReadArchiveEntries(archivePath)
            .Where(entry => GetEntryFileName(entry.Name).Equals(entryName, StringComparison.OrdinalIgnoreCase))
            .Select(entry => (ArchiveEntry?)entry)
            .FirstOrDefault();
        return entry is { } value ? ReadArchiveEntry(archivePath, value) : null;
    }

    private static string? FindArchive(string directory, params string[] names)
    {
        foreach (string name in names)
        {
            string? path = Directory.EnumerateFiles(directory)
                .FirstOrDefault(path => Path.GetFileName(path).Equals(name, StringComparison.OrdinalIgnoreCase));
            if (path != null)
                return path;
        }

        return null;
    }

    private static string? FindDirectory(string directory, string name)
    {
        return Directory.EnumerateDirectories(directory)
            .FirstOrDefault(path => Path.GetFileName(path).Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<ArchiveEntry> ReadArchiveEntries(string archivePath)
    {
        using var stream = File.OpenRead(archivePath);
        using var reader = new BinaryReader(stream, Encoding.ASCII, true);

        if (stream.Length < 16)
            throw new InvalidDataException($"Invalid OPI/OPA archive: {archivePath}");

        int signature = reader.ReadInt32();
        int count = reader.ReadInt32();
        int expectedSignature = Path.GetExtension(archivePath).Equals(".opa", StringComparison.OrdinalIgnoreCase)
            ? 1
            : 2;
        if (signature != expectedSignature || count < 0)
            throw new InvalidDataException($"Invalid OPI/OPA archive: {archivePath}");

        long tableSize = checked((long)count * 152);
        long tableOffset = stream.Length - tableSize;
        if (tableOffset < 16)
            throw new InvalidDataException($"Invalid OPI/OPA header table: {archivePath}");

        stream.Position = tableOffset;
        var entries = new List<ArchiveEntry>(count);
        for (int i = 0; i < count; i++)
        {
            int fileSignature = reader.ReadInt32();
            byte[] nameBytes = reader.ReadBytes(128);
            int end = Array.IndexOf(nameBytes, (byte)0);
            string name = Encoding.ASCII.GetString(nameBytes, 0, end < 0 ? nameBytes.Length : end).Trim();
            int offset = reader.ReadInt32();
            int size = reader.ReadInt32();
            _ = reader.ReadInt32();
            _ = reader.ReadInt32();
            _ = reader.ReadInt32();

            if (fileSignature != 1)
                continue;

            if (offset < 16 || size < 0 || (long)offset + size > tableOffset)
                throw new InvalidDataException($"Invalid archive entry '{name}' in {archivePath}");

            entries.Add(new ArchiveEntry(name, offset, size));
        }

        return entries;
    }

    private static MetadataFile ReadArchiveEntry(string archivePath, ArchiveEntry entry)
    {
        using var stream = File.OpenRead(archivePath);
        stream.Position = entry.Offset;
        var data = new byte[entry.Size];
        stream.ReadExactly(data);
        return new MetadataFile($"{Path.GetFileName(archivePath)}:{entry.Name}", data);
    }

    private static string GetEntryFileName(string path) => Path.GetFileName(path.Replace('\\', '/'));

    private readonly record struct ArchiveEntry(string Name, int Offset, int Size);
    private readonly record struct MetadataFile(string Source, byte[] Data);
}
