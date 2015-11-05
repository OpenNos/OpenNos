using OpenNos.Core;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Character
    {
        public long CharacterId { get; set; }

        public long AccountId { get; set; }

        public string Name { get; set; }

        public byte Slot { get; set; }

        public byte Gender { get; set; }

        public byte Class { get; set; }

        public byte HairStyle { get; set; }

        public byte HairColor { get; set; }

        public short Map { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public int Hp { get; set; }

        public int Mp { get; set; }

        public long Gold { get; set; }

        public byte JobLevel { get; set; }

        public long JobLevelXp { get; set; }

        public byte Level { get; set; }

        public long LevelXp { get; set; }

        public int Reput { get; set; }

        public int Dignite { get; set; }

        public int LastPulse { get; set; }

        public int Sp { get; set; }

        public int Authority { get; set; }

        public int Invisible { get; set; }

        public int ArenaWinner { get; set; }

        public int SpUpgrade { get; set; }

        public string fd()
        {
           return String.Format("fd {0} {1} {2} {3}", Reput, GetReputIco(), Dignite, Math.Abs(GetDigniteIco()));
        }
        public int XPLoad()
        {
            int u0 = 300, v0 = 540, v1 = 960;
            int[] u = new int[99];
            int[] v = new int[110];
            double var = 1;
            v[0] = 540;
            v[1] = 960;
            u[0] = 300;
            for (int i = 2; i < v.Length; i++)
            {
                v[i] = v[i - 1] + 420 + 120 * (i - 1);
            }
            for (int i = 1; i < u.Length; i++)
            {
                if (i == 14) var = 6 / 3;
                if (i == 39) var = (double)(19 / (double)3);
                u[i] = Convert.ToInt32(u[i - 1] + var * v[i - 1]);
                //Console.WriteLine("lvl " + (i) + ":" + u[i - 1]);
            }
            return u[Level - 1];
        }

        public int JobXPLoad()
        {
            return Class == 0 ? JobLevel-1 * 700 + 2200: 16200 + JobLevel*1400;
        }
        public string lev()
        {
           return String.Format("lev {0} {1} {2} {3} {4} {5} 0 2", Level, LevelXp, JobLevel, JobLevelXp,XPLoad(),JobXPLoad());
        }
        public string c_info()
        {
           return String.Format("c_info {0} - {1} {2} - {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} ", Name, -1, -1, CharacterId, Authority, Gender, HairStyle, HairColor, Class, GetReputIco(), 0, Sp, Invisible, 0, SpUpgrade, ArenaWinner);
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

        public string tit()
        {
           return String.Format("tit {0} {1}", Language.Instance.GetMessageFromKey(Class == 0 ? "ADVENTURER" : Class == 1 ? "SWORDMAN" : Class == 2 ? "ARCHER" : "MAGICIAN"), Name);
            
        }

        public string stat()
        {
            //TODO add max HP MP
            return String.Format("stat {0} {1} {2} {3} 0 1024", Hp, Hp, Mp, Mp);
           
        }

        public string at()
        {
            return String.Format("at {0} {1} {2} {3} 2 0 0 1", CharacterId, Map, MapX, MapY);
           
        }

        public string c_map()
        {
           return String.Format("c_map 0 {0} 1", Map);
           
        }

        public string cond()
        {
           return String.Format("cond 1 {0} 0 0 11", CharacterId);

        }

        public string exts()
        {
           return String.Format("exts 0 48 48 48"); 
        }
    }
}
