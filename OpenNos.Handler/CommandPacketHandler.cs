using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class CommandPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public CommandPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        public ClientSession Session { get { return _session; } }

        #endregion

        #region Methods

        [Packet("$AddMonster")]
        public void AddMonster(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short vnum = 0, isMoving = 0;

            Random rnd = new Random();

            if (packetsplit.Length == 4 && short.TryParse(packetsplit[2], out vnum) && short.TryParse(packetsplit[3], out isMoving))
            {
                NpcMonster npcmonster = ServerManager.GetNpc(vnum);
                if (npcmonster == null)
                    return;
                MapMonsterDTO monst = new MapMonsterDTO() { MonsterVNum = vnum, MapY = Session.Character.MapY, MapX = Session.Character.MapX, MapId = Session.Character.MapId, Position = (byte)Session.Character.Direction, IsMoving = isMoving == 1 ? true : false, MapMonsterId = MapMonster.generateMapMonsterId() };
                MapMonster monster = null;
                if (DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) == null)
                {
                    DAOFactory.MapMonsterDAO.Insert(monst);
                    monster = new MapMonster() { MonsterVNum = vnum, MapY = monst.MapY, Alive = true, CurrentHp = npcmonster.MaxHP, CurrentMp = npcmonster.MaxMP, MapX = monst.MapX, MapId = Session.Character.MapId, firstX = monst.MapX, firstY = monst.MapY, MapMonsterId = monst.MapMonsterId, Position = 1, IsMoving = isMoving == 1 ? true : false };
                    ServerManager.Monsters.Add(monster);
                    ServerManager.GetMap(Session.Character.MapId).Monsters.Add(monster);
                    ClientLinkManager.Instance.Broadcast(Session, monster.GenerateIn3(), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 10));
        }

        [Packet("$Ban")]
        public void Ban(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                ClientLinkManager.Instance.Kick(packetsplit[2]);
                if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                {
                    DAOFactory.AccountDAO.ToggleBan(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId);
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
                else
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME", 10));
        }

        [Packet("$ChangeClass")]
        public void ChangeClass(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte Class;
            if (packetsplit.Length > 2)
            {
                if (byte.TryParse(packetsplit[2], out Class) && Class < 4)
                {
                    ClientLinkManager.Instance.ClassChange(Session.Character.CharacterId, Class);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 10));
        }

        [Packet("$HeroLvl")]
        public void ChangeHeroLevel(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte hlevel;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out hlevel) && hlevel < 31 && hlevel > 0)
                {
                    Session.Character.HeroLevel = hlevel;
                    Session.Character.HeroXp = 0;
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HEROLEVEL_CHANGED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                    Session.Client.SendPacket(Session.Character.GenerateStatInfo());
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
                    this.GetStats(String.Empty);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$HeroLvl HEROLEVEL", 10));
        }

        [Packet("$JLvl")]
        public void ChangeJobLevel(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte joblevel;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out joblevel) && ((Session.Character.Class == 0 && joblevel <= 20) || (Session.Character.Class != 0 && joblevel <= 80)) && joblevel > 0)
                {
                    Session.Character.JobLevel = joblevel;
                    Session.Character.JobLevelXp = 0;
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOBLEVEL_CHANGED"), 0));
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(8), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$JLvl JOBLEVEL", 10));
        }

        [Packet("$Lvl")]
        public void ChangeLevel(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte level;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out level) && level < 100 && level > 0)
                {
                    Session.Character.Level = level;
                    Session.Character.LevelXp = 0;
                    Session.Character.Hp = (int)Session.Character.HPLoad();
                    Session.Character.Mp = (int)Session.Character.MPLoad();
                    Session.Client.SendPacket(Session.Character.GenerateStat());
                    Session.Client.SendPacket(Session.Character.GenerateStatInfo());
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LEVEL_CHANGED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
                    this.GetStats(String.Empty);

                    ClientLinkManager.Instance.UpdateGroup(Session.Character.CharacterId);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 10));
        }

        [Packet("$ChangeRep")]
        public void ChangeReputation(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long reput;
            if (packetsplit.Length != 3)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeRep REPUTATION", 10));
                return;
            }

            if (Int64.TryParse(packetsplit[2], out reput) && reput > 0)
            {
                Session.Character.Reput = reput;
                Session.Client.SendPacket(Session.Character.GenerateFd());
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REP_CHANGED"), 0));
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
            }
        }

        [Packet("$ChangeSex")]
        public void ChangeSex(string packet)
        {
            Session.Character.Gender = Session.Character.Gender == 1 ? (byte)0 : (byte)1;
            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SEX_CHANGED"), 0));
            Session.Client.SendPacket(Session.Character.GenerateEq());
            Session.Client.SendPacket(Session.Character.GenerateGender());
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
        }

        [Packet("$SPLvl")]
        public void ChangeSpecialistLevel(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte splevel;
            SpecialistInstance sp = Session.Character.EquipmentList.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
            if (sp != null && packetsplit.Length > 2 && Session.Character.UseSp)
            {
                if (Byte.TryParse(packetsplit[2], out splevel) && splevel <= 99 && splevel > 0)
                {
                    sp.SpLevel = splevel;
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SPLEVEL_CHANGED"), 0));
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(8), ReceiverType.AllOnMap);
                }
            }
            else

                Session.Client.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 10));
        }

        [Packet("$Help")]
        public void Command(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateSay("-----------Commands Info--------------", 10));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Shout MESSAGE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport Map X Y", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$TeleportToMe CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Gold AMOUNT", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Stat", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$JLvl JOBLEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$HeroLvl HEROLEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeSex", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeRep REPUTATION", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Kick CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$MapDance", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Kill CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Effect EFFECTID", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$PlayMusic MUSIC", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Invisible", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Position", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY PORTALTYPE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Shutdown", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 10));
        }

        [Packet("$CreateItem")]
        public void CreateItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte amount = 1, rare = 0, upgrade = 0, level = 0;
            short vnum, design = 0;
            ItemDTO iteminfo = null;
            if (packetsplit.Length != 5 && packetsplit.Length != 4 && packetsplit.Length != 3)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 10));
            }
            else if (short.TryParse(packetsplit[2], out vnum))
            {
                iteminfo = ServerManager.GetItem(vnum);
                if (iteminfo != null)
                {
                    if (iteminfo.IsColored)
                    {
                        if (packetsplit.Count() > 3)
                            short.TryParse(packetsplit[3], out design);
                    }
                    else if (iteminfo.Type == 0)
                    {
                        if (packetsplit.Length == 4)
                        {
                            byte.TryParse(packetsplit[3], out rare);
                        }
                        else if (packetsplit.Length == 5)
                        {
                            if (iteminfo.EquipmentSlot == Convert.ToByte((byte)EquipmentType.Sp))
                            {
                                byte.TryParse(packetsplit[3], out upgrade);
                                short.TryParse(packetsplit[4], out design);
                            }
                            else
                            {
                                byte.TryParse(packetsplit[3], out rare);
                                byte.TryParse(packetsplit[4], out upgrade);
                                if (upgrade == 0)
                                    if (iteminfo.BasicUpgrade != 0)
                                    {
                                        upgrade = iteminfo.BasicUpgrade;
                                    }
                            }
                        }
                    }
                    else
                    {
                        if (packetsplit.Length > 3)
                            byte.TryParse(packetsplit[3], out amount);
                    }
                    if (iteminfo.EquipmentSlot == Convert.ToByte((byte)EquipmentType.Sp))
                        level = 1;
                    //TODO inventoryitem
                    ItemInstance newItem = new ItemInstance();
                    //{
                    //    ItemInstanceId = Session.Character.InventoryList.GenerateInventoryItemId(),
                    //    Amount = amount,
                    //    ItemVNum = vnum,
                    //    Rare = rare,
                    //    Upgrade = upgrade,
                    //    Design = design,
                    //    Concentrate = 0,
                    //    CriticalLuckRate = 0,
                    //    CriticalRate = 0,
                    //    DamageMaximum = 0,
                    //    DamageMinimum = 0,
                    //    DarkElement = 0,
                    //    DistanceDefence = 0,
                    //    DistanceDefenceDodge = 0,
                    //    DefenceDodge = 0,
                    //    ElementRate = 0,
                    //    FireElement = 0,
                    //    HitRate = 0,
                    //    LightElement = 0,
                    //    IsFixed = false,
                    //    Ammo = 0,
                    //    MagicDefence = 0,
                    //    CloseDefence = 0,
                    //    SpXp = 0,
                    //    SpLevel = level,
                    //    SlDefence = 0,
                    //    SlElement = 0,
                    //    SlDamage = 0,
                    //    SlHP = 0,
                    //    WaterElement = 0,
                    //};
                    Inventory inv = Session.Character.InventoryList.CreateItem(newItem, Session.Character);
                    WearableInstance wearable = Session.Character.InventoryList.LoadBySlotAndType<WearableInstance>(inv.Slot,inv.Type);
                    ServersData.SetRarityPoint(ref wearable);
                    if (inv != null)
                    {
                        short Slot = inv.Slot;
                        if (Slot != -1)
                        {
                            Session.Client.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {amount}", 12));
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(vnum, inv.ItemInstance.Amount, iteminfo.Type, Slot, rare, design, upgrade));
                        }
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                    }
                }
                else
                {
                    Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_ITEM"), 0);
                }
            }
        }

        [Packet("$PortalTo")]
        public void CreatePortal(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short mapid, destx, desty = 0;
            sbyte portaltype = -1;
            if (packetsplit.Length > 4 && short.TryParse(packetsplit[2], out mapid) && short.TryParse(packetsplit[3], out destx) && short.TryParse(packetsplit[4], out desty))
            {
                if (ServerManager.GetMap(mapid) == null)
                    return;

                short mapId = Session.Character.MapId;
                short mapX = Session.Character.MapX;
                short mapY = Session.Character.MapY;
                if (packetsplit.Length > 5)
                    sbyte.TryParse(packetsplit[5], out portaltype);
                Portal portal = new Portal() { SourceMapId = mapId, SourceX = mapX, SourceY = mapY, DestinationMapId = mapid, DestinationX = destx, DestinationY = desty, Type = portaltype };
                ServerManager.GetMap(Session.Character.MapId).Portals.Add(portal);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateGp(portal), ReceiverType.AllOnMap);
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY PORTALTYPE", 10));
        }

        [Packet("$Effect")]
        public void Effect(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short arg = 0;
            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out arg);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(arg), ReceiverType.AllOnMap);
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Effect EFFECT", 10));
        }

        public void GetStats(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateStatChar());
        }

        [Packet("$Gold")]
        public void Gold(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long gold;
            if (packetsplit.Length > 2)
            {
                if (Int64.TryParse(packetsplit[2], out gold))
                {
                    if (gold <= 1000000000 && gold >= 0)
                    {
                        Session.Character.Gold = gold;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_SET"), 0));
                        Session.Client.SendPacket(Session.Character.GenerateGold());
                    }
                    else
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Gold AMOUNT", 10));
        }

        [Packet("$Invisible")]
        public void Invisible(string packet)
        {
            Session.Character.Invisible = !Session.Character.Invisible;
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateInvisible(), ReceiverType.AllOnMap);
            Session.Character.InvisibleGm = Session.Character.InvisibleGm ? false : true;
            if (Session.Character.InvisibleGm == true)
            {
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllOnMapExceptMe);
            }
            else
            {
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
            }
        }

        [Packet("$Kick")]
        public void Kick(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
                ClientLinkManager.Instance.Kick(packetsplit[2]);
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Kick CHARACTERNAME", 10));
        }

        [Packet("$Kill")]
        public void Kill(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 3)
            {
                string name = packetsplit[2];

                long? id = ClientLinkManager.Instance.GetProperty<long?>(name, "CharacterId");

                if (id != null)
                {
                    int? Hp = ClientLinkManager.Instance.GetProperty<int?>((long)id, "Hp");
                    if (Hp == 0)
                        return;
                    ClientLinkManager.Instance.SetProperty((long)id, "Hp", 0);

                    ClientLinkManager.Instance.SetProperty((long)id, "LastDefence", DateTime.Now);

                    ClientLinkManager.Instance.Broadcast(Session, $"su 1 {Session.Character.CharacterId} 1 {id} 1114 4 11 4260 0 0 0 0 {60000} 3 0", ReceiverType.AllOnMap);

                    ClientLinkManager.Instance.Broadcast(null, ClientLinkManager.Instance.GetUserMethod<string>((long)id, "GenerateStat"), ReceiverType.OnlySomeone, "", (long)id);
                    ClientLinkManager.Instance.AskRevive((long)id);
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Kill CHARACTERNAME", 10));
        }

        [Packet("$MapDance")]
        public void MapDance(string packet)
        {
            Session.CurrentMap.IsDancing = Session.CurrentMap.IsDancing == 0 ? 2 : 0;
            if (Session.CurrentMap.IsDancing == 2)
            {
                Session.Character.Dance();
                ClientLinkManager.Instance.Sessions.Where(s => s.Character.MapId.Equals(Session.Character.MapId) && s.Character.Name != Session.Character.Name).ToList().ForEach(s => ClientLinkManager.Instance.RequireBroadcastFromUser(Session, s.Character.CharacterId, "Dance"));
                ClientLinkManager.Instance.BroadcastToMap(Session.Character.MapId, "dance 2");
            }
            else
            {
                Session.Character.Dance();
                ClientLinkManager.Instance.Sessions.Where(s => s.Character.MapId.Equals(Session.Character.MapId) && s.Character.Name != Session.Character.Name).ToList().ForEach(s => ClientLinkManager.Instance.RequireBroadcastFromUser(Session, s.Character.CharacterId, "GenerateIn"));
                ClientLinkManager.Instance.BroadcastToMap(Session.Character.MapId, "dance");
            }
        }

        [Packet("$Morph")]
        public void Morph(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short[] arg = new short[4];
            bool verify = false;
            if (packetsplit.Length > 5)
            {
                verify = (short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && short.TryParse(packetsplit[5], out arg[3]));
            }
            switch (packetsplit.Length)
            {
                case 6:
                    if (verify)
                    {
                        if (arg[0] != 0)
                        {
                            Session.Character.UseSp = true;
                            Session.Character.Morph = arg[0];
                            Session.Character.MorphUpgrade = arg[1];
                            Session.Character.MorphUpgrade2 = arg[2];
                            Session.Character.ArenaWinner = arg[3];
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
                        }
                        else
                        {
                            Session.Character.UseSp = false;

                            Session.Client.SendPacket(Session.Character.GenerateCond());
                            Session.Client.SendPacket(Session.Character.GenerateLev());

                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
                            ClientLinkManager.Instance.Broadcast(Session, $"guri 6 1 {Session.Character.CharacterId} 0 0", ReceiverType.AllOnMap);
                        }
                    }
                    break;

                default:
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 10));
                    break;
            }
        }

        [Packet("$PlayMusic")]
        public void PlayMusic(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                if (packetsplit.Length <= 1) return;

                short arg;
                short.TryParse(packetsplit[2], out arg);
                if (arg > -1)
                    ClientLinkManager.Instance.Broadcast(Session, $"bgm {arg}", ReceiverType.AllOnMap);
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$PlayMusic BGMUSIC", 10));
        }

        [Packet("$Position")]
        public void Position(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateSay($"Map:{Session.Character.MapId} - X:{Session.Character.MapX} - Y:{Session.Character.MapY}", 12));
        }

        [Packet("$Rarify")]
        public void Rarify(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 10));
            }
            else
            {
                short itemslot = -1;
                short mode = -1;
                short protection = -1;
                short.TryParse(packetsplit[2], out itemslot);
                short.TryParse(packetsplit[3], out mode);
                short.TryParse(packetsplit[4], out protection);

                if (itemslot > -1 && mode > -1 && protection > -1)
                {
                    WearableInstance wearableInstance = Session.Character.InventoryList.LoadBySlotAndType<WearableInstance>(itemslot, 0);
                    if (wearableInstance != null)
                    {
                        wearableInstance.RarifyItem(Session, (RarifyMode)mode, (RarifyProtection)protection);
                    }
                }
            }
        }

        [Packet("$Resize")]
        public void Resize(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short arg = -1;

            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out arg);

                if (arg > -1)

                {
                    Session.Character.Size = arg;
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateScal(), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 10));
        }

        [Packet("$Shout")]
        public void Shout(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            if (packetsplit.Length > 2)
                for (int i = 2; i < packetsplit.Length; i++)
                    message += packetsplit[i] + " ";
            message.Trim();

            ClientLinkManager.Instance.Broadcast(Session, $"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}", ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
        }

        [Packet("$Shutdown")]
        public void Shutdown(string packet)
        {
            if (ClientLinkManager.Instance.TaskShutdown != null)
            {
                ClientLinkManager.Instance.ShutdownStop = true;
                ClientLinkManager.Instance.TaskShutdown = null;
            }
            else
            {
                ClientLinkManager.Instance.TaskShutdown = new Task(ShutdownTask);
                ClientLinkManager.Instance.TaskShutdown.Start();
            }
        }

        [Packet("$Speed")]
        public void Speed(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte arg = 0;
            bool verify = false;
            if (packetsplit.Length > 2)
            {
                verify = (byte.TryParse(packetsplit[2], out arg));
            }
            switch (packetsplit.Length)
            {
                case 3:
                    if (verify && arg < 60)
                    {
                        Session.Character.Speed = arg;
                        Session.Client.SendPacket(Session.Character.GenerateCond());
                    }
                    break;

                default:
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 10));
                    break;
            }
        }

        [Packet("$Stat")]
        public void Stat(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("TOTAL_SESSION")}: {ClientLinkManager.Instance.Sessions.Count()} ", 13));
        }

        [Packet("$Summon")]
        public void Summon(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short vnum = 0;
            byte qty = 1, move = 0;
            Random rnd = new Random();
            if (packetsplit.Length == 5 && short.TryParse(packetsplit[2], out vnum) && byte.TryParse(packetsplit[3], out qty) && byte.TryParse(packetsplit[4], out move))
            {
                NpcMonster npcmonster = ServerManager.GetNpc(vnum);
                if (npcmonster == null)
                    return;
                for (int i = 0; i < qty; i++)
                {
                    short mapx = (short)rnd.Next((Session.Character.MapX - qty) % Session.CurrentMap.XLength, (Session.Character.MapX + qty / 3) % Session.CurrentMap.YLength);
                    short mapy = (short)rnd.Next((Session.Character.MapY - qty) % Session.CurrentMap.XLength, (Session.Character.MapY + qty / 3) % Session.CurrentMap.YLength);
                    for (int j = 100; j > 0 && Session.CurrentMap != null && Session.CurrentMap.IsBlockedZone(mapx, mapy); j--)
                    {
                        mapx = (short)rnd.Next((Session.Character.MapX - qty) % Session.CurrentMap.XLength, (Session.Character.MapX + qty / 3) % Session.CurrentMap.YLength);
                        mapy = (short)rnd.Next((Session.Character.MapY - qty) % Session.CurrentMap.XLength, (Session.Character.MapY + qty / 3) % Session.CurrentMap.YLength);
                    }
                    MapMonster monst = new MapMonster() { MonsterVNum = vnum, Alive = true, CurrentHp = npcmonster.MaxHP, CurrentMp = npcmonster.MaxMP, MapY = mapy, MapX = mapx, MapId = Session.Character.MapId, firstX = mapx, firstY = mapy, MapMonsterId = MapMonster.generateMapMonsterId(), Position = 1, IsMoving = move != 0 ? true : false };
                    ServerManager.GetMap(Session.Character.MapId).Monsters.Add(monst);
                    ServerManager.Monsters.Add(monst);
                    ClientLinkManager.Instance.Broadcast(Session, monst.GenerateIn3(), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 10));
        }

        [Packet("$Teleport")]
        public void Teleport(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short[] arg = new short[3];
            bool verify = false;

            if (packetsplit.Length > 4)
            {
                verify = (short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && DAOFactory.MapDAO.LoadById(arg[0]) != null);
            }
            switch (packetsplit.Length)
            {
                case 3:
                    string name = packetsplit[2];
                    short? mapy = ClientLinkManager.Instance.GetProperty<short?>(name, "MapY");
                    short? mapx = ClientLinkManager.Instance.GetProperty<short?>(name, "MapX");
                    short? mapId = ClientLinkManager.Instance.GetProperty<short?>(name, "MapId");
                    if (mapy != null && mapx != null && mapId != null)
                    {
                        ClientLinkManager.Instance.MapOut(Session.Character.CharacterId);
                        Session.Character.MapId = (short)mapId;
                        Session.Character.MapX = (short)((short)(mapx) + 1);
                        Session.Character.MapY = (short)((short)(mapy) + 1);

                        ClientLinkManager.Instance.ChangeMap(Session.Character.CharacterId);
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                    break;

                case 5:
                    if (verify)
                    {
                        ClientLinkManager.Instance.MapOut(Session.Character.CharacterId);
                        Session.Character.MapId = arg[0];
                        Session.Character.MapX = arg[1];
                        Session.Character.MapY = arg[2];

                        ClientLinkManager.Instance.ChangeMap(Session.Character.CharacterId);
                    }
                    break;

                default:
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport MAP X Y", 10));
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 10));
                    break;
            }
        }

        [Packet("$TeleportToMe")]
        public void TeleportToMe(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if (packetsplit.Length == 3)
            {
                string name = packetsplit[2];

                long? id = ClientLinkManager.Instance.GetProperty<long?>(name, "CharacterId");

                if (id != null)
                {
                    ClientLinkManager.Instance.MapOut((long)id);
                    ClientLinkManager.Instance.SetProperty((long)id, "MapY", (short)((Session.Character.MapY) + (short)1));
                    ClientLinkManager.Instance.SetProperty((long)id, "MapX", (short)((Session.Character.MapX) + (short)1));
                    ClientLinkManager.Instance.SetProperty((long)id, "MapId", Session.Character.MapId);
                    ClientLinkManager.Instance.ChangeMap((long)id);
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$TeleportToMe CHARACTERNAME", 10));
        }

        [Packet("$Upgrade")]
        public void Upgrade(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 10));
            }
            else
            {
                short itemslot = -1;
                short mode = -1;
                short protection = -1;
                short.TryParse(packetsplit[2], out itemslot);
                short.TryParse(packetsplit[3], out mode);
                short.TryParse(packetsplit[4], out protection);

                if (itemslot > -1 && mode > -1 && protection > -1)
                {
                    WearableInstance wearableInstance = Session.Character.InventoryList.LoadBySlotAndType<WearableInstance>(itemslot, 0);
                    if (wearableInstance != null)
                    {
                        wearableInstance.UpgradeItem(Session, (UpgradeMode)mode, (UpgradeProtection)protection);
                    }
                }
            }
        }

        private void DeleteItem(byte type, short slot)
        {
            Session.Character.InventoryList.DeleteFromSlotAndType(slot, type);
            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));
        }

        private void GetStartupInventory()
        {
            foreach (String inv in Session.Character.GenerateStartupInventory())
            {
                Session.Client.SendPacket(inv);
            }
        }

        private async void ShutdownTask()
        {
            string message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 5);
            ClientLinkManager.Instance.Broadcast(Session, $"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}", ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            for (int i = 0; i < 60 * 4; i++)
            {
                await Task.Delay(1000);
                if (ClientLinkManager.Instance.ShutdownStop == true)
                {
                    ClientLinkManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 1);
            ClientLinkManager.Instance.Broadcast(Session, $"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}", ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (ClientLinkManager.Instance.ShutdownStop == true)
                {
                    ClientLinkManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 30);
            ClientLinkManager.Instance.Broadcast(Session, $"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}", ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (ClientLinkManager.Instance.ShutdownStop == true)
                {
                    ClientLinkManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            ClientLinkManager.Instance.Sessions.ForEach(s => s.Character?.Save());
            Environment.Exit(0);
        }

        #endregion
    }
}