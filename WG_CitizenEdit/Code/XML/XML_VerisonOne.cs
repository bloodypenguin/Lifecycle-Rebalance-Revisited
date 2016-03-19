using System;
using System.IO;
using System.Xml;

namespace WG_CitizenEdit
{
    public class XML_VersionOne : WG_XMLBaseVersion
    {
        private const string travelNodeName = "travel";
        private const string immigrateNodeName = "immigrate";
        private const string lifeSpanNodeName = "lifespan";
        private const string deathNodeName = "death";

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
                else if (node.Name.Equals(immigrateNodeName))
                {
                    readImmigrateNode(node);
                }
                else if (node.Name.Equals(lifeSpanNodeName))
                {
                    readLifeNode(node);
                }
                else if (node.Name.Equals(deathNodeName))
                {
                    readDeathNode(node);
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
                readWealthNode(node, index);
            }
        }


        public void readImmigrateNode(XmlNode root)
        {

        }


        public void readLifeNode(XmlNode root)
        {
            try
            {
                DataStore.lifeSpanMultiplier = Convert.ToInt32(root.InnerText);
            }
            catch (Exception e)
            {
                Debugging.bufferWarning("lifespan was not an integer: " + e.Message + ". Setting to 4");
                DataStore.lifeSpanMultiplier = 4;
            }
        }


        public void readDeathNode(XmlNode root)
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
                        Debugging.bufferWarning(e.Message);
                    }
                }
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
            attribute.Value = "1";
            rootNode.Attributes.Append(attribute);
            xmlDoc.AppendChild(rootNode);

            rootNode.AppendChild(makeTravelNode(xmlDoc));
            rootNode.AppendChild(makeImmigrateNode(xmlDoc));
            rootNode.AppendChild(makeLifeNode(xmlDoc));
            rootNode.AppendChild(makeDeathNode(xmlDoc));

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
        private XmlNode makeLifeNode(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.CreateElement(lifeSpanNodeName);
            XmlNode lifeSpanNode = xmlDoc.CreateElement("lifeSpan_Multiplier");
            node.InnerXml = DataStore.lifeSpanMultiplier.ToString();
            lifeSpanNode.AppendChild(node);

            return node;
        }


        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlNode makeImmigrateNode(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.CreateElement(immigrateNodeName);

            return node;
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
        private XmlNode makeDeathNode(XmlDocument xmlDoc)
        {
            XmlNode deathNode = xmlDoc.CreateElement(deathNodeName);

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

                deathNode.AppendChild(node);
            }

            return deathNode;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wealthNode"></param>
        /// <param name="density"></param>
        private void readWealthNode(XmlNode wealthNode, int density)
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
                readAgeNode(node, array[density]);
            } // end foreach
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="ageNode"></param>
        /// <param name="arrayRef"></param>
        private void readAgeNode(XmlNode ageNode, int[][] arrayRef)
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
                    Debugging.bufferWarning("readAgeNode: " + e.Message);
                }  
            } // end foreach
        }
    }
}