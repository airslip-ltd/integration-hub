using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Microsoft.Azure.Functions.Worker.Http;
using Serilog;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Implementations;

public class RequestValidationService : IRequestValidationService
{
    private readonly IAuthorisationPreparationService _authorisationPreparation;
    private readonly IHmacService _hmacService;
    private readonly ILogger _logger;

    public RequestValidationService(IAuthorisationPreparationService authorisationPreparation, 
        IHmacService hmacService, ILogger logger)
    {
        _authorisationPreparation = authorisationPreparation;
        _hmacService = hmacService;
        _logger = logger;
    }
    
    public bool ValidateRequest(string provider, HttpRequestData req)
    {
        bool supportedProvider = provider.TryParseIgnoreCase(out PosProviders parsedProvider);

        if (!supportedProvider)
        {
            _logger.Warning("{Provider} is an unsupported provider", provider);
            return false;
        }

        List<KeyValuePair<string, string>> queryStrings = _authorisationPreparation.GetParameters(parsedProvider, req);
                
        return _hmacService.Validate(parsedProvider, queryStrings);
    }
}