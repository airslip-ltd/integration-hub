using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using System.Net.Http;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IAuthorisationPreparationService
{
    HttpRequestMessage GetHttpRequestMessageForAccessToken(
        IntegrationDetails integrationDetails,
        Dictionary<string, string> parameters);
   
    Dictionary<string, string> GetParameters(HttpRequestData req);
    
    string? GetStateParameter(IReadOnlyDictionary<string, string> parameters);
   
    SensitiveCallbackInfo? TransformParametersToSensitiveCallbackInfo(Dictionary<string, string> parameters);
  
    ICollection<KeyValuePair<string, string>> CommerceQueryStringReplacer(
        Dictionary<string, string> parameters,
        string authoriseRouteFormat,
        string shopParameter,
        string codeParameter,
        string apiKey,
        string apiSecret,
        string callbackUrl,
        string appName);

    string GenerateMiddlewareDestinationUrl(
       string provider,
       IntegrationDetails integrationDetails,
       Dictionary<string, string> parameters,
       SensitiveCallbackInfo? sensitiveInfo);

    SensitiveCallbackInfo AddDynamicShopName(IntegrationDetails integrationDetails, Dictionary<string, string> parameters, SensitiveCallbackInfo sensitiveInfo);
}