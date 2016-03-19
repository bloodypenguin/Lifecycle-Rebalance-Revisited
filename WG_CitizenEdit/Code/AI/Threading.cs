using ColossalFramework;
using ICities;
using System.Text;

namespace WG_CitizenEdit
{
    public class Threading : ThreadingExtensionBase
    {
        public static int counter = 0;
        public static StringBuilder sb = new StringBuilder();

        public override void OnBeforeSimulationFrame()
        {
            // CitizenManager.SimulationStepImpl(int subStep)

            // Default aging ticks are per week
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095u) == 0u)
            {
                // Tick from 1 to the multiplier itself. Once over, reset
                if (++counter >= DataStore.lifeSpanMultiplier)
                {
                    counter = 0;
                }
Debugging.writeDebugToFile("new counter: " + counter + ". bound: " + DataStore.citizenNumberBounds[Threading.counter] + ", " + DataStore.citizenNumberBounds[Threading.counter + 1]);


Debugging.writeDebugToFile(sb.ToString());
sb.Remove(0, sb.Length);
            }
        }
    }
}
