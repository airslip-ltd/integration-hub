namespace Airslip.IntegrationHub.Core.Models
{
    public record ShopifyProviderAuthorisingDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Shop { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Hmac { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }
}
