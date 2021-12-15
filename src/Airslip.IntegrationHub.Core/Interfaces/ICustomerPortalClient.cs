using Airslip.Common.Types.Interfaces;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces
{
    public interface ICustomerPortalClient
    {
        Task<IResponse> CreateStub();
    }
}