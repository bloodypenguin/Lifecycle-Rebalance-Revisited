using System.Linq;
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
        private UICheckBox sunsetCheckBox, legacyCheckBox, vanillaCheckBox, retireCheckBox;
        private UISlider retirementSlider;

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
                UIControls.OptionsSpacer(panel, Margin, currentY, maxWidth);
                currentY += TitleMargin * 2f;
                UILabel calculationLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate("LBR_CAL"), textScale: 1.2f);
                calculationLabel.font = titleFont;
                currentY += calculationLabel.height + TitleMargin;

                sunsetCheckBox = UIControls.AddPlainCheckBox(panel, Translations.Translate("LBR_CAL_SUN"), Margin, currentY, !ModSettings.Settings.LegacyCalcs);
                sunsetCheckBox.label.textScale = 1.0f;
                currentY += sunsetCheckBox.height;

                legacyCheckBox = UIControls.AddPlainCheckBox(panel, Translations.Translate("LBR_CAL_LEG"), Margin, currentY, ModSettings.Settings.LegacyCalcs);
                legacyCheckBox.label.textScale = 1.0f;
                currentY += legacyCheckBox.height;

                vanillaCheckBox = UIControls.AddPlainCheckBox(panel, Translations.Translate("LBR_CAL_VAN"), Margin, currentY, !ModSettings.Settings.VanillaCalcs);
                vanillaCheckBox.label.textScale = 1.0f;
                currentY += vanillaCheckBox.height + TitleMargin;

                // Custom retirement ages.
                UIControls.OptionsSpacer(panel, Margin, currentY, maxWidth);
                currentY += TitleMargin * 2f;
                UILabel retirementLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate("LBR_RET"), textScale: 1.2f);
                retirementLabel.font = titleFont;
                currentY += retirementLabel.height + TitleMargin;

                retireCheckBox = UIControls.AddPlainCheckBox(panel, Translations.Translate("LBR_RET_USE"), Margin, currentY, ModSettings.Settings.CustomRetirement);
                currentY += retireCheckBox.height + Margin;
                
                retirementSlider = AgeSlider("LBR_RET_CUS", 50, 70, ModSettings.Settings.RetirementYear);
                retirementSlider.eventValueChanged += (control, value) =>
                {
                    // Update mod settings.
                    ModSettings.Settings.RetirementYear = (uint)value;

                    // Update configuration file.
                    ModSettings.Save();
                };

                UILabel retireNote1 = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate("LBR_RET_NT1"), maxWidth);
                currentY += retireNote1.height;
                UILabel retireNote2 = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate("LBR_RET_NT2"), maxWidth);
                currentY += retireNote2.height;

                // Event handlers (here so other controls referenced are all set up prior to referencing in handlers).
                sunsetCheckBox.eventCheckChanged += (control, isChecked) =>
                {
                    if (isChecked)
                    {
                        // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 0.
                        UpdateCheckboxes(0);
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
                        UpdateCheckboxes(1);
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
                        UpdateCheckboxes(2);
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
                    if (isChecked)
                    {
                        retirementSlider.Enable();
                    }
                    else
                    {
                        retirementSlider.Disable();
                    }

                    // Update configuration file.
                    ModSettings.Settings.CustomRetirement = isChecked;
                    ModSettings.Save();
                };

                // Show or hide notes attached to retirement slider to match visibility of slider itself.
                retirementSlider.eventIsEnabledChanged += (control, isEnabled) =>
                {
                    if (isEnabled)
                    {
                        retireNote1.Show();
                        retireNote2.Show();
                        retirementSlider.parent.Show();
                    }
                    else
                    {
                        retireNote1.Hide();
                        retireNote2.Hide();
                        retirementSlider.parent.Hide();
                    }
                };

                // Update our visibility status based on current settings.
                UpdateCheckboxes(ModSettings.Settings.VanillaCalcs ? 2 : ModSettings.Settings.LegacyCalcs ? 1 : 0);
            }
        }


        /// <summary>
        /// Updates calculation method checkboxes based on current selection.
        /// </summary>
        /// <param name="index">Index of currently selected option - 0 = SH calcs, 1 = legacy calcs, 2 = vanilla calcs</param>
        private void UpdateCheckboxes(int index)
        {
            // Disable checkboxes other than the selected one.
            if (index != 0)
            {
                sunsetCheckBox.isChecked = false;

                // Non-SH calculations selected.
                // Hide custom retirement options.
                retireCheckBox.Disable();
                retirementSlider.Disable();
            }
            else
            {
                // Sunset Harbor calculations selected.
                // Show custom retirement options.
                retireCheckBox.Enable();
                retireCheckBox.Show();

                // Show custom retirement options if the selection is checked.
                if (retireCheckBox.isChecked)
                {
                    retirementSlider.Enable();
                }
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

            // Save configuration file.
            ModSettings.Save();
        }


        /// <summary>
        /// Adds an age slider with age-in-years displayed dynamically below.
        /// </summary>
        /// <param name="yPos">Relative y-position indicator (will be incremented with slider height</param>
        /// <param name="labelKey">Translation key for slider label</param>
        /// <param name="initialValue">Initial slider value</param>
        /// <returns>New delay slider with attached game-time label</returns>
        private UISlider AgeSlider(string labelKey, uint min, uint max, uint initialValue)
        {
            // Create new slider.
            UISlider newSlider = UIControls.AddSlider(panel, Translations.Translate(labelKey), min, max, 1f, initialValue);
            newSlider.parent.relativePosition = new Vector2(Margin, currentY);

            // Age (in years) label.
            UILabel ageLabel = UIControls.AddLabel(newSlider.parent, Margin, newSlider.parent.height - Margin, string.Empty);
            newSlider.objectUserData = ageLabel;

            // Force set slider value to populate initial age label and add event handler.
            SetAgeLabel(newSlider, initialValue);
            newSlider.eventValueChanged += SetAgeLabel;

            // Increment y position indicator.
            currentY += newSlider.parent.height + ageLabel.height + Margin;

            return newSlider;
        }


        /// <summary>
        /// Sets the age (in years) label for a age slider.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="value"></param>
        private void SetAgeLabel(UIComponent control, float value)
        {
            // Ensure that there's a valid label attached to the slider.
            if (control.objectUserData is UILabel label)
            {
                // Format label to display age in years.
                label.text = string.Format("Age {0} years", value.ToString("n0"));
            }
        }
    }
}