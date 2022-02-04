using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.Ecwid;

public class EcwidAuthorisationDetail : BasicAuthorisationDetail
{
    [JsonProperty(PropertyName = "store_id")]
    public override string Login { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "access_token")]
    public override string Password { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "scope")]
    public override string AccessScope { get; set; } = string.Empty;

    public override string Shop => Login;


    public string Email { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "public_token")]
    public string PublicToken { get; set; } = string.Empty;
}