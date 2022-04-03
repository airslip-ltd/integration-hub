using Airslip.Common.Types.Interfaces;

namespace Airslip.IntegrationHub.Core.Responses;

public class MarketplaceChallengeResponse : ISuccess
{
    public string ChallengeResponse { get; }
    public bool IsIdentical { get; }

    public MarketplaceChallengeResponse(string challengeResponse, bool isIdentical)
    {
        ChallengeResponse = challengeResponse;
        IsIdentical = isIdentical;
    }
}