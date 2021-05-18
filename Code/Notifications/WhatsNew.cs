using System;
using System.Reflection;
using LifecycleRebalance.MessageBox;


namespace LifecycleRebalance
{
    /// <summary>
    /// "What's new" message box.  Based on macsergey's code in Intersection Marking Tool (Node Markup) mod.
    /// </summary>
    internal static class WhatsNew
    {
        // List of versions and associated update message lines (as translation keys).
        private readonly static WhatsNewMessage[] WhatsNewMessages = new WhatsNewMessage[]
        {
            new WhatsNewMessage
            {
                version = new Version("1.5.4.0"),
                messageKeys = true,
                messages = new string[]
                {
                    "LBR_154_NT1",
                    "LBR_154_NT2"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("1.5.2.0"),
                messageKeys = true,
                messages = new string[]
                {
                    "LBR_152_NT1",
                    "LBR_152_NT2",
                    "LBR_152_NT3"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("1.5.1.0"),
                messageKeys = true,
                messages = new string[]
                {
                    "LBR_151_NT1"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("1.5.0.0"),
                messageKeys = true,
                messages = new string[]
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
            ModSettings.whatsNewBetaVersion = WhatsNewMessages[0].betaVersion;
            Configuration<SettingsFile>.Save();

            return true;
        }


        /// <summary>
        /// Check if there's been an update since the last notification, and if so, show the update.
        /// </summary>
        internal static void ShowWhatsNew()
        {
            Logging.KeyMessage("checking for update notifications");

            // Get last notified version and current mod version.
            Version whatsNewVersion = new Version(ModSettings.whatsNewVersion);
            WhatsNewMessage latestMessage = WhatsNewMessages[0];

            // Don't show notification if we're already up to (or ahead of) the first what's new message (including Beta updates).
            if (whatsNewVersion < latestMessage.version || (whatsNewVersion == latestMessage.version && ModSettings.whatsNewBetaVersion < latestMessage.betaVersion))
            {
                // Show messagebox.
                Logging.KeyMessage("showing What's New messagebox");
                WhatsNewMessageBox messageBox = MessageBoxBase.ShowModal<WhatsNewMessageBox>();
                messageBox.Title = LifecycleRebalanceMod.ModName + " " + LifecycleRebalanceMod.Version;
                messageBox.DSAButton.eventClicked += (component, clickEvent) => DontShowAgain();
                messageBox.SetMessages(whatsNewVersion, WhatsNewMessages);
                Logging.KeyMessage("What's New messagebox complete");
            }
        }
    }


    /// <summary>
    /// Version message struct.
    /// </summary>
    public struct WhatsNewMessage
    {
        public Version version;
        public string versionHeader;
        public int betaVersion;
        public bool messageKeys;
        public string[] messages;
    }
}