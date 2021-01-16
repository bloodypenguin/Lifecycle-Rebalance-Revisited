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
            UIPanel speedTab = PanelUtils.AddTab(tabStrip, Translations.Translate("LBR_SPD"), tabIndex, true);

            // Lifespan multiplier.  Simple integer.
            UISlider lifeMult = PanelUtils.AddSliderWithValue(speedTab, Translations.Translate("LBR_SPD_FAC") + "\r\n\r\n" + Translations.Translate("LBR_SPD_FN1") + "\r\n\r\n" + Translations.Translate("LBR_SPD_FN2") + "\r\n\r\n" + Translations.Translate("LBR_SPD_FN3"), 1f, 15f, 1f, DataStore.lifeSpanMultiplier, (value) => { });

            // Reset to saved button.
            UIButton lifeMultReset = PanelUtils.CreateButton(speedTab, Translations.Translate("LBR_RTS"));
            lifeMultReset.eventClicked += (control, clickEvent) =>
            {
                // Retrieve saved value from datastore - inverted value (see above).
                lifeMult.value = DataStore.lifeSpanMultiplier;
            };

            // Turn off autolayout to fit next button to the right at the same y-value and increase button Y-value to clear slider.
            //speedTab.autoLayout = false;
            lifeMultReset.relativePosition = new Vector3(lifeMultReset.relativePosition.x, lifeMult.relativePosition.y + 40);

            // Save settings button.
            UIButton lifeMultSave = PanelUtils.CreateButton(speedTab, Translations.Translate("LBR_SAA"));
            lifeMultSave.relativePosition = PanelUtils.PositionRightOf(lifeMultReset);
            lifeMultSave.eventClicked += (control, value) =>
            {
                // Update mod settings - inverted value (see above).
                DataStore.lifeSpanMultiplier = (int)lifeMult.value;
                Logging.Message("lifespan multiplier set to: ", DataStore.lifeSpanMultiplier.ToString());

                // Update WG configuration file.
                PanelUtils.SaveXML();
            };
        }
    }
}