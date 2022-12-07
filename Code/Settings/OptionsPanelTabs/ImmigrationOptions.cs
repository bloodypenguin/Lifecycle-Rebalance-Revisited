// <copyright file="ImmigrationOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting health options.
    /// </summary>
    internal sealed class ImmigrationOptions : OptionsPanelTab
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImmigrationOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ImmigrationOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            Panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("LBR_IMM"), tabIndex, out _);

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

                // Use vanilla.
                UICheckBox immigrationCheckBox = UICheckBoxes.AddPlainCheckBox(Panel, Translations.Translate("LBR_IMM_VAR"));
                immigrationCheckBox.relativePosition = new Vector3(5f, 5f);
                immigrationCheckBox.isChecked = ModSettings.Settings.RandomImmigrantEd;
                immigrationCheckBox.eventCheckChanged += (c, isChecked) => { ModSettings.Settings.RandomImmigrantEd = isChecked; };

                // Boost immigrant education.
                UICheckBox immiEduBoostCheck = UICheckBoxes.AddPlainCheckBox(Panel, Translations.Translate("LBR_IMM_INC"));
                immiEduBoostCheck.relativePosition = new Vector3(5f, 50f);
                immiEduBoostCheck.isChecked = ModSettings.Settings.ImmiEduBoost;

                // Suppress immigrant education.
                UICheckBox immiEduDragCheck = UICheckBoxes.AddPlainCheckBox(Panel, Translations.Translate("LBR_IMM_DEC"));
                immiEduDragCheck.relativePosition = new Vector3(5f, 75f);
                immiEduDragCheck.isChecked = ModSettings.Settings.ImmiEduDrag;

                immiEduBoostCheck.eventCheckChanged += (c, isChecked) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.ImmiEduBoost = isChecked;

                    // Toggle immigrant boost check if needed.
                    if (isChecked && immiEduDragCheck.isChecked)
                    {
                        immiEduDragCheck.isChecked = false;
                    }
                };

                immiEduDragCheck.eventCheckChanged += (c, isChecked) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.ImmiEduDrag = isChecked;

                    // Toggle immigrant boost check if needed.
                    if (isChecked && immiEduBoostCheck.isChecked)
                    {
                        immiEduBoostCheck.isChecked = false;
                    }
                };
            }
        }
    }
}