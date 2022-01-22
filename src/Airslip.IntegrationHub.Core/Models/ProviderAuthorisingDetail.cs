namespace Airslip.IntegrationHub.Core.Models
{
    public record ProviderAuthorisingDetail
    {
        public virtual string ShortLivedCode { get; set; } = string.Empty;
        public virtual string StoreName { get; set; } = string.Empty;
        public virtual string EncryptedUserInfo { get; set; } = string.Empty;
        public string PermanentAccessUrl { get; set; } = string.Empty;
        public string? BaseUri { get; set; }
    }
}