using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IHmacService
{
    bool Validate(string provider, string apiSecret, List<KeyValuePair<string, string>> queryStrings);
}