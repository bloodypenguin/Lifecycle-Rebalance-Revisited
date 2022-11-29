// <copyright file="LifecycleLogging.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using System.IO;
    using System.Text;
    using AlgernonCommons;

    /// <summary>
    /// Logging utility class.
    /// </summary>
    internal static class LifecycleLogging
    {
        // Logging detail flags.
        internal static bool useDeathLog = false;
        internal static bool useImmigrationLog = false;
        internal static bool useTransportLog = false;
        internal static bool useSicknessLog = false;

        // Custom log names.
        internal static readonly string DeathLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle death log.txt";
        internal static readonly string ImmigrationLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle immigration log.txt";
        internal static readonly string TransportLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle transport log.txt";
        internal static readonly string SicknessLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle sickness log.txt";

        // Stringbuilder for messaging.
        private static StringBuilder message = new StringBuilder(128);

        /// <summary>
        /// Logs a message to a dedicated log file.
        /// </summary>
        /// <param name="filename">Log file to write to</param>
        /// <param name="messages">Message to log (individual strings will be concatenated)</param>
        internal static void WriteToLog(string filename, params object[] messages)
        {
            // Assemble text.
            message.Length = 0;
            for (int i = 0; i < messages.Length; ++i)
            {
                message.Append(messages[i]);
            }

            // Terminating period to confirm end of messaage.
            message.Append(".");

            // Write to file.
            try
            {
                // Open file for writing.
                using (FileStream fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write))

                // Append message.
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine(message);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "custom logging exception");
            }
        }
    }
}
