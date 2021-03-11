using System;
using ColossalFramework;


#pragma warning disable IDE0060 // Remove unused parameter


namespace LifecycleRebalance
{
    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.GetCarProbability - implements mod's transport probability settings for cars.
    /// Patch is manually applied (and unapplied) depending if custom transport mode probabilities setting is active or not.
    /// </summary>
    public static class GetCarProbabilityPatch
    {
        public static bool Prefix(ref int __result, ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
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

            if (Logging.UseTransportLog)
            {
                Logging.WriteToLog(Logging.TransportLogName, citizen.WealthLevel.ToString(), "-wealth ", ageGroup.ToString(), " has ", __result + "% chance of driving");
            }

            // Don't execute base method after this.
            return false;
        }
    }


    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.GetBikeProbability - implements mod's transport probability settings for bicycles.
    /// Patch is manually applied (and unapplied) depending if custom transport mode probabilities setting is active or not.
    /// </summary>
    public static class GetBikeProbabilityPatch
    {
        public static bool Prefix(ref int __result, ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            int bike = AIUtils.livesInBike ? DataStore.bikeIncrease : 0;

            // Original method return value.
            // Array cache has already been set when GetCarProbability was called.
            __result = (AIUtils.cacheArray[DataStore.BIKE] + bike);

            if (Logging.UseTransportLog)
            {
                Logging.WriteToLog(Logging.TransportLogName, "The same ", ageGroup.ToString(), " has ", __result.ToString(), "% chance of cycling");
            }

            // Don't execute base method after this.
            return false;
        }
    }


    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.GetTaxiProbability - implements mod's transport probability settings for taxis.
    /// Patch is manually applied (and unapplied) depending if custom transport mode probabilities setting is active or not.
    /// </summary>
    public static class GetTaxiProbabilityPatch
    {
        public static bool Prefix(ref int __result, ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            // Original method return value.
            // Array cache has already been set when GetCarProbability was called.
            __result = AIUtils.cacheArray[DataStore.TAXI];

            if (Logging.UseTransportLog)
            {
                Logging.WriteToLog(Logging.TransportLogName, "The same ", ageGroup.ToString(), " has ", __result.ToString(), "% chance of using a taxi");
            }

            // Don't execute base method after this.
            return false;
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter