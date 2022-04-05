
namespace Airslip.IntegrationHub.Core.Common.Discovery;

public interface IIntegrationDiscoveryService
{
    IntegrationDetails GetIntegrationDetails(string provider, string integration);
}