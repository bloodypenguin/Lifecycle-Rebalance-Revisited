
namespace LifecycleRebalanceRevisited
{
    [ConfigurationPath("LifecycleRebalance.xml")]
    public class LifecycleRebalanceSettingsFile
    {
        public bool UseLegacy { get; set; } = false;
        public bool LogDeaths { get; set; } = false;
        public bool LogImmigrants { get; set; } = false;
        public bool LogTransport { get; set; } = false;

    }

    public class LifecycleRebalanceSettings
    {
        // 1 divided by the number of game age increments per year for calculation purposes.
        public static double agePerDecadeFactor;
    }
}