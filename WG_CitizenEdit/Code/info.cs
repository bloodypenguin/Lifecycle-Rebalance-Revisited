using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;


namespace WG_CitizenEdit
{
    public class ResidentTravelRebalanceMod : IUserMod
    {
        public string Name
        {
            get { return "WG Resident Travel Rebalance"; }
        }
        public string Description
        {
            get { return "Changes the balance in the mode that citizens will travel to and from their locations"; }
        }
    }
}


