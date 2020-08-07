using OneNoteOCRDll;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [Group("Image Commands")]
    [Description("This command converts an image file into text for parsing.")]
    public class PerformOCRCommand : ScriptCommand
    {
        [XmlAttribute]
        [PropertyDescription("Image File Path")]
        [InputSpecification("Select the image to perform OCR text extraction on.")]
        [SampleUsage(@"C:\temp\myimages.png || {ProjectPath}\myimages.png || {vImageFile}")]
        [Remarks("")]
        [PropertyUIHelper(UIAdditionalHelperType.ShowVariableHelper)]
        [PropertyUIHelper(UIAdditionalHelperType.ShowFileSelectionHelper)]
        public string v_FilePath { get; set; }

        [XmlAttribute]
        [PropertyDescription("Output OCR Result Variable")]
        [InputSpecification("Select or provide a variable from the variable list.")]
        [SampleUsage("vUserVariable")]
        [Remarks("If you have enabled the setting **Create Missing Variables at Runtime** then you are not required" +
                 " to pre-define your variables; however, it is highly recommended.")]
        public string v_OutputUserVariableName { get; set; }

        public PerformOCRCommand()
        {
            DefaultPause = 0;
            CommandName = "PerformOCRCommand";
            SelectionName = "Perform OCR";
            CommandEnabled = true;
            CustomRendering = true;
        }

        public override void RunCommand(object sender)
        {
            var engine = (AutomationEngineInstance)sender;
            var vFilePath = v_FilePath.ConvertToUserVariable(engine);

            OneNoteOCR ocrEngine = new OneNoteOCR();
            OCRText[] ocrTextArray = ocrEngine.OcrTexts(vFilePath).ToArray();

            string endResult = "";
            foreach (var text in ocrTextArray)
                endResult += text.Text;

            endResult.StoreInUserVariable(engine, v_OutputUserVariableName);
        }

        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            RenderedControls.AddRange(CommandControls.CreateDefaultInputGroupFor("v_FilePath", this, editor));
            RenderedControls.AddRange(CommandControls.CreateDefaultOutputGroupFor("v_OutputUserVariableName", this, editor));

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return base.GetDisplayValue() + $" [File '{v_FilePath}' - Store OCR Result in '{v_OutputUserVariableName}']";
        }
    }
}