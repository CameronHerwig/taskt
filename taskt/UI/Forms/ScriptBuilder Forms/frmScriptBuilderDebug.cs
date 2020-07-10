using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using taskt.Core.Script;
using taskt.Properties;

namespace taskt.UI.Forms.ScriptBuilder_Forms
{
    public partial class frmScriptBuilder : Form
    {
        private void CreateDebugTab()
        {
            TabPage debugTab = new TabPage();
            debugTab.Name = "DebugVariables";
            debugTab.Text = "Variables";
            uiPaneTabs.TabPages.Add(debugTab);
            uiPaneTabs.SelectedTab = debugTab;
            LoadDebugTab(debugTab);
        }

        //TODO: Studio Step Into
        public delegate void LoadDebugTabDelegate(TabPage debugTab);
        private void LoadDebugTab(TabPage debugTab)
        {
            if (InvokeRequired)
            {
                var d = new LoadDebugTabDelegate(LoadDebugTab);
                Invoke(d, new object[] { debugTab });
            }
            else
            {
                DataTable variableValues = new DataTable();
                variableValues.Columns.Add("Name");
                variableValues.Columns.Add("Value");
                variableValues.TableName = "VariableValuesDataTable" + DateTime.Now.ToString("MMddyyhhmmss");

                DataGridView variablesGridViewHelper = new DataGridView();
                variablesGridViewHelper.Dock = DockStyle.Fill;
                variablesGridViewHelper.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                variablesGridViewHelper.AllowUserToAddRows = false;
                variablesGridViewHelper.AllowUserToDeleteRows = false;

                if (debugTab.Controls.Count == 0)
                    debugTab.Controls.Add(variablesGridViewHelper);
                else
                {
                    debugTab.Controls.RemoveAt(0);
                    debugTab.Controls.Add(variablesGridViewHelper);
                }


                List<ScriptVariable> engineVariables = _newEngine.EngineInstance.VariableList;
                foreach (var variable in engineVariables)
                {
                    DataRow[] foundVariables = variableValues.Select("Name = '" + variable.VariableName + "'");
                    if (foundVariables.Length == 0)
                    {
                        if (variable.VariableValue is string)
                            variableValues.Rows.Add(variable.VariableName, variable.VariableValue);
                        else
                            variableValues.Rows.Add(variable.VariableName, $"[{variable.VariableValue.GetType()}]");
                    }
                }
                variablesGridViewHelper.DataSource = variableValues;
            }           
        }

        #region Debug Buttons
        private void stepOverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _newEngine.uiBtnStepOver_Click(sender, e);
            IsScriptSteppedOver = true;
        }

        private void stepIntoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _newEngine.uiBtnStepInto_Click(sender, e);
            IsScriptSteppedInto = true;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _newEngine.uiBtnPause_Click(sender, e);
            if (pauseToolStripMenuItem.Tag.ToString() == "pause")
            {
                pauseToolStripMenuItem.Image = Resources.command_resume;
                pauseToolStripMenuItem.Tag = "resume";
            }

            else if (pauseToolStripMenuItem.Tag.ToString() == "resume")
            {
                stepIntoToolStripMenuItem.Visible = false;
                stepOverToolStripMenuItem.Visible = false;
                pauseToolStripMenuItem.Visible = true;
                cancelToolStripMenuItem.Visible = true;
                pauseToolStripMenuItem.Image = Resources.command_pause;
                pauseToolStripMenuItem.Tag = "pause";

                //When resuming, close debug tab if it's open
                if (uiPaneTabs.TabPages.Count == 3)
                    uiPaneTabs.TabPages.RemoveAt(2);

                IsScriptSteppedOver = false;
                IsScriptSteppedInto = false;
            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _newEngine.uiBtnCancel_Click(sender, e);

            stepIntoToolStripMenuItem.Visible = false;
            stepOverToolStripMenuItem.Visible = false;
            pauseToolStripMenuItem.Visible = false;
            cancelToolStripMenuItem.Visible = false;
        }
        #endregion
    }
}
