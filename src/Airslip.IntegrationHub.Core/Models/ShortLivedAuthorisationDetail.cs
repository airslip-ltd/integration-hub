using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Interfaces;
using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models
{
    public class ShortLivedAuthorisationDetail : IProviderAuthorisation
    {
        public virtual string ShortLivedCode { get; set; } = string.Empty;
        public virtual string StoreName { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "state")]
        public string StateEncryptedUserInfo { get; set; } = string.Empty;
    
        [JsonProperty(PropertyName = "user_info")]
        public string? PassedEncryptedUserInfo { get; set; }

        public virtual string EncryptedUserInfo
        {
            get => PassedEncryptedUserInfo ?? StateEncryptedUserInfo;
            set => StateEncryptedUserInfo = value;
        }
        public virtual string PermanentAccessUrl { get; set; } = string.Empty;
        public string? BaseUri { get; set; }

        [JsonProperty(PropertyName = "grant_type")]
        public virtual string GrantType { get; } = "authorization_code";
        public virtual string MiscellaneousInfo { get; set; } = string.Empty;
        
        public string? Scope { get; set; }

        public void FormatBaseUri(string value)
        {
            BaseUri = BaseUri is not null 
                ? string.Format(BaseUri, value) 
                : value;
        }

        [JsonProperty(PropertyName = "error")]
        public string? ErrorMessage { get; set; }
        
        [JsonProperty(PropertyName = "error_code")]
        public string? ErrorCode { get; set; }

        public SensitiveCallbackInfo SensitiveCallbackInfo { get; set; } = new();

        public void DecryptSensitiveInformation(string passPhrase)
        {
            if(!string.IsNullOrWhiteSpace(EncryptedUserInfo))
                SensitiveCallbackInfo = SensitiveInformationService.DecryptCallbackInfo(EncryptedUserInfo, passPhrase);
        }
    }
}