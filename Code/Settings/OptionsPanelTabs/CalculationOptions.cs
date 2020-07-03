using ICities;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting mod lifecycle calculation options.
    /// </summary>
    public class CalculationOptions
    {
        //
        public static readonly string[] retirementAges = { "50", "55", "60", "65" };

        // Key components.
        private UICheckBox sunsetCheckBox;
        private UICheckBox legacyCheckBox;
        private UICheckBox vanillaCheckBox;
        private UICheckBox retireCheckBox;
        private UIDropDown ageDropDown;

        /// <summary>
        /// Adds calculation options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public CalculationOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel calculationsTab = PanelUtils.AddTab(tabStrip, "Calculations", tabIndex, true);

            // Add warning text message.
            PanelUtils.AddLabel(calculationsTab, "WARNING:\r\nChanging settings during a game can temporarily disrupt city balance.\r\nSaving a backup before changing is HIGHLY recommended.");

            // Calculation models.
            PanelUtils.AddPanelSpacer(calculationsTab);
            PanelUtils.AddLabel(calculationsTab, "Lifecycle calculation model", 1.3f);

            sunsetCheckBox = PanelUtils.AddPlainCheckBox(calculationsTab, "Mod Sunset Harbor lifespans (default)");
            sunsetCheckBox.isChecked = !OptionsPanel.settings.UseLegacy;
            legacyCheckBox = PanelUtils.AddPlainCheckBox(calculationsTab, "Mod legacy lifespans (original WG mod) - shorter lifespans, fewer seniors");
            legacyCheckBox.isChecked = OptionsPanel.settings.UseLegacy;
            vanillaCheckBox = PanelUtils.AddPlainCheckBox(calculationsTab, "Vanilla Sunset Harbor lifespans - less variable lifespans, slightly more seniors");
            vanillaCheckBox.isChecked = OptionsPanel.settings.UseVanilla;

            // Custom retirement ages.
            PanelUtils.AddPanelSpacer(calculationsTab);
            PanelUtils.AddLabel(calculationsTab, "Retirement age options (only when using mod's Sunset Harbor lifespans)", 1.3f);

            retireCheckBox = PanelUtils.AddPlainCheckBox(calculationsTab, "Use custom retirement age");
            retireCheckBox.isChecked = OptionsPanel.settings.CustomRetirement;

            ageDropDown = PanelUtils.AddPlainDropDown(calculationsTab, "Custom retirement age", retirementAges, (OptionsPanel.settings.RetirementYear - 50) / 5);
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

            UILabel retireNote1 = PanelUtils.AddLabel(calculationsTab, "Decreasing retirement age won't change the status of citizens who have already retired under previous settings.");
            UILabel retireNote2 = PanelUtils.AddLabel(calculationsTab, "Increasing retirement age won't change the appearance of citzens who have already retired under previous settings.");

            // Show/hide controls based on initial settings.
            if (!OptionsPanel.settings.CustomRetirement)
            {
                ageDropDown.Disable();
                ageDropDown.Hide();
            }

            UpdateCheckboxes(OptionsPanel.settings.UseVanilla ? 2 : OptionsPanel.settings.UseLegacy ? 1 : 0);


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
                    ageDropDown.Show();
                }
                else
                {
                    ageDropDown.Disable();
                    ageDropDown.Hide();
                }

                // Update configuration file.
                OptionsPanel.settings.CustomRetirement = isChecked;
                Configuration<SettingsFile>.Save();
            };

            // Show or hide notes attached to age dropdown to match visibility of dropdown itself.
            ageDropDown.eventVisibilityChanged += delegate
            {
                if (ageDropDown.isVisible)
                {
                    retireNote1.Show();
                    retireNote2.Show();
                }
                else
                {
                    retireNote1.Hide();
                    retireNote2.Hide();
                }
            };
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
                retireCheckBox.Hide();
            }
            else
            {
                // Sunset Harbor calculations selected.
                // Show custom retirement options.
                retireCheckBox.Enable();
                ageDropDown.Enable();
                retireCheckBox.Show();
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