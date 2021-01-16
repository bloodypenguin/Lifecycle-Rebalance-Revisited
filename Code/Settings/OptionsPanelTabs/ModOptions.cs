using UnityEngine;
using ColossalFramework.UI;


namespace LifecycleRebalance
{
    /// <summary>
    /// Options panel for setting general mod options.
    /// </summary>
    public class ModOptions
    {
        /// <summary>
        /// Adds mod options tab to tabstrip.
        /// </summary
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public ModOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel modTab = PanelUtils.AddTab(tabStrip, Translations.Translate("LBR_SET"), tabIndex, true);

            // Language dropdown.
            UIDropDown languageDrop = PanelUtils.AddPlainDropDown(modTab, Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDrop.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                Configuration<SettingsFile>.Save();
            };

            // Logging options.
            UICheckBox deathCheckBox = PanelUtils.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGD"));
            deathCheckBox.isChecked = OptionsPanel.settings.LogDeaths;
            deathCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Logging.UseDeathLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogDeaths = isChecked;
                Configuration<SettingsFile>.Save();

                Logging.Message("death logging ", OptionsPanel.settings.LogDeaths ? "enabled" : "disabled");
            };

            UICheckBox immigrantCheckBox = PanelUtils.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGI"));
            immigrantCheckBox.isChecked = OptionsPanel.settings.LogImmigrants;
            immigrantCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Logging.UseImmigrationLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogImmigrants = isChecked;
                Configuration<SettingsFile>.Save();

                Logging.Message("immigrant logging ", OptionsPanel.settings.LogImmigrants ? "enabled" : "disabled");
            };

            UICheckBox transportCheckBox = PanelUtils.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGT"));
            transportCheckBox.isChecked = OptionsPanel.settings.LogTransport;
            transportCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Logging.UseTransportLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogTransport = isChecked;
                Configuration<SettingsFile>.Save();

                Logging.Message("transport choices logging ", OptionsPanel.settings.LogTransport ? "enabled" : "disabled");
            };

            UICheckBox sicknessCheckBox = PanelUtils.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGS"));
            sicknessCheckBox.isChecked = OptionsPanel.settings.LogSickness;
            sicknessCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Logging.UseSicknessLog = isChecked;

                // Update configuration file.
                OptionsPanel.settings.LogSickness = isChecked;
                Configuration<SettingsFile>.Save();

                Logging.Message("sickness logging ", OptionsPanel.settings.LogSickness ? "enabled" : "disabled");
            };
        }
    }
}