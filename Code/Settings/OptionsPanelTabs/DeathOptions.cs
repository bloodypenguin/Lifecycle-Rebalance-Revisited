// <copyright file="DeathOptions.cs" company="algernon (K. Algernon A. Sheppard)">
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
    using UnityEngine;

    /// <summary>
    /// Options panel for setting death options.
    /// </summary>
    internal sealed class DeathOptions : OptionsPanelTab
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeathOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal DeathOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            Panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("LBR_DTH"), tabIndex, out _, autoLayout: true);

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

                // Percentage of corpses requiring transport.  % of bodies requiring transport is more intuitive to user than % of vanishing corpses, so we invert the value.
                UISlider vanishingStiffs = UISliders.AddPlainSliderWithValue(Panel, 0f, 0f, Translations.Translate("LBR_DTH_TRN") + Environment.NewLine + Translations.Translate("LBR_DTH_TRD"), 0, 100, 1, 100 - DataStore.AutoDeadRemovalChance);

                // Reset to saved button.
                UIButton vanishingStiffReset = UIButtons.AddButton(Panel, 0f, 0f, Translations.Translate("LBR_RTS"));
                vanishingStiffReset.eventClicked += (c, p) =>
                {
                    // Retrieve saved value from datastore - inverted value (see above).
                    vanishingStiffs.value = 100 - DataStore.AutoDeadRemovalChance;
                };

                vanishingStiffReset.relativePosition = new Vector3(vanishingStiffReset.relativePosition.x, vanishingStiffReset.relativePosition.y + 30);

                // Save settings button.
                UIButton vanishingStiffsSave = UIButtons.AddButton(Panel, UILayout.PositionRightOf(vanishingStiffReset), Translations.Translate("LBR_SAA"));
                vanishingStiffsSave.eventClicked += (c, p) =>
                {
                    // Update mod settings - inverted value (see above).
                    DataStore.AutoDeadRemovalChance = 100 - (int)vanishingStiffs.value;
                    Logging.Message("autoDeadRemovalChance set to: ", DataStore.AutoDeadRemovalChance);

                    // Update WG configuration file.
                    DataStore.SaveXML();
                };
            }
        }
    }
}