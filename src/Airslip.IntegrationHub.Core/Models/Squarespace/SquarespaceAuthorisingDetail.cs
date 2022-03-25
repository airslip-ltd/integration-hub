using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.Squarespace
{
    public class SquarespaceAuthorisingDetail : ShortLivedAuthorisationDetail
    {
        [JsonProperty(PropertyName = "code")]
        public override string ShortLivedCode { get; set; } = string.Empty;
    }
}
