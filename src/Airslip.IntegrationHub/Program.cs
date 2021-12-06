using Airslip.Common.Auth.Functions.Extensions;
using Airslip.Common.Monitoring;
using Airslip.Common.Security.Configuration;
using Airslip.Common.Types.Configuration;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Airslip.IntegrationHub
{
    public class Program
    {
         public static void Main(string[] args)
        {
            BuildWebHost(args)
                .RunAsync()
                .Wait();
        }

        private static IHost BuildWebHost(string[] args)
        {
            IHost? host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(args);
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    IConfiguration config = new ConfigurationBuilder()
                        .AddDefaultConfig(args)
                        .Build();
                    
                    Logger logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(config)
                        .CreateLogger();
                    
                    services
                        .AddSingleton<ILogger>(_ => logger);
                    
                    services
                        .AddFluentValidation(options =>
                        {
                            options.RegisterValidatorsFromAssemblyContaining<Program>();
                        });

                    // Add Options
                    services
                        .AddOptions()
                        .Configure<JsonSerializerOptions>(options => 
                        {
                            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                            options.Converters.Add(new JsonStringEnumConverter());
                        })
                        .Configure<EnvironmentSettings>(config.GetSection(nameof(EnvironmentSettings)))
                        .Configure<PublicApiSettings>(config.GetSection(nameof(PublicApiSettings)))
                        .Configure<ApiKeyValidationSettings>(config.GetSection(nameof(ApiKeyValidationSettings)))
                        .Configure<EncryptionSettings>(config.GetSection(nameof(EncryptionSettings)))
                        .Configure<ApiBehaviorOptions>(options =>
                        {
                            options.SuppressModelStateInvalidFilter = true;
                        });
    
                    services
                        .AddAirslipFunctionAuth(config);

                    services
                        .AddProviderAuthorisation(config);

                    services
                        .UseHealthChecks();
                })
                .Build();

            return host;
        }
    }
}