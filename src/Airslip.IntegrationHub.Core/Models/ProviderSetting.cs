using Airslip.IntegrationHub.Core.Interfaces;

namespace Airslip.IntegrationHub.Core.Models
{
    public record ProviderSetting
    {
        public string BaseUri { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
        public ProviderAuthStrategy AuthStrategy { get; set; }

        public string FormatBaseUri(string value)
        {
            return string.Format(BaseUri, value);
        }
    }
}