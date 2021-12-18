using Airslip.Common.Types.Enums;

namespace Airslip.IntegrationHub.Core.Models
{
    public record UserInformation(AirslipUserType UserType, string EntityId);
}