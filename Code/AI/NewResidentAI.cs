using Harmony;
using ColossalFramework;
using System;
// TODO - These two only required for base game code pending conversion to Harmony 2 reverse redirect.
using UnityEngine;
using ColossalFramework.Threading;
using ColossalFramework.PlatformServices;


namespace LifecycleRebalanceRevisited
{
    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("GetCarProbability")]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(CitizenInstance), typeof(Citizen.AgeGroup) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    class NewGetCarProbability
    {
        static bool Prefix(ref int __result, ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            // Cache as best we can. The order of calls is car, bike, taxi
            NewResidentAI.citizenCache = citizenData.m_citizen;  // Not needed, but just in case

            Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[(int)((UIntPtr)citizenData.m_citizen)];
            ushort homeBuilding = citizen.m_homeBuilding;

            ItemClass.SubService subService = ItemClass.SubService.ResidentialLow;
            if (homeBuilding != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)homeBuilding];
                District district = instance.m_districts.m_buffer[instance.GetDistrict(building.m_position)];
                DistrictPolicies.CityPlanning cityPlanningPolicies = district.m_cityPlanningPolicies;

                NewResidentAI.livesInBike = (cityPlanningPolicies & DistrictPolicies.CityPlanning.EncourageBiking) != DistrictPolicies.CityPlanning.None;
                subService = Singleton<BuildingManager>.instance.m_buildings.m_buffer[homeBuilding].Info.GetSubService();
            }

            // Set the cache
            NewResidentAI.cacheArray = NewResidentAI.GetArray(citizen.WealthLevel, subService, ageGroup);

            // Original method return value.
            __result = NewResidentAI.cacheArray[DataStore.CAR];

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("GetBikeProbability")]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(CitizenInstance), typeof(Citizen.AgeGroup) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    class NewGetBikeProbability
    {
        static bool Prefix(ref int __result, ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            int bike = NewResidentAI.livesInBike ? DataStore.bikeIncrease : 0;

            // Original method return value.
            __result = (NewResidentAI.cacheArray[DataStore.BIKE] + bike);

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("GetTaxiProbability")]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(CitizenInstance), typeof(Citizen.AgeGroup) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    class NewGetTaxiProbability
    {
        static bool Prefix(ref int __result, ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            // Original method return value.
            __result = NewResidentAI.cacheArray[DataStore.TAXI];

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("CanMakeBabies")]
    [HarmonyPatch(new Type[] { typeof(uint), typeof(Citizen) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
    class NewCanMakeBabies
    {
        private static bool Prefix(ref bool __result, uint citizenID, ref Citizen data)
        {
            // data.m_family  access group data?
            // Only check child 1 and 2. Don't care about 3, won't fit if there's someone there :)
            //Building home = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding];
            //Randomizer randomizer = new Randomizer((int)citizenID);

            // Unlock males within all groups. Find partner except for initial seeding is the exact same age group, so shortcut is allowed
            __result = !data.Dead &&
                (
                  // Females are now limited to the time that they have aged. Males are excluded from this calculation so females can still have children
                  (Citizen.Gender.Male == Citizen.GetGender(citizenID)) ||
                  (((citizenID % DataStore.lifeSpanMultiplier) == Threading.counter) && (Citizen.GetAgeGroup(data.Age) == Citizen.AgeGroup.Adult))
                )
                && (data.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None;

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("UpdateAge")]
    [HarmonyPatch(new Type[] { typeof(uint), typeof(Citizen) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
    class NewUpdateAge
    {
        private static bool Prefix(ref bool __result, ref ResidentAI __instance, uint citizenID, ref Citizen data)
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
                        NewResidentAI.FinishSchoolOrWork(ref __instance, citizenID, ref data);
                    }
                }
                else if (num == 90 || num == 180)
                {
                    NewResidentAI.FinishSchoolOrWork(ref __instance, citizenID, ref data);
                }
                else if ((data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None && (num % 15 == 0))  // Workspeed multiplier?
                {
                    NewResidentAI.FinishSchoolOrWork(ref __instance, citizenID, ref data);
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

                    // Game defines years as being age divided by 3.5.  Hence, 35 age increments per decade.
                    // If older than the maximum index - lucky them, but keep going using that final index.
                    int index = Math.Min(num / 35, 10);

                    // Calculate 90% - 110%; using 100,000 as 100% (for precision).
                    int modifier = 100000 + ((150 * data.m_health) + (50 * data.m_wellbeing) - 10000);

                    // Death chance is simply if a random number between 0 and the modifier calculated above is less than the survival probability calculation for that decade of life.
                    // Also set maximum age of 400 (~114 years) to be consistent with the base game.
                    bool died = (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, modifier) < DataStore.survivalProbCalc[index]) || num > 400;

                    if (died)
                    {
                        NewResidentAI.Die(citizenID, ref data);

                        // Chance for 'vanishing corpse' (no need for deathcare).
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 99) < DataStore.autoDeadRemovalChance)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return true;
                        }
                    }
                    else if (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, modifier) < DataStore.sicknessProbCalc[index])
                    {
                        // Make people sick, if they're unlucky.
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

            // Original method return value.
            __result = false;

            // Don't execute base method after this.
            return false;
        }
    } // end UpdateAge


    public static class NewResidentAI
    {
        // Cache items with lowest values
        public static uint citizenCache = 0u;
        public static int[] cacheArray;
        public static bool livesInBike = false;


        // Copied from game code as placeholder before Harmony 2 becomes viable (reverse redirect required to access private game method).
        // TODO - convert to reverse redirect when Harmony 2 migration occurs.
        public static void FinishSchoolOrWork(ref ResidentAI __instance, uint citizenID, ref Citizen data)
        {
            if (data.m_workBuilding == 0)
            {
                return;
            }
            if (data.CurrentLocation == Citizen.Location.Work && data.m_homeBuilding != 0)
            {
                data.m_flags &= ~Citizen.Flags.Evacuating;
                __instance.StartMoving(citizenID, ref data, data.m_workBuilding, data.m_homeBuilding);
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            CitizenManager instance2 = Singleton<CitizenManager>.instance;
            uint num = instance.m_buildings.m_buffer[data.m_workBuilding].m_citizenUnits;
            int num2 = 0;
            do
            {
                if (num == 0)
                {
                    return;
                }
                uint nextUnit = instance2.m_units.m_buffer[num].m_nextUnit;
                CitizenUnit.Flags flags = instance2.m_units.m_buffer[num].m_flags;
                if ((flags & (CitizenUnit.Flags.Work | CitizenUnit.Flags.Student)) != 0)
                {
                    if ((flags & CitizenUnit.Flags.Student) != 0)
                    {
                        if (data.RemoveFromUnit(citizenID, ref instance2.m_units.m_buffer[num]))
                        {
                            BuildingInfo info = instance.m_buildings.m_buffer[data.m_workBuilding].Info;
                            if (info.m_buildingAI.GetEducationLevel1())
                            {
                                data.Education1 = true;
                            }
                            if (info.m_buildingAI.GetEducationLevel2())
                            {
                                if (!data.Education1)
                                {
                                    data.Education1 = true;
                                }
                                else
                                {
                                    data.Education2 = true;
                                }
                            }
                            if (info.m_buildingAI.GetEducationLevel3())
                            {
                                if (!data.Education1)
                                {
                                    data.Education1 = true;
                                }
                                else if (!data.Education2)
                                {
                                    data.Education2 = true;
                                }
                                else
                                {
                                    data.Education3 = true;
                                }
                            }
                            data.m_workBuilding = 0;
                            data.m_flags &= ~Citizen.Flags.Student;
                            if ((data.m_flags & Citizen.Flags.Original) != 0 && data.EducationLevel == Citizen.Education.ThreeSchools && instance2.m_fullyEducatedOriginalResidents++ == 0 && Singleton<SimulationManager>.instance.m_metaData.m_disableAchievements != SimulationMetaData.MetaBool.True)
                            {
                                ThreadHelper.dispatcher.Dispatch(delegate
                                {
                                    if (!PlatformService.achievements["ClimbingTheSocialLadder"].achieved)
                                    {
                                        PlatformService.achievements["ClimbingTheSocialLadder"].Unlock();
                                    }
                                });
                            }
                            return;
                        }
                    }
                    else if (data.RemoveFromUnit(citizenID, ref instance2.m_units.m_buffer[num]))
                    {
                        data.m_workBuilding = 0;
                        data.m_flags &= ~Citizen.Flags.Student;
                        return;
                    }
                }
                num = nextUnit;
            }
            while (++num2 <= 524288);
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
        }


        // Copied from game code with additions for logging of deaths and including all age ranges in deaths, not just seniors.
        public static void Die(uint citizenID, ref Citizen data)
        {
            data.Sick = false;
            data.Dead = true;
            data.SetParkedVehicle(citizenID, 0);
            if ((data.m_flags & Citizen.Flags.MovingIn) != 0)
            {
                return;
            }
            ushort num = data.GetBuildingByLocation();
            if (num == 0)
            {
                num = data.m_homeBuilding;
            }
            if (num != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[num].m_position;
                byte district = instance.GetDistrict(position);
                instance.m_districts.m_buffer[district].m_deathData.m_tempCount++;
                if (IsSenior(citizenID))
                {
                    instance.m_districts.m_buffer[district].m_deadSeniorsData.m_tempCount++;
                }
                instance.m_districts.m_buffer[district].m_ageAtDeathData.m_tempCount += (uint)data.Age;
                Debugging.writeLogToFile("Citizen died at age: " + data.Age + "(" + (int)(data.Age / 3.5) + " years).");
            }
        }


        // Copied from game code as placeholder before Harmony 2 becomes viable (reverse redirect required to access private game method).
        // TODO - won't be needed after Harmony 2 reverse redirect applied above.
        private static bool IsSenior(uint citizenID)
        {
            return Citizen.GetAgeGroup(Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID].Age) == Citizen.AgeGroup.Senior;
        }


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
    }
}
