using Airslip.Common.Types.Enums;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface IProviderDiscoveryService
    {
        PosProviders? GetProvider(string provider);
        ProviderDetails GetProviderDetails(PosProviders provider);
    }
}