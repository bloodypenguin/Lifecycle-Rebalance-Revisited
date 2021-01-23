using System;
using System.IO;
using System.Xml;
using System.Text;
using ICities;
using ColossalFramework;
using LifecycleRebalance.MessageBox;


namespace LifecycleRebalance
{
    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public class Loading : LoadingExtensionBase
    {
        // Used to flag if a conflicting mod is running.
        private static bool conflictingMod = false;

        // Setttings file.
        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first.
        public const String XML_FILE = "WG_CitizenEdit.xml";
        public static SettingsFile settingsFile;
        public static string currentFileLocation = "";

        // Status flags.
        public static volatile bool isModCreated = false;
        private static bool isModEnabled = false;


        /// <summary>
        /// Called by the game when the mod is initialised at the start of the loading process.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            // Don't do anything if not in game (e.g. if we're going into an editor).
            if (loading.currentMode != AppMode.Game)
            {
                isModEnabled = false;
                Logging.KeyMessage("not loading into game, skipping activation");
                return;
            }

            // Check for mod conflicts.
            if (ModUtils.IsModConflict())
            {
                // Conflict detected.
                conflictingMod = true;
                isModEnabled = false;

                // Unload Harmony patches and exit before doing anything further.
                Patcher.UnpatchAll();
                return;
            }

            // Passed all checks - okay to load (if we haven't already fo some reason).
            if (!isModEnabled)
            {
                isModEnabled = true;
                Logging.KeyMessage("v", LifecycleRebalance.Version, " loading");
            }
        }

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
            base.OnLevelLoaded(mode);

            // Check to see if a conflicting mod has been detected.
            if (conflictingMod)
            {
                // Mod conflict detected - display warning notification and exit.
                ListMessageBox modConflictBox = MessageBoxBase.ShowModal<ListMessageBox>();

                // Key text items.
                modConflictBox.AddParas(Translations.Translate("ERR_CON0"), Translations.Translate("LBR_ERR_FAT"), Translations.Translate("LBR_ERR_CON0"), Translations.Translate("ERR_CON1"));

                // Add conflicting mod name(s).
                modConflictBox.AddList(ModUtils.conflictingModNames.ToArray());

                // Closing para.
                modConflictBox.AddParas(Translations.Translate("LBR_ERR_CON1"));
            }

            // Don't do anything if we're not enabled or we've already been here.
            if (isModEnabled && !isModCreated)
            {
                // Wait for Harmony if it hasn't already happened.
                //if (!Patcher.patched)
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

                        // Harmony 2 wasn't loaded; display warning notification and exit.
                        ListMessageBox harmonyBox = MessageBoxBase.ShowModal<ListMessageBox>();

                        // Key text items.
                        harmonyBox.AddParas(Translations.Translate("ERR_HAR0"), Translations.Translate("LBR_ERR_HAR"), Translations.Translate("LBR_ERR_FAT"), Translations.Translate("ERR_HAR1"));

                        // List of dot points.
                        harmonyBox.AddList(Translations.Translate("ERR_HAR2"), Translations.Translate("ERR_HAR3"));

                        // Closing para.
                        harmonyBox.AddParas(Translations.Translate("MES_PAGE"));
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

                // Display update notifications.
                WhatsNew.ShowWhatsNew();
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
