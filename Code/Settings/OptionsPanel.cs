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

        // Parent UI panel reference.
        private static UIScrollablePanel optionsPanel;


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
            optionsPanel = ((UIHelper)helper).self as UIScrollablePanel;
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
            new SpeedOptions(tabStrip, 1);
            new DeathOptions(tabStrip, 2);
            new HealthOptions(tabStrip, 3);
            new TransportOptions(tabStrip, 4);
            new LoggingOptions(tabStrip, 5);

            // Start deactivated.
            optionsPanel.gameObject.SetActive(false);
        }


        /// <summary>
        /// Attaches an event hook to options panel visibility, to activate/deactivate our options panel as appropriate.
        /// Deactivating when not visible saves UI overhead and performance impacts, especially with so many UITextFields.
        /// </summary>
        public static void OptionsEventHook()
        {
            // Get options panel instance.
            UIPanel gameOptionsPanel = UIView.library.Get<UIPanel>("OptionsPanel");

            if (gameOptionsPanel == null)
            {
                Debug.Log("Lifecycle Rebalance Revisited: couldn't find OptionsPanel!");
            }
            else
            {
                // Simple event hook to enable/disable GameObject based on appropriate visibility.
                gameOptionsPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    optionsPanel.gameObject.SetActive(isVisible);
                };
            }
        }
    }
}