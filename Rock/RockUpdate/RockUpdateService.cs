using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Rock.RockUpdate.Interfaces;
using Rock.Web.Cache;

namespace Rock.RockUpdate
{
    /// <summary>
    /// Rock Update Service
    /// </summary>
    public class RockUpdateService : IRockUpdateService
    {
        private const string GET_RELEASE_LIST_URL = "http://localhost:57822/api/RockUpdate/GetReleasesList";
        private const string GET_RELEASE_LIST_SINCE_URL = "http://localhost:57822/api/RockUpdate/GetReleasesListSinceVersion";
        private const string EARLY_ACCESS_URL = "http://www.rockrms.com/api/RockUpdate/GetEarlyAccessStatus";
        private const string EARLY_ACCESS_REQUEST_URL = "http://www.rockrms.com/earlyaccessissues?RockInstanceId=";
        

        /// <summary>
        /// Gets the releases list from the rock server.
        /// </summary>
        /// <returns></returns>
        public List<RockRelease> GetReleasesList( Version version )
        {
            var request = new RestRequest( Method.GET );

            request.RequestFormat = DataFormat.Json;

            request.AddParameter( "rockInstanceId", Web.SystemSettings.GetRockInstanceId() );
            request.AddParameter( "releaseProgram", GetRockReleaseProgram().ToString().ToLower() );

            if ( version != null )
            {
                request.AddParameter( "sinceVersion", version.ToString() );
            }

            var client = new RestClient( version != null ? GET_RELEASE_LIST_SINCE_URL : GET_RELEASE_LIST_URL );
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return JsonConvert.DeserializeObject<List<RockRelease>>( response.Content );
            }

            return new List<RockRelease>();
        }

        /// <summary>
        /// Checks the early access status of this organization.
        /// </summary>
        /// <returns></returns>
        public bool IsEarlyAccessInstance()
        {
            var client = new RestClient( EARLY_ACCESS_URL );
            var request = new RestRequest( Method.GET );
            request.RequestFormat = DataFormat.Json;

            request.AddParameter( "rockInstanceId", Web.SystemSettings.GetRockInstanceId() );
            IRestResponse response = client.Execute( request );
            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return response.Content.AsBoolean();
            }

            return false;
        }

        /// <summary>
        /// Gets the rock early access request URL.
        /// </summary>
        /// <returns></returns>
        public string GetRockEarlyAccessRequestUrl()
        {
            return $"{EARLY_ACCESS_REQUEST_URL}{Web.SystemSettings.GetRockInstanceId()}";
        }

        /// <summary>
        /// Gets the rock release program.
        /// </summary>
        /// <returns></returns>
        public RockReleaseProgram GetRockReleaseProgram()
        {
            var releaseProgram = RockReleaseProgram.Production;

            var updateUrl = GlobalAttributesCache.Get().GetValue( "UpdateServerUrl" );
            if ( updateUrl.Contains( RockReleaseProgram.Alpha.ToString().ToLower() ) )
            {
                releaseProgram = RockReleaseProgram.Alpha;
            }
            else if ( updateUrl.Contains( RockReleaseProgram.Beta.ToString().ToLower() ) )
            {
                releaseProgram = RockReleaseProgram.Beta;
            }

            return releaseProgram;
        }

       
    }
}
