using Boformer.Redirection;
using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WG_Lifespan
{
    [TargetType(typeof(ResidentAI))]
    public class NewResidentAI : ResidentAI
    {
        public static volatile bool canTick = false;

        [RedirectMethod]
        private bool UpdateAge(uint citizenID, ref Citizen data)
        {
            if (canTick)
            {
                int num = data.Age + 1;
                // Print current date time in game. Singleton<SimulationManager>.instance.m_metaData.m_currentDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                // Threading.sb.Append(citizenID + ": " + num + "\n");

                if (num <= 45)
                {
                    if (num == 15 || num == 45)
                    {
                        FinishSchoolOrWork(citizenID, ref data);
                    }
                }
                else if (num == 90 || num == 180)
                {
                    FinishSchoolOrWork(citizenID, ref data);
                }
                else if ((data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None &&  (num % 15 == 0))
                {
                    FinishSchoolOrWork(citizenID, ref data);
                }

                if ((data.m_flags & Citizen.Flags.Original) != Citizen.Flags.None)
                {
                    CitizenManager instance = Singleton<CitizenManager>.instance;
                    if (instance.m_tempOldestOriginalResident < num)
                    {
                        instance.m_tempOldestOriginalResident = num;
                    }
                    if (num == 240)
                    {
                        Singleton<StatisticsManager>.instance.Acquire<StatisticInt32>(StatisticType.FullLifespans).Add(1);
                    }
                }
                data.Age = num;
                // Can change to make % checks to make it "early death"
                if (num >= 240 && data.CurrentLocation != Citizen.Location.Moving && data.m_vehicle == 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(240, 255) <= num)
                {
                    Die(citizenID, ref data);
                    if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        return true;
                    }
                }
            } // end if canTick
            return false;
        } // end UpdateAge


        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void FinishSchoolOrWork(uint citizenID, ref Citizen data)
        {
            //This line is required to make a large enough method to fit detour assembly code within it
            Debugging.writeDebugToFile("FinishSchoolOrWork not redirected!");
        }


        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Die(uint citizenID, ref Citizen data)
        {
            Debugging.writeDebugToFile("Die not redirected!");
        }
    }
}
