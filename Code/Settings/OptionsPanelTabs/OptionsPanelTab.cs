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
        /// <summary>
        /// Gets or sets a value indicating whether panel setup has been completed.
        /// </summary>
        protected bool IsSetup { get; set; } = false;

        /// <summary>
        /// Gets or sets the UIPanel reference.
        /// </summary>
        protected UIPanel Panel { get; set; }

        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal abstract void Setup();
    }
}