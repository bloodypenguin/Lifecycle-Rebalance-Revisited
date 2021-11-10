using HarmonyLib;


namespace LifecycleRebalance
{
    /// <summary>
    /// Harmony pre-emptive Prefix patch for Citizen.GetAgeGroup - part of custom age group implementation.
    /// </summary>
    [HarmonyPatch(typeof(Citizen), nameof(Citizen.GetAgeGroup))]
    public static class GetAgeGroupPatch
    {
        // Children - < 13 years = < 45 (rounded down from 45.5)
        // Teens - 13-18 years inclusive = < 66 (rounded down from 66.5)
        // Youg adults - 19-25 years inclusive = < 91

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
    }
}