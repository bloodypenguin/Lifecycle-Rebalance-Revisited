using System;
using System.IO;
using System.Xml;
using System.Text;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using System.Linq;
using Harmony;
using UnityEngine;


namespace LifecycleRebalanceRevisited
{
    public class LoadingExtension : LoadingExtensionBase
    {
        const string HarmonyID = "com.github.algernon-A.csl.lifecyclerebalancerevisited";
        private HarmonyInstance _harmony = HarmonyInstance.Create(HarmonyID);

        public const String XML_FILE = "WG_CitizenEdit.xml";

        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first
        private string currentFileLocation = "";
        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;

        // Used to flag if a conflicting mod is running.
        private static bool conflictingMod = false;
        
        public static bool IsModEnabled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id && mod.isEnabled));
        }


        public override void OnCreated(ILoading loading)
        {
            // Check for original WG Citizen Lifecycle Rebalance; if it's enabled, flag and don't activate this mod.
            if (IsModEnabled(654707599ul))
            {
                conflictingMod = true;
            }
            else if (!isModEnabled)
            {
                isModEnabled = true;
                UnityEngine.Debug.Log("Lifecycle Rebalance Revisited v" + LifecycleRebalanceRevisitedMod.version + " loading.");

                readFromXML();

                StringBuilder logMessage = new StringBuilder("Lifecycle Rebalance Revisited: survival probability table:\r\n");

                // Do conversion from survivalProbInXML
                for (int i = 0; i < DataStore.survivalProbInXML.Length; ++i)
                {
                    // Game defines years as being age divided by 3.5.  Hence, 35 age increments per decade.
                    double power = 1 / 35d;

                    // Using 100,000 as equivalent to 100%, for precision (as final figures are integer).
                    // Calculation is chance of death: 100% - (DecadeSurvival% ^ (1/IncrementsPerDecade)).
                    DataStore.survivalProbCalc[i] = (int)(100000 - (Math.Pow(DataStore.survivalProbInXML[i], power) * 100000));
                    logMessage.AppendLine(i + ": " + DataStore.survivalProbInXML[i] + " = " + DataStore.survivalProbCalc[i]);
                }
                UnityEngine.Debug.Log(logMessage);


                // Do conversion from sicknessProbInXML
                for (int i = 0; i < DataStore.sicknessProbInXML.Length; ++i)
                {
                    // Simple division
                    DataStore.sicknessProbCalc[i] = (int)(100000 * ((DataStore.sicknessProbInXML[i]) / 25));
                }
                
                // Harmony patches.
                _harmony.PatchAll(GetType().Assembly);
                UnityEngine.Debug.Log("Lifecycle Rebalance Revisited: patching complete.");
            }
        }


        public override void OnReleased()
        {
            if (isModEnabled)
            {
                isModEnabled = false;

                // Unapply Harmony patches.
                _harmony.UnpatchAll(HarmonyID);
                UnityEngine.Debug.Log("Lifecycle Rebalance Revisited: patches unapplied.");
            }
        }


        public override void OnLevelUnloading()
        {
            if (isLevelLoaded)
            {
                isLevelLoaded = false;
            }
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            // Check to see if a conflicting mod has been detected - if so, alert the user and abort operation.
            if (conflictingMod)
            {
                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("Lifecycle Rebalance Revisited", "Original WG Citizen Lifecycle Rebalance mod detected - Lifecycle Rebalance Revisited is shutting down to protect your game.  Only ONE of these mods can be enabled at the same time; please unsubscribe from WG Citizen Lifecycle Rebalance, which is now deprecated!", false);
            }
            else if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                if (!isLevelLoaded)
                {
                    isLevelLoaded = true;
                    Debugging.releaseBuffer();

                    // Prime Threading.counter to continue from frame index
                    int temp = (int) (Singleton<SimulationManager>.instance.m_currentFrameIndex / 4096u);
                    Threading.counter = temp % DataStore.lifeSpanMultiplier;
                }
            }

            try
            {
                WG_XMLBaseVersion xml = new XML_VersionTwo();
                xml.writeXML(currentFileLocation);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Lifecycle Rebalance Revisited: XML writing exception:\r\n" + e.Message);
            }

            UnityEngine.Debug.Log("Lifecycle Rebalance Revisited successfully loaded.");
        }


        /// <summary>
        ///
        /// </summary>
        private void readFromXML()
        {
            // Switch to default which is the cities skylines in the application data area.
            currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XML_FILE;

            if (File.Exists(currentFileLocation))
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionTwo();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(currentFileLocation);
                    int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);
                    if (version == 1)
                    {
                        reader = new XML_VersionOne();

                        // Make a back up copy of the old system to be safe
                        File.Copy(currentFileLocation, currentFileLocation + ".ver1", true);
                        string error = "Detected an old version of the XML (v1). " + currentFileLocation + ".ver1 has been created for future reference and will be upgraded to the new version.";
                        Debugging.bufferWarning(error);
                        UnityEngine.Debug.Log(error);
                    }
                    else if (version <= 0) // Uh oh... version 0 was a while back..
                    {
                        string error = "Detected an unsupported version of the XML (v0 or less). Backing up for a new configuration as :" + currentFileLocation + ".ver0";
                        Debugging.bufferWarning(error);
                        UnityEngine.Debug.Log(error);
                        File.Copy(currentFileLocation, currentFileLocation + ".ver0", true);
                        return;
                    }
                    reader.readXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.bufferWarning("The following exception(s) were detected while loading the XML file. Some (or all) values may not be loaded.");
                    Debugging.bufferWarning(e.Message);
                }
            }
            else
            {
                UnityEngine.Debug.Log("Lifecycle Rebalance Revisited: configuration file not found. Will output new file to : " + currentFileLocation);
            }
        }
    }
}
