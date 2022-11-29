// <copyright file="Loading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using ICities;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, Patcher>
    {
        // Setttings file.
        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first.
        public const string XML_FILE = "WG_CitizenEdit.xml";
        public static string s_currentFileLocation = "";

        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);

            // Ensure custom transport probability patches are applied, if applicable.
            PatcherManager<Patcher>.Instance.ApplyTransportPatches(ModSettings.Settings.UseTransportModes);

            // Apply sickness probabilities.
            CalculateSicknessProbabilities();

            // Prime Threading.counter to continue from frame index.
            int temp = (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex / 4096u);
            Threading.counter = temp % DataStore.lifeSpanMultiplier;

            // Create configuration file if none exists.
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionTwo();
                xml.WriteXML(s_currentFileLocation);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "XML configuration file error");
            }
        }

        /// <summary>
        ///
        /// </summary>
        public static void ReadFromXML()
        {
            // Switch to default which is the cities skylines in the application data area.
            s_currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XML_FILE;

            if (File.Exists(s_currentFileLocation))
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionTwo();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(s_currentFileLocation);
                    int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);
                    if (version == 1)
                    {
                        reader = new XML_VersionOne();

                        // Make a back up copy of the old system to be safe
                        File.Copy(s_currentFileLocation, s_currentFileLocation + ".ver1", true);
                        Logging.Message("Detected an old version of the XML (v1). ", s_currentFileLocation, ".ver1 has been created for future reference and will be upgraded to the new version");
                    }
                    else if (version <= 0) // Uh oh... version 0 was a while back..
                    {
                        Logging.Message("Detected an unsupported version of the XML (v0 or less). Backing up for a new configuration as :", s_currentFileLocation,".ver0");
                        File.Copy(s_currentFileLocation, s_currentFileLocation + ".ver0", true);
                        return;
                    }
                    reader.ReadXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Logging.LogException(e, "exception while loading the XML file. Some (or all) values may not be loaded");
                }
            }
            else
            {
                Logging.Message("configuration file not found. Will output new file to : ", s_currentFileLocation);
            }
        }


        /// <summary>
        /// Calculates and populates the sickness probabilities DataStore table from the configuration file.
        /// </summary>
        public static void CalculateSicknessProbabilities()
        {
            StringBuilder logMessage = new StringBuilder("sickness probability table using factor of " + ModSettings.Settings.decadeFactor + ":" + Environment.NewLine);

            // Do conversion from sicknessProbInXML
            for (int i = 0; i < DataStore.sicknessProbInXML.Length; ++i)
            {
                // Simple division
                DataStore.sicknessProbCalc[i] = (int)(100000 * ((DataStore.sicknessProbInXML[i]) * ModSettings.Settings.decadeFactor));
                logMessage.AppendLine(i + ": " + DataStore.sicknessProbInXML[i] + " : " + DataStore.sicknessProbCalc[i] + " : " + (int)(100000 * ((DataStore.sicknessProbInXML[i]) / 25)));
            }
            Logging.Message(logMessage);
        }
    }
}
