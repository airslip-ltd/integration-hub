using Airslip.Common.Types.Enums;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IHmacService
{
    bool Validate(
        PosProviders provider,
        List<KeyValuePair<string, string>> queryStrings);
}