using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Requests
{
    public class MiddlewareAuthorisationRequest : IResponse
    {
        public string Provider { get; set; } = string.Empty;
        public string StoreName { get; set; }= string.Empty;
        public string StoreUrl { get; set; }= string.Empty;
        public string Login { get; set; }= string.Empty;
        public string Password { get; set; }= string.Empty;
        public string? Reference { get; set; }= string.Empty;
    }
}