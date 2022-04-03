using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Requests.Marketplace;

public class DeleteMarketplaceRequest
{
    [JsonProperty(PropertyName = "challenge_code")]
    public string ChallengeCode { get; set; } = string.Empty;
}