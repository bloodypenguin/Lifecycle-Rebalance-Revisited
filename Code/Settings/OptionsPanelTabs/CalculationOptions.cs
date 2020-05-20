using ICities;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting mod lifecycle calculation options.
    /// </summary>
    public class CalculationOptions
    {
        // Key components.
        private UICheckBox sunsetCheckbox;
        private UICheckBox legacyCheckbox;
        private UICheckBox vanillaCheckbox;
        private UICheckBox retireCheckbox;
        private UIDropDown ageDropdown;

        /// <summary>
        /// Adds calculation options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public CalculationOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIHelper calculationsTab = PanelUtils.AddTab(tabStrip, "Calculations", tabIndex);

            UIHelperBase group0 = calculationsTab.AddGroup("Lifecycle Balance Revisited v" + LifecycleRebalance.Version);

            // Add warning text messages.
            UITextField warningText = (UITextField)group0.AddTextfield("WARNING:\r\nChanging settings during a game can temporarily disrupt city balance.\r\nSaving a backup before changing is HIGHLY recommended.", "", delegate { });
            warningText.Disable();

            // Calculation models.
            UIHelperBase group1 = calculationsTab.AddGroup("Lifecycle calculation model");

            sunsetCheckbox = (UICheckBox)group1.AddCheckbox("Use mod's Sunset Harbor lifespans (default)", !OptionsPanel.settings.UseLegacy, (isChecked) => { });
            legacyCheckbox = (UICheckBox)group1.AddCheckbox("Use mod's legacy lifespans (original WG mod) - shorter lifespans, fewer seniors", OptionsPanel.settings.UseLegacy, (isChecked) => { });
            vanillaCheckbox = (UICheckBox)group1.AddCheckbox("Use vanilla Sunset Harbor lifespans - less variable lifespans and slightly more seniors", OptionsPanel.settings.UseVanilla, (isChecked) => { });

            // Custom retirement ages.
            UIHelperBase group2 = calculationsTab.AddGroup("Retirement age options (only when using mod's Sunset Harbor lifespans)");

            retireCheckbox = (UICheckBox)group2.AddCheckbox("Use custom retirement age", OptionsPanel.settings.CustomRetirement, (isChecked) => { });

            ageDropdown = (UIDropDown)group2.AddDropdown("Custom retirement age", new string[] { "50", "55", "60", "65" }, (OptionsPanel.settings.RetirementYear - 50) / 5, (index) =>
            {
                int ageYears = 50 + (index * 5);

                // Update mod settings.
                ModSettings.RetirementYear = ageYears;
                
                // Update configuration file.
                OptionsPanel.settings.RetirementYear = ageYears;
                Configuration<SettingsFile>.Save();
            });

            UILabel retireNote1 = PanelUtils.AddLabel((UIPanel)ageDropdown.parent, "Decreasing retirement age won't change the status of citizens who have already retired under previous settings.");
            UILabel retireNote2 = PanelUtils.AddLabel((UIPanel)ageDropdown.parent, "Increasing retirement age won't change the appearance of citzens who have already retired under previous settings.");

            // Show/hide controls based on initial settings.
            if (!OptionsPanel.settings.CustomRetirement)
            {
                ageDropdown.Disable();
                ageDropdown.Hide();
            }

            UpdateCheckboxes(OptionsPanel.settings.UseVanilla ? 2 : OptionsPanel.settings.UseLegacy ? 1 : 0);


            // Event handlers (here so other controls referenced are all set up prior to referencing in handlers).
            sunsetCheckbox.eventCheckChanged += (control, isChecked) =>
            {
                if (isChecked)
                {
                // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 0.
                UpdateCheckboxes(0);
                }
                else if (!legacyCheckbox.isChecked && !vanillaCheckbox.isChecked)
                {
                // This has been unchecked when no others have been selected; reset it and make no changes.
                sunsetCheckbox.isChecked = true;
                }
            };

            legacyCheckbox.eventCheckChanged += (control, isChecked) =>
            {
                if (isChecked)
                {
                // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 1.
                UpdateCheckboxes(1);
                }
                else if (!sunsetCheckbox.isChecked && !vanillaCheckbox.isChecked)
                {
                // This has been unchecked when no others have been selected; reset it and make no changes.
                legacyCheckbox.isChecked = true;
                }
            };

            vanillaCheckbox.eventCheckChanged += (control, isChecked) =>
            {
                if (isChecked)
                {
                // If this has been checked, update group checkboxes and set configuration - index for this checkbox is 2.
                UpdateCheckboxes(2);
                }
                else if (!sunsetCheckbox.isChecked && !legacyCheckbox.isChecked)
                {
                // This has been unchecked when no others have been selected; reset it and make no changes.
                vanillaCheckbox.isChecked = true;
                }
            };

            retireCheckbox.eventCheckChanged += (control, isChecked) =>
            {
            // Update mod settings.
            ModSettings.CustomRetirement = isChecked;

            // Show/hide retirement age dropdown.
            if (isChecked)
                {
                    ageDropdown.Enable();
                    ageDropdown.Show();
                }
                else
                {
                    ageDropdown.Disable();
                    ageDropdown.Hide();
                }

                // Update configuration file.
                OptionsPanel.settings.CustomRetirement = isChecked;
                Configuration<SettingsFile>.Save();
            };

            // Show or hide notes attached to age dropdown to match visibility of dropdown itself.
            ageDropdown.eventVisibilityChanged += delegate
            {
                if (ageDropdown.isVisible)
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
                sunsetCheckbox.isChecked = false;

                // Non-SH calculations selected.
                // Hide custom retirement options.
                retireCheckbox.Disable();
                ageDropdown.Disable();
                retireCheckbox.parent.Hide();
            }
            else
            {
                // Sunset Harbor calculations selected.
                // Show custom retirement options.
                retireCheckbox.Enable();
                ageDropdown.Enable();
                retireCheckbox.parent.Show();
            }

            // Legacy calcs.
            if (index != 1)
            {
                // Legacy calcs not selected.
                legacyCheckbox.isChecked = false;
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
                vanillaCheckbox.isChecked = false;
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