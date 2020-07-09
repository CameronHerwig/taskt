using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using taskt.Core.Script;
using taskt.UI.CustomControls;
using System.Data;
using System.Diagnostics;

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

        private void LoadDebugTab(TabPage debugTab)
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
}
