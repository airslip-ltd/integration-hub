namespace Airslip.IntegrationHub.Core.Models
{
    public record ProviderSetting
    {
        public string BaseUri { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
        public bool ShortLivedCodeProcess { get; set; }
    }
}