using AutoMapper;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Character
    {
        public Character()
        {
            Mapper.CreateMap<CharacterDTO, Character>();
            Mapper.CreateMap<Character, CharacterDTO>();
        }

        private void OnPropertyChanged()
        {
            //CharacterDTO characterToUpdate = Mapper.Map<CharacterDTO>(this);

            //if (Name != null)
            //    DAOFactory.CharacterDAO.InsertOrUpdate(ref characterToUpdate);

        }

        private long characterId;
        public long CharacterId
        {
            get { return characterId; }
            set
            {
                characterId = value;
                
            }
        }

        public long accountId;
        public long AccountId
        {
            get { return accountId; }
            set
            {
                accountId = value;
                
            }
        }
        public string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                
            }
        }
        public int faction;
        public int Faction
        {
            get { return faction; }
            set
            {
                faction = value;

            }
        }
        public byte slot;
        public byte Slot
        {
            get { return slot; }
            set
            {
                slot = value;
                
            }
        }
        public byte gender;
        public byte Gender
        {
            get { return gender; }
            set
            {
                gender = value;
                
            }
        }

        public byte classobj;
        public byte Class
        {
            get { return classobj; }
            set
            {
                classobj = value;
                
            }
        }
        public byte hairStyle;
        public byte HairStyle
        {
            get { return hairStyle; }
            set
            {
                hairStyle = value;
                
            }
        }
        public byte hairColor;
        public byte HairColor
        {
            get { return hairColor; }
            set
            {
                hairColor = value;
                
            }
        }
        public short map;
        public short MapId
        {
            get { return map; }
            set
            {
                map = value;
                
            }
        }
        public short mapX;
        public short MapX
        {
            get { return mapX; }
            set
            {
                mapX = value;
                
            }
        }
        public short mapY;
        public short MapY
        {
            get { return mapY; }
            set
            {
                mapY = value;
                
            }
        }
        public int hp;
        public int Hp
        {
            get { return hp; }
            set
            {
                hp = value;
                
            }
        }
        public int mp;
        public int Mp
        {
            get { return mp; }
            set
            {
                mp = value;
                
            }
        }
        public long gold;
        public long Gold
        {
            get { return gold; }
            set
            {
                gold = value;
                
            }
        }
        public byte jobLevel;
        public byte JobLevel
        {
            get { return jobLevel; }
            set
            {
                jobLevel = value;
                
            }
        }
        public long jobLevelXp;
        public long JobLevelXp
        {
            get { return jobLevelXp; }
            set
            {
                jobLevelXp = value;
                
            }
        }
        public byte level;
        public byte Level
        {
            get { return level; }
            set
            {
                level = value;
                
            }
        }
        public long levelXp;
        public long LevelXp
        {
            get { return levelXp; }
            set
            {
                levelXp = value;
                
            }
        }
        public int reput;
        public int Reput
        {
            get { return reput; }
            set
            {
                reput = value;
                
            }
        }
        public int dignite;
        public int Dignite
        {
            get { return dignite; }
            set
            {
                dignite = value;
                
            }
        }
        public int lastPulse;
        public int LastPulse
        {
            get { return lastPulse; }
            set
            {
                lastPulse = value;
                
            }
        }
        public double lastPortal;
        public double LastPortal
        {
            get { return lastPortal; }
            set
            {
                lastPortal = value;
                
            }
        }
        public int sp;
        public int Morph
        {
            get { return sp; }
            set
            {
                sp = value;
                
            }
        }
        public int authority;
        public int Authority
        {
            get { return authority; }
            set
            {
                authority = value;
                
            }
        }
        public int invisible;
        public int Invisible
        {
            get { return invisible; }
            set
            {
                invisible = value;
                
            }
        }
        public int speed;
        public int Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                
            }
        }
        public int arenaWinner;
        public int ArenaWinner
        {
            get { return arenaWinner; }
            set
            {
                arenaWinner = value;
                
            }
        }
        public int spUpgrade;
        public int MorphUpgrade
        {
            get { return spUpgrade; }
            set
            {
                spUpgrade = value;
                
            }
        }
        public int spUpgrade2;
        public int MorphUpgrade2
        {
            get { return spUpgrade2; }
            set
            {
                spUpgrade2 = value;
                
            }
        }
        public int direction;
        public int Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                
            }
        }
        public int rested;
        public int Rested
        {
            get { return rested; }
            set
            {
                rested = value;
                
            }
        }

        public byte state;
        public byte State
        {
            get { return state; }
            set
            {
                state = value;
                
            }
        }

        public bool Update()
        {
            try
            {
                CharacterDTO characterToUpdate = Mapper.Map<CharacterDTO>(this);
                DAOFactory.CharacterDAO.InsertOrUpdate(ref characterToUpdate);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public string GenerateEff(int effectid)
        {
            return String.Format("eff 1 {0} {1}", CharacterId, effectid);
        }

        public List<String> GenerateGp()
        {
            List<String> gpList = new List<String>();
            foreach (Portal portal in ServerManager.GetMap(this.MapId).Portals)
                gpList.Add(String.Format("gp {0} {1} {2} {3} {4}", portal.SrcX, portal.SrcY, portal.DestMap, portal.Type, 0));
            return gpList;
        }
        public List<String> Generatein2()
        {
            List<String> in2List = new List<String>();
            foreach (Npc npc in ServerManager.GetMap(this.MapId).Npcs)
                in2List.Add(String.Format("in 2 {0} {1} {2} {3} {4} 100 100 9632 0 0 - 1 1 0 - 1 - 0 - 1 0 0 0 0 0 0 0 0", npc.Vnum, npc.NpcId,npc.MapX,npc.MapY,npc.Position));
            return in2List;
        }
        public string GenerateFd()
        {
            return String.Format("fd {0} {1} {2} {3}", Reput, GetReputIco(), Dignite, Math.Abs(GetDigniteIco()));
        }
        public string GenerateMapOut()
        {
            return String.Format("mapout");
        }

        public string GenerateOut()
        {
            return String.Format("out 1 {0}", CharacterId);
        }
        public double HPLoad()
        {
            return ServersData.HPData[Class, Level];
        }

        public double MPLoad()
        {
            return ServersData.MPData[Class, Level];
        }

        public double SPXPLoad()
        {
            return ServersData.SpXPData[JobLevel - 1];
        }


        public double XPLoad()
        {
            return ServersData.XPData[Level - 1];
        }

        public int HealthHPLoad()
        {
            if (rested == 1)
                return ServersData.HpHealth[Class];
            else
                return ServersData.HpHealthStand[Class];
        }
        public int HealthMPLoad()
        {
            if (rested == 1)
                return ServersData.MpHealth[Class];
            else
                return ServersData.MpHealthStand[Class];
        }
        public double JobXPLoad()
        {
            if (Class == (byte)ClassType.Adventurer)
                return ServersData.FirstJobXPData[JobLevel - 1];
            else
                return ServersData.SecondJobXPData[JobLevel - 1];
        }

        public string GenerateLev()
        {
            return String.Format("lev {0} {1} {2} {3} {4} {5} 0 2", Level, LevelXp, JobLevel, JobLevelXp, XPLoad(), JobXPLoad());
        }

        public string GenerateCInfo()
        {
            return String.Format("c_info {0} - {1} {2} - {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} ", Name, -1, -1, CharacterId, Authority, Gender, HairStyle, HairColor, Class, GetReputIco(), 0, Morph, Invisible, 0, MorphUpgrade, ArenaWinner);
        }

        public int GetDigniteIco()
        {
            int icoDignite = 1;
            if (Convert.ToInt32(Dignite) <= -100)
                icoDignite = 2;
            if (Convert.ToInt32(Dignite) <= -200)
                icoDignite = 3;
            if (Convert.ToInt32(Dignite) <= -400)
                icoDignite = 4;
            if (Convert.ToInt32(Dignite) <= -600)
                icoDignite = 5;
            if (Convert.ToInt32(Dignite) <= -800)
                icoDignite = 6;

            return icoDignite;

        }

        public int GetReputIco()
        {
            return Convert.ToInt64(Reput) <= 100 ? 1 : Convert.ToInt64(Reput) <= 300 ? 2 : Convert.ToInt64(Reput) <= 500 ? 3 : Convert.ToInt64(Reput) <= 1000 ? 4 : Convert.ToInt64(Reput) <= 1500 ? 5 : Convert.ToInt64(Reput) <= 2000 ? 6 : Convert.ToInt64(Reput) <= 4500 ? 7 :
            Convert.ToInt64(Reput) <= 7000 ? 8 : Convert.ToInt64(Reput) <= 10000 ? 9 : Convert.ToInt64(Reput) <= 19000 ? 10 : Convert.ToInt64(Reput) <= 38000 ? 11 : Convert.ToInt64(Reput) <= 50000 ? 12 : Convert.ToInt64(Reput) <= 80000 ? 13 : Convert.ToInt64(Reput) <= 120000 ? 14 :
                    Convert.ToInt64(Reput) <= 170000 ? 15 : Convert.ToInt64(Reput) <= 230000 ? 16 : Convert.ToInt64(Reput) <= 300000 ? 17 : Convert.ToInt64(Reput) <= 380000 ? 18 : Convert.ToInt64(Reput) <= 470000 ? 19 : Convert.ToInt64(Reput) <= 570000 ? 20 : Convert.ToInt64(Reput) <= 700000 ? 21 :
                    Convert.ToInt64(Reput) <= 1000000 ? 22 : Convert.ToInt64(Reput) <= 3000000 ? 23 : Convert.ToInt64(Reput) <= 5000000 ? 24 : Convert.ToInt64(Reput) <= 7000000 ? 25 : Convert.ToInt64(Reput) <= 10000000 ? 26 : Convert.ToInt64(Reput) <= 50000000 ? 27 : 30;

        }

        public string GenerateTit()
        {
            return String.Format("tit {0} {1}", Language.Instance.GetMessageFromKey(Class == (byte)ClassType.Adventurer ? ClassType.Adventurer.ToString().ToUpper() : Class == (byte)ClassType.Swordman ? ClassType.Swordman.ToString().ToUpper() : Class == (byte)ClassType.Archer ? ClassType.Archer.ToString().ToUpper() : ClassType.Magician.ToString().ToUpper()), Name);

        }

        public string GenerateStat()
        {
            //TODO add max HP MP
            return String.Format("stat {0} {1} {2} {3} 0 1024", Hp, HPLoad(), Mp, MPLoad());

        }

        public string GenerateAt()
        {
            return String.Format("at {0} {1} {2} {3} 2 0 0 1", CharacterId, MapId, MapX, MapY);

        }
        public string GenerateReqInfo()
        {
            return String.Format("tc_info {0} {1} {2} {3} {4} {9} 0 {8} {5} {6} 0 0 1 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 {7}", Level, Name, 0, 0, Class, GetReputIco(), GetDigniteIco(),Language.Instance.GetMessageFromKey("NO_PREZ_MESSAGE"), Language.Instance.GetMessageFromKey("NO_FAMILY"),Gender);

        }
        
        public string GenerateCMap()
        {
            return String.Format("c_map 0 {0} 1", MapId);

        }

        public string GenerateCond()
        {
            return String.Format("cond 1 {0} 0 0 {1}", CharacterId, Speed);

        }

        public string GenerateExts()
        {
            return String.Format("exts 0 48 48 48");
        }

        public string GenerateMv(int x, int y)
        {
            return String.Format("mv 1 {0} {1} {2} {3}", CharacterId, x, y, Speed);

        }
        public string GenerateCMode()
        {
            return String.Format("c_mode 1 {0} {1} {2} {3} {4}", CharacterId, Morph, MorphUpgrade, MorphUpgrade2, ArenaWinner);

        }

        public string GenerateSay(string message, int type)
        {
            return String.Format("say 1 {0} {1} {2}", CharacterId, type, message);

        }

        public string GenerateIn()
        {
            return String.Format("in 1 {0} - {1} {2} {3} {4} {5} {6} {7} {8} {9} -1.-1.-1.-1.-1.-1.-1.-1 {10} {11} {12} -1 0 0 0 0 0 0 0 0 -1 - {13} {16} 0 0 0 {14} 0 {15} 0 10", Name, CharacterId, MapX, MapY, Direction, (Authority == 2 ? 2 : 0), Gender, HairStyle, HairColor, Class, (int)((Hp / HPLoad()) * 100), (int)((Mp / MPLoad()) * 100), rested, (GetDigniteIco() == 1) ? GetReputIco() : -GetDigniteIco(), ArenaWinner, 0, invisible);

        }

        public string GenerateRest()
        {
            return String.Format("rest 1 {0} {1}", CharacterId, Rested);
        }

        public string GenerateDir()
        {
            return String.Format("dir 1 {0} {1}", CharacterId, Direction);
        }

        public string GenerateMsg(string message, int v)
        {
            return String.Format("msg {0} {1}", v, message);
        }

        public string GenerateSpk(object message, int v)
        {
            return String.Format("spk 1 {0} {1} {2} {3}", CharacterId, v, Name, message);
        }

        public string GenerateInfo(string message)
        {
            string str2 = "info " + name + " ne joue pas.";
            return String.Format("info {0}", message);
        }

        public string GenerateStatInfo()
        {
            return String.Format("st 1 {0} {1} {2} {3} {4} {5}", CharacterId, Level, (int)((Hp / HPLoad()) * 100), (int)((Mp / MPLoad()) * 100), Hp, Mp);
        }

        public string GenerateEq()
        {
             return String.Format("eq {0} {1} {2} {3} {4} {5} -1.-1.-1.-1.-1.-1.-1.-1 0 0", CharacterId, (Authority == 2 ? 2 : 0),Gender,HairStyle,HairColor,Class);
        }

        public string GenerateFaction()
        {
            return String.Format("fs {0}", Faction);

        }
    }
}
