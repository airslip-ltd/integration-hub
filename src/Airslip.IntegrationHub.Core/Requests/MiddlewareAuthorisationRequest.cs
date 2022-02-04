using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Interfaces;
using System;

namespace Airslip.IntegrationHub.Core.Requests
{
    public class MiddlewareAuthorisationRequest : IResponse
    {
        public string EntityId { get; set; } = string.Empty;
        public AirslipUserType AirslipUserType { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public virtual string StoreName { get; set; } = string.Empty;
        public string StoreUrl { get; set; }= string.Empty;
        public string Login { get; set; }= string.Empty;
        public string Password { get; set; }= string.Empty;
        public string? Environment { get; set; }
        public string? Reference { get; set; }
        public string? Context { get; set; }
        public string? Location { get; set; }
        public string? AdditionalFieldOne { get; set; }
        public string? AdditionalFieldTwo { get; set; }
        public string? AdditionalFieldThree { get; set; }
        public bool Failed => Provider == string.Empty;
    }
}
