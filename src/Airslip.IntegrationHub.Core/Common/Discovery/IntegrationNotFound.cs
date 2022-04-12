using Airslip.Common.Types.Failures;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Common.Discovery
{
    public record IntegrationNotFound : IntegrationDetails
    {
        public IntegrationNotFound() : base(string.Empty, string.Empty, new IntegrationSetting(), string.Empty)
        {

        }
    }
}