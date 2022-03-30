using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Models;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface ICallbackService
{
    IResponse GenerateUrl(ProviderDetails providerDetails, SensitiveCallbackInfo sensitiveCallbackInfo);
}