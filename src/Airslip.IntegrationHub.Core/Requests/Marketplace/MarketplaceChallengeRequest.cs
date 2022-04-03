using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Requests;

public class MarketplaceChallengeRequest
{
    [JsonProperty(PropertyName = "challenge_code")]
    public string ChallengeCode { get; set; } = string.Empty;
}