using ColossalFramework;
using HarmonyLib;


namespace LifecycleRebalance
{
    /// <summary>
    /// Harmony patch to slowly cycle through buildings and remove citizens with invalid flags.
    /// </summary>
    [HarmonyPatch(typeof(BuildingManager), "SimulationStepImpl")]
    public static class BuildingManagerPatch
    {
        /// <summary>
        /// Sequntial step count; start with a random value to ensure spread of checking over multiple saves.
        /// </summary>
        private static int stepCount = new System.Random().Next(192);

        // Local reference.


        /// <summary>
        /// Harmony Prefix patch to BuildingManager.SimulationStepImpl, to detect and remove any citizens in the building that don't have the 'created' flag set.
        /// </summary>
        /// <param name="__instance">Instance reference</param>
        /// <param name="subStep">Simulation cycle substep.</param>
        public static void Prefix(BuildingManager __instance, int subStep)
        {
            // Don't do anything when subStep is zero.
            if (subStep != 0)
            {
                // Local references.
                CitizenManager citizenManager = Singleton<CitizenManager>.instance;
                Citizen[] citizenBuffer = citizenManager.m_citizens.m_buffer;
                CitizenUnit[] citizenUnits = citizenManager.m_units.m_buffer;
                Building[] buildingBuffer = __instance.m_buildings.m_buffer;

                // Get current framecount to align with parent method's frame, so we avoid unnecessary cache misses.
                int frameCount = (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFF);
                ushort currentBuilding = (ushort)((frameCount * 192) + stepCount);

                // Increment our step counter when frameCount wraps back to zero (evey 256 increments).
                if (frameCount == 0)
                {
                    ++stepCount;

                    // If our step counter is greater than 191, wrap back to zero.
                    if (stepCount > 191)
                    {
                        stepCount = 0;
                    }
                }

                // Iterate through all CitizenUnits in current building.
                uint currentUnit = buildingBuffer[currentBuilding].m_citizenUnits;
                while (currentUnit != 0)
                {
                    // Only interested in home units.
                    if ((citizenUnits[currentUnit].m_flags & CitizenUnit.Flags.Home) != 0)
                    {
                        // Check for citizens with invalid flags in this household.
                        uint thisCitizen = citizenUnits[currentUnit].m_citizen0;
                        if (thisCitizen != 0)
                        {
                            Citizen.Flags citizenFlags = citizenBuffer[thisCitizen].m_flags;
                            if ((citizenFlags & Citizen.Flags.Created) == 0)
                            {
                                RemoveCitizen(citizenManager, ref citizenUnits[currentUnit].m_citizen0, currentUnit, currentBuilding, citizenFlags);
                            }
                        }
                        thisCitizen = citizenUnits[currentUnit].m_citizen1;
                        if (thisCitizen != 0)
                        {
                            Citizen.Flags citizenFlags = citizenBuffer[thisCitizen].m_flags;
                            if ((citizenFlags & Citizen.Flags.Created) == 0)
                            {
                                RemoveCitizen(citizenManager, ref citizenUnits[currentUnit].m_citizen1, currentUnit, currentBuilding, citizenFlags);
                            }
                        }
                        thisCitizen = citizenUnits[currentUnit].m_citizen2;
                        if (thisCitizen != 0)
                        {
                            Citizen.Flags citizenFlags = citizenBuffer[thisCitizen].m_flags;
                            if ((citizenFlags & Citizen.Flags.Created) == 0)
                            {
                                RemoveCitizen(citizenManager, ref citizenUnits[currentUnit].m_citizen2, currentUnit, currentBuilding, citizenFlags);
                            }
                        }
                        thisCitizen = citizenUnits[currentUnit].m_citizen3;
                        if (thisCitizen != 0)
                        {
                            Citizen.Flags citizenFlags = citizenBuffer[thisCitizen].m_flags;
                            if ((citizenFlags & Citizen.Flags.Created) == 0)
                            {
                                RemoveCitizen(citizenManager, ref citizenUnits[currentUnit].m_citizen3, currentUnit, currentBuilding, citizenFlags);
                            }
                        }
                        thisCitizen = citizenUnits[currentUnit].m_citizen4;
                        if (thisCitizen != 0)
                        {
                            Citizen.Flags citizenFlags = citizenBuffer[thisCitizen].m_flags;
                            if ((citizenFlags & Citizen.Flags.Created) == 0)
                            {
                                RemoveCitizen(citizenManager, ref citizenUnits[currentUnit].m_citizen4, currentUnit, currentBuilding, citizenFlags);
                            }
                        }
                    }

                    // Move on to next household in building.
                    currentUnit = citizenUnits[currentUnit].m_nextUnit;
                }
            }
        }


        /// <summary>
        /// Removes a citizen with invalid flags.
        /// </summary>
        /// <param name="citizenManager">CitizenManager instance reference</param>
        /// <param name="citizenID">Citizen ID to remove</param>
        /// <param name="citizenUnit">Owning CitizenUnit ID</param>
        /// <param name="buildingID">Home building ID</param>
        /// <param name="flags">Citizen flags</param>
        private static void RemoveCitizen(CitizenManager citizenManager, ref uint citizenID, uint citizenUnit, ushort buildingID, Citizen.Flags flags)
        {
            // Log messaged.
            Logging.Message("found citizen ", citizenID.ToString(), " in unit ", citizenUnit.ToString(), " of building ", buildingID.ToString(), " with invalid flags ", flags.ToString());

            // Remove citizen and reset reference in CitizenUnit.
            citizenManager.ReleaseCitizen(citizenID);
            citizenID = 0;
        }
    }
}
