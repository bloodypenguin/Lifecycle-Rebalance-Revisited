using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;


namespace LifecycleRebalance
{
    /// <summary>
    /// Tracks and implements mod settings when mod is running.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class ModSettings
    {
        // Settings file.
        [XmlIgnore]
        private static readonly string SettingsFileName = "LifecycleRebalance.xml";
        private static readonly string SettingsFile = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, SettingsFileName);


        // Age constants - vanilla values (in age units).
        private const int VanillaSchoolAge = 0;
        private const int VanillaTeenAge = 15;
        private const int VanillaYoungAge = 45;
        internal const int VanillaAdultAge = 90;
        private const int VanillaRetirementAge = 180;

        // Age units per year, defined by the game in 1.13 in District.GetAverageLifespan().
        internal const float AgePerYear = 3.5f;

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

        // Age constants - minimum and maximums (in years).
        internal const int MinSchoolStartYear = 0;
        internal const int MaxSchoolStartYear = 8;
        internal const int MinTeenStartYear = 10;
        internal const int MaxTeenStartYear = 14;
        internal const int MinYoungStartYear = 16;
        internal const int MaxYoungStartYear = 20;
        internal const int MinRetirementYear = 50;
        internal const int MaxRetirementYear = 70;


        // 1 divided by the number of game age increments per decade for calculation purposes.
        [XmlIgnore]
        internal double decadeFactor;

        // Ages.
        [XmlIgnore]
        private static int schoolStartAge = DefaultSchoolAge;
        [XmlIgnore]
        private static int teenStartAge = DefaultTeenAge;
        [XmlIgnore]
        private static int youngStartAge = DefaultYoungAge;
        [XmlIgnore]
        internal static int retirementAge = VanillaRetirementAge;

        // Modes.
        [XmlIgnore]
        private static bool customChildhood = true;


        /// <summary>
        /// The age at which children start school according to current settings.
        /// </summary>
        internal static int SchoolStartAge => customChildhood ? schoolStartAge : VanillaSchoolAge;


        /// <summary>
        /// The age at which children become teenagers (and start high school) according to current settings.
        /// </summary>
        internal static int TeenStartAge => customChildhood ? teenStartAge : VanillaTeenAge;


        /// <summary>
        /// The age at which teenagers become young adults (and start college/university) according to current settings.
        /// </summary>
        internal static int YoungStartAge => customChildhood ? youngStartAge : VanillaYoungAge;


        /// <summary>
        /// Settings file instance reference.
        /// </summary>
        [XmlIgnore]
        internal static ModSettings Settings { get; private set; }


        /// <summary>
        /// Language code.
        /// </summary>
        [XmlElement("Language")]
        public string XMLLanguage { get => Translations.Language; set => Translations.Language = value; }

        /// <summary>
        /// What's new notification version.
        /// </summary>
        [XmlElement("WhatsNewVersion")]
        public string whatsNewVersion = "0.0";


        /// <summary>
        /// What's new Beta notification version.
        /// </summary>
        [XmlElement("WhatsNewBeta")]
        public int whatsNewBetaVersion = 0;


        /// <summary>
        /// Tracks if we're using legacy lifecycle calculations and handles any changes.
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
        [XmlIgnore]
        private bool _vanillaCalcs = false;


        /// <summary>
        /// Tracks if we're using legacy lifecycle calculations and handles any changes.
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
                if (Loading.isModCreated)
                {
                    SetSurvivalProb();
                }

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }
        [XmlIgnore]
        private bool _legacyCalcs = false;


        /// <summary>
        /// Tracks if we're using custom retirement ages and handles any changes.
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
        [XmlIgnore]
        private bool _customRetirement = false;


        /// <summary>
        /// Tracks custom retirement ages and handles any changes.
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
        [XmlIgnore]
        private int _retirementYear = DefaultRetirementYear;


        /// <summary>
        /// Tracks if we're using custom transport mode options and handles any changes.
        /// </summary>
        [XmlElement("UseTransportModes")]
        public bool UseTransportModes
        {
            get => _useTransportModes;

            set
            {
                _useTransportModes = value;

                // Apply choices by applying or unapplying Harmony transport choice patches as required.
                Patcher.ApplyTransportPatches(value);
            }
        }
        [XmlIgnore]
        private bool _useTransportModes = true;


        /// <summary>
        // Whether or not to apply a randomisation factor to immigrant education levels.
        /// </summary>
        [XmlElement("RandomImmigrantEd")]
        public bool RandomImmigrantEd { get; set; } = false;


        /// <summary
        /// Immigrant education level boost enabled.
        /// </summary>
        [XmlElement("ImmiEduBoost")]
        public bool ImmiEduBoost { get; set; } = false;


        /// <summary>
        /// Immigrant education level drag enabled.
        /// </summary>
        [XmlElement("ImmiEduDrag")]
        public bool ImmiEduDrag { get; set; } = false;


        // Whether or not we're using custom childhood settings.
        [XmlElement("CustomChildhood")]
        public bool CustomChildhood
        {
            get => customChildhood;
            set => customChildhood = value;
        }


        /// <summary>
        /// The age (in age units) at which children will start school.
        /// </summary>
        [XmlElement("SchoolStartYear")]
        public int SchoolStartYear
        {
            get => (int)(schoolStartAge / AgePerYear);

            // Clamp age before assigning.
            set => schoolStartAge = (int)(Mathf.Clamp(value, MinSchoolStartYear, MaxSchoolStartYear) * AgePerYear);
        }


        /// <summary>
        /// The age (in years) at which children become teenagers (and start high school).
        /// </summary>
        [XmlElement("TeenStartYear")]
        public int TeenStartYear
        {
            get => (int)(teenStartAge / AgePerYear);

            // Clamp age before assigning.
            set => teenStartAge = (int)(Mathf.Clamp(value, MinTeenStartYear, MaxTeenStartYear) * AgePerYear);
        }


        /// <summary>
        /// The age (in years) at which teenagers become young adults (and start university/college).
        /// </summary>
        [XmlElement("YoungStartYear")]
        public int YoungStartYear
        {
            get => (int)(youngStartAge / AgePerYear);

            // Clamp age before assigning.
            set => youngStartAge = (int)(Mathf.Clamp(value, MinYoungStartYear, MaxYoungStartYear) * AgePerYear);
        }


        /// <summary>
        /// Enables/disables detailed debug logging to game output log.
        /// </summary>
        [XmlElement("DetailLogging")]
        public bool XMLDetailLogging { get => Logging.detailLogging; set => Logging.detailLogging = value; }


        /// <summary>
        /// Enables/disables detailed death logging.
        /// </summary>
        [XmlElement("LogDeaths")]
        public bool XMLLogDeaths { get => Logging.useDeathLog; set => Logging.useDeathLog = value; }


        /// <summary>
        /// Enables/disables detailed transport logging.
        /// </summary>
        [XmlElement("LogTransport")]
        public bool XMLLogTransport { get => Logging.useTransportLog; set => Logging.useTransportLog = value; }


        /// <summary>
        /// Enables/disables detailed sickness logging.
        /// </summary>
        [XmlElement("LogSickness")]
        public bool XMLLogSickness { get => Logging.useSicknessLog; set => Logging.useSicknessLog = value; }


        /// <summary>
        /// Enables/disables detailed immigration logging.
        /// </summary>
        [XmlElement("LogImmigration")]
        public bool XMLLogImmigration { get => Logging.useImmigrationLog; set => Logging.useImmigrationLog = value; }


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
                decadeFactor = 1d / 25d;
            }
            else
            {
                decadeFactor = 1d / 35d;
            }
        }


        /// <summary>
        /// Sets retirement age based on current settings.
        /// </summary>
        private void SetRetirementAge()
        {
            // Store current retirement age - used to avoid unnecessary logging.
            int oldRetirementAge = retirementAge;

            // Only set custom retirement age if not using vanilla or legacy calculations and the custom retirement option is enabled.
            if (!LegacyCalcs && !VanillaCalcs && CustomRetirement)
            {
                retirementAge = (int)(_retirementYear * AgePerYear);

                // Catch situations where retirementYear hasn't initialised yet.
                if (retirementAge == 0)
                {
                    retirementAge = VanillaRetirementAge;
                }
            }
            else
            {
                // Game default retirement age is 180.
                retirementAge = VanillaRetirementAge;
            }

            // Only log messages when the retirement age changes.
            if (retirementAge != oldRetirementAge)
            {
                Logging.Message("retirement age set to ", retirementAge);
            }
        }


        /// <summary>
        /// Populates the decadal survival probability table based on previously loaded XML settings and current mod settings.
        /// </summary>
        internal void SetSurvivalProb()
        {
            StringBuilder logMessage = new StringBuilder("survival probability table using factor of " + decadeFactor + ":" + Environment.NewLine);

            // Do conversion from survivalProbInXML
            for (int i = 0; i < DataStore.survivalProbInXML.Length; ++i)
            {
                // Using 100,000 as equivalent to 100%, for precision (as final figures are integer).
                if (_legacyCalcs)
                {
                    // Legacy WG calculation, using natural logarithm. Original author acknowledges its inaccuracy.
                    // Accurate for factors above ~0.95, but gets increasingly inaccurate below that, providing higher mortality than mathematical models.
                    // With default acutarial settings, mathematical model overstates 8th decade mortality by 1.3%, 9th decade by 4.3%, 10th decade by 8.3%.
                    DataStore.survivalProbCalc[i] = (int)(100000 - (100000 * (1 + (Math.Log(DataStore.survivalProbInXML[i]) * decadeFactor))));
                }
                else
                {
                    // Updated calculation balanced for 1.13, using exponent - exactly matches mathematical models.
                    // Calculation is chance of death: 100% - (DecadeSurvival% ^ (1/IncrementsPerDecade)).
                    DataStore.survivalProbCalc[i] = (int)(100000 - (Math.Pow(DataStore.survivalProbInXML[i], decadeFactor) * 100000));
                }
                logMessage.AppendLine(i + ": " + DataStore.survivalProbInXML[i] + " = " + DataStore.survivalProbCalc[i]);
            }
            Logging.Message(logMessage);
        }


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
    }
}