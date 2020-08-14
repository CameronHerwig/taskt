﻿using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using MimeKit.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Authentication;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using taskt.Core.Attributes.ClassAttributes;
using taskt.Core.Attributes.PropertyAttributes;
using taskt.Core.Command;
using taskt.Core.Enums;
using taskt.Core.Infrastructure;
using taskt.Core.Utilities.CommonUtilities;
using taskt.Engine;
using taskt.UI.CustomControls;

namespace taskt.Commands
{
    [Serializable]
    [Group("Email Commands")]
    [Description("This command sends an email with optional attachment(s) using SMTP protocol.")]

    public class GetIMAPEmailsCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Host")]
        [InputSpecification("Define the host/service name that the script should use.")]
        [SampleUsage("imap.gmail.com || {vHost}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_IMAPHost { get; set; }

        [XmlAttribute]
        [PropertyDescription("Port")]
        [InputSpecification("Define the port number that should be used when contacting the SMTP service.")]
        [SampleUsage("993 || {vPort}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public int v_IMAPPort { get; set; }

        [XmlAttribute]
        [PropertyDescription("Username")]
        [InputSpecification("Define the username to use when contacting the SMTP service.")]
        [SampleUsage("myRobot || {vUsername}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_IMAPUserName { get; set; }

        [XmlAttribute]
        [PropertyDescription("Password")]
        [InputSpecification("Define the password to use when contacting the SMTP service.")]
        [SampleUsage("password || {vPassword}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_IMAPPassword { get; set; }

        [XmlAttribute]
        [PropertyDescription("Source Mail Folder Name")]
        [InputSpecification("Enter the name of the mail folder the emails are located in.")]
        [SampleUsage("Inbox || {vFolderName}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_IMAPSourceFolder { get; set; }

        [XmlAttribute]
        [PropertyDescription("Filter")]
        [InputSpecification("Enter a valid Outlook filter string.")]
        [SampleUsage("[Subject] = 'Hello' || [Subject] = 'Hello' and [SenderName] = 'Jane Doe' || {vFilter} || None")]
        [Remarks("*Warning* Using 'None' as the Filter will return every MailItem in the selected Mail Folder.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_IMAPFilter { get; set; }

        [XmlAttribute]
        [PropertyDescription("Unread Only")]
        [PropertyUISelectionOption("Yes")]
        [PropertyUISelectionOption("No")]
        [InputSpecification("Specify whether to retrieve unread email messages only.")]
        [SampleUsage("")]
        [Remarks("")]
        public string v_IMAPGetUnreadOnly { get; set; }

        [XmlAttribute]
        [PropertyDescription("Mark As Read")]
        [PropertyUISelectionOption("Yes")]
        [PropertyUISelectionOption("No")]
        [InputSpecification("Specify whether to mark retrieved emails as read.")]
        [SampleUsage("")]
        [Remarks("")]
        public string v_IMAPMarkAsRead { get; set; }

        [XmlAttribute]
        [PropertyDescription("Save MailItems and Attachments")]
        [PropertyUISelectionOption("Yes")]
        [PropertyUISelectionOption("No")]
        [InputSpecification("Specify whether to save the email attachments to a local directory.")]
        [SampleUsage("")]
        [Remarks("")]
        public string v_IMAPSaveMessagesAndAttachments { get; set; }

        [XmlAttribute]
        [PropertyDescription("Output MailItem Directory")]
        [InputSpecification("Enter or Select the path of the directory to store the messages in.")]
        [SampleUsage(@"C:\temp\myfolder || {vFolderPath} || {ProjectPath}\myFolder")]
        [Remarks("This input is optional and will only be used if *Save MailItems and Attachments* is set to **Yes**.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        [PropertyUIHelper(UIAdditionalHelperType.ShowFolderSelectionHelper)]
        public string v_IMAPMessageDirectory { get; set; }

        [XmlAttribute]
        [PropertyDescription("Output Attachment Directory")]
        [InputSpecification("Enter or Select the path to the directory to store the attachments in.")]
        [SampleUsage(@"C:\temp\myfolder\attachments || {vFolderPath} || {ProjectPath}\myFolder\attachments")]
        [Remarks("This input is optional and will only be used if *Save MailItems and Attachments* is set to **Yes**.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        [PropertyUIHelper(UIAdditionalHelperType.ShowFolderSelectionHelper)]
        public string v_IMAPAttachmentDirectory { get; set; }

        [XmlAttribute]
        [PropertyDescription("Output MimeMessage List Variable")]
        [InputSpecification("Select or provide a variable from the variable list.")]
        [SampleUsage("vUserVariable")]
        [Remarks("If you have enabled the setting **Create Missing Variables at Runtime** then you are not required" +
                " to pre-define your variables; however, it is highly recommended.")]
        public string v_OutputUserVariableName { get; set; }

        //[XmlElement]
        //[PropertyDescription("SSL Validation")]
        //[PropertyUISelectionOption("Validate SSL")]
        //[PropertyUISelectionOption("Bypass SSL Validation")]
        //[InputSpecification("Select the appropriate option.")]
        //[SampleUsage("")]
        //[Remarks("This field manages whether taskt will attempt to validate the SSL connection.")]
        //public string v_SSLValidation { get; set; }

        public GetIMAPEmailsCommand()
        {
            CommandName = "GetIMAPEmailsCommand";
            SelectionName = "Get IMAP Emails";
            CommandEnabled = true;
            CustomRendering = true;
            //v_SSLValidation = "Validate SSL";
            v_IMAPSourceFolder = "INBOX";
            v_IMAPGetUnreadOnly = "No";
            v_IMAPMarkAsRead = "Yes";
            v_IMAPSaveMessagesAndAttachments = "No";
        }

        public override void RunCommand(object sender)
        {
            var engine = (AutomationEngineInstance)sender;

            string vIMAPHost = v_IMAPHost.ConvertToUserVariable(engine);
            string vIMAPPort = v_IMAPPort.ToString().ConvertToUserVariable(engine);
            string vIMAPUserName = v_IMAPUserName.ConvertToUserVariable(engine);
            string vIMAPPassword = v_IMAPPassword.ConvertToUserVariable(engine);
            string vIMAPSourceFolder = v_IMAPSourceFolder.ConvertToUserVariable(engine);
            string vIMAPFilter = v_IMAPFilter.ConvertToUserVariable(engine);
            string vIMAPMessageDirectory = v_IMAPMessageDirectory.ConvertToUserVariable(engine);
            string vIMAPAttachmentDirectory = v_IMAPAttachmentDirectory.ConvertToUserVariable(engine);

            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (sndr, certificate, chain, sslPolicyErrors) => true;
                client.SslProtocols = SslProtocols.Tls12;

                using (var cancel = new CancellationTokenSource())
                {
                    client.Connect(vIMAPHost, int.Parse(vIMAPPort), true, cancel.Token);

                    // If you want to disable an authentication mechanism,
                    // you can do so by removing the mechanism like this:
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(vIMAPUserName, vIMAPPassword, cancel.Token);

                    IMailFolder toplevel = client.GetFolder(client.PersonalNamespaces[0]);
                    IMailFolder foundFolder = FindFolder(toplevel, vIMAPSourceFolder);

                    if (foundFolder != null)
                        foundFolder.Open(FolderAccess.ReadWrite, cancel.Token);
                    else
                        throw new Exception("Source Folder not found");

                    SearchQuery query;
                    if (!string.IsNullOrEmpty(vIMAPFilter))
                    {
                        query = SearchQuery.MessageContains(vIMAPFilter)
                            .Or(SearchQuery.SubjectContains(vIMAPFilter))
                            .Or(SearchQuery.FromContains(vIMAPFilter))
                            .Or(SearchQuery.BccContains(vIMAPFilter))
                            .Or(SearchQuery.BodyContains(vIMAPFilter))
                            .Or(SearchQuery.CcContains(vIMAPFilter))
                            .Or(SearchQuery.ToContains(vIMAPFilter));
                    }
                    else
                        query = SearchQuery.All;

                    if (v_IMAPGetUnreadOnly == "Yes")
                        query = query.And(SearchQuery.NotSeen);

                    var filteredItems = foundFolder.Search(query, cancel.Token);

                    List<MimeMessage> outMail = new List<MimeMessage>();

                    foreach (UniqueId uid in filteredItems)
                    {
                        if (v_IMAPMarkAsRead == "Yes")
                            foundFolder.AddFlags(uid, MessageFlags.Seen, true);

                        MimeMessage message = foundFolder.GetMessage(uid, cancel.Token);

                        if (v_IMAPSaveMessagesAndAttachments == "Yes")                       
                            ProcessEmail(message, vIMAPMessageDirectory, vIMAPAttachmentDirectory);
                        
                        outMail.Add(message);

                    }
                    engine.AddVariable(v_OutputUserVariableName, outMail);

                    client.Disconnect(true, cancel.Token);
                    client.ServerCertificateValidationCallback = null;
                }
            }           
        }

        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPHost", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPPort", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPUserName", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPPassword", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPSourceFolder", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPFilter", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_IMAPGetUnreadOnly", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_IMAPMarkAsRead", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_IMAPSaveMessagesAndAttachments", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPMessageDirectory", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPAttachmentDirectory", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultOutputGroupFor("v_OutputUserVariableName", this, editor));

            //RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_SSLValidation", this, editor));

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + $" [From '{v_IMAPSourceFolder}' - Filter by '{v_IMAPFilter}' - Store MimeMessage List in '{v_OutputUserVariableName}']";
        }

        private IMailFolder FindFolder(IMailFolder toplevel, string name)
        {
            var subfolders = toplevel.GetSubfolders().ToList();

            foreach (var subfolder in subfolders)
            {
                if (subfolder.Name == name)
                    return subfolder;
            }

            foreach (var subfolder in subfolders)
            {
                var folder = FindFolder(subfolder, name);

                if (folder != null)
                    return folder;
            }

            return null;
        }

        private void ProcessEmail(MimeMessage message, string msgDirectory, string attDirectory)
        {
            if (Directory.Exists(msgDirectory))
                message.WriteTo(Path.Combine(msgDirectory, message.Subject + ".eml"));

            if (Directory.Exists(attDirectory))
            {
                foreach (var attachment in message.Attachments)
                {
                    if (attachment is MessagePart)
                    {
                        var fileName = attachment.ContentDisposition?.FileName;
                        var rfc822 = (MessagePart)attachment;

                        if (string.IsNullOrEmpty(fileName))
                            fileName = "attached-message.eml";

                        using (var stream = File.Create(Path.Combine(attDirectory, fileName)))
                            rfc822.Message.WriteTo(stream);
                    }
                    else
                    {
                        var part = (MimePart)attachment;
                        var fileName = part.FileName;

                        using (var stream = File.Create(Path.Combine(attDirectory, fileName)))
                            part.Content.DecodeTo(stream);
                    }
                }
            }           
        }
    }
}
