using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Interfaces;
using System.Net.Http;
using System.Text;

namespace Airslip.IntegrationHub.Core.Models.Shopify;

public class ShopifyPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public ShopifyPermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Method = HttpMethod.Post;
        Content = BuildStringContent(new ShopifyPermanentAccess(
                    providerDetails.ProviderSetting.AppId,
                    providerDetails.ProviderSetting.AppSecret,
                    shortLivedAuthorisationDetail.ShortLivedCode));
    }
}