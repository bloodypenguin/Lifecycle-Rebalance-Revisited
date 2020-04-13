using System;
using System.IO;
using ColossalFramework.Plugins;
using System.Text;
using UnityEngine;


namespace LifecycleRebalanceRevisited
{
    class Debugging
    {
        public static StringBuilder sb = new StringBuilder();

        public static bool UseDeathLog;
        public static bool UseImmigrationLog;
        public static bool UseTransportLog;

        public static string DeathLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle death log.txt";
        public static string ImmigrationLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle immigration log.txt";
        public static string TransportLogName = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Lifecycle transport log.txt";


        // Buffer warning
        public static void bufferWarning(string text)
        {
            sb.AppendLine("Realistic Population Revisited: " + text);
        }


        // Output buffer
        public static void ReleaseBuffer()
        {
            if (sb.Length > 0)
            {
                Debug.Log(sb.ToString());
                sb.Remove(0, sb.Length);
            }
        }


        // Check for debugging files and prepare to use them if they exists.
        public static void SetUpDebugging()
        {
            UseDeathLog = File.Exists(DeathLogName);
            UseImmigrationLog = File.Exists(ImmigrationLogName);
            UseTransportLog = File.Exists(TransportLogName);

            Debug.Log("Lifecycle Rebalance Revisited: death logging " + (UseDeathLog ? "enabled" : "disabled") + ", immigration logging " + (UseImmigrationLog ? "enabled" : "disabled") + ", transportation logging " + (UseTransportLog ? "enabled." : "disabled."));
        }


        // Log to dedicated file.
        public static void WriteToLog(string filename, String text)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(text);
            }
        }
    }
}
