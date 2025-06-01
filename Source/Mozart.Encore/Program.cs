using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Encore.Sessions;
using Encore.Hosting.Extensions;
using Encore.Messaging;
using Encore.Server;

using Mozart;

namespace Encore;

public class Program
{
    private static Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
            {
                // TODO: Implement config
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Server:Address"]              = "127.0.0.1",
                        ["Server:Port"]                 = "15010",
                        ["Server:MaxConnections"]       = ((int)SocketOptionName.MaxConnections).ToString(),
                        ["Server:PacketBufferSize"]     = "4096",
                        ["Session:Mode"]                = "Online",
                        ["Session:Db:Driver"]           = "SqlLite",
                        ["Session:Db:ConnectionString"] = "",
                        ["Db:Driver"]                   = "SqlLite",
                        ["Db:ConnectionString"]         = "",
                        ["Score:Gem"]                   = "1.0",
                        ["Score:Exp"]                   = "1.0"
                    })
                    .AddIniFile("config.ini", true, true)
                    .AddIniFile($"config.{context.HostingEnvironment}.ini", true, true)
                    .AddCommandLine(args);
            })
            .ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();

                builder.AddConsole()
                    .AddFilter("Microsoft.*", LogLevel.None);
            })
            .ConfigureFramer((context, builder) =>
            {
                builder.AddFramerFactory<SizePrefixedMessageFramer<short>>();
            })
            .ConfigureSessions((context, provider) =>
            {
                provider.UseSession<Session>()
                    .AddFactory<SessionFactory>()
                    .AddManager<SessionManager>();
            })
            .ConfigureFilters((context, builder) =>
            {
                builder.AddExceptionHandler<DefaultExceptionHandler>()
                    .AddExceptionLogger<DefaultExceptionLogger>();
            })
            .ConfigureRoutes((context, routes) =>
            {
                // Function-based routing: Safe with AOT
                // routes.UseCodec<DefaultMessageCodec>()
                //     .Map(async Task<AuthResponse> (Session session, AuthRequest request) =>
                //     {
                //         await Task.CompletedTask;
                //         return new AuthResponse
                //         {
                //             Result = AuthResult.Success,
                //             TimeBlockSubscription = new AuthResponse.TimeBlockSubscriptionInfo()
                //         };
                //     });

                // Controller-based routing: Not safe with AOT
                routes.UseCodec<DefaultMessageCodec>()
                    .Map<AuthController>()
                    .Map<PlanetController>()
                    .Map<MessagingController>()
                    .Map<MainRoomController>()
                    .Map<MyRoomController>()
                    .Map<ItemShopController>()
                    .Map<MusicShopController>()
                    .Map<WaitingController>()
                    .Map<PlayingController>();
            })
            .ConfigureServices((context, services) =>
            {
                // Configurations
                services.AddOptions<TcpOptions>()
                    .BindConfiguration(TcpOptions.Section);

                // Server
                services.AddHostedService<Worker>()
                    .AddTransient<IMessageCodec, DefaultMessageCodec>()
                    .AddTransient<ITcpServer, TcpServer>();

            })
            .Build();

        return host.RunAsync();
    }
}