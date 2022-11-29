// <copyright file="Citizen.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using HarmonyLib;

    /// <summary>
    ///  Custom age groups for citizens.
    /// </summary>
    [HarmonyPatch(typeof(Citizen))]
    public static class CustomAgeGroups
    {
        /// <summary>
        /// Harmony pre-emptive Prefix patch for Citizen.GetAgeGroup - part of custom age group implementation.
        /// </summary>
        /// <param name="__result">Original method result reference - age group</param>
        /// <param name="age">Citizen age (in age units)</param>
        /// <returns>Always false (never execute original method)</returns>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Citizen.GetAgeGroup))]
        public static bool GetAgeGroup(ref Citizen.AgeGroup __result, int age)
        {
            if (age < ModSettings.TeenStartAge)
            {
                __result = Citizen.AgeGroup.Child;
            }
            else if (age < ModSettings.YoungStartAge)
            {
                __result = Citizen.AgeGroup.Teen;
            }
            else if (age < ModSettings.VanillaAdultAge)
            {
                __result = Citizen.AgeGroup.Young;
            }
            else if (age < ModSettings.retirementAge)
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


        /// <summary>
        /// Harmony pre-emptive Prefix patch for Citizen.GetAgePhase - part of custom age group implementation.
        /// </summary>
        /// <param name="__result">Original method result reference - age group</param>
        /// <param name="education">Citizen education level</param>
        /// <param name="age">Citizen age (in age units)</param>
        /// <returns>Always false (never execute original method)</returns>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Citizen.GetAgePhase))]
        public static bool GetAgePhase(ref Citizen.AgePhase __result, Citizen.Education education, int age)
        {
            if (age < ModSettings.TeenStartAge)
            {
                __result = Citizen.AgePhase.Child;
            }
            else if (age < ModSettings.YoungStartAge)
            {
                __result = (Citizen.AgePhase)((int)Citizen.AgePhase.Teen0 + education);
            }
            else if (age < ModSettings.VanillaAdultAge)
            {
                __result = (Citizen.AgePhase)((int)Citizen.AgePhase.Young0 + education);
            }
            else if (age < ModSettings.retirementAge)
            {
                __result = (Citizen.AgePhase)((int)Citizen.AgePhase.Adult0 + education);
            }
            else
            {
                __result = (Citizen.AgePhase)((int)Citizen.AgePhase.Senior0 + education);
            }

            // Don't execute original method after this.
            return false;
        }


        /// <summary>
        /// Harmony Postfix patch to Citizen.GetCitizenHomeBehaviour to exclude children too young for school from elementary school eligibility counts.
        /// </summary>
        /// <param name="__instance">Instance reference</param>
        /// <param name="behaviour">Citizen behaviour struct reference</param>
        [HarmonyPatch(nameof(Citizen.GetCitizenHomeBehaviour))]
        [HarmonyPostfix]
        public static void GetCitizenHomeBehaviour(Citizen __instance, ref Citizen.BehaviourData behaviour)
        {
            // Only interested in children, who aren't dead, and aren't moving in.
            if (__instance.m_age < ModSettings.SchoolStartAge && !__instance.Dead && (__instance.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None)
            {
                // Undo any assignment to behaviour.m_elementaryEligibleCount done by base method (when Education1 flag isn't set).
                if (!__instance.Education1)
                {
                    --behaviour.m_elementaryEligibleCount;
                }
            }
        }
    }
}