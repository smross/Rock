// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using RestSharp;

namespace Rock.Update
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
