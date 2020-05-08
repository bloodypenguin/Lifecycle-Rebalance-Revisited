using System;
using UnityEngine;
using ICities;
using ColossalFramework.UI;
using System.Text.RegularExpressions;
using System.ComponentModel;

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
            // Load settings.
            SettingsFile settings = Configuration<SettingsFile>.Load();

            // Read configuration XML if we haven't already.
            if (!Loading.isModCreated)
            {
                Loading.readFromXML();
            }

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

            UIDropDown ageDropdown = (UIDropDown)group2.AddDropdown("Custom retirement age", new string[] { "50", "55", "60", "65" }, (settings.RetirementYear - 50) / 5, (index) =>
            {
                int ageYears = 50 + (index * 5);

                // Update mod settings.
                ModSettings.RetirementYear = ageYears;

                // Update configuration file.
                settings.RetirementYear = ageYears;
                Configuration<SettingsFile>.Save();
            });

            AddLabel((UIPanel)ageDropdown.parent, "Decreasing retirement age won't change the status of citizens who have already retired under previous settings.");
            AddLabel((UIPanel)ageDropdown.parent, "Increasing retirement age won't change the appearance of citzens who have already retired under previous settings.");

            // Deathcare options.
            UIHelperBase group3 = helper.AddGroup("The dearly departed");

            // Percentage of corpses requiring transport.  % of bodies requiring transport is more intuitive to user than % of vanishing corpses, so we invert the value.
            vanishingStiffs = AddSliderWithValue(group3, "% of dead bodies requiring deathcare transportation\r\n(Game default 67%, mod default 50%)", 0, 100, 1, 100 - DataStore.autoDeadRemovalChance, (value) => { });

            // Reset to saved button.
            UIButton vanishingStiffReset = (UIButton)group3.AddButton("Reset to saved", () =>
            {
                // Retrieve saved value from datastore - inverted value (see above).
                vanishingStiffs.value = 100 - DataStore.autoDeadRemovalChance;
            });

            // Turn off autolayout to fit next button to the right at the same y-value and increase button Y-value to clear slider.
            ((UIPanel)vanishingStiffReset.parent).autoLayout = false;
            ((UIPanel)vanishingStiffReset.parent).height += 20;
            vanishingStiffReset.relativePosition = new Vector3(vanishingStiffReset.relativePosition.x, vanishingStiffReset.relativePosition.y + 30);

            // Save settings button.
            UIButton vanishingStiffsSave = (UIButton)group3.AddButton("Save and apply", () =>
            {
                // Update mod settings - inverted value (see above).
                DataStore.autoDeadRemovalChance = 100 - (int)vanishingStiffs.value;
                Debug.Log("Lifecycle Rebalance Revisited: autoDeadRemovalChance set to: " + DataStore.autoDeadRemovalChance + "%.");

                // Update WG configuration file.
                SaveXML();
            });
            vanishingStiffsSave.relativePosition = PositionRightOf(vanishingStiffReset);

            // Illness options.
            UIHelperBase group4 = helper.AddGroup("Random illness % chance per decade of life\r\nDoes not affect sickness from specific causes, e.g. pollution or noise.");

            // Illness chance sliders.
            illnessChance = new UISlider[DataStore.sicknessProbInXML.Length];
            for (int i = 0; i < numDeciles; i++)
            {
                // Note this is using Sunset Harbor ages.  Legacy ages are shorter by around 40% (25/35).
                illnessChance[i] = AddSliderWithValue(group4, "Ages " + (i * 10) + "-" + ((i * 10) + 9) + " (default " + (defaultSicknessProbs[i] * 100) + ")", 0, 25, 0.05f, (float)DataStore.sicknessProbInXML[i] * 100, (value) => { });
            }

            // Reset to saved button.
            UIButton illnessResetSaved = (UIButton)group4.AddButton("Reset to saved", () =>
            {
                for (int i = 0; i < numDeciles; i++)
                {
                    // Retrieve saved values from datastore.
                    illnessChance[i].value = (float)DataStore.sicknessProbInXML[i] * 100;
                }
            });

            // Save settings button.
            UIButton illnessSave = (UIButton)group4.AddButton("Save and apply", () =>
            {
                // Update datastore with slider values.
                for (int i = 0; i < numDeciles; i++)
                {
                    DataStore.sicknessProbInXML[i] = illnessChance[i].value / 100;
                    DataStore.sicknessProbCalc[i] = (int)(1000 * illnessChance[i].value);
                }

                // Write to file.
                SaveXML();
            });

            // Turn off autolayout to fit next buttons to the right of the illness saved button.
            ((UIPanel)illnessResetSaved.parent).autoLayout = false;

            // Reset to default button.
            UIButton illnessResetDefault = (UIButton)group4.AddButton("Reset to default", () =>
            {
                for (int i = 0; i < numDeciles; i++)
                {
                    // Retrieve default values.
                    illnessChance[i].value = defaultSicknessProbs[i] * 100;
                }
            });
            illnessResetDefault.relativePosition = PositionRightOf(illnessResetSaved);

            // Reset to default button.
            UIButton illnessSetZero = (UIButton)group4.AddButton("Set all to zero", () =>
            {
                for (int i = 0; i < numDeciles; i++)
                {
                    // Reset everything to zero.
                    illnessChance[i].value = 0;
                }
            });
            illnessSetZero.relativePosition = PositionRightOf(illnessResetDefault);

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



        /// <summary>
        /// Adds a plain text label to the specified UI panel.
        /// </summary>
        /// <param name="panel">UI panel to add the label to</param>
        /// <param name="text">Label text</param>
        /// <returns></returns>
        private static void AddLabel(UIPanel panel, string text)
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
        }


        /// <summary>
        /// Adds a slider with a descriptive text label above and an automatically updating value label immediately to the right.
        /// </summary>
        /// <param name="helper">UIHelper panel to add the control to</param>
        /// <param name="text">Descriptive label text</param>
        /// <param name="min">Slider minimum value</param>
        /// <param name="max">Slider maximum value</param>
        /// <param name="step">Slider minimum step</param>
        /// <param name="defaultValue">Slider initial value</param>
        /// <param name="eventCallback">Slider event handler</param>
        /// <returns></returns>
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
        private static Vector3 PositionRightOf(UIComponent uIComponent, float margin = 10f, float verticalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }


        /// <summary>
        /// Updates XML configuration file with current settings.
        /// </summary>
        private static void SaveXML()
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