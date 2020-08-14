using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [Description("This command moves or copies selected emails in Outlook.")]

    public class MoveCopyIMAPEmailCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("MimeMessage")]
        [InputSpecification("Enter the MailItem to move or copy.")]
        [SampleUsage("{vMimeMessage}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_IMAPMailItem { get; set; }

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
        [PropertyDescription("Destination Mail Folder Name")]
        [InputSpecification("Enter the name of the Outlook mail folder the emails are being moved/copied to.")]
        [SampleUsage("New Folder || {vFolderName}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_IMAPDestinationFolder { get; set; }

        [XmlAttribute]
        [PropertyDescription("Mail Operation")]
        [PropertyUISelectionOption("Move MimeMessage")]
        [PropertyUISelectionOption("Copy MimeMessage")]
        [InputSpecification("Specify whether to move or copy the selected emails.")]
        [SampleUsage("")]
        [Remarks("Moving will remove the emails from the original folder while copying will not.")]
        public string v_IMAPOperationType { get; set; }

        [XmlAttribute]
        [PropertyDescription("Unread Only")]
        [PropertyUISelectionOption("Yes")]
        [PropertyUISelectionOption("No")]
        [InputSpecification("Specify whether to move/copy unread email messages only.")]
        [SampleUsage("")]
        [Remarks("")]
        public string v_IMAPMoveCopyUnreadOnly { get; set; }

        public MoveCopyIMAPEmailCommand()
        {
            CommandName = "MoveCopyIMAPEmailCommand";
            SelectionName = "Move/Copy IMAP Email";
            CommandEnabled = true;
            CustomRendering = true;
            v_IMAPOperationType = "Move MimeMessage";
            v_IMAPMoveCopyUnreadOnly = "Yes";
        }

        public override void RunCommand(object sender)
        {
            var engine = (AutomationEngineInstance)sender;
            MimeMessage vMailItem = (MimeMessage)VariableMethods.LookupVariable(engine, v_IMAPMailItem).VariableValue;
            string vIMAPHost = v_IMAPHost.ConvertToUserVariable(engine);
            string vIMAPPort = v_IMAPPort.ToString().ConvertToUserVariable(engine);
            string vIMAPUserName = v_IMAPUserName.ConvertToUserVariable(engine);
            string vIMAPPassword = v_IMAPPassword.ConvertToUserVariable(engine);
            var vIMAPDestinationFolder = v_IMAPDestinationFolder.ConvertToUserVariable(engine);

            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (sndr, certificate, chain, sslPolicyErrors) => true;
                client.SslProtocols = SslProtocols.None;

                using (var cancel = new CancellationTokenSource())
                {
                    client.Connect(vIMAPHost, int.Parse(vIMAPPort), true, cancel.Token);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(vIMAPUserName, vIMAPPassword, cancel.Token);

                    var splitId = vMailItem.MessageId.Split('#').ToList();
                    UniqueId messageId = UniqueId.Parse(splitId.Last());
                    splitId.RemoveAt(splitId.Count - 1);
                    string messageFolder = string.Join("", splitId);

                    IMailFolder toplevel = client.GetFolder(client.PersonalNamespaces[0]);
                    IMailFolder foundSourceFolder = GetIMAPEmailsCommand.FindFolder(toplevel, messageFolder);
                    IMailFolder foundDestinationFolder = GetIMAPEmailsCommand.FindFolder(toplevel, vIMAPDestinationFolder);

                    if (foundSourceFolder != null)
                        foundSourceFolder.Open(FolderAccess.ReadWrite, cancel.Token);
                    else
                        throw new Exception("Source Folder not found");

                    if (foundDestinationFolder == null)
                        throw new Exception("Destination Folder not found");

                    var messageSummary = foundSourceFolder.Fetch(new[] { messageId }, MessageSummaryItems.Flags);

                    if (v_IMAPOperationType == "Move MimeMessage")
                    {
                        if (v_IMAPMoveCopyUnreadOnly == "Yes")
                        {
                            if (!messageSummary[0].Flags.Value.HasFlag(MessageFlags.Seen))
                                foundSourceFolder.MoveTo(messageId, foundDestinationFolder, cancel.Token);
                        }
                        else
                            foundSourceFolder.MoveTo(messageId, foundDestinationFolder, cancel.Token);
                    }
                    else if (v_IMAPOperationType == "Copy MimeMessage")
                    {
                        //MailItem copyMail = null;
                        if (v_IMAPMoveCopyUnreadOnly == "Yes")
                        {
                            if (!messageSummary[0].Flags.Value.HasFlag(MessageFlags.Seen))
                                foundSourceFolder.CopyTo(messageId, foundDestinationFolder, cancel.Token);
                        }
                        else
                            foundSourceFolder.CopyTo(messageId, foundDestinationFolder, cancel.Token);
                    }

                    client.Disconnect(true, cancel.Token);
                    client.ServerCertificateValidationCallback = null;
                }
            } 
        }

        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPMailItem", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPHost", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPPort", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPUserName", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPPassword", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_IMAPDestinationFolder", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_IMAPOperationType", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_IMAPMoveCopyUnreadOnly", this, editor));

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + $" [{v_IMAPOperationType} '{v_IMAPMailItem}' to '{v_IMAPDestinationFolder}']";
        }
    }
}