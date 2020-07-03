using System.Text;
using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting health options.
    /// </summary>
    public class HealthOptions
    {
        // Sickness deciles; set to 10 (even though 11 in DataStore) as current WG XML v2 only stores the first 10.
        private const int numDeciles = 10;
        private static float[] defaultSicknessProbs = { 0.0125f, 0.0075f, 0.01f, 0.01f, 0.015f, 0.02f, 0.03f, 0.04f, 0.05f, 0.075f, 0.25f };


        /// <summary>
        /// Adds health options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public HealthOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel healthTab = PanelUtils.AddTab(tabStrip, "Health", tabIndex);

            // Illness options.
            UILabel illnessLabel = PanelUtils.AddLabel(healthTab, "Random illness % chance per decade of life\r\nDoes not affect sickness from specific causes, e.g. pollution or noise.");
            illnessLabel.relativePosition = Vector3.zero;

            // Set the intial Y position of the illness chance sliders.
            float currentY = illnessLabel.height + 10f;

            // Illness chance sliders.
            UISlider[] illnessChance = new UISlider[DataStore.sicknessProbInXML.Length];
            for (int i = 0; i < numDeciles; ++ i)
            {
                // Note this is using Sunset Harbor ages.  Legacy ages are shorter by around 40% (25/35).
                illnessChance[i] = PanelUtils.AddSliderWithValue(healthTab, "Ages " + (i * 10) + "-" + ((i * 10) + 9) + " (default " + (defaultSicknessProbs[i] * 100) + ")", 0, 25, 0.05f, (float)DataStore.sicknessProbInXML[i] * 100, (value) => { }, textScale: 0.9f);
                illnessChance[i].parent.relativePosition = new Vector3(0, currentY);
                currentY += illnessChance[i].parent.height;
            }

            // Add vertical gap for buttons.
            currentY += 5;

            // Reset to saved button.
            UIButton illnessResetSaved = PanelUtils.CreateButton(healthTab, "Reset to saved");
            illnessResetSaved.relativePosition = new Vector3(0f, currentY);
            illnessResetSaved.eventClicked += (control, clickEvent) =>
            {
                for (int i = 0; i < numDeciles; ++ i)
                {
                    // Retrieve saved values from datastore.
                    illnessChance[i].value = (float)DataStore.sicknessProbInXML[i] * 100;
                }
            };
            
            // Save settings button.
            UIButton illnessSave = PanelUtils.CreateButton(healthTab, "Save and apply");
            illnessSave.relativePosition = PanelUtils.PositionUnder(illnessResetSaved);
            illnessSave.eventClicked += (control, clickEvent) =>
            {
                StringBuilder logMessage = new StringBuilder("Lifecycle Rebalance Revisited: sickness probability table using factor of " + ModSettings.decadeFactor + ":\r\n");

                // Update datastore with slider values.
                for (int i = 0; i < numDeciles; ++ i)
                {
                    DataStore.sicknessProbInXML[i] = illnessChance[i].value / 100;

                    // Recalculate probabilities if the mod is loaded.
                    if (Loading.isModCreated)
                    {
                        Loading.CalculateSicknessProbabilities();
                    }
                }

                // Write to file.
                PanelUtils.SaveXML();
            };

            // Reset to default button.
            UIButton illnessResetDefault = PanelUtils.CreateButton(healthTab, "Reset to default");
            illnessResetDefault.relativePosition = PanelUtils.PositionRightOf(illnessResetSaved);
            illnessResetDefault.eventClicked += (control, clickEvent) =>
            {
                for (int i = 0; i < numDeciles; ++ i)
                {
                    // Retrieve default values.
                    illnessChance[i].value = defaultSicknessProbs[i] * 100;
                }
            };

            // Set to zero button.
            UIButton illnessSetZero = PanelUtils.CreateButton(healthTab, "Set all to zero");
            illnessSetZero.relativePosition = PanelUtils.PositionRightOf(illnessResetDefault);
            illnessSetZero.eventClicked += (control, clickEvent) =>
            {
                for (int i = 0; i < numDeciles; ++ i)
                {
                    // Reset everything to zero.
                    illnessChance[i].value = 0;
                }
            };
        }
    }
}