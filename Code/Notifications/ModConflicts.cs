using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ColossalFramework.Plugins;


namespace LifecycleRebalance
{
    /// <summary>
    /// Class to check for conflicting mods.
    /// </summary>
    public class ModConflicts
    {
        /// <summary>
        /// Checks for mod conflicts and displays user notification if necessary.
        /// </summary>
        /// <returns>True if conflict found, false otherwise</returns>
        public bool CheckConflicts()
        {
            bool conflictDetected = false;
            string conflictName = string.Empty;


            // Check for conflicting mods.
            if (IsModEnabled(654707599ul))
            {
                // Original WG Citizen Lifecycle Rebalance.
                conflictDetected = true;
                conflictName = "WG Citizen Lifecycle Rebalance";
                ErrorNotification.messageText = "Original WG Citizen Lifecycle Rebalance mod detected - Lifecycle Rebalance Revisited is shutting down to protect your game.  Only ONE of these mods can be enabled at the same time; please unsubscribe from WG Citizen Lifecycle Rebalance, which is now deprecated!";
            }
            else if (IsModInstalled(1372431101ul))
            {
                // Painter mod detected.
                conflictDetected = true;
                conflictName = "Painter";
                ErrorNotification.messageText = "The old Painter mod causes problems with the Harmony libraries used by this mod, resulting in random errors.  Please UNSUBSCRIBE from Painter (merely disabling is NOT sufficient); the Repaint mod can be used as a replacement.";
            }
            else if (IsModInstalled("VanillaGarbageBinBlocker"))
            {
                // Garbage Bin Conroller mod detected.
                conflictDetected = true;
                conflictName = "Garbage Bin Controller";
                Debugging.Message("Garbage Bin Controller mod detected - Lifecycle Rebalance Revisited exiting");
                ErrorNotification.messageText = "The Garbage Bin Controller mod causes problems with the Harmony libraries used by this mod, resulting in random errors.  Please UNSUBSCRIBE from Garbage Bin Controller (merely disabling is NOT sufficient).";
            }
            else if (IsModInstalled(2097938060) && IsModInstalled(2027161563))
            {
                // Beta and main version simultaneously installed.
                conflictDetected = true;
                conflictName = "Beta";
                ErrorNotification.messageText = "Lifecycle Rebalance Revisited: both Beta and production versions detected.  Lifecycle Rebalance Revisited is shutting down to protect your game.  Please only subscribe to one of these at a time.";
            }

            // Mod conflict was detected.  Notify the user.
            if (conflictDetected)
            {
                // Show error notification.  Message text has already been set above.
                ErrorNotification notification = new ErrorNotification();
                notification.Create();
                ErrorNotification.headerText = "Mod conflict detected!";
                notification.Show();

                Debugging.Message("incompatible " + conflictName + " mod detected.  Shutting down");
            }

            return conflictDetected;
        }


        /// <summary>
        /// Checks whether the given mod is subscribed via the workshop and enabled.
        /// </summary>
        /// <param name="id">Steam Workshop ID of the mod</param>
        /// <returns></returns>
        public bool IsModEnabled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id && mod.isEnabled));
        }


        /// <summary>
        /// Checks whether the given mod is subscribed via the workshop.
        /// </summary>
        /// <param name="id">Steam Workshop ID of the mod</param>
        /// <returns></returns>
        public bool IsModInstalled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id));
        }


        /// <summary>
        /// Checks to see if another mod is installed, based on a provided assembly name.
        /// </summary>
        /// <param name="assemblyName">Name of the mod assembly</param>
        /// <param name="enabledOnly">True if the mod needs to be enabled for the purposes of this check; false if it doesn't matter</param>
        /// <returns>True if the mod is installed (and, if enabledOnly is true, is also enabled), false otherwise</returns>
        internal bool IsModInstalled(string assemblyName, bool enabledOnly = false)
        {
            // Convert assembly name to lower case.
            string assemblyNameLower = assemblyName.ToLower();

            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.ToLower().Equals(assemblyNameLower))
                    {
                        Debugging.Message("found mod assembly " + assemblyName);
                        if (enabledOnly)
                        {
                            return plugin.isEnabled;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            // If we've made it here, then we haven't found a matching assembly.
            return false;
        }
    }
}