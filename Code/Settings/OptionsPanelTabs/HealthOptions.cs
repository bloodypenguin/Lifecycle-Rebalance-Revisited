using System;
using System.Text;
using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting health options.
    /// </summary>
    internal class HealthOptions : OptionsPanelTab
    {
        // Sickness deciles; set to 10 (even though 11 in DataStore) as current WG XML v2 only stores the first 10.
        private const int numDeciles = 10;
        private static readonly float[] defaultSicknessProbs = { 0.0125f, 0.0075f, 0.01f, 0.01f, 0.015f, 0.02f, 0.03f, 0.04f, 0.05f, 0.075f, 0.25f };


        /// <summary>
        /// Adds health options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal HealthOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            panel = PanelUtils.AddTab(tabStrip, Translations.Translate("LBR_HEA"), tabIndex);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }


        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!isSetup)
            {
                // Perform initial setup.
                isSetup = true;
                Logging.Message("setting up ", this.GetType().ToString());


                // Illness options.
                UILabel illnessLabel = PanelUtils.AddLabel(panel, Translations.Translate("LBR_HEA_ILL") + Environment.NewLine + Translations.Translate("LBR_HEA_ILD"));
                illnessLabel.relativePosition = Vector3.zero;

                // Set the intial Y position of the illness chance sliders.
                float currentY = illnessLabel.height + 10f;

                // Illness chance sliders.
                UISlider[] illnessChance = new UISlider[DataStore.sicknessProbInXML.Length];
                for (int i = 0; i < numDeciles; ++i)
                {
                    // Note this is using Sunset Harbor ages.  Legacy ages are shorter by around 40% (25/35).
                    illnessChance[i] = PanelUtils.AddSliderWithValue(panel, Translations.Translate("LBR_HEA_AGE") + " " + (i * 10) + "-" + ((i * 10) + 9) + " (" + Translations.Translate("LBR_DEF") + " " + (defaultSicknessProbs[i] * 100) + ")", 0, 25, 0.05f, (float)DataStore.sicknessProbInXML[i] * 100, (value) => { }, textScale: 0.9f);
                    illnessChance[i].parent.relativePosition = new Vector3(0, currentY);
                    currentY += illnessChance[i].parent.height - 3f;
                }

                // Add vertical gap for buttons.
                currentY += 5;

                // Reset to saved button.
                UIButton illnessResetSaved = PanelUtils.CreateButton(panel, Translations.Translate("LBR_RTS"));
                illnessResetSaved.relativePosition = new Vector3(0f, currentY);
                illnessResetSaved.eventClicked += (control, clickEvent) =>
                {
                    for (int i = 0; i < numDeciles; ++i)
                    {
                    // Retrieve saved values from datastore.
                    illnessChance[i].value = (float)DataStore.sicknessProbInXML[i] * 100;
                    }
                };

                // Save settings button.
                UIButton illnessSave = PanelUtils.CreateButton(panel, Translations.Translate("LBR_SAA"));
                illnessSave.relativePosition = PanelUtils.PositionUnder(illnessResetSaved);
                illnessSave.eventClicked += (control, clickEvent) =>
                {
                    StringBuilder logMessage = new StringBuilder("Lifecycle Rebalance Revisited: sickness probability table using factor of " + ModSettings.decadeFactor + ":" + Environment.NewLine);

                // Update datastore with slider values.
                for (int i = 0; i < numDeciles; ++i)
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
                UIButton illnessResetDefault = PanelUtils.CreateButton(panel, Translations.Translate("LBR_RTD"));
                illnessResetDefault.relativePosition = PanelUtils.PositionRightOf(illnessResetSaved);
                illnessResetDefault.eventClicked += (control, clickEvent) =>
                {
                    for (int i = 0; i < numDeciles; ++i)
                    {
                    // Retrieve default values.
                    illnessChance[i].value = defaultSicknessProbs[i] * 100;
                    }
                };

                // Set to zero button.
                UIButton illnessSetZero = PanelUtils.CreateButton(panel, Translations.Translate("LBR_ZRO"));
                illnessSetZero.relativePosition = PanelUtils.PositionRightOf(illnessResetDefault);
                illnessSetZero.eventClicked += (control, clickEvent) =>
                {
                    for (int i = 0; i < numDeciles; ++i)
                    {
                    // Reset everything to zero.
                    illnessChance[i].value = 0;
                    }
                };
            }
        }
    }
}