using ICities;
using ColossalFramework.UI;
using UnityEngine;


namespace LifecycleRebalanceRevisited
{
    public class LifecycleRebalanceRevisitedMod : IUserMod
    {
        public static string version = "1.3.1";

        public string Name => "Lifecycle Rebalance Revisited v" + version;
        
        public string Description => "Increases and randomises citizen life span, randomises the ages and education levels of immigrants, and changes how citizens travel to and from work.";

        private UICheckBox sunsetCheckbox;
        private UICheckBox legacyCheckbox;


        // Setup options UI
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Load configuration
            SettingsFile settings = Configuration<SettingsFile>.Load();

            UIHelperBase group = helper.AddGroup("Lifecycle Balance Revisited v" + version);

            sunsetCheckbox = (UICheckBox)group.AddCheckbox("Use Sunset Harbor lifespans - longer lifespans and more seniors", !settings.UseLegacy, (isChecked) =>
            {
                // There Can Only Be One (selected checkbox in this group).
                legacyCheckbox.isChecked = !isChecked;

                // Update mod settings.
                ModSettings.Legacy = !isChecked;

                // Update configuration file.
                settings.UseLegacy = !isChecked;
                Configuration<SettingsFile>.Save();
            });
            legacyCheckbox = (UICheckBox)group.AddCheckbox("Use legacy lifespans (original WG mod) - shorter lifespans and fewer seniors", settings.UseLegacy, (isChecked) =>
            {
                // There Can Only Be One (selected checkbox in this group).
                sunsetCheckbox.isChecked = !isChecked;

                // Update mod settings.
                ModSettings.Legacy = isChecked;

                // Update configuration file.
                settings.UseLegacy = isChecked;
                Configuration<SettingsFile>.Save();
            });

            UIHelperBase group2 = helper.AddGroup("Logging");

            group2.AddCheckbox("Log deaths to 'Lifecycle death log.txt'", settings.LogDeaths, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseDeathLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: death logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogDeaths = isChecked;
                Configuration<SettingsFile>.Save();
            });
            group2.AddCheckbox("Log immigrants to 'Lifecycle immigration log.txt'", settings.LogImmigrants, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseImmigrationLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: immigrant logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogImmigrants = isChecked;
                Configuration<SettingsFile>.Save();
            });
            group2.AddCheckbox("Log transport choices to 'Lifecycle transport log.txt'    WARNING - SLOW!", settings.LogTransport, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseTransportLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: transport choices logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogTransport = isChecked;
                Configuration<SettingsFile>.Save();
            });
        }

    }
}
