using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.AmazonSP;

public class AmazonSPAuthorisationDetail : BasicAuthorisationDetail
{
    // [JsonProperty(PropertyName = "access_token")]
    // public override string Login { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "refresh_token")]
    public override string Password { get; set; } = string.Empty;
}