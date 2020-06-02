using ICities;
using CitiesHarmony.API;


namespace LifecycleRebalance
{
    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public class LifecycleRebalance : IUserMod
    {
        public static string Version => "1.4.1 BETA";

        public string Name => "Lifecycle Rebalance Revisited v" + Version;
        
        public string Description => "Increases and randomises citizen life span, randomises the ages and education levels of immigrants, and changes how citizens travel to and from work.";


        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            // Apply Harmony patches via Cities Harmony.
            // Called here instead of OnCreated to allow the auto-downloader to do its work prior to launch.
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }


        /// <summary>
        /// Called by the game when the mod is disabled.
        /// </summary>
        public void OnDisabled()
        {
            // Unapply Harmony patches via Cities Harmony.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }


        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Create options panel.
            OptionsPanel optionsPanel = new OptionsPanel(helper);
        }
    }
}
