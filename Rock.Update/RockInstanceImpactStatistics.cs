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

using System;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Update
{
    public class RockInstanceImpactStatistics
    {
        private readonly IRockImpactService _rockImpactService;

        public RockInstanceImpactStatistics( IRockImpactService rockImpactService )
        {
            _rockImpactService = rockImpactService;
        }

        public void SendImpactStatisticsToSpark( bool includeOrganizationData, string version, string ipAddress, string environmentData )
        {
            ImpactLocation organizationLocation = null;
            var numberOfActiveRecords = 0;
            var publicUrl = string.Empty;
            var organizationName = string.Empty;

            using ( var rockContext = new RockContext() )
            {
                numberOfActiveRecords = new PersonService( rockContext ).Queryable( includeDeceased: false, includeBusinesses: false ).Count();

                if ( numberOfActiveRecords <= 100 || SystemSettings.GetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE ).AsDateTime().HasValue )
                {
                    return;
                }

                if ( includeOrganizationData )
                {
                    var globalAttributes = GlobalAttributesCache.Get();

                    // Fetch the organization address
                    var organizationAddressLocationGuid = globalAttributes.GetValue( "OrganizationAddress" ).AsGuid();
                    if ( !organizationAddressLocationGuid.Equals( Guid.Empty ) )
                    {
                        var location = new Rock.Model.LocationService( rockContext ).Get( organizationAddressLocationGuid );
                        if ( location != null )
                        {
                            organizationLocation = new ImpactLocation( location );
                        }
                    }
                    organizationName = globalAttributes.GetValue( "OrganizationName" );
                    publicUrl = globalAttributes.GetValue( "PublicApplicationRoot" );
                }
            }

            ImpactStatistic impactStatistic = new ImpactStatistic()
            {
                RockInstanceId = SystemSettings.GetRockInstanceId(),
                Version = version,
                IpAddress = ipAddress,
                PublicUrl = publicUrl,
                OrganizationName = organizationName,
                OrganizationLocation = organizationLocation,
                NumberOfActiveRecords = numberOfActiveRecords,
                EnvironmentData = environmentData
            };

            _rockImpactService.SendImpactStatisticsToSpark( impactStatistic );
        }
    }
}
