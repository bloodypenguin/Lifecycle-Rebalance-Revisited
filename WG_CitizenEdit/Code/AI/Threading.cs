using ColossalFramework;
using ICities;
using System.Text;

namespace WG_CitizenEdit
{
    public class Threading : ThreadingExtensionBase
    {
        public static int counter = 0;
        //public static StringBuilder sb = new StringBuilder();

        public override void OnBeforeSimulationFrame()
        {
            // CitizenManager.SimulationStepImpl(int subStep)

            // Default aging ticks are per week
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095u) == 0u)
            {
                ++counter;

                // Reset during next 'tick'
                if (counter == DataStore.lifeSpanMultiplier)
                {
/*
Debugging.writeDebugToFile("tick\n");
Debugging.writeDebugToFile(sb.ToString());
sb.Remove(0, sb.Length);
*/
                    NewResidentAI.canTick = true;
                    counter = 0;
                }
                else
                {
                    NewResidentAI.canTick = false;
                }
            }
        }
    }
}
