// <copyright file="Threading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using ColossalFramework;
    using ICities;

    /// <summary>
    /// Threading for citizen ageing speed.
    /// </summary>
    public class Threading : ThreadingExtensionBase
    {
        /// <summary>
        /// Ageing speed counter.
        /// </summary>
        private static int s_counter = 0;

        /// <summary>
        /// Gets the ageing speed counter.
        /// </summary>
        public static int Counter
        {
            get => s_counter;

            internal set { s_counter = value; }
        }

        /// <summary>
        /// Called by the game before every simulation frame.
        /// </summary>
        public override void OnBeforeSimulationFrame()
        {
            // Default aging ticks are per week
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095u) == 0u)
            {
                // Tick from 1 to the multiplier itself. Once over, reset
                if (++s_counter >= DataStore.LifeSpanMultiplier)
                {
                    s_counter = 0;
                }
            }
        }
    }
}
