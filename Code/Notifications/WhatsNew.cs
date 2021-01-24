using System;
using System.Reflection;
using System.Collections.Generic;
using LifecycleRebalance.MessageBox;



namespace LifecycleRebalance
{
    /// <summary>
    /// "What's new" message box.  Based on macsergey's code in Intersection Marking Tool (Node Markup) mod.
    /// </summary>
    internal static class WhatsNew
    {
        // List of versions and associated update message lines (as translation keys).
        private static Dictionary<Version, List<string>> Versions => new Dictionary<Version, List<String>>
        {
            {
                new Version("1.5.2"),
                new List<string>
                {
                    "LBR_152_NT1",
                    "LBR_152_NT2",
                    "LBR_152_NT3"
                }
            },
            {
                new Version("1.5.1"),
                new List<string>
                {
                    "LBR_151_NT1"
                }
            },
            {
                new Version("1.5.0"),
                new List<string>
                {
                    "LBR_150_NT1",
                    "LBR_150_NT2"
                }
            }
        };


        /// <summary>
        /// Close button action.
        /// </summary>
        /// <returns>True (always)</returns>
        public static bool Confirm() => true;

        /// <summary>
        /// 'Don't show again' button action.
        /// </summary>
        /// <returns>True (always)</returns>
        public static bool DontShowAgain()
        {
            // Save current version to settings file.
            ModSettings.whatsNewVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ModSettings.whatsNewBeta = LifecycleRebalance.Beta;
            Configuration<SettingsFile>.Save();

            return true;
        }


        /// <summary>
        /// Check if there's been an update since the last notification, and if so, show the update.
        /// </summary>
        internal static void ShowWhatsNew()
        {
            // Get last notified version and current mod version.
            Version whatsNewVersion = new Version(ModSettings.whatsNewVersion);
            Version modVersion = Assembly.GetExecutingAssembly().GetName().Version;

            // Don't show notification if we're already up to (or ahead of) this version AND there hasn't been a beta update.
            if (whatsNewVersion >= modVersion && ModSettings.whatsNewBeta.Equals(LifecycleRebalance.Beta))
            {
                return;
            }

            // Show messagebox.
            WhatsNewMessageBox messageBox = MessageBoxBase.ShowModal<WhatsNewMessageBox>();
            messageBox.Title = LifecycleRebalance.ModName + " " + LifecycleRebalance.Version;
            messageBox.DSAButton.eventClicked += (component, clickEvent) => DontShowAgain();
            messageBox.SetMessages(whatsNewVersion, Versions);
        }
    }
}