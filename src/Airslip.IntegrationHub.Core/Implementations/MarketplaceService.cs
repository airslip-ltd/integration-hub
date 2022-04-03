using Airslip.Common.Types;
using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Requests.Marketplace;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class MarketplaceService : IMarketplaceService
{
    public Task<IResponse> Delete(string provider, DeleteMarketplaceRequest request)
    {
        return Task.Run<IResponse>(() => Success.Instance);
    }
}