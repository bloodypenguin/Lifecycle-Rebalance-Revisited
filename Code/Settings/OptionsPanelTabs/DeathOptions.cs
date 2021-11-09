using System;
using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting death options.
    /// </summary>
    internal class DeathOptions : OptionsPanelTab
    {
        /// <summary>
        /// Adds death options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal DeathOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            panel = PanelUtils.AddTab(tabStrip, Translations.Translate("LBR_DTH"), tabIndex, true);

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

                // Percentage of corpses requiring transport.  % of bodies requiring transport is more intuitive to user than % of vanishing corpses, so we invert the value.
                UISlider vanishingStiffs = PanelUtils.AddSliderWithValue(panel, Translations.Translate("LBR_DTH_TRN") + Environment.NewLine + Translations.Translate("LBR_DTH_TRD"), 0, 100, 1, 100 - DataStore.autoDeadRemovalChance, (value) => { });

                // Reset to saved button.
                UIButton vanishingStiffReset = PanelUtils.CreateButton(panel, Translations.Translate("LBR_RTS"));
                vanishingStiffReset.eventClicked += (control, clickEvent) =>
                {
                // Retrieve saved value from datastore - inverted value (see above).
                vanishingStiffs.value = 100 - DataStore.autoDeadRemovalChance;
                };

                // Turn off autolayout to fit next button to the right at the same y-value and increase button Y-value to clear slider.
                //deathTab.autoLayout = false;
                vanishingStiffReset.relativePosition = new Vector3(vanishingStiffReset.relativePosition.x, vanishingStiffReset.relativePosition.y + 30);

                // Save settings button.
                UIButton vanishingStiffsSave = PanelUtils.CreateButton(panel, Translations.Translate("LBR_SAA"));
                vanishingStiffsSave.relativePosition = PanelUtils.PositionRightOf(vanishingStiffReset);
                vanishingStiffsSave.eventClicked += (control, clickEvent) =>
                {
                    // Update mod settings - inverted value (see above).
                    DataStore.autoDeadRemovalChance = 100 - (int)vanishingStiffs.value;
                    Logging.Message("autoDeadRemovalChance set to: ", DataStore.autoDeadRemovalChance.ToString());

                    // Update WG configuration file.
                    PanelUtils.SaveXML();
                };
            }
        }
    }
}