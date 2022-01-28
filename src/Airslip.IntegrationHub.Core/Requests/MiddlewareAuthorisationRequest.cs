using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Requests
{
    public class MiddlewareAuthorisationRequest : IResponse
    {
        public string EntityId { get; set; } = string.Empty;
        public AirslipUserType AirslipUserType { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public virtual string StoreName { get; set; }= string.Empty;
        public string StoreUrl { get; set; }= string.Empty;
        public string Login { get; set; }= string.Empty;
        public string Password { get; set; }= string.Empty;
        public string? Environment { get; set; }= string.Empty;
        public string? Reference { get; set; }= string.Empty;
        public string? LocationId { get; set; }= string.Empty;
    }
}