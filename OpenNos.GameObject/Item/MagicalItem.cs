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
using OpenNos.GameObject.Helpers;
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

        public override void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null)
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
                            session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(19, 1, session.Character.CharacterId, MappingHelper.GuriItemEffects[EffectValue]), session.Character.MapX, session.Character.MapY);
                        }
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    break;

                //respawn objects
                case 1:
                    if (session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                        return;
                    }
                    int type, secondaryType, inventoryType, slot;
                    if (packetsplit != null && int.TryParse(packetsplit[2], out type) && int.TryParse(packetsplit[3], out secondaryType) && int.TryParse(packetsplit[4], out inventoryType) && int.TryParse(packetsplit[5], out slot))
                    {
                        int packetType;
                        switch (EffectValue)
                        {
                            case 0:
                                if (Option == 0)
                                {
                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1 #u_i^{type}^{secondaryType}^{inventoryType}^{slot}^2 {Language.Instance.GetMessageFromKey("WANT_TO_SAVE_POSITION")}"));
                                }
                                else
                                {
                                    if (int.TryParse(packetsplit[6], out packetType))
                                    {
                                        switch (packetType)
                                        {
                                            case 1:
                                                session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^3"));
                                                break;

                                            case 2:
                                                session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^4"));
                                                break;

                                            case 3:
                                                session.Character.SetReturnPoint(session.Character.MapId, session.Character.MapX, session.Character.MapY);
                                                RespawnMapTypeDTO respawn = session.Character.Respawn;
                                                if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                                {
                                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawn.DefaultMapId, (short)(respawn.DefaultX + ServerManager.Instance.RandomNumber(-5, 5)), (short)(respawn.DefaultY + ServerManager.Instance.RandomNumber(-5, 5)));
                                                }
                                                session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                                break;

                                            case 4:
                                                RespawnMapTypeDTO respawnObj = session.Character.Respawn;
                                                if (respawnObj.DefaultX != 0 && respawnObj.DefaultY != 0 && respawnObj.DefaultMapId != 0)
                                                {
                                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawnObj.DefaultMapId, (short)(respawnObj.DefaultX + ServerManager.Instance.RandomNumber(-5, 5)), (short)(respawnObj.DefaultY + ServerManager.Instance.RandomNumber(-5, 5)));
                                                }
                                                session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                                break;
                                        }
                                    }
                                }
                                break;

                            case 1:
                                if (int.TryParse(packetsplit[6], out packetType))
                                {
                                    RespawnMapTypeDTO respawn = session.Character.Return;
                                    switch (packetType)
                                    {
                                        case 0:
                                            if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                            {
                                                session.SendPacket(UserInterfaceHelper.Instance.GenerateRp(respawn.DefaultMapId, respawn.DefaultX, respawn.DefaultY, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1"));
                                            }
                                            break;

                                        case 1:
                                            session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^2"));
                                            break;

                                        case 2:
                                            if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                            {
                                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawn.DefaultMapId, respawn.DefaultX, respawn.DefaultY);
                                            }
                                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                            break;
                                    }
                                }
                                break;

                            case 2:
                                if (Option == 0)
                                {
                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1"));
                                }
                                else
                                {
                                    ServerManager.Instance.JoinMiniland(session, session);
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
                                byte nextValue = (byte)ServerManager.Instance.RandomNumber(0, 127);
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
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ADVENTURERS_CANT_USE"), 10));
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
                        if (Option == 0)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(10, 3, session.Character.CharacterId, 1));
                        }
                    }
                    break;

                // bubbles
                case 16:
                    if (!session.Character.IsVehicled)
                    {
                        if (Option == 0)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(10, 4, session.Character.CharacterId, 1));
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
                            wig.Design = (byte)ServerManager.Instance.RandomNumber(0, 15);
                            session.SendPacket(session.Character.GenerateEq());
                            session.SendPacket(session.Character.GenerateEquipment());
                            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn());
                            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
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