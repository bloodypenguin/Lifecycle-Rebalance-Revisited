// <copyright file="XML_VersionThree.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1303 // Const field names should begin with upper-case letter
namespace LifecycleRebalance
{
    using System;
    using System.IO;
    using System.Xml;
    using AlgernonCommons;

    public class XML_VersionThree : WG_XMLBaseVersion
    {
        private const string travelNodeName = "travel";

        private const string migrateNodeName = "migrate";

        private const string lifeSpanNodeName = "lifespan";
        private const string survivalNodeName = "survival";
        private const string sicknessNodeName = "sickness";
        private const string sickDieNodeName = "sickDeathRate";
        private const string cheatHearseNodeName = "cheatHearse";

        public override void ReadXML(XmlDocument doc)
        {
            XmlElement root = doc.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name.Equals(travelNodeName))
                {
                    ReadTravelNode(node);
                }
                else if (node.Name.Equals(migrateNodeName))
                {
                    ReadImmigrateNode(node);
                }
                else if (node.Name.Equals(lifeSpanNodeName))
                {
                    ReadLifeNode(node);
                }
            }
        }

        public void ReadTravelNode(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                string name = node.Name;
                int index = 0;

                switch (name)
                {
                    case "high_density":
                        index = 1;
                        break;
                    case "low_density":
                        index = 0;
                        break;
                    default:
                        // Error case
                        break;
                }

                // Read inner attributes
                ReadTravelWealthNode(node, index);
            }
        }

        public void ReadImmigrateNode(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                int[] array = null;

                if (node.Name.Equals("single_adult"))
                {
                    array = DataStore.IncomingSingleAge;
                }
                else if (node.Name.Equals("family_adult"))
                {
                    array = DataStore.IncomingAdultAge;
                }
                else
                {
                    Logging.Error("Unknown immigration node");
                }

                try
                {
                    array[0] = Convert.ToInt32(node.Attributes["min"].InnerText);
                    array[1] = Convert.ToInt32(node.Attributes["max"].InnerText);
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "readImmigrateNode exception");
                }
            }
        }

        public void ReadLifeNode(XmlNode root)
        {
            try
            {
                DataStore.LifeSpanMultiplier = Convert.ToInt32(root.Attributes["modifier"].InnerText);
            }
            catch (Exception e)
            {
                Logging.Error("lifespan multiplier was not an integer: ", e.Message, ". Setting to 3");
                DataStore.LifeSpanMultiplier = 3;
            }

            if (DataStore.LifeSpanMultiplier <= 0)
            {
                Logging.Error("Detecting a lifeSpan multiplier less than or equal to 0 . Setting to 3");
                DataStore.LifeSpanMultiplier = 3;
            }

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name.Equals(survivalNodeName))
                {
                    ReadSurvivalNode(node);
                }
                else if (node.Name.Equals(sicknessNodeName))
                {
                    ReadSicknessNode(node);
                }
                else if (node.Name.Equals(sickDieNodeName))
                {
                    ReadHospitalNode(node);
                }
                else if (node.Name.Equals(cheatHearseNodeName))
                {
                    ReadCheatHearseNode(node);
                }
            }
        }

        public void ReadSurvivalNode(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                string[] attr = node.Name.Split(new char[] { '_' });
                string name = attr[0];
                int level = Convert.ToInt32(attr[1]) - 1;

                if (name.Equals("decile"))
                {
                    try
                    {
                        DataStore.SurvivalProbInXML[level] = Convert.ToDouble(node.Attributes["survival"].InnerText) / 100.0;
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "readSurvivalNode exception");
                    }
                }
            }
        }

        public void ReadSicknessNode(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                string[] attr = node.Name.Split(new char[] { '_' });
                string name = attr[0];
                int level = Convert.ToInt32(attr[1]) - 1;

                if (name.Equals("decile"))
                {
                    try
                    {
                        DataStore.SicknessProbInXML[level] = Convert.ToDouble(node.Attributes["chance"].InnerText) / 100.0;
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "readSicknessNode exception");
                    }
                }
            }
        }

        public void ReadHospitalNode(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                string[] attr = node.Name.Split(new char[] { '_' });
                string name = attr[0];
                int level = Convert.ToInt32(attr[1]) - 1;

                if (name.Equals("decile"))
                {
                    try
                    {
                        DataStore.SickDeathChance[level] = Convert.ToDouble(node.Attributes["chance"].InnerText) / 100.0;
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "readHospitalNode exception");
                    }
                }
            }
        }

        public void ReadCheatHearseNode(XmlNode root)
        {
            try
            {
                DataStore.AutoDeadRemovalChance = (int)Convert.ToDouble(root.Attributes["chance"].InnerText);
                if ((DataStore.AutoDeadRemovalChance < 0) && (DataStore.AutoDeadRemovalChance > 100))
                {
                    Logging.Error("Cheat hearse is out of range (0-100). Setting to 50");
                    DataStore.AutoDeadRemovalChance = 50;
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "readCheatHearseNode exception");
                DataStore.AutoDeadRemovalChance = 50;
            }
        }

        public override bool WriteXML(string fullPathFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("WG_CitizenEdit");
            XmlAttribute attribute = xmlDoc.CreateAttribute("version");
            attribute.Value = "3";
            rootNode.Attributes.Append(attribute);
            xmlDoc.AppendChild(rootNode);

            rootNode.AppendChild(MakeTravelNode(xmlDoc));
            XmlComment comment = xmlDoc.CreateComment("Lower age value: Young adult (45), Adult (90), Senior (180)");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("Numbers lower than young adult may cause economic havoc.");
            rootNode.AppendChild(comment);
            rootNode.AppendChild(MakeImmigrateNode(xmlDoc));

            rootNode.AppendChild(MakeLifeNode(xmlDoc));

            if (File.Exists(fullPathFileName))
            {
                try
                {
                    if (File.Exists(fullPathFileName + ".bak"))
                    {
                        File.Delete(fullPathFileName + ".bak");
                    }

                    File.Move(fullPathFileName, fullPathFileName + ".bak");
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "exception backing up configuration file");
                }
            }

            try
            {
                xmlDoc.Save(fullPathFileName);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception writing configuration file");
                return false;  // Only time when we say there's an error
            }

            return true;
        }

        private void MakeDensityNodes(XmlDocument xmlDoc, XmlNode root, int density)
        {
            string densityElementName = (density == 0) ? "low_density" : "high_density";
            XmlNode node = xmlDoc.CreateElement(densityElementName);
            string[] type = { "low_wealth", "med_wealth", "high_wealth", "low_wealth_eco", "med_wealth_eco", "high_wealth_eco", "low_wealth_w2w", "med_wealth_w2w", "high_wealth_w2w" };

            for (int i = 0; i < type.Length; i++)
            {
                XmlNode wealthNode = xmlDoc.CreateElement(type[i]);
                int[][] array;
                switch (i)
                {
                    case 1:
                        array = DataStore.TransportMedWealth[density];
                        break;
                    case 2:
                        array = DataStore.TransportHighWealth[density];
                        break;
                    case 3:
                        array = DataStore.TranportLowWealthEco[density];
                        break;
                    case 4:
                        array = DataStore.TransportMedWealthEco[density];
                        break;
                    case 5:
                        array = DataStore.TransportHighWealthEco[density];
                        break;
                    case 6:
                        // Don't record low-density for W2W.
                        if (density == 0)
                        {
                            continue;
                        }

                        array = DataStore.TransportLowWealthW2W;
                        break;
                    case 7:
                        // Don't record low-density for W2W.
                        if (density == 0)
                        {
                            continue;
                        }

                        array = DataStore.TransportMedWealthW2W;
                        break;
                    case 8:
                        // Don't record low-density for W2W.
                        if (density == 0)
                        {
                            continue;
                        }

                        array = DataStore.TransportHighWealthW2W;
                        break;
                    case 0:
                    default:
                        array = DataStore.TransportLowWealth[density];
                        break;
                }

                foreach (Citizen.AgeGroup j in Enum.GetValues(typeof(Citizen.AgeGroup)))
                {
                    wealthNode.AppendChild(MakeWealthArray(xmlDoc, j.ToString(), array[(int)j]));
                }

                node.AppendChild(wealthNode);
            }

            root.AppendChild(node);
        }

        private XmlNode MakeWealthArray(XmlDocument xmlDoc, string age, int[] array)
        {
            XmlNode node = xmlDoc.CreateElement(age);

            XmlAttribute attribute = xmlDoc.CreateAttribute("car");
            attribute.Value = Convert.ToString(array[DataStore.Car]);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("bike");
            attribute.Value = Convert.ToString(array[DataStore.Bike]);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("taxi");
            attribute.Value = Convert.ToString(array[DataStore.Taxi]);
            node.Attributes.Append(attribute);

            return node;
        }

        private XmlNode MakeImmigrateNode(XmlDocument xmlDoc)
        {
            XmlNode rootNode = xmlDoc.CreateElement(migrateNodeName);

            XmlNode node = xmlDoc.CreateElement("single_adult");
            XmlAttribute attribute = xmlDoc.CreateAttribute("min");
            attribute.Value = Convert.ToString(DataStore.IncomingSingleAge[0]);
            node.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("max");
            attribute.Value = Convert.ToString(DataStore.IncomingSingleAge[1]);
            node.Attributes.Append(attribute);
            rootNode.AppendChild(node);

            node = xmlDoc.CreateElement("family_adult");
            attribute = xmlDoc.CreateAttribute("min");
            attribute.Value = Convert.ToString(DataStore.IncomingAdultAge[0]);
            node.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("max");
            attribute.Value = Convert.ToString(DataStore.IncomingAdultAge[1]);
            node.Attributes.Append(attribute);
            rootNode.AppendChild(node);

            return rootNode;
        }

        private XmlNode MakeTravelNode(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.CreateElement(travelNodeName);
            MakeDensityNodes(xmlDoc, node, 0);
            MakeDensityNodes(xmlDoc, node, 1);

            return node;
        }

        private XmlNode MakeLifeNode(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.CreateElement(lifeSpanNodeName);
            XmlAttribute attribute = xmlDoc.CreateAttribute("modifier");
            attribute.Value = Convert.ToString(DataStore.LifeSpanMultiplier);
            node.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("workspeed");
            attribute.Value = Convert.ToString(DataStore.WorkSpeedMultiplier);
            node.Attributes.Append(attribute);

            XmlComment comment = xmlDoc.CreateComment("Percentage of people who survive to the next 10% of their life");
            node.AppendChild(comment);
            node.AppendChild(MakeSurvivalNode(xmlDoc));

            comment = xmlDoc.CreateComment("Percentage of people who become sick over the next 10% of their life");
            node.AppendChild(comment);
            node.AppendChild(MakeSicknessNode(xmlDoc));
            /*
            comment = xmlDoc.CreateComment("Percentage of people who die while sick");
            node.AppendChild(comment);
            node.AppendChild(makeSickDeathNode(xmlDoc));
            */
            comment = xmlDoc.CreateComment("Percentage of dead who will instantly disappear");
            node.AppendChild(comment);
            node.AppendChild(MakeCheatHearseNode(xmlDoc));

            return node;
        }

        private XmlNode MakeSurvivalNode(XmlDocument xmlDoc)
        {
            XmlNode survNode = xmlDoc.CreateElement(survivalNodeName);

            // 0 to 9, 10 deciles.
            for (int i = 0; i < 10; ++i)
            {
                XmlNode node = xmlDoc.CreateElement("decile_" + (i + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("survival");
                attribute.Value = Convert.ToString(DataStore.SurvivalProbInXML[i] * 100.0);
                node.Attributes.Append(attribute);

                survNode.AppendChild(node);
            }

            return survNode;
        }

        private XmlNode MakeSicknessNode(XmlDocument xmlDoc)
        {
            XmlNode sickNode = xmlDoc.CreateElement(sicknessNodeName);

            // 0 to 9, 10 deciles.
            for (int i = 0; i < 10; ++i)
            {
                XmlNode node = xmlDoc.CreateElement("decile_" + (i + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("chance");
                attribute.Value = Convert.ToString(DataStore.SicknessProbInXML[i] * 100.0);
                node.Attributes.Append(attribute);

                sickNode.AppendChild(node);
            }

            return sickNode;
        }

        private XmlNode MakeCheatHearseNode(XmlDocument xmlDoc)
        {
            XmlNode cheatHearseNode = xmlDoc.CreateElement(cheatHearseNodeName);

            XmlAttribute attribute = xmlDoc.CreateAttribute("chance");
            attribute.Value = Convert.ToString(DataStore.AutoDeadRemovalChance);
            cheatHearseNode.Attributes.Append(attribute);

            return cheatHearseNode;
        }

        private void ReadTravelWealthNode(XmlNode wealthNode, int density)
        {
            foreach (XmlNode node in wealthNode.ChildNodes)
            {
                string name = node.Name;
                int[][][] array;

                switch (name)
                {
                    case "low_wealth":
                        array = DataStore.TransportLowWealth;
                        break;
                    case "med_wealth":
                        array = DataStore.TransportMedWealth;
                        break;
                    case "high_wealth":
                        array = DataStore.TransportHighWealth;
                        break;
                    case "low_wealth_eco":
                        array = DataStore.TranportLowWealthEco;
                        break;
                    case "med_wealth_eco":
                        array = DataStore.TransportMedWealthEco;
                        break;
                    case "high_wealth_eco":
                        array = DataStore.TransportHighWealthEco;
                        break;
                    case "low_wealth_w2w":
                        // Duplicate to handle lack of density.
                        array = new int[][][] { DataStore.TransportLowWealthW2W, DataStore.TransportLowWealthW2W };
                        break;
                    case "med_wealth_w2w":
                        // Duplicate to handle lack of density.
                        array = new int[][][] { DataStore.TransportMedWealthW2W, DataStore.TransportMedWealthW2W };
                        break;
                    case "high_wealth_w2w":
                        // Duplicate to handle lack of density.
                        array = new int[][][] { DataStore.TransportHighWealthW2W, DataStore.TransportHighWealthW2W };
                        break;
                    default:
                        Logging.Error("readWealthNode. unknown element name: ", name);
                        return;
                }

                // Read inner attributes
                ReadTravelAgeNode(node, array[density]);
            } // end foreach
        }

        private void ReadTravelAgeNode(XmlNode ageNode, int[][] arrayRef)
        {
            foreach (XmlNode node in ageNode.ChildNodes)
            {
                string name = node.Name;
                int index = 0;

                if (name.Equals(Citizen.AgeGroup.Child.ToString()))
                {
                    index = 0;
                }
                else if (name.Equals(Citizen.AgeGroup.Teen.ToString()))
                {
                    index = 1;
                }
                else if (name.Equals(Citizen.AgeGroup.Young.ToString()))
                {
                    index = 2;
                }
                else if (name.Equals(Citizen.AgeGroup.Adult.ToString()))
                {
                    index = 3;
                }
                else if (name.Equals(Citizen.AgeGroup.Senior.ToString()))
                {
                    index = 4;
                }

                // Read inner attributes
                try
                {
                    arrayRef[index][DataStore.Car] = Convert.ToInt32(node.Attributes["car"].InnerText);
                    arrayRef[index][DataStore.Bike] = Convert.ToInt32(node.Attributes["bike"].InnerText);
                    arrayRef[index][DataStore.Taxi] = Convert.ToInt32(node.Attributes["taxi"].InnerText);
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "readTravelAgeNode exception");
                }
            } // end foreach
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore SA1303 // Const field names should begin with upper-case letter