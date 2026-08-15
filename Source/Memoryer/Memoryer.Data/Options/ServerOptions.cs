namespace Mozart.Options;

public enum DeploymentMode
{
    Full,
    Gateway,
    Channel,
    Relay
}

public class ServerOptions
{
    public const string Section = "Server";

    public DeploymentMode Mode { get; init; } = DeploymentMode.Full;
}
