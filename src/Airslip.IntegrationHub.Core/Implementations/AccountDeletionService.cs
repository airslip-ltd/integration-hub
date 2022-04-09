using Airslip.Common.Deletion.Interfaces;
using Airslip.Common.Deletion.Models;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Interfaces;
using Serilog;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class AccountDeletionService : IDeletionService
{
    private readonly IIntegrationDiscoveryService _discoveryService;
    private readonly IInternalMiddlewareClient _middlewareClient;
    private readonly ILogger _logger;

    public AccountDeletionService(
        IIntegrationDiscoveryService discoveryService, IInternalMiddlewareClient middlewareClient, ILogger logger)
    {
        _discoveryService = discoveryService;
        _middlewareClient = middlewareClient;
        _logger = logger;
    }

    public async Task<IResponse> DeleteRecord(string provider, string id, DeleteRequest requestDetails)
    {
        IntegrationDetails integrationDetails = _discoveryService.GetIntegrationDetails(provider);

        return await _middlewareClient.Delete(id,provider, integrationDetails, requestDetails);
    }
}