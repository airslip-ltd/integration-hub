using Airslip.Common.Types.Configuration;
using Airslip.IntegrationHub.Core.Models;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public record ProviderDetails(string DestinationBaseUri, PublicApiSetting PublicApiSetting, ProviderSetting ProviderSetting, string CallbackUrl);
}