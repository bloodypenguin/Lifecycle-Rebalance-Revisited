using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting transport options.
    /// </summary>
    public class TransportOptions
    {
        /// <summary>
        /// Adds death options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public TransportOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel transportTab = PanelUtils.AddTab(tabStrip, "Transport", tabIndex, true);

            UICheckBox transportCheckBox = PanelUtils.AddPlainCheckBox(transportTab, "Use custom transport mode probabilities (from configuration file)");
            transportCheckBox.isChecked = OptionsPanel.settings.UseTransportModes;
            transportCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                ModSettings.UseTransportModes = isChecked;

                // Update configuration file.
                OptionsPanel.settings.UseTransportModes = isChecked;
                Configuration<SettingsFile>.Save();
            };
        }
    }
}