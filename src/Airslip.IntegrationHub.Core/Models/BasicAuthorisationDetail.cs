namespace Airslip.IntegrationHub.Core.Models
{
    public class BasicAuthorisationDetail : IProviderAuthorisation
    {
        public virtual string Login { get; set; } = string.Empty;
        public virtual string Password { get; set; } = string.Empty;
        public virtual string AccessScope { get; set; } = string.Empty;
        public virtual string EncryptedUserInfo { get; set; } = string.Empty; // Contains Shop, UserId, UserType, EntityId
        public virtual string? Shop { get; set; } = string.Empty;
        public virtual string? Context { get; set; } = string.Empty;
    }

    public class BridgeAuthorisationDetail : IProviderAuthorisation
    {
    }
}