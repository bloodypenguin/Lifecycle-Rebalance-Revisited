using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using ICities;
using UnityEngine;
using ColossalFramework.Plugins;
using System.Diagnostics;
using Boformer.Redirection;

namespace WG_CitizenEdit
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public const String XML_FILE = "WG_CitizenEdit.xml";
        private readonly Dictionary<MethodInfo, Redirector> redirectsOnCreated = new Dictionary<MethodInfo, Redirector>();

        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first
        private string currentFileLocation = "";
        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;


        public override void OnCreated(ILoading loading)
        {
            if (!isModEnabled)
            {
                isModEnabled = true;
                readFromXML();

                // Do conversion from survivalProbInXML
                for (int i = 0; i < DataStore.survivalProbInXML.Length; ++i)
                {
                    // Natural log, C# is weird with names and this is approximate anyway
                    DataStore.survivalProbCalc[i] = (int) (100000 * (1 + (Math.Log(DataStore.survivalProbInXML[i]) / 25)));
                }

                DataStore.citizenNumberBounds = new int[DataStore.lifeSpanMultiplier + 1];
                DataStore.citizenNumberBounds[0] = 0;
                DataStore.citizenNumberBounds[DataStore.citizenNumberBounds.Length - 1] = CitizenManager.MAX_CITIZEN_COUNT + 1;
                int increment = CitizenManager.MAX_CITIZEN_COUNT / DataStore.lifeSpanMultiplier;

                for (int i = 1; i < DataStore.citizenNumberBounds.Length - 1; ++i) // Ignore ends
                {
                    DataStore.citizenNumberBounds[i] = DataStore.citizenNumberBounds[i - 1] + increment;
Debugging.writeDebugToFile("bounds: " + DataStore.citizenNumberBounds[i - 1] + ", " + DataStore.citizenNumberBounds[i]);
                }

                Redirect();
            }
        }


        public override void OnReleased()
        {
            if (isModEnabled)
            {
                isModEnabled = false;

                try
                {
                    WG_XMLBaseVersion xml = new XML_VersionOne();
                    xml.writeXML(currentFileLocation);
                }
                catch (Exception e)
                {
                    Debugging.panelMessage(e.Message);
                }

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
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                if (!isLevelLoaded)
                {
                    isLevelLoaded = true;
                }
                Debugging.releaseBuffer();
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
                WG_XMLBaseVersion reader = new XML_VersionOne();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(currentFileLocation);
                    reader.readXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.bufferWarning(e.Message);
                }
            }
            else
            {
                Debugging.bufferWarning("Configuration file not found. Will output new file to : " + currentFileLocation);
            }
        }
    }
}
