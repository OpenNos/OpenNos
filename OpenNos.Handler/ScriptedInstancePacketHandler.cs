using CloneExtensions;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Packets.ServerPackets;
using System;
using System.Linq;

namespace OpenNos.Handler
{
    internal class ScriptedInstancePacketHandler : IPacketHandler
    {
        #region Instantiation

        public ScriptedInstancePacketHandler(ClientSession session)
        {
            Session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        /// <summary>
        /// RSelPacket packet
        /// </summary>
        /// <param name="packet"></param>
        public void Escape(EscapePacket packet)
        {
            if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId, Session.Character.MapX, Session.Character.MapY);
            }
        }

        /// <summary>
        /// RSelPacket packet
        /// </summary>
        /// <param name="packet"></param>
        public void GetGift(RSelPacket packet)
        {
            if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                Guid mapInstanceId = ServerManager.Instance.GetBaseMapInstanceIdByMapId(Session.Character.MapId);
                MapInstance map = ServerManager.Instance.GetMapInstance(mapInstanceId);
                ScriptedInstance scriptedInstance = map.TimeSpaces.FirstOrDefault(s => s.PositionX == Session.Character.MapX && s.PositionY == Session.Character.MapY);
                if (scriptedInstance != null)
                {
                    Session.Character.GetReput(scriptedInstance.Reputation);

                    Session.Character.Gold = Session.Character.Gold + scriptedInstance.Gold > ServerManager.Instance.MaxGold ? ServerManager.Instance.MaxGold : Session.Character.Gold + scriptedInstance.Gold;
                    Session.SendPacket(Session.Character.GenerateGold());
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("GOLD_TS_END"), scriptedInstance.Gold), 10));

                    var rand = new Random().Next(scriptedInstance.DrawItems.Count);
                    var repay = "repay ";
                    Session.Character.GiftAdd(scriptedInstance.DrawItems[rand].VNum, scriptedInstance.DrawItems[rand].Amount);

                    for (int i = 0; i < 3; i++)
                    {
                        Gift gift = scriptedInstance.GiftItems.ElementAtOrDefault(i);
                        repay += gift == null ? "-1.0.0 " : $"{gift.VNum}.0.{gift.Amount} ";
                        if (gift != null)
                        {
                            Session.Character.GiftAdd(gift.VNum, gift.Amount);
                        }
                    }

                    // TODO: Add HasAlreadyDone
                    for (int i = 0; i < 2; i++)
                    {
                        Gift gift = scriptedInstance.SpecialItems.ElementAtOrDefault(i);
                        repay += gift == null ? "-1.0.0 " : $"{gift.VNum}.0.{gift.Amount} ";
                        if (gift != null)
                        {
                            Session.Character.GiftAdd(gift.VNum, gift.Amount);
                        }
                    }

                    repay += $"{scriptedInstance.DrawItems[rand].VNum}.0.{scriptedInstance.DrawItems[rand].Amount}";
                    Session.SendPacket(repay);
                }
            }
        }

        /// <summary>
        /// treq packet
        /// </summary>
        /// <param name="treqPacket"></param>
        public void GetTreq(TreqPacket treqPacket)
        {
            ScriptedInstance timespace = Session.CurrentMapInstance.TimeSpaces.FirstOrDefault(s => treqPacket.X == s.PositionX && treqPacket.Y == s.PositionY).GetClone();

            if (timespace != null)
            {
                if (treqPacket.StartPress == 1 || treqPacket.RecordPress == 1)
                {
                    timespace.LoadScript();
                    if (timespace.FirstMap == null) return;
                    foreach (var i in timespace.RequieredItems)
                    {
                        if (Session.Character.Inventory.CountItem(i.VNum) < i.Amount)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_REQUIERED_ITEM"), ServerManager.Instance.GetItem(i.VNum).Name), 0));
                            return;
                        }
                        Session.Character.Inventory.RemoveItemAmount(i.VNum, i.Amount);
                    }
                    if (timespace.LevelMinimum > Session.Character.Level)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_REQUIERED_LEVEL"), 0));
                        return;
                    }

                    Session.Character.MapX = timespace.PositionX;
                    Session.Character.MapY = timespace.PositionY;
                    ServerManager.Instance.TeleportOnRandomPlaceInMap(Session, timespace.FirstMap.MapInstanceId);
                    timespace.FirstMap.InstanceBag.Creator = Session.Character.CharacterId;
                    Session.SendPackets(timespace.GenerateMinimap());
                    Session.SendPacket(timespace.GenerateMainInfo());
                    Session.SendPacket(timespace.FirstMap.InstanceBag.GenerateScore());
                }
                else
                {
                    Session.SendPacket(timespace.GenerateRbr());
                }
            }
        }

        /// <summary>
        /// wreq packet
        /// </summary>
        /// <param name="packet"></param>
        public void GetWreq(WreqPacket packet)
        {
            foreach (ScriptedInstance portal in Session.CurrentMapInstance.TimeSpaces)
            {
                if (Session.Character.PositionY >= portal.PositionY - 1 && Session.Character.PositionY <= portal.PositionY + 1
                    && Session.Character.PositionX >= portal.PositionX - 1 && Session.Character.PositionX <= portal.PositionX + 1)
                {
                    switch (packet.Value)
                    {
                        case 0:
                            if (Session.Character.Group != null && Session.Character.Group.Characters.Any(s => !s.CurrentMapInstance.InstanceBag.Lock && s.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance && s.Character.MapId == portal.MapId && s.Character.CharacterId != Session.Character.CharacterId && s.Character.MapX == portal.PositionX && s.Character.MapY == portal.PositionY))
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#wreq^3^{Session.Character.CharacterId} #wreq^0^1 {Language.Instance.GetMessageFromKey("ASK_JOIN_TEAM_TS")}"));
                            }
                            else
                            {
                                Session.SendPacket(portal.GenerateRbr());
                            }
                            break;

                        case 1:
                            byte.TryParse(packet.Param.ToString(), out byte record);
                            GetTreq(new TreqPacket()
                            {
                                X = portal.PositionX,
                                Y = portal.PositionY,
                                RecordPress = record,
                                StartPress = 1
                            });
                            break;

                        case 3:
                            ClientSession character = Session.Character.Group?.Characters.Where(s => s.Character.CharacterId == packet.Param).FirstOrDefault();
                            if (character != null)
                            {
                                if (portal.LevelMinimum > Session.Character.Level)
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_REQUIERED_LEVEL"), 0));
                                    return;
                                }

                                MapCell mapcell = character.CurrentMapInstance.Map.GetRandomPosition();
                                Session.Character.MapX = portal.PositionX;
                                Session.Character.MapY = portal.PositionY;
                                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, character.CurrentMapInstance.MapInstanceId, mapcell.X, mapcell.Y);
                                Session.SendPacket(portal.GenerateMainInfo());
                                Session.SendPackets(portal.GenerateMinimap());
                                Session.SendPacket(portal.FirstMap.InstanceBag.GenerateScore());
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// GitPacket packet
        /// </summary>
        /// <param name="packet"></param>
        public void Git(GitPacket packet)
        {
            MapButton button = Session.CurrentMapInstance.Buttons.FirstOrDefault(s => s.MapButtonId == packet.ButtonId);
            if (button != null)
            {
                Session.CurrentMapInstance.Broadcast(button.GenerateOut());
                button.RunAction();
                Session.CurrentMapInstance.Broadcast(button.GenerateIn());
            }
        }

        /// <summary>
        /// rxitPacket packet
        /// </summary>
        /// <param name="rxitPacket"></param>
        public void InstanceExit(RxitPacket rxitPacket)
        {
            if (rxitPacket?.State == 1)
            {
                if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.TimeSpaceInstance)
                {
                    if (Session.CurrentMapInstance.InstanceBag.Lock)
                    {
                        //5seed
                        Session.CurrentMapInstance.InstanceBag.DeadList.Add(Session.Character.CharacterId);
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("DIGNITY_LOST"), 20), 11));
                        Session.Character.Dignity = Session.Character.Dignity < -980 ? -1000 : Session.Character.Dignity - 20;
                    }
                    else
                    {
                        //1seed
                    }
                    ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId, Session.Character.MapX, Session.Character.MapY);
                }
            }
        }

        #endregion
    }
}