﻿using System.Linq;
using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting mod lifecycle calculation options.
    /// </summary>
    internal class CalculationOptions : OptionsPanelTab
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float TitleMargin = Margin * 2f;

        // Layout index.
        private float currentY = 0f;

        // Key components.
        private UICheckBox sunsetCheckBox, legacyCheckBox, vanillaCheckBox, retireCheckBox, childCheckBox;
        private UISlider retirementSlider, schoolStartSlider, teenStartSlider, youngStartSlider;
        private UILabel shOnlyLabel1, shOnlyLabel2;

        /// <summary>
        /// Adds calculation options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            panel = PanelUtils.AddTab(tabStrip, Translations.Translate("LBR_SPN"), tabIndex);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }


        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!isSetup)
            {
                // Perform initial setup.
                isSetup = true;
                Logging.Message("setting up ", this.GetType());

                // Get panel width.
                float maxWidth = panel.width - (Margin * 2f);

                // Get title font.
                UIFont titleFont = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Semibold");

                // Add warning text message.
                UILabel warningLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate("LBR_SPN_WRN"), textScale: 1.2f);
                warningLabel.font = titleFont;
                currentY += warningLabel.height;
                warningLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate("LBR_SPN_BAL"), maxWidth);
                currentY += warningLabel.height;
                warningLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate("LBR_SPN_BAK"), maxWidth);
                currentY += warningLabel.height + TitleMargin;

                // Calculation models.
                AddTitle(panel, "LBR_CAL", titleFont, maxWidth);
                sunsetCheckBox = AddPlainCheckBox(panel, "LBR_CAL_SUN", !ModSettings.Settings.LegacyCalcs);
                legacyCheckBox = AddPlainCheckBox(panel, "LBR_CAL_LEG", ModSettings.Settings.LegacyCalcs);
                vanillaCheckBox = AddPlainCheckBox(panel, "LBR_CAL_VAN", !ModSettings.Settings.VanillaCalcs);
                vanillaCheckBox.label.textScale = 1.0f;

                // Custom retirement ages.
                AddTitle(panel, "LBR_RET", titleFont, maxWidth);
                retireCheckBox = AddPlainCheckBox(panel, "LBR_RET_USE", ModSettings.Settings.CustomRetirement);
                currentY += TitleMargin;

                retirementSlider = AgeSlider("LBR_RET_CUS", ModSettings.MinRetirementYear, ModSettings.MaxRetirementYear, ModSettings.Settings.RetirementYear);
                retirementSlider.eventValueChanged += (control, value) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.RetirementYear = (int)value;
                };
                retirementSlider.tooltip = Translations.Translate("LBR_RET_NT1") + System.Environment.NewLine + Translations.Translate("LBR_RET_NT2");

                // Custom childhood.
                AddTitle(panel, "LBR_CHI", titleFont, maxWidth);
                childCheckBox = AddPlainCheckBox(panel, "LBR_CHI_CUS", ModSettings.Settings.CustomChildhood);
                currentY += TitleMargin;

                schoolStartSlider = AgeSlider("LBR_CHI_SCH", ModSettings.MinSchoolStartYear, ModSettings.MaxSchoolStartYear, ModSettings.Settings.SchoolStartYear);
                schoolStartSlider.eventValueChanged += (control, value) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.SchoolStartYear = (int)value;
                };

                teenStartSlider = AgeSlider("LBR_CHI_TEE", ModSettings.MinTeenStartYear, ModSettings.MaxTeenStartYear, ModSettings.Settings.TeenStartYear);
                teenStartSlider.eventValueChanged += (control, value) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.TeenStartYear = (int)value;
                };

                youngStartSlider = AgeSlider("LBR_CHI_YOU", ModSettings.MinYoungStartYear, ModSettings.MaxYoungStartYear, ModSettings.Settings.YoungStartYear);
                youngStartSlider.eventValueChanged += (control, value) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.YoungStartYear = (int)value;
                };

                // Sunset harbor only labels.
                shOnlyLabel1 = UIControls.AddLabel(panel, retirementSlider.parent.relativePosition.x, retirementSlider.parent.relativePosition.y, Translations.Translate("LBR_SHO"));
                shOnlyLabel2 = UIControls.AddLabel(panel, schoolStartSlider.parent.relativePosition.x, schoolStartSlider.parent.relativePosition.y, Translations.Translate("LBR_SHO"));

                // Set initial visibility state of child sliders.
                schoolStartSlider.parent.isVisible = childCheckBox.isChecked;
                teenStartSlider.parent.isVisible = childCheckBox.isChecked;
                youngStartSlider.parent.isVisible = childCheckBox.isChecked;

                // Event handlers (here so other controls referenced are all set up prior to referencing in handlers).
                sunsetCheckBox.eventCheckChanged += (control, isChecked) =>
                {
                    if (isChecked)
                    {
                        // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 0.
                        UpdateVisibility(0);
                    }
                    else if (!legacyCheckBox.isChecked && !vanillaCheckBox.isChecked)
                    {
                        // This has been unchecked when no others have been selected; reset it and make no changes.
                        sunsetCheckBox.isChecked = true;
                    }
                };

                legacyCheckBox.eventCheckChanged += (control, isChecked) =>
                {
                    if (isChecked)
                    {
                        // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 1.
                        UpdateVisibility(1);
                    }
                    else if (!sunsetCheckBox.isChecked && !vanillaCheckBox.isChecked)
                    {
                        // This has been unchecked when no others have been selected; reset it and make no changes.
                        legacyCheckBox.isChecked = true;
                    }
                };

                vanillaCheckBox.eventCheckChanged += (control, isChecked) =>
                {
                    if (isChecked)
                    {
                        // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 2.
                        UpdateVisibility(2);
                    }
                    else if (!sunsetCheckBox.isChecked && !legacyCheckBox.isChecked)
                    {
                        // This has been unchecked when no others have been selected; reset it and make no changes.
                        vanillaCheckBox.isChecked = true;
                    }
                };

                retireCheckBox.eventCheckChanged += (control, isChecked) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.CustomRetirement = isChecked;

                    // Show/hide retirement age slider.
                    retirementSlider.parent.isVisible = isChecked;
                };

                childCheckBox.eventCheckChanged += (control, isChecked) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.CustomChildhood = isChecked;

                    // Show/hide childhood age sliders.
                    schoolStartSlider.parent.isVisible = isChecked;
                    teenStartSlider.parent.isVisible = isChecked;
                    youngStartSlider.parent.isVisible = isChecked;
                };

                // Update our visibility status based on current settings.
                UpdateVisibility(ModSettings.Settings.VanillaCalcs ? 2 : ModSettings.Settings.LegacyCalcs ? 1 : 0);
            }
        }


        /// <summary>
        /// Updates calculation method checkboxes based on current selection.
        /// </summary>
        /// <param name="index">Index of currently selected option - 0 = SH calcs, 1 = legacy calcs, 2 = vanilla calcs</param>
        private void UpdateVisibility(int index)
        {
            // Disable checkboxes other than the selected one.
            if (index != 0)
            {
                sunsetCheckBox.isChecked = false;

                // Non-SH calculations selected.
                // Hide custom retirement and childhood options.
                retireCheckBox.Disable();
                childCheckBox.Disable();
                retirementSlider.parent.Hide();
                schoolStartSlider.parent.Hide();
                teenStartSlider.parent.Hide();
                youngStartSlider.parent.Hide();

                // Show 'Sunset Harbor only' labels.
                shOnlyLabel1.Show();
                shOnlyLabel2.Show();
            }
            else
            {
                // Sunset Harbor calculations selected.
                // Enable custom retirement and childhood options.
                retireCheckBox.Enable();
                childCheckBox.Enable();

                // Show custom retirement slider if the selection is checked.
                if (retireCheckBox.isChecked)
                {
                    retirementSlider.parent.Show();
                }

                // Show custom childhood slider if the selection is checked.
                if (childCheckBox.isChecked)
                {
                    schoolStartSlider.parent.Show();
                    teenStartSlider.parent.Show();
                    youngStartSlider.parent.Show();
                }

                // Show 'Sunset Harbor only' labels.
                shOnlyLabel1.Hide();
                shOnlyLabel2.Hide();
            }

            // Legacy calcs.
            if (index != 1)
            {
                // Legacy calcs not selected.
                legacyCheckBox.isChecked = false;
                ModSettings.Settings.LegacyCalcs = false;
            }
            else
            {
                // Legacy calcs selected.
                ModSettings.Settings.LegacyCalcs = true;
            }

            // Vanilla calcs.
            if (index != 2)
            {
                // Vanilla calcs not selected.
                vanillaCheckBox.isChecked = false;
                ModSettings.Settings.VanillaCalcs = false;
            }
            else
            {
                // Vanilla calcs selected.
                ModSettings.Settings.VanillaCalcs = true;
            }
        }


        /// <summary>
        /// Adds an age slider with age-in-years displayed dynamically below.
        /// </summary>
        /// <param name="yPos">Relative y-position indicator (will be incremented with slider height</param>
        /// <param name="labelKey">Translation key for slider label</param>
        /// <param name="initialValue">Initial slider value</param>
        /// <returns>New delay slider with attached game-time label</returns>
        private UISlider AgeSlider(string labelKey, uint min, uint max, int initialValue)
        {
            // Create new slider.
            UISlider newSlider = UIControls.AddSliderWithValue(panel, Translations.Translate(labelKey), min, max, 1f, initialValue, 700f);
            newSlider.parent.relativePosition = new Vector2(Margin, currentY);

            // Increment y position indicator.
            currentY += newSlider.parent.height - Margin;

            return newSlider;
        }


        /// <summary>
        /// Creates a plain checkbox using the game's option panel checkbox template.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="textKey">Label translation key</param>
        /// <param name="isChecked">Initial checked state (default false)</param>
        /// <returns>New checkbox using the game's option panel template</returns>
        private UICheckBox AddPlainCheckBox(UIComponent parent, string textKey, bool isChecked = false)
        {
            UICheckBox checkBox = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate")) as UICheckBox;

            // Override defaults.
            checkBox.autoSize = false;
            checkBox.label.wordWrap = true;
            checkBox.label.autoSize = false;
            checkBox.label.autoHeight = true;
            checkBox.label.width = 700f;

            // Set text.
            checkBox.text = Translations.Translate(textKey);

            // Set relative position.
            checkBox.relativePosition = new Vector2(Margin, currentY);

            // Set checked state.
            checkBox.isChecked = isChecked;

            // Resize height to match text (if text has flowed over multiple lines).
            checkBox.height = checkBox.label.height;

            // Increment y position indicator.
            currentY += checkBox.height;

            return checkBox;
        }


        /// <summary>
        /// Adds a spacer and new title to the given panel.
        /// </summary>
        /// <param name="titleKey">Title translation key</param>
        /// <param name="titleFont">Title font</param>
        private void AddTitle(UIComponent parent, string titleKey, UIFont titleFont, float maxWidth)
        {
            currentY += Margin;
            UIControls.OptionsSpacer(parent, Margin, currentY, maxWidth);
            currentY += TitleMargin * 2f;
            UILabel calculationLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate(titleKey), textScale: 1.2f);
            calculationLabel.font = titleFont;
            currentY += calculationLabel.height + TitleMargin;
        }
    }
}