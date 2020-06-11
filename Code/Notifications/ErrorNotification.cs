using System;
using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Base class of the error notification panel.
    /// </summary>
    public class ErrorNotification : UIPanel
    {
        // Constants.
        protected const float panelWidth = 600;
        protected const float panelHeight = 300;
        protected const float spacing = 10;

        // Instance references.
        protected static GameObject uiGameObject;
        protected static ErrorNotification _instance;
        protected static ErrorNotification instance => _instance;

        // Message.
        public static string headerText, messageText;


        /// <summary>
        /// Creates the panel object in-game.
        /// </summary>
        public virtual void Create()
        {
            try
            {
                // Create new instance.
                // Give it a unique name for easy finding with ModTools.
                uiGameObject = new GameObject("LifecycleRebalanceErrorNotification");
                uiGameObject.transform.parent = UIView.GetAView().transform;
                _instance = uiGameObject.AddComponent<ErrorNotification>();
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
            base.Start();

            try
            {
                // Basic setup.
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = panelWidth;
                height = panelHeight;
                relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
                backgroundSprite = "UnlockingPanel2";

                // Put this behind other panels.
                zOrder = 2;

                // Title.
                AddText("Lifecycle Rebalance Revisited", spacing, spacing, 1.0f);

                // Note 1.
                float currentY = AddText(headerText, spacing, 40);

                // Note 2.
                currentY = AddText(messageText, spacing * 2, currentY + 20);

                // Auto resize panel to accomodate note.
                this.height = currentY + 60;

                // Close button.
                UIButton closeButton = CreateButton(this);
                closeButton.relativePosition = new Vector3(spacing, this.height - closeButton.height - spacing);
                closeButton.text = "Close";
                closeButton.Enable();

                // Event handler.
                closeButton.eventClick += (c, p) =>
                {
                    // Just hide this panel and destroy the game object - nothing more to do this load.
                    this.Hide();
                    GameObject.Destroy(uiGameObject);
                };
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }


        /// <summary>
        /// Adds text to the notification panel.
        /// </summary>
        /// <param name="text">Text to add</param>
        /// <param name="x">Relative x position</param>
        /// <param name="y">Relative y position</param>
        /// <param name="size">Text font size (default 0.8)</param>
        /// <returns>Relative y coordinate of the bottom of the new text field</returns>
        protected float AddText(string text, float x, float y, float size = 0.8f)
        {
            // Note 1.
            UILabel textLabel = this.AddUIComponent<UILabel>();
            textLabel.relativePosition = new Vector3(x, y);
            textLabel.textAlignment = UIHorizontalAlignment.Left;
            textLabel.text = text;
            textLabel.textScale = size;
            textLabel.autoSize = false;
            textLabel.autoHeight = true;
            textLabel.width = this.width - (x * 2);
            textLabel.wordWrap = true;

            return textLabel.height + y;
        }


        /// <summary>
        /// Creates a simple UI pushbutton.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected UIButton CreateButton(UIComponent parent)
        {
            UIButton button = parent.AddUIComponent<UIButton>();

            button.size = new Vector2(200f, 30f);
            button.textScale = 0.9f;
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.disabledTextColor = new Color32(128, 128, 128, 255);
            button.canFocus = false;

            return button;
        }
    }
}