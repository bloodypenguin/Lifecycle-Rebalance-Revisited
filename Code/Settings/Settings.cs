using System;
using System.Text;
using UnityEngine;


namespace LifecycleRebalance
{
    /// <summary>
    /// Values stored in the mod's settings file.
    /// </summary>
    [ConfigurationPath("LifecycleRebalance.xml")]
    public class SettingsFile
    {
        public int NotificationVersion { get; set; } = 0;
        public bool UseVanilla { get; set; } = false;
        public bool UseLegacy { get; set; } = false;
        public bool CustomRetirement { get ; set; } = false;
        public int RetirementYear { get; set; } = 65;
        public bool UseTransportModes { get; set; } = true;
        public bool LogDeaths { get; set; } = false;
        public bool LogImmigrants { get; set; } = false;
        public bool LogTransport { get; set; } = false;
        public bool LogSickness { get; set; } = false;

    }


    /// <summary>
    /// Tracks and implements mod settings when mod is running.
    /// </summary>
    public class ModSettings
    {
        // 1 divided by the number of game age increments per decade for calculation purposes.
        public static double decadeFactor;


        /// <summary>
        /// Tracks if we're using legacy lifecycle calculations and handles any changes.
        /// </summary>
        public static bool VanillaCalcs
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
        public static bool LegacyCalcs
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
        public static bool CustomRetirement
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
        public static int RetirementYear
        {
            get => _retirementYear;

            set
            {
                // Clamp retirement year between 50 and 65.
                _retirementYear = Math.Max(50, Math.Min(65, value));

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }
        private static int _retirementYear;


        /// <summary>
        /// Tracks if we're using custom transport mode options and handles any changes.
        /// </summary>
        public static bool UseTransportModes
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

                // Apply Harmony patch to GetAgeGroup.
                if (Loading.isModCreated)
                {
                    Patcher.ApplyPrefix(Patcher.OriginalGetAgeGroup, Patcher.GetAgeGroupPrefix);
                }
            }
            else
            {
                // Game default retirement age is 180.
                retirementAge = 180;
                // Unapply Harmony patch from GetAgeGroup.
                if (Loading.isModCreated)
                {
                    Patcher.RevertPrefix(Patcher.OriginalGetAgeGroup, Patcher.GetAgeGroupPrefix);
                }
            }

            // Only log messages when the retirement age changes.
            if (retirementAge != oldRetirementAge)
            {
                Debugging.Message("retirement age set to " + retirementAge);
            }
        }

        public static int retirementAge;


        /// <summary>
        /// Populates the decadal survival probability table based on previously loaded XML settings and current mod settings.
        /// </summary>
        public static void SetSurvivalProb()
        {
            StringBuilder logMessage = new StringBuilder("survival probability table using factor of " + decadeFactor + ":\r\n");

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
            Debugging.Message(logMessage.ToString());
        }
    }
}