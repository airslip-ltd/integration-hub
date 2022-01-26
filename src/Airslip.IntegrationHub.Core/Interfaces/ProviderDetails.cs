using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.IntegrationHub.Core.Models;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public record ProviderDetails(
        PosProviders Provider,
        string DestinationBaseUri,
        string RedirectUri,
        PublicApiSetting PublicApiSetting,
        ProviderSetting ProviderSetting);
}