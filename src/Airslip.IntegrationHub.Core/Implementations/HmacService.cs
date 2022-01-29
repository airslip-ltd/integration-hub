using Airslip.Common.Security.Implementations;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Implementations;

public class HmacService : IHmacService
{
    private readonly IProviderDiscoveryService _providerDiscoveryService;

    public HmacService(IProviderDiscoveryService providerDiscoveryService)
    {
        _providerDiscoveryService = providerDiscoveryService;
    }
        
    public bool Validate(PosProviders provider, List<KeyValuePair<string, string>> queryStrings)
    {
        // Need to add for WooCommerce
        string? hmacKey = _getHmacKey(provider);

        if (hmacKey is null)
            return true;

        KeyValuePair<string, string> hmacKeyValuePair = queryStrings.Get(hmacKey);
        string hmacValue = hmacKeyValuePair.Value;
        queryStrings.Remove(hmacKeyValuePair);
        ProviderDetails providerDetails = _providerDiscoveryService.GetProviderDetails(provider);

        return HmacCipher.Validate(queryStrings, hmacValue, providerDetails.ProviderSetting.AppSecret);
    }

    private static string? _getHmacKey(PosProviders provider)
    {
        return provider switch
        {
            PosProviders.Shopify => "hmac",
            _ => null
        };
    }
}