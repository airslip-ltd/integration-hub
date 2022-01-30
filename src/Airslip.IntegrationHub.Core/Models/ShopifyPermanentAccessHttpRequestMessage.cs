using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Interfaces;
using System.Net.Http;
using System.Text;

namespace Airslip.IntegrationHub.Core.Models;

public class ShopifyPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public ShopifyPermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Content = new StringContent(
            Json.Serialize(
                new ShopifyPermanentAccess(
                    providerDetails.ProviderSetting.AppId,
                    providerDetails.ProviderSetting.AppSecret,
                    shortLivedAuthorisationDetail.ShortLivedCode)),
            Encoding.UTF8,
            Json.MediaType);
    }
}