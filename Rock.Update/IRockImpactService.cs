using System;

namespace Rock.Update
{
    public interface IRockImpactService
    {
        void SendImpactStatisticsToSpark( ImpactStatistic impactStatistic );
    }
}