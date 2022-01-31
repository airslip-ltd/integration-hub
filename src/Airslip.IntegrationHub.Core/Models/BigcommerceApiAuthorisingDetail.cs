using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models;

public class BigCommerceApiAuthorisingDetail : ShortLivedAuthorisationDetail
{
    [JsonProperty(PropertyName = "code")]
    public override string ShortLivedCode { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "state")] 
    public override string EncryptedUserInfo { get; set; } = string.Empty; //"kRLN4HX2k5o0S4NWB70IaNwqqthvX4tRIa0aKh8UAB6OAjzTNmsgzH6EtsoOxM3dJRkQLbYtwRMsGT7EqY5+6OhAT7hFUAF97gihuj0Jcu8ZjEWTg9zdrf3jMuI1yAf4bt+AaENKcwX6DADi3SIGoujH3t15XRcVl8ZbM+WhvDw6sUITIbE3x800rhLSUKBF0OhshFOHE04KdHcqQdjZVu4K/w9G+q0ZT0t+jdBIdxjY1g/B7V3RmgoU87vqdwmT";
    
    [JsonProperty(PropertyName = "shop")]
    public override string StoreName { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "context")]
    public override string MiscellaneousInfo { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}