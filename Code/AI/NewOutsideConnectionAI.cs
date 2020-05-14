using ColossalFramework;
using System;
using UnityEngine;
using HarmonyLib;


namespace LifecycleRebalance
{
    //[HarmonyPatch(typeof(OutsideConnectionAI))]
    //[HarmonyPatch("StartConnectionTransferImpl")]
    //[HarmonyPatch(new Type[] { typeof(ushort), typeof(Building), typeof(TransferManager.TransferReason), typeof(TransferManager.TransferOffer), typeof(int), typeof(int), typeof(int) },
    //    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
    public class NewStartConnectionTransferImpl
    {
        // Copied from game code. Ugh.... 
        // OutsideConnectionAI
        // // Allow AdvancedOutsideConnection's prefix to execute before this one.
        //[HarmonyAfter(new string[] { "connection.outside.advanced" })]
        private static bool Prefix(bool __result, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer, int touristFactor0, int touristFactor1, int touristFactor2)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            VehicleInfo vehicleInfo = null;
            Citizen.Education education = Citizen.Education.Uneducated;
            int num = 0;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            if (material == TransferManager.TransferReason.DummyCar)
            {
                ushort building = offer.Building;
                if (building != 0)
                {
                    Vector3 position = instance.m_buildings.m_buffer[building].m_position;
                    if (Vector3.SqrMagnitude(position - data.m_position) > 40000f)
                    {
                        flag2 = true;
                        switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(39u))
                        {
                            case 0:
                                material = TransferManager.TransferReason.Ore;
                                break;
                            case 1:
                                material = TransferManager.TransferReason.Coal;
                                break;
                            case 2:
                                material = TransferManager.TransferReason.Oil;
                                break;
                            case 3:
                                material = TransferManager.TransferReason.Petrol;
                                break;
                            case 4:
                                material = TransferManager.TransferReason.Grain;
                                break;
                            case 5:
                                material = TransferManager.TransferReason.Food;
                                break;
                            case 6:
                                material = TransferManager.TransferReason.Logs;
                                break;
                            case 7:
                                material = TransferManager.TransferReason.Lumber;
                                break;
                            case 8:
                                material = TransferManager.TransferReason.Goods;
                                break;
                            case 9:
                                material = TransferManager.TransferReason.Goods;
                                break;
                            case 10:
                                material = TransferManager.TransferReason.Single0;
                                break;
                            case 11:
                                material = TransferManager.TransferReason.Single1;
                                break;
                            case 12:
                                material = TransferManager.TransferReason.Single2;
                                break;
                            case 13:
                                material = TransferManager.TransferReason.Single3;
                                break;
                            case 14:
                                material = TransferManager.TransferReason.Single0B;
                                break;
                            case 15:
                                material = TransferManager.TransferReason.Single1B;
                                break;
                            case 16:
                                material = TransferManager.TransferReason.Single2B;
                                break;
                            case 17:
                                material = TransferManager.TransferReason.Single3B;
                                break;
                            case 18:
                                material = TransferManager.TransferReason.DummyCar;
                                break;
                            case 19:
                                material = TransferManager.TransferReason.Family0;
                                break;
                            case 20:
                                material = TransferManager.TransferReason.Family1;
                                break;
                            case 21:
                                material = TransferManager.TransferReason.Family2;
                                break;
                            case 22:
                                material = TransferManager.TransferReason.Family3;
                                break;
                            case 23:
                                material = TransferManager.TransferReason.Shopping;
                                break;
                            case 24:
                                material = TransferManager.TransferReason.ShoppingB;
                                break;
                            case 25:
                                material = TransferManager.TransferReason.ShoppingC;
                                break;
                            case 26:
                                material = TransferManager.TransferReason.ShoppingD;
                                break;
                            case 27:
                                material = TransferManager.TransferReason.ShoppingE;
                                break;
                            case 28:
                                material = TransferManager.TransferReason.ShoppingF;
                                break;
                            case 29:
                                material = TransferManager.TransferReason.ShoppingG;
                                break;
                            case 30:
                                material = TransferManager.TransferReason.ShoppingH;
                                break;
                            case 31:
                                material = TransferManager.TransferReason.Entertainment;
                                break;
                            case 32:
                                material = TransferManager.TransferReason.EntertainmentB;
                                break;
                            case 33:
                                material = TransferManager.TransferReason.EntertainmentC;
                                break;
                            case 34:
                                material = TransferManager.TransferReason.EntertainmentD;
                                break;
                            case 35:
                                material = TransferManager.TransferReason.TouristA;
                                break;
                            case 36:
                                material = TransferManager.TransferReason.TouristB;
                                break;
                            case 37:
                                material = TransferManager.TransferReason.TouristC;
                                break;
                            case 38:
                                material = TransferManager.TransferReason.TouristD;
                                break;
                        }
                    }
                }
            }
            switch (material)
            {
                case TransferManager.TransferReason.Ore:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOre, ItemClass.Level.Level2);
                    break;
                case TransferManager.TransferReason.Coal:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOre, ItemClass.Level.Level1);
                    break;
                case TransferManager.TransferReason.Oil:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOil, ItemClass.Level.Level2);
                    break;
                case TransferManager.TransferReason.Petrol:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOil, ItemClass.Level.Level1);
                    break;
                case TransferManager.TransferReason.Grain:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialFarming, ItemClass.Level.Level2);
                    break;
                case TransferManager.TransferReason.Food:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialFarming, ItemClass.Level.Level1);
                    break;
                case TransferManager.TransferReason.Logs:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialForestry, ItemClass.Level.Level2);
                    break;
                case TransferManager.TransferReason.Lumber:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialForestry, ItemClass.Level.Level1);
                    break;
                case TransferManager.TransferReason.Goods:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialGeneric, ItemClass.Level.Level1);
                    break;
                case TransferManager.TransferReason.SortedMail:
                case TransferManager.TransferReason.IncomingMail:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPost, ItemClass.Level.Level5);
                    break;
                case TransferManager.TransferReason.UnsortedMail:
                case TransferManager.TransferReason.OutgoingMail:
                    flag3 = true;
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPost, ItemClass.Level.Level5);
                    break;
                case TransferManager.TransferReason.Single0:
                case TransferManager.TransferReason.Single0B:
                    education = Citizen.Education.Uneducated;
                    num = 1;
                    break;
                case TransferManager.TransferReason.Single1:
                case TransferManager.TransferReason.Single1B:
                    education = Citizen.Education.OneSchool;
                    num = 1;
                    break;
                case TransferManager.TransferReason.Single2:
                case TransferManager.TransferReason.Single2B:
                    education = Citizen.Education.TwoSchools;
                    num = 1;
                    break;
                case TransferManager.TransferReason.Single3:
                case TransferManager.TransferReason.Single3B:
                    education = Citizen.Education.ThreeSchools;
                    num = 1;
                    break;
                case TransferManager.TransferReason.Family0:
                    education = Citizen.Education.Uneducated;
                    num = Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 5);
                    break;
                case TransferManager.TransferReason.Family1:
                    education = Citizen.Education.OneSchool;
                    num = Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 5);
                    break;
                case TransferManager.TransferReason.Family2:
                    education = Citizen.Education.TwoSchools;
                    num = Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 5);
                    break;
                case TransferManager.TransferReason.Family3:
                    education = Citizen.Education.ThreeSchools;
                    num = Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 5);
                    break;
                case TransferManager.TransferReason.Shopping:
                case TransferManager.TransferReason.Entertainment:
                case TransferManager.TransferReason.ShoppingB:
                case TransferManager.TransferReason.ShoppingC:
                case TransferManager.TransferReason.ShoppingD:
                case TransferManager.TransferReason.ShoppingE:
                case TransferManager.TransferReason.ShoppingF:
                case TransferManager.TransferReason.ShoppingG:
                case TransferManager.TransferReason.ShoppingH:
                case TransferManager.TransferReason.EntertainmentB:
                case TransferManager.TransferReason.EntertainmentC:
                case TransferManager.TransferReason.EntertainmentD:
                case TransferManager.TransferReason.TouristA:
                case TransferManager.TransferReason.TouristB:
                case TransferManager.TransferReason.TouristC:
                case TransferManager.TransferReason.TouristD:
                    flag = true;
                    break;
                case TransferManager.TransferReason.DummyTrain:
                    if (offer.Building != buildingID)
                    {
                        flag2 = true;
                        vehicleInfo = ((Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) != 0) ? Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain, ItemClass.Level.Level1) : Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain, ItemClass.Level.Level4));
                    }
                    break;
                case TransferManager.TransferReason.DummyShip:
                    if (offer.Building != buildingID)
                    {
                        flag2 = true;
                        vehicleInfo = ((Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) != 0) ? Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, ItemClass.Level.Level1) : Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, ItemClass.Level.Level4));
                    }
                    break;
                case TransferManager.TransferReason.DummyPlane:
                    if (offer.Building != buildingID)
                    {
                        flag2 = true;
                        vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane, ItemClass.Level.Level1);
                    }
                    break;
                case TransferManager.TransferReason.DummyCar:
                    if (Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Trolleybus) && offer.Building != buildingID && Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                    {
                        vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportBus, ItemClass.Level.Level3);
                    }
                    break;
                default:
                    // Original method return value.
                    __result = false;

                    // Don't execute base method after this.
                    return false;
            }
            if (num != 0)
            {
                CitizenManager instance2 = Singleton<CitizenManager>.instance;
                ushort building2 = offer.Building;
                if (building2 != 0)
                {
                    uint num2 = 0u;
                    if (!flag2)
                    {
                        num2 = instance.m_buildings.m_buffer[building2].GetEmptyCitizenUnit(CitizenUnit.Flags.Home);
                    }
                    if (num2 != 0 || flag2)
                    {
                        int family = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                        ushort num3 = 0;
                        Citizen.Gender gender = Citizen.Gender.Male;

                        // Start of changes! ------------------------------------------------------- -------------------------------------------------------
                        int[] ageArray = num == 1 ? DataStore.incomingSingleAge : DataStore.incomingAdultAge;

                        int childrenAgeMax = 0;
                        int childrenAgeMin = 0;
                        int minAdultAge = 0;

                        // i is is the family member number for this incoming family.  0 is primary adult, 1 is secondary adults, and after that are children.
                        for (int i = 0; i < num; i++)
                        {
                            uint citizen = 0u;
                            //int min = (i >= 2) ? 0 : 90;
                            //int max = (i >= 2) ? 15 : 105;
                            int min = ageArray[0];
                            int max = ageArray[1];

                            if (i == 1)
                            {
                                // Age of second adult - shouldn't be too far from the first. Just because.
                                min = Math.Max(minAdultAge - 20, DataStore.incomingAdultAge[0]);
                                max = Math.Min(minAdultAge + 20, DataStore.incomingAdultAge[1]);
                            }
                            else if (i >= 2)
                            {
                                // Children.
                                min = childrenAgeMin;
                                max = childrenAgeMax;
                            }

                            // Calculate actual age randomly between minumum and maxiumum.
                            int age = Singleton<SimulationManager>.instance.m_randomizer.Int32(min, max);

                            // Adust age brackets for subsequent family members.
                            if (i == 0)
                            {
                                minAdultAge = age;
                            }
                            else if (i == 1)
                            {
                                // Restrict to adult age. Young adult is 18 according to National Institutes of Health... even if the young adult section in a library isn't that range.
                                minAdultAge = Math.Min(age, minAdultAge);

                                // Children should be between 80 and 180 younger than the youngest adult.
                                childrenAgeMax = Math.Max(minAdultAge - 80, 0);  // Allow people 10 ticks from 'adulthood' to have kids
                                childrenAgeMin = Math.Max(minAdultAge - 178, 0); // Accounting gestation, which isn't simulated yet (2 ticks)
                            }

                            Citizen.Education education2 = education;
                            if (i < 2)
                            {
                                // Adults.
                                // 24% different education levels
                                int eduModifier = Singleton<SimulationManager>.instance.m_randomizer.Int32(-12, 12) / 10;
                                education2 += eduModifier;
                                if (education2 < Citizen.Education.Uneducated)
                                {
                                    education2 = Citizen.Education.Uneducated;
                                }
                                else if (education2 > Citizen.Education.ThreeSchools)
                                {
                                    education2 = Citizen.Education.ThreeSchools;
                                }
                            }
                            else
                            {
                                // Children.
                                switch (Citizen.GetAgeGroup(age))
                                {
                                    case Citizen.AgeGroup.Child:
                                        education2 = Citizen.Education.Uneducated;
                                        break;
                                    case Citizen.AgeGroup.Teen:
                                        education2 = Citizen.Education.OneSchool;
                                        break;
                                    default:
                                        // Make it that 80% graduate from high school
                                        education2 = (Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 100) < 80) ? Citizen.Education.TwoSchools : education2 = Citizen.Education.OneSchool;
                                        break;
                                }
                            }

                            if (Debugging.UseImmigrationLog)
                            {
                                Debugging.WriteToLog(Debugging.ImmigrationLogName, "Family member " + i + " immigrating with age " + age + " (" + (int)(age / 3.5) + " years old) and education level " + education2 +".");
                            }

                            // End of changes? --------------------------------------------------------------------------------------------------------------
                            if (!((i != 1) ? instance2.CreateCitizen(out citizen, age, family, ref Singleton<SimulationManager>.instance.m_randomizer) : instance2.CreateCitizen(gender: (Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) >= 5) ? ((gender == Citizen.Gender.Male) ? Citizen.Gender.Female : Citizen.Gender.Male) : gender, citizen: out citizen, age: age, family: family, r: ref Singleton<SimulationManager>.instance.m_randomizer)))
                            {
                                break;
                            }
                            if (i == 0)
                            {
                                gender = Citizen.GetGender(citizen);
                            }
                            if (education2 >= Citizen.Education.OneSchool)
                            {
                                instance2.m_citizens.m_buffer[citizen].Education1 = true;
                            }
                            if (education2 >= Citizen.Education.TwoSchools)
                            {
                                instance2.m_citizens.m_buffer[citizen].Education1 = true;
                                instance2.m_citizens.m_buffer[citizen].Education2 = true;
                            }
                            if (education2 >= Citizen.Education.ThreeSchools)
                            {
                                instance2.m_citizens.m_buffer[citizen].Education1 = true;
                                instance2.m_citizens.m_buffer[citizen].Education2 = true;
                                instance2.m_citizens.m_buffer[citizen].Education3 = true;
                            }
                            if (flag2)
                            {
                                instance2.m_citizens.m_buffer[citizen].m_flags |= Citizen.Flags.DummyTraffic;
                            }
                            else
                            {
                                instance2.m_citizens.m_buffer[citizen].SetHome(citizen, 0, num2);
                            }
                            instance2.m_citizens.m_buffer[citizen].m_flags |= Citizen.Flags.MovingIn;
                            CitizenInfo citizenInfo = instance2.m_citizens.m_buffer[citizen].GetCitizenInfo(citizen);
                            if ((object)citizenInfo != null && instance2.CreateCitizenInstance(out ushort instance3, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo, citizen))
                            {
                                if (num3 == 0)
                                {
                                    citizenInfo.m_citizenAI.SetSource(instance3, ref instance2.m_instances.m_buffer[instance3], buildingID);
                                    citizenInfo.m_citizenAI.SetTarget(instance3, ref instance2.m_instances.m_buffer[instance3], building2);
                                    num3 = instance3;
                                }
                                else
                                {
                                    citizenInfo.m_citizenAI.SetSource(instance3, ref instance2.m_instances.m_buffer[instance3], buildingID);
                                    citizenInfo.m_citizenAI.JoinTarget(instance3, ref instance2.m_instances.m_buffer[instance3], num3);
                                }
                                instance2.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Moving;
                            }
                        }
                    }
                }
            }
            if (flag)
            {
                CitizenManager instance4 = Singleton<CitizenManager>.instance;
                ushort building3 = offer.Building;
                ushort transportLine = offer.TransportLine;
                if (building3 != 0)
                {
                    int family2 = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                    uint num4 = 0u;
                    if (!flag2)
                    {
                        num4 = instance.m_buildings.m_buffer[building3].GetEmptyCitizenUnit(CitizenUnit.Flags.Visit);
                    }
                    if (num4 != 0 || flag2)
                    {
                        int age2 = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 240);
                        Citizen.Wealth wealth = Citizen.Wealth.High;
                        int num5 = touristFactor0 + touristFactor1 + touristFactor2;
                        if (num5 != 0)
                        {
                            int num6 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num5);
                            if (num6 < touristFactor0)
                            {
                                wealth = Citizen.Wealth.Low;
                            }
                            else if (num6 < touristFactor0 + touristFactor1)
                            {
                                wealth = Citizen.Wealth.Medium;
                            }
                        }
                        if (instance4.CreateCitizen(out uint citizen2, age2, family2, ref Singleton<SimulationManager>.instance.m_randomizer))
                        {
                            instance4.m_citizens.m_buffer[citizen2].m_flags |= Citizen.Flags.Tourist;
                            instance4.m_citizens.m_buffer[citizen2].m_flags |= Citizen.Flags.MovingIn;
                            instance4.m_citizens.m_buffer[citizen2].WealthLevel = wealth;
                            if (flag2)
                            {
                                instance4.m_citizens.m_buffer[citizen2].m_flags |= Citizen.Flags.DummyTraffic;
                            }
                            else
                            {
                                instance4.m_citizens.m_buffer[citizen2].SetVisitplace(citizen2, 0, num4);
                            }
                            CitizenInfo citizenInfo2 = instance4.m_citizens.m_buffer[citizen2].GetCitizenInfo(citizen2);
                            if ((object)citizenInfo2 != null && instance4.CreateCitizenInstance(out ushort instance5, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo2, citizen2))
                            {
                                citizenInfo2.m_citizenAI.SetSource(instance5, ref instance4.m_instances.m_buffer[instance5], buildingID);
                                citizenInfo2.m_citizenAI.SetTarget(instance5, ref instance4.m_instances.m_buffer[instance5], building3);
                                instance4.m_citizens.m_buffer[citizen2].CurrentLocation = Citizen.Location.Moving;
                            }
                            if (!flag2)
                            {
                                StatisticBase statisticBase = Singleton<StatisticsManager>.instance.Acquire<StatisticArray>(StatisticType.IncomingTourists);
                                statisticBase = statisticBase.Acquire<StatisticInt32>((int)wealth, 3);
                                statisticBase.Add(1);
                            }
                        }
                    }
                }
                else if (transportLine != 0)
                {
                    TransportManager instance6 = Singleton<TransportManager>.instance;
                    int num7 = instance6.m_lines.m_buffer[transportLine].CountStops(transportLine);
                    if (num7 > 0)
                    {
                        int index = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num7);
                        ushort stop = instance6.m_lines.m_buffer[transportLine].GetStop(index);
                        if (stop != 0)
                        {
                            int family3 = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                            int age3 = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 240);
                            Citizen.Wealth wealth2 = Citizen.Wealth.High;
                            int num8 = touristFactor0 + touristFactor1 + touristFactor2;
                            if (num8 != 0)
                            {
                                int num9 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num8);
                                if (num9 < touristFactor0)
                                {
                                    wealth2 = Citizen.Wealth.Low;
                                }
                                else if (num9 < touristFactor0 + touristFactor1)
                                {
                                    wealth2 = Citizen.Wealth.Medium;
                                }
                            }
                            if (instance4.CreateCitizen(out uint citizen3, age3, family3, ref Singleton<SimulationManager>.instance.m_randomizer))
                            {
                                instance4.m_citizens.m_buffer[citizen3].m_flags |= Citizen.Flags.Tourist;
                                instance4.m_citizens.m_buffer[citizen3].m_flags |= Citizen.Flags.MovingIn;
                                instance4.m_citizens.m_buffer[citizen3].WealthLevel = wealth2;
                                CitizenInfo citizenInfo3 = instance4.m_citizens.m_buffer[citizen3].GetCitizenInfo(citizen3);
                                if ((object)citizenInfo3 != null && instance4.CreateCitizenInstance(out ushort instance7, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo3, citizen3))
                                {
                                    citizenInfo3.m_citizenAI.SetSource(instance7, ref instance4.m_instances.m_buffer[instance7], buildingID);
                                    citizenInfo3.m_citizenAI.SetTarget(instance7, ref instance4.m_instances.m_buffer[instance7], stop, targetIsNode: true);
                                    instance4.m_citizens.m_buffer[citizen3].CurrentLocation = Citizen.Location.Moving;
                                    StatisticBase statisticBase2 = Singleton<StatisticsManager>.instance.Acquire<StatisticArray>(StatisticType.IncomingTourists);
                                    statisticBase2 = statisticBase2.Acquire<StatisticInt32>((int)wealth2, 3);
                                    statisticBase2.Add(1);
                                }
                                else
                                {
                                    instance4.ReleaseCitizen(citizen3);
                                }
                            }
                        }
                    }
                }
            }
            if ((object)vehicleInfo != null)
            {
                Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                if (Singleton<VehicleManager>.instance.CreateVehicle(out ushort vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, data.m_position, material, flag3, !flag3))
                {
                    if (flag2)
                    {
                        vehicles.m_buffer[vehicle].m_flags |= Vehicle.Flags.DummyTraffic;
                        vehicles.m_buffer[vehicle].m_flags &= ~Vehicle.Flags.WaitingCargo;
                    }
                    vehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
                    vehicleInfo.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[vehicle], material, offer);
                    if (!flag2)
                    {
                        ushort building4 = offer.Building;
                        if (building4 != 0)
                        {
                            vehicleInfo.m_vehicleAI.GetSize(vehicle, ref vehicles.m_buffer[vehicle], out int size, out int _);
                            if (!flag3)
                            {
                                OutsideConnectionAI.ImportResource(building4, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[building4], material, size);
                            }
                        }
                    }
                }
            }
            
            // Original method return value.
            __result = true;

            // Don't execute base method after this.
            return false;
        } // end
    }
}
