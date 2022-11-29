// <copyright file="Patcher.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using HarmonyLib;

    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public class Patcher : PatcherBase
    {
        // Status flag.
        private bool _transportPatchesApplied = false;

        // Target methods for patches that are dynamically applied (instead of through Harmony annotations).
        // These are methods who can be either patched or unpatched depending on option settings.
        // We don't use AccessTools here, as doing so will cause runtime issues if Harmony isn't already installed.
        private MethodInfo OriginalGetCarProbability => AccessTools.Method(typeof(ResidentAI), "GetCarProbability", new Type[] { typeof(ushort), typeof(CitizenInstance).MakeByRefType(), typeof(Citizen.AgeGroup) });

        private MethodInfo OriginalGetBikeProbability => AccessTools.Method(typeof(ResidentAI), "GetBikeProbability");

        private MethodInfo OriginalGetTaxiProbability => AccessTools.Method(typeof(ResidentAI), "GetTaxiProbability");

        // Patch methods for patches that are dynamically applied.
        private MethodInfo GetCarProbabilityPrefix => AccessTools.Method(typeof(ResidentAITransportPatches), nameof(ResidentAITransportPatches.GetCarProbability));

        private MethodInfo GetBikeProbabilityPrefix => AccessTools.Method(typeof(ResidentAITransportPatches), nameof(ResidentAITransportPatches.GetBikeProbability));

        private MethodInfo GetTaxiProbabilityPrefix => AccessTools.Method(typeof(ResidentAITransportPatches), nameof(ResidentAITransportPatches.GetTaxiProbability));

        /// <summary>
        /// Apply custom transport probability patches.
        /// </summary>
        /// <param name="transportActive">True to enable patches, false to disable.</param>
        internal void ApplyTransportPatches(bool transportActive)
        {
            // Don't do anything if mod hasn't been created yet.
            if (Loading.IsCreated)
            {
                if (transportActive)
                {
                    // Don't do anything if patches already applied.
                    if (!_transportPatchesApplied)
                    {
                        Logging.Message("applying transport probability patches");
                        PrefixMethod(OriginalGetCarProbability, GetCarProbabilityPrefix);
                        PrefixMethod(OriginalGetBikeProbability, GetBikeProbabilityPrefix);
                        PrefixMethod(OriginalGetTaxiProbability, GetTaxiProbabilityPrefix);

                        _transportPatchesApplied = true;
                    }
                }
                else if (_transportPatchesApplied)
                {
                    Logging.Message("removing transport probability patches");
                    UnpatchMethod(OriginalGetCarProbability, GetCarProbabilityPrefix);
                    UnpatchMethod(OriginalGetBikeProbability, GetBikeProbabilityPrefix);
                    UnpatchMethod(OriginalGetTaxiProbability, GetTaxiProbabilityPrefix);

                    _transportPatchesApplied = false;
                }
            }
        }
    }
}