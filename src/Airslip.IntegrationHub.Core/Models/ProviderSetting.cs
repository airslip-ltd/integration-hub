using Airslip.IntegrationHub.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public List<AuthRequestTypes> HmacValidateOn { get; set; } = new();
        public string ReturnPage { get; set; } = string.Empty;
        
        public string FormatBaseUri(string value)
        {
            return string.Format(BaseUri, value);
        }

        public bool ShouldValidate(AuthRequestTypes authRequestType)
        {
            return HmacValidateOn.Contains(authRequestType);
        }

        public bool ValidateIfRequiresStoreName(string? storeName)
        {
            return RequiresStoreName && string.IsNullOrWhiteSpace(storeName);
        }
    }
}