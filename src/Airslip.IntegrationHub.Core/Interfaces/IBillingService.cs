using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Requests.Billing;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IBillingService
{
    Task<IResponse> Create(BillingRequest billingRequest);
}