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

                // Update configuration file.
                OptionsPanel.settings.LogDeaths = isChecked;
                Configuration<SettingsFile>.Save();

                Debug.Log("Lifecycle Rebalance Revisited: death logging " + (OptionsPanel.settings.LogDeaths ? "enabled." : "disabled."));
            });
            loggingTab.AddCheckbox("Log immigrants to 'Lifecycle immigration log.txt'", OptionsPanel.settings.LogImmigrants, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseImmigrationLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogImmigrants = isChecked;
                Configuration<SettingsFile>.Save();

                Debug.Log("Lifecycle Rebalance Revisited: immigrant logging " + (OptionsPanel.settings.LogImmigrants ? "enabled." : "disabled."));
            });
            loggingTab.AddCheckbox("Log custom transport choices to 'Lifecycle transport log.txt'    WARNING - SLOW!", OptionsPanel.settings.LogTransport, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseTransportLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogTransport = isChecked;
                Configuration<SettingsFile>.Save();

                Debug.Log("Lifecycle Rebalance Revisited: transport choices logging " + (OptionsPanel.settings.LogTransport ? "enabled." : "disabled."));
            });
            loggingTab.AddCheckbox("Log sickness events to 'Lifecycle sickness log.txt'", OptionsPanel.settings.LogSickness, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseSicknessLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogSickness = isChecked;
                Configuration<SettingsFile>.Save();
                 
                Debug.Log("Lifecycle Rebalance Revisited: sickness logging " + (OptionsPanel.settings.LogSickness ? "enabled." : "disabled."));
            });
        }
    }
}