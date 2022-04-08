using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Common.Discovery;

public interface IIntegrationUrlService
{
    Task<IResponse> GetAuthorisationUrl(
        string provider,
        SensitiveCallbackInfo sensitiveCallbackInfo,
        CancellationToken cancellationToken);

    Task<IResponse> ApproveIntegration(
        HttpRequestData req,
        string provider,
        CancellationToken cancellationToken = default);

    // Task<IResponse> ApproveIntegration(string provider, string integration, Dictionary<string, string> replacements, 
    //     CancellationToken cancellationToken);
    //
    // Task<IResponse> DeleteIntegration(string integrationId, 
    //     CancellationToken cancellationToken);
}