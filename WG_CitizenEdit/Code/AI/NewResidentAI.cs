using Boformer.Redirection;
using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Runtime.CompilerServices;

namespace WG_CitizenEdit
{
    [TargetType(typeof(ResidentAI))]
    public class NewResidentAI : ResidentAI
    {
        // Cache items with lowest values
        public static uint citizenCache = 0u;
        public static int[] cacheArray;
        public static bool livesInBike = false;
        

        [RedirectMethod]
        private int GetCarProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            // Cache as best we can. The order of calls is car, bike, taxi
            citizenCache = citizenData.m_citizen;  // Not needed, but just in case

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

                livesInBike = (cityPlanningPolicies & DistrictPolicies.CityPlanning.EncourageBiking) != DistrictPolicies.CityPlanning.None;
                subService = Singleton<BuildingManager>.instance.m_buildings.m_buffer[homeBuilding].Info.GetSubService();
            }

            // Set the cache
            cacheArray = getArray(citizen.WealthLevel, subService, ageGroup);
            return cacheArray[DataStore.CAR];
        }


        [RedirectMethod]
        private int GetBikeProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            int bike = livesInBike ? DataStore.bikeIncrease : 0;
            return (cacheArray[DataStore.BIKE] + bike);
        }


        [RedirectMethod]
        private int GetTaxiProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            return cacheArray[DataStore.TAXI];
        }


        [RedirectMethod]
        public bool CanMakeBabies(uint citizenID, ref Citizen data)
        {
            // data.m_family  access group data?
            // Only check child 1 and 2. Don't care about 3, won't fit if there's someone there :)
            //Building home = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding];
            //Randomizer randomizer = new Randomizer((int)citizenID);

            // Unlock males within all groups. Find partner except for initial seeding is the exact same age group, so shortcut is allowed
            return !data.Dead && ((Citizen.Gender.Male == Citizen.GetGender(citizenID)) || (Citizen.GetAgeGroup(data.Age) == Citizen.AgeGroup.Adult)) 
                && (data.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None;
        }


        [RedirectMethod]
        private bool UpdateAge(uint citizenID, ref Citizen data)
        {
            if ((citizenID % DataStore.lifeSpanMultiplier) == Threading.counter)
            {
                int num = data.Age + 1;
                // Threading.sb.Append(citizenID + ": " + num + "\n");
                //Debugging.writeDebugToFile(citizenID + ": " + num + " " + Threading.counter);

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
                else if ((data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None &&  (num % 15 == 0))  // Workspeed multiplier?
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
                    bool died = true;
                    int index = num / 25;

                    if (index < DataStore.survivalProbCalc.Length)
                    {
                       // Potential allow citizens to live up to 274 ticks
                       died = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 100000) > DataStore.survivalProbCalc[index];
                    }

                    if (died)
                    {
                        // Feel like splitting this out to spread it out
                        Die(citizenID, ref data);
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return true;
                        }   
                    }
                    else if (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 100000) < DataStore.sicknessProbCalc[index])
                    {
                        data.BadHealth = 4;
                        data.Sick = true;
                    } // end die and sick checks
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
