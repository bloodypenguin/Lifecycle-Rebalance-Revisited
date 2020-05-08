using ICities;


namespace LifecycleRebalance
{
    public class LifecycleRebalance : IUserMod
    {
        public static string version = "1.3.5";

        public string Name => "Lifecycle Rebalance Revisited v" + version;
        
        public string Description => "Increases and randomises citizen life span, randomises the ages and education levels of immigrants, and changes how citizens travel to and from work.";



        // Setup options UI
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Create options panel.
            OptionsPanel optionsPanel = new OptionsPanel(helper);
        }
    }
}
