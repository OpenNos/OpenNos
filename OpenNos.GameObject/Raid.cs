using System;
using System.Linq;
using OpenNos.Domain;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject
{
    public class Raid
    {
        #region Instantiation

        public Raid(ItemDTO seal)
        {
            RaidId = ServerManager.Instance.GetNextRaidId();
            RaidDesign = seal.EffectValue;
            Characters = new List<ClientSession>(RaidDesign == 23 ? 20 : RaidDesign == 20 ? 40 : 15);
            Listed = false;
            Finished = false;
            Launched = false;
            SetUpRaid();
        }

        private void SetUpRaid()
        {
            switch (RaidDesign)
            {
                case 0:
                    RaidName = "Cuby";
                    MinLvl = 1;
                    MaxLvl = 50;
                    break;
                default:
                    break;

            }
        }

        #endregion

        #region Members
        private bool _disposed;

        #endregion

        #region Properties

        public long RaidId { get; }

        public int RaidDesign { get; }

        public string RaidName { get; private set; }

        public bool Listed { get; set; }

        public bool Finished { get; set; }
        
        public bool Launched { get; set; }

        public ClientSession Leader => Characters[0];

        public List<ClientSession> Characters { get; private set; }

        public int MinLvl { get; set; }

        public int MaxLvl { get; set; }
        #endregion

        #region Methods

        public bool IsMemberOfRaid(long characterId)
        {
            return Characters.Any(t => t.Character.CharacterId == characterId);
        }

        public bool IsMemberOfRaid(ClientSession session)
        {
            return session != null && Characters.Any(t => t == session);
        }

        public string GenerateRaid(ClientSession player)
        {
            if (player == null) return "";
            string result = $"raid ";

            result += Leader == player ? "0" : "2";
            result = Characters.Aggregate(result, (current, session) => current + $" {session.Character.CharacterId}");
            return result.Remove(result.Length - 1);
        }

        public string GenerateRdlst()
        {
            string result = $"rdlst {MinLvl} {MaxLvl} 0 ";

            result = Characters.Aggregate(result,
                (current, session) =>
                    current +
                    $"{session.Character.Level}." +
                    $"{(session.Character.UseSp || session.Character.IsVehicled ? session.Character.Morph : -1)}." +
                    $"{(short) session.Character.Class}.0.{session.Character.Name}.{(short) session.Character.Gender}." +
                    $"{session.Character.CharacterId}.{session.Character.HeroLevel} ");
            return result;
        }

        public string GenerateRaidF(ClientSession player)
        {
            string result = $"raidf ";

            result += Leader == player ? "0 " : "2 ";
            result += $"{RaidDesign} ";
            result = Characters.Aggregate(result, (current, session) => current + $"{session.Character.CharacterId} ");
            return result.Remove(result.Length - 1);
        }

        public string GenerateRdlstf()
        {
            string result = $"rdlstf {MinLvl} {MaxLvl} 0 ";

            result += RaidDesign;
            result = Characters.Aggregate(result,
                (current, session) =>
                    current +
                    $" {session.Character.Level}." +
                    $"{(session.Character.UseSp || session.Character.IsVehicled ? session.Character.Morph : -1)}." +
                    $"{(short) session.Character.Class}.0.{session.Character.Name}.{(short) session.Character.Gender}." +
                    $"{session.Character.CharacterId}.{session.Character.HeroLevel}");
            return result;
        }
        
        public void AddToRaid(ClientSession session)
        {
            if (session == null || Launched) return;
            Characters.Add(session);
            session.Character.Raid = this;
        }
        public void Dispose()
        {
            if (_disposed) return;
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        public void UpdateVisual()
        {
            foreach (var player in Characters)
            {
                if (RaidDesign == 24 || RaidDesign == 20)
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

        public void LeaveRaid(ClientSession session)
        {
            if (session == null) return;
            SendEndPlayer(session);
            if (Launched)
            {
                ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId,
                    session.Character.MapX, session.Character.MapY);
            }
            session.Character.Raid = null;
            Characters.Remove(session);
            UpdateVisual();
        }

        public void DestroyRaid()
        {
            ServerManager.Instance.RemoveRaid(this);
            Dispose();
        }

        #endregion
    }
}
