using System;
using System.IO;
using System.Text;
using UnityEngine;


namespace LifecycleRebalance
{
    class Debugging
    {
        public static StringBuilder sb = new StringBuilder();

        public static bool UseDeathLog;
        public static bool UseImmigrationLog;
        public static bool UseTransportLog;
        public static bool UseSicknessLog;

        public static string DeathLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle death log.txt";
        public static string ImmigrationLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle immigration log.txt";
        public static string TransportLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle transport log.txt";
        public static string SicknessLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle sickness log.txt";


        // Buffer warning
        public static void bufferWarning(string text)
        {
            sb.AppendLine(text);
        }


        // Output buffer
        public static void ReleaseBuffer()
        {
            if (sb.Length > 0)
            {
                Debugging.Message(sb.ToString());
                sb.Remove(0, sb.Length);
            }
        }


        // Log to dedicated file.
        public static void WriteToLog(string filename, String text)
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(text);
                }
            }
            catch
            {
                Message("custom logging exception");
            }
        }


        /// <summary>
        /// Prints a single-line debugging message to the Unity output log.
        /// </summary>
        /// <param name="message">Message to log</param>
        internal static void Message(params string[] messages)
        {
            // Use StringBuilder for efficiency since we're doing a lot of manipulation here.
            // Start with mod name (to easily identify relevant messages), followed by colon to indicate start of actual message.
            StringBuilder message = new StringBuilder(LifecycleRebalance.ModName);
            message.Append(": ");

            // Add each message parameter.
            for (int i = 0; i < messages.Length; ++i)
            {
                message.Append(messages[i]);
            }

            // Terminating period to confirm end of messaage..
            message.Append(".");

            Debug.Log(message);
        }


        /// <summary>
        /// Prints an exception message to the Unity output log.
        /// </summary>
        /// <param name="message">Message to log</param>
        internal static void LogException(Exception exception)
        {
            // Use StringBuilder for efficiency since we're doing a lot of manipulation here.
            StringBuilder message = new StringBuilder();

            message.AppendLine("caught exception!");
            message.AppendLine("Exception:");
            message.AppendLine(exception.Message);
            message.AppendLine(exception.Source);
            message.AppendLine(exception.StackTrace);

            // Log inner exception as well, if there is one.
            if (exception.InnerException != null)
            {
                message.AppendLine("Inner exception:");
                message.AppendLine(exception.InnerException.Message);
                message.AppendLine(exception.InnerException.Source);
                message.AppendLine(exception.InnerException.StackTrace);
            }

            // Write to log.
            Message(message.ToString());
        }
    }
}
