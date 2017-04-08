using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Raid
    {
        #region Members

        private bool _disposed;

        #endregion

        #region Instantiation

        public Raid(ItemDTO seal)
        {
            RaidId = ServerManager.Instance.GetNextRaidId();
            RaidDesign = seal.EffectValue;
            Characters = new List<ClientSession>(RaidDesign == 23 ? 20 : RaidDesign == 20 ? 40 : 15);
            Listed = false;
            Finished = false;
            Launched = false;
        }

        #endregion

        #region Properties

        public List<ClientSession> Characters { get; private set; }

        public bool Finished { get; set; }

        public bool Launched { get; set; }

        public ClientSession Leader => Characters[0];

        public int LevelMaximum { get; set; }

        public int LevelMinimum { get; set; }

        public bool Listed { get; set; }

        public int RaidDesign { get; }

        public long RaidId { get; }

        #endregion

        #region Methods

        public void DestroyRaid()
        {
            ServerManager.Instance.RemoveRaid(this);
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        public string GenerateRaid(ClientSession player)
        {
            if (player == null) return "";
            string result = $"raid ";

            result += Leader == player ? "0" : "2";
            result = Characters.Aggregate(result, (current, session) => current + $" {session.Character.CharacterId}");
            return result.Remove(result.Length - 1);
        }

        public string GenerateRaidF(ClientSession player)
        {
            string result = $"raidf ";

            result += Leader == player ? "0 " : "2 ";
            result += $"{RaidDesign} ";
            result = Characters.Aggregate(result, (current, session) => current + $"{session.Character.CharacterId} ");
            return result.Remove(result.Length - 1);
        }

        public string GenerateRdlst()
        {
            string result = $"rdlst {LevelMinimum} {LevelMaximum} 0 ";

            result = Characters.Aggregate(result,
                (current, session) =>
                    current +
                    $"{session.Character.Level}." +
                    $"{(session.Character.UseSp || session.Character.IsVehicled ? session.Character.Morph : -1)}." +
                    $"{(short)session.Character.Class}.0.{session.Character.Name}.{(short)session.Character.Gender}." +
                    $"{session.Character.CharacterId}.{session.Character.HeroLevel} ");
            return result;
        }

        public string GenerateRdlstf()
        {
            string result = $"rdlstf {LevelMinimum} {LevelMaximum} 0 ";

            result += RaidDesign;
            result = Characters.Aggregate(result,
                (current, session) =>
                    current +
                    $" {session.Character.Level}." +
                    $"{(session.Character.UseSp || session.Character.IsVehicled ? session.Character.Morph : -1)}." +
                    $"{(short)session.Character.Class}.0.{session.Character.Name}.{(short)session.Character.Gender}." +
                    $"{session.Character.CharacterId}.{session.Character.HeroLevel}");
            return result;
        }

        public bool IsMemberOfRaid(long characterId)
        {
            return Characters.Any(t => t.Character.CharacterId == characterId);
        }

        public bool IsMemberOfRaid(ClientSession session)
        {
            return session != null && Characters.Any(t => t == session);
        }

        public void Join(ClientSession session)
        {
            if (session == null || Launched) return;
            Characters.Add(session);
            session.Character.Raid = this;
            UpdateVisual();
        }

        public void Kick(ClientSession session)
        {
            if (session == null) return;
            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("KICKED_FROM_RAID"), Leader.Character.Name), 0));
            if (Launched)
            {
                ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId,
                    session.Character.MapX, session.Character.MapY);
            }
            SendEndPlayer(session);
            session.Character.Raid = null;
            Characters.Remove(session);
            UpdateVisual();
        }

        public void Leave(ClientSession session)
        {
            if (session == null) return;
            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("LEFT_RAID")), 0));
            if (Launched)
            {
                ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId,
                    session.Character.MapX, session.Character.MapY);
            }
            SendEndPlayer(session);
            session.Character.Raid = null;
            Characters.Remove(session);
            UpdateVisual();
        }

        public void SendCreationPacket(ClientSession session)
        {
            if (RaidDesign == 20 || RaidDesign == 24)
            {
                session.SendPacket($"raidf 2 {session.Character.CharacterId}");
                session.SendPacket("raidf 1 1");
            }
            else
            {
                session.SendPacket($"raid 2 {session.Character.CharacterId}");
                session.SendPacket("raid 1 1");
            }
        }

        public void SendEndPlayer(ClientSession session)
        {
            if (session == null) return;
            if (RaidDesign == 24 || RaidDesign == 20)
            {
                session.SendPacket("raidf 1 0");
                session.SendPacket("raidf 2 - 1");
            }
            else
            {
                session.SendPacket("raid 1 0");
                session.SendPacket("raid 2 - 1");
            }
        }

        public void UpdateVisual()
        {
            foreach (var player in Characters)
            {
                if (RaidDesign != 24 || RaidDesign != 20) // ZENAS OR LAURENA
                {
                    player.SendPacket(GenerateRaid(player));
                    player.SendPacket(GenerateRdlst());
                }
                else
                {
                    player.SendPacket(GenerateRaidF(player));
                    player.SendPacket(GenerateRdlstf());
                }
            }
        }

        #endregion
    }
}