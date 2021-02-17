using System;

namespace Rock.RockUpdate
{
    public interface IRockImpactService
    {
        void SendImpactStatisticsToSpark( ImpactStatistic impactStatistic );
    }
}