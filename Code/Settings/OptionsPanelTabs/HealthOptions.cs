// <copyright file="HealthOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using System.Text;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting health options.
    /// </summary>
    internal sealed class HealthOptions : OptionsPanelTab
    {
        // Sickness deciles; set to 10 (even though 11 in DataStore) as current WG XML v2 only stores the first 10.
        private const int NumDeciles = 10;
        private static readonly float[] DefaultSicknessProbs = { 0.0125f, 0.0075f, 0.01f, 0.01f, 0.015f, 0.02f, 0.03f, 0.04f, 0.05f, 0.075f, 0.25f };

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthOptions"/> class.
        /// Adds health options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal HealthOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            Panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("LBR_HEA"), tabIndex, out _);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!IsSetup)
            {
                // Perform initial setup.
                IsSetup = true;
                Logging.Message("setting up ", this.GetType());

                // Illness options.
                UILabel illnessLabel = UILabels.AddLabel(Panel, 0f, 0f, Translations.Translate("LBR_HEA_ILL") + Environment.NewLine + Translations.Translate("LBR_HEA_ILD"));

                // Set the intial Y position of the illness chance sliders.
                float currentY = illnessLabel.height + 10f;

                // Illness chance sliders.
                UISlider[] illnessChance = new UISlider[DataStore.SicknessProbInXML.Length];
                for (int i = 0; i < NumDeciles; ++i)
                {
                    // Note this is using Sunset Harbor ages.  Legacy ages are shorter by around 40% (25/35).
                    illnessChance[i] = UISliders.AddPlainSliderWithValue(
                        Panel,
                        0f,
                        currentY,
                        Translations.Translate("LBR_HEA_AGE") + " " + (i * 10) + "-" + ((i * 10) + 9) + " (" + Translations.Translate("LBR_DEF") + " " + (DefaultSicknessProbs[i] * 100) + ")",
                        0f,
                        25f,
                        0.05f,
                        (float)DataStore.SicknessProbInXML[i] * 100);
                    currentY += illnessChance[i].parent.height - 3f;
                }

                // Add vertical gap for buttons.
                currentY += 5;

                // Reset to saved button.
                UIButton illnessResetSaved = UIButtons.AddButton(Panel, 0f, currentY, Translations.Translate("LBR_RTS"));
                illnessResetSaved.eventClicked += (c, p) =>
                {
                    for (int i = 0; i < NumDeciles; ++i)
                    {
                        // Retrieve saved values from datastore.
                        illnessChance[i].value = (float)DataStore.SicknessProbInXML[i] * 100;
                    }
                };

                // Save settings button.
                UIButton illnessSave = UIButtons.AddButton(Panel, UILayout.PositionUnder(illnessResetSaved), Translations.Translate("LBR_SAA"));
                illnessSave.eventClicked += (c, p) =>
                {
                    StringBuilder logMessage = new StringBuilder("Lifecycle Rebalance Revisited: sickness probability table using factor of " + ModSettings.Settings.DecadeFactor + ":" + Environment.NewLine);

                    // Update datastore with slider values.
                    for (int i = 0; i < NumDeciles; ++i)
                    {
                        DataStore.SicknessProbInXML[i] = illnessChance[i].value / 100;
                    }

                    // Recalculate probabilities.
                    DataStore.CalculateSicknessProbabilities();

                    // Write to file.
                    DataStore.SaveXML();
                };

                // Reset to default button.
                UIButton illnessResetDefault = UIButtons.AddButton(Panel, UILayout.PositionRightOf(illnessResetSaved), Translations.Translate("LBR_RTD"));
                illnessResetDefault.eventClicked += (c, p) =>
                {
                    for (int i = 0; i < NumDeciles; ++i)
                    {
                        // Retrieve default values.
                        illnessChance[i].value = DefaultSicknessProbs[i] * 100;
                    }
                };

                // Set to zero button.
                UIButton illnessSetZero = UIButtons.AddButton(Panel, UILayout.PositionRightOf(illnessResetDefault), Translations.Translate("LBR_ZRO"));
                illnessSetZero.eventClicked += (c, p) =>
                {
                    for (int i = 0; i < NumDeciles; ++i)
                    {
                        // Reset everything to zero.
                        illnessChance[i].value = 0;
                    }
                };
            }
        }
    }
}