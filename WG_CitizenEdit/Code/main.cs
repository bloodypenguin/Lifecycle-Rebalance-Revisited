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


namespace WG_CitizenEdit
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public const String XML_FILE = "WG_ResidentTravel.xml";

        private static string currentFileLocation;
        private static string[] methods = { "GetBikeProbability", "GetCarProbability", "GetTaxiProbability" };
        private static byte[][] segments = new byte[3][];
        private static bool isModEnabled = false;

        public override void OnCreated(ILoading loading)
        {
            if (!isModEnabled)
            {
                // Replace the one method call which is called when the city is loaded and EnsureCitizenUnits is used
                // ResidentialAI -> Game_ResidentialAI. This stops the buildings from going to game defaults on load.
                // This has no further effects on buildings as the templates are replaced by ResidentialAIMod
                for (int i = 0; i < methods.Length; i++)
                {
                    var oldMethod = typeof(ResidentAI).GetMethod(methods[i], 
                                                                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, 
                                                                 null,
                                                                 new Type[] { typeof(ushort), typeof(CitizenInstance).MakeByRefType(), typeof(Citizen.AgeGroup) },
                                                                 null);
                    var newMethod = typeof(ResidentAIMod).GetMethod(methods[i],
                                                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                                                    null,
                                                                    new Type[] { typeof(ushort), typeof(CitizenInstance).MakeByRefType(), typeof(Citizen.AgeGroup) },
                                                                    null);

                    try
                    {
                        segments[i] = RedirectionHelper.RedirectCalls(oldMethod, newMethod);
                    }
                    catch(Exception e)
                    {
                        Debugging.writeDebugToFile(methods[i] + ": " + e.Message);
                    }
                }
                isModEnabled = true;
            }
        }

        public override void OnReleased()
        {
            if (isModEnabled)
            {
                for (int i = 0; i < methods.Length; i++)
                {
                    var oldMethod = typeof(ResidentAI).GetMethod(methods[i],
                                                                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                                                 null,
                                                                 new Type[] { typeof(ushort), typeof(CitizenInstance).MakeByRefType(), typeof(Citizen.AgeGroup) },
                                                                 null);

                    RedirectionHelper.RestoreCalls(oldMethod, segments[i]);
                }
                isModEnabled = false;
            }
        }

        public override void OnLevelUnloading()
        {
            try
            {
                XML_VersionOne xml = new XML_VersionOne();
                xml.writeXML(currentFileLocation);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
            }
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                Stopwatch sw = Stopwatch.StartNew();
                readFromXML();
                sw.Stop();

                Debugging.panelMessage("Successfully loaded in " + sw.ElapsedMilliseconds + " ms.");
            }
        }


        /// <summary>
        ///
        /// </summary>
        private void readFromXML()
        {
            currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XML_FILE;

            if (File.Exists(currentFileLocation))
            {
                // Load in from XML - Designed to be flat file for ease
                XML_VersionOne reader = new XML_VersionOne();
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
