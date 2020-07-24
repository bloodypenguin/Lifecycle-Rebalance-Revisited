using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using HarmonyLib;
using System.Text;


namespace LifecycleRebalance
{
    /// <summary>
    /// Harmony transpiler to remove 'vanishing corpse' check from ResidentAI.UpdateHealth, so that it can be replaced with this mod's custom probabilities.
    /// </summary>
    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch("UpdateHealth")]
    public static class UpdateHealthTranspiler
    {
        /// <summary>
        /// Harmony transpiler patching ResidentAI.UpdateHealth.
        /// </summary>
        /// <param name="instructions">CIL code to alter.</param>
        /// <returns>Patched CIL code</returns>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
                    i++;


                    // Now, count forward from this operand until we encounter ldc.i4.2; that's the last instruction to be cut.
                    while (codes[i + cutCount].opcode != OpCodes.Ldc_I4_2)
                    {
                        cutCount++;
                    }

                    // The following instruction is a call to ColossalFramework.Math.Randomizer; we keep the call and replace the operand with our own KeepCorpse method.
                    codes[i + cutCount + 1].operand = AccessTools.Method(typeof(AIUtils), "KeepCorpse");

                    StringBuilder logMessage = new StringBuilder("Lifecycle Rebalance Revisited: ResidentAI.Die transpiler removing CIL (offset ");
                    logMessage.Append(cutCount);
                    logMessage.Append(") from ");
                    logMessage.Append(i);
                    logMessage.Append(" (");
                    logMessage.Append(codes[i].opcode);
                    logMessage.Append(" ");
                    logMessage.Append(codes[i].operand);
                    logMessage.Append(" to ");
                    logMessage.Append(codes[i + cutCount].opcode);
                    logMessage.Append(" ");
                    logMessage.Append(codes[i + cutCount].operand);
                    logMessage.AppendLine(")");
                    Debugging.Message(logMessage.ToString());

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