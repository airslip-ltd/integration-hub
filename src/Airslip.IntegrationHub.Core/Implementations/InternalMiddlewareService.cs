﻿using Airslip.Common.Security.Configuration;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using Microsoft.Extensions.Options;
using Serilog;
using System;

namespace Airslip.IntegrationHub.Core.Implementations
{
    public class InternalMiddlewareService : IInternalMiddlewareService
    {
        private readonly ILogger _logger;
        private readonly EncryptionSettings _encryptionSettings;

        public InternalMiddlewareService(
            IOptions<EncryptionSettings> encryptionOptions,
            ILogger logger)
        {
            _logger = logger;
            _encryptionSettings = encryptionOptions.Value;
        }
        
        public MiddlewareAuthorisationRequest BuildMiddlewareAuthorisationModel(
            ProviderDetails providerDetails,
            BasicAuthorisationDetail basicAuthorisationDetail)
        {
            if (basicAuthorisationDetail.EncryptedUserInfo == string.Empty)
            {
                _logger.Fatal("{Parameter} cannot be empty", basicAuthorisationDetail.EncryptedUserInfo);
                return new MiddlewareAuthorisationRequest();
            }
            
            SensitiveCallbackInfo sensitiveCallbackInfo = SensitiveCallbackInfo.DecryptCallbackInfo(
                basicAuthorisationDetail.EncryptedUserInfo,
                _encryptionSettings.PassPhraseToken);
            
            return new MiddlewareAuthorisationRequest
            {
                Provider = providerDetails.Provider.ToString(),
                StoreName = sensitiveCallbackInfo.Shop, // May need to consolidate store name and store url
                StoreUrl = providerDetails.ProviderSetting.FormatBaseUri(basicAuthorisationDetail.Shop ?? sensitiveCallbackInfo.Shop), // Need to change to StoreUrl
                Login = basicAuthorisationDetail.Login,
                Password = basicAuthorisationDetail.Password,
                EntityId = sensitiveCallbackInfo.EntityId,
                UserId = sensitiveCallbackInfo.UserId,
                AirslipUserType = sensitiveCallbackInfo.AirslipUserType,
                Environment = providerDetails.ProviderSetting.Environment,
                Location = providerDetails.ProviderSetting.Location,
                Context = basicAuthorisationDetail.Context
            };
        }
    }
}