namespace Airslip.IntegrationHub.Core.Common.Discovery;

public record IntegrationDetails(string Uri, string ApiKey, IntegrationSetting IntegrationSetting, string CallbackUrl);