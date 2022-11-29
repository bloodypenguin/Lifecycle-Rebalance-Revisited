// <copyright file="RealTimePatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using ColossalFramework;
    using HarmonyLib;

    /// <summary>
    /// Harmony patch to ensure that pre-school children don't go shopping when the Real Time mod is involved.
    /// Temporary fix until issue is resolved in Real Time.
    /// </summary>
    [HarmonyPatch(typeof(HumanAI), "FindVisitPlace")]
    public static class RealTimePatch
    {
        /// <summary>
        /// Harmony Prefix patch for HumanAI.FindVisitPlace, to ensure that pre-school children don't go shopping.
        /// </summary>
        /// <param name="citizenID">Citizen ID</param>
        /// <param name="reason">Transfer reason</param>
        /// <returns>False (preempt original method) if citizen is pre-school age, true (execute original method) otherwise</returns>
        public static bool Prefix(uint citizenID, TransferManager.TransferReason reason)
        {
            // Check if this is a request to go shopping.
            if (reason == TransferManager.TransferReason.Shopping || (reason >= TransferManager.TransferReason.ShoppingB && reason <= TransferManager.TransferReason.ShoppingH))
            {
                // Shopping - check if citizen age is pre-school.
                if (Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID].m_age < ModSettings.SchoolStartAge)
                {
                    // Pre-schooler; no shopping.
                    return false;
                }
            }

            // If we got here, there's nothing stopping shopping; continue on to original method.
            return true;
        }
    }
}