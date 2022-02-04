namespace Airslip.IntegrationHub.Core.Models;

public class PermanentAccessBase
{
    public virtual string? ApiKey { get; set; }
    public virtual string? ApiSecret { get; set; }
    public virtual string ShortLivedCode { get; set; } = string.Empty;
    public virtual string? GrantType { get; set; }
}