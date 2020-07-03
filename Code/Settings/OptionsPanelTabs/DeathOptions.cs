using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting death options.
    /// </summary>
    public class DeathOptions
    {
        /// <summary>
        /// Adds death options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public DeathOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel deathTab = PanelUtils.AddTab(tabStrip, "Death", tabIndex, true);

            // Percentage of corpses requiring transport.  % of bodies requiring transport is more intuitive to user than % of vanishing corpses, so we invert the value.
            UISlider vanishingStiffs = PanelUtils.AddSliderWithValue(deathTab, "% of dead bodies requiring deathcare transportation\r\n(Game default 67%, mod default 50%)", 0, 100, 1, 100 - DataStore.autoDeadRemovalChance, (value) => { });

            // Reset to saved button.
            UIButton vanishingStiffReset = PanelUtils.CreateButton(deathTab, "Reset to saved");
            vanishingStiffReset.eventClicked += (control, clickEvent) =>
            {
                // Retrieve saved value from datastore - inverted value (see above).
                vanishingStiffs.value = 100 - DataStore.autoDeadRemovalChance;
            };

            // Turn off autolayout to fit next button to the right at the same y-value and increase button Y-value to clear slider.
            //deathTab.autoLayout = false;
            vanishingStiffReset.relativePosition = new Vector3(vanishingStiffReset.relativePosition.x, vanishingStiffReset.relativePosition.y + 30);

            // Save settings button.
            UIButton vanishingStiffsSave = PanelUtils.CreateButton(deathTab, "Save and apply");
            vanishingStiffsSave.relativePosition = PanelUtils.PositionRightOf(vanishingStiffReset);
            vanishingStiffsSave.eventClicked += (control, clickEvent) =>
            {
                // Update mod settings - inverted value (see above).
                DataStore.autoDeadRemovalChance = 100 - (int)vanishingStiffs.value;
                Debug.Log("Lifecycle Rebalance Revisited: autoDeadRemovalChance set to: " + DataStore.autoDeadRemovalChance + "%.");

                // Update WG configuration file.
                PanelUtils.SaveXML();
            };
        }
    }
}