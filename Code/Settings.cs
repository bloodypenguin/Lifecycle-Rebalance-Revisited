using System;
using System.Text;
using UnityEngine;


namespace LifecycleRebalanceRevisited
{
    /// <summary>
    /// Values stored in the mod's settings file.
    /// </summary>
    [ConfigurationPath("LifecycleRebalance.xml")]
    public class SettingsFile
    {
        public bool UseLegacy { get; set; } = false;
        public bool CustomRetirement { get ; set; } = false;
        public int RetirementYear { get; set; }
        public bool LogDeaths { get; set; } = false;
        public bool LogImmigrants { get; set; } = false;
        public bool LogTransport { get; set; } = false;

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
        public static bool LegacyCalcs
        {
            get => _legacyCalcs;
            
            set
            {
                _legacyCalcs = value;

                // When we set this value, also recalculate age increments per decade.
                // Game in 1.13 defines years as being age divided by 3.5.  Hence, 35 age increments per decade.
                // Legacy mod behaviour worked on 25 increments per decade.
                decadeFactor = 1d / (value ? 25d : 35d);

                // Also recalculate the survival probability table if the game has been loaded (i.e. not from main menu options panel).
                if (LoadingExtension.isModCreated)
                {
                    SetSurvivalProb();
                }

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }
        private static bool _legacyCalcs;


        public static bool CustomRetirement
        {
            get => _customRetirement;
            
            set
            {
                _customRetirement = value;
                Debug.Log("Lifecycle Rebalance Revisited: custom retirement age " + (_customRetirement ? "enabled." : "disabled."));

                // A change here can affect retirement age in combination with other settings.
                SetRetirementAge();
            }
        }
        private static bool _customRetirement;

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


        private static void SetRetirementAge()
        {
            // Only set custom retirement age if not using legacy calculations and the custom retirement option is enabled.
            if (!LegacyCalcs && CustomRetirement)
            {
                retirementAge = (int)(_retirementYear * 3.5);
            }
            else
            {
                // Game default retirement age is 180.
                retirementAge = 180;
            }
            Debug.Log("Lifecycle Rebalance Revisited: retirement age set to " + retirementAge + ".");
        }

        public static int retirementAge;


        /// <summary>
        /// Populates the decadal survival probability table based on previously loaded XML settings and current mod settings.
        /// </summary>
        public static void SetSurvivalProb()
        {
            StringBuilder logMessage = new StringBuilder("Lifecycle Rebalance Revisited: survival probability table using factor of " + decadeFactor + ":\r\n");

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
            UnityEngine.Debug.Log(logMessage);
        }
    }
}