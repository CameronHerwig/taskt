using System;
using System.Windows.Forms;

namespace taskt.UI.Forms.Supplement_Forms
{
    public partial class frmAddVariable : ThemedForm
    {
        public frmAddVariable()
        {
            InitializeComponent();
        }

        public frmAddVariable(string VariableName, string variableValue, string variableType)
        {
            InitializeComponent();
            this.Text = "edit variable";
            lblHeader.Text = "edit variable";
            txtVariableName.Text = VariableName;
            txtDefaultValue.Text = variableValue;
            txtVariableType.Text = variableType;
        }

        private void frmAddVariable_Load(object sender, EventArgs e)
        {

        }

        private void uiBtnOk_Click(object sender, EventArgs e)
        {
            if (txtVariableName.Text.Trim() == string.Empty)
            {
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        private void uiBtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void txtVariableType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
