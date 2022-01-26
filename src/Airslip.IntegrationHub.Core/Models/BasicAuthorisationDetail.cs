using Airslip.IntegrationHub.Core.Interfaces;

namespace Airslip.IntegrationHub.Core.Models
{
    public class BasicAuthorisationDetail : IProviderAuthorisation
    {
        public virtual string Login { get; set; } = string.Empty;
        public virtual string Password { get; set; } = string.Empty;
        public virtual string AccessScope { get; set; } = string.Empty;
        public virtual string EncryptedUserInfo { get; set; } = string.Empty;
        public virtual string StoreName { get; set; } = string.Empty;
        public ProviderAuthStrategy ProviderAuthStrategy { get; set; } = ProviderAuthStrategy.Basic;
    }

    public class BridgeAuthorisationDetail : IProviderAuthorisation
    {
        public ProviderAuthStrategy ProviderAuthStrategy { get; set; } = ProviderAuthStrategy.Bridge;
    }
}