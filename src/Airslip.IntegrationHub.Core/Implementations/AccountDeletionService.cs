using Airslip.Common.Deletion.Interfaces;
using Airslip.Common.Deletion.Models;
using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Interfaces;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class AccountDeletionService : IDeletionService
{
    private readonly IIntegrationDiscoveryService _discoveryService;
    private readonly IInternalMiddlewareClient _middlewareClient;

    public AccountDeletionService(IIntegrationDiscoveryService discoveryService, IInternalMiddlewareClient middlewareClient)
    {
        _discoveryService = discoveryService;
        _middlewareClient = middlewareClient;
    }

    public async Task<IResponse> DeleteRecord(string integration, string id, DeleteRequest requestDetails)
    {
        IntegrationDetails integrationDetails = _discoveryService.GetIntegrationDetails(integration);

        return await _middlewareClient.Delete(id, integration, integrationDetails, requestDetails);
    }
}