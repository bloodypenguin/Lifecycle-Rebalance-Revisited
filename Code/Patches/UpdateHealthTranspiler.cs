// <copyright file="UpdateHealthTranspiler.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Harmony transpiler to remove 'vanishing corpse' check from ResidentAI.UpdateHealth and replace it with this mod's custom probabilities.
    /// </summary>
    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("UpdateHealth")]
    public static class UpdateHealthTranspiler
    {
        /// <summary>
        /// Harmony transpiler patching ResidentAI.UpdateHealth.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // The checks we're targeting for removal are fortunately clearly flagged by the call to Die().
            // We're going to remove the immediately following:
            // (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0); { Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID); return true; }
            var codes = new List<CodeInstruction>(instructions);

            // Deal with each of the operands consecutively and independently to avoid risk of error.

            // Stores the number of operands to cut.
            int cutCount = 0;

            // Iterate through each opcode in the CIL, looking for a calls
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand == AccessTools.Method(typeof(ResidentAI), "Die"))
                {
                    // Found call to ResidentAI.Die.
                    // Increment i to start from the following instruction.
                    ++i;

                    // Now, count forward from this operand until we encounter ldc.i4.2; that's the last instruction to be cut.
                    while (codes[i + cutCount].opcode != OpCodes.Ldc_I4_2)
                    {
                        ++cutCount;
                    }

                    // The following instruction is a call to ColossalFramework.Math.Randomizer; we keep the call and replace the operand with our own KeepCorpse method.
                    codes[i + cutCount + 1].operand = AccessTools.Method(typeof(ResidentAIPatches), nameof(ResidentAIPatches.KeepCorpse));

                    Logging.Message("ResidentAI.Die transpiler removing CIL (offset", cutCount, ") from ", i.ToString(), " (", codes[i].opcode, " ", codes[i].operand, " to ", codes[i + cutCount].opcode, ")");

                    // Remove the CIL from the ldarg.0 to the throw (inclusive).
                    // +1 to avoid fencepost error (need to include original instruction as well).
                    codes.RemoveRange(i, cutCount + 1);

                    // We're done with this one - no point in continuing the loop.
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }
}