using Airslip.Common.Types.Interfaces;
using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IAuthorisationService
{
    Task<IResponse> CreateAccount(
        HttpRequestData req,
        string provider);
}