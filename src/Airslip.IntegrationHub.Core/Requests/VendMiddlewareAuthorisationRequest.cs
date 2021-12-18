using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Requests
{
    public class VendMiddlewareAuthorisationRequest : MiddlewareAuthorisationRequest
    {
        [JsonProperty(PropertyName = "code")]
        public string ShortLivedAuthorisationCode { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "domain_prefix")]
        public new string StoreName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "user_id")]
        public string VendUserId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "state")] 
        public string AccountId { get; set; } = string.Empty;

        public string Signature { get; set; } = string.Empty;
        public string? Error { get; set; }
        public string GrantType { get; set; } = "authorization_code";
    }
}