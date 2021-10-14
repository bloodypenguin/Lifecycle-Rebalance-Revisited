using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Base class for options panel tabs.
    /// </summary>
    internal abstract class OptionsPanelTab
    {
        // Status flag.
        protected bool isSetup = false;

        // Panel reference.
        protected UIPanel panel;


        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal abstract void Setup();
    }
}