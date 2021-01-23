using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace LifecycleRebalance.MessageBox
{
    /// <summary>
    /// 'What's new' message box.
    /// </summary>
    public class WhatsNewMessageBox : DontShowAgainMessageBox
    {
        /// <summary>
        /// Sets the 'what's new' messages to display.
        /// </summary>
        /// <param name="messages">Version update messages to display, in order (newest versions first)</param>
        /// <param name="lastNotifiedVersion">Last notified version (version messages equal to or earlier than this will be minimized</param>
        public void SetMessages(Version lastNotifiedVersion, Dictionary<Version, string> messages)
        {
            // Iterate through each provided message and add it to the messagebox.
            foreach (KeyValuePair<Version, string> message in messages)
            {
                VersionMessage versionMessage = ScrollableContent.AddUIComponent<VersionMessage>();
                versionMessage.width = ScrollableContent.width;
                versionMessage.SetText(message.Key, message.Value);

                // Hide version messages that have already been notified.
                if (message.Key <= lastNotifiedVersion)
                {
                    versionMessage.IsMaximized = false;
                }
            }
        }


        /// <summary>
        /// Update message for given version.
        /// </summary>
        public class VersionMessage : UIPanel
        {
            // Components.
            private UIButton minimizeButton;
            private UILabel messageLabel;

            // Version title.
            private string versionTitle;


            /// <summary>
            /// Message maximimized/minimized state.
            /// </summary>
            public bool IsMaximized { get => messageLabel.isVisible; set => messageLabel.isVisible = value; }


            /// <summary>
            /// Constructor - performs basic setup.
            /// </summary>
            public VersionMessage()
            {
                // Basic setup.
                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                autoFitChildrenVertically = true;
                autoLayoutPadding = new RectOffset(0, 0, (int)Padding / 2, (int)Padding / 2);

                // Add minimize button.
                minimizeButton = AddUIComponent<UIButton>();
                minimizeButton.height = 20f;
                minimizeButton.horizontalAlignment = UIHorizontalAlignment.Left;
                minimizeButton.color = Color.white;
                minimizeButton.textHorizontalAlignment = UIHorizontalAlignment.Left;

                // Toggle visible (minimized) state when clicked.
                minimizeButton.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => IsMaximized = !IsMaximized;

                // Add message text label
                messageLabel = AddUIComponent<UILabel>();
                messageLabel.textAlignment = UIHorizontalAlignment.Left;
                messageLabel.verticalAlignment = UIVerticalAlignment.Middle;
                messageLabel.textScale = 0.8f;
                messageLabel.wordWrap = true;
                messageLabel.autoHeight = true;
                messageLabel.size = new Vector2(width - 2 * Padding, 0);
                messageLabel.relativePosition = new Vector3(17, 7);
                messageLabel.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;

                // Event handlers for minimizing/maximizing.
                messageLabel.eventTextChanged += (UIComponent component, string value) => messageLabel.PerformLayout();
                messageLabel.eventVisibilityChanged += (UIComponent component, bool value) => UpdateState();
            }


            /// <summary>
            /// Sets version message text.
            /// </summary>
            /// <param name="version">Version</param>
            /// <param name="message">Message text</param>
            public void SetText(Version version, string message)
            {
                // Set version header and message text.
                versionTitle = LifecycleRebalance.ModName + " " + version.ToString();
                messageLabel.text = message;

                // Always start maximized.
                IsMaximized = true;

                // Set state indictor.
                UpdateState();
            }


            /// <summary>
            /// Handles size changed events. for e.g. when text changes.  Called by game.
            /// </summary>
            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();

                // Set width of button and label to match new width of list item (whose width has been set by the MessageBox).
                if (minimizeButton != null)
                {
                    minimizeButton.width = width;
                };
                if (messageLabel != null)
                {
                    messageLabel.width = width;
                };
            }


            /// <summary>
            /// Sets minimized/maximized state indicator.
            /// </summary>
            private void UpdateState() => minimizeButton.text = (IsMaximized ? "▼ " : "► ") + versionTitle;
        }
    }
}
