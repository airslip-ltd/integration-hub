using Airslip.Common.Types.Enums;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface IProviderDiscoveryService
    {
        ProviderDetails GetProviderDetails(PosProviders provider, bool? testMode = null);
    }
}