using Airslip.Common.Security.Configuration;
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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

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
    

    public HttpRequestMessage GetHttpRequestMessageForAccessToken(IntegrationDetails integrationDetails, Dictionary<string, string> parameters)
    {
        string authBaseUri = integrationDetails.IntegrationSetting.AuthoriseBaseUri ?? integrationDetails.IntegrationSetting.AuthorisationBaseUri;
        string url = $"{authBaseUri}/{integrationDetails.IntegrationSetting.AuthoriseRouteFormat}";

        AuthorisationParameterNames authParameterNames = integrationDetails.IntegrationSetting.AuthorisationParameterNames;
        
        Dictionary<string, string> replacements = new();
        if(parameters.TryGetValue(authParameterNames.Shop, out string? shop))
            replacements.Add("shop", shop);
        
        if(parameters.TryGetValue(authParameterNames.Code, out string? code))
            replacements.Add("code", code);
        
        if(!string.IsNullOrEmpty(integrationDetails.IntegrationSetting.ApiKey))
            replacements.Add("apiKey", integrationDetails.IntegrationSetting.ApiKey);
        
        if(!string.IsNullOrEmpty(integrationDetails.IntegrationSetting.ApiSecret))
            replacements.Add("apiSecret", integrationDetails.IntegrationSetting.ApiSecret);

        if (!string.IsNullOrEmpty(integrationDetails.CallbackUrl))
            replacements.Add("callbackUrl", integrationDetails.CallbackUrl);

        url = url.ApplyReplacements(replacements);
        // TODO: Add key value pairs for all due to squarespace not working with a querystring POST
        // KeyValuePair<string, string>[] d = {
        //     new("grant_type", "authorization_code"),
        //     new("code", parameters["code"]),
        //     new("redirect_uri", integrationDetails.CallbackUrl) 
        // };

        HttpRequestMessage httpRequestMessage =  new()
        {
            RequestUri = new Uri(url),
            Method = integrationDetails.IntegrationSetting.ExchangeCodeMethodType switch
            {
                MethodTypes.POST => HttpMethod.Post,
                MethodTypes.GET => HttpMethod.Get,
                _ => HttpMethod.Post
            },
            //Content = new FormUrlEncodedContent(d)
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