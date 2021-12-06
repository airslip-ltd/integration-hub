using Airslip.Common.Types.Enums;
using Airslip.IntegrationHub.Core.Requests;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface IExternalAuthService
    {
        string GenerateCallbackUrl(PosProviders provider, string accountId, string? redirectUri = null);
        AuthorisationCallBackBase GetQueryParams(PosProviders provider, string queryStrings);
    }
}