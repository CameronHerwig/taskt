using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;
using System;
using taskt.Core.Enums;
using taskt.Core.IO;

namespace taskt.Core.Utilities.CommonUtilities
{
    /// <summary>
    /// Handles functionality for logging to files
    /// </summary>
    public class Logging
    {
        public Logger CreateFileLogger(string filePath, RollingInterval logInterval)
        {
            if (string.IsNullOrEmpty(filePath))
                filePath = Folders.GetFolder(FolderType.LogFolder) + "\\taskt Engine Logs.txt";
            try
            {
                return new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.File(filePath, rollingInterval: logInterval)
                    .CreateLogger();
            }
            catch (Exception)
            {
                filePath = Folders.GetFolder(FolderType.LogFolder) + "\\taskt Engine Logs.txt";
                return new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.File(filePath, rollingInterval: logInterval)
                    .CreateLogger();
            }     
        }

        public Logger CreateHTTPLogger(string uri)
        {
            return new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Http(uri)
                    .CreateLogger();
        }

        public Logger CreateSignalRLogger(string url, string logHub = "LogHub", string[] logGroupNames = null, string[] logUserIds = null)
        {
            return new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.SignalRClient(url,
                      hub: logHub, // default is LogHub
                      groupNames: logGroupNames, // default is null
                      userIds: logUserIds)// default is null
                    .CreateLogger();
        }

        public Logger CreateJsonLogger(string componentName, RollingInterval logInterval)
        {
            return new LoggerConfiguration()
                    .WriteTo.File(new CompactJsonFormatter(), Folders.GetFolder(FolderType.LogFolder) 
                        + "\\taskt " + componentName + " Logs.txt", rollingInterval: logInterval)
                    .CreateLogger();
        }
    }
}
