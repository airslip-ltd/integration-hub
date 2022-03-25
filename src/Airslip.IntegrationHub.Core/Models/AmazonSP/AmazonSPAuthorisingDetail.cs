using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.AmazonSP;

public class AmazonSPAuthorisingDetail : ShortLivedAuthorisationDetail
{
    [JsonProperty(PropertyName = "spapi_oauth_code")]
    public override string ShortLivedCode { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "selling_partner_id")]
    public override string StoreName { get; set; } = string.Empty;
}