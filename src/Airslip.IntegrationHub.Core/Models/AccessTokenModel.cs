using Airslip.Common.Types.Interfaces;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Models;

public class AccessTokenModel : ISuccess
{
    public Dictionary<string, string> Parameters { get; }

    public AccessTokenModel(Dictionary<string, string> parameters)
    {
        Parameters = parameters;
    }
}
