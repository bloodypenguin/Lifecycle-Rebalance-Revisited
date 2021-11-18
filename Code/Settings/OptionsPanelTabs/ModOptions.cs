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
            languageDrop.eventSelectedIndexChanged += (control, index) => { Translations.Index = index; };

            // Detail logging options.
            UICheckBox logCheckBox = PanelUtils.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LDT"));
            logCheckBox.isChecked = Logging.detailLogging;
            logCheckBox.eventCheckChanged += (control, isChecked) => { Logging.detailLogging = isChecked; };

            // Logging options.
            UICheckBox deathCheckBox = PanelUtils.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGD"));
            deathCheckBox.isChecked = Logging.useDeathLog;
            deathCheckBox.eventCheckChanged += (control, isChecked) => { Logging.useDeathLog = isChecked; };

            UICheckBox immigrantCheckBox = PanelUtils.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGI"));
            immigrantCheckBox.isChecked = Logging.useImmigrationLog;
            immigrantCheckBox.eventCheckChanged += (control, isChecked) => { Logging.useImmigrationLog = isChecked; };

            UICheckBox transportCheckBox = PanelUtils.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGT"));
            transportCheckBox.isChecked = Logging.useTransportLog;
            transportCheckBox.eventCheckChanged += (control, isChecked) => { Logging.useTransportLog = isChecked; };

            UICheckBox sicknessCheckBox = PanelUtils.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGS"));
            sicknessCheckBox.isChecked = Logging.useSicknessLog;
            sicknessCheckBox.eventCheckChanged += (control, isChecked) => { Logging.useSicknessLog = isChecked; };
        }
    }
}