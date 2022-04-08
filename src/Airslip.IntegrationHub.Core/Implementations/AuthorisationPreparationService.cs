using Airslip.Common.Security.Configuration;
using Airslip.Common.Security.Implementations;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Common;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Enums;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Airslip.IntegrationHub.Core.Implementations;

public class AuthorisationPreparationService : IAuthorisationPreparationService
{
    private readonly EncryptionSettings _encryptionSettings;
    private readonly IIntegrationDiscoveryService _discoveryService;
    private readonly ISensitiveInformationService _sensitiveInformationService;

    public AuthorisationPreparationService(IOptions<EncryptionSettings> encryptionOptions, IIntegrationDiscoveryService discoveryService, ISensitiveInformationService sensitiveInformationService)
    {
        _discoveryService = discoveryService;
        _sensitiveInformationService = sensitiveInformationService;
        _encryptionSettings = encryptionOptions.Value;
    }

    public SensitiveCallbackInfo? TransformParametersToSensitiveCallbackInfo(Dictionary<string,string> parameters)
    {
        string? state = GetStateParameter(parameters);

        return state is null 
            ? null 
            : _sensitiveInformationService.DecryptCallbackInfo(state);
    }
    
    public Dictionary<string,string> BankingQueryStringReplacer(Dictionary<string, string> parameters)
    {
        Dictionary<string, string> replacements = new();

        if(parameters.TryGetValue("consent", out string? consent))
            replacements.Add("consent", consent);
        
        if(parameters.TryGetValue("application-user-id", out string? appUserId))
            replacements.Add("appUserId", appUserId);
        
        if(parameters.TryGetValue("user-uuid", out string? userId))
            replacements.Add("userId", userId);
        
        if(parameters.TryGetValue("institution", out string? integrationProviderId))
            replacements.Add("integrationProviderId", integrationProviderId);

        return replacements;
    }

    public ICollection<KeyValuePair<string, string>> CommerceQueryStringReplacer(
        Dictionary<string, string> parameters,
        string authoriseRouteFormat,
        string shopParameter,
        string codeParameter,
        string apiKey,
        string apiSecret,
        string callbackUrl,
        string appName)
    {
        Dictionary<string, string> replacements = new();
        
        if(parameters.TryGetValue(shopParameter, out string? shop))
            replacements.Add("shop", shop);
        
        if(parameters.TryGetValue(codeParameter, out string? code))
            replacements.Add("code", HttpUtility.UrlEncode(code));
        
        if(!string.IsNullOrEmpty(apiKey))
            replacements.Add("apiKey", apiKey);
        
        if(!string.IsNullOrEmpty(apiSecret))
            replacements.Add("apiSecret", apiSecret);

        if (!string.IsNullOrEmpty(callbackUrl))
            replacements.Add("callbackUrl", callbackUrl);
        
        if(!string.IsNullOrEmpty(appName))
            replacements.Add("appName", appName);

        string relativeUrl = authoriseRouteFormat.ApplyReplacements(replacements);
        
        return relativeUrl
            .GetQueryParams()
            .ToList();
    }
    
    public HttpRequestMessage GetHttpRequestMessageForAccessToken(IntegrationDetails integrationDetails, Dictionary<string, string> parameters)
    {
        AuthorisationParameterNames authParameterNames = integrationDetails.IntegrationSetting.AuthorisationParameterNames;

        ICollection<KeyValuePair<string, string>> queryParams = CommerceQueryStringReplacer(
            parameters,
            integrationDetails.IntegrationSetting.AuthoriseRouteFormat,
            authParameterNames.Shop,
            authParameterNames.Code,
            integrationDetails.IntegrationSetting.ApiKey,
            integrationDetails.IntegrationSetting.ApiSecret,
            integrationDetails.CallbackUrl,
            integrationDetails.IntegrationSetting.AppName);
        
        Dictionary<string, string> replacements = new();
        
        if(parameters.TryGetValue(authParameterNames.Shop, out string? shop))
            replacements.Add("shop", shop);
        
        string requestUri = integrationDetails.IntegrationSetting.AuthoriseBaseUri!.ApplyReplacements(replacements);

        HttpRequestMessage httpRequestMessage =  new()
        {
            RequestUri = new Uri(requestUri),
            Method = integrationDetails.IntegrationSetting.ExchangeCodeMethodType switch
            {
                MethodTypes.POST => HttpMethod.Post,
                MethodTypes.GET => HttpMethod.Get,
                _ => HttpMethod.Post
            },
            Content = new FormUrlEncodedContent(queryParams)
        };
        
        if (integrationDetails.IntegrationSetting.AuthoriseHeadersRequired)
        {
            ProductInfoHeaderValue productValue = new("Airslip", "1.0");
            ProductInfoHeaderValue commentValue = new("(+https://www.airslip.com)");

            httpRequestMessage.Headers.UserAgent.Add(productValue);
            httpRequestMessage.Headers.UserAgent.Add(commentValue);
        }
        
        if (integrationDetails.IntegrationSetting.AuthoriseScheme == AuthenticationSchemes.Basic)
        {
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{integrationDetails.IntegrationSetting.ApiKey}:{integrationDetails.IntegrationSetting.ApiSecret}")));
        }

        return httpRequestMessage;
    }

    public string? GetStateParameter(IReadOnlyDictionary<string, string> parameters)
    {
        parameters.TryGetValue("user_info", out string? state);
        if (state is not null) 
            return state;
        
        parameters.TryGetValue("state", out state);
        if (state is not null) 
            return state;
        
        parameters.TryGetValue("user_id", out state);
        if (state is not null) 
            return state;

        return null;
    }

    public BasicAuthorisationDetail BuildSuccessfulAuthorisationModel(
        IntegrationDetails integrationDetails, 
        Dictionary<string, string> parameters)
    {
        AuthorisationParameterNames authParameterNames = integrationDetails.IntegrationSetting.AuthorisationParameterNames;
        
        parameters.TryGetValue(
            authParameterNames.Login,
            out string? loginValue);
        
        loginValue ??= integrationDetails.IntegrationSetting.ApiSecret;
        
        parameters.TryGetValue(
            authParameterNames.Password,
            out string? passwordValue);
        
        parameters.TryGetValue(
            authParameterNames.AccessScope,
            out string? scopeValue);
        
        parameters.TryGetValue(
            authParameterNames.Shop,
            out string? shopValue);
        
        return new BasicAuthorisationDetail
        {
            Login = loginValue,
            Password = passwordValue ?? string.Empty,
            AccessScope = scopeValue ?? string.Empty,
            Shop = shopValue
        };
    }

    public Dictionary<string, string> GetParameters(HttpRequestData req)
    {
        if (req.Method == "GET")
            return req.Url.Query.QueryStringToDictionary();
        
        object o = req.Body.DeserializeFunctionStream<object>();
        req.Body.Position = 0;
        string s = Json.Serialize(o);
        return Json.Deserialize<Dictionary<string, string>>(s);
    }
}