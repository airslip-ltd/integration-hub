using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface ICallbackService
{
    IResponse GenerateUrl(ProviderDetails providerDetails, string queryString);
}