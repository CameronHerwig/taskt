﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
using Group = taskt.Core.Attributes.ClassAttributes.Group;

namespace taskt.Commands
{
    [Serializable]
    [Group("Regex Commands")]
    [Description("This command allows you to Split Text based on RegEx Match")]
    [UsesDescription("Use this command when you want to Split Text based on Regex Pattern")]
    [ImplementationDescription("This command implements Split Action of Regex for given input Text and Regex Pattern and returns an array of strings")]
    public class RegexSplitCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Please input the data you want to perform regex on")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        [InputSpecification("Enter Variable or Text to apply Regex on")]
        [SampleUsage("**Hello** or **vSomeVariable**")]
        [Remarks("")]
        public string v_InputTextData { get; set; }

        [XmlAttribute]
        [PropertyDescription("Please enter regex pattern")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        [InputSpecification("Enter a Regex Pattern to apply to split text based on RegEx")]
        [SampleUsage(@"**^([\w\-]+)** or **vSomeVariable**")]
        [Remarks("")]
        public string v_RegEx { get; set; }

        [XmlAttribute]
        [PropertyDescription("Output Result Variable")]
        [InputSpecification("Create a new variable or select a variable from the list.")]
        [SampleUsage("{vUserVariable}")]
        [Remarks("Variables not pre-defined in the Variable Manager will be automatically generated at runtime.")]
        public string v_OutputUserVariableName { get; set; }

        public RegexSplitCommand()
        {
            CommandName = "RegexSplitCommand";
            SelectionName = "Regex Split";
            CommandEnabled = true;
            CustomRendering = true;
        }

        public override void RunCommand(object sender)
        {
            var engine = (AutomationEngineInstance)sender;
            var vInputData = v_InputTextData.ConvertUserVariableToString(engine);
            string vRegex = v_RegEx.ConvertUserVariableToString(engine);
            var vResultData = Regex.Split(vInputData, vRegex).ToList();

            vResultData.StoreInUserVariable(engine, v_OutputUserVariableName);
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + " [Apply Regex to '" + v_InputTextData + "', Get Result in: '" + v_OutputUserVariableName + "']";
        }

        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_RegEx", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_InputTextData", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultOutputGroupFor("v_OutputUserVariableName", this, editor));

            return RenderedControls;
        }
    }
}
