using UnityEngine;
using ICities;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Class to handle the mod settings options panel.
    /// </summary>
    public class OptionsPanel
    {
        // Settings file.
        public static SettingsFile settings;


        /// <summary>
        /// Options panel constructor.
        /// </summary>
        /// <param name="helper">UIHelperBase parent</param>
        public OptionsPanel(UIHelperBase helper)
        {
            // Load settings.
            settings = Configuration<SettingsFile>.Load();

            // Read configuration XML if we haven't already.
            if (!Loading.isModCreated)
            {
                Loading.readFromXML();
            }

            // Set up tab strip and containers.
            UIScrollablePanel optionsPanel = ((UIHelper)helper).self as UIScrollablePanel;
            optionsPanel.autoLayout = false;

            UITabstrip tabStrip = optionsPanel.AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0, 0);
            tabStrip.size = new Vector2(744, 713);

            UITabContainer tabContainer = optionsPanel.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 40);
            tabContainer.size = new Vector3(744, 713);
            tabStrip.tabPages = tabContainer;

            // Add tabs and panels.
            new CalculationOptions(tabStrip, 0);
            new DeathOptions(tabStrip, 1);
            new HealthOptions(tabStrip, 2);
            new TransportOptions(tabStrip, 3);
            new LoggingOptions(tabStrip, 4);
        }
    }
}