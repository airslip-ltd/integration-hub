using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Responses
{
    public record AuthCallbackGeneratorResponse(string CallBackUrl) : ISuccess;
}