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
        public const String OLD_XML_FILE = "WG_ResidentTravel.xml";
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
                    WG_XMLBaseVersion xml = new XML_VersionTwo();
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
            string oldFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + OLD_XML_FILE;
            currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XML_FILE;

            if (File.Exists(oldFileLocation))
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionOne();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(currentFileLocation);
                    reader.readXML(doc);
                    UnityEngine.Debug.Log("Upgrading XML. New file to be created: " + currentFileLocation);
                    File.Move(oldFileLocation, oldFileLocation + ".old");
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.panelMessage(e.Message);
                }
            }
            else if (File.Exists(currentFileLocation))
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionTwo();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(currentFileLocation);
                    reader.readXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.panelMessage(e.Message);
                }
            }
            else
            {
                Debugging.panelMessage("Configuration file not found. Will output new file to : " + currentFileLocation);
            }
        }
    }
}
