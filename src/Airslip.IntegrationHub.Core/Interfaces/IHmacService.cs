using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IHmacService
{
    bool Validate(string provider, string apiSecret, Dictionary<string, string> queryStrings);
}