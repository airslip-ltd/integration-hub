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
        ProviderSetting GetProviderSettings(PosProviders provider);
        ProviderDetails GetProviderDetails(PosProviders provider);
        string GenerateCallbackUrl(PosProviders provider, string queryString);

        bool ValidateHmac(
            PosProviders provider,
            List<KeyValuePair<string, string>> queryStrings);

        MiddlewareAuthorisationRequest GetMiddlewareAuthorisation(
            ProviderDetails providerDetails,
            BasicAuthorisationDetail basicAuthorisationDetail,
            string? storeUrl = null);

        Task<MiddlewareAuthorisationRequest> QueryPermanentAccessToken(
            ProviderDetails providerDetails,
            ShortLivedAuthorisationDetail shortLivedAuthorisationDetail);

        SensitiveCallbackInfo DecryptCallbackInfo(string cipherString);
    }
}