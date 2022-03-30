using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using Serilog;

namespace Airslip.IntegrationHub.Core.Implementations
{
    public class InternalMiddlewareService : IInternalMiddlewareService
    {
        private readonly ILogger _logger;

        public InternalMiddlewareService(ILogger logger)
        {
            _logger = logger;
        }

        public MiddlewareAuthorisationRequest BuildMiddlewareAuthorisationModel(
            ProviderDetails providerDetails,
            BasicAuthorisationDetail basicAuthorisationDetail)
        {
            if (basicAuthorisationDetail.SensitiveCallbackInfo.EntityId == string.Empty)
            {
                _logger.Fatal("{Parameter} cannot be empty", basicAuthorisationDetail.EncryptedUserInfo);
                return new MiddlewareAuthorisationRequest();
            }

            string shop = basicAuthorisationDetail.Shop ?? basicAuthorisationDetail.SensitiveCallbackInfo.Shop;

            return new MiddlewareAuthorisationRequest
            {
                Provider = providerDetails.Provider.ToString(),
                StoreName = shop, // May need to consolidate store name and store url
                StoreUrl = providerDetails.ProviderSetting.FormatBaseUri(shop), // Need to change to StoreUrl
                Login = basicAuthorisationDetail.Login,
                Password = basicAuthorisationDetail.Password,
                EntityId = basicAuthorisationDetail.SensitiveCallbackInfo.EntityId,
                UserId = basicAuthorisationDetail.SensitiveCallbackInfo.UserId,
                AirslipUserType = basicAuthorisationDetail.SensitiveCallbackInfo.AirslipUserType,
                Environment = providerDetails.ProviderSetting.Environment,
                Location = providerDetails.ProviderSetting.Location,
                Context = basicAuthorisationDetail.Context,
                AdditionalFieldOne = providerDetails.ProviderSetting.AdditionalFieldOne,
                AdditionalFieldTwo = providerDetails.ProviderSetting.AdditionalFieldTwo,
                AdditionalFieldThree = providerDetails.ProviderSetting.AdditionalFieldThree
            };
        }
    }
}