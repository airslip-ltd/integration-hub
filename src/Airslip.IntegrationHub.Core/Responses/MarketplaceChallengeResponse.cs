using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Responses;

public class MarketplaceChallengeResponse : ISuccess
{
    public string ChallengeResponse { get; }

    public MarketplaceChallengeResponse(string challengeResponse)
    {
        ChallengeResponse = challengeResponse;
    }
}