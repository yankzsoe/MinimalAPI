using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text.Json;

namespace MinimalAPI {
    public static class WeatherForecastRoute {


        public static void MapWeatherForecastEndpoints(this IEndpointRouteBuilder builder) {
            builder.MapGet("/api/weatherforecast", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] ILogger<Program> logger) => {

                ArgumentNullException.ThrowIfNull(nameof(httpContextAccessor.HttpContext));

                var transaction = Elastic.Apm.Agent.Tracer.CurrentTransaction;

                if (transaction == null) {
                    transaction = Elastic.Apm.Agent.Tracer.StartTransaction("WeatherForecastTrx", ApiConstants.TypeRequest);
                }

                try {
                    var userId = httpContextAccessor.HttpContext!.Request.Headers["UserId"].FirstOrDefault() ?? "Unknown";

                    // Record the user information to transacton
                    transaction.Context.User = new User {
                        Email = userId,
                        Id = userId,
                        UserName = "yankzsoe",
                    };

                    var forecast = GenerateWeatherForecastData();

                    var response = JsonSerializer.Serialize(forecast);
                    logger.LogInformation("ForecastRequest: {BodyRequest}, ForecastResponse: {BodyResponse}", null, response);

                    // Send response to client
                    httpContextAccessor.HttpContext.Response.ContentType = "application/json";
                    await httpContextAccessor.HttpContext.Response.WriteAsync(response);
                } catch (Exception ex) {
                    transaction.CaptureException(ex);
                    throw;
                } finally {
                    transaction.End();
                }
            });

            builder.MapPost("/api/weatherforecast", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] ILogger<Program> logger) => {
                ArgumentNullException.ThrowIfNull(nameof(httpContextAccessor.HttpContext));

                var transaction = Elastic.Apm.Agent.Tracer.CurrentTransaction;

                if (transaction == null) {
                    transaction = Elastic.Apm.Agent.Tracer.StartTransaction("WeatherForecastTrx", ApiConstants.TypeRequest);
                }

                try {
                    var userId = httpContextAccessor.HttpContext!.Request.Headers["UserId"].FirstOrDefault() ?? "Unknown";
                    var request = httpContextAccessor.HttpContext.Request;
                    var body = string.Empty;
                    using (var reader = new StreamReader(request.Body)) {
                        body = await reader.ReadToEndAsync();
                    }

                    // Record the user information to transacton
                    transaction.Context.User = new User {
                        Email = userId,
                        Id = userId,
                        UserName = "yankzsoe",
                    };

                    var forecast = GenerateWeatherForecastData();

                    var response = JsonSerializer.Serialize(forecast);
                    logger.LogInformation("ForecastRequest: {BodyRequest}, ForecastResponse: {BodyResponse}", body, response);

                    // Send response to client
                    httpContextAccessor.HttpContext.Response.ContentType = "application/json";
                    await httpContextAccessor.HttpContext.Response.WriteAsync(response);
                } catch (Exception ex) {
                    transaction.CaptureException(ex);
                    throw;
                } finally {
                    transaction.End();
                }
            });

            #region helper
            IEnumerable<WeatherForecast> GenerateWeatherForecastData() {
                return Enumerable.Range(1, 5).Select(index =>
                         new WeatherForecast {
                             Date = DateTime.Now.AddDays(index),
                             TemperatureC = Random.Shared.Next(-20, 55),
                             Summary = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" }[index]
                         })
                         .ToArray();
            }
            #endregion
        }
    }
}
