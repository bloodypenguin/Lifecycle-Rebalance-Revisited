using ICities;


namespace LifecycleRebalanceRevisited
{
    public class ResidentTravelRebalanceMod : IUserMod
    {
        public string Name
        {
            get { return "Lifecycle Rebalance Revisited 1.1"; }
        }
        public string Description
        {
            get { return "Increases and randomises citizen life span, randomises the ages and education levels of immigrants, and changes how citizens travel to and from work."; }
        }
    }
}
