using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace MinimalAPI {
    public static class LoggerConfig {
        public static void ConfigureSerilogAndElasticsearch(this WebApplicationBuilder builder) {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var elasticUrl = builder.Configuration["ElasticSearch:Url"];

            builder.Logging.ClearProviders();

            Log.Logger = new LoggerConfiguration()
           //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
           .Enrich.FromLogContext()
           .Enrich.WithEnvironmentName()
           .Enrich.WithMachineName()
           .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {ElasticApmTraceId} {ElasticApmTransactionId} {Level:u3}] {Username} {Message:lj}{NewLine}{Exception}"
                )
           //.WriteTo.Console()
           .WriteTo.Debug()
           .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUrl)) {
               AutoRegisterTemplate = true,
               AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
               IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name!.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
           })
           .ReadFrom.Configuration(builder.Configuration)
           .CreateLogger();

            Serilog.Debugging.SelfLog.Enable(Console.Error);

            builder.Host.UseSerilog();
        }
    }
}
