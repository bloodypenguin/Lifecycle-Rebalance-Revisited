using System;
using UnityEngine;
using ICities;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Utilities for Options Panel UI.
    /// </summary>
    internal static class PanelUtils
    {
        /// <summary>
        /// Adds a tab to a UI tabstrip.
        /// </summary>
        /// <param name="tabStrip">UIT tabstrip to add to</param>
        /// <param name="tabName">Name of this tab</param>
        /// <param name="tabIndex">Index number of this tab</param>
        /// <returns>UIHelper instance for the new tab panel</returns>
        internal static UIPanel AddTab(UITabstrip tabStrip, string tabName, int tabIndex, bool autoLayout = false)
        {
            const float Margin = 7.5f;

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
            tabButton.text = String.Empty;

            // Name label.
            UILabel tabLabel = tabButton.AddUIComponent<UILabel>();
            tabLabel.autoSize = true;
            tabLabel.textScale = 0.8f;
            tabLabel.text = tabName;
            tabLabel.PerformLayout();

            // Force tab size.
            tabButton.autoSize = false;
            tabButton.width = Mathf.Max(80f, tabLabel.width + Margin * 2f);

            // Centre name label.
            tabLabel.relativePosition = new Vector2((tabButton.width - tabLabel.width) / 2f, (tabButton.height - tabLabel.height ) / 2f);

            // Get tab root panel.
            UIPanel rootPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;

            // Panel setup.
            rootPanel.autoLayout = autoLayout;
            rootPanel.autoLayoutDirection = LayoutDirection.Vertical;
            rootPanel.autoLayoutPadding.top = 5;
            rootPanel.autoLayoutPadding.left = 10;

            return rootPanel;
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
        /// <param name="width">Slider width (excluding value label to right) (default 600)</param>
        /// <returns>New UI slider with attached labels</returns>
        internal static UISlider AddSliderWithValue(UIComponent parent, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback, float width = 600f, float textScale = 1f)
        {
            // Add slider component.
            UIPanel sliderPanel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsSliderTemplate")) as UIPanel;
            sliderPanel.Find<UILabel>("Label").text = text;

            // Label.
            UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
            sliderLabel.autoHeight = true;
            sliderLabel.width = width;
            sliderLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            sliderLabel.relativePosition = Vector3.zero;
            sliderLabel.relativePosition = Vector3.zero;
            sliderLabel.textScale = textScale;
            sliderLabel.text = text;

            // Slider configuration.
            UISlider newSlider = sliderPanel.Find<UISlider>("Slider");
            newSlider.minValue = min;
            newSlider.maxValue = max;
            newSlider.stepSize = step;
            newSlider.value = defaultValue;

            // Move default slider position to match resized label.
            sliderPanel.autoLayout = false;
            newSlider.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            newSlider.relativePosition = PositionUnder(sliderLabel);
            newSlider.width = width;

            // Increase height of panel to accomodate it all plus some extra space for margin.
            sliderPanel.autoSize = false;
            sliderPanel.width = width + 50f;
            sliderPanel.height = newSlider.relativePosition.y + newSlider.height + 15f;

            // Value label.
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
        /// Creates a pushbutton.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Text label</param>
        /// <param name="width">Width (default 100)</param>
        /// <param name="xPos">Relative x position (default 0)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <returns></returns>
        internal static UIButton CreateButton(UIComponent parent, string text, float width = 220f, float xPos = 0f, float yPos = 0f)
        {
            // Constants.
            const float Height = 30f;


            // Create button.
            UIButton button = parent.AddUIComponent<UIButton>();
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.disabledTextColor = new Color32(128, 128, 128, 255);
            button.canFocus = false;

            // Button size parameters.
            button.relativePosition = new Vector3(xPos, yPos);
            button.size = new Vector2(width, Height);
            button.textScale = 0.8f;

            // Label.
            button.text = text;

            return button;
        }


        /// <summary>
        /// Creates a plain checkbox using the game's option panel checkbox template.
        /// Allows for long (multi-line) text labels.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Descriptive label text</param>
        /// <returns></returns>
        internal static UICheckBox AddPlainCheckBox(UIComponent parent, string text)
        {
            UICheckBox checkBox = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate")) as UICheckBox;

            // Override defaults.
            checkBox.autoSize = false;
            checkBox.label.wordWrap = true;
            checkBox.label.autoSize = false;
            checkBox.label.autoHeight = true;
            checkBox.label.width = 700f;

            // Set text.
            checkBox.text = text;

            // Resize height to match text (if text has flowed over multiple lines).
            checkBox.height = checkBox.label.height;

            return checkBox;
        }


        /// <summary>
        /// Creates a plain dropdown using the game's option panel dropdown template.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Descriptive label text</param>
        /// <param name="items">Dropdown menu item list</param>
        /// <param name="selectedIndex">Initially selected index (default 0)</param>
        /// <param name="width">Width of dropdown (default 60)</param>
        /// <returns></returns>
        internal static UIDropDown AddPlainDropDown(UIComponent parent, string text, string[] items, int selectedIndex = 0, float width = 270f)
        {
            UIPanel panel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsDropdownTemplate")) as UIPanel;
            UIDropDown dropDown = panel.Find<UIDropDown>("Dropdown");

            // Set text.
            panel.Find<UILabel>("Label").text = text;

            // Slightly increase width.
            dropDown.autoSize = false;
            dropDown.width = width;

            // Add items.
            dropDown.items = items;
            dropDown.selectedIndex = selectedIndex;

            return dropDown;
        }


        /// <summary>
        /// Returns a relative position below a specified UI component, suitable for placing an adjacent component.
        /// </summary>
        /// <param name="uIComponent">Original (anchor) UI component</param>
        /// <param name="margin">Margin between components</param>
        /// <param name="horizontalOffset">Horizontal offset from first to second component</param>
        /// <returns></returns>
        internal static Vector3 PositionUnder(UIComponent uIComponent, float margin = 8f, float horizontalOffset = 0f)
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
        internal static Vector3 PositionRightOf(UIComponent uIComponent, float margin = 10f, float verticalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }


        /// <summary>
        /// Updates XML configuration file with current settings.
        /// </summary>
        internal static void SaveXML()
        {
            // Write to file.
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionTwo();
                xml.WriteXML(Loading.currentFileLocation);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML configuation file");
            }
        }


        /// <summary>
        /// Finds the named texture atlas.
        /// </summary>
        /// <param name="atlasName">Name to find</param>
        /// <returns>Atlas (default atlas if no name match found)</returns>
        internal static UITextureAtlas GetAtlas(string atlasName)
        {
            // Get active game atalases.
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];

            // Iterate through list, looking for a name match.
            for (int i = 0; i < atlases.Length; ++i)
            {
                if (atlases[i].name.Equals(atlasName))
                {
                    // Match found!  Return the matching atlas.
                    return atlases[i];
                }
            }

            // If we made it this far, we didn't find a match; return the default atlas.
            return UIView.GetAView().defaultAtlas;
        }
    }
}