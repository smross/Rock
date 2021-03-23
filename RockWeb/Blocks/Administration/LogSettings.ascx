<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LogSettings.ascx.cs" Inherits="RockWeb.Blocks.Administration.LogSettings" %>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <rock:modalalert id="mdAlert" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-wrench"></i> Log Settings</h1>
            </div>
            <div class="panel-body">
                <asp:Panel runat="server" ID="pnlReadOnlySettings">
                    <Rock:RockLiteral runat="server" ID="litVerbosityLevel" Label="Verbosity Level" CssClass="col-sm-6" />
                    <Rock:RockLiteral runat="server" ID="litDomains" Label="Domains" CssClass="col-sm-6" />
                    <asp:Button runat="server" ID="btnEdit" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlEditSettings" Visible="false">
                    <rock:notificationbox id="nbLoggingMessage" runat="server" notificationboxtype="Warning" title="Warning" visible="false" />

                    <rock:rockradiobuttonlist id="rblVerbosityLevel" runat="server"
                        label="Verbosity Level"
                        help="The specified value indicates which logging level events should be written to the log file."
                        repeatdirection="Horizontal"
                        validationgroup="LoggingSettings"></rock:rockradiobuttonlist>

                    <rock:rockcheckboxlist id="cblDomainsToLog" runat="server"
                        label="Domains to Output"
                        validationgroup="LoggingSettings"
                        repeatcolumns="5"
                        repeatdirection="Horizontal" />

                    <rock:numberbox runat="server" id="txtMaxFileSize" label="Max File Size (MB)"
                        help="The maximum size that the output file is allowed to reach before being rolled over to backup files."
                        cssclass="input-width-md js-max-file-size"
                        validationgroup="LoggingSettings"></rock:numberbox>

                    <rock:numberbox runat="server" id="txtFilesToRetain" label="Retained Backup Files"
                        help="The maximum number of backup files that are kept before the oldest is erased."
                        cssclass="input-width-md js-files-to-retain"
                        validationgroup="LoggingSettings"></rock:numberbox>

                    <p>Logs could take up to <span id="maxLogSize">400</span> MB on the server's filesystem.</p>

                    <div class="actions margin-t-lg">
                        <rock:bootstrapbutton
                            id="btnLoggingSave"
                            runat="server"
                            cssclass="btn btn-primary"
                            text="Save"
                            dataloadingtext="Saving..."
                            validationgroup="LoggingSetting"
                            onclick="btnLoggingSave_Click" />

                        <rock:bootstrapbutton
                            id="btnDeleteLog"
                            runat="server"
                            cssclass="btn btn-link"
                            text="Delete Log"
                            dataloadingtext="Deleting Log ..."
                            onclick="btnDeleteLog_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
