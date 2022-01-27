namespace Airslip.IntegrationHub.Core.Models;

public class PermanentAccessBase
{
    public virtual string? AppId { get; set; }
    public virtual string? AppSecret { get; set; }
    public virtual string ShortLivedCode { get; set; } = string.Empty;
    public virtual string? GrantType { get; set; }
}