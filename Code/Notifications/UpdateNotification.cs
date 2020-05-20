using System;
using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Base class of the update notification panel.
    /// </summary>
    public class UpdateNotification : ErrorNotification
    {


        /// <summary>
        /// Creates the panel object in-game.
        /// </summary>
        public override void Create()
        {
            try
            {
                // Destroy existing (if any) instances.
                uiGameObject = GameObject.Find("LifecycleRebalanceUpgradeNotification");
                if (uiGameObject != null)
                {
                    UnityEngine.Debug.Log("Lifecycle Rebalance Revisited: found existing upgrade notification instance.");
                    GameObject.Destroy(uiGameObject);
                }

                // Create new instance.
                // Give it a unique name for easy finding with ModTools.
                uiGameObject = new GameObject("LifecycleRebalanceUpgradeNotification");
                uiGameObject.transform.parent = UIView.GetAView().transform;
                _instance = uiGameObject.AddComponent<UpdateNotification>();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }


        /// <summary>
        /// Create the update notification panel; called by Unity just before any of the Update methods is called for the first time.
        /// </summary>
        public override void Start()
        {
            // Add text.
            headerText = "Lifecycle Rebalance Revisited has been updated to version 1.4.  Key update changes are:";
            messageText = "The setting changing the percentage of dead bodies that require transport now applies to ALL deaths (including those from sickness and pollution, not just old age).  Setting this to zero will should now completely remove the need for deathcare transportation (hearses) in your city.\r\n\r\nVanilla lifecycle calculation option - revert to base-game lifespans while still using other aspects of this mod.\r\n\r\nYou can now use the options panel to easily choose between using this mod's custom transport mode probabilities (from the configuration file) and using the game defaults.\r\n\r\nThe mod's option panel is now split into tabs for easier navigation.";

            base.Start();

            try
            {
                // "Don't show again" button.
                UIButton noShowButton = CreateButton(this);
                noShowButton.relativePosition = new Vector3(this.width - noShowButton.width - spacing, this.height - noShowButton.height - spacing);
                noShowButton.text = "Don't show again";
                noShowButton.Enable();

                // Event handler.
                noShowButton.eventClick += (c, p) =>
                {
                    // Update and save settings file.
                    Loading.settingsFile.NotificationVersion = 2;
                    Configuration<SettingsFile>.Save();

                    // Just hide this panel and destroy the game object - nothing more to do.
                    this.Hide();
                    GameObject.Destroy(uiGameObject);
                };
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}