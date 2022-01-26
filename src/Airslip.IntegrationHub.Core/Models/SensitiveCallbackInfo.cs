using Airslip.Common.Types.Enums;

namespace Airslip.IntegrationHub.Core.Models
{
    public record SensitiveCallbackInfo(
        AirslipUserType AirslipUserType,
        string EntityId,
        string UserId,
        string Shop)
    {
        public string? CallbackUrl { get; init; }
    }
}