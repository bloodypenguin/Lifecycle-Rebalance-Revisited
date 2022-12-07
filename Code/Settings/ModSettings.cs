// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using AlgernonCommons.XML;
    using UnityEngine;

    /// <summary>
    /// Tracks and implements mod settings when mod is running.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class ModSettings : SettingsXMLBase
    {
        /// <summary>
        /// Game default age at which citizens become adults.
        /// </summary>
        internal const int VanillaAdultAge = 90;

        /// <summary>
        /// Game default age units per year, defined by the game in 1.13 in District.GetAverageLifespan().
        /// </summary>
        internal const float AgePerYear = 3.5f;

        /// <summary>
        /// Minimum supported age at which children can start school.
        /// </summary>
        internal const int MinSchoolStartYear = 0;

        /// <summary>
        /// Maximum supported age at which children can start school.
        /// </summary>
        internal const int MaxSchoolStartYear = 8;

        /// <summary>
        /// Minimum supported age at which children become teens.
        /// </summary>
        internal const int MinTeenStartYear = 10;

        /// <summary>
        /// Maximum supported age at which children become teens.
        /// </summary>
        internal const int MaxTeenStartYear = 14;

        /// <summary>
        /// Minimum supported age at which teens become young adults.
        /// </summary>
        internal const int MinYoungStartYear = 16;

        /// <summary>
        /// Maximum supported age at which teens become young adults.
        /// </summary>
        internal const int MaxYoungStartYear = 20;

        /// <summary>
        /// Minimum supported retirement age.
        /// </summary>
        internal const int MinRetirementYear = 50;

        /// <summary>
        /// Maximum supported retirement age.
        /// </summary>
        internal const int MaxRetirementYear = 70;

        // Age constants - vanilla values (in age units).
        private const int VanillaSchoolAge = 0;
        private const int VanillaTeenAge = 15;
        private const int VanillaYoungAge = 45;
        private const int VanillaRetirementAge = 180;

        // Age constants - mod default custom values (in age units).
        // Early child - < 6 years = < 21
        // Children - < 11 years = < 38 age units (rounded down from 38.5)
        // Teens - 11-17 years inclusive = < 18 years = < 63 age units
        // Young adults - 18-25 years inclusive = < 91 (but game default is 90 so we just keep that)
        private const int DefaultSchoolAge = 21;
        private const int DefaultTeenAge = 38;
        private const int DefaultYoungAge = 63;

        // Default retirement age (in years, not age units!).
        private const int DefaultRetirementYear = 65;

        // Settings file.
        [XmlIgnore]
        private static readonly string SettingsFileName = "LifecycleRebalance.xml";
        private static readonly string SettingsFile = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, SettingsFileName);

        // Ages.
        [XmlIgnore]
        private static int s_schoolStartAge = DefaultSchoolAge;
        [XmlIgnore]
        private static int s_teenStartAge = DefaultTeenAge;
        [XmlIgnore]
        private static int s_youngStartAge = DefaultYoungAge;

        // Mode.
        [XmlIgnore]
        private static bool s_customChildhood = true;

        // Internal flags.
        private bool _vanillaCalcs = false;
        private bool _legacyCalcs = false;
        private bool _customRetirement = false;
        private int _retirementYear = DefaultRetirementYear;
        private bool _useTransportModes = true;

        /// <summary>
        /// Gets or sets a value indicating whether vanilla lifecycle calculations are in effect.
        /// </summary>
        [XmlElement("UseVanilla")]
        public bool VanillaCalcs
        {
            get => _vanillaCalcs;

            set
            {
                _vanillaCalcs = value;

                // When we set this value, also recalculate age increments per decade.
                SetDecadeFactor();

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether legacy lifecycle calculations are in effect.
        /// </summary>
        [XmlElement("UseLegacy")]
        public bool LegacyCalcs
        {
            get => _legacyCalcs;

            set
            {
                _legacyCalcs = value;

                // When we set this value, also recalculate age increments per decade.
                SetDecadeFactor();

                // Also recalculate the survival probability table if the game has been loaded (i.e. not from main menu options panel).
                if (Loading.IsLoaded)
                {
                    SetSurvivalProb();
                }

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether custom retirement ages are in effect.
        /// </summary>
        [XmlElement("CustomRetirement")]
        public bool CustomRetirement
        {
            get => _customRetirement;

            set
            {
                _customRetirement = value;

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }

        /// <summary>
        /// Gets or sets the custom retirement age.
        /// </summary>
        [XmlElement("RetirementAge")]
        public int RetirementYear
        {
            get => _retirementYear;

            set
            {
                // Clamp retirement year.
                _retirementYear = (int)Mathf.Clamp(value, MinRetirementYear, MaxRetirementYear);

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether custom transport mode probabilities are in effect.
        /// </summary>
        [XmlElement("UseTransportModes")]
        public bool UseTransportModes
        {
            get => _useTransportModes;

            set
            {
                _useTransportModes = value;

                // Apply choices by applying or unapplying Harmony transport choice patches as required.
                PatcherManager<Patcher>.Instance.ApplyTransportPatches(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to apply a randomisation factor to immigrant education levels.
        /// </summary>
        [XmlElement("RandomImmigrantEd")]
        public bool RandomImmigrantEd { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether immigrant education level boosting is enabled.
        /// </summary>
        [XmlElement("ImmiEduBoost")]
        public bool ImmiEduBoost { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether immigrant education level dragging is enabled.
        /// </summary>
        [XmlElement("ImmiEduDrag")]
        public bool ImmiEduDrag { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether custom childhood settings are enabled.
        /// </summary>
        [XmlElement("CustomChildhood")]
        public bool CustomChildhood
        {
            get => s_customChildhood;
            set => s_customChildhood = value;
        }

        /// <summary>
        /// Gets or sets the age (in age units) at which children will start school.
        /// </summary>
        [XmlElement("SchoolStartYear")]
        public int SchoolStartYear
        {
            get => Mathf.RoundToInt(s_schoolStartAge / AgePerYear);

            // Clamp age before assigning.
            set => s_schoolStartAge = (int)(Mathf.Clamp(value, MinSchoolStartYear, MaxSchoolStartYear) * AgePerYear);
        }

        /// <summary>
        /// Gets or sets the age (in years) at which children become teenagers (and start high school).
        /// </summary>
        [XmlElement("TeenStartYear")]
        public int TeenStartYear
        {
            get => Mathf.RoundToInt(s_teenStartAge / AgePerYear);

            // Clamp age before assigning.
            set => s_teenStartAge = (int)(Mathf.Clamp(value, MinTeenStartYear, MaxTeenStartYear) * AgePerYear);
        }

        /// <summary>
        /// Gets or sets the age (in years) at which teenagers become young adults (and start university/college).
        /// </summary>
        [XmlElement("YoungStartYear")]
        public int YoungStartYear
        {
            get => Mathf.RoundToInt(s_youngStartAge / AgePerYear);

            // Clamp age before assigning.
            set => s_youngStartAge = (int)(Mathf.Clamp(value, MinYoungStartYear, MaxYoungStartYear) * AgePerYear);
        }

        /// <summary>
        /// Gets or sets a value indicating whether detailed death logging is enabled.
        /// </summary>
        [XmlElement("LogDeaths")]
        public bool XMLLogDeaths { get => LifecycleLogging.UseDeathLog; set => LifecycleLogging.UseDeathLog = value; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed transport logging is enabled.
        /// </summary>
        [XmlElement("LogTransport")]
        public bool XMLLogTransport { get => LifecycleLogging.UseTransportLog; set => LifecycleLogging.UseTransportLog = value; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed sickness logging is enabled.
        /// </summary>
        [XmlElement("LogSickness")]
        public bool XMLLogSickness { get => LifecycleLogging.UseSicknessLog; set => LifecycleLogging.UseSicknessLog = value; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed immigration logging is enabled.
        /// </summary>
        [XmlElement("LogImmigration")]
        public bool XMLLogImmigration { get => LifecycleLogging.UseImmigrationLog; set => LifecycleLogging.UseImmigrationLog = value; }

        /// <summary>
        /// Gets the age at which children start school according to current settings.
        /// </summary>
        internal static int SchoolStartAge => s_customChildhood ? s_schoolStartAge : VanillaSchoolAge;

        /// <summary>
        /// Gets the age at which children become teenagers (and start high school) according to current settings.
        /// </summary>
        internal static int TeenStartAge => s_customChildhood ? s_teenStartAge : VanillaTeenAge;

        /// <summary>
        /// Gets the age at which teenagers become young adults (and start college/university) according to current settings.
        /// </summary>
        internal static int YoungStartAge => s_customChildhood ? s_youngStartAge : VanillaYoungAge;

        /// <summary>
        /// Gets or sets the retirement age.
        /// </summary>
        [XmlIgnore]
        internal static int RetirementAge { get; set; } = VanillaRetirementAge;

        /// <summary>
        /// Gets the current settings file instance.
        /// </summary>
        [XmlIgnore]
        internal static ModSettings Settings { get; private set; }

        /// <summary>
        /// Gets or sets the current decade factor.
        /// (1 divided by the number of game age increments per decade for calculation purposes).
        /// </summary>
        [XmlIgnore]
        internal double DecadeFactor { get; set; }

        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void Load()
        {
            try
            {
                string fileName = null;

                Logging.Message("reading settings");

                // See if a userdir settings file exists.
                if (File.Exists(SettingsFile))
                {
                    fileName = SettingsFile;
                }
                else if (File.Exists(SettingsFileName))
                {
                    // Otherwise, if an application settings file exists, use that.
                    fileName = SettingsFileName;
                }

                // Check to see if we found an existing configuration file.
                if (fileName != null)
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                        if (xmlSerializer.Deserialize(reader) is ModSettings modSettingsFile)
                        {
                            // Successful read - set settings file instance and return.
                            Settings = modSettingsFile;
                            return;
                        }
                        else
                        {
                            Logging.Error("couldn't deserialize settings file");
                        }
                    }
                }
                else
                {
                    Logging.Message("no settings file found");
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception reading XML settings file");
            }

            // If we got here, something went wrong; create new settings file instance.
            Settings = new ModSettings();
        }

        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void Save()
        {
            try
            {
                // Pretty straightforward.
                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(writer, ModSettings.Settings);
                }

                // Delete any old settings in app directory.
                if (File.Exists(SettingsFileName))
                {
                    File.Delete(SettingsFileName);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }

        /// <summary>
        /// Populates the decadal survival probability table based on previously loaded XML settings and current mod settings.
        /// </summary>
        internal void SetSurvivalProb()
        {
            StringBuilder logMessage = new StringBuilder("survival probability table using factor of " + DecadeFactor + ":" + Environment.NewLine);

            // Do conversion from survivalProbInXML
            for (int i = 0; i < DataStore.SurvivalProbInXML.Length; ++i)
            {
                // Using 100,000 as equivalent to 100%, for precision (as final figures are integer).
                if (_legacyCalcs)
                {
                    // Legacy WG calculation, using natural logarithm. Original author acknowledges its inaccuracy.
                    // Accurate for factors above ~0.95, but gets increasingly inaccurate below that, providing higher mortality than mathematical models.
                    // With default acutarial settings, mathematical model overstates 8th decade mortality by 1.3%, 9th decade by 4.3%, 10th decade by 8.3%.
                    DataStore.SurvivalProbCalc[i] = (int)(100000 - (100000 * (1 + (Math.Log(DataStore.SurvivalProbInXML[i]) * DecadeFactor))));
                }
                else
                {
                    // Updated calculation balanced for 1.13, using exponent - exactly matches mathematical models.
                    // Calculation is chance of death: 100% - (DecadeSurvival% ^ (1/IncrementsPerDecade)).
                    DataStore.SurvivalProbCalc[i] = (int)(100000 - (Math.Pow(DataStore.SurvivalProbInXML[i], DecadeFactor) * 100000));
                }

                logMessage.AppendLine(i + ": " + DataStore.SurvivalProbInXML[i] + " = " + DataStore.SurvivalProbCalc[i]);
            }

            Logging.Message(logMessage);
        }

        /// <summary>
        /// Sets ageing decade factor based on current settings.
        /// </summary>
        private void SetDecadeFactor()
        {
            // Game in 1.13 defines years as being age divided by 3.5.  Hence, 35 age increments per decade.
            // Legacy mod behaviour worked on 25 increments per decade.

            // Decade factor is 1/35, unless legacy calcs are used.
            if (LegacyCalcs && !VanillaCalcs)
            {
                DecadeFactor = 1d / 25d;
            }
            else
            {
                DecadeFactor = 1d / 35d;
            }
        }

        /// <summary>
        /// Sets retirement age based on current settings.
        /// </summary>
        private void SetRetirementAge()
        {
            // Store current retirement age - used to avoid unnecessary logging.
            int oldRetirementAge = RetirementAge;

            // Only set custom retirement age if not using vanilla or legacy calculations and the custom retirement option is enabled.
            if (!LegacyCalcs && !VanillaCalcs && CustomRetirement)
            {
                RetirementAge = (int)(_retirementYear * AgePerYear);

                // Catch situations where retirementYear hasn't initialised yet.
                if (RetirementAge == 0)
                {
                    RetirementAge = VanillaRetirementAge;
                }
            }
            else
            {
                // Game default retirement age is 180.
                RetirementAge = VanillaRetirementAge;
            }

            // Only log messages when the retirement age changes.
            if (RetirementAge != oldRetirementAge)
            {
                Logging.Message("retirement age set to ", RetirementAge);
            }
        }
    }
}