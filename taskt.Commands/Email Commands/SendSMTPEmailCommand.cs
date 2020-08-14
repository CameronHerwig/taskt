using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
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

    public class SendSMTPEmailCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Host")]
        [InputSpecification("Define the host/service name that the script should use.")]
        [SampleUsage("smtp.gmail.com || {vHost}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_SMTPHost { get; set; }

        [XmlAttribute]
        [PropertyDescription("Port")]
        [InputSpecification("Define the port number that should be used when contacting the SMTP service.")]
        [SampleUsage("587 || {vPort}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public int v_SMTPPort { get; set; }

        [XmlAttribute]
        [PropertyDescription("Username")]
        [InputSpecification("Define the username to use when contacting the SMTP service.")]
        [SampleUsage("myRobot || {vUsername}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_SMTPUserName { get; set; }

        [XmlAttribute]
        [PropertyDescription("Password")]
        [InputSpecification("Define the password to use when contacting the SMTP service.")]
        [SampleUsage("password || {vPassword}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_SMTPPassword { get; set; }

        [XmlAttribute]
        [PropertyDescription("Sender")]
        [InputSpecification("Enter the email address of the sender.")]
        [SampleUsage("myRobot@company.com || {vFromEmail}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_SMTPFromEmail { get; set; }

        [XmlAttribute]
        [PropertyDescription("Recipient(s)")]
        [InputSpecification("Enter the email address(es) of the recipient(s).")]
        [SampleUsage("test@test.com || test@test.com;test2@test.com || {vEmail} || {vEmail1};{vEmail2} || {vEmails}")]
        [Remarks("Multiple recipient email addresses should be delimited by a semicolon (;).")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_SMTPRecipients { get; set; }

        [XmlAttribute]
        [PropertyDescription("Email Subject")]
        [InputSpecification("Enter the subject of the email.")]
        [SampleUsage("Hello || {vSubject}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_SMTPSubject { get; set; }

        [XmlAttribute]
        [PropertyDescription("Email Body")]
        [InputSpecification("Enter text to be used as the email body.")]
        [SampleUsage("Everything ran ok at {DateTime.Now}  || {vBody}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_SMTPBody { get; set; }

        [XmlAttribute]
        [PropertyDescription("Attachment File Path(s)")]
        [InputSpecification("Enter the file path(s) of the file(s) to attach.")]
        [SampleUsage(@"C:\temp\myFile.xlsx || {vFile} || C:\temp\myFile1.xlsx;C:\temp\myFile2.xlsx || {vFile1};{vFile2} || {vFiles}")]
        [Remarks("This input is optional. Multiple attachments should be delimited by a semicolon (;).")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        [PropertyUIHelper(UIAdditionalHelperType.ShowFileSelectionHelper)]
        public string v_SMTPAttachments { get; set; }

        [XmlElement]
        [PropertyDescription("SSL Validation")]
        [PropertyUISelectionOption("Validate SSL")]
        [PropertyUISelectionOption("Bypass SSL Validation")]  
        [InputSpecification("Select the appropriate option.")]
        [SampleUsage("")]
        [Remarks("This field manages whether taskt will attempt to validate the SSL connection.")]
        public string v_SSLValidation { get; set; }

        public SendSMTPEmailCommand()
        {
            CommandName = "SendSMTPEmailCommand";
            SelectionName = "Send SMTP Email";
            CommandEnabled = true;
            CustomRendering = true;
            v_SSLValidation = "Validate SSL";
        }

        public override void RunCommand(object sender)
        {
            var engine = (AutomationEngineInstance)sender;
            //bypass ssl validation if requested
            if (v_SSLValidation.ConvertToUserVariable(engine) == "Bypass SSL Validation")
            {
                ServicePointManager.ServerCertificateValidationCallback = (sndr, certificate, chain, sslPolicyErrors) => true;
            }
                
            try
            {
                string vSMTPHost = v_SMTPHost.ConvertToUserVariable(engine);
                string vSMTPPort = v_SMTPPort.ToString().ConvertToUserVariable(engine);
                string vSMTPUserName = v_SMTPUserName.ConvertToUserVariable(engine);
                string vSMTPPassword = v_SMTPPassword.ConvertToUserVariable(engine);
                string vSMTPFromEmail = v_SMTPFromEmail.ConvertToUserVariable(engine);
                string vSMTPRecipients = v_SMTPRecipients.ConvertToUserVariable(engine);
                string vSMTPSubject = v_SMTPSubject.ConvertToUserVariable(engine);
                string vSMTPBody = v_SMTPBody.ConvertToUserVariable(engine);
                string vSMTPAttachments = v_SMTPAttachments.ConvertToUserVariable(engine);

                var client = new SmtpClient(vSMTPHost, int.Parse(vSMTPPort))
                {
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(vSMTPUserName, vSMTPPassword)                  
                };

                var splitRecipients = vSMTPRecipients.Split(';');
                foreach (var vSMTPToEmail in splitRecipients)
                {
                    var message = new MailMessage(vSMTPFromEmail, vSMTPToEmail, vSMTPSubject, vSMTPBody);

                    if (!string.IsNullOrEmpty(vSMTPAttachments))
                    {
                        var splitAttachments = vSMTPAttachments.Split(';');
                        foreach (var vSMTPattachment in splitAttachments)
                            message.Attachments.Add(new Attachment(vSMTPattachment));
                    }

                    client.Send(message);
                }              
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //restore default validation
                if (v_SSLValidation.ConvertToUserVariable(engine) == "Bypass SSL Validation")
                    ServicePointManager.ServerCertificateValidationCallback = null;
            }          
        }

        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            //create standard group controls
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SMTPHost", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SMTPPort", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SMTPUserName", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SMTPPassword", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SMTPFromEmail", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SMTPRecipients", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SMTPSubject", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SMTPBody", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_SMTPAttachments", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultDropdownGroupFor("v_SSLValidation", this, editor));

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + $" [To '{v_SMTPRecipients}' - Subject '{v_SMTPSubject}']";
        }
    }
}
