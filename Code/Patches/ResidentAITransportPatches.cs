// <copyright file="ResidentAITransportPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using ColossalFramework;

    /// <summary>
    /// Harmony patches for ResidentAI to implement mod's transport probability settings.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class ResidentAITransportPatches
    {
        // Cache for items with lowest values.
        private static int[] s_cacheArray;
        private static bool s_livesInBike = false;

        /// <summary>
        /// Harmony pre-emptive Prefix patch for ResidentAI.GetCarProbability - implements mod's transport probability settings for cars.
        /// Patch is manually applied (and unapplied) depending if custom transport mode probabilities setting is active or not.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="citizenData">Citizen data.</param>
        /// <param name="ageGroup">Citizen age group.</param>
        /// <returns>Always false (never execute original method).</returns>
        public static bool GetCarProbability(ref int __result, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[(int)((UIntPtr)citizenData.m_citizen)];
            ushort homeBuilding = citizen.m_homeBuilding;

            ItemClass.SubService subService = ItemClass.SubService.ResidentialLow;
            if (homeBuilding != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)homeBuilding];
                District district = instance.m_districts.m_buffer[instance.GetDistrict(building.m_position)];
                DistrictPolicies.CityPlanning cityPlanningPolicies = district.m_cityPlanningPolicies;

                s_livesInBike = (cityPlanningPolicies & DistrictPolicies.CityPlanning.EncourageBiking) != DistrictPolicies.CityPlanning.None;
                subService = Singleton<BuildingManager>.instance.m_buildings.m_buffer[homeBuilding].Info.GetSubService();
            }

            // Set the cache
            s_cacheArray = GetArray(citizen.WealthLevel, subService, ageGroup);

            // Original method return value.
            __result = s_cacheArray[DataStore.Car];

            if (LifecycleLogging.UseTransportLog)
            {
                LifecycleLogging.WriteToLog(LifecycleLogging.TransportLogName, citizen.WealthLevel, "-wealth ", ageGroup, " has ", __result + "% chance of driving");
            }

            // Don't execute base method after this.
            return false;
        }

        /// <summary>
        /// Harmony pre-emptive Prefix patch for ResidentAI.GetBikeProbability - implements mod's transport probability settings for bicycles.
        /// Patch is manually applied (and unapplied) depending if custom transport mode probabilities setting is active or not.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="ageGroup">Citizen age group.</param>
        /// <returns>Always false (never execute original method).</returns>
        public static bool GetBikeProbability(ref int __result, Citizen.AgeGroup ageGroup)
        {
            int bike = s_livesInBike ? DataStore.BikeIncrease : 0;

            // Original method return value.
            // Array cache has already been set when GetCarProbability was called.
            __result = s_cacheArray[DataStore.Bike] + bike;

            if (LifecycleLogging.UseTransportLog)
            {
                LifecycleLogging.WriteToLog(LifecycleLogging.TransportLogName, "The same ", ageGroup, " has ", __result, "% chance of cycling");
            }

            // Don't execute base method after this.
            return false;
        }

        /// <summary>
        /// Harmony pre-emptive Prefix patch for ResidentAI.GetTaxiProbability - implements mod's transport probability settings for taxis.
        /// Patch is manually applied (and unapplied) depending if custom transport mode probabilities setting is active or not.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="ageGroup">Citizen age group.</param>
        /// <returns>Always false (never execute original method).</returns>
        public static bool GetTaxiProbability(ref int __result, Citizen.AgeGroup ageGroup)
        {
            // Original method return value.
            // Array cache has already been set when GetCarProbability was called.
            __result = s_cacheArray[DataStore.Taxi];

            if (LifecycleLogging.UseTransportLog)
            {
                LifecycleLogging.WriteToLog(LifecycleLogging.TransportLogName, "The same ", ageGroup, " has ", __result, "% chance of using a taxi");
            }

            // Don't execute base method after this.
            return false;
        }

        /// <summary>
        /// Selects the appropriate data array based on parameters.
        /// </summary>
        /// <param name="wealthLevel">Citizen wealth level.</param>
        /// <param name="subService">Building subservice.</param>
        /// <param name="ageGroup">Citizen age group.</param>
        /// <returns>Relevant data array.</returns>
        private static int[] GetArray(Citizen.Wealth wealthLevel, ItemClass.SubService subService, Citizen.AgeGroup ageGroup)
        {
            int[][] array;

            bool eco = (subService == ItemClass.SubService.ResidentialHighEco) || (subService == ItemClass.SubService.ResidentialLowEco);
            int densityIndex = (subService == ItemClass.SubService.ResidentialHigh) || (subService == ItemClass.SubService.ResidentialHighEco) ? 1 : 0;
            if (eco)
            {
                switch (wealthLevel)
                {
                    case Citizen.Wealth.High:
                        array = DataStore.TransportHighWealthEco[densityIndex];
                        break;
                    case Citizen.Wealth.Medium:
                        array = DataStore.TransportMedWealthEco[densityIndex];
                        break;
                    case Citizen.Wealth.Low:
                    default:
                        array = DataStore.TranportLowWealthEco[densityIndex];
                        break;
                }
            }
            else
            {
                switch (wealthLevel)
                {
                    case Citizen.Wealth.High:
                        array = DataStore.TransportHighWealth[densityIndex];
                        break;
                    case Citizen.Wealth.Medium:
                        array = DataStore.TransportMedWealth[densityIndex];
                        break;
                    case Citizen.Wealth.Low:
                    default:
                        array = DataStore.TransportLowWealth[densityIndex];
                        break;
                }
            }

            return array[(int)ageGroup];
        }
    }
}