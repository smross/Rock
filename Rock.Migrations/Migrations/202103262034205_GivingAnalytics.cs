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
namespace Rock.Migrations
{
    /// <summary>
    /// GivingAnalytics
    /// </summary>
    public partial class GivingAnalytics : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JobUp();
            AlterContributionTabBlocksUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterContributionTabBlocksDown();
            JobDown();
        }

        /// <summary>
        /// Job up.
        /// </summary>
        private void JobUp()
        {
            Sql( $@"
                INSERT INTO ServiceJob (
                    IsSystem,
                    IsActive,
                    Name,
                    Description,
                    Class,
                    Guid,
                    CreatedDateTime,
                    NotificationStatus,
                    CronExpression
                ) VALUES (
                    1, -- IsSystem
                    1, -- IsActive
                    'Giving Analytics', -- Name
                    'Job that updates giving classification attributes as well as creating giving alerts.', -- Description
                    'Rock.Jobs.GivingAnalytics', -- Class
                    '{Rock.SystemGuid.ServiceJob.GIVING_ANALYTICS}', -- Guid
                    GETDATE(), -- Created
                    1, -- All notifications
                    '0 0 22 * * ?' -- Cron: 10pm everyday
                );" );
        }

        /// <summary>
        /// Job down.
        /// </summary>
        private void JobDown()
        {
            Sql( $"DELETE FROM ServiceJob WHERE Guid = '{Rock.SystemGuid.ServiceJob.GIVING_ANALYTICS}';" );
        }

        /// <summary>
        /// Alters the contribution tab blocks up.
        /// </summary>
        private void AlterContributionTabBlocksUp()
        {
            RockMigrationHelper.DeleteAttribute( "A6B71434-FD9B-45EC-AA50-07AE5D6BA384" );
            RockMigrationHelper.DeleteAttribute( "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17" );
            RockMigrationHelper.DeleteAttribute( "375F7220-04C6-4E41-B99A-A2CE494FD74A" );
            RockMigrationHelper.DeleteAttribute( "47B99CD1-FB63-44D7-8586-45BDCDF51137" );
            RockMigrationHelper.DeleteAttribute( "9BCE3FD8-9014-4120-9DCC-06C4936284BA" );
            RockMigrationHelper.DeleteBlock( "B33DF8C4-29B2-4DC5-B182-61FC255B01C0" );

            RockMigrationHelper.DeleteAttribute( "A6B71434-FD9B-45EC-AA50-07AE5D6BA384" );
            RockMigrationHelper.DeleteAttribute( "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17" );
            RockMigrationHelper.DeleteAttribute( "375F7220-04C6-4E41-B99A-A2CE494FD74A" );
            RockMigrationHelper.DeleteAttribute( "47B99CD1-FB63-44D7-8586-45BDCDF51137" );
            RockMigrationHelper.DeleteAttribute( "9BCE3FD8-9014-4120-9DCC-06C4936284BA" );
            RockMigrationHelper.DeleteBlock( "B33DF8C4-29B2-4DC5-B182-61FC255B01C0" );

            RockMigrationHelper.DeleteAttribute( "3E26B7DF-7A7F-4829-987F-47304C0F845E" );
            RockMigrationHelper.DeleteAttribute( "E9245CFD-4B11-4CE2-A120-BB3AC47C0974" );
            RockMigrationHelper.DeleteBlock( "212EB093-026A-4177-ACE4-25EA9E1DDD41" );

            RockMigrationHelper.DeleteAttribute( "A20E6DB1-B830-4D41-9003-43A184E4C910" );
            RockMigrationHelper.DeleteAttribute( "55FDABC3-22AE-4EE4-9883-8234E3298B99" );
            RockMigrationHelper.DeleteAttribute( "8158A336-8129-4E82-8B61-8C0E883CB91A" );
            RockMigrationHelper.DeleteAttribute( "9227E7D4-5725-49BD-A0B1-43B769E0A529" );
            RockMigrationHelper.DeleteAttribute( "5C3A012C-19A2-4EC7-8440-7534FE175591" );
            RockMigrationHelper.DeleteAttribute( "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6" );
            RockMigrationHelper.DeleteAttribute( "7B554631-3CD5-40C4-8E67-ECED56D4D7C1" );
            RockMigrationHelper.DeleteAttribute( "AC1EF7F3-7B06-4978-84DD-B38025FC2E7B" );
            RockMigrationHelper.DeleteAttribute( "346384B5-1ECE-4949-BFF4-712E1FAA4335" );
            RockMigrationHelper.DeleteAttribute( "5B439A86-D2AD-4223-8D1E-A50FF883D7C2B" );
            RockMigrationHelper.DeleteAttribute( "F37EB885-416A-4B70-B48E-8A25557C7B12" );
            RockMigrationHelper.DeleteAttribute( "F9A168F1-3E59-4C5F-8019-7B17D00B94C9" );
            RockMigrationHelper.DeleteBlock( "96599B45-E080-44AE-8CB7-CCCCA4873398" );

            RockMigrationHelper.DeleteAttribute( "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D" );
            RockMigrationHelper.DeleteAttribute( "831403EB-262E-4BC5-8B5E-F16153493BF5" );
            RockMigrationHelper.DeleteBlock( "013ACB2A-48AD-4325-9566-6A6B821C8C21" );

            RockMigrationHelper.DeleteBlock( "EF8BB598-E991-421F-96A1-3019B3D855A6" );

            RockMigrationHelper.DeleteBlock( "7C698D61-81C9-4942-BFE3-9839130C1A3E" );

            // Add/Update BlockType Giving Configuration        
            RockMigrationHelper.UpdateBlockType( "Giving Configuration", "Block used to view the giving.", "~/Blocks/Crm/PersonDetail/GivingConfiguration.ascx", "CRM > Person Detail", "486E470A-DBD8-48D6-9A97-5B1B490A401E" );
            // Add/Update BlockType Giving Overview      
            RockMigrationHelper.UpdateBlockType( "Giving Overview", "Block used to view the giving.", "~/Blocks/Crm/PersonDetail/GivingOverview.ascx", "CRM > Person Detail", "8C3C98EE-3427-4218-BF79-43C43B25CF07" );
            // Add Block Giving Configuration to Page: Contributions, Site: Rock RMS   
            RockMigrationHelper.AddBlock( true, "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "486E470A-DBD8-48D6-9A97-5B1B490A401E".AsGuid(), "Giving Configuration", "SectionA2", @"", @"", 0, "21B28504-6ED3-44E2-BB85-3401F8B1B96A" );
            // Add Block Giving Overview to Page: Contributions, Site: Rock RMS      
            RockMigrationHelper.AddBlock( true, "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8C3C98EE-3427-4218-BF79-43C43B25CF07".AsGuid(), "Giving Overview", "SectionA1", @"", @"", 0, "8A8806DB-78F8-42C5-9D09-3723A868D976" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            // Update Order for Page: Contributions,  Zone: SectionA1,  Block: Giving Overview    
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '8A8806DB-78F8-42C5-9D09-3723A868D976'" );
            // Update Order for Page: Contributions,  Zone: SectionA1,  Block: Transaction Yearly Summary   
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'EF8BB598-E991-421F-96A1-3019B3D855A6'" );
            // Update Order for Page: Contributions,  Zone: SectionA2,  Block: Contribution Statement List Lava  
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '96599B45-E080-44AE-8CB7-CCCCA4873398'" );
            // Update Order for Page: Contributions,  Zone: SectionA2,  Block: Giving Configuration         
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '21B28504-6ED3-44E2-BB85-3401F8B1B96A'" );
            // Update Order for Page: Contributions,  Zone: SectionA2,  Block: Person Transaction Links    
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '013ACB2A-48AD-4325-9566-6A6B821C8C21'" );
            // Update Order for Page: Contributions,  Zone: SectionC1,  Block: Bank Account List        
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '7C698D61-81C9-4942-BFE3-9839130C1A3E'" );
            // Update Order for Page: Contributions,  Zone: SectionC1,  Block: Finance - Giving Profile List              Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B33DF8C4-29B2-4DC5-B182-61FC255B01C0'"  );
            // Update Order for Page: Contributions,  Zone: SectionC1,  Block: Pledge List              Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '212EB093-026A-4177-ACE4-25EA9E1DDD41'"  );
            // Update Order for Page: Contributions,  Zone: SectionC1,  Block: Saved Account List              Sql( @"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = '6B21B99D-F048-4DEA-B994-A16972EA87FE'"  );
            // Update Order for Page: Contributions,  Zone: SectionC1,  Block: Transaction List              Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '9382B285-3EF6-47F7-94BB-A47C498196A3'"  );
            // Attribute for BlockType: Giving Configuration:Person Token Expire Minutes  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486E470A-DBD8-48D6-9A97-5B1B490A401E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Expire Minutes", "PersonTokenExpireMinutes", "Person Token Expire Minutes", @"The number of minutes the person token for the transaction is valid after it is issued.", 1, @"60", "F94B9ADD-4684-4196-8F51-B77352E327B1" );
            // Attribute for BlockType: Giving Configuration:Person Token Usage Limit   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486E470A-DBD8-48D6-9A97-5B1B490A401E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Usage Limit", "PersonTokenUsageLimit", "Person Token Usage Limit", @"The maximum number of times the person token for the transaction can be used.", 2, @"1", "07C2DDAB-9DD8-4E12-B903-8B2021F7BA4D" );
            // Attribute for BlockType: Giving Configuration:Max Years To Display  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486E470A-DBD8-48D6-9A97-5B1B490A401E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Years To Display", "MaxYearsToDisplay", "Max Years To Display", @"The maximum number of years to display (including the current year).", 5, @"3", "61937426-9DB3-4B2F-9448-F6F7E6B0539F" );
            // Attribute for BlockType: Giving Configuration:Add Transaction Page   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486E470A-DBD8-48D6-9A97-5B1B490A401E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Transaction Page", "AddTransactionPage", "Add Transaction Page", @"", 0, @"B1CA86DC-9890-4D26-8EBD-488044E1B3DD", "FAC63421-C934-4563-AAEE-E0EE83514B67" );
            // Attribute for BlockType: Giving Configuration:Contribution Statement Detail Page  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486E470A-DBD8-48D6-9A97-5B1B490A401E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Contribution Statement Detail Page", "ContributionStatementDetailPage", "Detail Page", @"The contribution statement detail page.", 6, @"", "0023B662-B17F-407A-8E50-DA1B5090CD7B" );
            // Attribute for BlockType: Giving Configuration:Pledge Detail Page   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486E470A-DBD8-48D6-9A97-5B1B490A401E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Pledge Detail Page", "PledgeDetailPage", "Pledge Detail Page", @"", 4, @"EF7AA296-CA69-49BC-A28B-901A8AAA9466", "B0F0F89E-400B-4BD9-A6CB-B1DE550CDBC7" );
            // Attribute for BlockType: Giving Configuration:Accounts       
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486E470A-DBD8-48D6-9A97-5B1B490A401E", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"A selection of accounts to use for checking if transactions for the current user exist.", 3, @"", "B7D51A75-A356-460C-BF6D-CA1AA0F3BF84" );
            // Attribute for BlockType: Giving Overview:Inactive Giver Cutoff (Days)  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8C3C98EE-3427-4218-BF79-43C43B25CF07", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Inactive Giver Cutoff (Days)", "InactiveGiverCutoff", "Inactive Giver Cutoff (Days)", @"The number of days after which a person is considered an inactive giver.", 0, @"365", "5796A617-552F-4CCE-B40B-9A7162B6FE6D" );
        }

        /// <summary>
        /// Alters the contribution tab blocks down.
        /// </summary>
        private void AlterContributionTabBlocksDown()
        {
            // Inactive Giver Cutoff (Days) Attribute for BlockType: Giving Overview              
            RockMigrationHelper.DeleteAttribute( "5796A617-552F-4CCE-B40B-9A7162B6FE6D" );
            // Pledge Detail Page Attribute for BlockType: Giving Configuration   
            RockMigrationHelper.DeleteAttribute( "B0F0F89E-400B-4BD9-A6CB-B1DE550CDBC7" );
            // Contribution Statement Detail Page Attribute for BlockType: Giving Configuration     
            RockMigrationHelper.DeleteAttribute( "0023B662-B17F-407A-8E50-DA1B5090CD7B" );
            // Max Years To Display Attribute for BlockType: Giving Configuration    
            RockMigrationHelper.DeleteAttribute( "61937426-9DB3-4B2F-9448-F6F7E6B0539F" );
            // Accounts Attribute for BlockType: Giving Configuration      
            RockMigrationHelper.DeleteAttribute( "B7D51A75-A356-460C-BF6D-CA1AA0F3BF84" );
            // Person Token Usage Limit Attribute for BlockType: Giving Configuration        
            RockMigrationHelper.DeleteAttribute( "07C2DDAB-9DD8-4E12-B903-8B2021F7BA4D" );
            // Person Token Expire Minutes Attribute for BlockType: Giving Configuration   
            RockMigrationHelper.DeleteAttribute( "F94B9ADD-4684-4196-8F51-B77352E327B1" );
            // Add Transaction Page Attribute for BlockType: Giving Configuration    
            RockMigrationHelper.DeleteAttribute( "FAC63421-C934-4563-AAEE-E0EE83514B67" );
            // Remove Block: Giving Overview, from Page: Contributions, Site: Rock RMS    
            RockMigrationHelper.DeleteBlock( "8A8806DB-78F8-42C5-9D09-3723A868D976" );
            // Remove Block: Giving Configuration, from Page: Contributions, Site: Rock RMS    
            RockMigrationHelper.DeleteBlock( "21B28504-6ED3-44E2-BB85-3401F8B1B96A" );
            // Delete BlockType Giving Overview            
            RockMigrationHelper.DeleteBlockType( "8C3C98EE-3427-4218-BF79-43C43B25CF07" ); // Giving Overview  
            // Delete BlockType Giving Configuration       
            RockMigrationHelper.DeleteBlockType( "486E470A-DBD8-48D6-9A97-5B1B490A401E" ); // Giving Configuration  
        }
    }
}