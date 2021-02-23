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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rock.Data;
using Rock.Model;
using Rock.Update;
using Rock.Update.Interfaces;
using Rock.Update.Models;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.RockUpdate
{
    [TestClass]
    public class RockInstanceImpactStatisticsTests : BaseRockTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CleanupTestData();
        }

        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldNotSendDataToServiceWhenSampleDataInUse()
        {
            EnsureMoreThen100Records();
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, DateTime.Now.ToString() );
            Thread.Sleep( 500 );

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( false, "1.13.0", "0.0.0.0", "data" );
            rockImpactService.Verify( x => x.SendImpactStatisticsToSpark( It.IsAny<ImpactStatistic>() ), Times.Never );
        }

        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldNotSendDataToServiceWhenFewerThen100Records()
        {
            EnsureFewerThen100Records();

            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, string.Empty );

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( false, "1.13.0", "0.0.0.0", "data" );
            rockImpactService.Verify( x => x.SendImpactStatisticsToSpark( It.IsAny<ImpactStatistic>() ), Times.Never );
        }

        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldSendDataToService()
        {
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, string.Empty );

            EnsureMoreThen100Records();

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( false, "1.13.0", "0.0.0.0", "data" );
            rockImpactService.Verify( x => x.SendImpactStatisticsToSpark( It.IsAny<ImpactStatistic>() ), Times.Once );
        }

        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldNotIncludeOrganizationData()
        {
            var expectedInstanceId = SystemSettings.GetRockInstanceId();
            var expectedVersion = "1.13.0";
            var expectedIpAddress = "192.168.1.0";
            var expectedEnvironmentData = "data";

            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, string.Empty );

            EnsureMoreThen100Records();

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( false, expectedVersion, expectedIpAddress, "data" );
            rockImpactService.Verify(
                x => x.SendImpactStatisticsToSpark( It.Is<ImpactStatistic>( i =>
                    i.RockInstanceId == expectedInstanceId
                    && i.Version == expectedVersion
                    && i.IpAddress == expectedIpAddress
                    && i.PublicUrl.IsNullOrWhiteSpace()
                    && i.OrganizationName.IsNullOrWhiteSpace()
                    && i.OrganizationLocation == null
                    && i.NumberOfActiveRecords == 0
                    && i.EnvironmentData == expectedEnvironmentData ) ),
                Times.Once );
        }

        /// <summary>
        /// Sends the impact statistics to spark should include organization data.
        /// </summary>
        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldIncludeOrganizationData()
        {
            var expectedInstanceId = SystemSettings.GetRockInstanceId();
            var expectedVersion = "1.13.0";
            var expectedIpAddress = "192.168.1.0";
            var expectedEnvironmentData = "data";

            var globalAttributes = GlobalAttributesCache.Get();
            var expectedOrganizationName = globalAttributes.GetValue( "OrganizationName" );
            var expectedPublicUrl = globalAttributes.GetValue( "PublicApplicationRoot" );

            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, string.Empty );

            EnsureMoreThen100Records();
            var expectedNumberOfRecords = 0;
            using ( var rockContext = new RockContext() )
            {
                expectedNumberOfRecords = new PersonService( rockContext ).Queryable( includeDeceased: false, includeBusinesses: false ).Count();
            }

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( true, expectedVersion, expectedIpAddress, "data" );
            rockImpactService.Verify(
                x => x.SendImpactStatisticsToSpark( It.Is<ImpactStatistic>( i =>
                    i.RockInstanceId == expectedInstanceId
                    && i.Version == expectedVersion
                    && i.IpAddress == expectedIpAddress
                    && i.PublicUrl == expectedPublicUrl
                    && i.OrganizationName == expectedOrganizationName
                    && i.OrganizationLocation != null
                    && i.NumberOfActiveRecords == expectedNumberOfRecords
                    && i.EnvironmentData == expectedEnvironmentData ) ),
                Times.Once );
        }

        private void EnsureMoreThen100Records()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var numberOfActiveRecords = personService.Queryable( includeDeceased: false, includeBusinesses: false ).Count();
                var numberOfRecordsToCreate = 101 - numberOfActiveRecords;

                while ( numberOfRecordsToCreate > 0 )
                {
                    var person = new Person
                    {
                        FirstName = Guid.NewGuid().ToString(),
                        LastName = Guid.NewGuid().ToString(),
                        Email = $"{Guid.NewGuid()}@test.com",
                    };
                    personService.Add( person );
                    numberOfRecordsToCreate--;
                }

                rockContext.SaveChanges();
            }
        }

        private void EnsureFewerThen100Records()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var numberOfActiveRecords = personService.Queryable( includeDeceased: false, includeBusinesses: false ).Count();
                var numberOfRecordsToRemove = numberOfActiveRecords - 50;

                while ( numberOfRecordsToRemove > 0 )
                {
                    var person = personService.Queryable( includeDeceased: false, includeBusinesses: false ).FirstOrDefault();
                    person.ForeignKey = "RockImpactTest";
                    person.RecordStatusReasonValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid() ).Id;
                    person.RecordStatusValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
                    person.IsDeceased = true;
                    numberOfRecordsToRemove--;
                    rockContext.SaveChanges();
                }
            }
        }

        private void CleanupTestData()
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( @"DELETE PersonSearchKey
                        WHERE EXISTS(
                        SELECT 1
                        FROM Person
                        INNER JOIN PersonAlias ON PersonAlias.PersonId = Person.Id
                        WHERE Email LIKE '%-%-%-%-%@test.com' AND PersonAlias.Id = PersonAliasId
                        )
                        DELETE PersonAlias WHERE EXISTS(
                        SELECT 1
                        FROM Person
                        WHERE Email LIKE '%-%-%-%-%@test.com' AND Id = PersonId
                        )
                        DELETE Person
                        WHERE Email LIKE '%-%-%-%-%@test.com'" );

                rockContext.Database.ExecuteSqlCommand( $@"UPDATE Person
                        SET IsDeceased = 1
	                        , RecordStatusReasonValueId = NULL
	                        , RecordStatusValueId = {DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id}
	                        , ForeignKey = NULL
                        WHERE ForeignKey = 'RockImpactTest'" );
            }
        }
    }
}
