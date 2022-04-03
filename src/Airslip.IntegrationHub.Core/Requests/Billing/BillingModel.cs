using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Requests.Billing;

public class BillingModel
{
    [JsonProperty(PropertyName = "recurring_application_charge")]
    public Bill Bill { get; set; } = new();
}

public class Bill
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "price")]
    public double Price { get; set; }

    [JsonProperty(PropertyName = "trial_days")]
    public int TrialDays { get; set; } 
    
    [JsonProperty(PropertyName = "test")]
    public bool TestMode { get; set; }
    
    [JsonProperty(PropertyName = "return_url")]
    public string ReturnUrl { get; set; } = string.Empty;
}