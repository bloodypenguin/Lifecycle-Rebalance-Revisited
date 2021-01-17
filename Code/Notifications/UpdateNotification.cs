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
                Logging.LogException(e, "exception creating update notification");
            }
        }


        /// <summary>
        /// Create the update notification panel; called by Unity just before any of the Update methods is called for the first time.
        /// </summary>
        public override void Start()
        {
            // Add text.
            headerText = Translations.Translate("LBR_150_TIT");
            messageText = Translations.Translate("LBR_150_NT1") + "\r\n\r\n" + Translations.Translate("LBR_150_NT2");

            base.Start();

            try
            {
                // "Don't show again" button.
                UIButton noShowButton = CreateButton(this);
                noShowButton.relativePosition = new Vector3(this.width - noShowButton.width - spacing, this.height - noShowButton.height - spacing);
                noShowButton.text = Translations.Translate("MES_DSA");
                noShowButton.Enable();

                // Event handler.
                noShowButton.eventClick += (c, p) =>
                {
                    // Update and save settings file.
                    Loading.settingsFile.NotificationVersion = 4;
                    Configuration<SettingsFile>.Save();

                    // Just hide this panel and destroy the game object - nothing more to do.
                    this.Hide();
                    GameObject.Destroy(uiGameObject);
                };
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception setting up update notification");
            }
        }
    }
}