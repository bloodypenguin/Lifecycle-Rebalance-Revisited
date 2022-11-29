// <copyright file="CalculationOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting mod lifecycle calculation options.
    /// </summary>
    internal sealed class CalculationOptions : OptionsPanelTab
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float TitleMargin = Margin * 2f;

        // Layout index.
        private float currentY = 0f;

        // Key components.
        private UICheckBox _sunsetCheckBox;
        private UICheckBox _legacyCheckBox;
        private UICheckBox _vanillaCheckBox;
        private UICheckBox _retireCheckBox;
        private UICheckBox _childCheckBox;
        private UISlider _retirementSlider;
        private UISlider _schoolStartSlider;
        private UISlider _teenStartSlider;
        private UISlider _youngStartSlider;
        private UILabel _shOnlyLabel1;
        private UILabel _shOnlyLabel2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal CalculationOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            Panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("LBR_SPN"), tabIndex, out _);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!IsSetup)
            {
                // Perform initial setup.
                IsSetup = true;
                Logging.Message("setting up ", this.GetType());

                // Get panel width.
                float maxWidth = Panel.width - (Margin * 2f);

                // Add warning text message.
                UILabel warningLabel = UILabels.AddLabel(Panel, Margin, currentY, Translations.Translate("LBR_SPN_WRN"), textScale: 1.2f);
                warningLabel.font = UIFonts.SemiBold;
                currentY += warningLabel.height;
                warningLabel = UILabels.AddLabel(Panel, Margin, currentY, Translations.Translate("LBR_SPN_BAL"), maxWidth);
                currentY += warningLabel.height;
                warningLabel = UILabels.AddLabel(Panel, Margin, currentY, Translations.Translate("LBR_SPN_BAK"), maxWidth);
                currentY += warningLabel.height + TitleMargin;

                // Calculation models.
                AddTitle(Panel, "LBR_CAL", maxWidth);
                _sunsetCheckBox = AddPlainCheckBox(Panel, "LBR_CAL_SUN", !ModSettings.Settings.LegacyCalcs);
                _legacyCheckBox = AddPlainCheckBox(Panel, "LBR_CAL_LEG", ModSettings.Settings.LegacyCalcs);
                _vanillaCheckBox = AddPlainCheckBox(Panel, "LBR_CAL_VAN", !ModSettings.Settings.VanillaCalcs);
                _vanillaCheckBox.label.textScale = 1.0f;

                // Set calculation model initial states.
                if (ModSettings.Settings.VanillaCalcs)
                {
                    _vanillaCheckBox.isChecked = ModSettings.Settings.VanillaCalcs;
                }
                else if (ModSettings.Settings.LegacyCalcs)
                {
                    _legacyCheckBox.isChecked = ModSettings.Settings.LegacyCalcs;
                }
                else
                {
                    _sunsetCheckBox.isChecked = true;
                }

                // Custom retirement ages.
                AddTitle(Panel, "LBR_RET", maxWidth);
                _retireCheckBox = AddPlainCheckBox(Panel, "LBR_RET_USE", ModSettings.Settings.CustomRetirement);
                currentY += TitleMargin;

                _retirementSlider = AgeSlider("LBR_RET_CUS", ModSettings.MinRetirementYear, ModSettings.MaxRetirementYear, ModSettings.Settings.RetirementYear);
                _retirementSlider.eventValueChanged += (c, value) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.RetirementYear = (int)value;
                };
                _retirementSlider.tooltip = Translations.Translate("LBR_RET_NT1") + System.Environment.NewLine + Translations.Translate("LBR_RET_NT2");

                // Custom childhood.
                AddTitle(Panel, "LBR_CHI", maxWidth);
                _childCheckBox = AddPlainCheckBox(Panel, "LBR_CHI_CUS", ModSettings.Settings.CustomChildhood);
                currentY += TitleMargin;

                _schoolStartSlider = AgeSlider("LBR_CHI_SCH", ModSettings.MinSchoolStartYear, ModSettings.MaxSchoolStartYear, ModSettings.Settings.SchoolStartYear);
                _schoolStartSlider.eventValueChanged += (c, value) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.SchoolStartYear = (int)value;
                };

                _teenStartSlider = AgeSlider("LBR_CHI_TEE", ModSettings.MinTeenStartYear, ModSettings.MaxTeenStartYear, ModSettings.Settings.TeenStartYear);
                _teenStartSlider.eventValueChanged += (c, value) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.TeenStartYear = (int)value;
                };

                _youngStartSlider = AgeSlider("LBR_CHI_YOU", ModSettings.MinYoungStartYear, ModSettings.MaxYoungStartYear, ModSettings.Settings.YoungStartYear);
                _youngStartSlider.eventValueChanged += (c, value) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.YoungStartYear = (int)value;
                };

                // Sunset harbor only labels.
                _shOnlyLabel1 = UILabels.AddLabel(Panel, _retirementSlider.parent.relativePosition.x, _retirementSlider.parent.relativePosition.y, Translations.Translate("LBR_SHO"));
                _shOnlyLabel2 = UILabels.AddLabel(Panel, _schoolStartSlider.parent.relativePosition.x, _schoolStartSlider.parent.relativePosition.y, Translations.Translate("LBR_SHO"));

                // Set initial visibility state of child sliders.
                _schoolStartSlider.parent.isVisible = _childCheckBox.isChecked;
                _teenStartSlider.parent.isVisible = _childCheckBox.isChecked;
                _youngStartSlider.parent.isVisible = _childCheckBox.isChecked;

                // Update our checkbox states visibility status based on current settings.
                UpdateVisibility(ModSettings.Settings.VanillaCalcs ? 2 : ModSettings.Settings.LegacyCalcs ? 1 : 0);

                // Event handlers (here so other controls referenced are all set up prior to referencing in handlers).
                _sunsetCheckBox.eventCheckChanged += (c, isChecked) =>
                {
                    if (isChecked)
                    {
                        // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 0.
                        UpdateVisibility(0);
                    }
                    else if (!_legacyCheckBox.isChecked && !_vanillaCheckBox.isChecked)
                    {
                        // This has been unchecked when no others have been selected; reset it and make no changes.
                        _sunsetCheckBox.isChecked = true;
                    }
                };

                _legacyCheckBox.eventCheckChanged += (c, isChecked) =>
                {
                    if (isChecked)
                    {
                        // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 1.
                        UpdateVisibility(1);
                    }
                    else if (!_sunsetCheckBox.isChecked && !_vanillaCheckBox.isChecked)
                    {
                        // This has been unchecked when no others have been selected; reset it and make no changes.
                        _legacyCheckBox.isChecked = true;
                    }
                };

                _vanillaCheckBox.eventCheckChanged += (c, isChecked) =>
                {
                    if (isChecked)
                    {
                        // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 2.
                        UpdateVisibility(2);
                    }
                    else if (!_sunsetCheckBox.isChecked && !_legacyCheckBox.isChecked)
                    {
                        // This has been unchecked when no others have been selected; reset it and make no changes.
                        _vanillaCheckBox.isChecked = true;
                    }
                };

                _retireCheckBox.eventCheckChanged += (c, isChecked) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.CustomRetirement = isChecked;

                    // Show/hide retirement age slider.
                    _retirementSlider.parent.isVisible = isChecked;
                };

                _childCheckBox.eventCheckChanged += (c, isChecked) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.CustomChildhood = isChecked;

                    // Show/hide childhood age sliders.
                    _schoolStartSlider.parent.isVisible = isChecked;
                    _teenStartSlider.parent.isVisible = isChecked;
                    _youngStartSlider.parent.isVisible = isChecked;
                };
            }
        }

        /// <summary>
        /// Updates calculation method checkboxes based on current selection.
        /// </summary>
        /// <param name="index">Index of currently selected option - 0 = SH calcs, 1 = legacy calcs, 2 = vanilla calcs.</param>
        private void UpdateVisibility(int index)
        {
            // Disable checkboxes other than the selected one.
            if (index != 0)
            {
                _sunsetCheckBox.isChecked = false;

                // Non-SH calculations selected.
                // Hide custom retirement and childhood options.
                _retireCheckBox.Disable();
                _childCheckBox.Disable();
                _retirementSlider.parent.Hide();
                _schoolStartSlider.parent.Hide();
                _teenStartSlider.parent.Hide();
                _youngStartSlider.parent.Hide();

                // Show 'Sunset Harbor only' labels.
                _shOnlyLabel1.Show();
                _shOnlyLabel2.Show();
            }
            else
            {
                // Sunset Harbor calculations selected.
                // Enable custom retirement and childhood options.
                _retireCheckBox.Enable();
                _childCheckBox.Enable();

                // Show custom retirement slider if the selection is checked.
                if (_retireCheckBox.isChecked)
                {
                    _retirementSlider.parent.Show();
                }

                // Show custom childhood slider if the selection is checked.
                if (_childCheckBox.isChecked)
                {
                    _schoolStartSlider.parent.Show();
                    _teenStartSlider.parent.Show();
                    _youngStartSlider.parent.Show();
                }

                // Show 'Sunset Harbor only' labels.
                _shOnlyLabel1.Hide();
                _shOnlyLabel2.Hide();
            }

            // Legacy calcs.
            if (index != 1)
            {
                // Legacy calcs not selected.
                _legacyCheckBox.isChecked = false;
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
                _vanillaCheckBox.isChecked = false;
                ModSettings.Settings.VanillaCalcs = false;
            }
            else
            {
                ModSettings.Settings.VanillaCalcs = true;
            }
        }

        /// <summary>
        /// Adds an age slider with age-in-years displayed dynamically below.
        /// </summary>
        /// <param name="labelKey">Translation key for slider label.</param>
        /// <param name="min">Minimum slider value.</param>
        /// <param name="max">Maximum slider value.</param>
        /// <param name="initialValue">Initial slider value.</param>
        /// <returns>New delay slider with attached game-time label.</returns>
        private UISlider AgeSlider(string labelKey, uint min, uint max, int initialValue)
        {
            // Create new slider.
            UISlider newSlider = UISliders.AddPlainSliderWithValue(Panel, Margin, currentY, Translations.Translate(labelKey), min, max, 1f, initialValue, 700f);

            // Increment y position indicator.
            currentY += newSlider.parent.height - Margin;

            return newSlider;
        }

        /// <summary>
        /// Creates a plain checkbox using the game's option panel checkbox template.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="textKey">Label translation key.</param>
        /// <param name="isChecked">Initial checked state (default false).</param>
        /// <returns>New checkbox using the game's option panel template.</returns>
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
        /// <param name="parent">Parent component.</param>
        /// <param name="titleKey">Title translation key.</param>
        /// <param name="maxWidth">Title spacer maximim width.</param>
        private void AddTitle(UIComponent parent, string titleKey, float maxWidth)
        {
            currentY += Margin;
            UILabel calculationLabel = UISpacers.AddTitleSpacer(parent, Margin, currentY, maxWidth, Translations.Translate(titleKey));
            currentY += calculationLabel.height + (TitleMargin * 2f);
        }
    }
}