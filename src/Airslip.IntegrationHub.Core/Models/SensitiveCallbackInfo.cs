using Airslip.Common.Types.Enums;

namespace Airslip.IntegrationHub.Core.Models;

public class SensitiveCallbackInfo
{
    public AirslipUserType AirslipUserType { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string UserId { get; set; }= string.Empty;
    public string Shop { get; set; } = string.Empty;
    public bool TestMode { get; set; }
    public string CipheredSensitiveInfo { get; set; } = string.Empty;
    public string? InstitutionId { get; set; }
}