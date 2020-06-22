﻿using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;
using taskt.Core.Automation.Attributes.ClassAttributes;
using taskt.Core.Automation.Attributes.PropertyAttributes;
using taskt.Core.Automation.Engine;
using taskt.Core.Utilities.CommonUtilities;
using taskt.UI.CustomControls;
using taskt.UI.Forms;
using Application = Microsoft.Office.Interop.Outlook.Application;

namespace taskt.Core.Automation.Commands
{
    [Serializable]
    [Group("Outlook Commands")]
    [Description("This command replies to selected emails in Outlook.")]

    public class ReplyToOutlookEmailsCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Source Mail Folder Name")]
        [InputSpecification("Enter the name of the Outlook mail folder the emails are located in.")]
        [SampleUsage("Inbox || {vFolderName}")]
        [Remarks("")]
        [PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        public string v_SourceFolder { get; set; }

        [XmlAttribute]
        [PropertyDescription("Filter")]
        [InputSpecification("Enter a valid Outlook filter string.")]
        [SampleUsage("[Subject] = 'Hello' and [SenderName] = 'Jane Doe' || {vFilter}")]
        [Remarks("This input is optional.")]
        [PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        public string v_Filter { get; set; }

        [XmlAttribute]
        [PropertyDescription("Mail Operation")]
        [PropertyUISelectionOption("Reply")]
        [PropertyUISelectionOption("Reply All")]
        [InputSpecification("Specify whether you intend to reply or reply all.")]
        [SampleUsage("")]
        [Remarks("Replying will reply to only the original sender. Reply all will reply to everyone in the recipient list.")]
        public string v_OperationType { get; set; }

        [XmlAttribute]
        [PropertyDescription("Email Body")]
        [InputSpecification("Enter text to be used as the email body.")]
        [SampleUsage("Dear John, ... || {vBody}")]
        [Remarks("")]
        [PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        public string v_Body { get; set; }

        [XmlAttribute]
        [PropertyDescription("Email Body Type")]
        [PropertyUISelectionOption("Plain")]
        [PropertyUISelectionOption("HTML")]
        [InputSpecification("Select the email body format.")]
        [Remarks("")]
        public string v_BodyType { get; set; }

        [XmlAttribute]
        [PropertyDescription("Attachment File Path(s)")]
        [InputSpecification("Enter the file path(s) of the file(s) to attach.")]
        [SampleUsage(@"C:\temp\myFile.xlsx || {vFile} || C:\temp\myFile1.xlsx;C:\temp\myFile2.xlsx || {vFile1};{vFile2} || {vFiles}")]
        [Remarks("This input is optional. Multiple attachments should be delimited by a semicolon (;).")]
        [PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        [PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowFileSelectionHelper)]
        public string v_Attachments { get; set; }

        public ReplyToOutlookEmailsCommand()
        {
            CommandName = "ReplyToOutlookEmailsCommand";
            SelectionName = "Reply To Outlook Emails";
            CommandEnabled = true;
            CustomRendering = true;
            v_OperationType = "Reply";
            v_BodyType = "Plain";
        }

        public override void RunCommand(object sender)
        {
            var engine = (AutomationEngineInstance)sender;
            var vSourceFolder = v_SourceFolder.ConvertToUserVariable(sender);
            var vFilter = v_Filter.ConvertToUserVariable(sender);
            var vBody = v_Body.ConvertToUserVariable(sender);
            var vAttachment = v_Attachments.ConvertToUserVariable(sender);

            Application outlookApp = new Application();
            AddressEntry currentUser = outlookApp.Session.CurrentUser.AddressEntry;
            NameSpace test = outlookApp.GetNamespace("MAPI");

            if (currentUser.Type == "EX")
            {
                MAPIFolder inboxFolder = test.GetDefaultFolder(OlDefaultFolders.olFolderInbox).Parent;
                MAPIFolder sourceFolder = inboxFolder.Folders[vSourceFolder];
                Items filteredItems = null;

                if (vFilter != "")
                    filteredItems = sourceFolder.Items.Restrict(vFilter);
                else
                    filteredItems = sourceFolder.Items;

                foreach (object _obj in filteredItems)
                {
                    if (_obj is MailItem)
                    {
                        MailItem tempMail = (MailItem)_obj;
                        if (v_OperationType == "Reply")
                        {
                            MailItem newMail = tempMail.Reply();
                            Reply(newMail, vBody, vAttachment);
                        }
                        else if(v_OperationType == "Reply All")
                        {
                            MailItem newMail = tempMail.ReplyAll();
                            Reply(newMail, vBody, vAttachment);
                        }
                    }
                }
            }
        }

        public override List<Control> Render(frmCommandEditor editor)
        {
            base.Render(editor);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SourceFolder", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_Filter", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_OperationType", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_Body", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_BodyType", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_Attachments", this, editor));

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + $" [From '{v_SourceFolder}' - Filter by '{v_Filter}' - {v_OperationType}]";
        }

        private void Reply(MailItem mail, string body, string attPath)
        {
            if (v_BodyType == "HTML")
                mail.HTMLBody = body;
            else 
                mail.Body = body;

            if (!string.IsNullOrEmpty(attPath))
            {
                var splitAttachments = attPath.Split(';');

                foreach (var attachment in splitAttachments)
                    mail.Attachments.Add(attachment);
            }
            mail.Send();
        }
    }
}
