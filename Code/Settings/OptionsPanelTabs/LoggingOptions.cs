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
            UIPanel loggingTab = PanelUtils.AddTab(tabStrip, "Logging", tabIndex, true);

            // Logging options.
            UICheckBox deathCheckBox = PanelUtils.AddPlainCheckBox(loggingTab, "Log deaths to 'Lifecycle death log.txt'");
            deathCheckBox.isChecked = OptionsPanel.settings.LogDeaths;
            deathCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Debugging.UseDeathLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogDeaths = isChecked;
                Configuration<SettingsFile>.Save();

                Debug.Log("Lifecycle Rebalance Revisited: death logging " + (OptionsPanel.settings.LogDeaths ? "enabled." : "disabled."));
            };

            UICheckBox immigrantCheckBox = PanelUtils.AddPlainCheckBox(loggingTab, "Log immigrants to 'Lifecycle immigration log.txt'");
            immigrantCheckBox.isChecked = OptionsPanel.settings.LogImmigrants;
            immigrantCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Debugging.UseImmigrationLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogImmigrants = isChecked;
                Configuration<SettingsFile>.Save();

                Debug.Log("Lifecycle Rebalance Revisited: immigrant logging " + (OptionsPanel.settings.LogImmigrants ? "enabled." : "disabled."));
            };

            UICheckBox transportCheckBox = PanelUtils.AddPlainCheckBox(loggingTab, "Log custom transport choices to 'Lifecycle transport log.txt' WARNING - SLOW!");
            transportCheckBox.isChecked = OptionsPanel.settings.LogTransport;
            transportCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Debugging.UseTransportLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogTransport = isChecked;
                Configuration<SettingsFile>.Save();

                Debug.Log("Lifecycle Rebalance Revisited: transport choices logging " + (OptionsPanel.settings.LogTransport ? "enabled." : "disabled."));
            };

            UICheckBox sicknessCheckBox = PanelUtils.AddPlainCheckBox(loggingTab, "Log sickness events to 'Lifecycle sickness log.txt'");
            sicknessCheckBox.isChecked = OptionsPanel.settings.LogSickness;
            sicknessCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Debugging.UseSicknessLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogSickness = isChecked;
                Configuration<SettingsFile>.Save();
                 
                Debug.Log("Lifecycle Rebalance Revisited: sickness logging " + (OptionsPanel.settings.LogSickness ? "enabled." : "disabled."));
            };
        }
    }
}