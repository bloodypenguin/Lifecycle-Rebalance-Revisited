// <copyright file="Threading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using ColossalFramework;
    using ICities;

    public class Threading : ThreadingExtensionBase
    {
        public static int counter = 0;

        public override void OnBeforeSimulationFrame()
        {
            // CitizenManager.SimulationStepImpl(int subStep)

            // Default aging ticks are per week
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095u) == 0u)
            {
                // Tick from 1 to the multiplier itself. Once over, reset
                if (++counter >= DataStore.lifeSpanMultiplier)
                {
                    counter = 0;
                }
            }
        }
    }
}
