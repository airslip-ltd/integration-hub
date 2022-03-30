using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.Ecwid;

public class EcwidAuthorisingDetail : ShortLivedAuthorisationDetail
{
    
    [JsonProperty(PropertyName = "code")]
    public override string ShortLivedCode { get; set; } = string.Empty;
}