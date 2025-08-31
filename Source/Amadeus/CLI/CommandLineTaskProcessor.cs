using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Amadeus.CLI;

public class CommandLineTaskProcessor
{
    private readonly string[] _args;
    private readonly IHostBuilder _builder;

    private IReadOnlyDictionary<string, CommandLineTasksBuilder.CommandLineTaskType> _types =
        new Dictionary<string, CommandLineTasksBuilder.CommandLineTaskType>();

    private CommandLineTaskProcessor(IHostBuilder builder, string[] args)
    {
        _builder = builder;
        _args    = args;
    }

    public static CommandLineTaskProcessor CreateDefaultProcessor(IHostBuilder builder, string[] args)
    {
        return new CommandLineTaskProcessor(builder, args);
    }

    public CommandLineTaskProcessor ConfigureCommandTasks(Action<ICommandLineTasksBuilder> configureDelegate)
    {
        var tasksBuilder = new CommandLineTasksBuilder();
        configureDelegate(tasksBuilder);

        _types = tasksBuilder.GetRegisteredTaskTypes();
        return this;
    }

    public bool Processable()
    {
        if (_args.Length == 0)
            return false;

        string arg = _args[0];
        if (arg is "--help" or "-h" or "-?" or "--version")
            return true;

        if (_args.Length > 1 && _args[0] == "--")
            return true;

        return !arg.StartsWith("--");
    }

    public async Task<int?> ProcessAsync()
    {
        if (!Processable())
            return null;

        List<string> helps    = ["--help", "-h", "-?", "help"];
        List<string> versions = ["--version", "version"];

        var root = new CommandLineBuilder()
            .UseDefaults();

        var host = _builder.Build();
        foreach ((string name, var descriptor) in _types)
        {
            var task    = (ICommandLineTask)ActivatorUtilities.CreateInstance(host.Services, descriptor.Type);
            var command = new Command(name, descriptor.Description);

            task.ConfigureCommand(command);
            if (command.Handler == null)
            {
                command.SetHandler(async () =>
                {
                    int exitCode = await task.ExecuteAsync(CancellationToken.None);
                    Environment.ExitCode = exitCode;
                });
            }

            if (versions.Any(v => v == name))
            {
                root.UseVersionOption(versions.ToArray())
                    .AddMiddleware(async void (ctx) =>
                    {
                        if (ctx.ParseResult.Tokens.Any(t => versions.Contains(t.Value.ToLowerInvariant())))
                            Environment.Exit(await task.ExecuteAsync());

                    }, MiddlewareOrder.ExceptionHandler);
            }
            else
                root.Command.Add(command);
        }

        return await root.Build().InvokeAsync(_args);
    }

}

public interface ICommandLineTasksBuilder
{
    CommandLineTasksBuilder AddCommandLineTask<T>()
        where T : ICommandLineTask;
}

public class CommandLineTasksBuilder : ICommandLineTasksBuilder
{
    private readonly Dictionary<string, CommandLineTaskType> _types = new();

    public class CommandLineTaskType
    {
        public required string Name        { get; init; }
        public required string Description { get; init; }
        public required Type   Type        { get; init; }
    }

    public CommandLineTasksBuilder()
    {
    }

    public CommandLineTasksBuilder AddCommandLineTask<T>()
        where T : ICommandLineTask
    {
        _types.Add(T.Name, new CommandLineTaskType()
        {
            Name = T.Name,
            Description = T.Description,
            Type = typeof(T)
        });
        return this;
    }

    public IReadOnlyDictionary<string, CommandLineTaskType> GetRegisteredTaskTypes() => _types;
} 