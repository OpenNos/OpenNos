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

        public int sp { get; set; }

        public int invisible { get; set; }

        public int arenaWinner { get; set; }

        public int spUpgrade { get; set; }

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
    }
}
