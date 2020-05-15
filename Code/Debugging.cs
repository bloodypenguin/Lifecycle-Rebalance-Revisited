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
                Debug.Log("Lifecycle Rebalance Revisited: custom logging exception.");
            }
        }
    }
}
