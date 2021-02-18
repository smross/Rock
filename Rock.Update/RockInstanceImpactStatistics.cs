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
