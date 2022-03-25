using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.BigCommerce;

public class BigCommerceApiAuthorisingDetail : ShortLivedAuthorisationDetail
{
    [JsonProperty(PropertyName = "code")]
    public override string ShortLivedCode { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "shop")]
    public override string StoreName { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "context")]
    public override string MiscellaneousInfo { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}