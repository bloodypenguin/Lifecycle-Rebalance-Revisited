// <copyright file="PanelUtils.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using AlgernonCommons;

    /// <summary>
    /// Utilities for Options Panel UI.
    /// </summary>
    internal static class PanelUtils
    {
        /// <summary>
        /// Updates XML configuration file with current settings.
        /// </summary>
        internal static void SaveXML()
        {
            // Write to file.
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionTwo();
                xml.WriteXML(Loading.s_currentFileLocation);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML configuation file");
            }
        }
    }
}