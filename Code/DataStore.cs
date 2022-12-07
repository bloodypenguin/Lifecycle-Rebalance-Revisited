// <copyright file="DataStore.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using AlgernonCommons;

    /// <summary>
    /// Mod datastore.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Datastore")]
    internal class DataStore
    {
        /// <summary>
        /// XML file name.
        /// </summary>
        public const string FileName = "WG_CitizenEdit.xml";

        /// <summary>
        /// Transport array index - car.
        /// </summary>
        public const int Car = 0;

        /// <summary>
        /// Transport array index - bike.
        /// </summary>
        public const int Bike = 1;

        /// <summary>
        /// Transport array index - taxi.
        /// </summary>
        public const int Taxi = 2;

        /// <summary>
        /// Citizen ageing speed.
        /// </summary>
        public static int LifeSpanMultiplier = 3;

        /// <summary>
        /// Citizen working speed.
        /// </summary>
        public static int WorkSpeedMultiplier = 1;

        /// <summary>
        /// Survival number to next decile. In units of 1/100000
        /// Each decile is 25 ticks. There are 5 ticks at the end. Kill the last one
        /// Source: http://www.aga.gov.au/publications/life_table_2010-12/ (averaged out with both genders and in blocks of 10 years).
        /// Per decile, raw data.
        /// </summary>
        public static double[] SurvivalProbInXML = { 0.99514, 0.99823, 0.99582, 0.99326, 0.98694, 0.97076, 0.93192, 0.82096, 0.50858, 0.11799, 0.01764 };

        /// <summary>
        /// Survival probability calculated factors.
        /// </summary>
        public static int[] SurvivalProbCalc = new int[SurvivalProbInXML.Length];

        /// <summary>
        /// Chance per decile of random sickness.
        /// </summary>
        public static double[] SicknessProbInXML = { 0.0125, 0.0075, 0.01, 0.01, 0.015, 0.02, 0.03, 0.04, 0.05, 0.075, 0.25 };

        /// <summary>
        /// Sickness probability calculated factors.
        /// </summary>
        public static int[] SicknessProbCalc = new int[SicknessProbInXML.Length];

        /// <summary>
        /// Portion per decile who will die if in healthcare (Replaces survival).
        /// </summary>
        public static double[] SickDeathChance = { 0.005, 0.005, 0.005, 0.0075, 0.01, 0.015, 0.02, 0.025, 0.03, 0.05, 0.1 };

        /// <summary>
        /// Percentage chance of dead body not requiring transport.
        /// </summary>
        public static int AutoDeadRemovalChance = 50;

        /// <summary>
        /// Immigrant age range for incoming singles.
        /// </summary>
        public static int[] IncomingSingleAge = { 65, 165 };

        /// <summary>
        /// Immigrant age range for incoming parents of families.
        /// </summary>
        public static int[] IncomingAdultAge = { 85, 185 };

        /// <summary>
        /// Percentage chance for bike riding if relevant policy is in effect.
        /// </summary>
        public static int BikeIncrease = 10;

        /// <summary>
        /// Residential transport probabilities - low wealth (low and high density standard residential).
        /// Car, bike, taxi, by age category (child/teen/young adult/adult/senior).
        /// </summary>
        public static int[][][] TransportLowWealth =
        {
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 10, 30, 0, },
                new int[] { 45, 20, 1, },
                new int[] { 60, 10, 2, },
                new int[] { 30,  2, 3, },
            },
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 2, 30, 0, },
                new int[] { 3, 20, 1, },
                new int[] { 5, 10, 2, },
                new int[] { 4,  2, 3, },
            },
        };

        /// <summary>
        /// Residential transport probabilities - medium wealth (low and high density standard residential).
        /// wealth, home building density, age, transportmode.
        /// Car, bike, taxi, by age category (child/teen/young adult/adult/senior).
        /// </summary>
        public static int[][][] TransportMedWealth =
        {
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 12, 30, 1, },
                new int[] { 50, 20, 2, },
                new int[] { 65, 10, 4, },
                new int[] { 35,  2, 6, },
            },
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 3, 30, 1, },
                new int[] { 5, 20, 2, },
                new int[] { 7, 10, 3, },
                new int[] { 6,  2, 5, },
            },
        };

        /// <summary>
        /// Residential transport probabilities - high wealth (low and high density standard residential).
        /// Car, bike, taxi, by age category (child/teen/young adult/adult/senior).
        /// </summary>
        public static int[][][] TransportHighWealth =
        {
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 15, 30, 2, },
                new int[] { 55, 20, 3, },
                new int[] { 70, 10, 4, },
                new int[] { 45,  2, 6, },
            },
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 4, 30, 2, },
                new int[] { 7, 20, 3, },
                new int[] { 9, 10, 4, },
                new int[] { 8,  1, 5, },
            },
        };

        /// <summary>
        /// Wall-to-wall residential transport probabilities - low wealth.
        /// Car, bike, taxi, by age category (child/teen/young adult/adult/senior).
        /// </summary>
        public static int[][] TransportLowWealthW2W =
        {
            new int[] { 0, 40, 0, },
            new int[] { 2, 30, 0, },
            new int[] { 3, 20, 1, },
            new int[] { 5, 10, 2, },
            new int[] { 4,  2, 3, },
        };

        /// <summary>
        /// Wall-to-wall residential transport probabilities - medium wealth.
        /// Car, bike, taxi, by age category (child/teen/young adult/adult/senior).
        /// </summary>
        public static int[][] TransportMedWealthW2W =
        {
            new int[] { 0, 40, 0, },
            new int[] { 3, 30, 1, },
            new int[] { 5, 20, 2, },
            new int[] { 7, 10, 3, },
            new int[] { 6,  2, 5, },
        };

        /// <summary>
        /// Wall-to-wall residential transport probabilities - high wealth.
        /// Car, bike, taxi, by age category (child/teen/young adult/adult/senior).
        /// </summary>
        public static int[][] TransportHighWealthW2W =
        {
            new int[] { 0, 40, 0, },
            new int[] { 4, 30, 2, },
            new int[] { 7, 20, 3, },
            new int[] { 9, 10, 4, },
            new int[] { 8,  1, 5, },
        };

        /// <summary>
        /// Eco-residential transport probabilities - low wealth (low and high density).
        /// Car, bike, taxi, by age category (child/teen/young adult/adult/senior).
        /// </summary>
        public static int[][][] TranportLowWealthEco =
        {
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 7, 30, 0, },
                new int[] { 25, 20, 1, },
                new int[] { 40, 10, 2, },
                new int[] { 20,  5, 3, },
            },
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 1, 30, 0, },
                new int[] { 2, 20, 1, },
                new int[] { 4, 10, 2, },
                new int[] { 3,  2, 3, },
            },
        };

        /// <summary>
        /// Eco-residential transport probabilities - medium wealth (low and high density).
        /// Car, bike, taxi, by age category (child/teen/young adult/adult/senior).
        /// </summary>
        public static int[][][] TransportMedWealthEco =
        {
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 8, 30, 1, },
                new int[] { 33, 20, 2, },
                new int[] { 43, 10, 4, },
                new int[] { 23,  2, 6, },
            },
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 2, 30, 1, },
                new int[] { 4, 20, 2, },
                new int[] { 5, 10, 3, },
                new int[] { 4,  2, 5, },
            },
        };

        /// <summary>
        /// Eco-residential transport probabilities - high wealth (low and high density).
        /// wealth, home building density, age, transportmode.
        /// Car, bike, taxi, by age category (child/teen/young adult/adult/senior).
        /// </summary>
        public static int[][][] TransportHighWealthEco =
        {
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 10, 30, 2, },
                new int[] { 37, 20, 3, },
                new int[] { 46, 10, 4, },
                new int[] { 30,  2, 6, },
            },
            new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 3, 30, 2, },
                new int[] { 4, 20, 3, },
                new int[] { 6, 10, 4, },
                new int[] { 5,  1, 5, },
            },
        };

        // Setttings file.
        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first.
        private static string s_currentFileLocation;

        /// <summary>
        /// Calculates and populates the sickness probabilities DataStore table from the configuration file.
        /// </summary>
        internal static void CalculateSicknessProbabilities()
        {
            StringBuilder logMessage = new StringBuilder("sickness probability table using factor of ");
            logMessage.Append(ModSettings.Settings.DecadeFactor);
            logMessage.AppendLine(":");

            // Do conversion from sicknessProbInXML
            for (int i = 0; i < SicknessProbInXML.Length; ++i)
            {
                // Simple division
                SicknessProbCalc[i] = (int)(100000 * (SicknessProbInXML[i] * ModSettings.Settings.DecadeFactor));
                logMessage.Append(i);
                logMessage.Append(": ");
                logMessage.Append(SicknessProbInXML[i]);
                logMessage.Append(" : ");
                logMessage.Append(SicknessProbCalc[i]);
                logMessage.Append(" : ");
                logMessage.AppendLine(((int)(100000 * SicknessProbInXML[i] / 25)).ToString());
            }

            Logging.Message(logMessage);
        }

        /// <summary>
        /// Reads the datastore from XML.
        /// </summary>
        internal static void ReadFromXML()
        {
            // Switch to default which is the cities skylines in the application data area.
            s_currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + FileName;

            if (File.Exists(s_currentFileLocation))
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionThree();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(s_currentFileLocation);
                    int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);
                    if (version == 2)
                    {
                        // Make a back up copy of the old system to be safe
                        File.Copy(s_currentFileLocation, s_currentFileLocation + ".ver2", true);
                        Logging.KeyMessage("Detected an old version of the XML (v2). ", s_currentFileLocation, ".ver2 has been created for future reference and will be upgraded to the new version");

                        reader = new XML_VersionTwo();
                    }
                    else if (version == 1)
                    {
                        // Make a back up copy of the old system to be safe
                        File.Copy(s_currentFileLocation, s_currentFileLocation + ".ver1", true);
                        Logging.KeyMessage("Detected an old version of the XML (v1). ", s_currentFileLocation, ".ver1 has been created for future reference and will be upgraded to the new version");

                        reader = new XML_VersionOne();

                    }
                    else if (version <= 0)
                    {
                        // Uh oh... version 0 was a while back..
                        Logging.KeyMessage("Detected an unsupported version of the XML (v0 or less). Backing up for a new configuration as :", s_currentFileLocation, ".ver0");
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
        /// Writes the current configuration to file.
        /// </summary>
        internal static void SaveXML()
        {
            // Create configuration file if none exists.
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionThree();
                xml.WriteXML(s_currentFileLocation);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "XML configuration file error");
            }
        }
    }
}