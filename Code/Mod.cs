using ICities;
using ColossalFramework.UI;
using CitiesHarmony.API;


namespace LifecycleRebalance
{
    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public class LifecycleRebalanceMod : IUserMod
    {
        // Public mod name and description.
        public string Name => ModName + " " + Version;
        public string Description => Translations.Translate("LBR_DESC");


        // Internal and private name and version components.
        internal static string ModName => "Lifecycle Rebalance Revisited";
        internal static string Version => BaseVersion + " " + Beta;
        internal static string Beta => "BETA 3";
        internal static int BetaVersion => 0;
        private static string BaseVersion => "1.5.4";



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

            // Load configuation file.
            Loading.ReadFromXML();
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
            // Setup options panel reference.
            OptionsPanel.optionsPanel = ((UIHelper)helper).self as UIScrollablePanel;
            OptionsPanel.optionsPanel.autoLayout = false;
        }
    }
}
