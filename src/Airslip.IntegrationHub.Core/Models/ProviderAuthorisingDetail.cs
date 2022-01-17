namespace Airslip.IntegrationHub.Core.Models
{
    public record ProviderAuthorisingDetail
    {
        public string ShortLivedCode { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string EncryptedUserInfo { get; set; } = string.Empty;
        public string PermanentAccessUrl { get; set; } = string.Empty;
        public string? BaseUri { get; set; }
    }
}