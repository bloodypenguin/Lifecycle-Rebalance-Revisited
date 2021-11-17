using System;
using System.Text;


namespace LifecycleRebalance
{
    /// <summary>
    /// Values stored in the mod's settings file.
    /// </summary>
    [ConfigurationPath("LifecycleRebalance.xml")]
    public class SettingsFile
    {
        public string WhatsNewVersion { get => ModSettings.whatsNewVersion; set => ModSettings.whatsNewVersion = value; }
        public int WhatsNewBeta { get => ModSettings.whatsNewBetaVersion; set => ModSettings.whatsNewBetaVersion = value; }
        public bool UseVanilla { get; set; } = false;
        public bool UseLegacy { get; set; } = false;
        public bool CustomRetirement { get ; set; } = false;
        public uint RetirementYear { get; set; } = 65;
        public bool UseTransportModes { get; set; } = true;
        public bool RandomImmigrantEd { get; set; } = true;
        public bool DetailLogging { get => Logging.detailLogging; set => Logging.detailLogging = value; }
        public bool LogDeaths { get; set; } = false;
        public bool LogImmigrants { get; set; } = false;
        public bool LogTransport { get; set; } = false;
        public bool LogSickness { get; set; } = false;
        public bool ImmiEduBoost { get => ModSettings.immiEduBoost; set => ModSettings.immiEduBoost = value; }
        public bool ImmiEduDrag { get => ModSettings.immiEduDrag; set => ModSettings.immiEduDrag = value; }

        // Language.
        public string Language
        {
            get
            {
                return Translations.Language;
            }
            set
            {
                Translations.Language = value;
            }
        }
    }


    /// <summary>
    /// Tracks and implements mod settings when mod is running.
    /// </summary>
    internal static class ModSettings
    {
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

        // What's new notification version.
        internal static string whatsNewVersion = "0.0";
        internal static int whatsNewBetaVersion = 0;

        // 1 divided by the number of game age increments per decade for calculation purposes.
        public static double decadeFactor;

        // Whether or not to apply a randomisation factor to immigrant education levels.
        internal static bool randomImmigrantEd;

        // Immigratnt education level boost/drag.
        internal static bool immiEduBoost = false;
        internal static bool immiEduDrag = false;

        // Whether or not we're using custom childhood settings.
        internal static bool customChildhood = false;


        /// <summary>
        /// Tracks if we're using legacy lifecycle calculations and handles any changes.
        /// </summary>
        internal static bool VanillaCalcs
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
        private static bool _vanillaCalcs;


        /// <summary>
        /// Tracks if we're using legacy lifecycle calculations and handles any changes.
        /// </summary>
        internal static bool LegacyCalcs
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
        private static bool _legacyCalcs;


        /// <summary>
        /// Tracks if we're using custom retirement ages and handles any changes.
        /// </summary>
        internal static bool CustomRetirement
        {
            get => _customRetirement;
            
            set
            {
                _customRetirement = value;

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }
        private static bool _customRetirement;



        /// <summary>
        /// Tracks custom retirement ages and handles any changes.
        /// </summary>
        internal static uint RetirementYear
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
        private static uint _retirementYear;


        /// <summary>
        /// Tracks if we're using custom transport mode options and handles any changes.
        /// </summary>
        internal static bool UseTransportModes
        {
            get => _useTransportModes;

            set
            {
                _useTransportModes = value;

                // Apply choices by applying or unapplying Harmony transport choice patches as required.

                if (Loading.isModCreated)
                {
                    if (value)
                    {
                        Patcher.ApplyPrefix(Patcher.OriginalGetCarProbability, Patcher.GetCarProbabilityPrefix);
                        Patcher.ApplyPrefix(Patcher.OriginalGetBikeProbability, Patcher.GetBikeProbabilityPrefix);
                        Patcher.ApplyPrefix(Patcher.OriginalGetTaxiProbability, Patcher.GetTaxiProbabilityPrefix);
                    }
                    else
                    {
                        Patcher.RevertPrefix(Patcher.OriginalGetCarProbability, Patcher.GetCarProbabilityPrefix);
                        Patcher.RevertPrefix(Patcher.OriginalGetBikeProbability, Patcher.GetBikeProbabilityPrefix);
                        Patcher.RevertPrefix(Patcher.OriginalGetTaxiProbability, Patcher.GetTaxiProbabilityPrefix);
                    }
                }
            }
        }
        private static bool _useTransportModes;


        /// <summary>
        /// The age (in age units) at which children will start school.
        /// </summary>
        internal static uint SchoolStartAge
        {
            get => customChildhood ? _schoolStartAge : VanillaSchoolAge;

            set => _schoolStartAge = value;
        }
        private static uint _schoolStartAge = DefaultSchoolAge;


        /// <summary>
        /// The age (in age units) at which children become teenagers (and start high school).
        /// </summary>
        internal static uint TeenStartAge
        {
            get => customChildhood ? _teenStartAge : VanillaTeenAge;

            set => _teenStartAge = value;
        }
        private static uint _teenStartAge = DefaultTeenAge;


        /// <summary>
        /// The age (in age units) at which teenagers become young adults (and start university/college).
        /// </summary>
        internal static uint YoungStartAge
        {
            get => customChildhood ? _youngStartAge : VanillaYoungAge;

            set => _youngStartAge = value;
        }
        private static uint _youngStartAge = DefaultYoungAge;


        /// <summary>
        /// Sets ageing decade factor based on current settings.
        /// </summary>
        private static void SetDecadeFactor()
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
        private static void SetRetirementAge()
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

        internal static int retirementAge;


        /// <summary>
        /// Populates the decadal survival probability table based on previously loaded XML settings and current mod settings.
        /// </summary>
        internal static void SetSurvivalProb()
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
    }
}