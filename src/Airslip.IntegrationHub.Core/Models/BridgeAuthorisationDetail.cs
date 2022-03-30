namespace Airslip.IntegrationHub.Core.Models;

public class BridgeAuthorisationDetail : IProviderAuthorisation
{
    public SensitiveCallbackInfo SensitiveCallbackInfo { get; set; } = new();
}