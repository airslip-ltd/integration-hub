using Airslip.IntegrationHub.Core.Enums;

namespace Airslip.IntegrationHub.Core.Common;

public class AuthorisationParameterNames : IIntegrationSettingError
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string AccessScope { get; set; } = string.Empty;
    public string Shop { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string StoreUrl { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string AdditionalValueOne { get; set; } = string.Empty;
    public string AdditionalValueTwo { get; set; } = string.Empty;
    public string AdditionalValueThree { get; set; } = string.Empty;
    public string IntegrationUserId { get; set; } = string.Empty;
    public string IntegrationProviderId { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}