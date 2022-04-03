using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Requests.Billing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class BillingService : IBillingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public BillingService(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<IResponse> Create(BillingRequest billingRequest)
    {
        BillingModel model = new()
        {
            Bill =
            {
                Name = "Airslip",
                Price = 0.01,
                TrialDays = 1000,
                TestMode = billingRequest.Test,
                ReturnUrl = "https://dev-secure.airslip.com"
            }
        };
        string url = $"https://{billingRequest.Shop}/admin/api/2022-04/recurring_application_charges.json";
        
        HttpRequestMessage httpRequestMessage = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url),
            Headers =
            {
                { "X-Shopify-Access-Token", "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"}
            },
            Content = new StringContent(
                Json.Serialize(model),
                Encoding.UTF8,
                Json.MediaType)
        };

        HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

        string content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(
                "Error sending request to provider for Url {DeleteUrl}, response code: {StatusCode}", 
                url, 
                response.StatusCode);

            try
            {
                return Json.Deserialize<ErrorResponse>(content);
            }
            catch (Exception)
            {
                return new ErrorResponse("SHOPIFY_BILLING_ERROR",  $"Error deleting {content}");
            }
        }

        dynamic obj = JObject.Parse(content);
        
        return new BillingResponse(obj.recurring_application_charge);
    }
}

public class BillingResponse : ISuccess
{    
    [JsonProperty(PropertyName = "recurring_application_charge")]
    public object Obj { get; }

    public BillingResponse(object obj)
    {
        Obj = obj;
    }
}