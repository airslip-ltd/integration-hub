using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Models;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IAuthorisationService
{
    Task<IResponse> CreateAccount(
        IntegrationDetails integrationDetails,
        IProviderAuthorisation providerAuthorisingDetail);
}