namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface IProviderDiscoveryService
    {
        ProviderDetails? GetPosProviderDetails(string provider, bool? testMode = null);
    }
}