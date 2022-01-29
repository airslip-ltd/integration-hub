using Airslip.Common.Security.Implementations;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using System;
using System.Linq;

namespace Airslip.IntegrationHub.Core.Models
{
    public record SensitiveCallbackInfo(
        AirslipUserType AirslipUserType,
        string EntityId,
        string UserId,
        string Shop)
    {
        public string? CallbackUrl { get; init; }
        
        public static (string cipheredSensitiveInfo, SensitiveCallbackInfo generateCallbackAuthRequest)
            GetEncryptedUserInformation(string queryString,  string passPhraseToken)
        {
            SensitiveCallbackInfo sensitiveCallbackAuthRequest = queryString.GetQueryParams<SensitiveCallbackInfo>();

            string serialisedUserInformation = Json.Serialize(sensitiveCallbackAuthRequest);

            string cipheredSensitiveInfo = StringCipher.EncryptForUrl(
                serialisedUserInformation,
                passPhraseToken);

            return (cipheredSensitiveInfo, sensitiveCallbackAuthRequest);
        }
        
        public static SensitiveCallbackInfo DecryptCallbackInfo(string cipherString, string passPhraseToken)
        {
            string decryptedUserInfo = string.Empty;
            try
            {
                decryptedUserInfo = StringCipher.Decrypt(cipherString,passPhraseToken);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                // WooCommerce replaces the Encoded values with spaces and removes the equals sign
                if (cipherString.Contains(' '))
                    cipherString = cipherString.Replace(" ", "+");

                decryptedUserInfo = StringCipher.Decrypt(cipherString, passPhraseToken);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if (cipherString.Last().ToString() != "=")
                    cipherString += "=";
                decryptedUserInfo = StringCipher.Decrypt(cipherString,passPhraseToken);
            }
            catch (Exception)
            {
                // ignored
            }

            return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
        }
    }
}