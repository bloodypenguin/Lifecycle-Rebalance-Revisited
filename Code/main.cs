using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ICities;
using System.Diagnostics;
using Boformer.Redirection;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using System.Linq;
using Harmony;

namespace LifecycleRebalanceRevisited
{
    public class LoadingExtension : LoadingExtensionBase
    {
        const string HarmonyID = "com.github.algernon-A.csl.lifecyclerebalancerevisited";
        private HarmonyInstance _harmony = HarmonyInstance.Create(HarmonyID);

        public const String XML_FILE = "WG_CitizenEdit.xml";
        private readonly Dictionary<MethodInfo, Redirector> redirectsOnCreated = new Dictionary<MethodInfo, Redirector>();

        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first
        private string currentFileLocation = "";
        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;
        private static Stopwatch sw;

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
                sw = Stopwatch.StartNew();

                readFromXML();

                // Do conversion from survivalProbInXML
                for (int i = 0; i < DataStore.survivalProbInXML.Length; ++i)
                {
                    // Natural log, C# is weird with names and this is approximate anyway
                    // TODO - Fix survival being at 0 log(0) causes issues!
                    DataStore.survivalProbCalc[i] = (int) (100000 - (100000 * (1 + (Math.Log(DataStore.survivalProbInXML[i]) / 25))));
                }

                // Do conversion from sicknessProbInXML
                for (int i = 0; i < DataStore.sicknessProbInXML.Length; ++i)
                {
                    // Simple division
                    DataStore.sicknessProbCalc[i] = (int)(100000 * ((DataStore.sicknessProbInXML[i]) / 25));
                }

                DataStore.citizenNumberBounds = new int[DataStore.lifeSpanMultiplier + 1];
                DataStore.citizenNumberBounds[0] = 0;
                DataStore.citizenNumberBounds[DataStore.citizenNumberBounds.Length - 1] = CitizenManager.MAX_CITIZEN_COUNT + 1;
                int increment = CitizenManager.MAX_CITIZEN_COUNT / DataStore.lifeSpanMultiplier;

                for (int i = 1; i < DataStore.citizenNumberBounds.Length - 1; ++i) // Ignore ends
                {
                    DataStore.citizenNumberBounds[i] = DataStore.citizenNumberBounds[i - 1] + increment;
                }

                Redirect();
                
                // Harmony patches.
                UnityEngine.Debug.Log("Lifecycle Rebalance Revisited: version 1.1 loading.");
                _harmony.PatchAll(GetType().Assembly);
                UnityEngine.Debug.Log("Lifecycle Rebalance Revisited: patching complete.");

                sw.Stop();
                UnityEngine.Debug.Log("WG_CitizenEdit: Successfully loaded in " + sw.ElapsedMilliseconds + " ms.");

            }
        }


        public override void OnReleased()
        {
            if (isModEnabled)
            {
                isModEnabled = false;

                RevertRedirect();
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
            // Check to see if an conflicting mod has been detected - if so, alert the user and abort operation.
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
                    Debugging.panelMessage("Successfully loaded in " + sw.ElapsedMilliseconds + " ms.");

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
                Debugging.panelMessage(e.Message);
            }
        }


        private void Redirect()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                try
                {
                    var r = RedirectionUtil.RedirectType(type);
                    if (r != null)
                    {
                        foreach (var pair in r)
                        {
                            redirectsOnCreated.Add(pair.Key, pair.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"An error occured while applying {type.Name} redirects!");
                    UnityEngine.Debug.Log(e.StackTrace);
                }
            }
        }

        private void RevertRedirect()
        {
            foreach (var kvp in redirectsOnCreated)
            {
                try
                {
                    kvp.Value.Revert();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"An error occured while reverting {kvp.Key.Name} redirect!");
                    UnityEngine.Debug.Log(e.StackTrace);
                }
            }
            redirectsOnCreated.Clear();
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
                    UnityEngine.Debug.LogException(e);
                }
            }
            else
            {
                UnityEngine.Debug.Log("Configuration file not found. Will output new file to : " + currentFileLocation);
            }
        }
    }
}
