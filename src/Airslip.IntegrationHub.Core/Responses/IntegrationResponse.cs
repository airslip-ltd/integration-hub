using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Responses;

public record IntegrationResponse(string Id) : ISuccess;