using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.Etsy;

public class EtsyAPIv3AuthorisationDetail : BasicAuthorisationDetail
{
    [JsonProperty(PropertyName = "refresh_token")]
    public override string Password { get; set; } = string.Empty;
}