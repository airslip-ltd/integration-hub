namespace Airslip.IntegrationHub.Core.Models
{
    public record ProviderAuthorisation
    {
        public virtual string Login { get; set; } = string.Empty;
        public virtual string Password { get; set; } = string.Empty;
        public virtual string AccessScope { get; set; } = string.Empty;
    }
}