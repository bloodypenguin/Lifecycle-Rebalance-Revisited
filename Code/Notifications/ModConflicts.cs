using System;
using System.Linq;
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

            // Check for original WG Citizen Lifecycle Rebalance.
            if (IsModEnabled(654707599ul))
            {
                conflictDetected = true;
                ErrorNotification.messageText = "Original WG Citizen Lifecycle Rebalance mod detected - Lifecycle Rebalance Revisited is shutting down to protect your game.  Only ONE of these mods can be enabled at the same time; please unsubscribe from WG Citizen Lifecycle Rebalance, which is now deprecated!";
            }
            // Otherwise, check for beta.
            else if (IsModInstalled(2097938060))
            {
                conflictDetected = true;
                ErrorNotification.messageText = "Lifecycle Rebalance Revisited BETA detected - Lifecycle Rebalance Revisited is shutting down to protect your game.  Please only subscribe to one of these at a time.";
            }

            // Mod conflict was detected.  Notify the user.
            if (conflictDetected)
            {
                // Show error notification.  Message text has already been set above.
                ErrorNotification notification = new ErrorNotification();
                notification.Create();
                ErrorNotification.headerText = "Mod conflict detected!";
                notification.Show();

                Debug.Log("Lifecycle Rebalance Revisited: incompatible mod detected.  Shutting down.");
            }

            return conflictDetected;
        }


        /// <summary>
        /// Checks whether the given mod is subscribed via the workshop and enabled.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsModEnabled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id && mod.isEnabled));
        }


        /// <summary>
        /// Checks whether the given mod is subscribed via the workshop.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsModInstalled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id));
        }
    }
}