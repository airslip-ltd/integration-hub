using Airslip.Common.Security.Configuration;
using Airslip.Common.Security.Implementations;
using Airslip.Common.Types.Configuration;
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Airslip.IntegrationHub.Core.Implementations;

public class AuthorisationPreparationService : IAuthorisationPreparationService
{
    private readonly SettingCollection<IntegrationSetting> _integrationSetting;
    private readonly ISensitiveInformationService _sensitiveInformationService;

    public AuthorisationPreparationService(
        IOptions<SettingCollection<IntegrationSetting>> integrationOptions,
        ISensitiveInformationService sensitiveInformationService)
    {
        _sensitiveInformationService = sensitiveInformationService;
        _integrationSetting = integrationOptions.Value;
    }

    public SensitiveCallbackInfo? TransformParametersToSensitiveCallbackInfo(Dictionary<string, string> parameters)
    {
        string? state = GetStateParameter(parameters);

        return state is null
            ? null
            : _sensitiveInformationService.DecryptCallbackInfo(state);
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

        if (parameters.TryGetValue(shopParameter, out string? shop))
            replacements.Add("shop", shop);

        if (parameters.TryGetValue(codeParameter, out string? code))
            replacements.Add("code", HttpUtility.UrlEncode(code));

        if (!string.IsNullOrEmpty(apiKey))
            replacements.Add("apiKey", apiKey);

        if (!string.IsNullOrEmpty(apiSecret))
            replacements.Add("apiSecret", apiSecret);

        if (!string.IsNullOrEmpty(callbackUrl))
            replacements.Add("callbackUrl", callbackUrl);

        if (!string.IsNullOrEmpty(appName))
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

        if (parameters.TryGetValue(authParameterNames.Shop, out string? shop))
            replacements.Add("shop", shop);

        string requestUri = integrationDetails.IntegrationSetting.AuthorisePathUri!.ApplyReplacements(replacements);

        HttpRequestMessage httpRequestMessage = new()
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
                AuthenticationSchemes.Basic.ToString(),
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{integrationDetails.IntegrationSetting.ApiKey}:{integrationDetails.IntegrationSetting.ApiSecret}")));
        }
        else if(integrationDetails.IntegrationSetting.AuthoriseScheme == AuthenticationSchemes.Bearer)
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                AuthenticationSchemes.Bearer.ToString(),
                integrationDetails.IntegrationSetting.ApiSecret);

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

    public string GenerateMiddlewareDestinationUrl(
        string provider,
        IntegrationDetails integrationDetails,
        Dictionary<string, string> parameters,
        SensitiveCallbackInfo? sensitiveInfo)
    {
        string destinationBaseUri = integrationDetails.IntegrationSetting.PublicApiSetting.ToBaseUri();
        IntegrationSetting middlewareSetting = _integrationSetting.GetSettingByName(integrationDetails.IntegrationSetting.PublicApiSettingName);

        string url = $"{destinationBaseUri}/{middlewareSetting.AuthoriseRouteFormat}";

        AuthorisationParameterNames providerAuthParameterNames = integrationDetails.IntegrationSetting.AuthorisationParameterNames;
        AuthorisationParameterNames middlewareAuthParameterNames = middlewareSetting.AuthorisationParameterNames;

        Dictionary<string, string> replacements = new();

        if(sensitiveInfo?.EntityId != null)
            replacements.Add("entityId", sensitiveInfo.EntityId);

        if (sensitiveInfo?.AirslipUserType != null)
            replacements.Add("airslipUserType", sensitiveInfo.AirslipUserType.ToString().ToLower());

        if (sensitiveInfo?.UserId != null)
            replacements.Add("userId", sensitiveInfo.UserId);

        replacements.Add("provider", provider);

        parameters.TryGetValue(providerAuthParameterNames.Login, out string? login);
        login ??= integrationDetails.IntegrationSetting.ApiSecret;

        if (!string.IsNullOrEmpty(login))
            replacements.Add(middlewareAuthParameterNames.Login, login);

        if (parameters.TryGetValue(providerAuthParameterNames.Password, out string? password))
            replacements.Add(middlewareAuthParameterNames.Password, password);

        if (parameters.TryGetValue(providerAuthParameterNames.Context, out string? context))
            replacements.Add(middlewareAuthParameterNames.Context, context);

        if (parameters.TryGetValue(providerAuthParameterNames.Environment, out string? environment))
            replacements.Add(middlewareAuthParameterNames.Environment, environment);

        if (parameters.TryGetValue(providerAuthParameterNames.IntegrationProviderId, out string? integrationProviderId))
            replacements.Add(middlewareAuthParameterNames.IntegrationProviderId, integrationProviderId);

        if (parameters.TryGetValue(providerAuthParameterNames.IntegrationUserId, out string? integrationUserId))
            replacements.Add(middlewareAuthParameterNames.IntegrationUserId, integrationUserId);

        if (parameters.TryGetValue(providerAuthParameterNames.Location, out string? location))
            replacements.Add(middlewareAuthParameterNames.Location, location);

        if (parameters.TryGetValue(providerAuthParameterNames.Reference, out string? reference))
            replacements.Add(middlewareAuthParameterNames.Reference, reference);

        if (parameters.TryGetValue(providerAuthParameterNames.RefreshToken, out string? refreshToken))
            replacements.Add(middlewareAuthParameterNames.RefreshToken, refreshToken);

        parameters.TryGetValue(providerAuthParameterNames.Shop, out string? shop);
        shop ??= sensitiveInfo?.Shop;

        if (!string.IsNullOrWhiteSpace(shop))
        {
            replacements.Add(middlewareAuthParameterNames.Shop, shop);

            string shopUrl = integrationDetails.IntegrationSetting.FormatBaseUri(shop);

            replacements.Add(middlewareAuthParameterNames.StoreUrl, shopUrl);
        }

        if (parameters.TryGetValue(providerAuthParameterNames.State, out string? state))
            replacements.Add(middlewareAuthParameterNames.State, state);

        if (parameters.TryGetValue(providerAuthParameterNames.AccessScope, out string? accessScope))
            replacements.Add(middlewareAuthParameterNames.AccessScope, accessScope);

        if (parameters.TryGetValue(providerAuthParameterNames.Code, out string? code))
            replacements.Add(middlewareAuthParameterNames.Code, code);

        if (parameters.TryGetValue(providerAuthParameterNames.AdditionalValueOne, out string? additionalValueOne))
            replacements.Add(middlewareAuthParameterNames.AdditionalValueOne, additionalValueOne);

        if (parameters.TryGetValue(providerAuthParameterNames.AdditionalValueTwo, out string? additionalValueTwo))
            replacements.Add(middlewareAuthParameterNames.AdditionalValueTwo, additionalValueTwo);

        if (parameters.TryGetValue(providerAuthParameterNames.AdditionalValueThree, out string? additionalValueThree))
            replacements.Add(middlewareAuthParameterNames.AdditionalValueThree, additionalValueThree);

        return url.ApplyReplacements(replacements);
    }
}