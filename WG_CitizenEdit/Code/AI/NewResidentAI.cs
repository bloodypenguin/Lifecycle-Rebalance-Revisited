using Boformer.Redirection;
using ColossalFramework;
using System;
using System.Runtime.CompilerServices;


namespace WG_CitizenEdit
{
    [TargetType(typeof(ResidentAI))]
    public class NewResidentAI : ResidentAI
    {
        [RedirectMethod]
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
                District district = instance.m_districts.m_buffer[instance.GetDistrict(building.m_position)];
                district.GetHealCapacity();
                DistrictPolicies.CityPlanning cityPlanningPolicies = district.m_cityPlanningPolicies;

                DataStore.livesInBike = (cityPlanningPolicies & DistrictPolicies.CityPlanning.EncourageBiking) != DistrictPolicies.CityPlanning.None;
                subService = Singleton<BuildingManager>.instance.m_buildings.m_buffer[homeBuilding].Info.GetSubService();
            }

            // Set the cache
            DataStore.cacheArray = getArray(citizen.WealthLevel, subService, ageGroup);
            //            Debugging.writeDebugToFile(instanceID + " car: " + DataStore.cacheArray[DataStore.CAR]);
            return DataStore.cacheArray[DataStore.CAR];
        }


        [RedirectMethod]
        private int GetBikeProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            int bike = DataStore.livesInBike ? DataStore.bikeIncrease : 0;
            //            Debugging.writeDebugToFile(instanceID + " bike: " + DataStore.cacheArray[DataStore.BIKE]);
            return (DataStore.cacheArray[DataStore.BIKE] + bike);
        }


        [RedirectMethod]
        private int GetTaxiProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            //            Debugging.writeDebugToFile(instanceID + " taxi: " + DataStore.cacheArray[DataStore.TAXI]);
            return DataStore.cacheArray[DataStore.TAXI];
        }


        [RedirectMethod]
        private bool UpdateAge(uint citizenID, ref Citizen data)
        {
            // Citzen numbers are all over the array
            // Costs a bit more in terms of CPU, but spreads out citizen movement and dying
            if ((DataStore.citizenNumberBounds[Threading.counter] <= citizenID & citizenID < DataStore.citizenNumberBounds[Threading.counter + 1]))
            {
                int num = data.Age + 1;
                // Print current date time in game. Singleton<SimulationManager>.instance.m_metaData.m_currentDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                // Threading.sb.Append(citizenID + ": " + num + "\n");

                if (num <= 45)
                {
                    if (num == 15 || num == 45)
                    {
                        FinishSchoolOrWork(citizenID, ref data);
                    }
                }
                else if (num == 90 || num == 180)
                {
                    FinishSchoolOrWork(citizenID, ref data);
                }
                else if ((data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None &&  (num % 15 == 0))
                {
                    FinishSchoolOrWork(citizenID, ref data);
                }

                if ((data.m_flags & Citizen.Flags.Original) != Citizen.Flags.None)
                {
                    CitizenManager instance = Singleton<CitizenManager>.instance;
                    if (instance.m_tempOldestOriginalResident < num)
                    {
                        instance.m_tempOldestOriginalResident = num;
                    }
                    if (num == 240)
                    {
                        Singleton<StatisticsManager>.instance.Acquire<StatisticInt32>(StatisticType.FullLifespans).Add(1);
                    }
                }

                data.Age = num;
                if (data.CurrentLocation != Citizen.Location.Moving && data.m_vehicle == 0)
                {
                    // Can change to make % checks to make it "early death". This is gender blind
                    bool died = true;
                    int index = num / 25;

                    if (index < DataStore.survivalProbCalc.Length)
                    {
                       // Potential allow citizens to live up to 274 ticks
                       died = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 100000) > DataStore.survivalProbCalc[index];
                    }

                    if (died)
                    {
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 100) > 75)
                        {
                            // 25% make citizen sick instead of killing them. Unless they're going to wrap around to 0
                            data.BadHealth = 4;
Threading.sb.Append("sick : " + num);
                        }
                        else
                        {
Threading.sb.Append("died : " + num);
                            // Feel like splitting this out to spread it out
                            Die(citizenID, ref data);
                            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                            {
                                Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                                return true;
                            }   
                        }
                    } // end died
                } // end moving check
            } // end if canTick
            return false;
        } // end UpdateAge


        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void FinishSchoolOrWork(uint citizenID, ref Citizen data)
        {
            //This line is required to make a large enough method to fit detour assembly code within it
            Debugging.writeDebugToFile("FinishSchoolOrWork not redirected!");
        }


        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Die(uint citizenID, ref Citizen data)
        {
            Debugging.writeDebugToFile("Die not redirected!");
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="wealthLevel"></param>
        /// <param name="subService"></param>
        /// <param name="ageGroup"></param>
        /// <returns></returns>
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
            return array[(int)ageGroup];
        }
    }
}
