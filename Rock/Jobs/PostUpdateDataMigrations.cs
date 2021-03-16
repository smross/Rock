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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper" )]
    [Description( "This job will run data migrations that need to ru." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, some updates could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]

    [BooleanField(
        "Post_V10_0_SundayDate_Migration",
        Key = AttributeKey.Post_V10_0_SundayDate_Migration_HasCompleted,

        DefaultBooleanValue = false )]

    [BooleanField(
        "Post_V12_4_InteractionIndexes_Migration_HasCompleted",
        Key = AttributeKey.Post_V12_4_InteractionIndexes_Migration_HasCompleted,
        DefaultBooleanValue = false )]
    public class PostUpdateDataMigrations : IJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
            public const string Post_V10_0_SundayDate_Migration_HasCompleted = "Post_V10_SundayDate_Migration_HasCompleted";
            public const string Post_V12_4_InteractionIndexes_Migration_HasCompleted = "Post_V12_4_InteractionIndexes_Migration_HasCompleted";
        }

        private int databaseCommandTimeout;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var jobId = context.GetJobId();
            var rockContext = new RockContext();
            var postUpdateDataMigrationsJob = new ServiceJobService( rockContext ).Get( jobId );

            // get the configured timeout, or default to 60 minutes if it is blank
            databaseCommandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            if ( postUpdateDataMigrationsJob.GetAttributeValue( AttributeKey.Post_V10_0_SundayDate_Migration_HasCompleted ).AsBoolean() == false )
            {
                if ( Post_V10_0_SundayDate_Migration() )
                {
                    postUpdateDataMigrationsJob.SetAttributeValue( AttributeKey.Post_V10_0_SundayDate_Migration_HasCompleted, true.ToTrueFalse() );
                }
            }

            if ( postUpdateDataMigrationsJob.GetAttributeValue( AttributeKey.Post_V12_4_InteractionIndexes_Migration_HasCompleted ).AsBoolean() == false )
            {
                if ( Post_V12_4_InteractionIndexes_Migration() )
                {
                    postUpdateDataMigrationsJob.SetAttributeValue( AttributeKey.Post_V12_4_InteractionIndexes_Migration_HasCompleted, true.ToTrueFalse() );
                }
            }

            postUpdateDataMigrationsJob.SaveAttributeValues();
        }


        private bool Post_V12_4_InteractionIndexes_Migration()
        {
            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = databaseCommandTimeout;

            // some run-twice-safe data migration ...

            return true;
        }


        private bool Post_V10_0_SundayDate_Migration()
        {
            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = databaseCommandTimeout;

            // some run-twice-safe data migration ...

            return true;
        }
    }
}
