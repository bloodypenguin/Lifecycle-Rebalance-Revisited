using System;
using System.Text;


namespace LifecycleRebalanceRevisited
{
    /// <summary>
    /// Values stored in the mod's settings file.
    /// </summary>
    [ConfigurationPath("LifecycleRebalance.xml")]
    public class SettingsFile
    {
        public bool UseLegacy { get; set; } = false;
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
        public static bool Legacy
        {
            get
            {
                return _legacy;
            }
            set
            {
                // When we set this value, also recalculate age increments per decade.
                // Also recalculate the survival probability table if the value has changed and game has been loaded (i.e. not from main menu options panel).
                decadeFactor = 1d / (value ? 25d : 35d);

                if (_legacy != value && LoadingExtension.isModCreated)
                {
                    SetSurvivalProb();
                }

                _legacy = value;
            }
        }
        private static bool _legacy;


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
                if (_legacy)
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