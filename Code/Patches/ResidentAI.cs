using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using ColossalFramework;
using HarmonyLib;


namespace LifecycleRebalance
{
    /// <summary>
    /// Harmony pre-emptive Prefix patch for ResidentAI.CanMakeBabies - implements mod's minor fix so that only adult females (of less than age 180) give birth.
    /// </summary>
    [HarmonyPatch(typeof(ResidentAI), nameof(ResidentAI.CanMakeBabies))]
    public static class CanMakeBabiesPatch
    {
        public static bool Prefix(ref bool __result, uint citizenID, ref Citizen data)
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
    [HarmonyPatch(typeof(ResidentAI), "UpdateAge")]
    public static class UpdateAgePatch
    {
        public static bool Prefix(ref bool __result, ref ResidentAI __instance, uint citizenID, ref Citizen data)
        {
            // Method result.
            bool removed = false;

            if ((citizenID % DataStore.lifeSpanMultiplier) == Threading.counter)
            {
                // Local reference.
                CitizenManager citizenManager = Singleton<CitizenManager>.instance;

                if (citizenID == 575960)
                {
                    Logging.Message("foundTarget");
                }

                int num = data.Age + 1;

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
                    if (citizenManager.m_tempOldestOriginalResident < num)
                    {
                        citizenManager.m_tempOldestOriginalResident = num;
                    }
                    if (num == 240)
                    {
                        Singleton<StatisticsManager>.instance.Acquire<StatisticInt32>(StatisticType.FullLifespans).Add(1);
                    }
                }

                data.Age = num;

                // Checking for death and sickness chances.
                // Citizens who are currently moving or currently in a vehicle aren't affected.
                if (data.CurrentLocation != Citizen.Location.Moving && data.m_vehicle == 0)
                {
                    // Local reference.
                    SimulationManager simulationManager = Singleton<SimulationManager>.instance;

                    bool died = false;

                    if (ModSettings.VanillaCalcs)
                    {
                        // Using vanilla lifecycle calculations.
                        int num2 = 240;
                        int num3 = 255;
                        int num4 = Mathf.Max(0, 145 - (100 - data.m_health) * 3);
                        if (num4 != 0)
                        {
                            num2 += num4 / 3;
                            num3 += num4;
                        }
                        if (num >= num2)
                        {
                            bool flag = simulationManager.m_randomizer.Int32(2000u) < 3;
                            died = (simulationManager.m_randomizer.Int32(num2 * 100, num3 * 100) / 100 <= num || flag);
                        }
                    }
                    else
                    {
                        // Using custom lifecycle calculations.
                        // Game defines years as being age divided by 3.5.  Hence, 35 age increments per decade.
                        // Legacy mod behaviour worked on 25 increments per decade.
                        // If older than the maximum index - lucky them, but keep going using that final index.
                        int index = Math.Min((int)(num * ModSettings.decadeFactor), 10);

                        // Calculate 90% - 110%; using 100,000 as 100% (for precision).
                        int modifier = 100000 + ((150 * data.m_health) + (50 * data.m_wellbeing) - 10000);

                        // Death chance is simply if a random number between 0 and the modifier calculated above is less than the survival probability calculation for that decade of life.
                        // Also set maximum age of 400 (~114 years) to be consistent with the base game.
                        died = (simulationManager.m_randomizer.Int32(0, modifier) < DataStore.survivalProbCalc[index]) || num > 400;

                        // Check for sickness chance if they haven't died.
                        if (!died && simulationManager.m_randomizer.Int32(0, modifier) < DataStore.sicknessProbCalc[index])
                        {
                            // Make people sick, if they're unlucky.
                            data.Sick = true;

                            if (Logging.UseSicknessLog)
                            {
                                Logging.WriteToLog(Logging.SicknessLogName, "Citizen became sick with chance factor ", DataStore.sicknessProbCalc[index].ToString());
                            }
                        }
                    }

                    // Handle citizen death.
                    if (died)
                    {
                        // Check if citizen is only remaining parent and there are children.
                        uint unitID = data.GetContainingUnit(citizenID, Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding].m_citizenUnits, CitizenUnit.Flags.Home);
                        CitizenUnit containingUnit = citizenManager.m_units.m_buffer[unitID];

                        // Log if we're doing that.
                        if (Logging.UseDeathLog)
                        {
                            Logging.WriteToLog(Logging.DeathLogName, "Killed citzen ", citizenID.ToString(), " at age ", data.Age.ToString(), " (", ((int)(data.Age / 3.5)).ToString(), " years old) with family ", containingUnit.m_citizen0.ToString(), ", " + containingUnit.m_citizen1.ToString(), ", " + containingUnit.m_citizen2.ToString(), ", ", containingUnit.m_citizen3.ToString(), ", ", containingUnit.m_citizen4.ToString());
                        }

                        // Reverse redirect to access private method Die().
                        DieRev(__instance, citizenID, ref data);

                        // If there are no adults remaining in this CitizenUnit, remove the others, as orphan households end up in simulation purgatory.
                        bool isParent = containingUnit.m_citizen0 == citizenID || containingUnit.m_citizen1 == citizenID;
                        bool singleParent = isParent && (containingUnit.m_citizen0 == 0 || containingUnit.m_citizen1 == 0);
                        bool hasChild = containingUnit.m_citizen2 != 0 || containingUnit.m_citizen3 != 0 || containingUnit.m_citizen4 != 0;

                        if (singleParent && hasChild)
                        {
                            for (int i = 0; i < 2; ++i)
                            {
                                uint currentChild;
                                switch(i)
                                {
                                    case 0:
                                        currentChild = containingUnit.m_citizen2;
                                        break;
                                    case 1:
                                        currentChild = containingUnit.m_citizen3;
                                        break;
                                    default:
                                        currentChild = containingUnit.m_citizen4;
                                        break;
                                }

                                if (currentChild != 0)
                                {
                                    if (Logging.UseDeathLog)
                                    {
                                        Logging.WriteToLog(Logging.DeathLogName, "Removed orphan ", currentChild.ToString());
                                        citizenManager.ReleaseCitizen(currentChild);
                                    }
                                }
                            }
                        }

                        // Chance for 'vanishing corpse' (no need for deathcare).
                        if (!AIUtils.KeepCorpse())
                        {
                            citizenManager.ReleaseCitizen(citizenID);
                            removed = true;
                        }
                    }
                }
            }

            // Original method return value.
            __result = removed;

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
            string message = "FinishSchoolOrWork reverse Harmony patch wasn't applied";
            Logging.Error(message, instance.ToString(), citizenID.ToString(), data.ToString());
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

            string message = "Die reverse Harmony patch wasn't applied";
            Logging.Error(message, instance.ToString(), citizenID.ToString(), data.ToString());
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