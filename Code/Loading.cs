using System;
using System.IO;
using System.Xml;
using System.Text;
using UnityEngine;
using ICities;
using ColossalFramework;


namespace LifecycleRebalance
{
    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public class Loading : LoadingExtensionBase
    {
        public const String XML_FILE = "WG_CitizenEdit.xml";
        public static SettingsFile settingsFile;

        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first.
        public static string currentFileLocation = "";
        public static volatile bool isModCreated = false;
        

        /// <summary>
        /// Called by the game when the mode is released.
        /// </summary>

        public override void OnReleased()
        {
            if (isModCreated)
            {
                isModCreated = false;
            }
        }


        /// <summary>
        /// Called by the game when level loading is complete.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            // Don't do anything if not in game.
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
            {
                Logging.KeyMessage("not loading into game; exiting");
                return;
            }

            // Don't do anything if we've already been here.
            if (!isModCreated)
            {
                // Check for mod conflicts.
                ModConflicts modConflicts = new ModConflicts();
                if (modConflicts.CheckConflicts())
                {
                    // Conflict detected.  Unpatch everything before exiting without doing anything further.
                    Patcher.UnpatchAll();
                    return;
                }

                Logging.KeyMessage("v", LifecycleRebalance.Version, " loading");

                // Wait for Harmony if it hasn't already happened.
                if (!Patcher.patched)
                {
                    // Set timeout counter, just in case.
                    DateTime startTime = DateTime.Now;

                    try
                    {
                        Logging.Message("waiting for Harmony");
                        while (!Patcher.patched)
                        {
                            if (CitiesHarmony.API.HarmonyHelper.IsHarmonyInstalled)
                            {
                                Patcher.PatchAll();
                                break;
                            }

                            // Three minutes should be sufficient wait.
                            if (DateTime.Now > startTime.AddMinutes(3))
                            {
                                throw new TimeoutException("Harmony loading timeout: " + startTime.ToString() + " : " + DateTime.Now.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "Harmony loading exception");

                        // Show error notification to user.
                        ErrorNotification notification = new ErrorNotification();
                        notification.Create();
                        ErrorNotification.headerText = "Harmony loading error!";
                        ErrorNotification.messageText = "Lifecycle Rebalance Revisited can't load properly because the required Harmony mod dependency didn't load.  In most cases, a simple game restart should be enough to fix this.\r\n\r\nIf this notification persists, please manually subscribe to the Harmony mod on the Steam workshop (if you're already subscribed, try unsubscribing and re-subscribing) and restart your game again.";
                        notification.Show();
                        return;
                    }
                }

                Logging.Message("Harmony ready, proceeding");

                // Set flag.
                isModCreated = true;

                // Load and apply mod settings (configuration file loaded above).
                settingsFile = Configuration<SettingsFile>.Load();
                ModSettings.VanillaCalcs = settingsFile.UseVanilla;
                ModSettings.LegacyCalcs = settingsFile.UseLegacy;
                ModSettings.CustomRetirement = settingsFile.CustomRetirement;
                ModSettings.RetirementYear = settingsFile.RetirementYear;
                ModSettings.UseTransportModes = settingsFile.UseTransportModes;
                ModSettings.randomImmigrantEd = settingsFile.RandomImmigrantEd;
                Logging.UseDeathLog = settingsFile.LogDeaths;
                Logging.UseImmigrationLog = settingsFile.LogImmigrants;
                Logging.UseTransportLog = settingsFile.LogTransport;
                Logging.UseSicknessLog = settingsFile.LogSickness;

                // Apply sickness probabilities.
                CalculateSicknessProbabilities();

                // Report status.
                Logging.Message("death logging ", Logging.UseDeathLog ? "enabled" : "disabled", ", immigration logging ", Logging.UseImmigrationLog ? "enabled" : "disabled", ", transportation logging ", Logging.UseTransportLog ? "enabled" : "disabled");

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
                    Logging.LogException(e, "XML configuration file error");
                }

                // Set up options panel event handler.
                OptionsPanel.OptionsEventHook();

                Logging.KeyMessage("successfully loaded");

                // Check if we need to display update notification.
                if (settingsFile.NotificationVersion != 4)
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
                        Logging.Message("Detected an old version of the XML (v1). ", currentFileLocation, ".ver1 has been created for future reference and will be upgraded to the new version");
                    }
                    else if (version <= 0) // Uh oh... version 0 was a while back..
                    {
                        Logging.Message("Detected an unsupported version of the XML (v0 or less). Backing up for a new configuration as :", currentFileLocation,".ver0");
                        File.Copy(currentFileLocation, currentFileLocation + ".ver0", true);
                        return;
                    }
                    reader.readXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Logging.LogException(e, "exception while loading the XML file. Some (or all) values may not be loaded");
                }
            }
            else
            {
                Logging.Message("configuration file not found. Will output new file to : ", currentFileLocation);
            }
        }


        /// <summary>
        /// Calculates and populates the sickness probabilities DataStore table from the configuration file.
        /// </summary>
        public static void CalculateSicknessProbabilities()
        {
            StringBuilder logMessage = new StringBuilder("sickness probability table using factor of " + ModSettings.decadeFactor + ":\r\n");

            // Do conversion from sicknessProbInXML
            for (int i = 0; i < DataStore.sicknessProbInXML.Length; ++i)
            {
                // Simple division
                DataStore.sicknessProbCalc[i] = (int)(100000 * ((DataStore.sicknessProbInXML[i]) * ModSettings.decadeFactor));
                logMessage.AppendLine(i + ": " + DataStore.sicknessProbInXML[i] + " : " + DataStore.sicknessProbCalc[i] + " : " + (int)(100000 * ((DataStore.sicknessProbInXML[i]) / 25)));
            }
            Logging.Message(logMessage.ToString());
        }
    }
}
