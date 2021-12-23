using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;

namespace Airslip.IntegrationHub.Configuration
{
    public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
    {
        public override OpenApiInfo Info { get; set; } = new()
        {
            Version = "v1",
            Title = "Integration Hub API",
            Description = "This is the API used for managing and collating responses for matching algorithms.",
            Contact = new OpenApiContact()
            {
                Name = "Airslip Support",
                Email = "support@airslip.com",
                Url = new Uri("https://www.airslip.com"),
            },
            License = new OpenApiLicense()
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT"),
            }
        };

        public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
    }
}