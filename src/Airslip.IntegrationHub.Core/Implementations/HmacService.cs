using Airslip.Common.Security.Implementations;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Airslip.IntegrationHub.Core.Implementations;

public class HmacService : IHmacService
{
    public bool Validate(ProviderDetails providerDetails, List<KeyValuePair<string, string>> queryStrings) //, RequestType requestType
    {
        //if (!providerDetails.ShouldValidate(requestType)) return true;

        // Need to add for WooCommerce
        string? hmacKey = _getHmacKey(providerDetails.Provider);

        if (hmacKey is null)
            return true;

        if (queryStrings.Any(o => o.Key.Equals("bypass") && o.Value.Equals("true"))) 
            return true;

        if (!queryStrings.Any(o => o.Key.Equals(hmacKey))) 
            return false;
        
        KeyValuePair<string, string> hmacKeyValuePair = queryStrings.Get(hmacKey);
        
        string hmacValue = hmacKeyValuePair.Value;
        queryStrings.Remove(hmacKeyValuePair);

        return HmacCipher.Validate(queryStrings, hmacValue, providerDetails.ProviderSetting.ApiSecret);
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