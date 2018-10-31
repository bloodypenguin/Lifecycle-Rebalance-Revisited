using Boformer.Redirection;
using ColossalFramework;
using System;
using UnityEngine;

namespace WG_CitizenEdit
{
    [TargetType(typeof(OutsideConnectionAI))]
    public class NewOutsideConnectionAI : OutsideConnectionAI
    {
        [RedirectMethod]
        // Copied from game code. Ugh.... 
        // OutsideConnectionAI
        private static bool StartConnectionTransferImpl(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer, int touristFactor0, int touristFactor1, int touristFactor2)
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
                    Vector3 position = instance.m_buildings.m_buffer[(int)building].m_position;
                    if (Vector3.SqrMagnitude(position - data.m_position) > 40000f)
                    {
                        flag2 = true;
                        switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(38u))
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
                                material = TransferManager.TransferReason.Family0;
                                break;
                            case 19:
                                material = TransferManager.TransferReason.Family1;
                                break;
                            case 20:
                                material = TransferManager.TransferReason.Family2;
                                break;
                            case 21:
                                material = TransferManager.TransferReason.Family3;
                                break;
                            case 22:
                                material = TransferManager.TransferReason.Shopping;
                                break;
                            case 23:
                                material = TransferManager.TransferReason.ShoppingB;
                                break;
                            case 24:
                                material = TransferManager.TransferReason.ShoppingC;
                                break;
                            case 25:
                                material = TransferManager.TransferReason.ShoppingD;
                                break;
                            case 26:
                                material = TransferManager.TransferReason.ShoppingE;
                                break;
                            case 27:
                                material = TransferManager.TransferReason.ShoppingF;
                                break;
                            case 28:
                                material = TransferManager.TransferReason.ShoppingG;
                                break;
                            case 29:
                                material = TransferManager.TransferReason.ShoppingH;
                                break;
                            case 30:
                                material = TransferManager.TransferReason.Entertainment;
                                break;
                            case 31:
                                material = TransferManager.TransferReason.EntertainmentB;
                                break;
                            case 32:
                                material = TransferManager.TransferReason.EntertainmentC;
                                break;
                            case 33:
                                material = TransferManager.TransferReason.EntertainmentD;
                                break;
                            case 34:
                                material = TransferManager.TransferReason.TouristA;
                                break;
                            case 35:
                                material = TransferManager.TransferReason.TouristB;
                                break;
                            case 36:
                                material = TransferManager.TransferReason.TouristC;
                                break;
                            case 37:
                                material = TransferManager.TransferReason.TouristD;
                                break;
                        }
                    }
                }
            }
            switch (material)
            {
                case TransferManager.TransferReason.Oil:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOil, ItemClass.Level.Level2);
                    goto IL_63C;
                case TransferManager.TransferReason.Ore:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOre, ItemClass.Level.Level2);
                    goto IL_63C;
                case TransferManager.TransferReason.Logs:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialForestry, ItemClass.Level.Level2);
                    goto IL_63C;
                case TransferManager.TransferReason.Grain:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialFarming, ItemClass.Level.Level2);
                    goto IL_63C;
                case TransferManager.TransferReason.Goods:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialGeneric, ItemClass.Level.Level1);
                    goto IL_63C;
                case TransferManager.TransferReason.PassengerTrain:
                case TransferManager.TransferReason.PartnerYoung:
                case TransferManager.TransferReason.PartnerAdult:
                case TransferManager.TransferReason.LeaveCity0:
                case TransferManager.TransferReason.LeaveCity1:
                case TransferManager.TransferReason.LeaveCity2:
                case TransferManager.TransferReason.GarbageMove:
                case TransferManager.TransferReason.MetroTrain:
                case TransferManager.TransferReason.PassengerPlane:
                case TransferManager.TransferReason.PassengerShip:
                case TransferManager.TransferReason.DeadMove:
                case TransferManager.TransferReason.DummyCar:
                IL_33B:
                    switch (material)
                    {
                        case TransferManager.TransferReason.TouristA:
                        case TransferManager.TransferReason.TouristB:
                        case TransferManager.TransferReason.TouristC:
                        case TransferManager.TransferReason.TouristD:
                            goto IL_53D;
                        case TransferManager.TransferReason.UnsortedMail:
                        case TransferManager.TransferReason.OutgoingMail:
                            flag3 = true;
                            vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPost, ItemClass.Level.Level5);
                            goto IL_63C;
                        case TransferManager.TransferReason.SortedMail:
                        case TransferManager.TransferReason.IncomingMail:
                            vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPost, ItemClass.Level.Level5);
                            goto IL_63C;
                    }
                    return false;
                case TransferManager.TransferReason.Coal:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOre, ItemClass.Level.Level1);
                    goto IL_63C;
                case TransferManager.TransferReason.Family0:
                    education = Citizen.Education.Uneducated;
                    num = Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 5);
                    goto IL_63C;
                case TransferManager.TransferReason.Family1:
                    education = Citizen.Education.OneSchool;
                    num = Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 5);
                    goto IL_63C;
                case TransferManager.TransferReason.Family2:
                    education = Citizen.Education.TwoSchools;
                    num = Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 5);
                    goto IL_63C;
                case TransferManager.TransferReason.Family3:
                    education = Citizen.Education.ThreeSchools;
                    num = Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 5);
                    goto IL_63C;
                case TransferManager.TransferReason.Single0:
                case TransferManager.TransferReason.Single0B:
                    education = Citizen.Education.Uneducated;
                    num = 1;
                    goto IL_63C;
                case TransferManager.TransferReason.Single1:
                case TransferManager.TransferReason.Single1B:
                    education = Citizen.Education.OneSchool;
                    num = 1;
                    goto IL_63C;
                case TransferManager.TransferReason.Single2:
                case TransferManager.TransferReason.Single2B:
                    education = Citizen.Education.TwoSchools;
                    num = 1;
                    goto IL_63C;
                case TransferManager.TransferReason.Single3:
                case TransferManager.TransferReason.Single3B:
                    education = Citizen.Education.ThreeSchools;
                    num = 1;
                    goto IL_63C;
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
                    goto IL_53D;
                case TransferManager.TransferReason.Petrol:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOil, ItemClass.Level.Level1);
                    goto IL_63C;
                case TransferManager.TransferReason.Food:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialFarming, ItemClass.Level.Level1);
                    goto IL_63C;
                case TransferManager.TransferReason.Lumber:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialForestry, ItemClass.Level.Level1);
                    goto IL_63C;
                case TransferManager.TransferReason.DummyTrain:
                    if (offer.Building != buildingID)
                    {
                        flag2 = true;
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                        {
                            vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain, ItemClass.Level.Level4);
                        }
                        else
                        {
                            vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain, ItemClass.Level.Level1);
                        }
                    }
                    goto IL_63C;
                case TransferManager.TransferReason.DummyShip:
                    if (offer.Building != buildingID)
                    {
                        flag2 = true;
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                        {
                            vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, ItemClass.Level.Level4);
                        }
                        else
                        {
                            vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, ItemClass.Level.Level1);
                        }
                    }
                    goto IL_63C;
                case TransferManager.TransferReason.DummyPlane:
                    if (offer.Building != buildingID)
                    {
                        flag2 = true;
                        vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane, ItemClass.Level.Level1);
                    }
                    goto IL_63C;
            }
            // Copy paste to get it to work
            switch (material)
            {
                case TransferManager.TransferReason.TouristA:
                case TransferManager.TransferReason.TouristB:
                case TransferManager.TransferReason.TouristC:
                case TransferManager.TransferReason.TouristD:
                    goto IL_53D;
                case TransferManager.TransferReason.UnsortedMail:
                case TransferManager.TransferReason.OutgoingMail:
                    flag3 = true;
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPost, ItemClass.Level.Level5);
                    goto IL_63C;
                case TransferManager.TransferReason.SortedMail:
                case TransferManager.TransferReason.IncomingMail:
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPost, ItemClass.Level.Level5);
                    goto IL_63C;
            }
            return false;
        IL_53D:
            flag = true;
        IL_63C:
            if (num != 0)
            {
                CitizenManager instance2 = Singleton<CitizenManager>.instance;
                ushort building2 = offer.Building;
                if (building2 != 0)
                {
                    uint num2 = 0u;
                    if (!flag2)
                    {
                        num2 = instance.m_buildings.m_buffer[(int)building2].GetEmptyCitizenUnit(CitizenUnit.Flags.Home);
                    }
                    if (num2 != 0u || flag2)
                    {
                        int family = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                        ushort num3 = 0;
                        Citizen.Gender gender = Citizen.Gender.Male;
                        int[] ageArray = num == 1 ? DataStore.incomingSingleAge : DataStore.incomingAdultAge;

                        // Start of changes! ------------------------------------------------------- -------------------------------------------------------
                        int childrenAgeMax = 0; // 80 less than the youngest adult
                        int childrenAgeMin = 0; // 180 less than the youngest adult
                        int minAdultAge = 0;
                        for (int i = 0; i < num; i++)
                        {
                            uint num4 = 0u;
                            //int min = (i >= 2) ? 0 : 90;
                            //int max = (i >= 2) ? 15 : 105;
                            int min = ageArray[0];
                            int max = ageArray[1];

                            if (i == 1)
                            {
                                // min max shouldn't be too far from the first. Just because.
                                min = Math.Max(minAdultAge - 20, DataStore.incomingAdultAge[0]);
                                max = Math.Min(minAdultAge + 20, DataStore.incomingAdultAge[1]);
                            }
                            else if (i >= 2)
                            {
                                min = childrenAgeMin;
                                max = childrenAgeMax;
                            }

                            int age = Singleton<SimulationManager>.instance.m_randomizer.Int32(min, max);

                            if (i == 0)
                            {
                                minAdultAge = age;
                            }
                            else if (i == 1)
                            {
                                // Restrict to adult age. Young adult is 18 according to National Institutes of Health... even if the young adult section in a library isn't that range.
                                minAdultAge = Math.Min(age, minAdultAge);
                                childrenAgeMax = Math.Max(minAdultAge - 80, 0);  // Allow people 10 ticks from 'adulthood' to have kids
                                childrenAgeMin = Math.Max(minAdultAge - 178, 0); // Accounting gestation, which isn't simulated yet (2 ticks)
                            }

                            Citizen.Education education2 = education;
                            if (i < 2)
                            {
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
                            else // children
                            {
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
                            // End of changes? --------------------------------------------------------------------------------------------------------------
                            bool flag5;
                            if (i == 1)
                            {
                                bool flag4 = Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) < 5;
                                Citizen.Gender gender2;
                                if (flag4)
                                {
                                    gender2 = gender;
                                }
                                else
                                {
                                    gender2 = ((gender != Citizen.Gender.Male) ? Citizen.Gender.Male : Citizen.Gender.Female);
                                }
                                flag5 = instance2.CreateCitizen(out num4, age, family, ref Singleton<SimulationManager>.instance.m_randomizer, gender2);
                            }
                            else
                            {
                                flag5 = instance2.CreateCitizen(out num4, age, family, ref Singleton<SimulationManager>.instance.m_randomizer);
                            }
                            if (!flag5)
                            {
                                break;
                            }
                            if (i == 0)
                            {
                                gender = Citizen.GetGender(num4);
                            }
                            if (education2 >= Citizen.Education.OneSchool)
                            {
                                instance2.m_citizens.m_buffer[(int)((UIntPtr)num4)].Education1 = true;
                            }
                            if (education2 >= Citizen.Education.TwoSchools)
                            {
                                instance2.m_citizens.m_buffer[(int)((UIntPtr)num4)].Education1 = true;
                                instance2.m_citizens.m_buffer[(int)((UIntPtr)num4)].Education2 = true;
                            }
                            if (education2 >= Citizen.Education.ThreeSchools)
                            {
                                instance2.m_citizens.m_buffer[(int)((UIntPtr)num4)].Education1 = true;
                                instance2.m_citizens.m_buffer[(int)((UIntPtr)num4)].Education2 = true;
                                instance2.m_citizens.m_buffer[(int)((UIntPtr)num4)].Education3 = true;
                            }
                            if (flag2)
                            {
                                Citizen[] expr_85E_cp_0 = instance2.m_citizens.m_buffer;
                                UIntPtr expr_85E_cp_1 = (UIntPtr)num4;
                                expr_85E_cp_0[(int)expr_85E_cp_1].m_flags = (expr_85E_cp_0[(int)expr_85E_cp_1].m_flags | Citizen.Flags.DummyTraffic);
                            }
                            else
                            {
                                instance2.m_citizens.m_buffer[(int)((UIntPtr)num4)].SetHome(num4, 0, num2);
                            }
                            Citizen[] expr_8A3_cp_0 = instance2.m_citizens.m_buffer;
                            UIntPtr expr_8A3_cp_1 = (UIntPtr)num4;
                            expr_8A3_cp_0[(int)expr_8A3_cp_1].m_flags = (expr_8A3_cp_0[(int)expr_8A3_cp_1].m_flags | Citizen.Flags.MovingIn);
                            CitizenInfo citizenInfo = instance2.m_citizens.m_buffer[(int)((UIntPtr)num4)].GetCitizenInfo(num4);
                            ushort num5;
                            if (citizenInfo != null && instance2.CreateCitizenInstance(out num5, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo, num4))
                            {
                                if (num3 == 0)
                                {
                                    citizenInfo.m_citizenAI.SetSource(num5, ref instance2.m_instances.m_buffer[(int)num5], buildingID);
                                    citizenInfo.m_citizenAI.SetTarget(num5, ref instance2.m_instances.m_buffer[(int)num5], building2);
                                    num3 = num5;
                                }
                                else
                                {
                                    citizenInfo.m_citizenAI.SetSource(num5, ref instance2.m_instances.m_buffer[(int)num5], buildingID);
                                    citizenInfo.m_citizenAI.JoinTarget(num5, ref instance2.m_instances.m_buffer[(int)num5], num3);
                                }
                                instance2.m_citizens.m_buffer[(int)((UIntPtr)num4)].CurrentLocation = Citizen.Location.Moving;
                            }
                        }
                    }
                }
            }
            if (flag)
            {
                CitizenManager instance3 = Singleton<CitizenManager>.instance;
                ushort building3 = offer.Building;
                ushort transportLine = offer.TransportLine;
                if (building3 != 0)
                {
                    int family2 = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                    uint num6 = 0u;
                    if (!flag2)
                    {
                        num6 = instance.m_buildings.m_buffer[(int)building3].GetEmptyCitizenUnit(CitizenUnit.Flags.Visit);
                    }
                    if (num6 != 0u || flag2)
                    {
                        int age2 = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 240);
                        Citizen.Wealth wealth = Citizen.Wealth.High;
                        int num7 = touristFactor0 + touristFactor1 + touristFactor2;
                        if (num7 != 0)
                        {
                            int num8 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num7);
                            if (num8 < touristFactor0)
                            {
                                wealth = Citizen.Wealth.Low;
                            }
                            else if (num8 < touristFactor0 + touristFactor1)
                            {
                                wealth = Citizen.Wealth.Medium;
                            }
                        }
                        uint num9;
                        if (instance3.CreateCitizen(out num9, age2, family2, ref Singleton<SimulationManager>.instance.m_randomizer))
                        {
                            Citizen[] expr_ABB_cp_0 = instance3.m_citizens.m_buffer;
                            UIntPtr expr_ABB_cp_1 = (UIntPtr)num9;
                            expr_ABB_cp_0[(int)expr_ABB_cp_1].m_flags = (expr_ABB_cp_0[(int)expr_ABB_cp_1].m_flags | Citizen.Flags.Tourist);
                            Citizen[] expr_ADC_cp_0 = instance3.m_citizens.m_buffer;
                            UIntPtr expr_ADC_cp_1 = (UIntPtr)num9;
                            expr_ADC_cp_0[(int)expr_ADC_cp_1].m_flags = (expr_ADC_cp_0[(int)expr_ADC_cp_1].m_flags | Citizen.Flags.MovingIn);
                            instance3.m_citizens.m_buffer[(int)((UIntPtr)num9)].WealthLevel = wealth;
                            if (flag2)
                            {
                                Citizen[] expr_B20_cp_0 = instance3.m_citizens.m_buffer;
                                UIntPtr expr_B20_cp_1 = (UIntPtr)num9;
                                expr_B20_cp_0[(int)expr_B20_cp_1].m_flags = (expr_B20_cp_0[(int)expr_B20_cp_1].m_flags | Citizen.Flags.DummyTraffic);
                            }
                            else
                            {
                                instance3.m_citizens.m_buffer[(int)((UIntPtr)num9)].SetVisitplace(num9, 0, num6);
                            }
                            CitizenInfo citizenInfo2 = instance3.m_citizens.m_buffer[(int)((UIntPtr)num9)].GetCitizenInfo(num9);
                            ushort num10;
                            if (citizenInfo2 != null && instance3.CreateCitizenInstance(out num10, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo2, num9))
                            {
                                citizenInfo2.m_citizenAI.SetSource(num10, ref instance3.m_instances.m_buffer[(int)num10], buildingID);
                                citizenInfo2.m_citizenAI.SetTarget(num10, ref instance3.m_instances.m_buffer[(int)num10], building3);
                                instance3.m_citizens.m_buffer[(int)((UIntPtr)num9)].CurrentLocation = Citizen.Location.Moving;
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
                    TransportManager instance4 = Singleton<TransportManager>.instance;
                    int num11 = instance4.m_lines.m_buffer[(int)transportLine].CountStops(transportLine);
                    if (num11 > 0)
                    {
                        int index = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num11);
                        ushort stop = instance4.m_lines.m_buffer[(int)transportLine].GetStop(index);
                        if (stop != 0)
                        {
                            int family3 = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                            int age3 = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 240);
                            Citizen.Wealth wealth2 = Citizen.Wealth.High;
                            int num12 = touristFactor0 + touristFactor1 + touristFactor2;
                            if (num12 != 0)
                            {
                                int num13 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num12);
                                if (num13 < touristFactor0)
                                {
                                    wealth2 = Citizen.Wealth.Low;
                                }
                                else if (num13 < touristFactor0 + touristFactor1)
                                {
                                    wealth2 = Citizen.Wealth.Medium;
                                }
                            }
                            uint num14;
                            if (instance3.CreateCitizen(out num14, age3, family3, ref Singleton<SimulationManager>.instance.m_randomizer))
                            {
                                Citizen[] expr_D29_cp_0 = instance3.m_citizens.m_buffer;
                                UIntPtr expr_D29_cp_1 = (UIntPtr)num14;
                                expr_D29_cp_0[(int)expr_D29_cp_1].m_flags = (expr_D29_cp_0[(int)expr_D29_cp_1].m_flags | Citizen.Flags.Tourist);
                                Citizen[] expr_D4A_cp_0 = instance3.m_citizens.m_buffer;
                                UIntPtr expr_D4A_cp_1 = (UIntPtr)num14;
                                expr_D4A_cp_0[(int)expr_D4A_cp_1].m_flags = (expr_D4A_cp_0[(int)expr_D4A_cp_1].m_flags | Citizen.Flags.MovingIn);
                                instance3.m_citizens.m_buffer[(int)((UIntPtr)num14)].WealthLevel = wealth2;
                                CitizenInfo citizenInfo3 = instance3.m_citizens.m_buffer[(int)((UIntPtr)num14)].GetCitizenInfo(num14);
                                ushort num15;
                                if (citizenInfo3 != null && instance3.CreateCitizenInstance(out num15, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo3, num14))
                                {
                                    citizenInfo3.m_citizenAI.SetSource(num15, ref instance3.m_instances.m_buffer[(int)num15], buildingID);
                                    citizenInfo3.m_citizenAI.SetTarget(num15, ref instance3.m_instances.m_buffer[(int)num15], stop, true);
                                    instance3.m_citizens.m_buffer[(int)((UIntPtr)num14)].CurrentLocation = Citizen.Location.Moving;
                                    StatisticBase statisticBase2 = Singleton<StatisticsManager>.instance.Acquire<StatisticArray>(StatisticType.IncomingTourists);
                                    statisticBase2 = statisticBase2.Acquire<StatisticInt32>((int)wealth2, 3);
                                    statisticBase2.Add(1);
                                }
                                else
                                {
                                    instance3.ReleaseCitizen(num14);
                                }
                            }
                        }
                    }
                }
            }
            if (vehicleInfo != null)
            {
                Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                ushort num16;
                if (Singleton<VehicleManager>.instance.CreateVehicle(out num16, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, data.m_position, material, flag3, !flag3))
                {
                    if (flag2)
                    {
                        Vehicle[] expr_E93_cp_0 = vehicles.m_buffer;
                        ushort expr_E93_cp_1 = num16;
                        expr_E93_cp_0[(int)expr_E93_cp_1].m_flags = (expr_E93_cp_0[(int)expr_E93_cp_1].m_flags | Vehicle.Flags.DummyTraffic);
                        Vehicle[] expr_EB2_cp_0 = vehicles.m_buffer;
                        ushort expr_EB2_cp_1 = num16;
                        expr_EB2_cp_0[(int)expr_EB2_cp_1].m_flags = (expr_EB2_cp_0[(int)expr_EB2_cp_1].m_flags & ~Vehicle.Flags.WaitingCargo);
                    }
                    vehicleInfo.m_vehicleAI.SetSource(num16, ref vehicles.m_buffer[(int)num16], buildingID);
                    vehicleInfo.m_vehicleAI.StartTransfer(num16, ref vehicles.m_buffer[(int)num16], material, offer);
                    if (!flag2)
                    {
                        ushort building4 = offer.Building;
                        if (building4 != 0)
                        {
                            int amount;
                            int num17;
                            vehicleInfo.m_vehicleAI.GetSize(num16, ref vehicles.m_buffer[(int)num16], out amount, out num17);
                            if (!flag3)
                            {
                                OutsideConnectionAI.ImportResource(building4, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building4], material, amount);
                            }
                        }
                    }
                }
            }
            return true;
        } // end
    }
}
