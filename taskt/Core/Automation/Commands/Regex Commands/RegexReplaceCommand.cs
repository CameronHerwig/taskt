﻿using System;
using System.Linq;
using System.Xml.Serialization;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using taskt.UI.Forms;
using taskt.UI.CustomControls;
using System.Text.RegularExpressions;
using taskt.Core.Utilities.CommonUtilities;

namespace taskt.Core.Automation.Commands
{
    [Serializable]
    [Attributes.ClassAttributes.Group("Regex Commands")]
    [Attributes.ClassAttributes.Description("This command allows you to replace all the matches in Text based on RegEx")]
    [Attributes.ClassAttributes.UsesDescription("Use this command when you want to replace all matches in text based on Regex Pattern")]
    [Attributes.ClassAttributes.ImplementationDescription("This command implements Replace Action of Regex for given input Text and Regex Pattern and returns a text after replacement")]
    public class RegexReplaceCommand : ScriptCommand
    {
        [XmlAttribute]
        [Attributes.PropertyAttributes.PropertyDescription("Please input the data you want to perform regex on")]
        [Attributes.PropertyAttributes.PropertyUIHelper(Attributes.PropertyAttributes.PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        [Attributes.PropertyAttributes.InputSpecification("Enter Variable or Text to apply Regex on")]
        [Attributes.PropertyAttributes.SampleUsage("**Hello** or **vSomeVariable**")]
        [Attributes.PropertyAttributes.Remarks("")]
        public string v_InputTextData { get; set; }

        [XmlAttribute]
        [Attributes.PropertyAttributes.PropertyDescription("Please enter regex pattern")]
        [Attributes.PropertyAttributes.PropertyUIHelper(Attributes.PropertyAttributes.PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        [Attributes.PropertyAttributes.InputSpecification("Enter a Regex Pattern to apply to replace matches with given text")]
        [Attributes.PropertyAttributes.SampleUsage(@"**^([\w\-]+)** or **vSomeVariable**")]
        [Attributes.PropertyAttributes.Remarks("")]
        public string v_RegEx { get; set; }

        [XmlAttribute]
        [Attributes.PropertyAttributes.PropertyDescription("Please input the data (text) to replace all the matches")]
        [Attributes.PropertyAttributes.PropertyUIHelper(Attributes.PropertyAttributes.PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        [Attributes.PropertyAttributes.InputSpecification("Enter Variable or Text to replace the matches")]
        [Attributes.PropertyAttributes.SampleUsage("**Hello** or **vSomeVariable**")]
        [Attributes.PropertyAttributes.Remarks("")]
        public string v_ReplaceTextData { get; set; }

        [XmlAttribute]
        [Attributes.PropertyAttributes.PropertyDescription("Please select variable to get regex result")]
        [Attributes.PropertyAttributes.InputSpecification("Select or provide a variable from the variable list")]
        [Attributes.PropertyAttributes.SampleUsage("**vSomeVariable**")]
        [Attributes.PropertyAttributes.Remarks("")]
        public string v_OutputVariableName { get; set; }

        public RegexReplaceCommand()
        {
            this.CommandName = "RegexReplaceCommand";
            this.SelectionName = "Regex Replace";
            this.CommandEnabled = true;
            this.CustomRendering = true;
        }

        public override void RunCommand(object sender)
        {
            var engine = (Core.Automation.Engine.AutomationEngineInstance)sender;
            var vInputData = v_InputTextData.ConvertToUserVariable(engine);
            string vRegex = v_RegEx.ConvertToUserVariable(engine);
            string vReplaceData = v_ReplaceTextData.ConvertToUserVariable(engine);
            string resultData = Regex.Replace(vInputData, vRegex, vReplaceData);

            resultData.StoreInUserVariable(sender, v_OutputVariableName);
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + " [Replace All Matches with '" + v_ReplaceTextData + "', Get Result in: '" + v_OutputVariableName + "']";
        }

        public override List<Control> Render(frmCommandEditor editor)
        {
            base.Render(editor);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_RegEx", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_InputTextData", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_ReplaceTextData", this, editor));
            RenderedControls.Add(CommandControls.CreateDefaultLabelFor("v_OutputVariableName", this));
            var VariableNameControl = CommandControls.CreateStandardComboboxFor("v_OutputVariableName", this).AddVariableNames(editor);
            RenderedControls.AddRange(CommandControls.CreateUIHelpersFor("v_OutputVariableName", this, new Control[] { VariableNameControl }, editor));
            RenderedControls.Add(VariableNameControl);

            return RenderedControls;
        }
    }
}
