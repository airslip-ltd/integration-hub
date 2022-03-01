using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.AmazonSP;

public class AmazonSPAuthorisationDetail : BasicAuthorisationDetail
{
    [JsonProperty(PropertyName = "refresh_token")]
    public override string Password { get; set; } = string.Empty;
}