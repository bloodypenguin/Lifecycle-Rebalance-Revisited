using System;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Globalization;


namespace LifecycleRebalance
{/// <summary>
 /// Class to handle the mod settings options panel.
 /// </summary>
    internal static class OptionsPanel
    {
        // Parent UI panel reference.
        internal static UIScrollablePanel optionsPanel;
        private static UIPanel gameOptionsPanel;

        // Instance reference.
        private static GameObject optionsGameObject;


        /// <summary>
        /// Attaches an event hook to options panel visibility, to create/destroy our options panel as appropriate.
        /// Destroying when not visible saves UI overhead and performance impacts, especially with so many UITextFields.
        /// </summary>
        internal static void OptionsEventHook()
        {
            // Get options panel instance.
            gameOptionsPanel = UIView.library.Get<UIPanel>("OptionsPanel");

            if (gameOptionsPanel == null)
            {
                Logging.Error("couldn't find OptionsPanel");
            }
            else
            {
                // Simple event hook to create/destroy GameObject based on appropriate visibility.
                gameOptionsPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    // Create/destroy based on visible.
                    if (isVisible)
                    {
                        Create();
                    }
                    else
                    {
                        Close();
                    }
                };

                // Recreate panel on system locale change.
                LocaleManager.eventLocaleChanged += LocaleChanged;
            }
        }


        /// <summary>
        /// Refreshes the options panel (destroys and rebuilds) on a locale change when the options panel is open.
        /// </summary>
        internal static void LocaleChanged()
        {
            if (gameOptionsPanel != null && gameOptionsPanel.isVisible)
            {
                Close();
                Create();
            }
        }


        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        private static void Create()
        {
            try
            {
                // We're now visible - create our gameobject, and give it a unique name for easy finding with ModTools.
                optionsGameObject = new GameObject("RealPopOptionsPanel");

                // Attach to game options panel.
                optionsGameObject.transform.parent = optionsPanel.transform;

                // Create a base panel attached to our game object, perfectly overlaying the game options panel.
                UIPanel basePanel = optionsGameObject.AddComponent<UIPanel>();
                basePanel.absolutePosition = optionsPanel.absolutePosition;
                basePanel.width = optionsPanel.width;
                basePanel.height = 735f;

                // Add tabstrip.
                UITabstrip tabStrip = basePanel.AddUIComponent<UITabstrip>();
                tabStrip.relativePosition = new Vector3(0, 0);
                tabStrip.size = new Vector2(744, 713);

                // Tab container (the panels underneath each tab).
                UITabContainer tabContainer = basePanel.AddUIComponent<UITabContainer>();
                tabContainer.relativePosition = new Vector3(0, 40);
                tabContainer.size = new Vector3(744, 713);
                tabStrip.tabPages = tabContainer;

                // Add tabs and panels.
                new ModOptions(tabStrip, 0);
                new CalculationOptions(tabStrip, 1);
                new SpeedOptions(tabStrip, 2);
                new DeathOptions(tabStrip, 3);
                new HealthOptions(tabStrip, 4);
                new TransportOptions(tabStrip, 5);
                new ImmigrationOptions(tabStrip, 6);
                tabStrip.selectedIndex = 0;

                // Event handler for tab index change; setup the selected tab.
                tabStrip.eventSelectedIndexChanged += (control, index) =>
                {
                    if (tabStrip.tabs[index].objectUserData is OptionsPanelTab childTab)
                    {
                        childTab.Setup();
                    }
                };

                // Change tab size and text scale (to fit them all in...).
                foreach (UIButton button in tabStrip.components)
                {
                    button.textScale = 0.8f;
                    button.width = 100f;
                }

                // Ensure initial selected tab (doing a 'quickstep' via the second tab to ensure proper events are triggered).
                tabStrip.selectedIndex = 1;
                tabStrip.selectedIndex = 0;
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating options panel");
            }
        }


        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        private static void Close()
        {
            // Save settings on close.
            ModSettings.Save();

            // We're no longer visible - destroy our game object.
            if (optionsGameObject != null)
            {
                GameObject.Destroy(optionsGameObject);
                optionsGameObject = null;
            }
        }
    }
}