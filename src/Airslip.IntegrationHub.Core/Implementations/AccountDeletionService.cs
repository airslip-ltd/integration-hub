using Airslip.Common.Deletion.Interfaces;
using Airslip.Common.Deletion.Models;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Interfaces;
using Serilog;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class AccountDeletionService : IDeletionService
{
    private readonly IProviderDiscoveryService _providerDiscoveryService;
    private readonly IInternalMiddlewareClient _middlewareClient;
    private readonly ILogger _logger;

    public AccountDeletionService(IProviderDiscoveryService providerDiscoveryService, 
        IInternalMiddlewareClient middlewareClient, ILogger logger)
    {
        _providerDiscoveryService = providerDiscoveryService;
        _middlewareClient = middlewareClient;
        _logger = logger;
    }
    
    public async Task<IResponse> DeleteRecord(string integration, string id, DeleteRequest requestDetails)
    {
        ProviderDetails? providerDetails = _providerDiscoveryService.GetPosProviderDetails(integration);

        if (providerDetails is null)
        {
            _logger.Warning("{Provider} is an unsupported provider", integration);
            return new NotFoundResponse("Provider", integration);
        }

        return await _middlewareClient.Delete(id, providerDetails, requestDetails);
    }
}