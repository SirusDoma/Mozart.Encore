using System.CommandLine;

namespace Amadeus.CLI;


public interface ICommandLineTask
{
    static abstract string Name { get; }

    static abstract string Description { get; }

    void ConfigureCommand(Command command) {}

    int Execute()
        => 0;

    Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Execute());
}