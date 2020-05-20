using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using UnityEngine;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;


namespace LifecycleRebalance
{
    public class Loading : LoadingExtensionBase
    {
        public const String XML_FILE = "WG_CitizenEdit.xml";
        public static SettingsFile settingsFile;

        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first
        public static string currentFileLocation = "";
        public static volatile bool isModCreated = false;
        

        public static bool IsModEnabled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id && mod.isEnabled));
        }


        public override void OnReleased()
        {
            if (isModCreated)
            {
                isModCreated = false;
            }
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            // Don't do anything if not in game.
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
            {
                Debug.Log("Lifecycle Rebalance Revisited: not loading into game; exiting.");
                return;
            }

            // Don't do anything if we've already been here.
            if (!isModCreated)
            {
                Debug.Log("Lifecycle Rebalance Revisited v" + LifecycleRebalance.Version + " loading.");

                // Check for original WG Citizen Lifecycle Rebalance; if it's enabled, flag and don't activate this mod.
                if (IsModEnabled(654707599ul))
                {
                    ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                    panel.SetMessage("Lifecycle Rebalance Revisited", "Original WG Citizen Lifecycle Rebalance mod detected - Lifecycle Rebalance Revisited is shutting down to protect your game.  Only ONE of these mods can be enabled at the same time; please unsubscribe from WG Citizen Lifecycle Rebalance, which is now deprecated!", false);
                    Debug.Log("Lifecycle Rebalance Revisited: incompatible mod detected.  Shutting down.");

                    // Unapply Harmony patches before returning without doing anything
                    Patcher.UnpatchAll();
                }

                // Load configuation file.
                readFromXML();

                // Wait for Harmony if it hasn't already happened.
                if (!Patcher.patched)
                {
                    // Set timeout counter, just in case.
                    DateTime startTime = DateTime.Now;

                    try
                    {
                        Debug.Log("Lifecycle Rebalance Revisited: waiting for Harmony.");
                        while (!Patcher.patched)
                        {
                            if (CitiesHarmony.API.HarmonyHelper.IsHarmonyInstalled)
                            {
                                Patcher.PatchAll();
                                break;
                            }

                            // Two minutes should be sufficient wait.
                            if (DateTime.Now > startTime.AddMinutes(2))
                            {
                                throw new TimeoutException("Harmony loading timeout");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Lifecycle Rebalance Revisited: Harmony loading exception!");
                        Debug.LogException(e);
                        ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                        panel.SetMessage("Lifecycle Rebalance Revisited", "Lifecycle Rebalance Revisited can't load properly because the Harmony mod dependency didn't load.  Please subscribe to the Harmony mod on the Steam workshop and restart your game.", false);
                        return;
                    }
                }

                Debug.Log("Lifecycle Rebalance Revisited: Harmony ready, proceeding.");

                // Set flag.
                isModCreated = true;

                // Load and apply mod settings (configuration file loaded above).
                settingsFile = Configuration<SettingsFile>.Load();
                ModSettings.VanillaCalcs = settingsFile.UseVanilla;
                ModSettings.LegacyCalcs = settingsFile.UseLegacy;
                ModSettings.CustomRetirement = settingsFile.CustomRetirement;
                ModSettings.RetirementYear = settingsFile.RetirementYear;
                ModSettings.UseTransportModes = settingsFile.UseTransportModes;
                Debugging.UseDeathLog = settingsFile.LogDeaths;
                Debugging.UseImmigrationLog = settingsFile.LogImmigrants;
                Debugging.UseTransportLog = settingsFile.LogTransport;
                Debugging.UseSicknessLog = settingsFile.LogSickness;

                // Apply sickness probabilities.
                CalculateSicknessProbabilities();

                // Report status and any debugging messages.
                Debug.Log("Lifecycle Rebalance Revisited: death logging " + (Debugging.UseDeathLog ? "enabled" : "disabled") + ", immigration logging " + (Debugging.UseImmigrationLog ? "enabled" : "disabled") + ", transportation logging " + (Debugging.UseTransportLog ? "enabled." : "disabled."));
                Debugging.ReleaseBuffer();

                // Prime Threading.counter to continue from frame index.
                int temp = (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex / 4096u);
                Threading.counter = temp % DataStore.lifeSpanMultiplier;
                try
                {
                    WG_XMLBaseVersion xml = new XML_VersionTwo();
                    xml.writeXML(currentFileLocation);
                }
                catch (Exception e)
                {
                    Debug.Log("Lifecycle Rebalance Revisited: XML writing exception:\r\n" + e.Message);
                }

                Debug.Log("Lifecycle Rebalance Revisited successfully loaded.");

                // Check if we need to display update notification.
                if (settingsFile.NotificationVersion != 2)
                {
                    // No update notification "Don't show again" flag found; show the notification.
                    UpdateNotification notification = new UpdateNotification();
                    notification.Create();
                    notification.Show();
                }
            }
        }


        /// <summary>
        ///
        /// </summary>
        public static void readFromXML()
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

        public static void CalculateSicknessProbabilities()
        {
            StringBuilder logMessage = new StringBuilder("Lifecycle Rebalance Revisited: sickness probability table using factor of " + ModSettings.decadeFactor + ":\r\n");

            // Do conversion from sicknessProbInXML
            for (int i = 0; i < DataStore.sicknessProbInXML.Length; ++i)
            {
                // Simple division
                DataStore.sicknessProbCalc[i] = (int)(100000 * ((DataStore.sicknessProbInXML[i]) * ModSettings.decadeFactor));
                logMessage.AppendLine(i + ": " + DataStore.sicknessProbInXML[i] + " : " + DataStore.sicknessProbCalc[i] + " : " + (int)(100000 * ((DataStore.sicknessProbInXML[i]) / 25)));
            }
            Debug.Log(logMessage);
        }
    }
}
