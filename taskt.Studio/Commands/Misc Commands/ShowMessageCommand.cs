﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;
using taskt.Core.Attributes.ClassAttributes;
using taskt.Core.Attributes.PropertyAttributes;
using taskt.Core.Command;
using taskt.Core.Enums;
using taskt.Core.Infrastructure;
using taskt.Core.Utilities.CommonUtilities;
using taskt.UI.CustomControls;
using taskt.UI.Forms;

namespace taskt.Commands
{
    [Serializable]
    [Group("Misc Commands")]
    [Description("This command displays a message to the user.")]
    public class ShowMessageCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Message")]      
        [InputSpecification("Specify any text or variable value that should be displayed on screen.")]
        [SampleUsage("Hello World || {vMyText} || Hello {vName}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_Message { get; set; }

        [XmlAttribute]
        [PropertyDescription("Close After X (Seconds)")]
        [InputSpecification("Specify how many seconds to display the message on screen. After the specified time," + 
                            "\nthe message box will be automatically closed and script will resume execution.")]
        [SampleUsage("0 || 5 || {vSeconds})")]
        [Remarks("Set value to 0 to remain open indefinitely.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_AutoCloseAfter { get; set; }
        public ShowMessageCommand()
        {
            CommandName = "MessageBoxCommand";
            SelectionName = "Show Message";
            CommandEnabled = true;          
            CustomRendering = true;
            v_AutoCloseAfter = "0";
        }

        public override void RunCommand(object sender)
        {
            var engine = (Engine.AutomationEngineInstance)sender;
            string variableMessage = v_Message.ConvertUserVariableToString(engine);
            int closeAfter = int.Parse(v_AutoCloseAfter.ConvertUserVariableToString(engine));
            variableMessage = variableMessage.Replace("\\n", Environment.NewLine);

            if (engine.TasktEngineUI == null)
            {
                engine.ReportProgress("Complex Messagebox Supported With UI Only");
                MessageBox.Show(variableMessage, "Message Box Command", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //automatically close messageboxes for server requests
            if (engine.ServerExecution && closeAfter <= 0)
            {
                closeAfter = 10;
            }

            var result = ((frmScriptEngine)engine.TasktEngineUI).Invoke(new Action(() =>
                {
                    engine.TasktEngineUI.ShowMessage(variableMessage, "MessageBox Command", DialogType.OkOnly, closeAfter);
                }
            ));

        }
        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_Message", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_AutoCloseAfter", this, editor));

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + $" ['{v_Message}']";
        }
    }
}
