// <copyright file="ModOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting general mod options.
    /// </summary>
    internal sealed class ModOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ModOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel modTab = UITabstrips.AddTextTab(tabStrip, Translations.Translate("LBR_SET"), tabIndex, out _, autoLayout: true);

            // Language dropdown.
            UIDropDown languageDrop = UIDropDowns.AddPlainDropDown(modTab, 0f, 0f, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDrop.eventSelectedIndexChanged += (c, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };

            // Detail logging options.
            UICheckBox logCheckBox = UICheckBoxes.AddPlainCheckBox(modTab, Translations.Translate("DETAIL_LOGGING"));
            logCheckBox.isChecked = Logging.DetailLogging;
            logCheckBox.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };

            // Logging options.
            UICheckBox deathCheckBox = UICheckBoxes.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGD"));
            deathCheckBox.isChecked = LifecycleLogging.UseDeathLog;
            deathCheckBox.eventCheckChanged += (c, isChecked) => { LifecycleLogging.UseDeathLog = isChecked; };

            UICheckBox immigrantCheckBox = UICheckBoxes.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGI"));
            immigrantCheckBox.isChecked = LifecycleLogging.UseImmigrationLog;
            immigrantCheckBox.eventCheckChanged += (c, isChecked) => { LifecycleLogging.UseImmigrationLog = isChecked; };

            UICheckBox transportCheckBox = UICheckBoxes.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGT"));
            transportCheckBox.isChecked = LifecycleLogging.UseTransportLog;
            transportCheckBox.eventCheckChanged += (c, isChecked) => { LifecycleLogging.UseTransportLog = isChecked; };

            UICheckBox sicknessCheckBox = UICheckBoxes.AddPlainCheckBox(modTab, Translations.Translate("LBR_SET_LGS"));
            sicknessCheckBox.isChecked = LifecycleLogging.UseSicknessLog;
            sicknessCheckBox.eventCheckChanged += (c, isChecked) => { LifecycleLogging.UseSicknessLog = isChecked; };
        }
    }
}