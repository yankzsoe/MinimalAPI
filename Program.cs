using Elastic.Apm.AspNetCore;

namespace MinimalAPI {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.ConfigureSerilogAndElasticsearch();
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();


            var app = builder.Build();

            // Enable ElasticAPM
            app.UseElasticApm(builder.Configuration
                // Uncomment this for diagnose to our API request & response
                //new HttpDiagnosticsSubscriber(),

                // Uncomment this for diagnose query to our database using efcore
                //new EfCoreDiagnosticsSubscriber()
                );

            //app.UseSerilogRequestLogging(opt => { });

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapWeatherForecastEndpoints();

            app.Run();
        }
    }
}
