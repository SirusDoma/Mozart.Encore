﻿using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Encore.Hosting.Extensions;
using Encore.Hosting.Logging;
using Encore.Messaging;
using Encore.Server;

using Mozart.Controllers;
using Mozart.Controllers.Internal;
using Mozart.Controllers.Filters;
using Mozart.Data.Contexts;
using Mozart.Data.Repositories;
using Mozart.Options;
using Mozart.Contexts;
using Mozart.Services;
using Mozart.CLI;
using Mozart.Entities;
using Mozart.Events;
using Mozart.Sessions;
using Mozart.Workers.Channels;
using Mozart.Workers.Gateway;

namespace Mozart;

public class Program
{
    public static Version Version        => new(1, 1, 0);
    public static Version NetworkVersion => new(3, 10);
    public static string RepositoryUrl   => "https://github.com/SirusDoma/Mozart.Encore";

    private static async Task<int> Main(string[] args)
    {
        var hostBuilder = CreateHostBuilder(args);

        // Check for custom command
        int? code = await ProcessCommandLine(hostBuilder, args);
        if (code != null)
            return code.Value;

        // Otherwise configure the rest of server and start it
        var host = hostBuilder
            .ConfigureFramer((context, builder) =>
            {
                builder.AddFramerFactory<SizePrefixedMessageFramer<short>>();
            })
            .ConfigureSessions((context, provider) =>
            {
                provider.UseSession<Session>()
                    .AddFactory<ISessionFactory>(svc => new SessionFactory(svc))
                    .AddManager<ISessionManager>(_ => new SessionManager());
            })
            .ConfigureFilters((context, builder) =>
            {
                var options = context.Configuration
                    .GetSection(ServerOptions.Section)
                    .Get<ServerOptions>() ?? new ServerOptions();

                builder.AddExceptionHandler<DefaultExceptionHandler>()
                    .AddExceptionLogger<DefaultExceptionLogger>()
                    .AddFilter<SessionScopeLoggerFilter>();

                if (options.Mode == DeploymentMode.Gateway)
                    builder.AddFilter<GatewayFilter>();
            })
            .ConfigureRoutes((context, provider) =>
            {
                var options = context.Configuration
                    .GetSection(ServerOptions.Section)
                    .Get<ServerOptions>() ?? new ServerOptions();

                // Controller-based routing: Not safe with AOT
                var routes = provider.UseCodec<DefaultMessageCodec>()
                    .Map<AuthController>()
                    .Map<PlanetController>()
                    .Map<MessagingController>()
                    .Map<MainRoomController>()
                    .Map<MyRoomController>()
                    .Map<ItemShopController>()
                    .Map<MusicShopController>()
                    .Map<WaitingController>()
                    .Map<PlayingController>();

                switch (options.Mode)
                {
                    case DeploymentMode.Gateway:
                        routes.Map<GatewayController>(c => c.AddFilter<InternalLoggerFilter>());
                        break;
                    case DeploymentMode.Channel:
                        routes.Map<ChannelController>(c => c.AddFilter<InternalLoggerFilter>());
                        break;
                }
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IMozartServer, MozartServer>();

                var options = context.Configuration
                    .GetSection(ServerOptions.Section)
                    .Get<ServerOptions>() ?? new ServerOptions();

                switch (options.Mode)
                {
                    case DeploymentMode.Channel:
                        services.AddSingleton<IGatewayClient, GatewayClient>()
                            .AddSingleton<IUserSessionFactory, UserSessionFactory>()
                            .AddHostedService<ChannelWorker>();

                        break;
                    case DeploymentMode.Gateway:
                        services.AddSingleton<IClientServer, ClientServer>()
                            .AddSingleton<IGatewayServer, GatewayServer>()
                            .AddSingleton<IChannelAggregator, ChannelAggregator>()
                            .AddSingleton<IClientSessionFactory, ClientSessionFactory>()
                            .AddSingleton<IChannelSessionFactory, ChannelSessionFactory>()
                            .AddSingleton<IChannelSessionManager, ChannelSessionManager>()
                            .AddHostedService<GatewayWorker>();

                        break;
                    case DeploymentMode.Full:
                        services.AddHostedService<DefaultWorker>();
                        break;
                }
            })
            .Build();

        await host.RunAsync();
        return 0;
    }

    private static async Task<int?> ProcessCommandLine(IHostBuilder hostBuilder, string[] args)
    {
        return await CommandLineTaskProcessor.CreateDefaultProcessor(hostBuilder, args)
            .ConfigureCommandTasks(builder =>
            {
                builder.AddCommandLineTask<DatabaseInitCommandTask>()
                    .AddCommandLineTask<RegisterUserCommandTask>()
                    .AddCommandLineTask<AuthorizeUserCommandTask>()
                    .AddCommandLineTask<VersionCommandTask>();
            })
            .ProcessAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Server:Mode"]                 = "Full",
                        ["Server:Address"]              = "127.0.0.1",
                        ["Server:Port"]                 = "15010",
                        ["Server:MaxConnections"]       = ((int)SocketOptionName.MaxConnections).ToString(),
                        ["Server:PacketBufferSize"]     = "4096",
                        ["Db:Driver"]                   = "Sqlite",
                        ["Db:Name"]                     = "O2JAM",
                        ["Db:Url"]                      = "Data Source=O2JAM.db",
                        ["Game:AllowSoloInVersus"]      = "true",
                        ["Game:SingleModeRewardLevelLimit"] = "10",
                        ["Metadata:ItemData"]           = "Itemdata.dat",
                        ["Gateway:Channels:0:Id"]       = "0",
                        ["Auth:Mode"]                   = "Default",
                        ["Auth:RevokeOnStartp"]         = "true",
                        ["Score:Gem"]                   = "1.0",
                        ["Score:Exp"]                   = "1.0"
                    })
                    .AddIniFile("config.ini", true, true)
                    .AddIniFile($"config.{context.HostingEnvironment}.ini", true, true)
                    .AddIniFile("mozart.ini", true, true)
                    .AddCommandLine(args);
            })
            .ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders()
                    .AddConsole(options => options.FormatterName = "EncoreLoggerFormatter")
                    .AddConsoleFormatter<EncoreConsoleFormatter, EncoreConsoleFormatterOptions>()
                    .AddFilter("Microsoft.*", LogLevel.None)
                    .SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureServices((context, services) =>
            {
                // Configurations
                services.AddOptions<MetadataOptions>()
                    .BindConfiguration(MetadataOptions.Section);
                services.AddOptions<DatabaseOptions>()
                    .BindConfiguration(DatabaseOptions.Section);
                services.AddOptions<TcpOptions>()
                    .BindConfiguration(TcpOptions.Section);
                services.AddOptions<ServerOptions>()
                    .BindConfiguration(ServerOptions.Section);
                services.AddOptions<GatewayOptions>()
                    .BindConfiguration(GatewayOptions.Section);
                services.AddOptions<AuthOptions>()
                    .BindConfiguration(AuthOptions.Section);
                services.AddOptions<GameOptions>()
                    .BindConfiguration(GameOptions.Section);

                // Database contexts
                services.AddDbContextFactory<UserDbContext>((provider, builder) =>
                {
                    var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                    _ = options.Driver switch
                    {
                        DatabaseDriver.Memory =>
                            builder.UseInMemoryDatabase(options.Name),

                        DatabaseDriver.Sqlite =>
                            builder.UseSqlite(options.Url, ctx =>
                            {
                                ctx.MigrationsAssembly("Mozart.Migrations.Sqlite");
                                if (options.CommandTimeout != null)
                                    ctx.CommandTimeout(options.CommandTimeout.Value);

                                if (options.MaxBatchSize != null)
                                    ctx.MaxBatchSize(options.MaxBatchSize.Value);

                                if (options.MinBatchSize != null)
                                    ctx.MinBatchSize(options.MinBatchSize.Value);
                            }),

                        DatabaseDriver.SqlServer =>
                            builder.UseSqlServer(options.Url, ctx =>
                            {
                                ctx.MigrationsAssembly("Mozart.Migrations.SqlServer");
                                if (options.CommandTimeout != null)
                                    ctx.CommandTimeout(options.CommandTimeout.Value);

                                if (options.MaxBatchSize != null)
                                    ctx.MaxBatchSize(options.MaxBatchSize.Value);

                                if (options.MinBatchSize != null)
                                    ctx.MinBatchSize(options.MinBatchSize.Value);
                            }),

                        DatabaseDriver.MySql =>
                            builder.UseMySql(options.Url, ServerVersion.AutoDetect(options.Url), ctx =>
                            {
                                ctx.MigrationsAssembly("Mozart.Migrations.MySql");
                                if (options.CommandTimeout != null)
                                    ctx.CommandTimeout(options.CommandTimeout.Value);

                                if (options.MaxBatchSize != null)
                                    ctx.MaxBatchSize(options.MaxBatchSize.Value);

                                if (options.MinBatchSize != null)
                                    ctx.MinBatchSize(options.MinBatchSize.Value);
                            }),

                        DatabaseDriver.Postgres =>
                            builder.UseNpgsql(options.Url, ctx =>
                            {
                                ctx.MigrationsAssembly("Mozart.Migrations.Postgres");
                                if (options.CommandTimeout != null)
                                    ctx.CommandTimeout(options.CommandTimeout.Value);

                                if (options.MaxBatchSize != null)
                                    ctx.MaxBatchSize(options.MaxBatchSize.Value);

                                if (options.MinBatchSize != null)
                                    ctx.MinBatchSize(options.MinBatchSize.Value);
                            }),

                        DatabaseDriver.MongoDb =>
                            builder.UseMongoDB(options.Url, options.Name),

                        _ => throw new ArgumentOutOfRangeException(nameof(options), options.Driver, "Invalid driver")
                    };
                });

                // Repositories
                services.AddScoped<IUserRepository, UserRepository>()
                    .AddScoped<ICredentialRepository, CredentialRepository>()
                    .AddScoped<ISessionRepository, SessionRepository>();

                // Application contexts
                services.AddScoped<IAuthContext, AuthContext>();

                // Event subscribers
                // Note: The subscriber lifetime is mostly tied to the object that it's subscribing
                //       (e.g, Use singleton when subscribing events from singleton service)
                services.AddScoped<IEventPublisher<Room>, RoomEventPublisher>();
                services.AddScoped<IEventPublisher<ScoreTracker>, ScoreTrackerEventPublisher>();
                services.AddSingleton<IEventPublisher<RoomService>, RoomServiceEventPublisher>();

                // Services
                services.AddSingleton<IMetadataResolver, MetadataResolver>()
                    .AddSingleton<IChannelService, ChannelService>()
                    .AddSingleton<IRoomService, RoomService>()
                    .AddScoped<IIdentityService, IdentityService>();
            });
    }
}