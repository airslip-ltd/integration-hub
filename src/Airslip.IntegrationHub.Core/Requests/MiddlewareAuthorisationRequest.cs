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
        public string? Environment { get; set; }= string.Empty;
        public string? Reference { get; set; }= string.Empty;
        public string? Context { get; set; }= string.Empty;
        public string? Location { get; set; }= string.Empty;
        public string? AdditionalFieldOne { get; set; }= string.Empty;
        public string? AdditionalFieldTwo { get; set; }= string.Empty;
        public string? AdditionalFieldThree { get; set; }= string.Empty;
        public bool Failed => Provider == string.Empty;
    }
}
