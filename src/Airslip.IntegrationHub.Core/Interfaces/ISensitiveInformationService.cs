using Airslip.IntegrationHub.Core.Models;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface ISensitiveInformationService
{
    SensitiveCallbackInfo DeserializeSensitiveInfoQueryString(string queryString);
    SensitiveCallbackInfo DecryptCallbackInfo(string cipherString);
}