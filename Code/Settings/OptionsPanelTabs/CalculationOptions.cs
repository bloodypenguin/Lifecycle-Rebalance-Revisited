using System;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting mod lifecycle calculation options.
    /// </summary>
    internal class CalculationOptions : OptionsPanelTab
    {
        //
        public static readonly string[] retirementAges = { "50", "55", "60", "65" };

        // Key components.
        private UICheckBox sunsetCheckBox, legacyCheckBox, vanillaCheckBox, retireCheckBox;
        private UIDropDown ageDropDown;

        /// <summary>
        /// Adds calculation options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            panel = PanelUtils.AddTab(tabStrip, Translations.Translate("LBR_SPN"), tabIndex, true);

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
                Logging.Message("setting up ", this.GetType().ToString());


                // Add warning text message.
                PanelUtils.AddLabel(panel, Translations.Translate("LBR_SPN_WRN") + Environment.NewLine + Translations.Translate("LBR_SPN_BAL") + Environment.NewLine + Translations.Translate("LBR_SPN_BAK"));

                // Calculation models.
                PanelUtils.AddPanelSpacer(panel);
                PanelUtils.AddLabel(panel, Translations.Translate("LBR_CAL"), 1.3f);

                sunsetCheckBox = PanelUtils.AddPlainCheckBox(panel, Translations.Translate("LBR_CAL_SUN"));
                sunsetCheckBox.isChecked = !OptionsPanel.settings.UseLegacy;
                legacyCheckBox = PanelUtils.AddPlainCheckBox(panel, Translations.Translate("LBR_CAL_LEG"));
                legacyCheckBox.isChecked = OptionsPanel.settings.UseLegacy;
                vanillaCheckBox = PanelUtils.AddPlainCheckBox(panel, Translations.Translate("LBR_CAL_VAN"));
                vanillaCheckBox.isChecked = OptionsPanel.settings.UseVanilla;

                // Custom retirement ages.
                PanelUtils.AddPanelSpacer(panel);
                PanelUtils.AddLabel(panel, Translations.Translate("LBR_RET"), 1.3f);

                retireCheckBox = PanelUtils.AddPlainCheckBox(panel, Translations.Translate("LBR_RET_USE"));
                retireCheckBox.isChecked = OptionsPanel.settings.CustomRetirement;

                ageDropDown = PanelUtils.AddPlainDropDown(panel, Translations.Translate("LBR_RET_CUS"), retirementAges, (OptionsPanel.settings.RetirementYear - 50) / 5);
                ageDropDown.eventSelectedIndexChanged += (control, index) =>
                {
                    int ageYears = 50 + (index * 5);

                // Update mod settings.
                ModSettings.RetirementYear = ageYears;

                // Update configuration file.
                OptionsPanel.settings.RetirementYear = ageYears;
                    Configuration<SettingsFile>.Save();
                };

                // Add enabled/disabled event handler to age dropdown to repopulate items on re-enabling. 
                ageDropDown.eventIsEnabledChanged += (control, isEnabled) =>
                {
                    if (isEnabled)
                    {
                        ageDropDown.items = retirementAges;
                        ageDropDown.selectedIndex = (OptionsPanel.settings.RetirementYear - 50) / 5;
                    }
                };

                UILabel retireNote1 = PanelUtils.AddLabel(panel, Translations.Translate("LBR_RET_NT1"));
                UILabel retireNote2 = PanelUtils.AddLabel(panel, Translations.Translate("LBR_RET_NT2"));

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
                ModSettings.CustomRetirement = isChecked;

                // Show/hide retirement age dropdown.
                if (isChecked)
                    {
                        ageDropDown.Enable();
                    }
                    else
                    {
                        ageDropDown.Disable();
                    }

                // Update configuration file.
                OptionsPanel.settings.CustomRetirement = isChecked;
                    Configuration<SettingsFile>.Save();
                };

                // Show or hide notes attached to age dropdown to match visibility of dropdown itself.
                ageDropDown.eventIsEnabledChanged += (control, isEnabled) =>
                {
                    if (isEnabled)
                    {
                        retireNote1.Show();
                        retireNote2.Show();
                        ageDropDown.parent.Show();
                    }
                    else
                    {
                        retireNote1.Hide();
                        retireNote2.Hide();
                        ageDropDown.parent.Hide();
                    }
                };

                // Update our visibility status based on current settings.
                UpdateCheckboxes(OptionsPanel.settings.UseVanilla ? 2 : OptionsPanel.settings.UseLegacy ? 1 : 0);
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
                ageDropDown.Disable();
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
                    ageDropDown.Enable();
                }
            }

            // Legacy calcs.
            if (index != 1)
            {
                // Legacy calcs not selected.
                legacyCheckBox.isChecked = false;
                ModSettings.LegacyCalcs = false;
                OptionsPanel.settings.UseLegacy = false;
            }
            else
            {
                // Legacy calcs selected.
                ModSettings.LegacyCalcs = true;
                OptionsPanel.settings.UseLegacy = true;
            }

            // Vanilla calcs.
            if (index != 2)
            {
                // Vanilla calcs not selected.
                vanillaCheckBox.isChecked = false;
                ModSettings.VanillaCalcs = false;
                OptionsPanel.settings.UseVanilla = false;
            }
            else
            {
                // Vanilla calcs selected.
                ModSettings.VanillaCalcs = true;
                OptionsPanel.settings.UseVanilla = true;
            }

            // Save configuration file.
            Configuration<SettingsFile>.Save();
        }
    }
}