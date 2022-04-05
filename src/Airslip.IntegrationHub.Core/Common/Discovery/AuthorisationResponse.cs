using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Common.Discovery
{
    public record AuthorisationResponse(string AuthorisationUrl) : ISuccess;
}