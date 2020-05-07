using System;
using UnityEngine;
using ICities;
using ColossalFramework.UI;
using System.Text.RegularExpressions;

namespace LifecycleRebalance
{
    /// <summary>
    /// Class to handle the mod settings options panel.
    /// </summary>
    public class OptionsPanel
    {
        // Components.
        private UICheckBox sunsetCheckbox;
        private UICheckBox legacyCheckbox;
        private UICheckBox retireCheckbox;
        private UISlider vanishingStiffs;
        private UISlider[] illnessChance;

        // Sickness deciles; set to 10 (even though 11 in DataStore) as current WG XML v2 only stores the first 10.
        private const int numDeciles = 10;
        private static float[] defaultSicknessProbs = { 0.0125f, 0.0075f, 0.01f, 0.01f, 0.015f, 0.02f, 0.03f, 0.04f, 0.05f, 0.075f, 0.25f };

        public OptionsPanel(UIHelperBase helper)
        {
            // Load configuration
            SettingsFile settings = Configuration<SettingsFile>.Load();

            UIHelperBase group0 = helper.AddGroup("Lifecycle Balance Revisited v" + LifecycleRebalance.version);

            // Add warning text messages.
            UITextField warningText = (UITextField)group0.AddTextfield("WARNING:\r\nChanging settings during a game can temporarily disrupt city balance.\r\nSaving a backup before changing is HIGHLY recommended.", "", delegate { });
            warningText.Disable();

            // Calculation models.
            UIHelperBase group1 = helper.AddGroup("Lifecycle calculation model");

            sunsetCheckbox = (UICheckBox)group1.AddCheckbox("Use Sunset Harbor lifespans - longer lifespans and more seniors", !settings.UseLegacy, (isChecked) =>
            {
                // There Can Only Be One (selected checkbox in this group).
                legacyCheckbox.isChecked = !isChecked;

                // Update mod settings.
                ModSettings.LegacyCalcs = !isChecked;

                // Update configuration file.
                settings.UseLegacy = !isChecked;
                Configuration<SettingsFile>.Save();
            });

            legacyCheckbox = (UICheckBox)group1.AddCheckbox("Use legacy lifespans (original WG mod) - shorter lifespans and fewer seniors", settings.UseLegacy, (isChecked) =>
            {
                // There Can Only Be One (selected checkbox in this group).
                // Leave all processing to be done by sunsetCheckbox via state change.
                sunsetCheckbox.isChecked = !isChecked;
            });

            // Custom retirement ages.
            UIHelperBase group2 = helper.AddGroup("EXPERIMENTAL FEATURES - Sunset Harbor lifespans only");

            retireCheckbox = (UICheckBox)group2.AddCheckbox("Use custom retirement age (Sunset Harbor lifespans only)", settings.CustomRetirement, (isChecked) =>
            {
                // Update mod settings.
                ModSettings.CustomRetirement = isChecked;

                // Update configuration file.
                settings.CustomRetirement = isChecked;
                Configuration<SettingsFile>.Save();
            });

            group2.AddDropdown("Custom retirement age", new string[] { "50", "55", "60", "65" }, (settings.RetirementYear - 50) / 5, (index) =>
            {
                int ageYears = 50 + (index * 5);

                // Update mod settings.
                ModSettings.RetirementYear = ageYears;

                // Update configuration file.
                settings.RetirementYear = ageYears;
                Configuration<SettingsFile>.Save();
            });

            // Add text about retirement age.
            UITextField retirementText = (UITextField)group2.AddTextfield("Decreasing retirement age won't change the status of citizens who have\r\n        already retired under previous settings.\r\nIncreasing retirement age won't change the appearance of citzens who have\r\n        already retired under previous settings.", "", delegate { });
            retirementText.Disable();

            // Deathcare options.
            UIHelperBase group3 = helper.AddGroup("The dearly departed");

            // Percentage of corpses requiring transport.
            vanishingStiffs = AddSliderWithValue(group3, "% of corpses requiring deathcare transportation (game default 33%)", 0, 100, 1, DataStore.autoDeadRemovalChance, (value) =>
            {
                // Update mod settings.
                DataStore.autoDeadRemovalChance = (int)value;
                Debug.Log("Lifecycle Rebalance Revisited: autoDeadRemovalChance set to: " + (int)value + "%.");

                // Update WG configuration file.
                try
                {
                    WG_XMLBaseVersion xml = new XML_VersionTwo();
                    xml.writeXML(Loading.currentFileLocation);
                }
                catch (Exception e)
                {
                    Debug.Log("Lifecycle Rebalance Revisited: XML writing exception:\r\n" + e.Message);
                }
            });

            // Illness options.
            UIHelperBase group4 = helper.AddGroup("Random illness chance per decade of life");

            // Read XML if we haven't already.
            if (!Loading.isModCreated)
            {
                Loading.readFromXML();
            }

            // Illness chance sliders.
            illnessChance = new UISlider[DataStore.sicknessProbInXML.Length];
            for (int i = 0; i < numDeciles; i++)
            {
                // Note this is using Sunset Harbor ages.  Legacy ages are shorter by around 40% (25/35).
                illnessChance[i] = AddSliderWithValue(group4, "Ages " + (i * 10) + "-" + ((i * 10) + 9), 0, 100, 0.05f, (float)DataStore.sicknessProbInXML[i] * 100, (value) => { });
            }

            // Reset to saved button.
            UIButton illnessResetToSaved = (UIButton)group4.AddButton("Reset to saved", () =>
            {
                for (int i = 0; i < numDeciles; i++)
                {
                    // Retrieve saved values from datastore.
                    illnessChance[i].value = (float)DataStore.sicknessProbInXML[i] * 100;
                }
            });

            // Reset to default button.
            UIButton illnessResetToDefault = (UIButton)group4.AddButton("Reset to default", () =>
            {
                for (int i = 0; i < numDeciles; i++)
                {
                    // Retrieve default values.
                    illnessChance[i].value = defaultSicknessProbs[i] * 100;
                }
            });

            // Save settings button.
            UIButton illnessSave = (UIButton)group4.AddButton("Save", () =>
            {
                // Update datastore with slider values.
                for (int i = 0; i < numDeciles; i++)
                {
                    DataStore.sicknessProbInXML[i] = illnessChance[i].value / 100;
                    DataStore.sicknessProbCalc[i] = (int)(1000 * illnessChance[i].value);
                }

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
            });


            // Logging options.
            UIHelperBase group5 = helper.AddGroup("Logging");

            group5.AddCheckbox("Log deaths to 'Lifecycle death log.txt'", settings.LogDeaths, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseDeathLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: death logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogDeaths = isChecked;
                Configuration<SettingsFile>.Save();
            });
            group5.AddCheckbox("Log immigrants to 'Lifecycle immigration log.txt'", settings.LogImmigrants, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseImmigrationLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: immigrant logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogImmigrants = isChecked;
                Configuration<SettingsFile>.Save();
            });
            group5.AddCheckbox("Log transport choices to 'Lifecycle transport log.txt'    WARNING - SLOW!", settings.LogTransport, (isChecked) =>
            {
                // Update mod settings.
                Debugging.UseTransportLog = isChecked;
                Debug.Log("Lifecycle Rebalance Revisited: transport choices logging " + (settings.UseLegacy ? "enabled." : "disabled."));

                // Update configuration file.
                settings.LogTransport = isChecked;
                Configuration<SettingsFile>.Save();
            });
        }
        
        private static UISlider AddSliderWithValue(UIHelperBase helper, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback)
        {
            // Slider control.
            UISlider newSlider = helper.AddSlider(text, min, max, step, defaultValue, value => { }) as UISlider;

            // Get parent.
            UIPanel parentPanel = newSlider.parent as UIPanel;
            parentPanel.autoLayout = false;

            // Change default slider label position and size.
            UILabel sliderLabel = parentPanel.Find<UILabel>("Label");
            sliderLabel.width = 500;
            sliderLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            sliderLabel.relativePosition = Vector3.zero;

            // Move default slider position to match resized labe.
            newSlider.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            newSlider.relativePosition = PositionUnder(sliderLabel);
            newSlider.width = 500;

            // Value label.
            UILabel valueLabel = parentPanel.AddUIComponent<UILabel>();
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

        private static Vector3 PositionUnder(UIComponent uIComponent, float margin = 8f, float horizontalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + horizontalOffset, uIComponent.relativePosition.y + uIComponent.height + margin);
        }

        private static Vector3 PositionRightOf(UIComponent uIComponent, float margin = 8f, float verticalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }
    }
}