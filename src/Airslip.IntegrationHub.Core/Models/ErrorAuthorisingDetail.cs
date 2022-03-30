namespace Airslip.IntegrationHub.Core.Models;

public sealed class ErrorAuthorisingDetail : IProviderAuthorisation
{
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public SensitiveCallbackInfo SensitiveCallbackInfo { get; set; } = new();
}