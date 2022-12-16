// <copyright file="Loading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using ICities;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, Patcher>
    {
        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);

            // Ensure custom transport probability patches are applied, if applicable.
            PatcherManager<Patcher>.Instance.ApplyTransportPatches(ModSettings.Settings.UseTransportModes);

            // Apply sickness probabilities.
            DataStore.CalculateSicknessProbabilities();

            // Prime Threading.counter to continue from frame index.
            int temp = (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex / 4096u);
            Threading.Counter = temp % DataStore.LifeSpanMultiplier;

            PatcherManager<Patcher>.Instance.ListMethods();
        }
    }
}
