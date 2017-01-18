/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class MagicalItem : Item
    {
        #region Instantiation

        public MagicalItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, bool delay = false, string[] packetsplit = null)
        {
            switch (Effect)
            {
                // airwaves - eventitems
                case 0:
                    if (ItemType == ItemType.Event)
                    {
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateEff(EffectValue));
                        if (MappingHelper.GuriItemEffects.ContainsKey(EffectValue))
                        {
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateGuri(19, 1, MappingHelper.GuriItemEffects[EffectValue]), session.Character.MapX, session.Character.MapY);
                        }
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    break;

                //respawn objects
                case 1:
                    if (session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseInstance)
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                        return;
                    }
                    int x1;
                    int x2;
                    int x3;
                    int x4;
                    if (packetsplit != null && int.TryParse(packetsplit[2], out x1) && int.TryParse(packetsplit[3], out x2) && int.TryParse(packetsplit[4], out x3) && int.TryParse(packetsplit[5], out x4))
                    {
                        int x5;
                        switch (EffectValue)
                        {
                            case 0:
                                if (!delay)
                                {
                                    session.SendPacket(session.Character.GenerateDialog($"#u_i^{x1}^{x2}^{x3}^{x4}^1 #u_i^{x1}^{x2}^{x3}^{x4}^2 {Language.Instance.GetMessageFromKey("WANT_TO_SAVE_POSITION")}"));
                                }
                                else
                                {
                                    if (int.TryParse(packetsplit[6], out x5))
                                    {
                                        switch (x5)
                                        {
                                            case 1:
                                                session.SendPacket(session.Character.GenerateDelay(5000, 7, $"#u_i^{x1}^{x2}^{x3}^{x4}^3"));
                                                break;

                                            case 2:
                                                session.SendPacket(session.Character.GenerateDelay(5000, 7, $"#u_i^{x1}^{x2}^{x3}^{x4}^4"));
                                                break;

                                            case 3:
                                                session.Character.SetReturnPoint(session.Character.MapId, session.Character.MapX, session.Character.MapY);
                                                RespawnMapTypeDTO resp = session.Character.Respawn;
                                                if (resp.DefaultX != 0 && resp.DefaultY != 0 && resp.DefaultMapId != 0)
                                                {
                                                    ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, resp.DefaultMapId, (short)(resp.DefaultX + ServerManager.RandomNumber(-5, 5)), (short)(resp.DefaultY + ServerManager.RandomNumber(-5, 5)));
                                                }
                                                session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                                break;

                                            case 4:
                                                RespawnMapTypeDTO respa = session.Character.Respawn;
                                                if (respa.DefaultX != 0 && respa.DefaultY != 0 && respa.DefaultMapId != 0)
                                                {
                                                    ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, respa.DefaultMapId, (short)(respa.DefaultX + ServerManager.RandomNumber(-5, 5)), (short)(respa.DefaultY + ServerManager.RandomNumber(-5, 5)));
                                                }
                                                session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                                break;
                                        }
                                    }
                                }
                                break;

                            case 1:
                                if (int.TryParse(packetsplit[6], out x5))
                                {
                                    RespawnMapTypeDTO resp = session.Character.Return;
                                    switch (x5)
                                    {
                                        case 0:
                                            if (resp.DefaultX != 0 && resp.DefaultY != 0 && resp.DefaultMapId != 0)
                                            {
                                                session.SendPacket(session.Character.GenerateRp(resp.DefaultMapId, resp.DefaultX, resp.DefaultY, $"#u_i^{x1}^{x2}^{x3}^{x4}^1"));
                                            }
                                            break;

                                        case 1:
                                            session.SendPacket(session.Character.GenerateDelay(5000, 7, $"#u_i^{x1}^{x2}^{x3}^{x4}^2"));
                                            break;

                                        case 2:
                                            if (resp.DefaultX != 0 && resp.DefaultY != 0 && resp.DefaultMapId != 0)
                                            {
                                                ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, resp.DefaultMapId, resp.DefaultX, resp.DefaultY);
                                            }
                                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                            break;
                                    }
                                }
                                break;
                            case 2:
                                if (!delay)
                                {
                                    session.SendPacket(session.Character.GenerateDelay(5000, 7, $"#u_i^{x1}^{x2}^{x3}^{x4}^1"));
                                }
                                else
                                {
                                    ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                                    ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, session.Character.MinilandId, 3, 8);
                                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                }
                                break;
                        }
                    }
                    break;

                // dyes or waxes
                case 10:
                case 11:
                    if (!session.Character.IsVehicled)
                    {
                        if (Effect == 10)
                        {
                            if (EffectValue == 99)
                            {
                                byte nextValue = (byte)ServerManager.RandomNumber(0, 127);
                                session.Character.HairColor = Enum.IsDefined(typeof(HairColorType), nextValue) ? (HairColorType)nextValue : 0;
                            }
                            else
                            {
                                session.Character.HairColor = Enum.IsDefined(typeof(HairColorType), (byte)EffectValue) ? (HairColorType)EffectValue : 0;
                            }
                        }
                        else
                        {
                            if (session.Character.Class == (byte)ClassType.Adventurer && EffectValue > 1)
                            {
                                session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ADVENTURERS_CANT_USE"), 10));
                                return;
                            }
                            session.Character.HairStyle = Enum.IsDefined(typeof(HairStyleType), (byte)EffectValue) ? (HairStyleType)EffectValue : 0;
                        }
                        session.SendPacket(session.Character.GenerateEq());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx());
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    break;

                // dignity restoration
                case 14:
                    if ((EffectValue == 100 || EffectValue == 200) && session.Character.Dignity < 100 && !session.Character.IsVehicled)
                    {
                        session.Character.Dignity += EffectValue;
                        if (session.Character.Dignity > 100)
                        {
                            session.Character.Dignity = 100;
                        }
                        session.SendPacket(session.Character.GenerateFd());
                        session.SendPacket(session.Character.GenerateEff(49 - session.Character.Faction));
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    else if (EffectValue == 2000 && session.Character.Dignity < 100 && !session.Character.IsVehicled)
                    {
                        session.Character.Dignity = 100;
                        session.SendPacket(session.Character.GenerateFd());
                        session.SendPacket(session.Character.GenerateEff(49 - session.Character.Faction));
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    break;

                // speakers
                case 15:
                    if (!session.Character.IsVehicled)
                    {
                        if (!delay)
                        {
                            session.SendPacket(session.Character.GenerateGuri(10, 3, 1));
                        }
                    }
                    break;

                // bubbles
                case 16:
                    if (!session.Character.IsVehicled)
                    {
                        if (!delay)
                        {
                            session.SendPacket(session.Character.GenerateGuri(10, 4, 1));
                        }
                    }
                    break;

                // wigs
                case 30:
                    if (!session.Character.IsVehicled)
                    {
                        WearableInstance wig = session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);
                        if (wig != null)
                        {
                            wig.Design = (byte)ServerManager.RandomNumber(0, 15);
                            session.SendPacket(session.Character.GenerateEq());
                            session.SendPacket(session.Character.GenerateEquipment());
                            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn());
                            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                        }
                    }
                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType()));
                    break;
            }
        }

        #endregion
    }
}