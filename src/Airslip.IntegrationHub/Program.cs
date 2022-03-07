using Airslip.Common.Auth.Functions.Extensions;
using Airslip.Common.Auth.Functions.Middleware;
using Airslip.Common.Deletion;
using Airslip.Common.Functions.Extensions;
using Airslip.Common.Monitoring;
using Airslip.Common.Security.Configuration;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.UseNewtonsoftJson();
                    worker.UseMiddleware<ApiKeyAuthenticationMiddleware>();
                    worker.UseMiddleware<ApiKeyAuthorisationMiddleware>();
                })
                .ConfigureOpenApi()
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddDefaultConfig(args);
                })
                .UseSerilog((context, config) =>
                {
                    config.ReadFrom.Configuration(context.Configuration);
                })
                .ConfigureServices((context, services) =>
                {
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
                        .Configure<EnvironmentSettings>(context.Configuration.GetSection(nameof(EnvironmentSettings)))
                        .Configure<PublicApiSettings>(context.Configuration.GetSection(nameof(PublicApiSettings)))
                        .Configure<ApiKeyValidationSettings>(context.Configuration.GetSection(nameof(ApiKeyValidationSettings)))
                        .Configure<EncryptionSettings>(context.Configuration.GetSection(nameof(EncryptionSettings)))
                        .Configure<ApiBehaviorOptions>(options =>
                        {
                            options.SuppressModelStateInvalidFilter = true;
                        });

                    services
                        .AddAirslipFunctionAuth(context.Configuration);

                    services
                        .UseHealthChecks();

                    services
                        .AddProviderAuthorisation(context.Configuration)
                        .UseDeletion<AccountDeletionService>();
                    
                    services
                        .AddSingleton<IInternalMiddlewareClient, InternalMiddlewareClient>()
                        .AddHttpClient<InternalMiddlewareClient>((serviceProvider, httpClient) =>
                        {
                            IOptions<PublicApiSettings> settings = serviceProvider.GetRequiredService<IOptions<PublicApiSettings>>();
                            string baseUri = settings.Value.GetSettingByName("Api2Cart").ToBaseUri();
                            httpClient.AddDefaults(baseUri);
                        });
                    
                    SettingCollection<ProviderSetting> appSettings = new();
                    context.Configuration.GetSection($"{nameof(ProviderSetting)}s").Bind(appSettings);
                    
                    // TODO: Create validation for known provider settings so there is never an empty api key
                    
                })
                .Build();

            return host;
        }
    }
    
    public static class HttpClientFactoryExtensions
    {
        public static void AddDefaults(this HttpClient httpClient, string baseUri)
        {
            httpClient.BaseAddress = new Uri(baseUri);

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(Json.MediaType));
        }
    }
}