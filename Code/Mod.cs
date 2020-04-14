using ICities;
using ColossalFramework.UI;


namespace LifecycleRebalanceRevisited
{
    public class LifecycleRebalanceRevisitedMod : IUserMod
    {
        public static string version = "1.3";

        public string Name => "Lifecycle Rebalance Revisited v" + version;
        
        public string Description => "Increases and randomises citizen life span, randomises the ages and education levels of immigrants, and changes how citizens travel to and from work.";

        private UICheckBox sunsetCheckbox;
        private UICheckBox legacyCheckbox;


        // Setup options UI
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Load configuration
            LifecycleRebalanceSettingsFile settings = Configuration<LifecycleRebalanceSettingsFile>.Load();

            UIHelperBase group = helper.AddGroup("Lifecycle Balance Revisited");

            sunsetCheckbox = (UICheckBox)group.AddCheckbox("Use Sunset Harbor lifespans - longer lifespans and more seniors", !settings.UseLegacy, delegate (bool isChecked)
            {
                settings.UseLegacy = !isChecked;
                legacyCheckbox.isChecked = !isChecked;
                Configuration<LifecycleRebalanceSettingsFile>.Save();
            });
            legacyCheckbox = (UICheckBox)group.AddCheckbox("Use legacy lifespans (original WG mod) - shorter lifespans and fewer seniors", settings.UseLegacy, delegate (bool isChecked)
            {
                settings.UseLegacy = isChecked;
                sunsetCheckbox.isChecked = !isChecked;
                Configuration<LifecycleRebalanceSettingsFile>.Save();
            });
            group.AddGroup(" ");
            group.AddCheckbox("Log deaths to 'Lifecycle death log.txt'", settings.LogDeaths, (isChecked) => { settings.LogDeaths = isChecked; Configuration<LifecycleRebalanceSettingsFile>.Save(); });
            group.AddCheckbox("Log immigrants to 'Lifecycle immigration log.txt'", settings.LogImmigrants, (isChecked) => { settings.LogImmigrants = isChecked; Configuration<LifecycleRebalanceSettingsFile>.Save(); });
            group.AddCheckbox("Log transport choices to 'Lifecycle transport log.txt'", settings.LogTransport, (isChecked) => { settings.LogTransport = isChecked; Configuration<LifecycleRebalanceSettingsFile>.Save(); });
            group.AddGroup("WARNING: Logging transport choices will slow your game!");
        }

    }
}
