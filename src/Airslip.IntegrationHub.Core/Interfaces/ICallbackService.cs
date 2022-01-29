using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface ICallbackService
{
    IResponse GenerateUrl(string provider, string queryString);
}