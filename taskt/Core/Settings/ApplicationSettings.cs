﻿using System;
using System.IO;
using taskt.Core.IO;
using System.Xml.Serialization;

namespace taskt.Core.Settings
{
    /// <summary>
    /// Defines settings for the entire application
    /// </summary>
    [Serializable]
    public class ApplicationSettings
    {
        public ServerSettings ServerSettings { get; set; }
        public EngineSettings EngineSettings { get; set; }
        public ClientSettings ClientSettings { get; set; }
        public LocalListenerSettings ListenerSettings { get; set; }

        public ApplicationSettings()
        {
            ServerSettings = new ServerSettings();
            EngineSettings = new EngineSettings();
            ClientSettings = new ClientSettings();
            ListenerSettings = new LocalListenerSettings();
        }

        public void Save(ApplicationSettings appSettings)
        {
            //create settings directory
            var settingsDir = Folders.GetFolder(Folders.FolderType.SettingsFolder);

            //if directory does not exist then create directory
            if (!Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }

            //create file path
            var filePath = Path.Combine(settingsDir, "AppSettings.xml");

            //create filestream
            var fileStream = File.Create(filePath);

            //output to xml file
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
            serializer.Serialize(fileStream, appSettings);
            fileStream.Close();
        }

        public ApplicationSettings GetOrCreateApplicationSettings()
        {
            //create settings directory
            var settingsDir = Folders.GetFolder(Folders.FolderType.SettingsFolder);

            //create file path
            var filePath = Path.Combine(settingsDir, "AppSettings.xml");

            ApplicationSettings appSettings;
            if (File.Exists(filePath))
            {
                //open file and return it or return new settings on error
                var fileStream = File.Open(filePath, FileMode.Open);

                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
                    appSettings = (ApplicationSettings)serializer.Deserialize(fileStream);
                }
                catch (Exception)
                {
                    appSettings = new ApplicationSettings();
                }

                fileStream.Close();
            }
            else
            {
                appSettings = new ApplicationSettings();
            }

            return appSettings;
        }
    }
}