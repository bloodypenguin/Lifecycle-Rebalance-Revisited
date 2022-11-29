// <copyright file="SpeedOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting mod lifecycle speed options.
    /// </summary>
    internal class SpeedOptions : OptionsPanelTab
    {
        /// <summary>
        /// Adds speed options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal SpeedOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("LBR_SPD"), tabIndex, out _);

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
                Logging.Message("setting up ", this.GetType());

                // Lifespan multiplier.  Simple integer.
                UISlider lifeMult = UISliders.AddPlainSliderWithValue(
                    panel,
                    0f,
                    0f,
                    Translations.Translate("LBR_SPD_FAC") + Environment.NewLine + Translations.Translate("LBR_SPD_FN1") + Environment.NewLine + Environment.NewLine + Translations.Translate("LBR_SPD_FN2") +  Environment.NewLine + Translations.Translate("LBR_SPD_FN3"),
                    1f,
                    340f,
                    1f,
                    DataStore.lifeSpanMultiplier,
                    680f);

                // Reset to saved button.
                UIButton lifeMultReset = UIButtons.AddButton(panel, UILayout.PositionUnder(lifeMult), Translations.Translate("LBR_RTS"));
                lifeMultReset.eventClicked += (c, p) =>
                {
                    // Retrieve saved value from datastore - inverted value (see above).
                    lifeMult.value = DataStore.lifeSpanMultiplier;
                };


                // Save settings button.
                UIButton lifeMultSave = UIButtons.AddButton(panel, UILayout.PositionUnder(lifeMultReset), Translations.Translate("LBR_SAA"));
                lifeMultSave.eventClicked += (c, p) =>
                {
                    // Update mod settings - inverted value (see above).
                    DataStore.lifeSpanMultiplier = (int)lifeMult.value;
                    Logging.Message("lifespan multiplier set to: ", DataStore.lifeSpanMultiplier);

                    // Update WG configuration file.
                    PanelUtils.SaveXML();
                };
            }
        }
    }
}