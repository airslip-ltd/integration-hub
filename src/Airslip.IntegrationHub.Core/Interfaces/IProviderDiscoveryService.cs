using Airslip.Common.Types.Enums;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface IProviderDiscoveryService
    {
        ProviderDetails GetProviderDetails(string provider);

        string GenerateCallbackUrl(PosProviders provider, string accountId, string? shopName = null, bool? isOnline = false, string? redirectUri = null);
        bool Validate(PosProviders provider, List<KeyValuePair<string, string>> queryStrings, string hmacKey);
    }
}