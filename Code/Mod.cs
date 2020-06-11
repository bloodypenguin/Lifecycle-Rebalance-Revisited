using ICities;
using ColossalFramework.UI;
using CitiesHarmony.API;


namespace LifecycleRebalance
{
    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public class LifecycleRebalance : IUserMod
    {
        public static string Version => "1.4.1";

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

            // Check to see if UIView is ready (to attach the options panel event handler).
            if (UIView.GetAView() != null)
            {
                // It's ready - attach options panel event handler now.
                OptionsPanel.OptionsEventHook();
            }
            else
            {
                // Otherwise, queue the hook for when the intro's finished loading.
                LoadingManager.instance.m_introLoaded += OptionsPanel.OptionsEventHook;
            }
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
