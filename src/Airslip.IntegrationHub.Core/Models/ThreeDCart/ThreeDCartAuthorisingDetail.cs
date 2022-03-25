using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.ThreeDCart;

public class ThreeDCartAuthorisingDetail : ShortLivedAuthorisationDetail
{
    [JsonProperty(PropertyName = "code")]
    public override string ShortLivedCode { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "shop")]
    public override string StoreName { get; set; } = string.Empty;
}