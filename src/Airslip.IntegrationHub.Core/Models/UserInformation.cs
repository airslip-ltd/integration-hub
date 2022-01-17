using Airslip.Common.Types.Enums;

namespace Airslip.IntegrationHub.Core.Models
{
    public record UserInformation(AirslipUserType AirslipUserType, string EntityId, string UserId);
}