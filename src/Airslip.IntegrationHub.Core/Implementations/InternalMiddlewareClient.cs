﻿using Airslip.Common.Deletion.Models;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Requests;
using Airslip.IntegrationHub.Core.Responses;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations
{
    public class InternalMiddlewareClient : IInternalMiddlewareClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly PublicApiSettings _publicApiSettings;

        public InternalMiddlewareClient(
            HttpClient httpClient,
            ILogger logger,
            IOptions<PublicApiSettings> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _publicApiSettings = options.Value;
        }

        public async Task<IResponse> Authorise(string apiKey, string url)
        {
            try
            {
                _logger.Information("Posting to integration middleware for Url {PostUrl}", url);

                HttpRequestMessage httpRequestMessage = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Headers =
                    {
                        { "x-api-key", apiKey}
                    },
                    //Content = new StringContent(
                    //    Json.Serialize(middlewareAuthorisationRequest),
                    //    Encoding.UTF8,
                    //    Json.MediaType)
                };

                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    _logger.Error(
                        "Error posting request to provider for Url {PostUrl}, response code: {StatusCode}, content: {Content}",
                        url,
                        response.StatusCode,
                        content);

                    return new ErrorResponse("MIDDLEWARE_ERROR", $"Error authorising in internal middleware {content}");
                }

                _logger.Information("Got response for post to integration middleware for Url {PostUrl}, response code: {StatusCode}", url, response.StatusCode);

                return await response.CommonResponseHandler<IntegrationResponse>();
            }
            catch (HttpRequestException hre)
            {
                _logger.Error(hre,
                    "Error posting request to integration middleware for Url {PostUrl}, response code: {StatusCode}",
                    url, hre.StatusCode);
                return new InvalidResource(nameof(Authorise), hre.Message);
            }
            catch (Exception ee)
            {
                _logger.Fatal(ee, "Error posting request to integration middleware for Url {PostUrl}", url);
                return new InvalidResource("UNHANDLED_ERROR", ee.Message);
            }
        }

        public async Task<IResponse> Delete(string accountId, string provider, IntegrationDetails integrationDetails, DeleteRequest deleteRequest)
        {
            string url = Endpoints.Delete(integrationDetails.IntegrationSetting.PublicApiSetting.ToBaseUri(), provider, accountId);

            try
            {
                _logger.Information("Posting to integration middleware for Url {PostUrl}",
                    url);

                HttpRequestMessage httpRequestMessage = new()
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(url),
                    Headers =
                    {
                        { "x-api-key", integrationDetails.IntegrationSetting.PublicApiSetting.ApiKey}
                    },
                    Content = new StringContent(
                        Json.Serialize(deleteRequest),
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
                        return new ErrorResponse("MIDDLEWARE_ERROR", $"Error deleting {content}");
                    }
                }

                _logger.Information("Got response for post to integration middleware for Url {PostUrl}, response code: {StatusCode}", url, response.StatusCode);

                return Json.Deserialize<DeleteResponse>(content);
            }
            catch (HttpRequestException hre)
            {
                _logger.Error(hre,
                    "Error posting request to integration middleware for Url {PostUrl}, response code: {StatusCode}",
                    url, hre.StatusCode);
                return new InvalidResource(nameof(Authorise), hre.Message);
            }
            catch (Exception ee)
            {
                _logger.Fatal(ee, "Error posting request to integration middleware for Url {PostUrl}", url);
                return new InvalidResource("UNHANDLED_ERROR", ee.Message);
            }
        }
    }

    internal static class Endpoints
    {
        public static string Authorise(string baseUri, string provider) =>
            $"{baseUri}/auth/{provider}";

        public static string Delete(string baseUri, string provider, string accountId) =>
            $"{baseUri}/delete/{provider}/{accountId}";
    }
}