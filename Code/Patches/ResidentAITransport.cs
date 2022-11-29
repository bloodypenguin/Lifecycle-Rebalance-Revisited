// <copyright file="ResidentAITransport.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using AlgernonCommons;
    using ColossalFramework;

    /// <summary>
    /// Harmony pre-emptive Prefix patches for ResidentAI to implements mod's transport probability settings.
    /// </summary>
    public static class ResidentAITransport
    {
        /// <summary>
        /// Harmony pre-emptive Prefix patch for ResidentAI.GetCarProbability - implements mod's transport probability settings for cars.
        /// Patch is manually applied (and unapplied) depending if custom transport mode probabilities setting is active or not.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="citizenData">Citizen data</param>
        /// <param name="ageGroup">Citizen age group</param>
        /// <returns>Always false (never execute original method)</returns>
        public static bool GetCarProbability(ref int __result, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            // Cache as best we can. The order of calls is car, bike, taxi
            AIUtils.citizenCache = citizenData.m_citizen;  // Not needed, but just in case

            Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[(int)((UIntPtr)citizenData.m_citizen)];
            ushort homeBuilding = citizen.m_homeBuilding;

            ItemClass.SubService subService = ItemClass.SubService.ResidentialLow;
            if (homeBuilding != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)homeBuilding];
                District district = instance.m_districts.m_buffer[instance.GetDistrict(building.m_position)];
                DistrictPolicies.CityPlanning cityPlanningPolicies = district.m_cityPlanningPolicies;

                AIUtils.livesInBike = (cityPlanningPolicies & DistrictPolicies.CityPlanning.EncourageBiking) != DistrictPolicies.CityPlanning.None;
                subService = Singleton<BuildingManager>.instance.m_buildings.m_buffer[homeBuilding].Info.GetSubService();
            }

            // Set the cache
            AIUtils.cacheArray = AIUtils.GetArray(citizen.WealthLevel, subService, ageGroup);

            // Original method return value.
            __result = AIUtils.cacheArray[DataStore.CAR];

            if (LifecycleLogging.useTransportLog)
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
        /// <param name="__result">Original method result</param>
        /// <param name="ageGroup">Citizen age group</param>
        /// <returns>Always false (never execute original method)</returns>
        public static bool GetBikeProbability(ref int __result, Citizen.AgeGroup ageGroup)
        {
            int bike = AIUtils.livesInBike ? DataStore.bikeIncrease : 0;

            // Original method return value.
            // Array cache has already been set when GetCarProbability was called.
            __result = (AIUtils.cacheArray[DataStore.BIKE] + bike);

            if (LifecycleLogging.useTransportLog)
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
        /// <param name="__result">Original method result</param>
        /// <param name="ageGroup">Citizen age group</param>
        /// <returns>Always false (never execute original method)</returns>
        public static bool GetTaxiProbability(ref int __result, Citizen.AgeGroup ageGroup)
        {
            // Original method return value.
            // Array cache has already been set when GetCarProbability was called.
            __result = AIUtils.cacheArray[DataStore.TAXI];

            if (LifecycleLogging.useTransportLog)
            {
                LifecycleLogging.WriteToLog(LifecycleLogging.TransportLogName, "The same ", ageGroup, " has ", __result, "% chance of using a taxi");
            }

            // Don't execute base method after this.
            return false;
        }
    }
}