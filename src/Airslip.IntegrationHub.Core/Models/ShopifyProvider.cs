namespace Airslip.IntegrationHub.Core.Models;

public class ShopifyProvider : IProvider
{
    public bool IsOnline { get; set; }
    public string Shop { get; set; } = string.Empty;
}