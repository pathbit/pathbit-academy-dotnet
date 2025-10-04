namespace ViaCepLogger.Api.Extensions;

public static class SerilogExtensions
{
    public static WebApplication UseDefaultSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "Unknown");
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown");
            };
        });

        return app;
    }

    public static IHostBuilder UseDefaultSerilog(this IHostBuilder hostBuilder)
    {
        const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Application} {Environment} {MachineName} {ThreadId} {Message:lj}{NewLine}{Exception}";

        return hostBuilder.UseSerilog((context, _, configuration) =>
        {
            var seqSection = context.Configuration.GetSection("Seq");
            var seqServerUrl = seqSection["ServerUrl"] ?? "http://localhost:5341";
            var seqApiKey = seqSection["ApiKey"];
            var applicationName = seqSection["ApplicationName"]
                                   ?? context.HostingEnvironment.ApplicationName
                                   ?? "ViaCepLogger.Api";

            var isRunningInContainer = string.Equals(
                Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.ToLowerInvariant() ?? string.Empty,
                "true",
                StringComparison.OrdinalIgnoreCase);

            var configuredBridge = seqSection.GetValue<bool?>("UseAgentBridge");
            var configuredHttp = seqSection.GetValue<bool?>("UseHttpIngestion");

            var useAgentBridge = isRunningInContainer
                ? (configuredBridge ?? true)
                : (configuredBridge ?? false);

            var useHttpIngestion = isRunningInContainer
                ? (configuredHttp ?? false)
                : (configuredHttp ?? true);

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", applicationName)
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

            configuration.WriteTo.Async(sink =>
            {
                if (useAgentBridge)
                {
                    sink.Console(new RenderedCompactJsonFormatter());
                }
                else
                {
                    sink.Console(outputTemplate: outputTemplate);
                }
            });

            if (useHttpIngestion)
            {
                configuration.WriteTo.Async(sink =>
                {
                    if (!string.IsNullOrWhiteSpace(seqApiKey))
                    {
                        sink.Seq(serverUrl: seqServerUrl, apiKey: seqApiKey);
                    }
                    else
                    {
                        sink.Seq(serverUrl: seqServerUrl);
                    }
                });
            }
        });
    }
}
