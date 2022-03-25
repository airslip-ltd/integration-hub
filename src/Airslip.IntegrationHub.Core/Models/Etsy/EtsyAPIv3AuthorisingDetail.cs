using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.Etsy;

public class EtsyAPIv3AuthorisingDetail : ShortLivedAuthorisationDetail
{
    [JsonProperty(PropertyName = "code")]
    public override string ShortLivedCode { get; set; } = string.Empty;
        
    public override string PermanentAccessUrl { get; set; } = "";
        
    public string? Error { get; set; }
        
    [JsonProperty(PropertyName = "error_description")]
    public string? ErrorDescription { get; set; }
        
    [JsonProperty(PropertyName = "error_uri")]
    public string? ErrorUri { get; set; }
}