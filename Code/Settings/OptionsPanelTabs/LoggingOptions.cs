using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting logging options.
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// Adds logging options tab to tabstrip.
        /// </summary
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public LoggingOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIHelper loggingTab = PanelUtils.AddTab(tabStrip, "Logging", tabIndex);

            // Logging options.
            loggingTab.AddCheckbox("Log deaths to 'Lifecycle death log.txt'", OptionsPanel.settings.LogDeaths, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseDeathLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: death logging " + (OptionsPanel.settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                OptionsPanel.settings.LogDeaths = isChecked;
                Configuration<SettingsFile>.Save();
            });
            loggingTab.AddCheckbox("Log immigrants to 'Lifecycle immigration log.txt'", OptionsPanel.settings.LogImmigrants, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseImmigrationLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: immigrant logging " + (OptionsPanel.settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                OptionsPanel.settings.LogImmigrants = isChecked;
                Configuration<SettingsFile>.Save();
            });
            loggingTab.AddCheckbox("Log transport choices to 'Lifecycle transport log.txt'    WARNING - SLOW!", OptionsPanel.settings.LogTransport, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseTransportLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: transport choices logging " + (OptionsPanel.settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                OptionsPanel.settings.LogTransport = isChecked;
                Configuration<SettingsFile>.Save();
            });
            loggingTab.AddCheckbox("Log sickness events to 'Lifecycle sickness log.txt'", OptionsPanel.settings.LogSickness, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseSicknessLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: sickness logging " + (OptionsPanel.settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                OptionsPanel.settings.LogSickness = isChecked;
                Configuration<SettingsFile>.Save();
            });
        }
    }
}