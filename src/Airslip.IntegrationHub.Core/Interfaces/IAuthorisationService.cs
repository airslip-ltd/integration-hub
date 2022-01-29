using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IAuthorisationService
{
    Task<IResponse> CreateAccount(
        ProviderDetails providerDetails,
        IProviderAuthorisation providerAuthorisingDetail);
}