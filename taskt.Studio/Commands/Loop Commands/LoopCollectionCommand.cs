using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    [Group("Loop Commands")]
    [Description("This command iterates over a collection to let user perform actions on the collection items.")]
    public class LoopCollectionCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Input Collection")]
        [InputSpecification("Provide a collection variable.")]
        [SampleUsage("{vMyCollection}")]
        [Remarks("If the collection is a DataTable then the output item will be a DataRow and its column value can be accessed using the " +
            "dot operator like {vDataRow.ColumnName}.")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        public string v_LoopParameter { get; set; }

        [XmlAttribute]
        [PropertyDescription("Output Collection Item Variable")]
        [InputSpecification("Select or provide a variable from the variable list.")]
        [SampleUsage("{vUserVariable}")]
        [Remarks("If you have enabled the setting **Create Missing Variables at Runtime** then you are not required" +
                 " to pre-define your variables; however, it is highly recommended.")]
        public string v_OutputUserVariableName { get; set; }

        public LoopCollectionCommand()
        {
            CommandName = "LoopCollectionCommand";
            SelectionName = "Loop Collection";
            CommandEnabled = true;
            CustomRendering = true;
        }

        public override void RunCommand(object sender, ScriptAction parentCommand)
        {
            LoopCollectionCommand loopCommand = (LoopCollectionCommand)parentCommand.ScriptCommand;
            var engine = (AutomationEngineInstance)sender;

            int loopTimes;
            var complexVariable = v_LoopParameter.LookupComplexVariable(engine);           

            //if null then throw exception
            if (complexVariable == null)
            {
                throw new System.Exception("Complex Variable '" + v_LoopParameter + 
                    "' not found. Ensure the variable exists before attempting to modify it.");
            }

            dynamic listToLoop;
            if (complexVariable is List<string>)
            {
                listToLoop = (List<string>)complexVariable;
            }
            else if (complexVariable is List<IWebElement>)
            {
                listToLoop = (List<IWebElement>)complexVariable;
            }
            else if (complexVariable is DataTable)
            {
                listToLoop = ((DataTable)complexVariable).Rows;
            }
            else if (complexVariable is List<MailItem>)
            {
                listToLoop = (List<MailItem>)complexVariable;
            }
            else if ((complexVariable.ToString().StartsWith("[")) && 
                (complexVariable.ToString().EndsWith("]")) && 
                (complexVariable.ToString().Contains(",")))
            {
                //automatically handle if user has given a json array
                JArray jsonArray = JsonConvert.DeserializeObject(complexVariable.ToString()) as JArray;

               var itemList = new List<string>();
                foreach (var item in jsonArray)
                {
                    var value = (JValue)item;
                    itemList.Add(value.ToString());
                }

                itemList.StoreInUserVariable(engine, v_LoopParameter);
                listToLoop = itemList;
            }
            else
            {
                throw new System.Exception("Complex Variable List Type<T> Not Supported");
            }

            loopTimes = listToLoop.Count;

            for (int i = 0; i < loopTimes; i++)
            {
                engine.ReportProgress("Starting Loop Number " + (i + 1) + "/" + loopTimes + " From Line " + loopCommand.LineNumber);
                
                if(listToLoop[i] is string)
                {
                    ((string)listToLoop[i]).StoreInUserVariable(engine, v_OutputUserVariableName);
                }
                else
                {
                    listToLoop[i].StoreInUserVariable(engine, v_OutputUserVariableName);
                }
                foreach (var cmd in parentCommand.AdditionalScriptCommands)
                {
                    if (engine.IsCancellationPending)
                        return;

                    engine.ExecuteCommand(cmd);

                    if (engine.CurrentLoopCancelled)
                    {
                        engine.ReportProgress("Exiting Loop From Line " + loopCommand.LineNumber);
                        engine.CurrentLoopCancelled = false;
                        return;
                    }

                    if (engine.CurrentLoopContinuing)
                    {
                        engine.ReportProgress("Continuing Next Loop From Line " + loopCommand.LineNumber);
                        engine.CurrentLoopContinuing = false;
                        break;
                    }
                }

                engine.ReportProgress("Finished Loop From Line " + loopCommand.LineNumber);
            }
        }

        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_LoopParameter", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultOutputGroupFor("v_OutputUserVariableName", this, editor));
            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return $"Loop Collection '{v_LoopParameter}' - Store Collection Item in '{v_OutputUserVariableName}'";
        }
    }
}