using Airslip.Common.Security.Implementations;
using Airslip.Common.Types.Enums;
using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Airslip.IntegrationHub.Core.Implementations;

public class HmacService : IHmacService
{
    public bool Validate(string provider, string apiSecret, Dictionary<string, string> queryStrings)
    {
        string? hmacKey = _getHmacKey(provider);

        if (hmacKey is null)
            return true;

        if (queryStrings.Any(o => o.Key.Equals("bypass") && o.Value.Equals("true"))) 
            return true;

        if (!queryStrings.Any(o => o.Key.Equals(hmacKey))) 
            return false;

        queryStrings.TryGetValue(hmacKey, out string? hmacValue);

        hmacValue ??= string.Empty;
        
        queryStrings.Remove(hmacKey);

        return HmacCipher.Validate(queryStrings, hmacValue, apiSecret);
    }

    private static string? _getHmacKey(string provider)
    {
        return provider switch
        {
            nameof(PosProviders.Shopify) => "hmac",
            _ => null
        };
    }
}