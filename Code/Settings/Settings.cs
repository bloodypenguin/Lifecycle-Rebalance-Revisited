using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;


namespace LifecycleRebalance
{
    /// <summary>
    /// Tracks and implements mod settings when mod is running.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class ModSettings
    {
        [XmlIgnore]
        private static readonly string SettingsFileName = "LifecycleRebalance.xml";

        // Age constants - vanilla values.
        public const uint VanillaSchoolAge = 0;
        public const uint VanillaTeenAge = 15;
        public const uint VanillaYoungAge = 45;
        public const uint VanillaRetirementAge = 180;
        public const float AgePerYear = 3.5f;

        // Age constants - mod default custom values.
        // Early child - < 6 years = < 21
        // Children - < 13 years = < 45 (rounded down from 45.5)
        // Teens - 13-18 years inclusive = < 66 (rounded down from 66.5)
        // Youg adults - 19-25 years inclusive = < 91
        private const uint DefaultSchoolAge = 21;
        private const uint DefaultTeenAge = 45;
        private const uint DefaultYoungAge = 66;


        // Retirement age.
        [XmlIgnore]
        internal static int retirementAge;

        // 1 divided by the number of game age increments per decade for calculation purposes.
        [XmlIgnore]
        internal double decadeFactor;


        /// <summary>
        /// Settings file instance reference.
        /// </summary>
        [XmlIgnore]
        internal static ModSettings Settings { get; private set; }


        /// <summary>
        /// Language code.
        /// </summary>
        [XmlElement("Language")]
        public string Language { get => Translations.Language; set => Translations.Language = value; }

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
        public uint RetirementYear
        {
            get => _retirementYear;

            set
            {
                // Clamp retirement year between 50 and 70.
                _retirementYear = Math.Max(50, Math.Min(70, value));

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }
        [XmlIgnore]
        private uint _retirementYear = 65;


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
        public bool customChildhood = false;


        /// <summary>
        /// The age (in age units) at which children will start school.
        /// </summary>
        [XmlElement("SchoolStartAge")]
        public uint SchoolStartAge
        {
            get => customChildhood ? _schoolStartAge : VanillaSchoolAge;

            set => _schoolStartAge = value;
        }
        [XmlIgnore]
        private uint _schoolStartAge = DefaultSchoolAge;


        /// <summary>
        /// The age (in age units) at which children become teenagers (and start high school).
        /// </summary>
        [XmlElement("TeenStartAge")]
        public uint TeenStartAge
        {
            get => customChildhood ? _teenStartAge : VanillaTeenAge;

            set => _teenStartAge = value;
        }
        [XmlIgnore]
        private uint _teenStartAge = DefaultTeenAge;


        /// <summary>
        /// The age (in age units) at which teenagers become young adults (and start university/college).
        /// </summary>
        [XmlElement("YoungStartAge")]
        public uint YoungStartAge
        {
            get => customChildhood ? _youngStartAge : VanillaYoungAge;

            set => _youngStartAge = value;
        }
        [XmlIgnore]
        private uint _youngStartAge = DefaultYoungAge;


        /// <summary>
        /// Enables/disables detailed debug logging to game output log.
        /// </summary>
        [XmlElement("DetailLogging")]
        public bool DetailLogging { get => Logging.detailLogging; set => Logging.detailLogging = value; }


        /// <summary>
        /// Enables/disables detailed death logging.
        /// </summary>
        [XmlElement("LogDeaths")]
        public bool LogDeaths { get => Logging.useDeathLog; set => Logging.useDeathLog = value; }


        /// <summary>
        /// Enables/disables detailed transport logging.
        /// </summary>
        [XmlElement("LogTransport")]
        public bool LogTransport { get => Logging.useTransportLog; set => Logging.useTransportLog = value; }


        /// <summary>
        /// Enables/disables detailed sickness logging.
        /// </summary>
        [XmlElement("LogSickness")]
        public bool LogSickness { get => Logging.useSicknessLog; set => Logging.useSicknessLog = value; }


        /// <summary>
        /// Enables/disables detailed immigration logging.
        /// </summary>
        [XmlElement("LogImmigration")]
        public bool LogImmigration { get => Logging.useImmigrationLog; set => Logging.useImmigrationLog = value; }


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
                retirementAge = (int)(_retirementYear * 3.5);

                // Catch situations where retirementYear hasn't initialised yet.
                if (retirementAge == 0)
                {
                    retirementAge = 180;
                }
            }
            else
            {
                // Game default retirement age is 180.
                retirementAge = 180;
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
                Logging.Message("reading settings");

                // Check to see if configuration file exists.
                if (File.Exists(SettingsFileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(SettingsFileName))
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
                // Pretty straightforward.  Serialisation is within GBRSettingsFile class.
                using (StreamWriter writer = new StreamWriter(SettingsFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(writer, ModSettings.Settings);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }
    }
}