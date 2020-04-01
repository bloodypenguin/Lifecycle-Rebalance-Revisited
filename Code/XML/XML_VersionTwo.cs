using System;
using System.IO;
using System.Xml;

namespace WG_CitizenEdit
{
    public class XML_VersionTwo : WG_XMLBaseVersion
    {
        private const string travelNodeName = "travel";

        private const string migrateNodeName = "migrate";
        private const string emigrateNodeName = "emigrate";

        private const string lifeSpanNodeName = "lifespan";
        private const string familyName = "family";
        private const string survivalNodeName = "survival";
        private const string sicknessNodeName = "sickness";
        private const string sickDieNodeName = "sickDeathRate";
        private const string cheatHearseNodeName = "cheatHearse";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// 
        public override void readXML(XmlDocument doc)
        {
            XmlElement root = doc.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name.Equals(travelNodeName))
                {
                    readTravelNode(node);
                }
                else if (node.Name.Equals(migrateNodeName))
                {
                    readImmigrateNode(node);
                }
                else if (node.Name.Equals(emigrateNodeName))
                {
                    readImmigrateNode(node);
                }
                else if (node.Name.Equals(lifeSpanNodeName))
                {
                    readLifeNode(node);
                }
            }
        }


        public void readTravelNode(XmlNode root)
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
                readTravelWealthNode(node, index);
            }
        }


        public void readImmigrateNode(XmlNode root)
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
                    Debugging.bufferWarning("Unknown immigration node");
                }

                try
                {
                    array[0] = Convert.ToInt32(node.Attributes["min"].InnerText);
                    array[1] = Convert.ToInt32(node.Attributes["max"].InnerText);
                }
                catch (Exception e)
                {
                    Debugging.bufferWarning("readImmigrateNode: " + e.Message);
                }
            }
        }


        public void readLifeNode(XmlNode root)
        {
            try
            {
                DataStore.lifeSpanMultiplier = Convert.ToInt32(root.Attributes["modifier"].InnerText);
            }
            catch (Exception e)
            {
                Debugging.bufferWarning("lifespan multiplier was not an integer: " + e.Message + ". Setting to 3");
                DataStore.lifeSpanMultiplier = 3;
            }
/*
            try
            {
                DataStore.workSpeedMultiplier = Convert.ToInt32(root.Attributes["workspeed"].InnerText);
            }
            catch (Exception e)
            {
                Debugging.bufferWarning("workspeed multiplier was not an integer: " + e.Message + ". Setting to 3");
                DataStore.workSpeedMultiplier = 3;
            }
*/

            if (DataStore.lifeSpanMultiplier <= 0)
            {
                Debugging.bufferWarning("Detecting a lifeSpan multiplier less than or equal to 0 . Setting to 3");
                DataStore.lifeSpanMultiplier = 3;
            }
/*
            if (DataStore.workSpeedMultiplier <= 0)
            {
                Debugging.bufferWarning("Detecting a work speed multiplier less than or equal to 0 . Setting to 3");
                DataStore.workSpeedMultiplier = 3;
            }

            DataStore.workNumberCheck = 15 / DataStore.workSpeedMultiplier;
*/
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name.Equals(survivalNodeName))
                {
                    readSurvivalNode(node);
                }
                else if (node.Name.Equals(sicknessNodeName))
                {
                    readSicknessNode(node);
                }
                else if (node.Name.Equals(sickDieNodeName))
                {
                    readHospitalNode(node);
                }
                else if (node.Name.Equals(cheatHearseNodeName))
                {
                    readCheatHearseNode(node);
                }
            }
        }


        public void readSurvivalNode(XmlNode root)
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
                        DataStore.survivalProbInXML[level] = Convert.ToDouble(node.Attributes["survival"].InnerText) / 100.0;
                    }
                    catch (Exception e)
                    {
                        Debugging.bufferWarning(e.Message);
                    }
                }
            }
        }


        public void readSicknessNode(XmlNode root)
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
                        DataStore.sicknessProbInXML[level] = Convert.ToDouble(node.Attributes["chance"].InnerText) / 100.0;
                    }
                    catch (Exception e)
                    {
                        Debugging.bufferWarning(e.Message);
                    }
                }
            }
        }


        public void readHospitalNode(XmlNode root)
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
                        DataStore.sickDeathChance[level] = Convert.ToDouble(node.Attributes["chance"].InnerText) / 100.0;
                    }
                    catch (Exception e)
                    {
                        Debugging.bufferWarning(e.Message);
                    }
                }
            }
        }



        public void readEmigrationNode(XmlNode root)
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
                        DataStore.emigrateChance[level] = Convert.ToDouble(node.Attributes["chance"].InnerText) / 100.0;
                    }
                    catch (Exception e)
                    {
                        Debugging.bufferWarning(e.Message);
                    }
                }
            }
        }


        public void readCheatHearseNode(XmlNode root)
        {
            try
            {
                DataStore.autoDeadRemovalChance = (int) Convert.ToDouble(root.Attributes["chance"].InnerText);
                if ((DataStore.autoDeadRemovalChance < 0) && (DataStore.autoDeadRemovalChance > 100))
                {
                    Debugging.bufferWarning("Cheat hearse is out of range (0-100). Setting to 50");
                    DataStore.autoDeadRemovalChance = 50;
                }
            }
            catch (Exception e)
            {
                Debugging.bufferWarning(e.Message);
                DataStore.autoDeadRemovalChance = 50;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPathFileName"></param>
        /// <returns></returns>
        public override bool writeXML(string fullPathFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("WG_CitizenEdit");
            XmlAttribute attribute = xmlDoc.CreateAttribute("version");
            attribute.Value = "2";
            rootNode.Attributes.Append(attribute);
            xmlDoc.AppendChild(rootNode);

            rootNode.AppendChild(makeTravelNode(xmlDoc));
            XmlComment comment = xmlDoc.CreateComment("Lower age value: Young adult (45), Adult (90), Senior (180)");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("Numbers lower than young adult may cause economic havoc.");
            rootNode.AppendChild(comment);
            rootNode.AppendChild(makeImmigrateNode(xmlDoc));
            //rootNode.AppendChild(makeEmigrationNode(xmlDoc));
            rootNode.AppendChild(makeLifeNode(xmlDoc));

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
                    UnityEngine.Debug.LogException(e);
                }
            }

            try
            {
                xmlDoc.Save(fullPathFileName);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
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
        private void makeDensityNodes(XmlDocument xmlDoc, XmlNode root, int density)
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
                    wealthNode.AppendChild(makeWealthArray(xmlDoc, j.ToString(), array[(int)j]));
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
        private XmlNode makeWealthArray(XmlDocument xmlDoc, String age, int[] array)
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
        private XmlNode makeImmigrateNode(XmlDocument xmlDoc)
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
        private XmlNode makeTravelNode(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.CreateElement(travelNodeName);
            makeDensityNodes(xmlDoc, node, 0);
            makeDensityNodes(xmlDoc, node, 1);

            return node;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode makeLifeNode(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.CreateElement(lifeSpanNodeName);
            XmlAttribute attribute = xmlDoc.CreateAttribute("modifier");
            attribute.Value = Convert.ToString(DataStore.lifeSpanMultiplier);
            node.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("workspeed");
            attribute.Value = Convert.ToString(DataStore.workSpeedMultiplier); 
            node.Attributes.Append(attribute);

            XmlComment comment = xmlDoc.CreateComment("Percentage of people who survive to the next 10% of their life");
            node.AppendChild(comment);
            node.AppendChild(makeSurvivalNode(xmlDoc));

            comment = xmlDoc.CreateComment("Percentage of people who become sick over the next 10% of their life");
            node.AppendChild(comment);
            node.AppendChild(makeSicknessNode(xmlDoc));
            /*
            comment = xmlDoc.CreateComment("Percentage of people who die while sick");
            node.AppendChild(comment);
            node.AppendChild(makeSickDeathNode(xmlDoc));
            */
            comment = xmlDoc.CreateComment("Percentage of dead who will instantly disappear");
            node.AppendChild(comment);
            node.AppendChild(makeCheatHearseNode(xmlDoc));

            return node;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode makeSurvivalNode(XmlDocument xmlDoc)
        {
            XmlNode survNode = xmlDoc.CreateElement(survivalNodeName);

            // 0 to 9, 10 deciles.
            for (int i = 0; i < 10; ++i)
            {
                XmlNode node = xmlDoc.CreateElement("decile_" + (i + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("survival");
                attribute.Value = Convert.ToString(DataStore.survivalProbInXML[i] * 100.0);
                node.Attributes.Append(attribute);

                survNode.AppendChild(node);
            }

            return survNode;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode makeSicknessNode(XmlDocument xmlDoc)
        {
            XmlNode sickNode = xmlDoc.CreateElement(sicknessNodeName);

            // 0 to 9, 10 deciles.
            for (int i = 0; i < 10; ++i)
            {
                XmlNode node = xmlDoc.CreateElement("decile_" + (i + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("chance");
                attribute.Value = Convert.ToString(DataStore.sicknessProbInXML[i] * 100.0);
                node.Attributes.Append(attribute);

                sickNode.AppendChild(node);
            }

            return sickNode;
        }

        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode makeSickDeathNode(XmlDocument xmlDoc)
        {
            XmlNode hospitalNode = xmlDoc.CreateElement(sickDieNodeName);

            // 0 to 9, 10 deciles.
            for (int i = 0; i < 10; ++i)
            {
                XmlNode node = xmlDoc.CreateElement("decile_" + (i + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("chance");
                attribute.Value = Convert.ToString(DataStore.sickDeathChance[i] * 100.0);
                node.Attributes.Append(attribute);

                hospitalNode.AppendChild(node);
            }

            return hospitalNode;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode makeEmigrationNode(XmlDocument xmlDoc)
        {
            XmlNode emigrationNode = xmlDoc.CreateElement(emigrateNodeName);

            // 0 to 9, 10 deciles.
            for (int i = 0; i < 10; ++i)
            {
                XmlNode node = xmlDoc.CreateElement("decile_" + (i + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("chance");
                attribute.Value = Convert.ToString(DataStore.emigrateChance[i] * 100.0);
                node.Attributes.Append(attribute);

                emigrationNode.AppendChild(node);
            }

            return emigrationNode;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode makeCheatHearseNode(XmlDocument xmlDoc)
        {
            XmlNode cheatHearseNode = xmlDoc.CreateElement(cheatHearseNodeName);

            XmlAttribute attribute = xmlDoc.CreateAttribute("chance");
            attribute.Value = Convert.ToString(DataStore.autoDeadRemovalChance);
            cheatHearseNode.Attributes.Append(attribute);

            return cheatHearseNode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wealthNode"></param>
        /// <param name="density"></param>
        private void readTravelWealthNode(XmlNode wealthNode, int density)
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
                        Debugging.bufferWarning("readWealthNode. unknown element name: " + name);
                        return;
                }

                // Read inner attributes
                readTravelAgeNode(node, array[density]);
            } // end foreach
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="ageNode"></param>
        /// <param name="arrayRef"></param>
        private void readTravelAgeNode(XmlNode ageNode, int[][] arrayRef)
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
                    arrayRef[index][DataStore.TAXI] = Convert.ToInt32(node.Attributes["taxi"].InnerText);
                }
                catch (Exception e)
                {
                    Debugging.bufferWarning("readAgeNode: " + e.Message);
                }
            } // end foreach
        }
    }
}