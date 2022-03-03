using Airslip.Common.Types.Enums;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IHmacService
{
    bool Validate(ProviderDetails providerDetails, List<KeyValuePair<string, string>> queryStrings);
}