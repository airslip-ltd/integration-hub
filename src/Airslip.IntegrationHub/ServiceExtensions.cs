using Airslip.Common.Types.Configuration;
using Airslip.IntegrationHub.Core.Common;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Airslip.IntegrationHub;

public static class ServiceExtensions
{
    /// <summary>
    /// Adds consent authorisation capabilities to your app
    /// </summary>
    /// <param name="services">The service collection to append services to</param>
    /// <param name="configuration">The primary configuration where relevant elements can be found</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IServiceCollection AddProviderAuthorisation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .Configure<SettingCollection<IntegrationSetting>>(configuration.GetSection($"{nameof(IntegrationSetting)}s"))
            .AddScoped<IOAuth2Service, OAuth2Service>()
            .AddScoped<IHmacService, HmacService>()
            .AddScoped<IInternalMiddlewareService, InternalMiddlewareService>()
            .AddScoped<IAuthorisationPreparationService, AuthorisationPreparationService>()
            .AddScoped<IRequestValidationService, RequestValidationService>()
            .AddScoped<ISensitiveInformationService, SensitiveInformationService>()
            .AddScoped<IMarketplaceService, MarketplaceService>()
            .AddScoped<IIntegrationUrlService, IntegrationUrlService>()
            .AddScoped<IIntegrationDiscoveryService, IntegrationDiscoveryService>()
            .AddScoped<IMarketplaceService, MarketplaceService>();

        return services;
    }
}