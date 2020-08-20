using System;
using System.Collections.Generic;
using System.Windows.Forms;
using taskt.Core.Attributes.ClassAttributes;
using taskt.Core.Command;
using taskt.Core.Infrastructure;

namespace taskt.Commands
{
    [Serializable]
    [Group("Misc Commands")]
    [Description("This command allows you to add an in-line comment to the script.")]
    [UsesDescription("Use this command when you want to add code comments or document code.  Usage of variables (ex. [vVar]) within the comment block will be parsed and displayed when running the script.")]
    [ImplementationDescription("This command is for visual purposes only")]
    public class AddCodeCommentCommand : ScriptCommand
    {
        public AddCodeCommentCommand()
        {
            CommandName = "AddCodeCommentCommand";
            SelectionName = "Add Code Comment";
            DisplayForeColor = System.Drawing.Color.ForestGreen;
            CommandEnabled = true;
            CustomRendering = true;
        }
        public override List<Control> Render(IfrmCommandEditor editor)
        {
            base.Render(editor);

            return RenderedControls;
        }

        public override string GetDisplayValue()
        {
            return "// Comment: " + v_Comment;
        }
    }
}