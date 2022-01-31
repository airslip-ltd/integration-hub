using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models
{
    public class ShortLivedAuthorisationDetail : IProviderAuthorisation
    {
        public virtual string ShortLivedCode { get; set; } = string.Empty;
        public virtual string StoreName { get; set; } = string.Empty;
        public virtual string EncryptedUserInfo { get; set; } = string.Empty;
        public virtual string PermanentAccessUrl { get; set; } = string.Empty;
        public string? BaseUri { get; set; }

        [JsonProperty(PropertyName = "grant_type")]
        public virtual string GrantType { get; } = "authorization_code";
        public virtual string MiscellaneousInfo { get; set; } = string.Empty;

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