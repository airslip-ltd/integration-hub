using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Models;
using AutoMapper;

namespace Airslip.IntegrationHub.Core
{
    public static class AutoMapperExtensions
    {
        public static void AddShopify(
            this IMapperConfigurationExpression mapperConfigurationExpression,
            SettingCollection<ProviderSetting> providerSettings)
        {
            ProviderSetting providerSetting = providerSettings.GetSettingByName(PosProviders.Shopify.ToString());

            mapperConfigurationExpression
                .CreateMap<ShopifyProviderAuthorisingDetail, ProviderAuthorisingDetail>()
                .ForMember(d => d.StoreName, c => c.MapFrom(s => s.Shop.Replace(".myshopify.com", "")))
                .ForMember(d => d.ShortLivedCode, c => c.MapFrom(s => s.Code))
                .ForMember(d => d.EncryptedUserInfo, c => c.MapFrom(s => s.State))
                .ForMember(d => d.BaseUri, c => c.MapFrom(s => string.Format(providerSetting.BaseUri, s.Shop)))
                .ForMember(d => d.PermanentAccessUrl,
                    c => c.MapFrom(s => $"https://{s.Shop}/admin/oauth/access_token"));
        }
    }
}