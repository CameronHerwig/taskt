﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;
using taskt.Core.Attributes.ClassAttributes;
using taskt.Core.Attributes.PropertyAttributes;
using taskt.Core.Command;
using taskt.Core.Enums;
using taskt.Core.Infrastructure;
using taskt.Core.Script;
using taskt.Core.Utilities.CommonUtilities;
using taskt.Engine;
using taskt.UI.CustomControls;

namespace taskt.Commands
{
    [Serializable]
    [Group("Variable Commands")]
    [Description("This command sets the current index of a variable.")]
    public class SetVariableIndexCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Variable Name")]
        [InputSpecification("Select or provide a variable from the variable list.")]
        [SampleUsage("vSomeVariable || {vSomeVariable}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_VariableName { get; set; }

        [XmlAttribute]
        [PropertyDescription("Variable Index")]
        [InputSpecification("Enter the index of the variable.")]
        [SampleUsage("1 || 2 || {vIndex}")]
        [Remarks("You can use variables in input if you encase them within braces {vIndex}. You can also perform basic math operations.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_Index { get; set; }

        public SetVariableIndexCommand()
        {
            CommandName = "SetVariableIndexCommand";
            SelectionName = "Set Variable Index";
            CommandEnabled = true;
            CustomRendering = true;
        }

        public override void RunCommand(object sender)
        {
            //get sending instance
            var engine = (AutomationEngineInstance)sender;
            var requiredVariable = v_VariableName.LookupVariable(engine);

            if (requiredVariable != null)
            {
                var index = int.Parse(v_Index.ConvertToUserVariable(engine));
                requiredVariable.CurrentPosition = index;
            }
            else
            {
                throw new Exception("Attempted to update variable index, but variable was not found. Enclose variables within braces, ex. {vVariable}");
            }
        }

        public override List<Control> Render(IfrmCommandEditor editor)
        {
            //custom rendering
            base.Render(editor);

            //create control for variable name
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_VariableName", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_Index", this, editor));

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + $" [Update Variable '{v_VariableName}' Index to '{v_Index}']";
        }
    }
}