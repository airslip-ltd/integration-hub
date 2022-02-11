using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Requests.GDPR;

public class GDPRRequest
{
    [JsonProperty(PropertyName = "shop_id")]
    public int ShopId { get; set; }

    [JsonProperty(PropertyName = "shop_domain")]
    public string ShopDomain { get; set; } = string.Empty;
        
    [JsonProperty(PropertyName = "orders_requested")]
    public string[]? OrdersRequested { get; set; }

    [JsonProperty(PropertyName = "orders_to_redact")]
    public string[]? OrdersToDelete { get; set; }
        
    [JsonProperty(PropertyName = "customer")]
    public GDPRCustomer? GDPRCustomer { get; set; }
        
    [JsonProperty(PropertyName = "data_request")]
    public GDPRDataRequest? GDPRDataRequest { get; set; }
}