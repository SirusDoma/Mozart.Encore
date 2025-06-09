using System.Collections.ObjectModel;

namespace Encore.Server;

public sealed partial class CommandDispatcher
{
    private record ResponseFrame(object? Request, byte[] Payload);

    private class CommandResponse
    {
        public bool IsEmpty => Frames.Count == 0;

        public CommandResponse(IEnumerable<ResponseFrame> frames)
        {
            Frames = new ReadOnlyCollection<ResponseFrame>(frames.ToList());
        }

        public CommandResponse(params ResponseFrame[] frames)
        {
            Frames = new ReadOnlyCollection<ResponseFrame>(frames.ToList());
        }

        public ReadOnlyCollection<ResponseFrame> Frames { get; }

        public static CommandResponse Empty => new();

        public static CommandResponse Single(object? request, byte[] payload) =>
            new(new ResponseFrame(request, payload));
    }
}