using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net;

using Mozart.Options;

namespace Mozart.Web;

public class WebServer
{
    public static void Build(IWebHostBuilder builder)
    {
        builder
            .UseKestrel((context, server) =>
            {
                var options = context.Configuration
                    .GetSection(HttpOptions.Section)
                    .Get<HttpOptions>() ?? new HttpOptions();

                server.Listen(IPAddress.Parse(options.Address), options.Port);
            })
            .ConfigureServices((services) => {
                services.AddRouting();
                services.AddHealthChecks();
            })
            .Configure((app) =>
            {
                var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
                var logger = app.ApplicationServices.GetRequiredService<ILogger<WebServer>>();

                lifetime.ApplicationStarted.Register(() =>
                {
                    using (logger.BeginScope("System"))
                    {
                        logger.LogInformation("Web server started: Listening @ {address}",
                            app.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses);
                    }
                });

                app.UseRouting();
                app.UseEndpoints((endpoints) =>
                {
                    endpoints.MapHealthChecks("/healthz");
                    endpoints.MapGet("/ping", PingEndpoint.Get);
                    endpoints.MapPost("/authenticate", AuthenticateEndpoint.Post);
                    endpoints.MapPost("/register", RegisterEndpoint.Post);
                });
            });
    }
}
