using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using ColossalFramework;
using HarmonyLib;


namespace LifecycleRebalance
{
    [HarmonyPatch]

    public static class StartConnectionTransferImplPatch
    {
        const int educationVarIndex = 2;
        const int numVarIndex = 3;
        const int iVarIndex = 16;
        const int flag4VarIndex = 22;

        public static MethodBase TargetMethod() => AccessTools.Method(typeof(OutsideConnectionAI), "StartConnectionTransferImpl");

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            Debug.Log($"LifecycleRebalance Transpiler start");

            var ageArray = generator.DeclareLocal(typeof(int[]));
            var childrenAgeMax = generator.DeclareLocal(typeof(int));
            var childrenAgeMin = generator.DeclareLocal(typeof(int));
            var minAdultAge = generator.DeclareLocal(typeof(int));

            var instructionsEnumerator = instructions.GetEnumerator();
            var instruction = (CodeInstruction)null;
            var forStartFounded = false;
            var left = 1;

            //find for start
            while (instructionsEnumerator.MoveNext() && left > 0)
            {
                instruction = instructionsEnumerator.Current;
                Debug.Log(instruction.ToString());

                yield return instruction;

                if (forStartFounded)
                    left -= 1;
                else if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder builder && builder.LocalIndex == iVarIndex)
                {
                    forStartFounded = true;

                    //set additional local variable value
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Stloc_S, childrenAgeMax.LocalIndex);

                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Stloc_S, childrenAgeMin.LocalIndex);

                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Stloc_S, minAdultAge.LocalIndex);

                    yield return new CodeInstruction(OpCodes.Ldloc_S, numVarIndex);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StartConnectionTransferImplPatch), nameof(StartConnectionTransferImplPatch.GetAgeArray)));
                    yield return new CodeInstruction(OpCodes.Stloc_S, ageArray.LocalIndex);
                }

            }

            //save first for operation lable
            var startForLabels = instructionsEnumerator.Current.labels;

            //skip
            do
            {
                instruction = instructionsEnumerator.Current;
                Debug.Log($"SKIP {instruction}");
            }
            while ((instruction.opcode != OpCodes.Stloc_S || !(instruction.operand is LocalBuilder builder && builder.LocalIndex == flag4VarIndex)) && instructionsEnumerator.MoveNext());

            //call changes method block
            yield return new CodeInstruction(OpCodes.Ldloc_S, iVarIndex) { labels = startForLabels };
            yield return new CodeInstruction(OpCodes.Ldloc_S, educationVarIndex);
            yield return new CodeInstruction(OpCodes.Ldloc_S, ageArray.LocalIndex);
            yield return new CodeInstruction(OpCodes.Ldloca_S, childrenAgeMax.LocalIndex);
            yield return new CodeInstruction(OpCodes.Ldloca_S, childrenAgeMin.LocalIndex);
            yield return new CodeInstruction(OpCodes.Ldloca_S, minAdultAge.LocalIndex);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StartConnectionTransferImplPatch), nameof(StartConnectionTransferImplPatch.Changes)));

            while (instructionsEnumerator.MoveNext())
            {
                instruction = instructionsEnumerator.Current;
                Debug.Log(instruction.ToString());
                yield return instruction;
            }

            Debug.Log($"LifecycleRebalance Transpiler complite");
        }

        public static void Changes(int i, Citizen.Education education, int[] ageArray, ref int childrenAgeMax, ref int childrenAgeMin, ref int minAdultAge)
        {
            int min = ageArray[0];
            int max = ageArray[1];

            if (Debugging.UseImmigrationLog)
            {
                Debugging.WriteToLog(Debugging.ImmigrationLogName, $"{nameof(i)}={i};{nameof(childrenAgeMin)}={childrenAgeMin};{nameof(childrenAgeMax)}={childrenAgeMax};{nameof(minAdultAge)}={minAdultAge};{nameof(min)}={min};{nameof(max)}={max};");
            }

            if (i == 1)
            {
                // Age of second adult - shouldn't be too far from the first. Just because.
                min = Math.Max(minAdultAge - 20, DataStore.incomingAdultAge[0]);
                max = Math.Min(minAdultAge + 20, DataStore.incomingAdultAge[1]);
            }
            else if (i >= 2)
            {
                // Children.
                min = childrenAgeMin;
                max = childrenAgeMax;
            }

            // Calculate actual age randomly between minumum and maxiumum.
            int age = Singleton<SimulationManager>.instance.m_randomizer.Int32(min, max);

            // Adust age brackets for subsequent family members.
            if (i == 0)
            {
                minAdultAge = age;
            }
            else if (i == 1)
            {
                // Restrict to adult age. Young adult is 18 according to National Institutes of Health... even if the young adult section in a library isn't that range.
                minAdultAge = Math.Min(age, minAdultAge);

                // Children should be between 80 and 180 younger than the youngest adult.
                childrenAgeMax = Math.Max(minAdultAge - 80, 0);  // Allow people 10 ticks from 'adulthood' to have kids
                childrenAgeMin = Math.Max(minAdultAge - 178, 0); // Accounting gestation, which isn't simulated yet (2 ticks)
            }

            if (i < 2)
            {
                // Adults.
                // 24% different education levels
                int eduModifier = Singleton<SimulationManager>.instance.m_randomizer.Int32(-12, 12) / 10;
                education += eduModifier;
                if (education < Citizen.Education.Uneducated)
                {
                    education = Citizen.Education.Uneducated;
                }
                else if (education > Citizen.Education.ThreeSchools)
                {
                    education = Citizen.Education.ThreeSchools;
                }
            }
            else
            {
                // Children.
                switch (Citizen.GetAgeGroup(age))
                {
                    case Citizen.AgeGroup.Child:
                        education = Citizen.Education.Uneducated;
                        break;
                    case Citizen.AgeGroup.Teen:
                        education = Citizen.Education.OneSchool;
                        break;
                    default:
                        // Make it that 80% graduate from high school
                        education = (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 100) < 80) ? Citizen.Education.TwoSchools : education = Citizen.Education.OneSchool;
                        break;
                }
            }

            if (Debugging.UseImmigrationLog)
            {
                Debugging.WriteToLog(Debugging.ImmigrationLogName, "Family member " + i + " immigrating with age " + age + " (" + (int)(age / 3.5) + " years old) and education level " + education + ".");
            }
        }

        public static int[] GetAgeArray(int num) => num == 1 ? DataStore.incomingSingleAge : DataStore.incomingAdultAge;
    }
}