using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;
using ColossalFramework;
using System.Runtime.CompilerServices;


namespace WG_CitizenEdit
{
    public class ResidentAIMod
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="citizenData"></param>
        /// <param name="ageGroup"></param>
        /// <returns></returns>
        private int GetBikeProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            int bike = DataStore.livesInBike ? DataStore.bikeIncrease : 0;
//            Debugging.writeDebugToFile(instanceID + " bike: " + DataStore.cacheArray[DataStore.BIKE]);
            return (DataStore.cacheArray[DataStore.BIKE] + bike);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="citizenData"></param>
        /// <param name="ageGroup"></param>
        /// <returns></returns>
        private int GetCarProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            // Cache as best we can. The order of calls is car, bike, taxi
            DataStore.citizenCache = citizenData.m_citizen;  // Not needed, but just in case

            Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[(int)((UIntPtr)citizenData.m_citizen)];
            ushort homeBuilding = citizen.m_homeBuilding;

            ItemClass.SubService subService = ItemClass.SubService.ResidentialLow;
            if (homeBuilding != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)homeBuilding];
                DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int) instance.GetDistrict(building.m_position)].m_cityPlanningPolicies;

                DataStore.livesInBike = (cityPlanningPolicies & DistrictPolicies.CityPlanning.EncourageBiking) != DistrictPolicies.CityPlanning.None;
                subService = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)homeBuilding].Info.GetSubService();
            }

            // Set the cache
            DataStore.cacheArray = getArray(citizen.WealthLevel, subService, ageGroup);
//            Debugging.writeDebugToFile(instanceID + " car: " + DataStore.cacheArray[DataStore.CAR]);
            return DataStore.cacheArray[DataStore.CAR];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="citizenData"></param>
        /// <param name="ageGroup"></param>
        /// <returns></returns>
        private int GetTaxiProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
//            Debugging.writeDebugToFile(instanceID + " taxi: " + DataStore.cacheArray[DataStore.TAXI]);
            return DataStore.cacheArray[DataStore.TAXI];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wealthLevel"></param>
        /// <param name="subService"></param>
        /// <param name="ageGroup"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] getArray(Citizen.Wealth wealthLevel, ItemClass.SubService subService, Citizen.AgeGroup ageGroup)
        {
            int[][] array;
            int densityIndex = (subService == ItemClass.SubService.ResidentialHigh) ? 1 : 0;
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
            return array[(int) ageGroup];
        }
    }
}
