using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using ColossalFramework;
using HarmonyLib;


namespace LifecycleRebalance
{
    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.GetCarProbability - implements mod's transport probability settings for cars.
    /// </summary>
    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("GetCarProbability")]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(CitizenInstance), typeof(Citizen.AgeGroup) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    class GetCarProbabilityPatch
    {
        static bool Prefix(ref int __result, ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
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

            if (Debugging.UseTransportLog)
            {
                Debugging.WriteToLog(Debugging.TransportLogName, citizen.WealthLevel + "-wealth " + ageGroup + " has " + __result + "% chance of driving.");
            }

            // Don't execute base method after this.
            return false;
        }
    }


    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.GetBikeProbability - implements mod's transport probability settings for bicycles.
    /// </summary>
    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("GetBikeProbability")]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(CitizenInstance), typeof(Citizen.AgeGroup) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    class GetBikeProbabilityPatch
    {
        static bool Prefix(ref int __result, ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            int bike = AIUtils.livesInBike ? DataStore.bikeIncrease : 0;

            // Original method return value.
            // Array cache has already been set when GetCarProbability was called.
            __result = (AIUtils.cacheArray[DataStore.BIKE] + bike);

            if (Debugging.UseTransportLog)
            {
                Debugging.WriteToLog(Debugging.TransportLogName, "The same " + ageGroup + " has " + __result + "% chance of cycling.");
            }

            // Don't execute base method after this.
            return false;
        }
    }


    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.GetTaxiProbability - implements mod's transport probability settings for taxis.
    /// </summary>
    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("GetTaxiProbability")]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(CitizenInstance), typeof(Citizen.AgeGroup) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    class GetTaxiProbabilityPatch
    {
        static bool Prefix(ref int __result, ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
        {
            // Original method return value.
            // Array cache has already been set when GetCarProbability was called.
            __result = AIUtils.cacheArray[DataStore.TAXI];

            if (Debugging.UseTransportLog)
            {
                Debugging.WriteToLog(Debugging.TransportLogName, "The same " + ageGroup + " has " + __result + "% chance of using a taxi.");
            }

            // Don't execute base method after this.
            return false;
        }
    }


    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.CanMakeBabies - implements mod's minor fix so that only adult females (of less than age 180) give birth.
    /// </summary>
    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("CanMakeBabies")]
    [HarmonyPatch(new Type[] { typeof(uint), typeof(Citizen) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
    class CanMakeBabiesPatch
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
                  // Females are now limited to the time that they have aged.  Males are excluded from this calculation so females can still have children.
                  (Citizen.Gender.Male == Citizen.GetGender(citizenID)) ||
                  // Exclude females over default maximum adult age (51.4 years).
                  (data.Age <= 180 &&
                  (((citizenID % DataStore.lifeSpanMultiplier) == Threading.counter) && (Citizen.GetAgeGroup(data.Age) == Citizen.AgeGroup.Adult)))
                )
                && (data.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None;

            // Don't execute base method after this.
            return false;
        }
    }


    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.UpdateAge - implements mod's ageing and deathcare rate functions.
    /// CRITICAL for mod functionality.
    /// </summary>
    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("UpdateAge")]
    [HarmonyPatch(new Type[] { typeof(uint), typeof(Citizen) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
    class UpdateAgePatch
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
                        FinishSchoolOrWorkRev(__instance, citizenID, ref data);
                    }
                }
                else if (num == 90 || num >= ModSettings.retirementAge)
                {
                    FinishSchoolOrWorkRev(__instance, citizenID, ref data);
                }
                else if ((data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None && (num % 15 == 0))  // Workspeed multiplier?
                {
                    FinishSchoolOrWorkRev(__instance, citizenID, ref data);
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
                    // Legacy mod behaviour worked on 25 increments per decade.
                    // If older than the maximum index - lucky them, but keep going using that final index.
                    int index = Math.Min((int)(num * ModSettings.decadeFactor), 10);

                    // Calculate 90% - 110%; using 100,000 as 100% (for precision).
                    int modifier = 100000 + ((150 * data.m_health) + (50 * data.m_wellbeing) - 10000);

                    // Death chance is simply if a random number between 0 and the modifier calculated above is less than the survival probability calculation for that decade of life.
                    // Also set maximum age of 400 (~114 years) to be consistent with the base game.
                    bool died = (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, modifier) < DataStore.survivalProbCalc[index]) || num > 400;

                    if (died)
                    {
                        DieRev(__instance, citizenID, ref data);

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

                        if (Debugging.UseSicknessLog)
                        {
                            Debugging.WriteToLog(Debugging.SicknessLogName, "Citizen became sick with chance factor " + DataStore.sicknessProbCalc[index] + ".");
                        }
                    }
                } // end moving check
            } // end if canTick

            // Original method return value.
            __result = false;

            // Don't execute base method after this.
            return false;
        }


        /// <summary>
        /// Reverse patch for ResidentAI.FinishSchoolOrWork to access private method of original instance.
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="citizenID">ID of this citizen (for game method)</param>
        /// <param name="data">Citizen data (for game method)</param>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(ResidentAI)), "FinishSchoolOrWork")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FinishSchoolOrWorkRev(object instance, uint citizenID, ref Citizen data)
        {
            string message = "Lifecycle Rebalance Revisited: FinishSchoolOrWork reverse Harmony patch wasn't applied.";
            Debug.Log(message);
            throw new NotImplementedException(message);
        }


        /// <summary>
        /// Reverse patch for ResidentAI.Die to access private method of original instance.
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="citizenID">ID of this citizen (for game method)</param>
        /// <param name="data">Citizen data (for game method)</param>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(ResidentAI)), "Die")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DieRev(object instance, uint citizenID, ref Citizen data)
        {
            string message = "Lifecycle Rebalance Revisited: Die reverse Harmony patch wasn't applied.";
            Debug.Log(message);
            throw new NotImplementedException(message);
        }
    }


    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.GetAgeGroup - part of custom retirement age implementation.
    /// Patch is manually applied (and unapplied) depending if custom retirement age setting is active or not.
    /// </summary>
    public static class GetAgeGroupPatch
    {
        public static bool Prefix(ref Citizen.AgeGroup __result, int age)
        {
            if (age < 15)
            {
                __result = Citizen.AgeGroup.Child;
            }
            else if (age < 45)
            {
                __result = Citizen.AgeGroup.Teen;
            }
            else if (age < 90)
            {
                __result = Citizen.AgeGroup.Young;
            }
            else if (age <  ModSettings.retirementAge)
            {
                __result = Citizen.AgeGroup.Adult;
            }
            else
            {
                __result = Citizen.AgeGroup.Senior;
            }

            // Don't execute original method after this.
            return false;
        }
    }
}
