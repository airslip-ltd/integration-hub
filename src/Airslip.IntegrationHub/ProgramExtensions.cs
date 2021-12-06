using Microsoft.Extensions.Configuration;
using System;

namespace Airslip.IntegrationHub
{
    public static class ProgramExtensions
    {
        private static readonly string EnvironmentName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Production";

        public static IConfigurationBuilder AddDefaultConfig(this IConfigurationBuilder builder, string[] args)
        {
            return builder
                .AddJsonFile("appSettings.json", false)
                .AddJsonFile($"appSettings.{EnvironmentName}.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
        }
    }
}