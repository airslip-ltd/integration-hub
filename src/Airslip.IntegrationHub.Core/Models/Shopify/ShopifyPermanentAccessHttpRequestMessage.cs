using Airslip.IntegrationHub.Core.Interfaces;
using System.Net.Http;

namespace Airslip.IntegrationHub.Core.Models.Shopify;

public class ShopifyPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public ShopifyPermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Method = HttpMethod.Post;
        Content = BuildStringContent(new ShopifyPermanentAccess(
                    providerDetails.ProviderSetting.ApiKey,
                    providerDetails.ProviderSetting.ApiSecret,
                    shortLivedAuthorisationDetail.ShortLivedCode));
    }
}