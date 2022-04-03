using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Requests.Marketplace;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IMarketplaceService
{
    Task<IResponse> Delete(string provider, DeleteMarketplaceRequest request);
}