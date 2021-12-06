using Airslip.Common.Types.Enums;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface IExternalAuthService
    {
        string GenerateCallbackUrl(PosProviders provider, string accountId, string? redirectUri = null);
    }
}