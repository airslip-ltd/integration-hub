namespace Airslip.IntegrationHub.Core.Models
{
    public class ShortLivedAuthorisationDetail : IProviderAuthorisation
    {
        public virtual string ShortLivedCode { get; set; } = string.Empty;
        public virtual string StoreName { get; set; } = string.Empty;
        public virtual string EncryptedUserInfo { get; set; } = string.Empty;
        public string PermanentAccessUrl { get; set; } = string.Empty;
        public string? BaseUri { get; set; }

        public void FormatBaseUri(string value)
        {
            BaseUri = BaseUri is not null 
                ? string.Format(BaseUri, value) 
                : value;
        }
    }

    public interface IProviderAuthorisation
    {
    }
}