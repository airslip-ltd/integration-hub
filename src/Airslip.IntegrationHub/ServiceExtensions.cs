using Airslip.Common.Types.Configuration;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Airslip.IntegrationHub
{
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
                .Configure<SettingCollection<ProviderSetting>>(configuration.GetSection("ProviderSettings"))
                .AddScoped<IProviderDiscoveryService, ProviderDiscoveryService>()
                .AddScoped<ICustomerPortalClient, CustomerPortalClient>();

            return services;
        }
    }
}