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
using Rock;
using Rock.Logging;
using Rock.Security;
using Rock.SystemKey;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Used to view the <see cref="Rock.Logging.RockLogEvent"/> logged via RockLogger.
    /// </summary>
    [System.ComponentModel.DisplayName( "Log Settings" )]
    [System.ComponentModel.Category( "Administration" )]
    [System.ComponentModel.Description( "Block to edit system log settings." )]

    public partial class LogSettings : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if( !IsUserAuthorized( Authorization.EDIT ) )
            {
                btnEdit.Visible = false;
            }
        }
        #endregion

        protected void btnLoggingSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            nbLoggingMessage.Visible = true;

            var logConfig = new RockLogSystemSettings
            {
                LogLevel = rblVerbosityLevel.SelectedValue.ConvertToEnum<RockLogLevel>( RockLogLevel.Off ),
                DomainsToLog = cblDomainsToLog.SelectedValues,
                MaxFileSize = txtMaxFileSize.Text.AsInteger(),
                NumberOfLogFiles = txtFilesToRetain.Text.AsInteger()
            };

            Rock.Web.SystemSettings.SetValue( SystemSetting.ROCK_LOGGING_SETTINGS, logConfig.ToJson() );

            Rock.Logging.RockLogger.Log.ReloadConfiguration();

            nbLoggingMessage.NotificationBoxType = NotificationBoxType.Success;
            nbLoggingMessage.Title = string.Empty;
            nbLoggingMessage.Text = "Setting saved successfully.";
        }

        protected void btnDeleteLog_Click( object sender, EventArgs e )
        {
            nbLoggingMessage.Visible = true;

            RockLogger.Log.Delete();

            nbLoggingMessage.NotificationBoxType = NotificationBoxType.Success;
            nbLoggingMessage.Title = string.Empty;
            nbLoggingMessage.Text = "The log files were successfully deleted.";
        }

        protected void btnEdit_Click( object sender, EventArgs e )
        {
            if ( IsUserAuthorized( Authorization.EDIT ) )
            {
                pnlEditSettings.Visible = true;
                pnlReadOnlySettings.Visible = false;
                HideSecondaryBlocks( true );
            }
        }
    }
}