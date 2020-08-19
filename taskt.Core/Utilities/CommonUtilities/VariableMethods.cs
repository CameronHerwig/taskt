﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using taskt.Core.Infrastructure;
using taskt.Core.Script;

namespace taskt.Core.Utilities.CommonUtilities
{
    public static class VariableMethods
    {
        /// <summary>
        /// Replaces variable placeholders ({variable}) with variable text.
        /// </summary>
        /// <param name="sender">The script engine instance (frmScriptEngine) which contains session variables.</param>
        public static string ConvertToUserVariable(this string str, IEngine engine)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            if (engine == null)
                return str;

            if (str.Length < 2)
                return str;

            var variableList = engine.VariableList;
            var systemVariables = Common.Common.GenerateSystemVariables();

            var variableSearchList = new List<ScriptVariable>();
            variableSearchList.AddRange(variableList);
            variableSearchList.AddRange(systemVariables);

            var elementSearchList = engine.ElementList;

            //check if it's an element first
            if (str.StartsWith("<") && str.EndsWith(">"))
            {
                string potentialElement = str.TrimStart('<').TrimEnd('>');
                var matchingElement = elementSearchList.Where(elem => elem.ElementName == potentialElement).FirstOrDefault();
                if (matchingElement != null)
                {
                    //if element found, store it in str and continue checking for variables
                    str = matchingElement.ElementValue;
                }
            }

            //variable markers
            var startVariableMarker = "{";
            var endVariableMarker = "}";

            if (!str.Contains(startVariableMarker) || !str.Contains(endVariableMarker))
            {
                return str.CalculateVariables(engine);
            }
                
            //split by custom markers
            string[] potentialVariables = str.Split(new string[] { startVariableMarker, endVariableMarker }, StringSplitOptions.None);

            foreach (var potentialVariable in potentialVariables)
            {
                if (potentialVariable.Length == 0) 
                    continue;

                string varcheckname = potentialVariable;
                bool isSystemVar = systemVariables.Any(vars => vars.VariableName == varcheckname);

                if (potentialVariable.Split('.').Length == 2 && !isSystemVar)
                {
                    varcheckname = potentialVariable.Split('.')[0];
                }

                var varCheck = variableSearchList.Where(v => v.VariableName == varcheckname).FirstOrDefault();
                    
                if (potentialVariable == "taskt.EngineContext") 
                    varCheck.VariableValue = engine.GetEngineContext();

                if (varCheck != null)
                {
                    var searchVariable = startVariableMarker + potentialVariable + endVariableMarker;

                    if (str.Contains(searchVariable))
                    {
                        str = str.Replace(searchVariable, (string)varCheck.VariableValue);
                    }
                }                           
            }
            return str.CalculateVariables(engine);
        }

        public static object LookupComplexVariable(this string variableName, IEngine engine)
        {
            ScriptVariable requiredVariable;

            if (variableName.StartsWith("{") && variableName.EndsWith("}"))
            {
                //reformat and attempt
                var reformattedVariable = variableName.Replace("{", "").Replace("}", "");
                requiredVariable = engine.VariableList.Where(var => var.VariableName == reformattedVariable).FirstOrDefault();
            }
            else
                throw new Exception("Variable markers '{}' missing. '" + variableName + "' could not be found.");

            if (requiredVariable != null)
                return requiredVariable.VariableValue;
            else
                return null;
        }

        private static string CalculateVariables(this string str, IEngine engine)
        {
            if (!engine.AutoCalculateVariables)
                return str;
            else
            {
                //track math chars
                var mathChars = new List<char>();
                mathChars.Add('*');
                mathChars.Add('+');
                mathChars.Add('-');
                mathChars.Add('=');
                mathChars.Add('/');

                //if the string matches the char then return
                //as the user does not want to do math
                if (mathChars.Any(f => f.ToString() == str) || (mathChars.Any(f => str.StartsWith(f.ToString()))))
                    return str;

                //bypass math for types that are dates
                DateTime dateTest;
                if (DateTime.TryParse(str, out dateTest) && (str.Split('/').Length == 3 || str.Split('-').Length == 3))
                    return str;

                //test if math is required
                if (mathChars.Any(f => str.Contains(f)))
                {
                    try
                    {
                        DataTable dt = new DataTable();
                        var v = dt.Compute(str, "");
                        return v.ToString();
                    }
                    catch (Exception)
                    {
                        return str;
                    }
                }
                else
                    return str;
            }
        }

        /// <summary>
        /// Stores value of the object to a user-defined variable.
        /// </summary>
        /// <param name="sender">The script engine instance (frmScriptEngine) which contains session variables.</param>
        /// <param name="targetVariable">the name of the user-defined variable to override with new value</param>
        public static void StoreInUserVariable(this object variableValue, IEngine engine, string variableName)
        {
            if (variableName.StartsWith("{") && variableName.EndsWith("}"))
                variableName = variableName.Replace("{", "").Replace("}", "");           
            else
                throw new Exception("Variable markers '{}' missing. Output variable is invalid.");

            if (engine.VariableList.Any(f => f.VariableName == variableName))
            {
                //update existing variable
                var existingVariable = engine.VariableList.FirstOrDefault(f => f.VariableName == variableName);
                existingVariable.VariableName = variableName;
                existingVariable.VariableValue = variableValue;
            }
            else
            {
                //add new variable
                var newVariable = new ScriptVariable();
                newVariable.VariableName = variableName;
                newVariable.VariableValue = variableValue;
                engine.VariableList.Add(newVariable);
            }
        }       

        /// <summary>
        /// Converts a string to SecureString
        /// </summary>
        /// <param name="value">The string to be converted to SecureString</param>
        public static SecureString GetSecureString(this string value)
        {
            SecureString secureString = new System.Net.NetworkCredential(string.Empty, value).SecurePassword;
            return secureString;
        }

        public static string ConvertSecureStringToString(this SecureString secureString)
        {
            string strValue = new System.Net.NetworkCredential(string.Empty, secureString).Password;
            return strValue;
        }


    }
}
