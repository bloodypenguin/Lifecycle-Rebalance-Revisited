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
            return !data.Dead &&
                (
                  // Females are now limited to the time that they have aged. Males are excluded from this calculation so females can still have children
                  (Citizen.Gender.Male == Citizen.GetGender(citizenID)) ||
                  (((citizenID % DataStore.lifeSpanMultiplier) == Threading.counter) && (Citizen.GetAgeGroup(data.Age) == Citizen.AgeGroup.Adult))
                )
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
                else if ((data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None && (num % 15 == 0))  // Workspeed multiplier?
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

                    // Potential allow citizens to live up to 274+ ticks
                    int index = num / 25;
                    bool died = true;
                    int modifier = 100000 + ((150 * data.m_health) + (50 * data.m_wellbeing) - 10000); // 90 - 110

                    if (index < DataStore.survivalProbCalc.Length)
                    {
                        int check = DataStore.survivalProbCalc[index];

                        // Find if at hospital to change death chance
                        ushort buildingID = data.GetBuildingByLocation();
                        Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
                        //if (b.Info.m_class.m_service.Equals(ItemClass.Service.HealthCare) && data.Sick)
                        if (data.Sick)
                        {
//Debugging.writeDebugToFile(citizenID + ". Modifier: " + modifier + ", survival: " + DataStore.survivalProbCalc[index] + ", sick: " + DataStore.sicknessProbCalc[index]);
                            // death chance is flat percentage
                            //modifier = (int) (modifier * DataStore.sickDeathChance[index]);
                        }

                        died = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, modifier) < check;
                    }

                    if (died)
                    {
                        Die(citizenID, ref data);
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 99) < DataStore.autoDeadRemovalChance)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return true;
                        }
                    }
                    else if (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, modifier) < DataStore.sicknessProbCalc[index])
                    {
                        data.Sick = true;
                    }
/*
                    else if (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, modifier) < DataStore.emigrateChance[index])
                    {
                        // If only one in there, then it's okay. Otherwise
                        Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding];
                        uint unitID = b.FindCitizenUnit(CitizenUnit.Flags.None, citizenID);
                        CitizenUnit home = Singleton<CitizenManager>.instance.m_units.m_buffer[unitID];
                        if ((home.m_citizen0 != citizenID) || (home.m_citizen1 != citizenID))
                        {
                            // Check if else where
//                            Debugging.writeDebugToFile("Eject family");
                        }
                        // ResidentAI.MoveFamily(uint homeID, ref CitizenUnit data, ushort targetBuilding)
                    } // end die and sick checks
*/
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
    }
}
