using System.Reflection;
using UnityEngine;
using HarmonyLib;
using CitiesHarmony.API;


namespace LifecycleRebalance
{
    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public static class Patcher
    {
        // Unique harmony identifier.
        private const string harmonyID = "com.github.algernon-A.csl.lifecyclerebalancerevisited";

        // Flag.
        public static bool patched = false;


        /// <summary>
        /// Apply all Harmony patches.
        /// </summary>
        public static void PatchAll()
        {
            // Don't do anything if already patched.
            if (!patched)
            {
                // Ensure Harmony is ready before patching.
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Debug.Log("Lifecycle Rebalance Revisited v" + LifecycleRebalance.Version + ": deploying Harmony patches.");

                    // Apply all annotated patches and update flag.
                    Harmony harmonyInstance = new Harmony(harmonyID);
                    harmonyInstance.PatchAll();
                    patched = true;
                }
                else
                {
                    Debug.Log("Lifecycle Rebalance Revisited: Harmony not ready.");
                }
            }
        }


        public static void UnpatchAll()
        {
            // Only unapply if patches appplied.
            if (patched)
            {
                Debug.Log("Lifecycle Rebalance Revisited: reverting Harmony patches.");

                // Unapply patches, but only with our HarmonyID.
                Harmony harmonyInstance = new Harmony(harmonyID);
                harmonyInstance.UnpatchAll(harmonyID);
                patched = false;
            }
        }


        private static MethodInfo OriginalMethod => typeof(Citizen).GetMethod("GetAgeGroup");

        public static void ApplyGetAgeGroup()
        {
            Harmony harmonyInstance = new Harmony(harmonyID);

            // Check if the patch is already installed before proceeding.
            if (!IsGetAgeGroupInstalled(harmonyInstance))
            {
                Debug.Log("Lifecycle Rebalance Revisited: applying GetAgeGroup patch.");
                var getAgePrefix = typeof(GetAgeGroupPatch).GetMethod("Prefix");
                harmonyInstance.Patch(OriginalMethod, new HarmonyMethod(getAgePrefix), null);
            }
            else
            {
                Debug.Log("Lifecycle Rebalance Revisited: GetAgeGroup patch already applied, doing nothing.");
            }
        }


        public static void RevertGetAgeGroup()
        {
            Harmony harmonyInstance = new Harmony(harmonyID);

            // Check if the patch is installed before proceeding.
            if (IsGetAgeGroupInstalled(harmonyInstance))
            {
                Debug.Log("Lifecycle Rebalance Revisited: removing GetAgeGroup patch.");
                harmonyInstance.Unpatch(OriginalMethod, typeof(GetAgeGroupPatch).GetMethod("Prefix"));
            }
            else
            {
                Debug.Log("Lifecycle Rebalance Revisited: GetAgeGroup patch not applied, doing nothing.");
            }
        }


        public static bool IsGetAgeGroupInstalled(Harmony harmonyInstance)
        {
            var patches = Harmony.GetPatchInfo(OriginalMethod);
            if (patches != null)
            {
                foreach (var patch in patches.Prefixes)
                {
                    if (patch.owner == Patcher.harmonyID)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}