namespace Encore.Sessions;

public interface ISession
{
    IDictionary<string, object> Properties { get; }

    Task Execute(CancellationToken cancellationToken);

    Task WriteFrame(byte[] payload, CancellationToken cancellationToken);
}
