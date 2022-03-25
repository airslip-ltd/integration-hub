using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.BigCommerce;

public class BigCommerceApiAuthorisationDetail : BasicAuthorisationDetail
{
    [JsonProperty(PropertyName = "access_token")]
    public override string Password { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "scope")]
    public override string AccessScope { get; set; } = string.Empty;

    public BigCommerceUser User { get; set; } = new();
    
    public override string? Context { get; set; } = string.Empty;
}

public class BigCommerceUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
