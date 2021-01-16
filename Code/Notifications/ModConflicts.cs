using System;
using System.Linq;
using System.Reflection;
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
                ErrorNotification.messageText = Translations.Translate("LBR_CON_WGO");
            }
            else if (IsModInstalled(1372431101ul))
            {
                // Painter mod detected.
                conflictDetected = true;
                conflictName = "Painter";
                ErrorNotification.messageText = Translations.Translate("LBR_CON_PTR"); ;
            }
            else if (IsModInstalled("VanillaGarbageBinBlocker"))
            {
                // Garbage Bin Conroller mod detected.
                conflictDetected = true;
                conflictName = "Garbage Bin Controller";
                ErrorNotification.messageText = Translations.Translate("LBR_CON_GBC");
            }
            else if (IsModInstalled(2097938060) && IsModInstalled(2027161563))
            {
                // Beta and main version simultaneously installed.
                conflictDetected = true;
                conflictName = "Beta";
                ErrorNotification.messageText = Translations.Translate("LBR_CON_BET");
            }

            // Mod conflict was detected.  Notify the user.
            if (conflictDetected)
            {
                // Show error notification.  Message text has already been set above.
                ErrorNotification notification = new ErrorNotification();
                notification.Create();
                ErrorNotification.headerText = Translations.Translate("LBR_CON");
                notification.Show();

                Logging.Error("incompatible ", conflictName, " mod detected.  Shutting down");
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
                        Logging.Message("found mod assembly ", assemblyName);
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