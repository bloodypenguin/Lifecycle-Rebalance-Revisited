using System.Text;
using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting health options.
    /// </summary>
    public class ImmigrationOptions
    {
        // Sickness deciles; set to 10 (even though 11 in DataStore) as current WG XML v2 only stores the first 10.
        private const int numDeciles = 10;
        private static float[] defaultSicknessProbs = { 0.0125f, 0.0075f, 0.01f, 0.01f, 0.015f, 0.02f, 0.03f, 0.04f, 0.05f, 0.075f, 0.25f };


        /// <summary>
        /// Adds health options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public ImmigrationOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel immigrationTab = PanelUtils.AddTab(tabStrip, "Immigration", tabIndex);

            // Use vanilla.
            UICheckBox immigrationCheckBox = PanelUtils.AddPlainCheckBox(immigrationTab, "Apply 25% variation to immigrant education levels (game default off, mod default on)");
            immigrationCheckBox.isChecked = OptionsPanel.settings.RandomImmigrantEd;
            immigrationCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                ModSettings.UseTransportModes = isChecked;

                // Update configuration file.
                OptionsPanel.settings.RandomImmigrantEd = isChecked;
                Configuration<SettingsFile>.Save();
            };
        }
    }
}