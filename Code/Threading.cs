using ColossalFramework;
using ICities;


namespace LifecycleRebalance
{
    public class Threading : ThreadingExtensionBase
    {
        public static int counter = 0;

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
            }
        }
    }
}
