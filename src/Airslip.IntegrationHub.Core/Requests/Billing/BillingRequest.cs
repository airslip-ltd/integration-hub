using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Requests.Billing;

public class BillingRequest
{
    [JsonProperty(PropertyName = "shop")]
    public string Shop { get; set; } = string.Empty;
    
    public bool Test { get; set; }
}