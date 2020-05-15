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

                // Checking for death and sickness chances.
                // Citizens who are currently moving or currently in a vehicle aren't affected.
                if (data.CurrentLocation != Citizen.Location.Moving && data.m_vehicle == 0)
                {
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
                            bool flag = Singleton<SimulationManager>.instance.m_randomizer.Int32(2000u) < 3;
                            died = (Singleton<SimulationManager>.instance.m_randomizer.Int32(num2 * 100, num3 * 100) / 100 <= num || flag);
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
                        died = (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, modifier) < DataStore.survivalProbCalc[index]) || num > 400;

                        // Check for sickness chance if they haven't died.
                        if (!died && Singleton<SimulationManager>.instance.m_randomizer.Int32(0, modifier) < DataStore.sicknessProbCalc[index])
                        {
                            // Make people sick, if they're unlucky.
                            data.Sick = true;

                            if (Debugging.UseSicknessLog)
                            {
                                Debugging.WriteToLog(Debugging.SicknessLogName, "Citizen became sick with chance factor " + DataStore.sicknessProbCalc[index] + ".");
                            }
                        }
                    }

                    if (died)
                    {
                        if (Debugging.UseDeathLog)
                        {
                            Debugging.WriteToLog(Debugging.DeathLogName, "Citizen died at age: " + data.Age + " (" + (int)(data.Age / 3.5) + " years old).");
                        }

                        // Reverse redirect to access private method Die().
                        DieRev(__instance, citizenID, ref data);

                        // Chance for 'vanishing corpse' (no need for deathcare).
                        if (!AIUtils.KeepCorpse())
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return true;
                        }
                    }
                }
            }

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
