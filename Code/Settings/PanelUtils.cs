using System;
using UnityEngine;
using ICities;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Utilities for Options Panel UI.
    /// </summary>
    public static class PanelUtils
    {
        /// <summary>
        /// Adds a tab to a UI tabstrip.
        /// </summary>
        /// <param name="tabStrip">UIT tabstrip to add to</param>
        /// <param name="tabName">Name of this tab</param>
        /// <param name="tabIndex">Index number of this tab</param>
        /// <returns>UIHelper instance for the new tab panel</returns>
        public static UIHelper AddTab(UITabstrip tabStrip, string tabName, int tabIndex)
        {
            // Create tab.
            UIButton tabButton = tabStrip.AddTab(tabName);

            // Sprites.
            tabButton.normalBgSprite = "SubBarButtonBase";
            tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";
            tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
            tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            tabButton.pressedBgSprite = "SubBarButtonBasePressed";

            // Tooltip.
            tabButton.tooltip = tabName;

            tabStrip.selectedIndex = tabIndex;

            // Force width.
            tabButton.width = 120;

            // Get tab root panel.
            UIPanel rootPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;

            // Panel setup.
            rootPanel.autoLayout = true;
            rootPanel.autoLayoutDirection = LayoutDirection.Vertical;
            rootPanel.autoLayoutPadding.top = 5;
            rootPanel.autoLayoutPadding.left = 10;

            // UIHelper.
            return new UIHelper(rootPanel);
        }


        /// <summary>
        /// Adds a plain text label to the specified UI panel.
        /// </summary>
        /// <param name="panel">UI panel to add the label to</param>
        /// <param name="text">Label text</param>
        /// <returns></returns>
        public static UILabel AddLabel(UIPanel panel, string text)
        {
            // Add label.
            UILabel label = (UILabel)panel.AddUIComponent<UILabel>();
            label.autoSize = false;
            label.autoHeight = true;
            label.wordWrap = true;
            label.width = 700;
            label.text = text;

            // Increase panel height to compensate.
            panel.height += label.height;

            return label;
        }


        /// <summary>
        /// Adds a slider with a descriptive text label above and an automatically updating value label immediately to the right.
        /// </summary>
        /// <param name="parent">Panel to add the control to</param>
        /// <param name="text">Descriptive label text</param>
        /// <param name="min">Slider minimum value</param>
        /// <param name="max">Slider maximum value</param>
        /// <param name="step">Slider minimum step</param>
        /// <param name="defaultValue">Slider initial value</param>
        /// <param name="eventCallback">Slider event handler</param>
        /// <returns></returns>
        public static UISlider AddSliderWithValue(UIPanel parent, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback)
        {
            // Add slider component.
            UIPanel sliderPanel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsSliderTemplate")) as UIPanel;
            sliderPanel.Find<UILabel>("Label").text = text;

            // Slider configuration.
            UISlider newSlider = sliderPanel.Find<UISlider>("Slider");
            newSlider.minValue = min;
            newSlider.maxValue = max;
            newSlider.stepSize = step;
            newSlider.value = defaultValue;

            // Label.
            UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
            sliderLabel.width = 500;
            sliderLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            sliderLabel.relativePosition = Vector3.zero;

            // Move default slider position to match resized label.
            newSlider.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            newSlider.relativePosition = PositionUnder(sliderLabel);
            newSlider.width = 500;

            // Value label.
            sliderPanel.autoLayout = false;
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.text = newSlider.value.ToString();
            valueLabel.relativePosition = PositionRightOf(newSlider, 8f, 1f);

            // Event handler to update value label.
            newSlider.eventValueChanged += (component, value) =>
            {
                valueLabel.text = value.ToString();
                eventCallback(value);
            };

            return newSlider;
        }


        /// <summary>
        /// Returns a relative position below a specified UI component, suitable for placing an adjacent component.
        /// </summary>
        /// <param name="uIComponent">Original (anchor) UI component</param>
        /// <param name="margin">Margin between components</param>
        /// <param name="horizontalOffset">Horizontal offset from first to second component</param>
        /// <returns></returns>
        private static Vector3 PositionUnder(UIComponent uIComponent, float margin = 8f, float horizontalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + horizontalOffset, uIComponent.relativePosition.y + uIComponent.height + margin);
        }


        /// <summary>
        /// Returns a relative position to the right of a specified UI component, suitable for placing an adjacent component.
        /// </summary>
        /// <param name="uIComponent">Original (anchor) UI component</param>
        /// <param name="margin">Margin between components</param>
        /// <param name="verticalOffset">Vertical offset from first to second component</param>
        /// <returns></returns>
        public static Vector3 PositionRightOf(UIComponent uIComponent, float margin = 10f, float verticalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }


        /// <summary>
        /// Updates XML configuration file with current settings.
        /// </summary>
        public static void SaveXML()
        {
            // Write to file.
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionTwo();
                xml.writeXML(Loading.currentFileLocation);
            }
            catch (Exception e)
            {
                Debug.Log("Lifecycle Rebalance Revisited: XML writing exception:\r\n" + e.Message);
            }
        }
    }
}