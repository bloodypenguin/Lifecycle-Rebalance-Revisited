// <copyright file="ModUtils.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using ColossalFramework.Plugins;

    /// <summary>
    /// Class that manages interactions with other mods, including compatibility and functionality checks.
    /// </summary>
    internal static class ModUtils
    {
        internal static List<string> conflictingModNames;

        /// <summary>
        /// Checks for any known mod conflicts.
        /// </summary>
        /// <returns>True if a mod conflict was detected, false otherwise</returns>
        internal static bool IsModConflict()
        {
            // Initialise flag and list of conflicting mods.
            bool conflictDetected = false;
            conflictingModNames = new List<string>();

            // Duplicate real pop mod detection.
            bool lifecycleRevisitedFound = false;

            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    switch (assembly.GetName().Name)
                    {
                        case "LifecycleRebalanceRevisited":
                            // Have we already found an instance?
                            if (lifecycleRevisitedFound)
                            {
                                // Yes - flag as duplicate.
                                conflictDetected = true;
                                conflictingModNames.Add("Lifecycle Rebalance Revisited (duplicate)");
                            }
                            // Flag instance as found.
                            lifecycleRevisitedFound = true;
                            break;
                        case "WG_CitizenEdit":
                            // Original WG mod.
                            conflictDetected = true;
                            conflictingModNames.Add("Citizen Lifecycle Rebalance");
                            break;
                        case "ImmigrantEducationMod":
                            // Customizable Education - kudos to the creator who kindly acknowledged the conflict upfont on their workshop page!
                            conflictDetected = true;
                            conflictingModNames.Add("Customizable Education");
                            break;
                        case "VanillaGarbageBinBlocker":
                            // Garbage Bin Controller
                            conflictDetected = true;
                            conflictingModNames.Add("Garbage Bin Controller");
                            break;
                        case "Painter":
                            // Painter - this one is trickier because both Painter and Repaint use Painter.dll (thanks to CO savegame serialization...)
                            if(plugin.userModInstance.GetType().ToString().Equals("Painter.UserMod"))
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Painter");
                            }
                            break;
                    }
                }
            }

            // Was a conflict detected?
            if (conflictDetected)
            {
                // Yes - log each conflict.
                foreach (string conflictingMod in conflictingModNames)
                {
                    Logging.Error("Conflicting mod found: ", conflictingMod);
                }
                Logging.Error("exiting due to mod conflict");
            }

            return conflictDetected;
        }
    }
}
