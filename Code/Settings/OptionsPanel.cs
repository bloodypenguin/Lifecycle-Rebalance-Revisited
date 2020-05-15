using System.Text;
using UnityEngine;
using ICities;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Class to handle the mod settings options panel.
    /// </summary>
    public class OptionsPanel
    {
        // Sickness deciles; set to 10 (even though 11 in DataStore) as current WG XML v2 only stores the first 10.
        private const int numDeciles = 10;
        private static float[] defaultSicknessProbs = { 0.0125f, 0.0075f, 0.01f, 0.01f, 0.015f, 0.02f, 0.03f, 0.04f, 0.05f, 0.075f, 0.25f };

        // Settings file.
        private static SettingsFile settings;

        public OptionsPanel(UIHelperBase helper)
        {
            // Load settings.
            settings = Configuration<SettingsFile>.Load();

            // Read configuration XML if we haven't already.
            if (!Loading.isModCreated)
            {
                Loading.readFromXML();
            }

            // Set up tab strip and containers.
            UIScrollablePanel optionsPanel = ((UIHelper)helper).self as UIScrollablePanel;
            optionsPanel.autoLayout = false;

            UITabstrip tabStrip = optionsPanel.AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0, 0);
            tabStrip.size = new Vector2(744, 713);

            UITabContainer tabContainer = optionsPanel.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 40);
            tabContainer.size = new Vector3(744, 713);
            tabStrip.tabPages = tabContainer;

            // Add tabs and panels.
            CalculationsTab(tabStrip, 0);
            DeathTab(tabStrip, 1);
            HealthTab(tabStrip, 2);
            LoggingTab(tabStrip, 3);
        }


        /// <summary>
        /// Adds calculation options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        private static void CalculationsTab(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIHelper calculationsTab = PanelUtils.AddTab(tabStrip, "Calculations", tabIndex);

            UIHelperBase group0 = calculationsTab.AddGroup("Lifecycle Balance Revisited v" + LifecycleRebalance.Version);

            // Add warning text messages.
            UITextField warningText = (UITextField)group0.AddTextfield("WARNING:\r\nChanging settings during a game can temporarily disrupt city balance.\r\nSaving a backup before changing is HIGHLY recommended.", "", delegate { });
            warningText.Disable();

            // Calculation models.
            UIHelperBase group1 = calculationsTab.AddGroup("Lifecycle calculation model");

            UICheckBox sunsetCheckbox = (UICheckBox)group1.AddCheckbox("Use Sunset Harbor lifespans - longer lifespans and more seniors", !settings.UseLegacy, (isChecked) => { });
            UICheckBox legacyCheckbox = (UICheckBox)group1.AddCheckbox("Use legacy lifespans (original WG mod) - shorter lifespans and fewer seniors", settings.UseLegacy, (isChecked) => { });

            // Custom retirement ages.
            UIHelperBase group2 = calculationsTab.AddGroup("EXPERIMENTAL FEATURES - Sunset Harbor lifespans only");

            UICheckBox retireCheckbox = (UICheckBox)group2.AddCheckbox("Use custom retirement age (Sunset Harbor lifespans only)", settings.CustomRetirement, (isChecked) => { });

            UIDropDown ageDropdown = (UIDropDown)group2.AddDropdown("Custom retirement age", new string[] { "50", "55", "60", "65" }, (settings.RetirementYear - 50) / 5, (index) =>
            {
                int ageYears = 50 + (index * 5);

                // Update mod settings.
                ModSettings.RetirementYear = ageYears;

                // Update configuration file.
                settings.RetirementYear = ageYears;
                Configuration<SettingsFile>.Save();
            });

            UILabel retireNote1 = PanelUtils.AddLabel((UIPanel)ageDropdown.parent, "Decreasing retirement age won't change the status of citizens who have already retired under previous settings.");
            UILabel retireNote2 = PanelUtils.AddLabel((UIPanel)ageDropdown.parent, "Increasing retirement age won't change the appearance of citzens who have already retired under previous settings.");

            // Show/hide controls based on initial settings.
            if (!settings.CustomRetirement)
            {
                ageDropdown.Disable();
                ageDropdown.Hide();
                retireNote1.Hide();
                retireNote2.Hide();
            }

            if (settings.UseLegacy)
            {
                retireCheckbox.Disable();
                ageDropdown.Disable();
                retireCheckbox.parent.Hide();
            }

            // Event handlers (here so other controls referenced are all set up prior to referencing in handlers).
            sunsetCheckbox.eventCheckChanged += (control, isChecked) =>
            {
                // There Can Only Be One (selected checkbox in this group).
                legacyCheckbox.isChecked = !isChecked;

                // Update mod settings.
                ModSettings.LegacyCalcs = !isChecked;

                // Update configuration file.
                settings.UseLegacy = !isChecked;
                Configuration<SettingsFile>.Save();

                retireCheckbox.parent.Hide();

                // Show custom retirement age options.
                retireCheckbox.Enable();
                ageDropdown.Enable();
                retireCheckbox.parent.Show();
            };

            legacyCheckbox.eventCheckChanged += (control, isChecked) =>
            {
                // There Can Only Be One (selected checkbox in this group).
                // Leave all processing to be done by sunsetCheckbox via state change.
                sunsetCheckbox.isChecked = !isChecked;

                // Hide custom retirement options.
                retireCheckbox.Disable();
                ageDropdown.Disable();
                retireCheckbox.parent.Hide();
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
                    retireNote1.Show();
                    retireNote2.Show();
                }
                else
                {
                    ageDropdown.Disable();
                    ageDropdown.Hide();
                    retireNote1.Hide();
                    retireNote2.Hide();
                }

                // Update configuration file.
                settings.CustomRetirement = isChecked;
                Configuration<SettingsFile>.Save();
            };
        }


        /// <summary>
        /// Adds death options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        private static void DeathTab(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIHelper deathTab = PanelUtils.AddTab(tabStrip, "Death", tabIndex);

            // Percentage of corpses requiring transport.  % of bodies requiring transport is more intuitive to user than % of vanishing corpses, so we invert the value.
            UISlider vanishingStiffs = PanelUtils.AddSliderWithValue(deathTab, "% of dead bodies requiring deathcare transportation\r\n(Game default 67%, mod default 50%)", 0, 100, 1, 100 - DataStore.autoDeadRemovalChance, (value) => { });

            // Reset to saved button.
            UIButton vanishingStiffReset = (UIButton)deathTab.AddButton("Reset to saved", () =>
            {
                // Retrieve saved value from datastore - inverted value (see above).
                vanishingStiffs.value = 100 - DataStore.autoDeadRemovalChance;
            });

            // Turn off autolayout to fit next button to the right at the same y-value and increase button Y-value to clear slider.
            ((UIPanel)vanishingStiffReset.parent).autoLayout = false;
            ((UIPanel)vanishingStiffReset.parent).height += 20;
            vanishingStiffReset.relativePosition = new Vector3(vanishingStiffReset.relativePosition.x, vanishingStiffReset.relativePosition.y + 30);

            // Save settings button.
            UIButton vanishingStiffsSave = (UIButton)deathTab.AddButton("Save and apply", () =>
            {
                // Update mod settings - inverted value (see above).
                DataStore.autoDeadRemovalChance = 100 - (int)vanishingStiffs.value;
                Debug.Log("Lifecycle Rebalance Revisited: autoDeadRemovalChance set to: " + DataStore.autoDeadRemovalChance + "%.");

                // Update WG configuration file.
                PanelUtils.SaveXML();
            });
            vanishingStiffsSave.relativePosition = PanelUtils.PositionRightOf(vanishingStiffReset);
        }


        /// <summary>
        /// Adds health options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        private static void HealthTab(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIHelper healthTab = PanelUtils.AddTab(tabStrip, "Health", tabIndex);

            // Illness options.
            UIHelperBase healthGroup = healthTab.AddGroup("Random illness % chance per decade of life\r\nDoes not affect sickness from specific causes, e.g. pollution or noise.");

            // Illness chance sliders.
            UISlider[] illnessChance = new UISlider[DataStore.sicknessProbInXML.Length];
            for (int i = 0; i < numDeciles; i++)
            {
                // Note this is using Sunset Harbor ages.  Legacy ages are shorter by around 40% (25/35).
                illnessChance[i] = PanelUtils.AddSliderWithValue(healthGroup, "Ages " + (i * 10) + "-" + ((i * 10) + 9) + " (default " + (defaultSicknessProbs[i] * 100) + ")", 0, 25, 0.05f, (float)DataStore.sicknessProbInXML[i] * 100, (value) => { });
            }

            // Reset to saved button.
            UIButton illnessResetSaved = (UIButton)healthGroup.AddButton("Reset to saved", () =>
            {
                for (int i = 0; i < numDeciles; i++)
                {
                    // Retrieve saved values from datastore.
                    illnessChance[i].value = (float)DataStore.sicknessProbInXML[i] * 100;
                }
            });

            // Save settings button.
            UIButton illnessSave = (UIButton)healthGroup.AddButton("Save and apply", () =>
            {
                StringBuilder logMessage = new StringBuilder("Lifecycle Rebalance Revisited: sickness probability table using factor of " + ModSettings.decadeFactor + ":\r\n");

                // Update datastore with slider values.
                for (int i = 0; i < numDeciles; i++)
                {
                    DataStore.sicknessProbInXML[i] = illnessChance[i].value / 100;

                    // Recalculate probabilities if the mod is loaded.
                    if (Loading.isModCreated)
                    {
                        Loading.CalculateSicknessProbabilities();
                    }
                }

                // Write to file.
                PanelUtils.SaveXML();
            });

            // Turn off autolayout to fit next buttons to the right of the illness saved button.
            ((UIPanel)illnessResetSaved.parent).autoLayout = false;

            // Reset to default button.
            UIButton illnessResetDefault = (UIButton)healthGroup.AddButton("Reset to default", () =>
            {
                for (int i = 0; i < numDeciles; i++)
                {
                    // Retrieve default values.
                    illnessChance[i].value = defaultSicknessProbs[i] * 100;
                }
            });
            illnessResetDefault.relativePosition = PanelUtils.PositionRightOf(illnessResetSaved);

            // Reset to default button.
            UIButton illnessSetZero = (UIButton)healthGroup.AddButton("Set all to zero", () =>
            {
                for (int i = 0; i < numDeciles; i++)
                {
                    // Reset everything to zero.
                    illnessChance[i].value = 0;
                }
            });
            illnessSetZero.relativePosition = PanelUtils.PositionRightOf(illnessResetDefault);
        }


        /// <summary>
        /// Adds logging options tab to tabstrip.
        /// </summary
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        private static void LoggingTab(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIHelper loggingTab = PanelUtils.AddTab(tabStrip, "Logging", tabIndex);

            // Logging options.
            loggingTab.AddCheckbox("Log deaths to 'Lifecycle death log.txt'", settings.LogDeaths, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseDeathLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: death logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogDeaths = isChecked;
                Configuration<SettingsFile>.Save();
            });
            loggingTab.AddCheckbox("Log immigrants to 'Lifecycle immigration log.txt'", settings.LogImmigrants, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseImmigrationLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: immigrant logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogImmigrants = isChecked;
                Configuration<SettingsFile>.Save();
            });
            loggingTab.AddCheckbox("Log transport choices to 'Lifecycle transport log.txt'    WARNING - SLOW!", settings.LogTransport, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseTransportLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: transport choices logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogTransport = isChecked;
                Configuration<SettingsFile>.Save();
            });
            loggingTab.AddCheckbox("Log sickness events to 'Lifecycle sickness log.txt'", settings.LogSickness, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseSicknessLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: sickness logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogSickness = isChecked;
                Configuration<SettingsFile>.Save();
            });
        }
    }
}