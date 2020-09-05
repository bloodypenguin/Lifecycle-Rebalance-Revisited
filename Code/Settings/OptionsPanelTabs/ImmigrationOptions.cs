using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting health options.
    /// </summary>
    public class ImmigrationOptions
    {
        /// <summary>
        /// Adds immigration options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public ImmigrationOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel immigrationTab = PanelUtils.AddTab(tabStrip, "Immigration", tabIndex);

            // Use vanilla.
            UICheckBox immigrationCheckBox = PanelUtils.AddPlainCheckBox(immigrationTab, "Apply 25% variation to immigrant education levels (game default off, mod default on)");
            immigrationCheckBox.relativePosition = new Vector3(5f, 5f);
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