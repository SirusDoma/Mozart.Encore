using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mozart.CLI;

public class CommandLineTaskProcessor
{
    private readonly IHostBuilder _builder;

    private IReadOnlyDictionary<string, CommandLineTasksBuilder.CommandLineTaskType> _types =
        new Dictionary<string, CommandLineTasksBuilder.CommandLineTaskType>();

    private CommandLineTaskProcessor(IHostBuilder builder)
    {
        _builder = builder;
    }

    public static CommandLineTaskProcessor CreateDefaultProcessor(IHostBuilder builder)
    {
        return new CommandLineTaskProcessor(builder);
    }

    public CommandLineTaskProcessor ConfigureCommandTasks(Action<ICommandLineTasksBuilder> configureDelegate)
    {
        var tasksBuilder = new CommandLineTasksBuilder();
        configureDelegate(tasksBuilder);

        _types = tasksBuilder.GetRegisteredTaskTypes();
        return this;
    }

    public bool IsExecutable(string[] args)
    {
        if (args.Length == 0)
            return false;

        string arg = args[0];
        if (arg is "--help" or "-h" or "-?" or "--version")
            return true;

        if (args.Length > 1 && args[0] == "--")
            return true;

        return !arg.StartsWith("--");
    }

    public async Task<int?> ExecuteAsync(string[] args)
    {
        if (!IsExecutable(args))
            return null;

        List<string> helps    = ["--help", "-h", "-?", "help"];
        List<string> versions = ["--version", "version"];

        var root = new RootCommand();

        var host = _builder.Build();
        foreach ((string name, var descriptor) in _types)
        {
            var task    = (ICommandLineTask)ActivatorUtilities.CreateInstance(host.Services, descriptor.Type);
            var command = new Command(name, descriptor.Description);

            task.ConfigureCommand(command);
            if (command.Action == null)
            {
                command.SetAction(async (_, cancellationToken) =>
                {
                    int exitCode = await task.ExecuteAsync(cancellationToken);
                    Environment.ExitCode = exitCode;
                });
            }

            if (versions.Any(v => v == name))
            {
                var option = root.Options.First(o => o.Name == "--version" || o.Aliases.Contains(name));
                option.Aliases.Add(name);
                option.Action = command.Action;
            }
            else
                root.Subcommands.Add(command);
        }

        return await root.Parse(args).InvokeAsync();
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
