using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace Rock.RockUpdate
{
    public class RockImpactService : IRockImpactService
    {
        private const string SEND_IMPACT_URL = "http://www.rockrms.com/api/impacts/save";

        public void SendImpactStatisticsToSpark( ImpactStatistic impactStatistic )
        {
            var client = new RestClient( SEND_IMPACT_URL );
            var request = new RestRequest( Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddBody( impactStatistic );
            client.Execute( request );
        }
    }
}
