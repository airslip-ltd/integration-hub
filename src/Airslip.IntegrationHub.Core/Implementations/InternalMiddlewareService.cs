using Airslip.IntegrationHub.Core.Common.Discovery;
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
            string provider,
            IntegrationDetails integrationDetails,
            SensitiveCallbackInfo sensitiveCallbackInfo,
            BasicAuthorisationDetail basicAuthorisationDetail)
        {
            if (sensitiveCallbackInfo.EntityId == string.Empty)
            {
                _logger.Fatal("{Parameter} cannot be empty", basicAuthorisationDetail.EncryptedUserInfo);
                return new MiddlewareAuthorisationRequest();
            }

            string shop = basicAuthorisationDetail.Shop ?? sensitiveCallbackInfo.Shop;
            string shopUrl = integrationDetails.IntegrationSetting.FormatBaseUri(shop);

            return new MiddlewareAuthorisationRequest
            {
                Provider = provider,
                StoreName = shop,
                StoreUrl = shopUrl,
                Login = basicAuthorisationDetail.Login,
                Password = basicAuthorisationDetail.Password,
                EntityId = sensitiveCallbackInfo.EntityId,
                UserId = sensitiveCallbackInfo.UserId,
                AirslipUserType = sensitiveCallbackInfo.AirslipUserType,
                Environment = integrationDetails.IntegrationSetting.Environment,
                Location = integrationDetails.IntegrationSetting.Location,
                Context = basicAuthorisationDetail.Context,
                AdditionalFieldOne = integrationDetails.IntegrationSetting.AdditionalFieldOne,
                AdditionalFieldTwo = integrationDetails.IntegrationSetting.AdditionalFieldTwo,
                AdditionalFieldThree = integrationDetails.IntegrationSetting.AdditionalFieldThree
            };
        }
    }
}