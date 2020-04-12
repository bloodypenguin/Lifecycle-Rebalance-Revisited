using ICities;


namespace LifecycleRebalanceRevisited
{
    public class LifecycleRebalanceRevisitedMod : IUserMod
    {
        public static string version = "1.2";

        public string Name => "Lifecycle Rebalance Revisited v" + version;
        
        public string Description => "Increases and randomises citizen life span, randomises the ages and education levels of immigrants, and changes how citizens travel to and from work.";
    }
}
