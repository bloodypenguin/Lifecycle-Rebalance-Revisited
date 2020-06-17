using System.Text;
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
            UIHelper healthTab = PanelUtils.AddTab(tabStrip, "Health", tabIndex);

            // Illness options.
            PanelUtils.AddLabel((UIPanel)healthTab.self, "Random illness % chance per decade of life\r\nDoes not affect sickness from specific causes, e.g. pollution or noise.");

            // Illness chance sliders.
            UISlider[] illnessChance = new UISlider[DataStore.sicknessProbInXML.Length];
            for (int i = 0; i < numDeciles; ++ i)
            {
                // Note this is using Sunset Harbor ages.  Legacy ages are shorter by around 40% (25/35).
                illnessChance[i] = PanelUtils.AddSliderWithValue((UIPanel)healthTab.self, "Ages " + (i * 10) + "-" + ((i * 10) + 9) + " (default " + (defaultSicknessProbs[i] * 100) + ")", 0, 25, 0.05f, (float)DataStore.sicknessProbInXML[i] * 100, (value) => { });
            }

            // Reset to saved button.
            UIButton illnessResetSaved = (UIButton)healthTab.AddButton("Reset to saved", () =>
            {
                for (int i = 0; i < numDeciles; ++ i)
                {
                    // Retrieve saved values from datastore.
                    illnessChance[i].value = (float)DataStore.sicknessProbInXML[i] * 100;
                }
            });

            // Save settings button.
            UIButton illnessSave = (UIButton)healthTab.AddButton("Save and apply", () =>
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
            });

            // Turn off autolayout to fit next buttons to the right of the illness saved button (above the save settings button).
            ((UIPanel)illnessResetSaved.parent).autoLayout = false;

            // Reset to default button.
            UIButton illnessResetDefault = (UIButton)healthTab.AddButton("Reset to default", () =>
            {
                for (int i = 0; i < numDeciles; ++ i)
                {
                    // Retrieve default values.
                    illnessChance[i].value = defaultSicknessProbs[i] * 100;
                }
            });
            illnessResetDefault.relativePosition = PanelUtils.PositionRightOf(illnessResetSaved);

            // Set to zero button.
            UIButton illnessSetZero = (UIButton)healthTab.AddButton("Set all to zero", () =>
            {
                for (int i = 0; i < numDeciles; ++ i)
                {
                    // Reset everything to zero.
                    illnessChance[i].value = 0;
                }
            });
            illnessSetZero.relativePosition = PanelUtils.PositionRightOf(illnessResetDefault);
        }
    }
}