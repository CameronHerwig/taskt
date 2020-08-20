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
using taskt.Engine;
using taskt.UI.CustomControls;

namespace taskt.Commands
{
    [Serializable]
    [Group("Engine Commands")]
    [Description("This command allows you to stop a program or a process.")]
    [UsesDescription("Use this command to close an application by its name such as 'chrome'. Alternatively, you may use the Close Window or Thick App Command instead.")]
    [ImplementationDescription("This command implements 'Process.CloseMainWindow'.")]
    public class StopwatchCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Enter the instance name of the Stopwatch")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        [InputSpecification("Provide a unique instance or way to refer to the stopwatch")]
        [SampleUsage("**myStopwatch**, **{vStopWatch}**")]
        [Remarks("")]
        public string v_StopwatchName { get; set; }

        [XmlAttribute]
        [PropertyDescription("Enter the Stopwatch Action")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        [InputSpecification("Provide a unique instance or way to refer to the stopwatch")]
        [SampleUsage("**myStopwatch**, **{vStopWatch}**")]
        [Remarks("")]
        public string v_StopwatchAction { get; set; }

        [XmlAttribute]
        [PropertyDescription("Optional - Specify String Format")]
        [InputSpecification("Specify if a specific string format is required.")]
        [SampleUsage("MM/dd/yy, hh:mm, etc.")]
        [Remarks("")]
        public string v_ToStringFormat { get; set; }

        [XmlAttribute]
        [PropertyDescription("Output Result Variable")]
        [InputSpecification("Create a new variable or select a variable from the list.")]
        [SampleUsage("{vUserVariable}")]
        [Remarks("Variables not pre-defined in the Variable Manager will be automatically generated at runtime.")]
        public string v_OutputUserVariableName { get; set; }

        [XmlIgnore]
        [NonSerialized]
        public ComboBox StopWatchComboBox;

        [XmlIgnore]
        [NonSerialized]
        public List<Control> MeasureControls;

        public StopwatchCommand()
        {
            CommandName = "StopwatchCommand";
            SelectionName = "Stopwatch";
            CommandEnabled = true;
            CustomRendering = true;
            v_StopwatchName = "default";
            v_StopwatchAction = "Start Stopwatch";
        }

        public override void RunCommand(object sender)
        {
            var engine = (AutomationEngineInstance)sender;   
            var instanceName = v_StopwatchName.ConvertToUserVariable(engine);
            System.Diagnostics.Stopwatch stopwatch;

            var action = v_StopwatchAction.ConvertToUserVariable(engine);

            switch (action)
            {
                case "Start Stopwatch":
                    //start a new stopwatch
                    stopwatch = new System.Diagnostics.Stopwatch();
                    engine.AddAppInstance(instanceName, stopwatch);
                    stopwatch.Start();
                    break;
                case "Stop Stopwatch":
                    //stop existing stopwatch
                    stopwatch = (System.Diagnostics.Stopwatch)engine.AppInstances[instanceName];
                    stopwatch.Stop();
                    break;
                case "Restart Stopwatch":
                    //restart which sets to 0 and automatically starts
                    stopwatch = (System.Diagnostics.Stopwatch)engine.AppInstances[instanceName];
                    stopwatch.Restart();
                    break;
                case "Reset Stopwatch":
                    //reset which sets to 0
                    stopwatch = (System.Diagnostics.Stopwatch)engine.AppInstances[instanceName];
                    stopwatch.Reset();
                    break;
                case "Measure Stopwatch":
                    //check elapsed which gives measure
                    stopwatch = (System.Diagnostics.Stopwatch)engine.AppInstances[instanceName];
                    string elapsedTime;
                    if (string.IsNullOrEmpty(v_ToStringFormat))
                    {
                        elapsedTime = stopwatch.Elapsed.ToString();
                    }
                    else
                    {
                        var format = v_ToStringFormat.ConvertToUserVariable(engine);
                        elapsedTime = stopwatch.Elapsed.ToString(format);
                    }

                    elapsedTime.StoreInUserVariable(engine, v_OutputUserVariableName);

                    break;
                default:
                    throw new NotImplementedException("Stopwatch Action '" + action + "' not implemented");
            }



        }
        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_StopwatchName", this, editor));

            var StopWatchComboBoxLabel = CommandControls.CreateDefaultLabelFor("v_StopwatchAction", this);
            StopWatchComboBox = (ComboBox)CommandControls.CreateDropdownFor("v_StopwatchAction", this);
            StopWatchComboBox.DataSource = new List<string> { "Start Stopwatch", "Stop Stopwatch", "Restart Stopwatch", "Reset Stopwatch", "Measure Stopwatch" };
            StopWatchComboBox.SelectedIndexChanged += StopWatchComboBox_SelectedValueChanged;
            RenderedControls.Add(StopWatchComboBoxLabel);
            RenderedControls.Add(StopWatchComboBox);

            MeasureControls.AddRange(CommandControls.CreateDefaultOutputGroupFor("v_OutputUserVariableName", this, editor));
            MeasureControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_ToStringFormat", this, editor));

            foreach (var ctrl in MeasureControls)
            {
                ctrl.Visible = false;
            }
            RenderedControls.AddRange(MeasureControls);

            return RenderedControls;
        }

        private void StopWatchComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
           
            if (StopWatchComboBox.SelectedValue.ToString() == "Measure Stopwatch")
            {
                foreach (var ctrl in MeasureControls)
                                 ctrl.Visible = true;
               
            }
            else {
                foreach (var ctrl in MeasureControls)
                    ctrl.Visible = false;
            }
            
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + " [Action: " + v_StopwatchAction + ", Name: " + v_StopwatchName + "]";
        }
    }
}