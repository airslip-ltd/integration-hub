using Airslip.Common.Types.Enums;

namespace Airslip.IntegrationHub.Core.Models
{
    public record GenerateCallbackInformation(
        AirslipUserType AirslipUserType,
        string EntityId,
        string UserId,
        string Shop)
    {
        public string? CallbackUrl { get; init; }
    }
}