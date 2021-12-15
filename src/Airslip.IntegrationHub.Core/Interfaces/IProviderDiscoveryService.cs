using Airslip.Common.Types.Enums;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface IProviderDiscoveryService
    {
        ProviderDetails GetProviderDetails(string provider);

        string GenerateCallbackUrl(PosProviders provider, string accountId, string? shopName = null, bool? isOnline = false, string? redirectUri = null);
    }
}