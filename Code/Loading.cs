using System;
using System.IO;
using System.Xml;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using System.Linq;
using Harmony;
using UnityEngine;


namespace LifecycleRebalance
{
    public class Loading : LoadingExtensionBase
    {
        public const string HarmonyID = "com.github.algernon-A.csl.lifecyclerebalancerevisited";
        public static HarmonyInstance harmony = HarmonyInstance.Create(HarmonyID);

        public const String XML_FILE = "WG_CitizenEdit.xml";

        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first
        private string currentFileLocation = "";
        private static volatile bool isLevelLoaded = false;
        public static volatile bool isModCreated = false;

        // Used to flag if a conflicting mod is running.
        private static bool conflictingMod = false;

        
        public static bool IsModEnabled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id && mod.isEnabled));
        }


        public override void OnCreated(ILoading loading)
        {
            UnityEngine.Debug.Log("Lifecycle Rebalance Revisited v" + LifecycleRebalance.version + " loading.");

            // Check for original WG Citizen Lifecycle Rebalance; if it's enabled, flag and don't activate this mod.
            if (IsModEnabled(654707599ul))
            {
                conflictingMod = true;
                Debug.Log("Lifecycle Rebalance Revisited: incompatible mod detected.  Shutting down.");
            }
            else if (!isModCreated)
            {
                // Harmony patches.
                harmony.PatchAll(GetType().Assembly);
                UnityEngine.Debug.Log("Lifecycle Rebalance Revisited: patching complete.");

                isModCreated = true;

                // Load configuation file.
                readFromXML();

                // Load and apply mod settings.
                SettingsFile settings = Configuration<SettingsFile>.Load();
                ModSettings.LegacyCalcs = settings.UseLegacy;
                ModSettings.CustomRetirement = settings.CustomRetirement;
                ModSettings.RetirementYear = settings.RetirementYear;
                Debugging.UseDeathLog = settings.LogDeaths;
                Debugging.UseImmigrationLog = settings.LogImmigrants;
                Debugging.UseTransportLog = settings.LogTransport;

                // Do conversion from sicknessProbInXML
                for (int i = 0; i < DataStore.sicknessProbInXML.Length; ++i)
                {
                    // Simple division
                    DataStore.sicknessProbCalc[i] = (int)(100000 * ((DataStore.sicknessProbInXML[i]) / 25));
                }
            }
        }


        public override void OnReleased()
        {
            if (isModCreated)
            {
                isModCreated = false;

                // Unapply Harmony patches.
                harmony.UnpatchAll(HarmonyID);
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
                    Debugging.ReleaseBuffer();

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

            Debug.Log("Lifecycle Rebalance Revisited: death logging " + (Debugging.UseDeathLog ? "enabled" : "disabled") + ", immigration logging " + (Debugging.UseImmigrationLog ? "enabled" : "disabled") + ", transportation logging " + (Debugging.UseTransportLog ? "enabled." : "disabled."));

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
