using Airslip.IntegrationHub.Core.Models;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public record ProviderDetails(string Uri, ProviderSetting ProviderSetting, string CallbackUrl);
}