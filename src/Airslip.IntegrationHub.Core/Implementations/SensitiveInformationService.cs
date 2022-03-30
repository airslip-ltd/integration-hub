using Airslip.Common.Security.Configuration;
using Airslip.Common.Security.Implementations;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using System;
using System.Linq;

namespace Airslip.IntegrationHub.Core.Implementations
{
    public class SensitiveInformationService : ISensitiveInformationService
    {
        private readonly EncryptionSettings _encryptionSettings;
        private readonly ILogger _logger;

        public SensitiveInformationService(
            IOptions<EncryptionSettings> encryptionOptions)
        {
            _encryptionSettings = encryptionOptions.Value;
            _logger = Logger.None;
        }

        public SensitiveCallbackInfo DeserializeSensitiveInfoQueryString(string queryString)
        {
            SensitiveCallbackInfo sensitiveCallbackInfo = queryString.GetQueryParams<SensitiveCallbackInfo>();
            
            string serialisedUserInformation = Json.Serialize(sensitiveCallbackInfo);

            string cipheredSensitiveInfo = StringCipher.EncryptForUrl(
                serialisedUserInformation,
                _encryptionSettings.PassPhraseToken);

            sensitiveCallbackInfo.CipheredSensitiveInfo = cipheredSensitiveInfo;

            return sensitiveCallbackInfo;
        }

        public SensitiveCallbackInfo DecryptCallbackInfo(string cipherString)
        {
            string decryptedUserInfo;
            try
            {
                decryptedUserInfo = StringCipher.Decrypt(cipherString, _encryptionSettings.PassPhraseToken);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception e)
            {
                _logger.Debug("Error: {ErrorMessage}", e.Message);
            }

            try
            {
                // WooCommerce replaces the Encoded values with spaces and removes the equals sign
                if (cipherString.Contains(' '))
                    cipherString = cipherString.Replace(" ", "+");

                decryptedUserInfo = StringCipher.Decrypt(cipherString, _encryptionSettings.PassPhraseToken);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception e)
            {
                _logger.Debug("Error: {ErrorMessage}", e.Message);
            }

            try
            {
                if (cipherString.Last().ToString() != "=")
                    cipherString += "=";
                
                decryptedUserInfo = StringCipher.Decrypt(cipherString, _encryptionSettings.PassPhraseToken);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception e)
            {
                _logger.Debug("Error: {ErrorMessage}", e.Message);
            }

            try
            {
                cipherString += "=";
                
                decryptedUserInfo = StringCipher.Decrypt(cipherString, _encryptionSettings.PassPhraseToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
        }
        
        public static SensitiveCallbackInfo DecryptCallbackInfo(string cipherString, string passPhrase)
        {
            string decryptedUserInfo;
            try
            {
                decryptedUserInfo = StringCipher.Decrypt(cipherString, passPhrase);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception e)
            {
                Log.Debug("Error: {ErrorMessage}", e.Message);
            }

            try
            {
                // WooCommerce replaces the Encoded values with spaces and removes the equals sign
                if (cipherString.Contains(' '))
                    cipherString = cipherString.Replace(" ", "+");

                decryptedUserInfo = StringCipher.Decrypt(cipherString, passPhrase);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception e)
            {
                Log.Debug("Error: {ErrorMessage}", e.Message);
            }

            try
            {
                if (cipherString.Last().ToString() != "=")
                    cipherString += "=";
                
                decryptedUserInfo = StringCipher.Decrypt(cipherString, passPhrase);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception e)
            {
                Log.Debug("Error: {ErrorMessage}", e.Message);
            }

            try
            {
                cipherString += "=";
                
                decryptedUserInfo =StringCipher.Decrypt(cipherString, passPhrase);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
        }
    }
}