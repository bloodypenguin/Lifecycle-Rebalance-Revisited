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

        // Buffer warning
        public static void bufferWarning(string text)
        {
            sb.AppendLine("Realistic Population Revisited: " + text);
        }

        // Output buffer
        public static void releaseBuffer()
        {
            if (sb.Length > 0)
            {
                Debug.Log(sb.ToString());
                sb.Remove(0, sb.Length);
            }
        }

        // Log to dedicated file.
        public static void writeLogToFile(String text)
        {
            using (FileStream fs = new FileStream(ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Deaths.txt", FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(text);
            }
        }
    }
}
