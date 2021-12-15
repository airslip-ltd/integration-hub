using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Responses;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations
{
    public class CustomerPortalClient : ICustomerPortalClient
    {
        public Task<IResponse> CreateStub()
        {
            return Task.Run<IResponse>(() => new AccountResponse("id"));
        }
    }
}