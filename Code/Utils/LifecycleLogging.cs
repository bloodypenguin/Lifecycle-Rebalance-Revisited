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
        /// <summary>
        /// Detailed death log filename.
        /// </summary>
        internal static readonly string DeathLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle death log.txt";

        /// <summary>
        /// Detailed immigration log filename.
        /// </summary>
        internal static readonly string ImmigrationLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle immigration log.txt";

        /// <summary>
        /// Detailed transport log filename.
        /// </summary>
        internal static readonly string TransportLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle transport log.txt";

        /// <summary>
        /// Detailed sickness log filename.
        /// </summary>
        internal static readonly string SicknessLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle sickness log.txt";

        // Stringbuilder for messaging.
        private static readonly StringBuilder Message = new StringBuilder(128);

        /// <summary>
        /// Gets or sets a value indicating whether detailed death logging is enabled.
        /// </summary>
        internal static bool UseDeathLog { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether detailed immigration logging is enabled.
        /// </summary>
        internal static bool UseImmigrationLog { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether detailed transport logging is enabled.
        /// </summary>
        internal static bool UseTransportLog { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether detailed sickness logging is enabled.
        /// </summary>
        internal static bool UseSicknessLog { get; set; } = false;

        /// <summary>
        /// Logs a message to a dedicated log file.
        /// </summary>
        /// <param name="filename">Log file to write to</param>
        /// <param name="messages">Message to log (individual strings will be concatenated)</param>
        internal static void WriteToLog(string filename, params object[] messages)
        {
            // Assemble text.
            Message.Length = 0;
            for (int i = 0; i < messages.Length; ++i)
            {
                Message.Append(messages[i]);
            }

            // Terminating period to confirm end of messaage.
            Message.Append(".");

            // Write to file.
            try
            {
                // Open file for writing.
                using (FileStream fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write))

                // Append message.
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine(Message);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "custom logging exception");
            }
        }
    }
}
