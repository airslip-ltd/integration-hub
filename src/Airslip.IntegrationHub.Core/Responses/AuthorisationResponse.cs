using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Responses
{
    public record AuthorisationResponse(string Id, string? Name) : ISuccess;
}