using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting mod lifecycle speed options.
    /// </summary>
    public class SpeedOptions
    {
        /// <summary>
        /// Adds speed options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public SpeedOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIHelper speedTab = PanelUtils.AddTab(tabStrip, "Age speed", tabIndex);

            // Lifespan multiplier.  Simple integer.
            UISlider lifeMult = PanelUtils.AddSliderWithValue((UIPanel)speedTab.self, "Factor to slow down how fast citizens age, compared to base game.\r\n\r\n1 is base game speed (35 in-game weeks is equal to 10 years of age), 2 is twice as slow (70 in-game weeks for 10 years of age), 3 is three times as slow, etc.  A multiplier of 15 means that citizens will age approximately one year for each in-game year.\r\n\r\nDoes not affect lifecycles or ages - only the speed at which citizens age relative to game speed.\r\n\r\n(Game default 1, mod default 3)", 1f, 15f, 1f, DataStore.lifeSpanMultiplier, (value) => { });

            // Reset to saved button.
            UIButton lifeMultReset = (UIButton)speedTab.AddButton("Reset to saved", () =>
            {
                // Retrieve saved value from datastore - inverted value (see above).
                lifeMult.value = DataStore.lifeSpanMultiplier;
            });

            // Turn off autolayout to fit next button to the right at the same y-value and increase button Y-value to clear slider.
            ((UIPanel)lifeMultReset.parent).autoLayout = false;
            ((UIPanel)lifeMultReset.parent).height += 20;
            lifeMultReset.relativePosition = new Vector3(lifeMultReset.relativePosition.x, lifeMult.relativePosition.y + 40);

            // Save settings button.
            UIButton lifeMultSave = (UIButton)speedTab.AddButton("Save and apply", () =>
            {
                // Update mod settings - inverted value (see above).
                DataStore.lifeSpanMultiplier = (int)lifeMult.value;
                Debug.Log("Lifecycle Rebalance Revisited: lifespan multiplier set to: " + DataStore.lifeSpanMultiplier + ".");

                // Update WG configuration file.
                PanelUtils.SaveXML();
            });
            lifeMultSave.relativePosition = PanelUtils.PositionRightOf(lifeMultReset);
        }
    }
}