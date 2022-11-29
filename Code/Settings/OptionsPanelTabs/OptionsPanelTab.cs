// <copyright file="OptionsPanelTab.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using ColossalFramework.UI;

    /// <summary>
    /// Base class for options panel tabs.
    /// </summary>
    internal abstract class OptionsPanelTab
    {
        // Status flag.
        protected bool isSetup = false;

        // Panel reference.
        protected UIPanel panel;


        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal abstract void Setup();
    }
}