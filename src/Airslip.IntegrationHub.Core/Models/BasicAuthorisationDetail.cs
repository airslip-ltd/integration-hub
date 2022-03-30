using Airslip.IntegrationHub.Core.Implementations;

namespace Airslip.IntegrationHub.Core.Models
{
    public class BasicAuthorisationDetail : IProviderAuthorisation
    {
        public virtual string Login { get; set; } = string.Empty;
        public virtual string Password { get; set; } = string.Empty;
        public virtual string AccessScope { get; set; } = string.Empty;
        public virtual string EncryptedUserInfo { get; set; } = string.Empty; // Contains Shop, UserId, UserType, EntityId
        public virtual string? Shop { get; set; }
        public virtual string? Context { get; set; }
        public SensitiveCallbackInfo SensitiveCallbackInfo { get; set; } = new();
        public void DecryptSensitiveInformation(string passPhrase)
        {
            SensitiveCallbackInfo = SensitiveInformationService.DecryptCallbackInfo(EncryptedUserInfo, passPhrase);
        }
    }
}