namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface IProviderDiscoveryService
    {
        ProviderDetails? GetProviderDetails(string provider, bool? testMode = null);
    }
}