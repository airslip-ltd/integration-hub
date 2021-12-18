using Airslip.Common.Types.Enums;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface IProviderDiscoveryService
    {
        ProviderSetting GetProviderSettings(string provider);
        ProviderDetails GetProviderDetails(string provider, string queryString);
        string GenerateCallbackUrl(PosProviders provider, string queryString, string? redirectUri = null);
        bool Validate(PosProviders provider, List<KeyValuePair<string, string>> queryStrings);
        Task<MiddlewareAuthorisationRequest> GetBody(string provider);
        PermanentAccessBase GetPermanentAccessBody(
            PosProviders provider,
            ProviderSetting providerSetting,
            string shortLivedCode);

        Task<MiddlewareAuthorisationRequest> QueryPermanentAccessToken(
            string provider, 
            ProviderDetails providerDetails);
    }
}