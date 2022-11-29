// <copyright file="XML_VersionOne.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using System.IO;
    using System.Xml;
    using AlgernonCommons;

    public class XML_VersionOne : WG_XMLBaseVersion
    {
        private const string travelNodeName = "travel";

        private const string migrateNodeName = "migrate";

        private const string lifeSpanNodeName = "lifespan";
        private const string survivalNodeName = "survival";
        private const string sicknessNodeName = "sickness";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// 
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
                    array = DataStore.incomingSingleAge;
                }
                else if (node.Name.Equals("family_adult"))
                {
                    array = DataStore.incomingAdultAge;
                }
                else
                {
                    Logging.Message("Unknown immigration node");
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
                DataStore.lifeSpanMultiplier = Convert.ToInt32(root.Attributes["modifier"].InnerText);
            }
            catch (Exception e)
            {
                Logging.Error("lifespan multiplier was not an integer: ", e.Message, ". Setting to 3");
                DataStore.lifeSpanMultiplier = 3;
            }

            if (DataStore.lifeSpanMultiplier <= 0)
            {
                Logging.Error("Detecting a lifeSpan multiplier less than or equal to 0 . Setting to 3");
                DataStore.lifeSpanMultiplier = 3;
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
            }
        }


        public void ReadSurvivalNode(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name.Equals("decile"))
                {
                    try
                    {
                        int index = Convert.ToInt32(node.Attributes["num"].InnerText) - 1;
                        DataStore.survivalProbInXML[index] = Convert.ToDouble(node.Attributes["survival"].InnerText) / 100.0;
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "exception deserializing SurvivalNode");
                    }
                }
            }
        }


        public void ReadSicknessNode(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name.Equals("decile"))
                {
                    try
                    {
                        int index = Convert.ToInt32(node.Attributes["num"].InnerText) - 1;
                        DataStore.sicknessProbInXML[index] = Convert.ToDouble(node.Attributes["chance"].InnerText) / 100.0;
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "exception deserializing SicknessNode");
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPathFileName"></param>
        /// <returns></returns>
        public override bool WriteXML(string fullPathFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("WG_CitizenEdit");
            XmlAttribute attribute = xmlDoc.CreateAttribute("version");
            attribute.Value = "1";
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="array"></param>
        /// <param name="rootPopNode"></param>
        /// <param name="consumNode"></param>
        /// <param name="pollutionNode"></param>
        private void MakeDensityNodes(XmlDocument xmlDoc, XmlNode root, int density)
        {
            string densityElementName = (density == 0) ? "low_density" : "high_density";
            XmlNode node = xmlDoc.CreateElement(densityElementName);
            string[] type = { "low_wealth", "med_wealth", "high_wealth" };

            for (int i = 0; i < type.Length; i++)
            {
                XmlNode wealthNode = xmlDoc.CreateElement(type[i]);
                int[][] array;
                switch (i)
                {
                    case 1:
                        array = DataStore.wealth_med[density];
                        break;
                    case 2:
                        array = DataStore.wealth_high[density];
                        break;
                    case 0:
                    default:
                        array = DataStore.wealth_low[density];
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private XmlNode MakeWealthArray(XmlDocument xmlDoc, String age, int[] array)
        {
            XmlNode node = xmlDoc.CreateElement(age);

            XmlAttribute attribute = xmlDoc.CreateAttribute("car");
            attribute.Value = Convert.ToString(array[DataStore.CAR]);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("bike");
            attribute.Value = Convert.ToString(array[DataStore.BIKE]);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("taxi");
            attribute.Value = Convert.ToString(array[DataStore.TAXI]);
            node.Attributes.Append(attribute);

            return node;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode MakeImmigrateNode(XmlDocument xmlDoc)
        {
            XmlNode rootNode = xmlDoc.CreateElement(migrateNodeName);

            XmlNode node = xmlDoc.CreateElement("single_adult");
            XmlAttribute attribute = xmlDoc.CreateAttribute("min");
            attribute.Value = Convert.ToString(DataStore.incomingSingleAge[0]);
            node.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("max");
            attribute.Value = Convert.ToString(DataStore.incomingSingleAge[1]);
            node.Attributes.Append(attribute);
            rootNode.AppendChild(node);

            node = xmlDoc.CreateElement("family_adult");
            attribute = xmlDoc.CreateAttribute("min");
            attribute.Value = Convert.ToString(DataStore.incomingAdultAge[0]);
            node.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("max");
            attribute.Value = Convert.ToString(DataStore.incomingAdultAge[1]);
            node.Attributes.Append(attribute);
            rootNode.AppendChild(node);

            return rootNode;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode MakeTravelNode(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.CreateElement(travelNodeName);
            MakeDensityNodes(xmlDoc, node, 0);
            MakeDensityNodes(xmlDoc, node, 1);

            return node;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode MakeLifeNode(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.CreateElement(lifeSpanNodeName);
            XmlAttribute attribute = xmlDoc.CreateAttribute("modifier");
            attribute.Value = Convert.ToString(DataStore.lifeSpanMultiplier);
            node.Attributes.Append(attribute);

            xmlDoc.CreateComment("Percentage of people who survive to the next 10% of their life");
            node.AppendChild(MakeSurvivalNode(xmlDoc));
            xmlDoc.CreateComment("Percentage of people who become sick over the next 10% of their life");
            node.AppendChild(MakeSicknessNode(xmlDoc));

            return node;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode MakeSurvivalNode(XmlDocument xmlDoc)
        {
            XmlNode survNode = xmlDoc.CreateElement(survivalNodeName);

            // 0 to 9, 10 deciles.
            for (int i = 0; i < 10; ++i)
            {
                XmlNode node = xmlDoc.CreateElement("decile");
                XmlAttribute attribute = xmlDoc.CreateAttribute("num");
                attribute.Value = Convert.ToString(i + 1);
                node.Attributes.Append(attribute);

                attribute = xmlDoc.CreateAttribute("survival");
                attribute.Value = Convert.ToString(DataStore.survivalProbInXML[i] * 100.0);
                node.Attributes.Append(attribute);

                survNode.AppendChild(node);
            }

            return survNode;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode MakeSicknessNode(XmlDocument xmlDoc)
        {
            XmlNode sickNode = xmlDoc.CreateElement(sicknessNodeName);

            // 0 to 9, 10 deciles.
            for (int i = 0; i < 10; ++i)
            {
                XmlNode node = xmlDoc.CreateElement("decile");
                XmlAttribute attribute = xmlDoc.CreateAttribute("num");
                attribute.Value = Convert.ToString(i + 1);
                node.Attributes.Append(attribute);

                attribute = xmlDoc.CreateAttribute("chance");
                attribute.Value = Convert.ToString(DataStore.sicknessProbInXML[i] * 100.0);
                node.Attributes.Append(attribute);

                sickNode.AppendChild(node);
            }

            return sickNode;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wealthNode"></param>
        /// <param name="density"></param>
        private void ReadTravelWealthNode(XmlNode wealthNode, int density)
        {
            foreach (XmlNode node in wealthNode.ChildNodes)
            {
                string name = node.Name;
                int[][][] array;

                switch (name)
                {
                    case "low_wealth":
                        array = DataStore.wealth_low;
                        break;
                    case "med_wealth":
                        array = DataStore.wealth_med;
                        break;
                    case "high_wealth":
                        array = DataStore.wealth_high;
                        break;
                    default:
                        Logging.Error("readWealthNode. unknown element name: " + name);
                        return;
                }

                // Read inner attributes
                ReadTravelAgeNode(node, array[density]);
            } // end foreach
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="ageNode"></param>
        /// <param name="arrayRef"></param>
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
                    arrayRef[index][DataStore.CAR] = Convert.ToInt32(node.Attributes["car"].InnerText);
                    arrayRef[index][DataStore.BIKE] = Convert.ToInt32(node.Attributes["bike"].InnerText);
                    arrayRef[index][DataStore.TAXI] =  Convert.ToInt32(node.Attributes["taxi"].InnerText);
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "readAgeNode exception");
                }  
            } // end foreach
        }
    }
}