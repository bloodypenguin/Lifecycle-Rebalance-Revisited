using ICities;
using ColossalFramework.UI;


namespace LifecycleRebalanceRevisited
{
    public class LifecycleRebalanceRevisitedMod : IUserMod
    {
        public static string version = "1.2";

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
        }
    }
}
