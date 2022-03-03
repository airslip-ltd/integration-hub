using Airslip.IntegrationHub.Core.Interfaces;
using System;

namespace Airslip.IntegrationHub.Core.Models
{
    public record ProviderSetting
    {
        public string BaseUri { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string MiddlewareDestinationAppName { get; set; } = string.Empty;
        public ProviderAuthStrategy AuthStrategy { get; set; }
        public string? AppName { get; set; }
        public string? Environment { get; set; }
        public string? Location { get; set; }
        public string? AdditionalFieldOne { get; set; }
        public string? AdditionalFieldTwo { get; set; }
        public string? AdditionalFieldThree { get; set; }
        public bool RequiresStoreName { get; set; }
        public string AppListingUrl { get; set; } = string.Empty;
        public string[] HmacValidateOn { get; set; } = Array.Empty<string>();

        public string FormatBaseUri(string value)
        {
            return string.Format(BaseUri, value);
        }
    }
}