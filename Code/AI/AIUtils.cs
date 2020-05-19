using ColossalFramework;


namespace LifecycleRebalance
{
    /// <summary>
    /// Utility methods for AI patches.
    /// </summary>
    public static class AIUtils
    {
        // Cache for items with lowest values.
        public static uint citizenCache = 0u;
        public static int[] cacheArray;
        public static bool livesInBike = false;


        /// <summary>
        /// Selects the appropriate data array based on parameters.
        /// </summary>
        /// <param name="wealthLevel">Citizen wealth level</param>
        /// <param name="subService">Building subservice</param>
        /// <param name="ageGroup">Citizen age group</param>
        /// <returns></returns>
        public static int[] GetArray(Citizen.Wealth wealthLevel, ItemClass.SubService subService, Citizen.AgeGroup ageGroup)
        {
            int[][] array;
            // TODO - split for eco
            bool eco = (subService == ItemClass.SubService.ResidentialHighEco) || (subService == ItemClass.SubService.ResidentialLowEco);
            int densityIndex = (subService == ItemClass.SubService.ResidentialHigh) || (subService == ItemClass.SubService.ResidentialHighEco) ? 1 : 0;
            if (eco)
            {
                switch (wealthLevel)
                {
                    case Citizen.Wealth.High:
                        array = DataStore.eco_wealth_high[densityIndex];
                        break;
                    case Citizen.Wealth.Medium:
                        array = DataStore.eco_wealth_med[densityIndex];
                        break;
                    case Citizen.Wealth.Low:
                    default:
                        array = DataStore.eco_wealth_low[densityIndex];
                        break;
                }
            }
            else
            {
                switch (wealthLevel)
                {
                    case Citizen.Wealth.High:
                        array = DataStore.wealth_high[densityIndex];
                        break;
                    case Citizen.Wealth.Medium:
                        array = DataStore.wealth_med[densityIndex];
                        break;
                    case Citizen.Wealth.Low:
                    default:
                        array = DataStore.wealth_low[densityIndex];
                        break;
                }
            }
            return array[(int)ageGroup];
        }


        /// <summary>
        /// Calculates whether or not a corpse should remain (to be picked up deathcare services), or 'vanish into thin air'.
        /// </summary>
        /// <returns>True if the corpse should remain, False if the corpse should vanish</returns>
        public static bool KeepCorpse()
        {
            return Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 99) > DataStore.autoDeadRemovalChance;
        }
    }
}